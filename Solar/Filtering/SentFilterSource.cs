using System.Collections.Generic;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	/// <summary>
	/// 送信済み フィルタソースを表します。
	/// </summary>
	public class SentFilterSource : FilterSource
	{
		/// <summary>
		/// クライアントと取得範囲を指定しソースからエントリを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <returns>取得したエントリ。</returns>
		protected override IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range)
		{
			return client.Statuses.UserTimeline(client.Account.UserID, range);
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
				(Status _) => !_.IsDirectMessage && _.UserName == _.Account.Name,
				_ => false
			);
		}
	}
}
