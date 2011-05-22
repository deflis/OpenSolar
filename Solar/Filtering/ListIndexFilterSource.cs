using System.Collections.Generic;
using System.Linq;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	class ListIndexFilterSource : FilterSource
	{
		public override bool Serializable
		{
			get
			{
				return false;
			}
		}

		public override bool Pagable
		{
			get
			{
				return false;
			}
		}

		public string UserName
		{
			get;
			set;
		}

		public IndexMode Mode
		{
			get;
			set;
		}

		protected override IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range)
		{
			var name = this.UserName ?? client.Account.Name;
			IEnumerable<IEntry> rt;

			switch (this.Mode)
			{
				case IndexMode.Index:
					rt = client.Lists.Index(name);

					break;
				case IndexMode.Subscriptions:
					rt = client.Lists.Subscriptions(name);

					break;
				case IndexMode.Memberships:
					rt = client.Lists.Memberships(name);

					break;
				default:
					rt = Enumerable.Empty<IEntry>();

					break;
			}

			if (name != client.Account.Name)
				rt = rt.ReduceAuthenticatedQuery();

			return rt;
		}

		protected override bool StreamEntryMatches(IEntry entry)
		{
			return false;
		}

		public enum IndexMode
		{
			Index,
			Subscriptions,
			Memberships,
		}
	}
}
