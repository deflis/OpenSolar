using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Codeplex.Data;
using Ignition;

namespace Lunar
{
	/// <summary>
	/// つぶやきを表します。
	/// </summary>
	public class Status : ServiceObject, IEquatable<Status>, IEntry, IComparable<Status>
	{
		/// <summary>
		/// Twitter クライアントと基になる JSON オブジェクトを指定し Status の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">Twitter クライアント。</param>
		/// <param name="json">基になる JSON オブジェクト。</param>
		public Status(TwitterClient client, DynamicJson json)
			: base(json)
		{
			this.Account = client.Account;
			this.User = this.json.user() ? new User(client, this.json.user, this) : null;
			this.RetweetedStatus = this.json.retweeted_status() ? new Status(client, this.json.retweeted_status) : null;
		}

		/// <summary>
		/// Twitter クライアント、基になる JSON オブジェクトおよび関連するユーザを指定し Status の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">Twitter クライアント。</param>
		/// <param name="json">基になる JSON オブジェクト。</param>
		/// <param name="user">関連するユーザ。</param>
		public Status(TwitterClient client, DynamicJson json, User user)
			: this(client, json)
		{
			this.User = user;

			if (this.RetweetedStatus != null &&
				this.RetweetedStatus.User == null)
				this.RetweetedStatus = null;
		}

		/// <summary>
		/// Twitter クライアントおよび関連するユーザを指定し Status の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">Twitter クライアント。</param>
		/// <param name="user">関連するユーザ。</param>
		public Status(TwitterClient client, User user)
			: this(client, new DynamicJson()
				.Apply<DynamicJson>
				(
					_ => ((dynamic)_).created_at = user.CreatedAt.ToString("ddd MMM dd HH:mm:ss zz00 yyyy", CultureInfo.InvariantCulture),
					_ => ((dynamic)_).text = null,
					_ => ((dynamic)_).id_str = "-1"
				), user)
		{
		}

		/// <summary>
		/// このつぶやきを Retweet します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>Retweet した結果のつぶやき。</returns>
		public Status Retweet()
		{
			if (this.IsDirectMessage)
				throw new InvalidOperationException("direct messages cannot be retweeted");
			else if (this.IsOwned)
				throw new InvalidOperationException("statuses cannot be retweeted by yourself");

			return TwitterClient.CurrentInstance.Statuses.Retweet(this.StatusID);
		}

		/// <summary>
		/// このつぶやきを削除します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>削除したつぶやき。</returns>
		public virtual Status Destroy()
		{
			if (!this.IsOwned)
				throw new InvalidOperationException("status must be yours");

			return TwitterClient.CurrentInstance.Statuses.Destroy(this.StatusID);
		}

		/// <summary>
		/// このつぶやきをお気に入りします。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>お気に入りしたつぶやき。</returns>
		public Status Favorite()
		{
			if (this.IsDirectMessage)
				throw new InvalidOperationException("direct messages cannot be favorited");
			else if (this.Favorited)
				throw new InvalidOperationException("status already favorited");

			this.Favorited = true;

			return TwitterClient.CurrentInstance.Favorites.Create(this.StatusID);
		}

		/// <summary>
		/// このつぶやきをお気に入りから外します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>お気に入りから外したつぶやき。</returns>
		public Status Unfavorite()
		{
			if (this.IsDirectMessage)
				throw new InvalidOperationException("direct messages cannot be unfavorited");
			else if (!this.Favorited)
				throw new InvalidOperationException("status not favorited");

			this.Favorited = false;

			return TwitterClient.CurrentInstance.Favorites.Destroy(this.StatusID);
		}

		/// <summary>
		/// 取得したアカウントを取得します。
		/// </summary>
		public AccountToken Account
		{
			get;
			private set;
		}

		/// <summary>
		/// ダイレクトメッセージであるかどうかを取得します。
		/// </summary>
		public virtual bool IsDirectMessage
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// 検索結果であるかどうかを取得します。
		/// </summary>
		public virtual bool IsSearchResult
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Retweet であるかどうかを取得します。
		/// </summary>
		public bool IsRetweet
		{
			get
			{
				return this.RetweetedStatus != null;
			}
		}

