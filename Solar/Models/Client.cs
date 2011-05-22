using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ignition;
using Ignition.Linq;
using Ignition.Presentation;
using IronPython.Hosting;
using Lunar;
using Microsoft.Scripting.Hosting;
using Solar.Filtering;
using Solar.Scripting;

namespace Solar.Models
{
	/// <summary>
	/// Solar クライアント
	/// </summary>
	public class Client : NotifyObject, IDisposable
	{
		/// <summary>
		/// 重要度の低い例外の発生時に発生します。(発生元のカテゴリ, 発生した例外)
		/// </summary>
		public event EventHandler<EventArgs<Category, Exception>> ThrowWarning;
		/// <summary>
		/// 重要度の高い例外の発生時に発生します。(発生元のカテゴリ, 発生した例外)
		/// </summary>
		public event EventHandler<EventArgs<Category, Exception>> ThrowError;
		/// <summary>
		/// スクリプト実行中の例外の発生時に発生します。(発生元のファイル, 発生した例外)
		/// </summary>
		public event EventHandler<EventArgs<string, Exception>> ThrowScriptError;
		/// <summary>
		/// 投稿前に発生します。(投稿文, 投稿をキャンセルするかどうか)
		/// </summary>
		public event EventHandler<EventArgs<string, bool>> Posting;
		/// <summary>
		/// 投稿後に発生します。(投稿成功状況)
		/// </summary>
		public event EventHandler<EventArgs<bool>> Posted;
		/// <summary>
		/// 更新前に発生します。(対象のカテゴリ)
		/// </summary>
		public event EventHandler<EventArgs<Category>> Refreshing;
		/// <summary>
		/// 更新後に発生します。(対象のカテゴリ, 新着エントリ)
		/// </summary>
		public event EventHandler<EventArgs<Category, IList<IEntry>>> Refreshed;
		/// <summary>
		/// スクロール先取得前に発生します。(対象のカテゴリ)
		/// </summary>
		public event EventHandler<EventArgs<Category>> RequestingNewPage;
		/// <summary>
		/// スクロール先取得後に発生します。(対象のカテゴリ)
		/// </summary>
		public event EventHandler<EventArgs<Category>> RequestedNewPage;
		/// <summary>
		/// 認証要求時に発生します。(認証)
		/// </summary>
		public event EventHandler<EventArgs<OAuthAuthorization>> Authenticate;
		/// <summary>
		/// 設定の読み込み時に発生します。(対象の設定)
		/// </summary>
		public event EventHandler<EventArgs<Settings>> LoadSettings;
		/// <summary>
		/// 設定の保存時に発生します。(対象の設定)
		/// </summary>
		public event EventHandler<EventArgs<Settings>> SaveSettings;
		/// <summary>
		/// User Streams 初期化時に発生します。(対象の Stream)
		/// </summary>
		public event EventHandler<EventArgs<TwitterStream>> StreamInitialize;
		/// <summary>
		/// User Streams 接続時に発生します。(対象のアカウント)
		/// </summary>
		public event EventHandler<EventArgs<AccountToken>> StreamConnected;
		/// <summary>
		/// User Streams 切断時に発生します。(対象のアカウント)
		/// </summary>
		public event EventHandler<EventArgs<AccountToken>> StreamDisconnected;
		/// <summary>
		/// 本体の終了時に発生します。
		/// </summary>
		public event EventHandler Shutdown;
		/// <summary>
		/// キャッシュのクリア時に発生します。
		/// </summary>
		public event EventHandler ClearCache;

		/// <summary>
		/// インスタンス
		/// </summary>
		public static readonly Client Instance = new Client();
		readonly ConcurrentDictionary<Category, int> updateTimes = new ConcurrentDictionary<Category, int>();
		readonly ConcurrentDictionary<AccountToken, TwitterStream> streams = new ConcurrentDictionary<AccountToken, TwitterStream>();
		readonly ConcurrentDictionary<AccountToken, int> streamsRetry = new ConcurrentDictionary<AccountToken, int>();
		readonly ScriptWatcher urlExpanders;
		readonly ScriptWatcher urlShorteners;
		readonly ScriptWatcher filterSourceScripts;
		readonly ScriptWatcher filterTermsScripts;
		Timer updateTimer;
		bool autoUpdating;

		internal bool IsDesignMode
		{
			get;
			set;
		}

