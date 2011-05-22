using System;
using System.Collections.Generic;
using System.Linq;
using Ignition;

namespace Lunar
{
	/// <summary>
	/// Twitter URI 構築を提供します。
	/// </summary>
	public static class TwitterUriBuilder
	{
		static readonly Uri baseUri = new Uri("http://api.twitter.com/1/");
		static readonly Uri oAuthBaseUri = new Uri("http://api.twitter.com/oauth/");
		static readonly Uri searchBaseUri = new Uri("http://search.twitter.com/");

		/// <summary>
		/// HTTPS を使用するかどうかを取得または設定します。
		/// </summary>
		public static bool UseHttps
		{
			get;
			set;
		}

		#region Util

		static Uri Build(string path)
		{
			return Build(path, null);
		}

		static Uri Build(string path, object args)
		{
			return Build(baseUri, path, args);
		}

		static Uri Build(Uri baseUri, string path)
		{
			return Build(baseUri, path, null);
		}

		static Uri Build(Uri baseUri, string path, object args)
		{
			var ub = new UriBuilder(baseUri);

			ub.Path += path;

			if (args != null)
				foreach (var i in args.GetType().GetProperties())
				{
					var value = i.GetValue(args, null);

					if (value == null ||
						value is int && (int)value == 0 ||
						value is long && (long)value == 0 ||
						value is StatusID && (StatusID)value == 0 ||
						value is UserID && (UserID)value == 0 ||
						value is ListID && (ListID)value == 0)
						continue;

					if (i.Name == "page" && value.Equals(1))
						continue;

					if (value is bool)
						SetQuery(ub, i.Name, value.ToString().ToLower());
					else
						SetQuery(ub, i.Name, value.ToString());
				}

			if (UseHttps)
			{
				ub.Scheme = Uri.UriSchemeHttps;
				ub.Port = 443;
			}

			return ub.Uri;
		}

		static void SetQuery(UriBuilder ub, string name, string value)
		{
			var str = name + "=" + Uri.EscapeDataString(value);

			if (string.IsNullOrEmpty(ub.Query))
				ub.Query = str;
			else
				ub.Query = ub.Query.TrimStart('?') + "&" + str;
		}

		#endregion

#pragma warning disable 1591

		public static Uri ReportSpam(UserID id)
		{
			return Build
			(
				"report_spam.json",
				new
				{
					user_id = id,
				}
			);
		}
		public static Uri ReportSpam(string id)
		{
			return Build
			(
				"report_spam.json",
				new
				{
					screen_name = id,
				}
			);
		}

		public static class Statuses
		{
			public static Uri PublicTimeline(StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					"statuses/public_timeline.json",
					new
					{
						since_id = sinceID,
						max_id = maxID,
						count,
						page,
						include_entities = true,
						include_my_retweet = true,
					}
				);
			}

			public static Uri HomeTimeline(StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					"statuses/home_timeline.json",
					new
					{
						since_id = sinceID,
						max_id = maxID,
						count,
						page,
						include_entities = true,
						include_my_retweet = true,
					}
				);
			}

