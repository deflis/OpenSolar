using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Codeplex.Data;
using Ignition;
using Ignition.Presentation;

namespace Lunar
{
	/// <summary>
	/// User Streams へのアクセスを提供します。
	/// </summary>
	public class TwitterStream : IDisposable
	{
		Thread streamingThread;
		Timer reconnectTimer;

		/// <summary>
		/// 接続されたときに発生します。
		/// </summary>
		public event EventHandler Connected;
		/// <summary>
		/// 切断されたときに発生します。
		/// </summary>
		public event EventHandler Disconnected;
		/// <summary>
		/// Web 例外が発生したときに発生します。
		/// </summary>
		public event EventHandler<EventArgs<WebException>> WebError;
		/// <summary>
		/// 接続エラーが発生したときに発生します。
		/// </summary>
		public event EventHandler<EventArgs<IOException>> ConnectionError;
		/// <summary>
		/// その他の例外が発生したときに発生します。
		/// </summary>
		public event EventHandler<EventArgs<Exception>> Exception;
		/// <summary>
		/// フォローリストを受信したときに発生します。
		/// </summary>
		public event EventHandler<EventArgs<IList<UserID>>> Friends;
		/// <summary>
		/// つぶやきを受信したときに発生します。
		/// </summary>
		public event EventHandler<EventArgs<Status>> Status;
		/// <summary>
		/// 削除通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<EventArgs<StatusID>> DeleteStatus;
		/// <summary>
		/// ダイレクトメッセージを受信したときに発生します。
		/// </summary>
		public event EventHandler<EventArgs<DirectMessage>> DirectMessage;
		/// <summary>
		/// フォローした通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Follow;
		/// <summary>
		/// フォローされた通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Followed;
		/// <summary>
		/// アンフォローした通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Unfollow;
		/// <summary>
		/// ブロックした通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Block;
		/// <summary>
		/// アンブロックした通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Unblock;
		/// <summary>
		/// お気に入りした通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Favorite;
		/// <summary>
		/// お気に入りされた通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Favorited;
		/// <summary>
		/// お気に入り解除した通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Unfavorite;
		/// <summary>
		/// お気に入り解除された通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Unfavorited;
		/// <summary>
		/// Retweet した通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Retweet;
		/// <summary>
		/// Retweet された通知を受信したときに発生します。
		/// </summary>
		public event EventHandler<TwitterStreamEventArgs> Retweeted;

