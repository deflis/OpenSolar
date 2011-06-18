using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Codeplex.Data;
using Ignition;

namespace Lunar
{
	/// <summary>
	/// Twitter クライアント機能を提供します。
	/// </summary>
	public class TwitterClient : IDisposable
	{
		[ThreadStatic]
		static TwitterClient currentInstance;

		DateTime nonAuthorizedRateLimitExpires;

		/// <summary>
		/// 現在のスレッドの TwitterClient コンテキストを取得します。
		/// </summary>
		public static TwitterClient CurrentInstance
		{
			get
			{
				return currentInstance;
			}
			private set
			{
				currentInstance = value;
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
		/// 認証を取得します。
		/// </summary>
		public OAuthAuthorization Authorization
		{
			get;
			private set;
		}

		/// <summary>
		/// アカウントを取得または設定します。
		/// </summary>
		public AccountToken Account
		{
			get
			{
				return this.Authorization.Token;
			}
			set
			{
				this.Authorization.Token = value;
			}
		}

		/// <summary>
		/// つぶやきに関する機能を提供します。
		/// </summary>
		public TwitterStatuses Statuses
		{
			get;
			private set;
		}

		/// <summary>
		/// リストに関する機能を提供します。
		/// </summary>
		public TwitterLists Lists
		{
			get;
			private set;
		}

		/// <summary>
		/// ダイレクトメッセージに関する機能を提供します。
		/// </summary>
		public TwitterDirectMessages DirectMessages
		{
			get;
			private set;
		}

		/// <summary>
		/// お気に入りに関する機能を提供します。
		/// </summary>
		public TwitterFavorites Favorites
		{
			get;
			private set;
		}

		/// <summary>
		/// ユーザに関する機能を提供します。
		/// </summary>
		public TwitterUsers Users
		{
			get;
			private set;
		}

		/// <summary>
		/// 関係に関する機能を提供します。
		/// </summary>
		public TwitterFriendships Friendships
		{
			get;
			private set;
		}

		/// <summary>
		/// ブロックに関する機能を提供します。
		/// </summary>
		public TwitterBlocks Blocks
		{
			get;
			private set;
		}

		/// <summary>
		/// API 制限情報を取得します。
		/// </summary>
		public RateLimit RateLimit
		{
			get;
			private set;
		}

		/// <summary>
		/// 認証されているかどうかを取得します。
		/// </summary>
		public bool IsAuthorized
		{
			get
			{
				return this.Account != null
					&& this.Account.IsAuthorized;
			}
		}

		/// <summary>
		/// TwitterClient の新しいインスタンスを初期化します。
		/// スレッド一つにつき一つの TwitterClient のみが存在可能です。
		/// </summary>
		public TwitterClient()
			: this(null)
		{
		}

		/// <summary>
		/// アカウントを指定し、TwitterClient の新しいインスタンスを初期化します。
		/// スレッド一つにつき一つの TwitterClient のみが存在可能です。
		/// </summary>
		/// <param name="authorized">アカウント。</param>
		public TwitterClient(AccountToken authorized)
			: this(authorized, new StatusCache())
		{
		}

		/// <summary>
		/// アカウントおよびキャッシュを指定し、TwitterClient の新しいインスタンスを初期化します。
		/// スレッド一つにつき一つの TwitterClient のみが存在可能です。
		/// </summary>
		/// <param name="authorized">アカウント。</param>
		/// <param name="statusCache">キャッシュ。</param>
		public TwitterClient(AccountToken authorized, StatusCache statusCache)
		{
			if (CurrentInstance != null)
				throw new InvalidOperationException("only one TwitterClient instance allowed in a thread");

			CurrentInstance = this;
			this.Authorization = new OAuthAuthorization();
			this.Statuses = new TwitterStatuses(this);
			this.Lists = new TwitterLists(this);
			this.DirectMessages = new TwitterDirectMessages(this);
			this.Favorites = new TwitterFavorites(this);
			this.Users = new TwitterUsers(this);
			this.Friendships = new TwitterFriendships(this);
			this.Blocks = new TwitterBlocks(this);
			this.Account = authorized;
			this.StatusCache = statusCache;
		}

		/// <summary>
		/// TwitterClient により使用されているすべてのリソースを開放します。
		/// </summary>
		public void Dispose()
		{
			this.Authorization.Dispose();
			CurrentInstance = null;
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// dtor
		/// </summary>
		~TwitterClient()
		{
			Dispose();
		}

		dynamic UploadDynamic(Uri uri)
		{
			return UploadDynamic(uri, "POST", null);
		}
		dynamic UploadDynamic(Uri uri, object args)
		{
			return UploadDynamic(uri, "POST", args);
		}
		dynamic UploadDynamic(Uri uri, string method)
		{
			return UploadDynamic(uri, method, null);
		}
		dynamic UploadDynamic(Uri uri, string method, object args)
		{
			using (var client = new WebClient
			{
				Encoding = Encoding.UTF8,
			})
			{
				var rawParameters = args == null ? Enumerable.Empty<string>() : args.GetType().GetProperties().Select(_ => new
				{
					_.Name,
					Value = _.GetValue(args, null),
				})
				.Where(_ => _.Value != null)
				.Select(_ => _.Name + "=" + OAuthAuthorization.EscapeDataString(_.Value.ToString()));

				if (!string.IsNullOrEmpty(uri.Query))
				{
					rawParameters = rawParameters.Concat(uri.Query.TrimStart('?').Split('&')).Freeze();
					uri = new Uri(uri.GetLeftPart(UriPartial.Path));
				}

				var parameters = string.Join("&", rawParameters);

				if (this.IsAuthorized)
					if (string.IsNullOrEmpty(parameters))
						parameters = this.Authorization.CreateParameters(method, uri, parameters);
					else
						parameters = parameters + "&" + this.Authorization.CreateParameters(method, uri, parameters);

				SetHeaders(client);

				try
				{
					if (method == "POST")
					{
						client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

						return DynamicJson.Parse(client.UploadString(uri, method, parameters));
					}
					else
						return DynamicJson.Parse(client.UploadString(uri.AbsoluteUri + (string.IsNullOrEmpty(parameters) ? "" : "?" + parameters), method, string.Empty));
				}
				catch (WebException ex)
				{
					throw CatchWebException(this.Account, ex);
				}
			}
		}

		dynamic DownloadDynamic(Uri uri)
		{
			return DownloadDynamic(uri, ReduceAuthenticatedQueryScope.Current != null);
		}
		dynamic DownloadDynamic(Uri uri, bool reduceAuthenticatedQuery)
		{
			if (reduceAuthenticatedQuery && DateTime.Now > nonAuthorizedRateLimitExpires)
				try
				{
					return DownloadDynamicInternal(uri, null);
				}
				catch (RateLimitExceededException ex)
				{
					nonAuthorizedRateLimitExpires = ex.RateLimit.Reset;
				}
				catch
				{
				}

			return DownloadDynamicInternal(uri, this.Authorization);
		}

		dynamic DownloadDynamicInternal(Uri uri, OAuthAuthorization authorization)
		{
			using (var client = new WebClient
			{
				Encoding = Encoding.UTF8,
			})
			{
				var baseUri = uri;

				if (authorization != null && authorization.Token != null)
				{
					var ub = new UriBuilder(uri);

					if (string.IsNullOrEmpty(ub.Query))
						ub.Query = authorization.CreateParameters("GET", uri);
					else
						ub.Query = ub.Query.Substring(1) + "&" + authorization.CreateParameters("GET", uri);

					uri = ub.Uri;
				}

				SetHeaders(client);

				try
				{
					return Util.Retry(() =>
					{
						dynamic rt;

						if (CacheScope.Current != null)
							rt = DynamicJson.Parse(CacheScope.Current.ReadWithCaching(new CacheKey(this.Account, baseUri), _ => client.DownloadString(uri)));
						else
							rt = DynamicJson.Parse(client.DownloadString(uri));

						if (authorization != null)
							SetRateLimit(this.Account, client.ResponseHeaders);

						return rt;
					});
				}
				catch (WebException ex)
				{
					throw CatchWebException(authorization == null ? null : this.Account, ex);
				}
			}
		}

		void SetRateLimit(AccountToken account, WebHeaderCollection responseHeaders)
		{
			if (this.Account != null && responseHeaders != null)
			{
				var remaining = responseHeaders["X-RateLimit-Remaining"];
				var limit = responseHeaders["X-RateLimit-Limit"];
				var reset = responseHeaders["X-RateLimit-Reset"];

				if (!string.IsNullOrEmpty(remaining) &&
					!string.IsNullOrEmpty(limit) &&
					!string.IsNullOrEmpty(reset))
					this.RateLimit = new RateLimit(account, int.Parse(remaining), int.Parse(limit), new DateTime(1970, 1, 1).AddSeconds(int.Parse(reset)).ToLocalTime());
			}
		}

		Exception CatchWebException(AccountToken account, WebException ex)
		{
			if (ex.Response is HttpWebResponse)
			{
				var res = (HttpWebResponse)ex.Response;

				if (account != null)
					SetRateLimit(account, res.Headers);

				if (res.StatusCode == HttpStatusCode.Unauthorized)
					return new OAuthUnauthorizedException(ContentedWebException.Create(ex));
				else if (res.StatusCode == HttpStatusCode.BadRequest && this.RateLimit.Remaining == 0)
					return new RateLimitExceededException(this.RateLimit);
				else
					return ContentedWebException.Create(ex);
			}

			return new WebException(ex.Message, ex, ex.Status, ex.Response);
		}

		void SetHeaders(WebClient client)
		{
			client.Headers.Add(HttpRequestHeader.UserAgent, "Solar");
		}

		class CacheKey : IEquatable<CacheKey>
		{
			public AccountToken Account
			{
				get;
				private set;
			}

			public Uri Uri
			{
				get;
				private set;
			}

			public CacheKey(AccountToken account, Uri uri)
			{
				this.Account = account;
				this.Uri = uri;
			}

			public bool Equals(CacheKey other)
			{
				return this.Account == other.Account
					&& this.Uri == other.Uri;
			}

			public override bool Equals(object obj)
			{
				return obj is CacheKey ? Equals((CacheKey)obj) : base.Equals(obj);
			}

			public override int GetHashCode()
			{
				return (this.Account == null ? 0 : this.Account.GetHashCode()) ^ this.Uri.GetHashCode();
			}
		}

		/// <summary>
		/// つぶやきに関する機能を提供します。
		/// </summary>
		public class TwitterStatuses
		{
			readonly TwitterClient client;

			internal TwitterStatuses(TwitterClient client)
			{
				this.client = client;
			}

			/// <summary>
			/// パブリックタイムラインを取得します。
			/// </summary>
			/// <returns>パブリックタイムラインのつぶやき。</returns>
			public IEnumerable<Status> PublicTimeline()
			{
				return PublicTimeline(new StatusRange());
			}

			/// <summary>
			/// 指定した範囲のパブリックタイムラインを取得します。
			/// </summary>
			/// <param name="range">取得範囲。</param>
			/// <returns>パブリックタイムラインのつぶやき。</returns>
			public IEnumerable<Status> PublicTimeline(StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.Statuses.PublicTimeline(range.SinceID, range.MaxID, range.Count, range.Page)))
					if (i != null)
						yield return client.StatusCache.SetStatus(new Status(client, i));
			}

			/// <summary>
			/// ホームタイムラインを取得します。
			/// </summary>
			/// <returns>ホームタイムラインのつぶやき。</returns>
			public IEnumerable<Status> HomeTimeline()
			{
				return HomeTimeline(new StatusRange());
			}

			/// <summary>
			/// 指定した範囲のホームタイムラインを取得します。
			/// </summary>
			/// <param name="range">取得範囲。</param>
			/// <returns>ホームタイムラインのつぶやき。</returns>
			public IEnumerable<Status> HomeTimeline(StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.Statuses.HomeTimeline(range.SinceID, range.MaxID, range.Count, range.Page), false))
					if (i != null)
						yield return client.StatusCache.SetStatus(new Status(client, i));
			}

