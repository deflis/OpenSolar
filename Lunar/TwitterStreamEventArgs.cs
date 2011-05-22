using System;
using System.Globalization;
using Codeplex.Data;

namespace Lunar
{
	/// <summary>
	/// User Streams イベント データを提供します。
	/// </summary>
	public class TwitterStreamEventArgs : EventArgs
	{
		dynamic json;

		/// <summary>
		/// ソースを取得します。
		/// </summary>
		public User Source
		{
			get;
			private set;
		}

		/// <summary>
		/// 対象を取得します。
		/// </summary>
		public User Target
		{
			get;
			private set;
		}

		/// <summary>
		/// 対象のつぶやきが存在する場合、対象のつぶやきを取得します。
		/// </summary>
		public Status TargetStatus
		{
			get;
			private set;
		}

		/// <summary>
		/// 発生日時を取得します。
		/// </summary>
		public DateTime CreatedAt
		{
			get
			{
				return json.created_at()
				   ? DateTime.ParseExact(json.created_at, new[] { "ddd MMM dd HH:mm:ss zz00 yyyy", "ddd, dd MMM yyyy HH:mm:ss zz00" }, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None).ToLocalTime()
				   : DateTime.MinValue;
			}
		}

		/// <summary>
		/// イベントの種類を取得します。
		/// </summary>
		public string Type
		{
			get
			{
				return json.@event;
			}
		}

		/// <summary>
		/// 取得したアカウントを取得します。
		/// </summary>
		public AccountToken Account
		{
			get;
			private set;
		}

		/// <summary>
		/// Twitter クライアントと基になる JSON オブジェクトを指定し TwitterStreamEventArgs の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">Twitter クライアント。</param>
		/// <param name="json">基になる JSON オブジェクト。</param>
		public TwitterStreamEventArgs(TwitterClient client, DynamicJson json)
		{
			this.json = json;
			this.Account = client.Account;
			this.Source = new User(client, this.json.source);
			this.Target = new User(client, this.json.target);
			this.TargetStatus = this.json.target_object() ? new Status(client, this.json.target_object, this.Target) : null;
		}
	}
}
