using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Lunar;
using System.Collections;

namespace Solar.Filtering
{
	public class EnumerableFilterSource : FilterSource
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

		public IEnumerable<IEntry> Sequence
		{
			get;
			set;
		}

		protected override IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range)
		{
			return this.Sequence;
		}

		protected override bool StreamEntryMatches(IEntry entry)
		{
			return false;
		}
	}
}
