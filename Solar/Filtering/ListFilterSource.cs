using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// リスト フィルタソースを表します。
	/// </summary>
	public class ListFilterSource : FilterSource, IEquatable<ListFilterSource>
	{
		readonly HashSet<UserID> following = new HashSet<UserID>();

		/// <summary>
		/// リスト パスを取得または設定します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Path
		{
			get
			{
				return this.UserName == null
					? this.UserID == 0
						? this.ListName
						: this.UserID + "/" + this.ListName
					: "@" + this.UserName + "/" + this.ListName;
			}
			set
			{
				if (value.Contains("/"))
				{
					var sl = value.Split('/');
					long rt = 0;

					if (value.StartsWith("@"))
						this.UserName = sl.First().Substring(1);
					else if (long.TryParse(sl.First(), out rt))
						this.UserID = rt;
					else
						this.UserName = sl.First();

					this.UserID = rt;
					this.ListID = default(ListID);
					this.ListName = sl.Last();
				}
				else
				{
					this.UserID = default(UserID);
					this.UserName = null;
					this.ListID = default(ListID);
					this.ListName = value;
				}
			}
		}

		/// <summary>
		/// ユーザ名を取得または設定します。
		/// </summary>
		[DefaultValue(null)]
		public string UserName
		{
			get;
			set;
		}

		/// <summary>
		/// ユーザ ID を取得または設定します。
		/// </summary>
		[DefaultValue(0)]
		public UserID UserID
		{
			get;
			set;
		}

		/// <summary>
		/// リスト名を取得または設定します。
		/// </summary>
		[DefaultValue(null)]
		public string ListName
		{
			get;
			set;
		}

		/// <summary>
		/// リスト ID を取得または設定します。
		/// </summary>
		[DefaultValue(0)]
		public ListID ListID
		{
			get;
			set;
		}

		/// <summary>
		/// クライアントと取得範囲を指定しソースからエントリを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <returns>取得したエントリ。</returns>
		protected override IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range)
		{
			if (this.UserName != null || this.UserID != 0)
				if (this.UserName != null &&
					this.UserID == 0 ||
					this.ListName != null &&
					this.ListID == 0)
				{
					List list;

					if (this.UserID == 0)
						if (this.ListID == 0)
							list = client.Lists.Get(this.UserName, this.ListName);
						else
							list = client.Lists.Get(this.UserName, this.ListID);
					else
						if (this.ListID == 0)
							list = client.Lists.Get(this.UserID, this.ListName);
						else
							list = client.Lists.Get(this.UserID, this.ListID);

					this.UserName = list.User.Name;
					this.UserID = list.User.UserID;
					this.ListName = list.Name;
					this.ListID = list.ListID;
				}


			if (this.UserID == 0)
				if (string.IsNullOrEmpty(this.UserName))
					if (this.ListID == 0)
						return client.Lists.Statuses(client.Account.Name, this.ListName, range);
					else
						return client.Lists.Statuses(client.Account.Name, this.ListID, range);
				else
					if (this.ListID == 0)
						return client.Lists.Statuses(this.UserName, this.ListName, range);
					else
						return client.Lists.Statuses(this.UserName, this.ListID, range);
			else
				if (this.ListID == 0)
					return client.Lists.Statuses(this.UserID, this.ListName, range);
				else
					return client.Lists.Statuses(this.UserID, this.ListID, range);
		}

		/// <summary>
		/// 指定した ListFilterSource が現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="other">現在の ListFilterSource と比較する FilterSource。</param>
		/// <returns>等しいかどうか。</returns>
		public bool Equals(ListFilterSource other)
		{
			return this.MemberwiseEquals(other);
		}

		/// <summary>
		/// 現在の ListFilterSource のハッシュコードを取得します。
		/// </summary>
		/// <returns>現在の ListFilterSource のハッシュコード。</returns>
		public override int GetHashCode()
		{
			return Equatable.GetMemberwiseHashCode(this);
		}

		/// <summary>
		/// 指定したオブジェクトが現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="obj">現在の ListFilterSource と比較するオブジェクト。</param>
		/// <returns>等しいかどうか。</returns>
		public override bool Equals(object obj)
		{
			return obj is ListFilterSource ? Equals((ListFilterSource)obj) : base.Equals(obj);
		}

		/// <summary>
		/// 指定したエントリがこのソースから取得できるエントリとして扱うかどうかを取得します。
		/// </summary>
		/// <param name="entry">判定するエントリ。</param>
		/// <returns>このソースから取得できるエントリとして扱うかどうか。</returns>
		protected override bool StreamEntryMatches(IEntry entry)
		{
			return entry.TypeMatch
			(
				(Status _) => !_.IsDirectMessage && following.Contains(_.UserID),
				_ => false
			);
		}
	}
}