		/// <summary>
		/// Retweet でないかどうかを取得します。
		/// </summary>
		public bool IsNotRetweet
		{
			get
			{
				return !this.IsRetweet;
			}
		}

		/// <summary>
		/// Retweet でなくて、かつダイレクトメッセージでもないかどうかを取得します。
		/// </summary>
		public bool IsNotRetweetAndDirectMessage
		{
			get
			{
				return this.IsNotRetweet && !this.IsDirectMessage;
			}
		}

		/// <summary>
		/// このつぶやきが取得したアカウントからのつぶやきであるかどうかを取得します。
		/// </summary>
		public bool IsOwned
		{
			get
			{
				if (!this.IsSearchResult && this.Account != null)
					return this.User.UserID == this.Account.UserID;
				else
					return false;
			}
		}

		/// <summary>
		/// このつぶやきが取得したアカウントからのつぶやきでないかどうかを取得します。
		/// </summary>
		public bool IsNotOwned
		{
			get
			{
				return !this.IsOwned;
			}
		}

		/// <summary>
		/// 本文を取得します。
		/// </summary>
		public virtual string Text
		{
			get
			{
				return json.text() ? json.text : null;
			}
		}

		/// <summary>
		/// ID を取得します。
		/// </summary>
		public virtual StatusID StatusID
		{
			get
			{
				return json.id() ? long.Parse(json.id_str) : default(StatusID);
			}
		}

		/// <summary>
		/// truncated
		/// </summary>
		public virtual bool Truncated
		{
			get
			{
				return json.truncated() ? json.truncated : false;
			}
		}

		/// <summary>
		/// 返信先の ID を取得します。
		/// </summary>
		public virtual StatusID InReplyToStatusID
		{
			get
			{
				return json.in_reply_to_status_id_str() ? long.Parse(json.in_reply_to_status_id_str ?? "0") : 0;
			}
		}

		/// <summary>
		/// 返信先のユーザ ID を取得します。
		/// </summary>
		public virtual UserID InReplyToUserID
		{
			get
			{
				return json.in_reply_to_user_id_str() ? long.Parse(json.in_reply_to_user_id_str ?? "0") : 0;
			}
		}

		/// <summary>
		/// 返信であるかどうかを取得します。
		/// </summary>
		public bool IsReply
		{
			get
			{
				return this.InReplyToStatusID != 0;
			}
		}

		/// <summary>
		/// 返信先のユーザ名を取得します。
		/// </summary>
		public virtual string InReplyToUserName
		{
			get
			{
				if (this.InReplyToUserID == 0)
					return null;
				else
					return this.Mentions.First();
			}
		}

		/// <summary>
		/// 言及しているユーザ名を取得します。
		/// </summary>
		public virtual IEnumerable<string> Mentions
		{
			get
			{
				return json.entities()
					? ((dynamic[])json.entities.user_mentions).Select(_ => (string)_.screen_name)
					: Regex.Matches(this.Text, "@([A-Za-z0-9_]+)", RegexOptions.Compiled).Cast<Match>().Select(_ => _.Groups[1].Value);
			}
		}

		/// <summary>
		/// このつぶやきのユーザを取得します。
		/// </summary>
		public virtual User User
		{
			get;
			private set;
		}

		/// <summary>
		/// アドレスを取得します。
		/// </summary>
		public string Uri
		{
			get
			{
				return "http://twitter.com/" + this.UserName + "/status/" + this.StatusID;
			}
		}

		/// <summary>
		/// protected であるかどうかを取得します。
		/// </summary>
		public virtual bool Protected
		{
			get
			{
				return this.User.Protected;
			}
		}

		/// <summary>
		/// ユーザ名を取得します。
		/// </summary>
		public virtual string UserName
		{
			get
			{
				return this.User == null ? null : this.User.Name;
			}
		}

		/// <summary>
		/// ユーザの名前を取得します。
		/// </summary>
		public virtual string FullUserName
		{
			get
			{
				return this.User == null ? null : this.User.FullName;
			}
		}

