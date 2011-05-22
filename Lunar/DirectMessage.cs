using Codeplex.Data;

namespace Lunar
{
	/// <summary>
	/// ダイレクトメッセージを表します。
	/// </summary>
	public class DirectMessage : Status
	{
		/// <summary>
		/// Twitter クライアントと基になる JSON オブジェクトを指定し DirectMessage の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="client">Twitter クライアント。</param>
		/// <param name="json">基になる JSON オブジェクト。</param>
		public DirectMessage(TwitterClient client, DynamicJson json)
			: base(client, json, new User(client, ((dynamic)json).sender))
		{
			this.Recipient = new User(client, this.json.recipient);
		}

		/// <summary>
		/// ダイレクトメッセージであるかどうかを取得します。
		/// </summary>
		public override bool IsDirectMessage
		{
			get
			{
				return true;
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
		/// 差出人のユーザ ID を取得します。
		/// </summary>
		public override UserID UserID
		{
			get
			{
				return json.sender_id;
			}
		}

		/// <summary>
		/// 差出人のユーザ名を取得します。
		/// </summary>
		public override string UserName
		{
			get
			{
				return json.sender_screen_name;
			}
		}

		/// <summary>
		/// 受取人を取得します。
		/// </summary>
		public User Recipient
		{
			get;
			private set;
		}

		/// <summary>
		/// 受取人のユーザ名を取得します。
		/// </summary>
		public override string RecipientName
		{
			get
			{
				return this.Recipient.Name;
			}
		}

		/// <summary>
		/// このダイレクトメッセージを削除します。
		/// このメソッドは、TwitterClient インスタンスの存在するコンテキストで実行する必要があります。
		/// </summary>
		/// <returns>削除されたダイレクトメッセージ。</returns>
		public override Status Destroy()
		{
			return TwitterClient.CurrentInstance.DirectMessages.Destroy(this.StatusID);
		}
	}
}