			/// <summary>
			/// 自分の RT を取得します。
			/// </summary>
			/// <returns>自分の RT。</returns>
			public IEnumerable<Status> RetweetedByMe()
			{
				return RetweetedByMe(new StatusRange());
			}

			/// <summary>
			/// 指定した範囲の自分の RT を取得します。
			/// </summary>
			/// <param name="range">取得範囲。</param>
			/// <returns>自分の RT。</returns>
			public IEnumerable<Status> RetweetedByMe(StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.Statuses.RetweetedByMe(range.SinceID, range.MaxID, range.Count, range.Page), false))
					if (i != null)
						yield return client.StatusCache.SetStatus(new Status(client, i));
			}

			/// <summary>
			/// 自分への RT を取得します。
			/// </summary>
			/// <returns>自分への RT。</returns>
			public IEnumerable<Status> RetweetedToMe()
			{
				return RetweetedToMe(new StatusRange());
			}

			/// <summary>
			/// 指定した範囲の自分への RT を取得します。
			/// </summary>
			/// <param name="range">取得範囲。</param>
			/// <returns>自分への RT。</returns>
			public IEnumerable<Status> RetweetedToMe(StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.Statuses.RetweetedToMe(range.SinceID, range.MaxID, range.Count, range.Page), false))
					if (i != null)
						yield return client.StatusCache.SetStatus(new Status(client, i));
			}

			/// <summary>
			/// RT された自分のつぶやきを取得します。
			/// </summary>
			/// <returns>RT された自分のつぶやき。</returns>
			public IEnumerable<Status> RetweetsOfMe()
			{
				return RetweetsOfMe(new StatusRange());
			}

			/// <summary>
			/// 指定した範囲の RT された自分のつぶやきを取得します。
			/// </summary>
			/// <param name="range">取得範囲。</param>
			/// <returns>RT された自分のつぶやき。</returns>
			public IEnumerable<Status> RetweetsOfMe(StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.Statuses.RetweetsOfMe(range.SinceID, range.MaxID, range.Count, range.Page), false))
					if (i != null)
						yield return client.StatusCache.SetStatus(new Status(client, i));
			}

			/// <summary>
			/// 指定したユーザ ID のユーザタイムラインを取得します。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <returns>ユーザタイムライン。</returns>
			public IEnumerable<Status> UserTimeline(UserID id)
			{
				return UserTimeline(id, new StatusRange());
			}

			/// <summary>
			/// 指定したユーザ ID および範囲のユーザタイムラインを取得します。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <param name="range">取得範囲。</param>
			/// <returns>ユーザタイムライン。</returns>
			public IEnumerable<Status> UserTimeline(UserID id, StatusRange range)
			{
				return UserTimeline(id.ToString(), range);
			}

			/// <summary>
			/// 指定したユーザ名のユーザタイムラインを取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>ユーザタイムライン。</returns>
			public IEnumerable<Status> UserTimeline(string userName)
			{
				return UserTimeline(userName, new StatusRange());
			}

			/// <summary>
			/// 指定したユーザ名および範囲のユーザタイムラインを取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <param name="range">取得範囲。</param>
			/// <returns>ユーザタイムライン。</returns>
			public IEnumerable<Status> UserTimeline(string userName, StatusRange range)
			{
				var uri = TwitterUriBuilder.Statuses.UserTimeline(userName, range.SinceID, range.MaxID, range.Count, range.Page);
				dynamic rt;

				if (ReduceAuthenticatedQueryScope.Current != null)
				{
					long id;
					var user = long.TryParse(userName, out id)
						? client.StatusCache.RetrieveUser(id, _ => null)
						: client.StatusCache.RetrieveUser(userName, _ => null);

					rt = client.DownloadDynamic(uri, user == null || !user.Protected);
				}
				else
					rt = client.DownloadDynamic(uri);

				foreach (dynamic i in rt)
					if (i != null)
						yield return client.StatusCache.SetStatus(new Status(client, i));
			}

			/// <summary>
			/// 返信を取得します。
			/// </summary>
			/// <returns>返信。</returns>
			public IEnumerable<Status> Mentions()
			{
				return Mentions(new StatusRange());
			}

			/// <summary>
			/// 指定した範囲の返信を取得します。
			/// </summary>
			/// <param name="range">取得範囲。</param>
			/// <returns>返信。</returns>
			public IEnumerable<Status> Mentions(StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.Statuses.Mentions(range.SinceID, range.MaxID, range.Count, range.Page), false))
					if (i != null)
						yield return client.StatusCache.SetStatus(new Status(client, i));
			}

			/// <summary>
			/// 指定した ID のつぶやきを取得します。
			/// </summary>
			/// <param name="id">ID。</param>
			/// <returns>つぶやき。</returns>
			public Status Get(StatusID id)
			{
				return client.StatusCache.RetrieveStatus(id, _ => new Status(client, client.DownloadDynamic(TwitterUriBuilder.Statuses.Show(_))));
			}

			/// <summary>
			/// 指定された内容のつぶやきを投稿します。
			/// </summary>
			/// <param name="status">本文。</param>
			/// <returns>投稿されたつぶやき。</returns>
			public Status Update(string status)
			{
				return Update(status, default(StatusID));
			}

			/// <summary>
			/// 返信先を指定して指定された内容のつぶやきを投稿します。
			/// </summary>
			/// <param name="status">本文。</param>
			/// <param name="inReplyToStatusID">返信先の ID。</param>
			/// <returns>投稿されたつぶやき。</returns>
			public Status Update(string status, StatusID inReplyToStatusID)
			{
				return client.StatusCache.SetStatus(new Status(client, client.UploadDynamic(TwitterUriBuilder.Statuses.Update(), new
				{
					status = status,
					in_reply_to_status_id = inReplyToStatusID == default(StatusID) ? null : (StatusID?)inReplyToStatusID,
				})));
			}

			/// <summary>
			/// 指定された ID のつぶやきを削除します。
			/// </summary>
			/// <param name="id">ID。</param>
			/// <returns>削除されたつぶやき。</returns>
			public Status Destroy(StatusID id)
			{
				return client.StatusCache.SetStatus(new Status(client, client.UploadDynamic(TwitterUriBuilder.Statuses.Destroy(id))));
			}

			/// <summary>
			/// 指定された ID のつぶやきを Retweet します。
			/// </summary>
			/// <param name="id">ID。</param>
			/// <returns>Retweet した結果のつぶやき。</returns>
			public Status Retweet(StatusID id)
			{
				return client.StatusCache.SetStatus(new Status(client, client.UploadDynamic(TwitterUriBuilder.Statuses.Retweet(id))));
			}

			/// <summary>
			/// フォローしているユーザのつぶやきを取得します。
			/// </summary>
			/// <returns>フォローしているユーザのつぶやき。</returns>
			public IEnumerable<IEnumerable<Status>> Friends()
			{
				return Friends(client.Account.UserID);
			}

			/// <summary>
			/// 指定されたユーザがフォローしているユーザのつぶやきを取得します。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <returns>フォローしているユーザのつぶやき。</returns>
			public IEnumerable<IEnumerable<Status>> Friends(UserID id)
			{
				return Friends(id.ToString());
			}

			/// <summary>
			/// 指定されたユーザがフォローしているユーザのつぶやきを取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			public IEnumerable<IEnumerable<Status>> Friends(string userName)
			{
				long cursor = -1;

				do
				{
					var page = client.DownloadDynamic(TwitterUriBuilder.Statuses.Friends(userName, cursor));

					yield return ((dynamic[])page.users).Select(_ => client.StatusCache.SetUser(new User(client, _))).Select(_ => _.Status ?? new Status(client, _));

					cursor = (long)page.next_cursor;
				}
				while (cursor != 0);
			}

			/// <summary>
			/// フォローされているユーザのつぶやきを取得します。
			/// </summary>
			/// <returns>フォローされているユーザのつぶやき。</returns>
			public IEnumerable<IEnumerable<Status>> Followers()
			{
				return Followers(client.Account.UserID);
			}

			/// <summary>
			/// 指定されたユーザがフォローされているユーザのつぶやきを取得します。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <returns>フォローされているユーザのつぶやき。</returns>
			public IEnumerable<IEnumerable<Status>> Followers(UserID id)
			{
				return Followers(id.ToString());
			}

			/// <summary>
			/// 指定されたユーザがフォローされているユーザのつぶやきを取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>フォローされているユーザのつぶやき。</returns>
			public IEnumerable<IEnumerable<Status>> Followers(string userName)
			{
				long cursor = -1;

				do
				{
					var page = client.DownloadDynamic(TwitterUriBuilder.Statuses.Followers(userName, cursor));

					yield return ((dynamic[])page.users).Select(_ => client.StatusCache.SetUser(new User(client, _))).Select(_ => _.Status ?? new Status(client, _));

					cursor = (long)page.next_cursor;
				}
				while (cursor != 0);
			}

			/// <summary>
			/// 指定された文字列で Twitter 検索します。
			/// </summary>
			/// <param name="query">検索文字列。</param>
			/// <returns>検索結果。</returns>
			public IEnumerable<Status> Search(string query)
			{
				return Search(query, new StatusRange());
			}

			/// <summary>
			/// 指定された文字列で Twitter 検索します。
			/// </summary>
			/// <param name="query">検索文字列。</param>
			/// <param name="range">取得範囲。</param>
			/// <returns>検索結果。</returns>
			public IEnumerable<Status> Search(string query, StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamicInternal(TwitterUriBuilder.Search.Query(query, range.SinceID, range.MaxID, range.Count, range.Page), null).results)
					if (i != null)
						yield return client.StatusCache.SetStatus(new SearchResult(client, i));
			}
		}

		/// <summary>
		/// リストに関する機能を提供します。
		/// </summary>
		public class TwitterLists
		{
			readonly TwitterClient client;

			internal TwitterLists(TwitterClient client)
			{
				this.client = client;
			}

			/// <summary>
			/// リストのタイムラインを取得します。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listID">リストの ID。</param>
			/// <returns>リストのタイムライン。</returns>
			public IEnumerable<Status> Statuses(UserID id, ListID listID)
			{
				return Statuses(id.ToString(), listID.ToString(), new StatusRange());
			}

			/// <summary>
			/// 指定した範囲のリストのタイムラインを取得します。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listID">リストの ID。</param>
			/// <param name="range">取得範囲。</param>
			/// <returns>リストのタイムライン。</returns>
			public IEnumerable<Status> Statuses(UserID id, ListID listID, StatusRange range)
			{
				return Statuses(id.ToString(), listID.ToString(), range);
			}

			/// <summary>
			/// リストのタイムラインを取得します。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>リストのタイムライン。</returns>
			public IEnumerable<Status> Statuses(UserID id, string listName)
			{
				return Statuses(id.ToString(), listName, new StatusRange());
			}

			/// <summary>
			/// 指定した範囲のリストのタイムラインを取得します。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listName">リスト名。</param>
			/// <param name="range">取得範囲。</param>
			/// <returns>リストのタイムライン。</returns>
			public IEnumerable<Status> Statuses(UserID id, string listName, StatusRange range)
			{
				return Statuses(id.ToString(), listName, range);
			}

			/// <summary>
			/// リストのタイムラインを取得します。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listID">リストの ID。</param>
			/// <returns>リストのタイムライン。</returns>
			public IEnumerable<Status> Statuses(string userName, ListID listID)
			{
				return Statuses(userName, listID.ToString(), new StatusRange());
			}

			/// <summary>
			/// 指定した範囲のリストのタイムラインを取得します。
			/// </summary>
			/// <param name="userName">リストのユーザ ID。</param>
			/// <param name="listID">リストの ID。</param>
			/// <param name="range">取得範囲。</param>
			/// <returns>リストのタイムライン。</returns>
			public IEnumerable<Status> Statuses(string userName, ListID listID, StatusRange range)
			{
				return Statuses(userName, listID.ToString(), range);
			}

			/// <summary>
			/// リストのタイムラインを取得します。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>リストのタイムライン。</returns>
			public IEnumerable<Status> Statuses(string userName, string listName)
			{
				return Statuses(userName, listName, new StatusRange());
			}

			/// <summary>
			/// 指定した範囲のリストのタイムラインを取得します。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listName">リスト名。</param>
			/// <param name="range">取得範囲。</param>
			/// <returns>リストのタイムライン。</returns>
			public IEnumerable<Status> Statuses(string userName, string listName, StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.Lists.Statuses(userName, listName, range.SinceID, range.MaxID, range.Count, range.Page), false))
					if (i != null)
						yield return client.StatusCache.SetStatus(new Status(client, i));
			}

			/// <summary>
			/// 新しいリストを作成します。
			/// </summary>
			/// <param name="listName">リスト名。</param>
			/// <param name="mode">リストの公開種別。</param>
			/// <param name="description">リストの解説文。</param>
			/// <returns>作成されたリスト。</returns>
			public List Create(string listName, ListMode mode, string description)
			{
				return new List(client, client.UploadDynamic(TwitterUriBuilder.Lists.Create(client.Account.Name), new
				{
					name = listName,
					mode = mode == ListMode.None ? null : mode == ListMode.Public ? "public" : "private",
					description,
				}));
			}

			/// <summary>
			/// リストの情報を変更します。省略されたパラメータは変更されません。
			/// </summary>
			/// <param name="listID">変更するリストの ID。</param>
			/// <param name="newListName">新しいリスト名。</param>
			/// <param name="mode">新しい公開種別。</param>
			/// <param name="description">新しい解説文。</param>
			/// <returns>変更されたリスト。</returns>
			public List Update(ListID listID, string newListName = null, ListMode mode = ListMode.None, string description = null)
			{
				return Update(listID.ToString(), newListName, mode, description);
			}

			/// <summary>
			/// リストの情報を変更します。省略されたパラメータは変更されません。
			/// </summary>
			/// <param name="listName">変更するリストの ID。</param>
			/// <param name="newListName">新しいリスト名。</param>
			/// <param name="mode">新しい公開種別。</param>
			/// <param name="description">新しい解説文。</param>
			/// <returns>変更されたリスト。</returns>
			public List Update(string listName, string newListName = null, ListMode mode = ListMode.None, string description = null)
			{
				return new List(client, client.UploadDynamic(TwitterUriBuilder.Lists.Update(client.Account.UserID.ToString(), listName), new
				{
					name = newListName,
					mode = mode == ListMode.None ? null : mode == ListMode.Public ? "public" : "private",
					description,
				}));
			}

			/// <summary>
			/// 現在のアカウントのリストの一覧を取得します。
			/// </summary>
			/// <returns>リストの一覧。</returns>
			public IEnumerable<List> Index()
			{
				return Index(client.Account.UserID);
			}

			/// <summary>
			/// 指定されたユーザのリストの一覧を取得します。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <returns>リストの一覧。</returns>
			public IEnumerable<List> Index(UserID id)
			{
				return Index(id.ToString());
			}

			/// <summary>
			/// 指定されたユーザのリストの一覧を取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>リストの一覧。</returns>
			public IEnumerable<List> Index(string userName)
			{
				long cursor = -1;

				do
				{
					var page = client.DownloadDynamic(TwitterUriBuilder.Lists.Index(userName, cursor), false);

					foreach (var i in page.lists)
						yield return new List(client, i);

					cursor = (long)page.next_cursor;
				}
				while (cursor != 0);
			}

			/// <summary>
			/// 現在のアカウントがフォローしているリストの一覧を取得します。
			/// </summary>
			/// <returns>フォローしているリストの一覧。</returns>
			public IEnumerable<List> Subscriptions()
			{
				return Subscriptions(client.Account.UserID);
			}

			/// <summary>
			/// 指定されたユーザがフォローしているリストの一覧を取得します。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <returns>フォローしているリストの一覧。</returns>
			public IEnumerable<List> Subscriptions(UserID id)
			{
				return Subscriptions(id.ToString());
			}

			/// <summary>
			/// 指定されたユーザがフォローしているリストの一覧を取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>フォローしているリストの一覧。</returns>
			public IEnumerable<List> Subscriptions(string userName)
			{
				long cursor = -1;

				do
				{
					var page = client.DownloadDynamic(TwitterUriBuilder.Lists.Subscriptions(userName, cursor), false);

					foreach (var i in page.lists)
						yield return new List(client, i);

					cursor = (long)page.next_cursor;
				}
				while (cursor != 0);
			}

			/// <summary>
			/// リストをフォローしているユーザを取得します。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listID">リスト ID。</param>
			/// <returns>リストをフォローしているユーザ。</returns>
			public IEnumerable<User> Subscribers(UserID id, ListID listID)
			{
				return Subscribers(id.ToString(), listID.ToString());
			}

			/// <summary>
			/// リストをフォローしているユーザを取得します。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>リストをフォローしているユーザ。</returns>
			public IEnumerable<User> Subscribers(UserID id, string listName)
			{
				return Subscribers(id.ToString(), listName);
			}

			/// <summary>
			/// リストをフォローしているユーザを取得します。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listID">リスト ID。</param>
			/// <returns>リストをフォローしているユーザ。</returns>
			public IEnumerable<User> Subscribers(string userName, ListID listID)
			{
				return Subscribers(userName, listID.ToString());
			}

			/// <summary>
			/// リストをフォローしているユーザを取得します。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>リストをフォローしているユーザ。</returns>
			public IEnumerable<User> Subscribers(string userName, string listName)
			{
				long cursor = -1;

				do
				{
					var page = client.DownloadDynamic(TwitterUriBuilder.Lists.Subscribers(userName, listName, cursor), false);

					foreach (var i in page.users)
						yield return new User(client, i);

					cursor = (long)page.next_cursor;
				}
				while (cursor != 0);
			}

			/// <summary>
			/// 指定したリストをフォローします。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listID">リスト ID。</param>
			/// <returns>フォローしたリスト。</returns>
			public List Subscribe(UserID id, ListID listID)
			{
				return Subscribe(id, listID.ToString());
			}

			/// <summary>
			/// 指定したリストをフォローします。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>フォローしたリスト。</returns>
			public List Subscribe(UserID id, string listName)
			{
				return Subscribe(id, listName);
			}

			/// <summary>
			/// 指定したリストをフォローします。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listID">リスト ID。</param>
			/// <returns>フォローしたユーザ。</returns>
			public List Subscribe(string userName, ListID listID)
			{
				return Subscribe(userName, listID.ToString());
			}

			/// <summary>
			/// 指定したリストをフォローします。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>フォローしたリスト。</returns>
			public List Subscribe(string userName, string listName)
			{
				return new List(client, client.UploadDynamic(TwitterUriBuilder.Lists.Subscribe(userName, listName)));
			}

			/// <summary>
			/// 指定したリストをアンフォローします。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listID">リスト ID。</param>
			/// <returns>アンフォローしたリスト。</returns>
			public List Unsubscribe(UserID id, ListID listID)
			{
				return Unsubscribe(id, listID.ToString());
			}

			/// <summary>
			/// 指定したリストをアンフォローします。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>アンフォローしたリスト。</returns>
			public List Unsubscribe(UserID id, string listName)
			{
				return Unsubscribe(id, listName);
			}

			/// <summary>
			/// 指定したリストをアンフォローします。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listID">リスト ID。</param>
			/// <returns>アンフォローしたリスト。</returns>
			public List Unsubscribe(string userName, ListID listID)
			{
				return Unsubscribe(userName, listID.ToString());
			}

			/// <summary>
			/// 指定したリストをアンフォローします。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>アンフォローしたリスト。</returns>
			public List Unsubscribe(string userName, string listName)
			{
				return new List(client, client.UploadDynamic(TwitterUriBuilder.Lists.Subscribe(userName, listName), "DELETE"));
			}

			/// <summary>
			/// リスト情報を取得します。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listID">リスト ID。</param>
			/// <returns>リスト。</returns>
			public List Get(UserID id, ListID listID)
			{
				return Get(id.ToString(), listID.ToString());
			}

			/// <summary>
			/// リスト情報を取得します。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>リスト。</returns>
			public List Get(UserID id, string listName)
			{
				return Get(id.ToString(), listName);
			}

			/// <summary>
			/// リスト情報を取得します。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listID">リスト ID。</param>
			/// <returns>リスト。</returns>
			public List Get(string userName, ListID listID)
			{
				return Get(userName, listID.ToString());
			}

			/// <summary>
			/// リスト情報を取得します。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>リスト。</returns>
			public List Get(string userName, string listName)
			{
				return new List(client, client.DownloadDynamic(TwitterUriBuilder.Lists.Show(userName, listName), false));
			}

			/// <summary>
			/// 指定したリストを削除します。
			/// </summary>
			/// <param name="listID">リスト ID。</param>
			/// <returns>削除されたリスト。</returns>
			public List Destroy(ListID listID)
			{
				return Destroy(listID.ToString());
			}

			/// <summary>
			/// 指定したリストを削除します。
			/// </summary>
			/// <param name="listName">リスト名。</param>
			/// <returns>削除されたリスト。</returns>
			public List Destroy(string listName)
			{
				return new List(client, client.UploadDynamic(TwitterUriBuilder.Lists.Destroy(client.Account.UserID.ToString(), listName), "DELETE"));
			}

			/// <summary>
			/// 現在のアカウントのユーザがフォローされているリストの一覧を取得します。
			/// </summary>
			/// <returns>フォローされているリストの一覧。</returns>
			public IEnumerable<List> Memberships()
			{
				return Memberships(client.Account.UserID);
			}

			/// <summary>
			/// 指定されたユーザがフォローされているリストの一覧を取得します。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <returns>フォローされているリストの一覧。</returns>
			public IEnumerable<List> Memberships(UserID id)
			{
				return Memberships(id.ToString());
			}

			/// <summary>
			/// 指定されたユーザがフォローされているリストの一覧を取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>フォローされているリストの一覧。</returns>
			public IEnumerable<List> Memberships(string userName)
			{
				long cursor = -1;

				do
				{
					var page = client.DownloadDynamic(TwitterUriBuilder.Lists.Memberships(userName, cursor), false);

					foreach (var i in page.lists)
						yield return new List(client, i);

					cursor = (long)page.next_cursor;
				}
				while (cursor != 0);
			}

			/// <summary>
			/// リストがフォローしているユーザを取得します。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listID">リスト ID。</param>
			/// <returns>リストがフォローしているユーザ。</returns>
			public IEnumerable<User> Members(UserID id, ListID listID)
			{
				return Members(id.ToString(), listID.ToString());
			}

			/// <summary>
			/// リストがフォローしているユーザを取得します。
			/// </summary>
			/// <param name="id">リストのユーザ ID。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>リストがフォローしているユーザ。</returns>
			public IEnumerable<User> Members(UserID id, string listName)
			{
				return Members(id.ToString(), listName);
			}

			/// <summary>
			/// リストがフォローしているユーザを取得します。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listID">リスト ID。</param>
			/// <returns>リストがフォローしているユーザ。</returns>
			public IEnumerable<User> Members(string userName, ListID listID)
			{
				return Members(userName, listID.ToString());
			}

			/// <summary>
			/// リストがフォローしているユーザを取得します。
			/// </summary>
			/// <param name="userName">リストのユーザ名。</param>
			/// <param name="listName">リスト名。</param>
			/// <returns>リストがフォローしているユーザ。</returns>
			public IEnumerable<User> Members(string userName, string listName)
			{
				long cursor = -1;

				do
				{
					var page = client.DownloadDynamic(TwitterUriBuilder.Lists.Members(userName, listName, cursor), false);

					foreach (var i in page.users)
						yield return new User(client, i);

					cursor = (long)page.next_cursor;
				}
				while (cursor != 0);
			}

			/// <summary>
			/// 指定したユーザをリストに追加します。
			/// </summary>
			/// <param name="listID">リスト ID。</param>
			/// <param name="add">追加するユーザ ID。</param>
			/// <returns>ユーザを追加したリスト。</returns>
			public List AddMember(ListID listID, UserID add)
			{
				return AddMember(listID.ToString(), add.ToString());
			}

			/// <summary>
			/// 指定したユーザをリストに追加します。
			/// </summary>
			/// <param name="listID">リスト ID。</param>
			/// <param name="add">追加するユーザ名。</param>
			/// <returns>ユーザを追加したリスト。</returns>
			public List AddMember(ListID listID, string add)
			{
				return AddMember(listID.ToString(), add);
			}

			/// <summary>
			/// 指定したユーザをリストに追加します。
			/// </summary>
			/// <param name="listName">リスト名。</param>
			/// <param name="add">追加するユーザ ID。</param>
			/// <returns>ユーザを追加したリスト。</returns>
			public List AddMember(string listName, UserID add)
			{
				return AddMember(listName, add.ToString());
			}

			/// <summary>
			/// 指定したユーザをリストに追加します。
			/// </summary>
			/// <param name="listName">リスト名。</param>
			/// <param name="add">追加するユーザ名。</param>
			/// <returns>ユーザを追加したリスト。</returns>
			public List AddMember(string listName, string add)
			{
				return new List(client, client.UploadDynamic(TwitterUriBuilder.Lists.Members(client.Account.Name, listName, add)));
			}

			/// <summary>
			/// 指定したユーザをリストから削除します。
			/// </summary>
			/// <param name="listID">リスト ID。</param>
			/// <param name="remove">削除するユーザ ID。</param>
			/// <returns>ユーザを削除したリスト。</returns>
			public List RemoveMember(ListID listID, UserID remove)
			{
				return RemoveMember(listID.ToString(), remove.ToString());
			}

			/// <summary>
			/// 指定したユーザをリストから削除します。
			/// </summary>
			/// <param name="listID">リスト ID。</param>
			/// <param name="remove">削除するユーザ名。</param>
			/// <returns>ユーザを削除したリスト。</returns>
			public List RemoveMember(ListID listID, string remove)
			{
				return RemoveMember(listID.ToString(), remove);
			}

			/// <summary>
			/// 指定したユーザをリストから削除します。
			/// </summary>
			/// <param name="listName">リスト名。</param>
			/// <param name="remove">削除するユーザ ID。</param>
			/// <returns>ユーザを削除したリスト。</returns>
			public List RemoveMember(string listName, UserID remove)
			{
				return RemoveMember(listName, remove.ToString());
			}

			/// <summary>
			/// 指定したユーザをリストから削除します。
			/// </summary>
			/// <param name="listName">リスト名。</param>
			/// <param name="remove">削除するユーザ名。</param>
			/// <returns>ユーザを削除したリスト。</returns>
			public List RemoveMember(string listName, string remove)
			{
				return new List(client, client.UploadDynamic(TwitterUriBuilder.Lists.Members(client.Account.Name, listName, remove), "DELETE"));
			}
		}

		/// <summary>
		/// ダイレクトメッセージに関する機能を提供します。
		/// </summary>
		public class TwitterDirectMessages
		{
			readonly TwitterClient client;

			internal TwitterDirectMessages(TwitterClient client)
			{
				this.client = client;
			}

			/// <summary>
			/// 受信したダイレクトメッセージを取得します。
			/// </summary>
			/// <returns>受信したダイレクトメッセージ。</returns>
			public IEnumerable<DirectMessage> Received()
			{
				return Received(new StatusRange());
			}

			/// <summary>
			/// 受信したダイレクトメッセージを取得します。
			/// </summary>
			/// <param name="range">取得範囲。</param>
			/// <returns>受信したダイレクトメッセージ。</returns>
			public IEnumerable<DirectMessage> Received(StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.DirectMessages.Received(range.SinceID, range.MaxID, range.Count, range.Page), false))
					if (i != null)
						yield return new DirectMessage(client, i);
			}

			/// <summary>
			/// 送信したダイレクトメッセージを取得します。
			/// </summary>
			/// <returns>送信したダイレクトメッセージ。</returns>
			public IEnumerable<DirectMessage> Sent()
			{
				return Sent(new StatusRange());
			}

			/// <summary>
			/// 送信したダイレクトメッセージを取得します。
			/// </summary>
			/// <param name="range">取得範囲。</param>
			/// <returns>送信したダイレクトメッセージ。</returns>
			public IEnumerable<DirectMessage> Sent(StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.DirectMessages.Sent(range.SinceID, range.MaxID, range.Count, range.Page), false))
					if (i != null)
						yield return new DirectMessage(client, i);
			}

			/// <summary>
			/// 指定したユーザにダイレクトメッセージを送信します。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <param name="text">ユーザ名。</param>
			/// <returns>送信されたダイレクトメッセージ。</returns>
			public DirectMessage Send(UserID id, string text)
			{
				return Send(id.ToString(), text);
			}

			/// <summary>
			/// 指定したユーザにダイレクトメッセージを送信します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <param name="text">ユーザ名。</param>
			/// <returns>送信されたダイレクトメッセージ。</returns>
			public DirectMessage Send(string userName, string text)
			{
				return new DirectMessage(client, client.UploadDynamic(TwitterUriBuilder.DirectMessages.New(), new
				{
					user = userName,
					text,
				}));
			}

			/// <summary>
			/// 指定したダイレクトメッセージを削除します。
			/// </summary>
			/// <param name="id">ID。</param>
			/// <returns>削除されたダイレクトメッセージ。</returns>
			public DirectMessage Destroy(StatusID id)
			{
				return new DirectMessage(client, client.UploadDynamic(TwitterUriBuilder.DirectMessages.Destroy(id), "DELETE"));
			}
		}

		/// <summary>
		/// お気に入りに関する機能を提供します。
		/// </summary>
		public class TwitterFavorites
		{
			readonly TwitterClient client;

			internal TwitterFavorites(TwitterClient client)
			{
				this.client = client;
			}

			/// <summary>
			/// 現在のアカウントのお気に入りを取得します。
			/// </summary>
			/// <returns>お気に入りの一覧。</returns>
			public IEnumerable<Status> Index()
			{
				return Index(1);
			}

			/// <summary>
			/// 指定したページの現在のアカウントのお気に入りを取得します。
			/// </summary>
			/// <param name="page">ページ。</param>
			/// <returns>お気に入りの一覧。</returns>
			public IEnumerable<Status> Index(int page)
			{
				return Index(client.Account.UserID, page);
			}

			/// <summary>
			/// 指定したユーザのお気に入りを取得します。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <returns>お気に入りの一覧。</returns>
			public IEnumerable<Status> Index(UserID id)
			{
				return Index(id, 1);
			}

			/// <summary>
			/// 指定したユーザの指定したページのお気に入りを取得します。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <param name="page">ページ。</param>
			/// <returns>お気に入りの一覧。</returns>
			public IEnumerable<Status> Index(UserID id, int page)
			{
				return Index(id.ToString(), page);
			}

			/// <summary>
			/// 指定したユーザのお気に入りを取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>お気に入りの一覧。</returns>
			public IEnumerable<Status> Index(string userName)
			{
				return Index(userName, 1);
			}

			/// <summary>
			/// 指定したユーザの指定したページのお気に入りを取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <param name="page">ページ。</param>
			/// <returns>お気に入りの一覧。</returns>
			public IEnumerable<Status> Index(string userName, int page)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.Favorites.Index(userName, page), false))
					if (i != null)
						yield return client.StatusCache.SetStatus(new Status(client, i));
			}

			/// <summary>
			/// 指定されたつぶやきをお気に入りに追加します。
			/// </summary>
			/// <param name="id">ID。</param>
			/// <returns>お気に入りに追加されたつぶやき。</returns>
			public Status Create(StatusID id)
			{
				return client.StatusCache.SetStatus(new Status(client, client.UploadDynamic(TwitterUriBuilder.Favorites.Create(id))));
			}

			/// <summary>
			/// 指定されたつぶやきをお気に入りから削除します。
			/// </summary>
			/// <param name="id">ID。</param>
			/// <returns>お気に入りから削除されたつぶやき。</returns>
			public Status Destroy(StatusID id)
			{
				return client.StatusCache.SetStatus(new Status(client, client.UploadDynamic(TwitterUriBuilder.Favorites.Destroy(id), "DELETE")));
			}
		}

		/// <summary>
		/// ユーザに関する機能を提供します。
		/// </summary>
		public class TwitterUsers
		{
			readonly TwitterClient client;

			internal TwitterUsers(TwitterClient client)
			{
				this.client = client;
			}

			/// <summary>
			/// 指定したユーザの情報を取得します。
			/// </summary>
			/// <param name="id">ID。</param>
			/// <returns>ユーザ情報。</returns>
			public User Get(UserID id)
			{
				return client.StatusCache.RetrieveUser(id, _ => new User(client, client.DownloadDynamic(TwitterUriBuilder.Users.Show(_.ToString()))));
			}

			/// <summary>
			/// 指定したユーザの情報を取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>ユーザ情報。</returns>
			public User Get(string userName)
			{
				return client.StatusCache.RetrieveUser(userName, _ => new User(client, client.DownloadDynamic(TwitterUriBuilder.Users.Show(_))));
			}

			/// <summary>
			/// 指定された文字列でユーザを検索します。
			/// </summary>
			/// <param name="query">検索文字列。</param>
			/// <returns>ユーザのつぶやき。</returns>
			public IEnumerable<Status> Search(string query)
			{
				return Search(query, new StatusRange());
			}

			/// <summary>
			/// 指定された文字列でユーザを検索します。
			/// </summary>
			/// <param name="query">検索文字列。</param>
			/// <param name="range">取得範囲。</param>
			/// <returns>ユーザのつぶやき。</returns>
			public IEnumerable<Status> Search(string query, StatusRange range)
			{
				foreach (dynamic i in client.DownloadDynamic(TwitterUriBuilder.Users.Search(query, range.Count, range.Page), false))
				{
					var rt = client.StatusCache.SetUser(new User(client, i));

					yield return rt.Status ?? new Status(client, rt);
				}
			}
		}

		/// <summary>
		/// 関係に関する機能を提供します。
		/// </summary>
		public class TwitterFriendships
		{
			TwitterClient client;

			internal TwitterFriendships(TwitterClient client)
			{
				this.client = client;
			}

			/// <summary>
			/// 現在のアカウントと指定されたユーザの関係を取得します。
			/// </summary>
			/// <param name="targetID">対象のユーザ ID。</param>
			/// <returns>関係。</returns>
			public Relationship Get(UserID targetID)
			{
				return new Relationship(client.DownloadDynamic(TwitterUriBuilder.Friendships.Show(null, targetID), false));
			}

			/// <summary>
			/// 現在のアカウントと指定されたユーザの関係を取得します。
			/// </summary>
			/// <param name="targetName">対象のユーザ名。</param>
			/// <returns>関係。</returns>
			public Relationship Get(string targetName)
			{
				return new Relationship(client.DownloadDynamic(TwitterUriBuilder.Friendships.Show(null, targetName), false));
			}

			/// <summary>
			/// 指定されたユーザの間の関係を取得します。
			/// </summary>
			/// <param name="sourceID">元のユーザ ID。</param>
			/// <param name="targetID">対象のユーザ ID。</param>
			/// <returns>関係。</returns>
			public Relationship Get(UserID sourceID, UserID targetID)
			{
				return new Relationship(client.DownloadDynamic(TwitterUriBuilder.Friendships.Show(sourceID, targetID), false));
			}

			/// <summary>
			/// 指定されたユーザの間の関係を取得します。
			/// </summary>
			/// <param name="sourceID">元のユーザ ID。</param>
			/// <param name="targetName">対象のユーザ名。</param>
			/// <returns>関係。</returns>
			public Relationship Get(UserID sourceID, string targetName)
			{
				return new Relationship(client.DownloadDynamic(TwitterUriBuilder.Friendships.Show(sourceID, targetName), false));
			}

			/// <summary>
			/// 指定されたユーザの間の関係を取得します。
			/// </summary>
			/// <param name="sourceName">元のユーザ名。</param>
			/// <param name="targetID">対象のユーザ ID。</param>
			/// <returns>関係。</returns>
			public Relationship Get(string sourceName, UserID targetID)
			{
				return new Relationship(client.DownloadDynamic(TwitterUriBuilder.Friendships.Show(sourceName, targetID), false));
			}

			/// <summary>
			/// 指定されたユーザの間の関係を取得します。
			/// </summary>
			/// <param name="sourceName">元のユーザ名。</param>
			/// <param name="targetName">対象のユーザ名。</param>
			/// <returns>関係。</returns>
			public Relationship Get(string sourceName, string targetName)
			{
				return new Relationship(client.DownloadDynamic(TwitterUriBuilder.Friendships.Show(sourceName, targetName), false).relationship);
			}

			/// <summary>
			/// 指定したユーザをフォローします。
			/// </summary>
			/// <param name="id">ID。</param>
			/// <returns>フォローしたユーザ。</returns>
			public User Create(UserID id)
			{
				return Create(id.ToString());
			}

			/// <summary>
			/// 指定したユーザをフォローします。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>フォローしたユーザ。</returns>
			public User Create(string userName)
			{
				return client.StatusCache.SetUser(new User(client, client.UploadDynamic(TwitterUriBuilder.Friendships.Create(userName))));
			}

			/// <summary>
			/// 指定したユーザをアンフォローします。
			/// </summary>
			/// <param name="id">ID。</param>
			/// <returns>アンフォローしたユーザ。</returns>
			public User Destroy(UserID id)
			{
				return Destroy(id.ToString());
			}

			/// <summary>
			/// 指定したユーザをアンフォローします。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>アンフォローしたユーザ。</returns>
			public User Destroy(string userName)
			{
				return client.StatusCache.SetUser(new User(client, client.UploadDynamic(TwitterUriBuilder.Friendships.Destroy(userName), "DELETE")));
			}

			/// <summary>
			/// 自分へのフォローリクエストを取得します。
			/// </summary>
			/// <returns>自分へのフォローリクエスト。</returns>
			public IEnumerable<UserID> Incoming()
			{
				long cursor = -1;

				do
				{
					var page = client.DownloadDynamic(TwitterUriBuilder.Friendships.Incoming(cursor), false);

					foreach (var i in page.ids)
						yield return i;

					cursor = (long)page.next_cursor;
				}
				while (cursor != 0);
			}

			/// <summary>
			/// 自分のフォローリクエストを取得します。
			/// </summary>
			/// <returns>自分のフォローリクエスト。</returns>
			public IEnumerable<UserID> Outgoing()
			{
				long cursor = -1;

				do
				{
					var page = client.DownloadDynamic(TwitterUriBuilder.Friendships.Outgoing(cursor), false);

					foreach (var i in page.ids)
						yield return i;

					cursor = (long)page.next_cursor;
				}
				while (cursor != 0);
			}
		}

		/// <summary>
		/// ブロックに関する機能を提供します。
		/// </summary>
		public class TwitterBlocks
		{
			TwitterClient client;

			internal TwitterBlocks(TwitterClient client)
			{
				this.client = client;
			}

			/// <summary>
			/// ブロックしているユーザの一覧を取得します。
			/// </summary>
			/// <returns>ブロックしているユーザの一覧。</returns>
			public IEnumerable<Status> Blocking()
			{
				foreach (var i in client.DownloadDynamic(TwitterUriBuilder.Blocks.Blocking(), false))
				{
					var rt = client.StatusCache.SetUser(new User(client, i));

					yield return rt.Status ?? new Status(client, rt);
				}
			}

			/// <summary>
			/// 指定したユーザをブロックします。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <returns>ブロックしたユーザ。</returns>
			public User Create(UserID id)
			{
				return client.StatusCache.SetUser(new User(client, client.UploadDynamic(TwitterUriBuilder.Blocks.Create(id))));
			}

			/// <summary>
			/// 指定したユーザをブロックします。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>ブロックしたユーザ。</returns>
			public User Create(string userName)
			{
				return client.StatusCache.SetUser(new User(client, client.UploadDynamic(TwitterUriBuilder.Blocks.Create(userName))));
			}

			/// <summary>
			/// 指定したユーザをアンブロックします。
			/// </summary>
			/// <param name="id">ユーザ ID。</param>
			/// <returns>アンブロックしたユーザ。</returns>
			public User Destroy(UserID id)
			{
				return client.StatusCache.SetUser(new User(client, client.UploadDynamic(TwitterUriBuilder.Blocks.Destroy(id), "DELETE")));
			}

			/// <summary>
			/// 指定したユーザをアンブロックします。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>アンブロックしたユーザ。</returns>
			public User Destroy(string userName)
			{
				return client.StatusCache.SetUser(new User(client, client.UploadDynamic(TwitterUriBuilder.Blocks.Destroy(userName), "DELETE")));
			}

			/// <summary>
			/// 指定したユーザをブロックしているかどうかを取得します。
			/// </summary>
			/// <param name="id">ユーザ名。</param>
			/// <returns>ブロックしているかどうか。</returns>
			public bool Exists(UserID id)
			{
				try
				{
					client.DownloadDynamic(TwitterUriBuilder.Blocks.Exists(id), false);

					return true;
				}
				catch
				{
					return false;
				}
			}

			/// <summary>
			/// 指定したユーザをブロックしているかどうかを取得します。
			/// </summary>
			/// <param name="userName">ユーザ名。</param>
			/// <returns>ブロックしているかどうか。</returns>
			public bool Exists(string userName)
			{
				try
				{
					client.DownloadDynamic(TwitterUriBuilder.Blocks.Exists(userName), false);

					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		/// <summary>
		/// 指定されたユーザをスパムとして報告します。
		/// </summary>
		/// <param name="id">ユーザ ID。</param>
		public void ReportSpam(UserID id)
		{
			this.UploadDynamic(TwitterUriBuilder.ReportSpam(id), false);
		}

		/// <summary>
		/// 指定されたユーザをスパムとして報告します。
		/// </summary>
		/// <param name="userName">ユーザ名。</param>
		public void ReportSpam(string userName)
		{
			this.UploadDynamic(TwitterUriBuilder.ReportSpam(userName), false);
		}
	}

	internal static class Util
	{
		public static TReturn Retry<TReturn>(Func<TReturn> func, int retryCount = 3)
		{
			int i = 0;
			while (true)
			{
				try
				{
					var ret = func();
					return ret;
				}
				catch (Exception)
				{
					Thread.Sleep(1000);
					if (++i > retryCount)
					{
						throw;
					}
				}
			}
		}

		public static void Retry(Action action, int retryCount = 3)
		{
			int i = 0;
			while (true)
			{
				try
				{
					action();
					return;
				}
				catch (Exception)
				{
					Thread.Sleep(1000);
					if (++i > retryCount)
					{
						throw;
					}
				}
			}
		}

	}
}
