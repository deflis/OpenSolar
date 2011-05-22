using System;
using System.Collections.Generic;
using System.Linq;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// 検索 フィルタソースを表します。
	/// </summary>
	public class SearchFilterSource : FilterSource, IEquatable<SearchFilterSource>
	{
		/// <summary>
		/// 検索クエリを取得または設定します。
		/// </summary>
		public string Query
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
			if (string.IsNullOrEmpty(this.Query))
				return Enumerable.Empty<IEntry>();
			else
				return client.Statuses.Search(this.Query, range);
		}

		/// <summary>
		/// 指定した SearchFilterSource が現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="other">現在の SearchFilterSource と比較する FilterSource。</param>
		/// <returns>等しいかどうか。</returns>
		public bool Equals(SearchFilterSource other)
		{
			return this.MemberwiseEquals(other);
		}

		/// <summary>
		/// 現在の SearchFilterSource のハッシュコードを取得します。
		/// </summary>
		/// <returns>現在の SearchFilterSource のハッシュコード。</returns>
		public override int GetHashCode()
		{
			return Equatable.GetMemberwiseHashCode(this);
		}

		/// <summary>
		/// 指定したオブジェクトが現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="obj">現在の SearchFilterSource と比較するオブジェクト。</param>
		/// <returns>等しいかどうか。</returns>
		public override bool Equals(object obj)
		{
			return obj is SearchFilterSource ? Equals((SearchFilterSource)obj) : base.Equals(obj);
		}

		/// <summary>
		/// 指定したエントリがこのソースから取得できるエントリとして扱うかどうかを取得します。
		/// </summary>
		/// <param name="entry">判定するエントリ。</param>
		/// <returns>このソースから取得できるエントリとして扱うかどうか。</returns>
		protected override bool StreamEntryMatches(IEntry entry)
		{
			return false;
		}
	}
}
