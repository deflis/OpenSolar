using System;
using System.Collections.Generic;
using System.Globalization;
using Codeplex.Data;

namespace Lunar
{
	/// <summary>
	/// ユーザ情報を提供します。
	/// </summary>
	public class User : ServiceObject
	{
		/// <summary>
		/// Twitter クライアントと基になる JSON オブジェクトを指定し、User の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">Twitter クライアント。</param>
		/// <param name="json">基になる JSON オブジェクト。</param>
		public User(TwitterClient client, DynamicJson json)
			: base(json)
		{
			this.Account = client.Account;
			this.Status = this.json.status() ? new Status(client, this.json.status, this) : null;
		}

		/// <summary>
		/// Twitter クライアントと基になる JSON オブジェクトおよび関連したつぶやきを指定し、User の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">Twitter クライアント。</param>
		/// <param name="json">基になる JSON オブジェクト。</param>
		/// <param name="status">関連したつぶやき。</param>
		public User(TwitterClient client, DynamicJson json, Status status)
			: this(client, json)
		{
			this.Status = status;
		}

		/// <summary>
		/// このユーザをフォローします。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>フォローしたユーザ。</returns>
		public User Follow()
		{
			return TwitterClient.CurrentInstance.Friendships.Create(this.UserID);
		}

		/// <summary>
		/// このユーザをアンフォローします。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>アンフォローしたユーザ。</returns>
		public User Unfollow()
		{
			return TwitterClient.CurrentInstance.Friendships.Destroy(this.UserID);
		}

		/// <summary>
		/// このユーザがフォローしているリストを取得します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>このユーザがフォローしているリスト。</returns>
		public IEnumerable<List> Subscriptions()
		{
			return TwitterClient.CurrentInstance.Lists.Subscriptions(this.UserID);
		}

		/// <summary>
		/// このユーザをフォローしているリストを取得します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>このユーザをフォローしているリスト。</returns>
		public IEnumerable<List> Memberships()
		{
			return TwitterClient.CurrentInstance.Lists.Memberships(this.UserID);
		}

		/// <summary>
		/// このユーザにより作成されたリストを取得します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>このユーザにより作成されたリスト。</returns>
		public IEnumerable<List> Lists()
		{
			return TwitterClient.CurrentInstance.Lists.Index(this.UserID);
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
		/// 関連付けられたつぶやきを取得します。
		/// </summary>
		public Status Status
		{
			get;
			private set;
		}

		/// <summary>
		/// ユーザ ID を取得します。
		/// </summary>
		public UserID UserID
		{
			get
			{
				return long.Parse(json.id_str);
			}
		}

		/// <summary>
		/// 名前を取得します。
		/// </summary>
		public string FullName
		{
			get
			{
				return json.name;
			}
		}

		/// <summary>
		/// ユーザ名を取得します。
		/// </summary>
		public string Name
		{
			get
			{
				return json.screen_name;
			}
		}

		/// <summary>
		/// 場所を取得します。
		/// </summary>
		public string Location
		{
			get
			{
				return json.location;
			}
		}

		/// <summary>
		/// 解説を取得します。
		/// </summary>
		public string Description
		{
			get
			{
				return json.description;
			}
		}

		/// <summary>
		/// 画像を取得します。
		/// </summary>
		public Uri ProfileImage
		{
			get
			{
				return string.IsNullOrEmpty(json.profile_image_url) ? null : new Uri(json.profile_image_url);
			}
		}

		/// <summary>
		/// Web サイトを取得します。
		/// </summary>
		public Uri WebSite
		{
			get
			{
				return string.IsNullOrEmpty(json.url) || !Uri.IsWellFormedUriString(json.url, UriKind.Absolute) ? null : new Uri(json.url);
			}
		}

		/// <summary>
		/// protected であるかどうかを取得します。
		/// </summary>
		public bool Protected
		{
			get
			{
				return json.@protected() ? json.@protected : false;
			}
		}

		/// <summary>
		/// protected でないかどうかを取得します。
		/// </summary>
		public bool IsNotProtected
		{
			get
			{
				return !this.Protected;
			}
		}

		/// <summary>
		/// フォロワー数を取得します。
		/// </summary>
		public int FollowersCount
		{
			get
			{
				return (int)json.followers_count;
			}
		}

		/// <summary>
		/// フォロー数を取得します。
		/// </summary>
		public int FollowingCount
		{
			get
			{
				return (int)json.friends_count;
			}
		}

		/// <summary>
		/// お気に入り数を取得します。
		/// </summary>
		public int FavouritesCount
		{
			get
			{
				return (int)json.favourites_count;
			}
		}

		/// <summary>
		/// つぶやき数を取得します。
		/// </summary>
		public int StatusesCount
		{
			get
			{
				return (int)json.statuses_count;
			}
		}

		/// <summary>
		/// 作成日時を取得します。
		/// </summary>
		public DateTime CreatedAt
		{
			get
			{
				return DateTime.ParseExact(json.created_at, "ddd MMM dd HH:mm:ss zz00 yyyy", CultureInfo.InvariantCulture.DateTimeFormat).ToLocalTime();
			}
		}

		/// <summary>
		/// 認証済みであるかどうかを取得します。
		/// </summary>
		public bool Verified
		{
			get
			{
				return json.verified;
			}
		}
	}
}