		/// <summary>
		/// キャッシュを指定し TwitterStream の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="statusCache">キャッシュ。</param>
		public TwitterStream(StatusCache statusCache)
		{
			this.StatusCache = statusCache;
			this.Track = new NotifyCollection<string>()
				.Apply(_ => _.CollectionChanged += (sender, e) => ChangedAndReconnect());
			this.Follows = new NotifyCollection<UserID>()
				.Apply(_ => _.CollectionChanged += (sender, e) => ChangedAndReconnect());
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
		/// アカウントを取得します。
		/// </summary>
		public AccountToken Account
		{
			get;
			private set;
		}

		/// <summary>
		/// すでに接続中であるかどうかを取得します。
		/// </summary>
		public bool IsStreamOpen
		{
			get
			{
				return streamingThread != null;
			}
		}

		/// <summary>
		/// 切断済みであるかどうかを取得します。
		/// </summary>
		public bool Terminated
		{
			get;
			private set;
		}

		/// <summary>
		/// track パラメータを取得または設定します。
		/// </summary>
		public NotifyCollection<string> Track
		{
			get;
			private set;
		}

		/// <summary>
		/// follow パラメータを取得または設定します。
		/// </summary>
		public NotifyCollection<UserID> Follows
		{
			get;
			private set;
		}

		void ChangedAndReconnect()
		{
			if (reconnectTimer == null)
				reconnectTimer = new Timer(_ =>
				{
					Disconnect();
					Connect(new OAuthAuthorization(this.Account));
				}, null, 1000, Timeout.Infinite);
			else
				reconnectTimer.Change(1000, Timeout.Infinite);
		}

		void ThreadMain(object state)
		{
			var auth = (OAuthAuthorization)state;

			this.Account = auth.Token;
			this.Terminated = false;

			try
			{
				var ub = new UriBuilder(TwitterUriBuilder.Stream.User(this.Track, this.Follows));
				var query = string.IsNullOrEmpty(ub.Query) ? null : ub.Query.TrimStart('?');

				ub.Query = null;

				using (var wc = new CustomWebClient
				{
					Headers =
					{
						{ HttpRequestHeader.UserAgent, "Solar/" + Assembly.GetEntryAssembly().GetName().Version },
					},
				})
				using (var ns = wc.OpenPost(ub.Uri, (string.IsNullOrEmpty(query) ? null : query + "&") + auth.CreateParameters("POST", ub.Uri, query)))
				using (var sr = new StreamReader(ns))
				{
					Connected.RaiseEvent(this, EventArgs.Empty);

					try
					{
						foreach (var i in sr.EnumerateLines()
											.Where(_ => !string.IsNullOrEmpty(_))
											.Select(DynamicJson.Parse))
						{
							if (IsDelete(i))
							{
								// delete
								if (i.delete.status())
									DeleteStatus.RaiseEvent(this, new EventArgs<StatusID>(i.delete.status.id));
							}
							else if (IsFriends(i))
							{
								// friends
								Friends.RaiseEvent(this, new EventArgs<IList<UserID>>(((dynamic[])i.friends).Select(_ => (UserID)_).Freeze()));
							}
							else if (IsEvent(i))
							{
								// event
								using (var client = new TwitterClient(auth.Token, StatusCache))
								{
									var e = new TwitterStreamEventArgs(client, i);

									switch (e.Type)
									{
										case "follow":
											if (e.Source.UserID == this.Account.UserID)
												Follow.RaiseEvent(this, e);
											else
												Followed.RaiseEvent(this, e);

											break;
										case "unfollow":
											Unfollow.RaiseEvent(this, e);

											break;
										case "block":
											Block.RaiseEvent(this, e);

											break;
										case "unblock":
											Unblock.RaiseEvent(this, e);

											break;
										case "favorite":
											if (e.Source.UserID == this.Account.UserID)
												Favorite.RaiseEvent(this, e);
											else
												Favorited.RaiseEvent(this, e);

											break;
										case "unfavorite":
											if (e.Source.UserID == this.Account.UserID)
												Unfavorite.RaiseEvent(this, e);
											else
												Unfavorited.RaiseEvent(this, e);

											break;
										case "retweet":
											if (e.Source.UserID == this.Account.UserID)
												Retweet.RaiseEvent(this, e);
											else
												Retweeted.RaiseEvent(this, e);

											break;
									}
								}
							}
							else if (IsDirectMessage(i))
							{
								// direct message
								using (var client = new TwitterClient(auth.Token, StatusCache))
									DirectMessage.RaiseEvent(this, new EventArgs<DirectMessage>(new DirectMessage(client, i.direct_message)));
							}
							else if (IsStatus(i))
							{
								// status
								using (var client = new TwitterClient(auth.Token, StatusCache))
									Status.RaiseEvent(this, new EventArgs<Status>(new Status(client, i)));
							}
						}
					}
					finally
					{
						wc.LastRequest.Abort();
					}
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (WebException ex)
			{
				WebError.RaiseEvent(this, new EventArgs<WebException>(ContentedWebException.Create(ex)));
			}
			catch (IOException ex)
			{
				ConnectionError.RaiseEvent(this, new EventArgs<IOException>(ex));
			}
			catch (Exception ex)
			{
				Exception.RaiseEvent(this, new EventArgs<Exception>(ex));
			}
			finally
			{
				streamingThread = null;
				Disconnected.RaiseEvent(this, EventArgs.Empty);
				this.Terminated = true;
			}
		}

		bool IsDelete(dynamic json)
		{
			return json.delete();
		}

		bool IsFriends(dynamic json)
		{
			return json.friends();
		}

		bool IsEvent(dynamic json)
		{
			return json.@event();
		}

		bool IsDirectMessage(dynamic json)
		{
			return json.direct_message();
		}

		bool IsStatus(dynamic json)
		{
			return json.retweeted();
		}

		/// <summary>
		/// 指定された認証を使用して非同期に User Streams に接続を開始します。
		/// </summary>
		/// <param name="auth">OAuth 認証。</param>
		/// <returns>接続が開始されたかどうか。</returns>
		public bool Connect(OAuthAuthorization auth)
		{
			if (streamingThread != null)
				return false;

			streamingThread = new Thread(ThreadMain)
			{
				IsBackground = true,
			};
			streamingThread.Start(auth);

			return true;
		}

		void Disconnect()
		{
			if (!this.IsStreamOpen)
				return;

			if (streamingThread != null)
			{
				streamingThread.Abort();
				streamingThread = null;
			}

			if (reconnectTimer != null)
			{
				reconnectTimer.Dispose();
				reconnectTimer = null;
			}

			this.Terminated = true;
		}

		/// <summary>
		/// すでに接続されていた場合、切断を開始します。
		/// </summary>
		public void Dispose()
		{
			Disconnect();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// dtor
		/// </summary>
		~TwitterStream()
		{
			Dispose();
		}

		class CustomWebClient : WebClient
		{
			public WebRequest LastRequest
			{
				get;
				private set;
			}

			public Stream OpenPost(Uri address, string data)
			{
				var req = GetWebRequest(address);

				req.Method = "POST";

				using (var sw = new StreamWriter(req.GetRequestStream()))
					sw.Write(data);

				return req.GetResponse().GetResponseStream();
			}

			protected override WebRequest GetWebRequest(Uri address)
			{
				return this.LastRequest = base.GetWebRequest(address).Apply
				(
					_ => _.Timeout = 2000
				);
			}
		}
	}
}
