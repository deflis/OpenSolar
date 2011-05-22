using System.Collections.Generic;
using Ignition;
using Lunar;
using Solar.Models;

namespace Solar.Filtering
{
	/// <summary>
	/// ホーム フィルタソースを表します。
	/// </summary>
	public class HomeFilterSource : FilterSource
	{
		/// <summary>
		/// クライアントと取得範囲を指定しソースからエントリを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <returns>取得したエントリ。</returns>
		protected override IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range)
		{
			return client.Statuses.HomeTimeline(range);
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
				(Status _) => !_.IsDirectMessage
						   && (_.UserID == _.Account.UserID
						   || Client.Instance.Friends.ContainsKey(_.Account)
						   && Client.Instance.Friends[_.Account].Contains(_.UserID)),
				_ => false
			);
		}
	}
}
