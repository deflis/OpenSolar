using System.Collections.Generic;
using System.Linq;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	class SearchCacheFilterSource : FilterSource
	{
		/// <summary>
		/// 保存可能かどうかを取得します。
		/// </summary>
		public override bool Serializable
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// ページに分けて取得することが可能かどうかを取得します。
		/// </summary>
		public override bool Pagable
		{
			get
			{
				return false;
			}
		}

		internal string Query
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
			if (this.Query == null)
				return Enumerable.Empty<IEntry>();

			var words = this.Query.Split('　', ' ');

			if (words.Any())
				return client.StatusCache.GetStatuses().Where(_ => words.All(_.UserName.Contains) || words.All(_.Text.Contains));
			else
				return Enumerable.Empty<IEntry>();
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
				(Status _) =>
				{
					if (this.Query == null)
						return false;

					var words = this.Query.Split('　', ' ');

					return words.Any()
						&& (words.All(_.UserName.Contains) || words.All(_.Text.Contains));
				},
				_ => false
			);
		}
	}
}
