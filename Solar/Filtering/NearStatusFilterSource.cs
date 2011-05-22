using System.Collections.Generic;
using System.Linq;
using Lunar;

namespace Solar.Filtering
{
	class NearStatusFilterSource : FilterSource
	{
		internal StatusID RootStatus
		{
			get;
			set;
		}

		internal int NearCount
		{
			get;
			set;
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

		internal NearStatusFilterSource()
		{
			this.NearCount = 5;
		}

		/// <summary>
		/// クライアントと取得範囲を指定しソースからエントリを取得します。
		/// </summary>
		/// <param name="client">クライアント。</param>
		/// <param name="range">取得範囲。</param>
		/// <returns>取得したエントリ。</returns>
		protected override IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range)
		{
			var root = client.Statuses.Get(this.RootStatus);

			using (new ReduceAuthenticatedQueryScope())
				return client.Statuses.UserTimeline(root.UserName, new StatusRange(sinceID: root.StatusID, count: this.NearCount))
					.Concat(client.Statuses.UserTimeline(root.UserName, new StatusRange(maxID: root.StatusID, count: this.NearCount + 1)));
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
