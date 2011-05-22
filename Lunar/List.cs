using System;
using System.Collections.Generic;
using Codeplex.Data;

namespace Lunar
{
	/// <summary>
	/// Twitter リストへのアクセスを提供します。
	/// </summary>
	public class List : ServiceObject, IEntry
	{
		/// <summary>
		/// Twitter クライアントと基になる JSON オブジェクトを指定し List の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">Twitter クライアント。</param>
		/// <param name="json">基になる JSON オブジェクト。</param>
		public List(TwitterClient client, DynamicJson json)
			: base(json)
		{
			this.Account = client.Account;
			this.User = this.json.user() ? new User(client, this.json.user) : null;
		}

		/// <summary>
		/// このリストを削除します。
		/// </summary>
		/// <returns>削除されたリスト。</returns>
		public List Destroy()
		{
			return TwitterClient.CurrentInstance.Lists.Destroy(this.ListID);
		}

		/// <summary>
		/// このリストをフォローします。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>フォローしたリスト。</returns>
		public List Subscribe()
		{
			return TwitterClient.CurrentInstance.Lists.Subscribe(this.User.UserID, this.ListID);
		}

		/// <summary>
		/// このリストをアンフォローします。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>アンフォローしたリスト。</returns>
		public List Unsubscribe()
		{
			return TwitterClient.CurrentInstance.Lists.Unsubscribe(this.User.UserID, this.ListID);
		}

		/// <summary>
		/// このリストがフォローしているメンバーを取得します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>メンバー一覧。</returns>
		public IEnumerable<User> Members()
		{
			return TwitterClient.CurrentInstance.Lists.Members(this.User.UserID, this.ListID);
		}

		/// <summary>
		/// 指定したフォローメンバーをこのリストに追加します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <param name="id">メンバーのユーザ ID。</param>
		/// <returns>フォローメンバーが追加されたリスト。</returns>
		public List AddMember(UserID id)
		{
			return AddMember(id.ToString());
		}

		/// <summary>
		/// 指定したフォローメンバーをこのリストに追加します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <param name="userName">メンバーのユーザ名。</param>
		/// <returns>フォローメンバーが追加されたリスト。</returns>
		public List AddMember(string userName)
		{
			if (!this.IsOwned)
				throw new InvalidOperationException("list must be yours");

			return TwitterClient.CurrentInstance.Lists.AddMember(this.ListID, userName);
		}

		/// <summary>
		/// 指定したフォローメンバーをこのリストから削除します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <param name="id">メンバーのユーザ ID。</param>
		/// <returns>フォローメンバーが削除されたリスト。</returns>
		public List RemoveMember(UserID id)
		{
			return RemoveMember(id.ToString());
		}

		/// <summary>
		/// 指定したフォローメンバーをこのリストから削除します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <param name="userName">メンバーのユーザ名。</param>
		/// <returns>フォローメンバーが削除されたリスト。</returns>
		public List RemoveMember(string userName)
		{
			if (!this.IsOwned)
				throw new InvalidOperationException("list must be yours");

			return TwitterClient.CurrentInstance.Lists.RemoveMember(this.ListID, userName);
		}

		/// <summary>
		/// このリストの情報を変更します。省略されたパラメータは変更されません。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <param name="newListName">新しいリスト名。</param>
		/// <param name="mode">新しい公開種別。</param>
		/// <param name="description">新しい解説文。</param>
		/// <returns>変更されたリスト。</returns>
		public List Update(string newListName = null, ListMode mode = ListMode.None, string description = null)
		{
			if (!this.IsOwned)
				throw new InvalidOperationException("list must be yours");

			return TwitterClient.CurrentInstance.Lists.Update(this.ListID, newListName, mode, description);
		}

		/// <summary>
		/// このリストをフォローしているユーザを取得します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>このリストをフォローしているユーザ。</returns>
		public IEnumerable<User> Subscribers()
		{
			return TwitterClient.CurrentInstance.Lists.Subscribers(this.User.UserID, this.ListID);
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
		/// このリストが取得したアカウントにより作成されたものであるかを取得します。
		/// </summary>
		public bool IsOwned
		{
			get
			{
				if (this.Account != null)
					return this.User.UserID == this.Account.UserID;
				else
					return false;
			}
		}


		/// <summary>
		/// このリストが取得したアカウントにより作成されたものでないかを取得します。
		/// </summary>
		public bool IsNotOwned
		{
			get
			{
				return !this.IsOwned;
			}
		}

		/// <summary>
		/// ユーザを取得します。
		/// </summary>
		public User User
		{
			get;
			private set;
		}

		/// <summary>
		/// リストの ID を取得します。
		/// </summary>
		public ListID ListID
		{
			get
			{
				return long.Parse(json.id_str);
			}
		}

		/// <summary>
		/// リスト名を取得します。
		/// </summary>
		public string Name
		{
			get
			{
				return json.slug;
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
		/// リストのフォロワー数を取得します。
		/// </summary>
		public int SubscriberCount
		{
			get
			{
				return (int)json.subscriber_count;
			}
		}

		/// <summary>
		/// リストのフォロー数を取得します。
		/// </summary>
		public int MemberCount
		{
			get
			{
				return (int)json.member_count;
			}
		}

		/// <summary>
		/// URL を取得します。
		/// </summary>
		public Uri Uri
		{
			get
			{
				return new Uri("http://twitter.com" + json.uri);
			}
		}

		/// <summary>
		/// リストの公開種別を取得します。
		/// </summary>
		public ListMode Mode
		{
			get
			{
				return json.mode == "private" ? ListMode.Private : ListMode.Public;
			}
		}

		string IEntry.UserName
		{
			get
			{
				return this.User.Name;
			}
		}

		string IEntry.Text
		{
			get
			{
				return this.Description;
			}
		}

		DateTime IEntry.CreatedAt
		{
			get
			{
				return DateTime.MinValue;
			}
		}

		long IEntry.ID
		{
			get
			{
				return this.ListID;
			}
		}

		/// <summary>
		/// 指定した IEntry が現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="other">現在の IEntry と比較する IEntry。</param>
		/// <returns>等しいかどうか。</returns>
		public bool Equals(IEntry other)
		{
			return other != null
				&& other.ID == this.ListID;
		}
	}
}