		/// <summary>
		/// スクリプト実行環境を取得します。
		/// </summary>
		public static ScriptRuntime Runtime
		{
			get;
			private set;
		}

		/// <summary>
		/// このオブジェクトが破棄済みかどうかを取得します。
		/// </summary>
		public bool Disposed
		{
			get;
			private set;
		}

		/// <summary>
		/// アカウントに割り当てられている TwitterStream を取得します。
		/// </summary>
		public IDictionary<AccountToken, TwitterStream> Streams
		{
			get
			{
				return streams;
			}
		}

		/// <summary>
		/// フォローしているユーザの ID コレクションを取得します。
		/// </summary>
		public ConcurrentDictionary<AccountToken, HashSet<UserID>> Friends
		{
			get;
			private set;
		}

		/// <summary>
		/// API 呼び出し制限に関する情報を取得します。
		/// </summary>
		public NotifyCollection<RateLimit> RateLimits
		{
			get;
			private set;
		}

		/// <summary>
		/// カテゴリ グループを取得します。
		/// </summary>
		public NotifyCollection<CategoryGroup> Groups
		{
			get
			{
				if (Settings.Default == null)
					return null;

				return Settings.Default.CategoryGroups;
			}
		}

		/// <summary>
		/// アカウントを取得します。
		/// </summary>
		public NotifyCollection<AccountToken> Accounts
		{
			get
			{
				if (Settings.Default == null)
					return null;

				return Settings.Default.Accounts;
			}
		}

		/// <summary>
		/// キャッシュを取得します。
		/// </summary>
		public StatusCache StatusCache
		{
			get;
			private set;
		}

		/// <summary>
		/// User Streams を一時停止するかどうかを取得します。
		/// </summary>
		public bool BypassStreams
		{
			get;
			internal set;
		}

		/// <summary>
		/// 最終更新を取得します。
		/// </summary>
		public DateTime LastUpdate
		{
			get
			{
				return GetValue(() => this.LastUpdate);
			}
			private set
			{
				SetValue(() => this.LastUpdate, value);
			}
		}

		/// <summary>
		/// 短縮 URL 展開スクリプトを取得します。
		/// </summary>
		public IEnumerable<ScriptScope> UrlExpanders
		{
			get
			{
				return urlExpanders.Scripts.Values;
			}
		}

		/// <summary>
		/// URL 短縮スクリプト名を取得します。
		/// </summary>
		public IEnumerable<string> UrlShorteners
		{
			get
			{
				return urlShorteners.Scripts.Keys.Select(Path.GetFileNameWithoutExtension);
			}
		}

		/// <summary>
		/// フィルタ ソース スクリプト名を取得します。
		/// </summary>
		public IEnumerable<string> FilterSourceScripts
		{
			get
			{
				return filterSourceScripts.Scripts.Keys;
			}
		}

		/// <summary>
		/// フィルタ項目 スクリプト名を取得します。
		/// </summary>
		public IEnumerable<string> FilterTermsScripts
		{
			get
			{
				return filterTermsScripts.Scripts.Keys;
			}
		}

		internal ScriptWatcher FilterSourceScriptWatcher
		{
			get
			{
				return filterSourceScripts;
			}
		}

		internal ScriptWatcher FilterTermsScriptWatcher
		{
			get
			{
				return filterTermsScripts;
			}
		}

		/// <summary>
		/// User Streams 接続要求を無視するアカウントの一覧を取得します。
		/// </summary>
		public HashSet<AccountToken> IgnoreStreamConnection
		{
			get;
			private set;
		}

		Client()
		{
			this.StatusCache = new StatusCache();
			this.StatusCache.ReleaseStatuses += (sender, e) => e.Value = this.StatusCache.GetStatuses().Freeze().Except(Category.GetInstances().SelectMany(_ => _.Statuses).OfType<Status>());
			this.StatusCache.ReleaseUsers += (sender, e) => e.Value = this.StatusCache.GetUsers().Freeze().Except(this.StatusCache.GetStatuses().Select(_ => _.User));

			this.IgnoreStreamConnection = new HashSet<AccountToken>();
			this.RateLimits = new NotifyCollection<RateLimit>();
			this.Friends = new ConcurrentDictionary<AccountToken, HashSet<UserID>>();
			urlExpanders = new ScriptWatcher(this, App.UrlExpanderScriptsPath);
			urlShorteners = new ScriptWatcher(this, App.UrlShortenerScriptsPath);
			filterSourceScripts = new ScriptWatcher(this, App.FilterSourceScriptsPath);
			filterTermsScripts = new ScriptWatcher(this, App.FilterTermsScriptsPath);
			urlShorteners.Changed += (sender, e) => OnPropertyChanged("UrlShorteners");
		}

