using System;
using System.Collections.Generic;
using System.Linq;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	class SearchUsersFilterSource : FilterSource, IEquatable<SearchUsersFilterSource>
	{
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
				return client.Users.Search(this.Query, range);
		}

		public bool Equals(SearchUsersFilterSource other)
		{
			return this.MemberwiseEquals(other);
		}

		public override bool Equals(object obj)
		{
			return obj is SearchUsersFilterSource ? Equals((SearchUsersFilterSource)obj) : base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Equatable.GetMemberwiseHashCode(this);
		}

		protected override bool StreamEntryMatches(IEntry entry)
		{
			return false;
		}
	}
}
