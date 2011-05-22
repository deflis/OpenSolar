using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Lunar;

namespace Solar.Filtering
{
	class QueryCacheFilterSource : FilterSource
	{
		public override bool Pagable
		{
			get
			{
				return false;
			}
		}

		public override bool Serializable
		{
			get
			{
				return false;
			}
		}

		public string Query
		{
			get;
			set;
		}

		protected override IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range)
		{
			if (string.IsNullOrEmpty(this.Query))
				return Enumerable.Empty<IEntry>();
			else
				return client.StatusCache.GetStatuses()
										 .AsQueryable()
										 .Where(this.Query);
		}

		protected override bool StreamEntryMatches(IEntry entry)
		{
			return false;
		}
	}
}