		internal void OnThrowScriptError(EventArgs<string, Exception> e)
		{
			ThrowScriptError.DispatchEvent(this, e);
		}

		internal void OnClearCache()
		{
			ClearCache.RaiseEvent(this, EventArgs.Empty);
		}

		/// <summary>
		/// 指定した文字列に含まれている URL を指定した短縮スクリプトで短縮します。
		/// </summary>
		/// <param name="shortener">短縮スクリプト。</param>
		/// <param name="post">URL が含まれる文字列。</param>
		/// <returns>URL が短縮された文字列。</returns>
		public string ShortenUrls(string shortener, string post)
		{
			if (string.IsNullOrEmpty(post))
				return post;

			try
			{
				shortener += ".py";

				if (urlShorteners.Scripts.ContainsKey(shortener))
					post = LinkConverter.UriRegex.Replace(post, m => urlExpanders.Scripts.All(_ =>
					{
						try
						{
							return !_.Value.GetVariable("IsShort")(new Uri(m.Value));
						}
						catch (Exception ex)
						{
							ThrowScriptError.DispatchEvent(this, new EventArgs<string, Exception>(_.Key, ex));
						}

						return true;
					}) ? urlShorteners.Scripts[shortener].GetVariable("Shorten")(new Uri(m.Value)) : m.Value);
			}
			catch (Exception ex)
			{
				ThrowScriptError.DispatchEvent(this, new EventArgs<string, Exception>(shortener, ex));
			}

			return post;
		}