			public static Uri UserTimeline(string id, StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					"statuses/user_timeline/" + id + ".json",
					new
					{
						include_rts = 1,
						since_id = sinceID,
						max_id = maxID,
						count,
						page,
						include_entities = true,
						include_my_retweet = true,
					}
				);
			}

			public static Uri Mentions(StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					"statuses/mentions.json",
					new
					{
						since_id = sinceID,
						max_id = maxID,
						count,
						page,
						include_entities = true,
					}
				);
			}

			public static Uri Show(StatusID id)
			{
				return Build
				(
					"statuses/show/" + id + ".json",
					new
					{
						include_entities = true,
					}
				);
			}

			public static Uri Update()
			{
				return Build
				(
					"statuses/update.json",
					new
					{
						include_entities = true,
					}
				);
			}

			public static Uri Destroy(StatusID id)
			{
				return Build
				(
					"statuses/destroy/" + id + ".json",
					new
					{
						include_entities = true,
					}
				);
			}

			public static Uri Retweet(StatusID id)
			{
				return Build
				(
					"statuses/retweet/" + id + ".json",
					new
					{
						include_entities = true,
					}
				);
			}

			public static Uri Friends(string id, long cursor = default(long))
			{
				return Build
				(
					"statuses/friends/" + id + ".json",
					new
					{
						cursor,
						include_entities = true,
					}
				);
			}

			public static Uri Followers(string id, long cursor = default(long))
			{
				return Build
				(
					"statuses/followers/" + id + ".json",
					new
					{
						cursor,
						include_entities = true,
					}
				);
			}

			public static Uri RetweetedByMe(StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					"statuses/retweeted_by_me.json",
					new
					{
						since_id = sinceID,
						max_id = maxID,
						count,
						page,
						include_entities = true,
					}
				);
			}

			public static Uri RetweetedToMe(StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					"statuses/retweeted_to_me.json",
					new
					{
						since_id = sinceID,
						max_id = maxID,
						count,
						page,
						include_entities = true,
					}
				);
			}

			public static Uri RetweetsOfMe(StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					"statuses/retweets_of_me.json",
					new
					{
						since_id = sinceID,
						max_id = maxID,
						count,
						page,
						include_entities = true,
					}
				);
			}
		}
		public static class Lists
		{
			public static Uri Create(string id)
			{
				return Build(id + "/lists.json");
			}

			public static Uri Update(string id, string listID)
			{
				return Build(id + "/lists/" + listID + ".json");
			}

			public static Uri Statuses(string id, string listID, StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					id + "/lists/" + listID + "/statuses.json",
					new
					{
						since_id = sinceID,
						max_id = maxID,
						per_page = count,
						page,
						include_entities = true,
					}
				);
			}

			public static Uri Index(string id, long cursor = default(long))
			{
				return Build
				(
					id + "/lists.json",
					new
					{
						cursor,
					}
				);
			}

			public static Uri Subscriptions(string id, long cursor = default(long))
			{
				return Build
				(
					id + "/lists/subscriptions.json",
					new
					{
						cursor,
					}
				);
			}

			public static Uri Show(string id, string listID)
			{
				return Build(id + "/lists/" + listID + ".json");
			}

			public static Uri Destroy(string id, string listID)
			{
				return Build(id + "/lists/" + listID + ".json");
			}

			public static Uri Memberships(string id, long cursor = default(long))
			{
				return Build
				(
					id + "/lists/memberships.json",
					new
					{
						cursor,
					}
				);
			}

			public static Uri Members(string id, string listID, long cursor = default(long))
			{
				return Build
				(
					id + "/" + listID + "/members.json",
					new
					{
						cursor,
					}
				);
			}

			public static Uri Members(string id, string listID, string userID)
			{
				return Build
				(
					id + "/" + listID + "/members.json",
					new
					{
						user_id = userID,
					}
				);
			}

			public static Uri Subscribers(string id, string listID, long cursor = default(long))
			{
				return Build
				(
					id + "/" + listID + "/subscribers.json",
					new
					{
						cursor,
					}
				);
			}

			public static Uri Subscribe(string id, string listID)
			{
				return Build(id + "/" + listID + "/subscribe.json");
			}
		}
		public static class DirectMessages
		{
			public static Uri Received(StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					"direct_messages.json",
					new
					{
						since_id = sinceID,
						max_id = maxID,
						per_page = count,
						page,
					}
				);
			}

			public static Uri Sent(StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					"direct_messages/sent.json",
					new
					{
						since_id = sinceID,
						max_id = maxID,
						per_page = count,
						page,
					}
				);
			}

			public static Uri New()
			{
				return Build("direct_messages/new.json");
			}

			public static Uri Destroy(StatusID id)
			{
				return Build("direct_messages/destroy/" + id + ".json");
			}
		}
		public static class OAuth
		{
			public static Uri RequestToken()
			{
				return Build(oAuthBaseUri, "request_token");
			}

			public static Uri AccessToken()
			{
				return Build(oAuthBaseUri, "access_token");
			}

			public static Uri Authorize(string requestToken)
			{
				return Build
				(
					oAuthBaseUri,
					"authorize",
					new
					{
						oauth_token = requestToken,
					}
				);
			}
		}
		public static class Search
		{
			public static Uri Query(string q, StatusID sinceID, StatusID maxID, int count, int page)
			{
				return Build
				(
					searchBaseUri,
					"search.json",
					new
					{
						q,
						lang = "ja",
						locale = "ja",
						since_id = sinceID,
						max_id = maxID,
						rpp = count,
						page,
					}
				);
			}
		}
		public static class Favorites
		{
			public static Uri Index(string id, int page)
			{
				return Build
				(
					"favorites/" + id + ".json",
					new
					{
						page,
						include_entities = true,
					}
				);
			}

			public static Uri Create(StatusID id)
			{
				return Build
				(
					"favorites/create/" + id + ".json",
					new
					{
						include_entities = true,
					}
				);
			}

			public static Uri Destroy(StatusID id)
			{
				return Build
				(
					"favorites/destroy/" + id + ".json",
					new
					{
						include_entities = true,
					}
				);
			}
		}
		public static class Users
		{
			public static Uri Show(string id)
			{
				return Build
				(
					"users/show/" + id + ".json",
					new
					{
						include_entities = true,
					}
				);
			}

			public static Uri Search(string q, int count, int page)
			{
				return Build
				(
					"users/search.json",
					new
					{
						q,
						per_page = count,
						page,
						include_entities = true,
					}
				);
			}
		}
		public static class Friendships
		{
			public static Uri Show(UserID sourceID, UserID targetID)
			{
				return Build
				(
					"friendships/show",
					new
					{
						source_id = sourceID,
						target_id = targetID,
					}
				);
			}
			public static Uri Show(UserID sourceID, string targetName)
			{
				return Build
				(
					"friendships/show",
					new
					{
						source_id = sourceID,
						target_screen_name = targetName,
					}
				);
			}
			public static Uri Show(string sourceName, UserID targetID)
			{
				return Build
				(
					"friendships/show",
					new
					{
						source_screen_name = sourceName,
						target_id = targetID,
					}
				);
			}
			public static Uri Show(string sourceName, string targetName)
			{
				return Build
				(
					"friendships/show.json",
					new
					{
						source_screen_name = sourceName,
						target_screen_name = targetName,
					}
				);
			}

			public static Uri Create(string id)
			{
				return Build("friendships/create/" + id + ".json");
			}

			public static Uri Destroy(string id)
			{
				return Build("friendships/destroy/" + id + ".json");
			}

			public static Uri Incoming(long cursor)
			{
				return Build
				(
					"friendships/incoming.json",
					new
					{
						cursor,
					}
				);
			}

			public static Uri Outgoing(long cursor)
			{
				return Build
				(
					"friendships/outgoing.json",
					new
					{
						cursor,
					}
				);
			}
		}
		public static class Friends
		{
			public static Uri Ids(string id, long cursor = default(long))
			{
				return Build
				(
					"friends/ids/" + id + ".json",
					new
					{
						cursor,
					}
				);
			}
		}
		public static class Followers
		{
			public static Uri Ids(string id, long cursor = default(long))
			{
				return Build
				(
					"followers/ids/" + id + ".json",
					new
					{
						cursor,
					}
				);
			}
		}
		public static class Blocks
		{
			public static Uri Blocking()
			{
				return Build
				(
					"blocks/blocking.json",
					new
					{
						include_entities = true,
					}
				);
			}

			public static Uri Create(UserID id)
			{
				return Build
				(
					"blocks/create.json",
					new
					{
						user_id = id,
					}
				);
			}
			public static Uri Create(string id)
			{
				return Build
				(
					"blocks/create.json",
					new
					{
						screen_name = id,
						include_entities = true,
					}
				);
			}

			public static Uri Destroy(UserID id)
			{
				return Build
				(
					"blocks/destroy.json",
					new
					{
						user_id = id,
					}
				);
			}
			public static Uri Destroy(string id)
			{
				return Build
				(
					"blocks/destroy.json",
					new
					{
						screen_name = id,
						include_entities = true,
					}
				);
			}

			public static Uri Exists(UserID id)
			{
				return Build
				(
					"blocks/exists.json",
					new
					{
						user_id = id,
						include_entities = true,
					}
				);
			}
			public static Uri Exists(string id)
			{
				return Build
				(
					"blocks/exists.json",
					new
					{
						screen_name = id,
						include_entities = true,
					}
				);
			}
		}
		public static class Stream
		{
			public static Uri User(IEnumerable<string> track, IEnumerable<UserID> follow)
			{
				return Build
				(
					new Uri("https://userstream.twitter.com/2/"),
					"user.json",
					new
					{
						track = track == null || !track.Any() ? null : track.Join(","),
						follow = follow == null || !follow.Any() ? null : track.Join(","),
					}
				);
			}
		}

#pragma warning restore 1591
	}
}
