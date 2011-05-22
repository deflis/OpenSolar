using System;
using System.Collections.Generic;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// ユーザ フィルタソースを表します。
	/// </summary>
	public class UserFilterSource : FilterSource, IEquatable<UserFilterSource>
	{
		/// <summary>
		/// 対象のユーザ名を取得または設定します。
		/// </summary>
		public string UserName
		{
			get;
			set;
		}

		/// <summary>
		/// 対象のユーザ ID を取得または設定します。
		/// </summary>
		public UserID UserID
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
			if (this.UserID == 0 || this.UserName == null)
			{
				var user = client.Users.Get(this.UserID == 0 ? this.UserName : this.UserID.ToString());

				this.UserID = user.UserID;
				this.UserName = user.Name;
			}

			return client.Statuses.UserTimeline(this.UserID, range).ReduceAuthenticatedQuery();
		}

		/// <summary>
		/// 指定した UserFilterSource が現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="other">現在の UserFilterSource と比較する UserFilterSource。</param>
		/// <returns>等しいかどうか。</returns>
		public bool Equals(UserFilterSource other)
		{
			return this.MemberwiseEquals(other);
		}

		/// <summary>
		/// 現在の UserFilterSource のハッシュコードを取得します。
		/// </summary>
		/// <returns>現在の UserFilterSource のハッシュコード。</returns>
		public override int GetHashCode()
		{
			return Equatable.GetMemberwiseHashCode(this);
		}

		/// <summary>
		/// 指定したオブジェクトが現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="obj">現在の UserFilterSource と比較するオブジェクト。</param>
		/// <returns>等しいかどうか。</returns>
		public override bool Equals(object obj)
		{
			return obj is UserFilterSource ? Equals((UserFilterSource)obj) : base.Equals(obj);
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
				(Status _) => !_.IsDirectMessage && (_.UserName == this.UserName || _.UserID == this.UserID),
				_ => false
			);
		}
	}
}