		internal void Run()
		{
			Load();

			if (!this.Accounts.Any())
				using (var auth = new OAuthAuthorization())
				{
					Authenticate.RaiseEvent(this, new EventArgs<OAuthAuthorization>(auth));

					if (auth.Token != null)
						this.Accounts.Add(auth.Token);
				}

			if (!this.Groups.Any())
				this.Groups.Add(new CategoryGroup
				{
					new Category("Home", new HomeFilterSource()),
					new Category("Mentions", new MentionsFilterSource())
					{
						Filter =
						{
							Terms =
							{
								new ContainsFilterTerms
								{
									Except = true,
									Keywords =
									{
										"RT @",
									},
								},
							},
						},
						Interval = TimeSpan.FromMinutes(10),
					},
					new Category("RT", new HomeFilterSource(), new MentionsFilterSource(), new RetweetedByMeFilterSource(), new RetweetedToMeFilterSource(), new RetweetsOfMeFilterSource())
					{
						Filter =
						{
							Terms =
							{
								new ContainsFilterTerms
								{
									Keywords =
									{
										"RT @",
									},
								},
							},
						},
						Interval = TimeSpan.FromMinutes(10),
					},
					new Category("DM", new DirectMessagesReceivedFilterSource(), new DirectMessagesSentFilterSource())
					{
						Interval = TimeSpan.FromMinutes(10),
					},
					new Category("Favorites", new FavoritesFilterSource())
					{
						Interval = TimeSpan.FromMinutes(10),
					},
				});

			updateTimer = new Timer(TimerCallback, null, 0, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
			Update();
		}

		void TimerCallback(object state)
		{
			if (!autoUpdating)
				using (FinallyBlock.Create(autoUpdating = true, _ => autoUpdating = false))
				{
					var categories = this.Groups.SelectMany()
												.Freeze();

					lock (updateTimes)
						updateTimes.Keys.Where(_ => !categories.Contains(_))
										.Freeze()
										.RunWhile(updateTimes.Remove);

					Task.Factory.StartNew(() =>
					{
						IEnumerable<Category> list;

						lock (updateTimes)
							list = updateTimes.Keys.Freeze();

						list.Where(_ =>
						{
							lock (updateTimes)
								return updateTimes.ContainsKey(_)
									&& updateTimes[_]++ > _.Interval.TotalSeconds
									&& (DateTime.Now - _.LastUpdate).TotalSeconds > 1;
						})
							.Run(_ => UpdateInternal(EnumerableEx.Wrap(_)));
					});
				}
		}

		void Load()
		{
			if (File.Exists(Settings.FileName))
				LoadSettings.RaiseEvent(this, new EventArgs<Settings>(Settings.Default));

			var px = Settings.Default.Connection.GetWebProxy();

			if (px != null)
				WebRequest.DefaultWebProxy = px;

			OnPropertyChanged("Accounts");
			OnPropertyChanged("Groups");

			var scripts = App.ScriptsPath;
			var userScripts = Path.Combine(App.StartupPath, "UserScripts");
			var files = Directory.GetFiles(scripts, "*.py");

			if (Directory.Exists(scripts))
				foreach (var i in files)
				{
					EnsureScriptRuntime();

					try
					{
						Runtime.ExecuteFile(ScriptWatcher.GetUserOrDefault(i));
					}
					catch (Exception ex)
					{
						ThrowScriptError.RaiseEvent(this, new EventArgs<string, Exception>(Path.GetFileName(i), ex));
					}
				}

			if (Directory.Exists(userScripts))
			{
				var userFiles = Directory.GetFiles(userScripts, "*.py");
				var fileNames = new HashSet<string>(files.Select(Path.GetFileName));

				foreach (var i in userFiles.Where(_ => !fileNames.Contains(Path.GetFileName(_))))
				{
					EnsureScriptRuntime();

					try
					{
						Runtime.ExecuteFile(i);
					}
					catch (Exception ex)
					{
						ThrowScriptError.RaiseEvent(this, new EventArgs<string, Exception>(Path.GetFileName(i), ex));
					}
				}
			}
		}

		void Save()
		{
			SaveSettings.DispatchEvent(this, new EventArgs<Settings>(Settings.Default));
			Settings.Default.Save();
		}

		internal static ScriptRuntime EnsureScriptRuntime()
		{
			if (Runtime == null)
			{
				var asm = Assembly.GetEntryAssembly();

				Runtime = Python.CreateRuntime();
				Runtime.LoadAssembly(asm);

				foreach (var j in AppDomain.CurrentDomain.GetAssemblies())
					Runtime.LoadAssembly(j);
			}

			return Runtime;
		}

		/// <summary>
		/// アカウントと本文を指定し非同期に投稿を開始します。
		/// </summary>
		/// <param name="account">アカウント</param>
		/// <param name="text">本文</param>
		/// <returns>投稿タスク</returns>
		public Task<bool> Post(AccountToken account, string text)
		{
			return Post(account, text, (_, s) => _.Statuses.Update(ShortenUrls(Settings.Default.Post.AutoShortenUrl, s)));
		}

		/// <summary>
		/// アカウント、本文、返信先を指定し非同期に投稿を開始します。
		/// </summary>
		/// <param name="account">アカウント</param>
		/// <param name="text">本文</param>
		/// <param name="inReplyTo">返信先</param>
		/// <returns>投稿タスク</returns>
		public Task<bool> Post(AccountToken account, string text, Status inReplyTo)
		{
			return Post(account, text, (_, s) => _.Statuses.Update(ShortenUrls(Settings.Default.Post.AutoShortenUrl, s), inReplyTo.StatusID));
		}

		/// <summary>
		/// アカウント、本文、送信先を指定し非同期にダイレクトメッセージの送信を開始します。
		/// </summary>
		/// <param name="account">アカウント</param>
		/// <param name="text">本文</param>
		/// <param name="directMessageDestination">送信先</param>
		/// <returns>送信タスク</returns>
		public Task<bool> Post(AccountToken account, string text, string directMessageDestination)
		{
			return Post(account, text, (_, s) => _.DirectMessages.Send(directMessageDestination, ShortenUrls(Settings.Default.Post.AutoShortenUrl, s)));
		}

		Task<bool> Post(AccountToken account, string text, Func<TwitterClient, string, Status> post)
		{
			try
			{
				var e = new EventArgs<string, bool>(text, false);

				Posting.DispatchEvent(this, e);

				if (e.Value2)
					return null;

				return Task.Factory.StartNew<bool>(() =>
				{
					try
					{
						using (var client = new TwitterClient(account, this.StatusCache))
							AppendStatus(post(client, e.Value1), true);

						Posted.DispatchEvent(this, new EventArgs<bool>(true));

						return true;
					}
					catch (Exception ex)
					{
						OnThrowError(null, ex);
						Posted.DispatchEvent(this, new EventArgs<bool>(false));

						return false;
					}
				});
			}
			catch (Exception ex)
			{
				OnThrowError(null, ex);
				Posted.DispatchEvent(this, new EventArgs<bool>(false));

				return null;
			}
		}

		/// <summary>
		/// アカウントとつぶやきを指定し非同期に Retweet を開始します。
		/// </summary>
		/// <param name="account">アカウント</param>
		/// <param name="status">Retweet するつぶやき</param>
		/// <returns>Retweet タスク</returns>
		public Task Retweet(AccountToken account, Status status)
		{
			return Task.Factory.StartNew(() =>
			{
				try
				{
					using (new TwitterClient(account, this.StatusCache))
						AppendStatus(status.Retweet(), true);
				}
				catch (Exception ex)
				{
					OnThrowError(null, ex);
				}
			});
		}

		/// <summary>
		/// アカウントとつぶやきを指定し非同期にお気に入りに登録または登録解除を開始します。
		/// </summary>
		/// <param name="account">アカウント</param>
		/// <param name="status">つぶやき</param>
		/// <returns>お気に入りに登録または登録解除タスク</returns>
		public Task ToggleFavorite(AccountToken account, Status status)
		{
			return Task.Factory.StartNew(() =>
			{
				var past = status.Favorited;

				try
				{
					using (new TwitterClient(account, this.StatusCache))
					{
						if (status.Favorited)
							status.Unfavorite();
						else
							status.Favorite();
					}
				}
				catch (Exception ex)
				{
					status.Favorited = past;
					OnThrowError(null, ex);
				}
			});
		}

		/// <summary>
		/// つぶやきまたはダイレクトメッセージを指定し非同期に削除を開始します。
		/// </summary>
		/// <param name="status">つぶやきまたはダイレクトメッセージ</param>
		/// <returns>削除タスク</returns>
		public Task Remove(Status status)
		{
			return Task.Factory.StartNew(() =>
			{
				try
				{
					using (new TwitterClient(status.Account, this.StatusCache))
						RemoveStatus(status.Destroy());
				}
				catch (Exception ex)
				{
					OnThrowError(null, ex);
				}
			});
		}

		/// <summary>
		/// 全カテゴリの更新を開始します。
		/// </summary>
		/// <returns>更新タスク</returns>
		public Task Update()
		{
			return Update((Category)null);
		}

		/// <summary>
		/// 指定したカテゴリおよびその関連カテゴリの更新を開始します。
		/// </summary>
		/// <param name="category">更新するカテゴリ</param>
		/// <returns>更新タスク</returns>
		public Task Update(Category category)
		{
			return Task.Factory.StartNew(() => UpdateInternal(category == null ? null : EnumerableEx.Wrap(category)));
		}

		/// <summary>
		/// 指定したカテゴリおよびその関連カテゴリの更新を開始します。
		/// </summary>
		/// <param name="category">更新するカテゴリ</param>
		/// <returns>更新タスク</returns>
		public Task Update(IEnumerable<Category> category)
		{
			return Task.Factory.StartNew(() => UpdateInternal(category));
		}

		void AppendStatus(Status status, bool isSelfPost)
		{
			foreach (var i in this.Groups.SelectMany())
				if (isSelfPost && !i.Filter.Sources.All(ExceptSourceStreaming(status.Account)))
					continue;
				else if (isSelfPost && i.Filter.Sources.Any(source => source.TypeMatch
				(
					(HomeFilterSource _) => !status.IsDirectMessage && (_.Account == 0 || status.Account.UserID == _.Account),
					(SentFilterSource _) => !status.IsDirectMessage && (_.Account == 0 || status.Account.UserID == _.Account),
					(PublicFilterSource _) => !status.IsDirectMessage,
					(SearchFilterSource _) => status.Text.Contains(_.Query),
					(DirectMessagesSentFilterSource _) => status.IsDirectMessage && (_.Account == 0 || status.Account.UserID == _.Account)
				) && i.Filter.Terms.All(_ => _.FilterStatuses(EnumerableEx.Wrap(status)).Any())))
				{
					var idx = i.Statuses.IndexOf(status);

					if (idx == -1)
						i.Statuses.Insert(0, status);
					else
						i.Statuses[idx] = status;

					if (Settings.Default.Timeline.AutoResetNewCount)
						i.ClearUnreads();
					else if (i.Unreads > 0)
						i.Unreads++;
				}
				else if (!isSelfPost)
				{
					var idx = i.Statuses.IndexOf(status);

					if (idx == -1)
						i.Statuses.Insert(0, status);
					else
						i.Statuses[idx] = status;

					if (Settings.Default.Timeline.AutoResetNewCount)
						i.ClearUnreads();
					else if (i.Unreads > 0)
						i.Unreads++;
				}
		}

		void UpdateStatus(Status status)
		{
			UpdateStatus(status, _ => true);
		}

		void UpdateStatus(Status status, Func<Status, bool> replace)
		{
			foreach (var i in Category.GetInstances()
									  .Where(_ => _.Statuses.Contains(status)))
			{
				var idx = i.Statuses.IndexOf(status);

				if (replace((Status)i.Statuses[idx]))
				{
					i.Statuses.Insert(idx, status);
					i.Statuses.RemoveAt(idx + 1);
				}
			}
		}

		void RemoveStatus(StatusID id)
		{
			RemoveStatus(this.StatusCache.RetrieveStatus(id, _ => null));
		}

		void RemoveStatus(Status status)
		{
			if (status == null)
				return;

			foreach (var i in this.Groups.SelectMany()
								  .Select(_ => _.Statuses)
								  .Where(_ => _.Contains(status)))
				i.Remove(status);

			this.StatusCache.RemoveStatus(status.StatusID);
		}

		internal Task<bool> RequestNewPage(Category category)
		{
			return Task.Factory.StartNew(() =>
			{
				var usingAccounts = category.Filter.Sources.Select(_ => _.Account)
														   .Distinct()
														   .Freeze();
				var hasAll = usingAccounts.Contains(0);

				RequestingNewPage.DispatchEvent(this, new EventArgs<Category>(category));

				try
				{
					foreach (var i in hasAll ? this.Accounts : this.Accounts.Where(_ => usingAccounts.Contains(_.UserID)))
						using (var client = new TwitterClient(i, this.StatusCache))
							try
							{
								category.RequestNewPage(client);
							}
							catch (WebException)
							{
							}
				}
				catch (Exception ex)
				{
					OnThrowWarning(category, ex);
					RequestedNewPage.DispatchEvent(this, new EventArgs<Category>(category));

					return false;
				}

				RequestedNewPage.DispatchEvent(this, new EventArgs<Category>(category));

				return true;
			});
		}

		void UpdateInternal(IEnumerable<Category> categories)
		{
			this.StatusCache.Clean();

			if (categories == null || !categories.Any())
				categories = this.Groups.SelectMany()
										.Freeze();
			else if (!categories.Except(this.Groups.SelectMany()).Any())
			{
				var sources = categories.SelectMany(_ => _.Filter.Sources)
										.Freeze();

				categories = this.Groups.SelectMany()
										.Where(_ => _.Filter.Sources.Intersect(sources)
																	.Any())
										.Freeze();
			}

			this.RateLimits.Where(_ => !this.Accounts.Contains(_.Account))
						   .Freeze()
						   .RunWhile(this.RateLimits.Remove);

			using (new CacheScope())
				categories.AsParallel()
						  .ForAll(_ =>
				{
					var total = 0;
					var lastup = _.LastUpdate;
					IList<IEntry> rt = null;

					Refreshing.DispatchEvent(this, new EventArgs<Category>(_));

					lock (updateTimes)
						updateTimes[_] = 0;

					if (Settings.Default.Timeline.AutoResetNewCount)
						_.Unreads = 0;

					try
					{
						foreach (var i in this.Accounts.Reverse())
							using (var client = new TwitterClient(i, this.StatusCache))
							{
								if (!client.IsAuthorized)
									lock (this)
										Authenticate(this, new EventArgs<OAuthAuthorization>(client.Authorization));

								var sr = _.Statuses.OfType<Status>().Any()
									? new StatusRange(sinceID: (_.Statuses.OfType<Status>().Where(__ => __.UserName != client.Account.Name).FirstOrDefault() ?? _.Statuses.OfType<Status>().FirstOrDefault()).StatusID, count: 200)
									: null;

								rt = _.Update(client, sr, ExceptSourceStreaming(i));
								total += rt.Count;

								if (client.RateLimit.Account != null)
								{
									this.RateLimits.RemoveWhere(__ => __.Account == i);
									this.RateLimits.Add(client.RateLimit);
								}
							}
					}
					catch (Exception ex)
					{
						OnThrowWarning(_, ex);
					}
					finally
					{
						Refreshed.DispatchEvent(this, new EventArgs<Category, IList<IEntry>>(_, lastup == DateTime.MinValue ? new List<IEntry>() : rt));
					}

					if (_.CheckUnreads && _.LastUpdate != DateTime.MinValue)
						if (Settings.Default.Timeline.AutoResetNewCount)
							_.Unreads = total;
						else
							_.Unreads += total;

					_.LastUpdate = DateTime.Now;
				});

			if (this.Groups.SelectMany().Intersect(categories).Any())
				UpdateStreaming();

			this.LastUpdate = DateTime.Now;
		}

		Func<FilterSource, bool> ExceptSourceStreaming(AccountToken token)
		{
			return source =>
			{
				if (BypassStreams)
					return true;

				var isStreamingAccount = source.Account == 0 && this.Accounts.All(IsStreaming) || source.Account == token.UserID && IsStreaming(token);

				return source.TypeMatch
				(
					(HomeFilterSource _) => !isStreamingAccount,
					(MentionsFilterSource _) => !isStreamingAccount,
					(SentFilterSource _) => !isStreamingAccount,
					(DirectMessagesReceivedFilterSource _) => !isStreamingAccount,
					(DirectMessagesSentFilterSource _) => !isStreamingAccount,
					(FavoritesFilterSource _) => !isStreamingAccount || _.UserName != null,
					_ => true
				);
			};
		}

		void UpdateStreaming()
		{
			lock (streams)
			{
				streams.Keys.Where(_ => !IsStreaming(_))
							.Freeze()
							.Run(_ =>
							{
								var s = streams[_];

								if (s.IsStreamOpen)
									s.Dispose();

								streams.Remove(_);
							});

				if (Settings.Default.Connection.EnableUserStreams)
					this.Accounts.Except(streams.Keys)
								 .Except(this.IgnoreStreamConnection)
								 .Where(_ => !streamsRetry.ContainsKey(_) || streamsRetry[_]-- <= 0)
								 .Freeze()
								 .Reverse()
								 .Run(k => streams.Add(k, CreateStream(k)));
			}
		}

		TwitterStream CreateStream(AccountToken k)
		{
			return new TwitterStream(this.StatusCache).Apply
			(
				_ => _.Connected += (sender, e) => StreamConnected.DispatchEvent(this, new EventArgs<AccountToken>(k)),
				_ => _.Disconnected += (sender, e) => StreamDisconnected.DispatchEvent(this, new EventArgs<AccountToken>(k)),
				_ => _.Friends += (sender, e) => this.Friends[_.Account] = new HashSet<UserID>(e.Value),
				_ => _.Follow += (sender, e) => this.Friends[_.Account].Add(e.Target.UserID),
				_ => _.Unfollow += (sender, e) => this.Friends[_.Account].Remove(e.Target.UserID),
				_ => _.Block += (sender, e) => this.Friends[_.Account].Remove(e.Target.UserID),
				_ => _.Status += (sender, e) =>
				{
					if (!BypassStreams)
						AppendStreamStatus(e.Value);
				},
				_ => _.DeleteStatus += (sender, e) =>
				{
					if (!BypassStreams)
						RemoveStatus(e.Value);
				},
				_ => _.Favorite += (sender, e) =>
				{
					if (!BypassStreams)
					{
						UpdateStatus(this.StatusCache.SetStatus(e.TargetStatus), s => !s.Favorited);

						foreach (var i in Category.GetInstances().Where(__ => __.Filter.Sources.OfType<FavoritesFilterSource>().Where(___ => ___.UserName == null || ___.UserName == k.Name).Any()))
							if (!i.Statuses.Contains(e.TargetStatus))
								i.Statuses.Insert(0, e.TargetStatus);
					}
				},
				_ => _.Unfavorite += (sender, e) =>
				{
					if (!BypassStreams)
					{
						UpdateStatus(this.StatusCache.SetStatus(e.TargetStatus), s => s.Favorited);

						foreach (var i in Category.GetInstances().Where(__ => __.Filter.Sources.OfType<FavoritesFilterSource>().Where(___ => ___.UserName == null || ___.UserName == k.Name).Any()))
							i.Statuses.Remove(e.TargetStatus);
					}
				},
				_ => _.DirectMessage += (sender, e) =>
				{
					if (!BypassStreams)
						AppendStreamStatus(e.Value);
				},
				_ => _.WebError += (sender, e) => StreamError(_.Account, e.Value),
				_ => _.ConnectionError += (sender, e) => StreamError(_.Account, e.Value),
				_ => _.Exception += (sender, e) => StreamError(_.Account, e.Value),
				_ => StreamInitialize.RaiseEvent(this, new EventArgs<TwitterStream>(_)),
				_ => _.Connect(new OAuthAuthorization(k))
			);
		}

		void StreamError(AccountToken account, Exception ex)
		{
			streamsRetry[account] = !streamsRetry.ContainsKey(account) ? 1 : Math.Min(streamsRetry[account] + 1, 6);
			App.Log(ex);
		}

		/// <summary>
		/// 指定したアカウントの User Streams が接続済みかどうかを取得します。
		/// </summary>
		/// <param name="account">アカウント。</param>
		/// <returns>指定したアカウントの User Streams が接続済みかどうか。</returns>
		public bool IsStreaming(AccountToken account)
		{
			if (account == null)
				return streams.Any()
					&& streams.All(_ => _.Value.IsStreamOpen && !_.Value.Terminated);
			else
				return streams.ContainsKey(account)
					&& streams[account].IsStreamOpen
					&& !streams[account].Terminated;
		}

		/// <summary>
		/// 指定したアカウントの User Streams の接続を開始します。
		/// </summary>
		/// <param name="account">アカウント。</param>
		public void BeginStreaming(AccountToken account)
		{
			if (streams.ContainsKey(account))
			{
				if (streams[account].Terminated)
					streams[account] = CreateStream(account);
			}
			else
				streams[account] = CreateStream(account);
		}

		/// <summary>
		/// 指定したアカウントの User Streams の接続を終了します。
		/// </summary>
		/// <param name="account">アカウント。</param>
		public void EndStreaming(AccountToken account)
		{
			if (streams.ContainsKey(account) &&
				streams[account].IsStreamOpen)
				streams[account].Dispose();
		}

		void AppendStreamStatus(IEntry entry)
		{
			if (entry is Status)
				this.StatusCache.SetStatus((Status)entry);

			foreach (var i in Category.GetInstances()
									  .Where(_ => _.Filter.Sources.Any(__ => (__.Account == 0 || entry.Account.UserID == __.Account) && __.DoesStreamEntryMatches(entry))))
			{
				var entries = i.Filter.FilterStatuses(EnumerableEx.Wrap(entry)).Freeze();

				i.AppendStatuses(entries, () => Refreshing.DispatchEvent(this, new EventArgs<Category>(i)), () => Refreshed.DispatchEvent(this, new EventArgs<Category, IList<IEntry>>(i, entries)));
			}
		}

		internal Task ClearStatuses()
		{
			return Task.Factory.StartNew(() =>
			{
				foreach (var i in this.Groups.SelectMany())
					i.ClearStatuses();
			});
		}

		internal Task ApplyFilter()
		{
			return Task.Factory.StartNew(() =>
			{
				foreach (var i in this.Groups.SelectMany())
					i.ApplyFilter();
			});
		}

		internal void ClearUnreads(Category category)
		{
			category.ClearUnreads();
		}

		void OnThrowWarning(Category category, Exception ex)
		{
			ThrowWarning.DispatchEvent(this, new EventArgs<Category, Exception>(category, ex));
		}

		void OnThrowError(Category category, Exception ex)
		{
			ThrowError.DispatchEvent(this, new EventArgs<Category, Exception>(category, ex));
		}

		/// <summary>
		/// このオブジェクトで使用されているリソースを開放します。
		/// </summary>
		public void Dispose()
		{
			if (!this.Disposed)
				lock (this)
				{
					streams.Run(_ => _.Value.Dispose());
					streams.Clear();
					streamsRetry.Clear();
					updateTimes.Clear();

					if (updateTimer != null)
						updateTimer.Dispose();

					urlShorteners.Dispose();
					urlExpanders.Dispose();
					filterSourceScripts.Dispose();
					filterTermsScripts.Dispose();

					if (Settings.Default != null &&
						!this.IsDesignMode)
						Save();

					Shutdown.RaiseEvent(this, EventArgs.Empty);

					this.Disposed = true;
					GC.SuppressFinalize(this);
				}
		}

		/// <summary>
		/// dtor
		/// </summary>
		~Client()
		{
			Dispose();
		}
	}
}
