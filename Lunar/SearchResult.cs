using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codeplex.Data;

namespace Lunar
{
	/// <summary>
	/// 検索結果を表します。
	/// </summary>
	public class SearchResult : Status
	{
		User user;
		StatusCache statusCache;
		static readonly SortedDictionary<string, object> getUserLock = new SortedDictionary<string, object>();

		/// <summary>
		/// Twitter クライアントと基になる JSON オブジェクトを指定し DirectMessage の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">Twitter クライアント。</param>
		/// <param name="json">基になる JSON オブジェクト。</param>
		public SearchResult(TwitterClient client, DynamicJson json)
			: base(client, json)
		{
			this.statusCache = client.StatusCache;
		}

		/// <summary>
		/// 検索結果であるかどうかを取得します。
		/// </summary>
		public override bool IsSearchResult
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// ユーザ名を取得します。
		/// </summary>
		public override string UserName
		{
			get
			{
				return json.from_user;
			}
		}

		/// <summary>
		/// protected であるかどうかを取得します。
		/// </summary>
		public override bool Protected
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// ユーザの名前を取得します。
		/// </summary>
		public override string FullUserName
		{
			get
			{
				var rt = GetValue(() => this.FullUserName);

				if (rt == null)
					Task.Factory.StartNew(() =>
					{
						using (new ReduceAuthenticatedQueryScope())
						using (var client = new TwitterClient(this.Account, statusCache))
							try
							{
								SetValue(() => this.FullUserName, (user ?? (user = client.Users.Get(this.UserName))).FullName);
							}
							catch
							{
							}
					}, TaskCreationOptions.LongRunning);

				return rt;
			}
		}

		/// <summary>
		/// ユーザ ID を取得します。
		/// </summary>
		public override UserID UserID
		{
			get
			{
				var rt = GetValue(() => this.UserID);

				if (rt == 0)
					Task.Factory.StartNew(() =>
					{
						using (new ReduceAuthenticatedQueryScope())
						using (var client = new TwitterClient(this.Account, statusCache))
							try
							{
								SetValue(() => this.UserID, (user ?? (user = client.Users.Get(this.UserName))).UserID);
							}
							catch
							{
							}
					}, TaskCreationOptions.LongRunning);

				return rt;
			}
		}

		/// <summary>
		/// ユーザ画像を取得します。
		/// </summary>
		public override Uri ProfileImage
		{
			get
			{
				return new Uri(json.profile_image_url);
			}
		}

		/// <summary>
		/// お気に入りしているかどうかを取得します。
		/// </summary>
		public override bool Favorited
		{
			get;
			set;
		}
	}
}