		/// <summary>
		/// ユーザ ID を取得します。
		/// </summary>
		public virtual UserID UserID
		{
			get
			{
				return this.User.UserID;
			}
		}

		/// <summary>
		/// ユーザアイコンを取得します。
		/// </summary>
		public virtual Uri ProfileImage
		{
			get
			{
				return this.User == null ? null : this.User.ProfileImage;
			}
		}

		/// <summary>
		/// 投稿ソースを取得します。
		/// </summary>
		public virtual string Source
		{
			get
			{
				return json.source() ? json.source : null;
			}
		}

		/// <summary>
		/// 投稿ソース名を取得します。
		/// </summary>
		public virtual string SourceName
		{
			get
			{
				return this.Source == null ? null : Regex.Replace(this.Source, @"<.*?>", string.Empty);
			}
		}

		/// <summary>
		/// 投稿ソースのアドレスを取得します。
		/// </summary>
		public virtual Uri SourceUri
		{
			get
			{
				if (this.Source == null)
					return null;

				var m = Regex.Match(this.Source, @"href=""(.*?)""");

				if (m.Success)
					return new Uri(m.Groups[1].Value);
				else
					return new Uri("http://twitter.com/");
			}
		}

		/// <summary>
		/// お気に入りしているかどうかを取得します。
		/// </summary>
		public virtual bool Favorited
		{
			get
			{
				return json.favorited() ? json.favorited : false;
			}
			set
			{
				OnPropertyChanged("Favorited", json.favorited, json.favorited = value);
			}
		}

		/// <summary>
		/// 投稿日時を取得します。
		/// </summary>
		public virtual DateTime CreatedAt
		{
			get
			{
				return json.created_at()
					? DateTime.ParseExact(json.created_at, new[] { "ddd MMM dd HH:mm:ss zz00 yyyy", "ddd, dd MMM yyyy HH:mm:ss zz00" }, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None).ToLocalTime()
					: DateTime.MinValue;
			}
		}

		/// <summary>
		/// contributors
		/// </summary>
		public IEnumerable<UserID> Contributors
		{
			get
			{
				return json.contributors() ? json.contributors : null;
			}
		}

		/// <summary>
		/// このつぶやきが Retweet の場合、Retweet 先を取得します。
		/// </summary>
		public Status RetweetedStatus
		{
			get;
			private set;
		}

		/// <summary>
		/// このつぶやきが Retweet の場合、Retweet 先を取得します。
		/// そうでなければ、このつぶやきを返します。
		/// </summary>
		public Status RetweetedStatusOrSelf
		{
			get
			{
				return this.RetweetedStatus ?? this;
			}
		}

		/// <summary>
		/// 受取人の名前を取得します。
		/// </summary>
		public virtual string RecipientName
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// 指定した Status が現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="other">現在の Status と比較する Status。</param>
		/// <returns>等しいかどうか。</returns>
		public bool Equals(Status other)
		{
			return other != null
				&& other.StatusID == this.StatusID;
		}

		/// <summary>
		/// 指定したオブジェクトが現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="obj">現在の Status と比較するオブジェクト。</param>
		/// <returns>等しいかどうか。</returns>
		public override bool Equals(object obj)
		{
			return obj is Status ? Equals((Status)obj) : base.Equals(obj);
		}

		/// <summary>
		/// 現在の Status のハッシュコードを取得します。
		/// </summary>
		/// <returns>現在の Status のハッシュコード。</returns>
		public override int GetHashCode()
		{
			return this.StatusID.GetHashCode();
		}

		long IEntry.ID
		{
			get
			{
				return this.StatusID;
			}
		}

		/// <summary>
		/// 指定した IEntry が現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="other">現在の Status と比較する IEntry。</param>
		/// <returns>等しいかどうか。</returns>
		public bool Equals(IEntry other)
		{
			return other != null
				&& other.ID == this.StatusID;
		}

		public int CompareTo(Status other)
		{
			return other == null ? 0 : other.StatusID.CompareTo(this.StatusID);
		}
	}
}
