using System;
using System.Collections.Generic;
using Ignition;
using Lunar;

namespace Solar.Filtering
{
	class ReplyChainFilterSource : FilterSource, IEquatable<ReplyChainFilterSource>
	{
		public StatusID RootStatus
		{
			get;
			set;
		}

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

		protected override IEnumerable<IEntry> GetStatuses(TwitterClient client, StatusRange range)
		{
			var status = client.Statuses.Get(this.RootStatus);

			yield return status;

			while (status.InReplyToStatusID != 0)
			{
				var user = client.StatusCache.RetrieveUser(status.InReplyToUserID, _ => null);

				if (user == null || user.Protected)
				{
					try
					{
						status = client.Statuses.Get(status.InReplyToStatusID);
					}
					catch
					{
						yield break;
					}

					yield return status;
				}
				else
				{
					using (new ReduceAuthenticatedQueryScope())
						try
						{
							status = client.Statuses.Get(status.InReplyToStatusID);
						}
						catch
						{
							yield break;
						}

					yield return status;
				}
			}
		}

		public bool Equals(ReplyChainFilterSource other)
		{
			return this.MemberwiseEquals(other);
		}

		public override bool Equals(object obj)
		{
			return obj is ReplyChainFilterSource ? Equals((ReplyChainFilterSource)obj) : base.Equals(obj);
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
