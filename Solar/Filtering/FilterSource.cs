using System;
using System.Collections.Generic;
using System.Linq;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// フィルタソースを表します。
	/// </summary>
	public abstract class FilterSource : IEquatable<FilterSource>
	{
		/// <summary>
		/// 使用するアカウントを取得または設定します。0 の場合、すべてのアカウントを使用します。
		/// </summary>
		public UserID Account
		{
			get;
			set;
		}

		bool ShouldSerializeAccount()
		{
			return this.Account != 0;
		}

		/// <summary>
		/// 保存可能かどうかを取得します。
		/// </summary>
		public virtual bool Serializable
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// ページに分けて取得することが可能かどうかを取得します。
		/// </summary>
		public virtual bool Pagable
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// クライアントと取得範囲を指定しソースからエントリを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <returns>取得したエントリ。</returns>
		public IEnumerable<IEntry> GetStatusesFromSource(TwitterClient client, StatusRange range)
		{
			if (this.Account != 0 &&
				this.Account != client.Account.UserID)
				return Enumerable.Empty<IEntry>();
			else
				return GetStatuses(client, range);
		}

		/// <summary>
		/// クライアントと取得範囲を指定しソースからエントリを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <returns>取得したエントリ。</returns>
		protected abstract IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range);

		/// <summary>
		/// 指定したエントリがこのソースから取得できるエントリとして扱うかどうかを取得します。
		/// </summary>
		/// <param name="entry">判定するエントリ。</param>
		/// <returns>このソースから取得できるエントリとして扱うかどうか。</returns>
		public bool DoesStreamEntryMatches(IEntry entry)
		{
			return StreamEntryMatches(entry);
		}

		/// <summary>
		/// 指定したエントリがこのソースから取得できるエントリとして扱うかどうかを取得します。
		/// </summary>
		/// <param name="entry">判定するエントリ。</param>
		/// <returns>このソースから取得できるエントリとして扱うかどうか。</returns>
		protected abstract bool StreamEntryMatches(IEntry entry);

		/// <summary>
		/// 指定した FilterSource が現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="other">現在の FilterSource と比較する FilterSource。</param>
		/// <returns>等しいかどうか。</returns>
		public bool Equals(FilterSource other)
		{
			return this.MemberwiseEquals(other);
		}

		/// <summary>
		/// 指定したオブジェクトが現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="obj">現在の FilterSource と比較するオブジェクト。</param>
		/// <returns>等しいかどうか。</returns>
		public override bool Equals(object obj)
		{
			return obj is FilterSource ? Equals((FilterSource)obj) : base.Equals(obj);
		}

		/// <summary>
		/// 現在の FilterSource のハッシュコードを取得します。
		/// </summary>
		/// <returns>現在の FilterSource のハッシュコード。</returns>
		public override int GetHashCode()
		{
			return Equatable.GetMemberwiseHashCode(this);
		}
	}
}
