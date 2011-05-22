using System.Collections.Generic;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// お気に入り フィルタソースを表します。
	/// </summary>
	public class FavoritesFilterSource : FilterSource
	{
		/// <summary>
		/// 取得先のユーザ名を取得または設定します。null の場合、ログインユーザになります。
		/// </summary>
		public string UserName
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
			return client.Favorites.Index(this.UserName ?? client.Account.Name, range.Page);
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
				(Status _) => _.Favorited,
				_ => false
			);
		}
	}
}
