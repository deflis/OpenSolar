using System;

namespace Lunar
{
	/// <summary>
	/// アカウント情報を提供します。
	/// </summary>
	[Serializable]
	public class AccountToken : IEquatable<AccountToken>
	{
		/// <summary>
		/// ユーザ名を取得します。
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// ユーザ ID を取得します。
		/// </summary>
		public UserID UserID
		{
			get;
			set;
		}

		/// <summary>
		/// OAuth 認証トークンを取得します。
		/// </summary>
		public string OAuthToken
		{
			get;
			set;
		}

		/// <summary>
		/// OAuth 秘密認証トークンを取得します。
		/// </summary>
		public string OAuthTokenSecret
		{
			get;
			set;
		}

		/// <summary>
		/// OAuth 認証されているかどうかを取得します。
		/// </summary>
		public bool IsAuthorized
		{
			get
			{
				return !string.IsNullOrEmpty(this.OAuthToken)
					&& !string.IsNullOrEmpty(this.OAuthTokenSecret);
			}
		}

		/// <summary>
		/// 保持されている OAuth 認証情報をクリアします。
		/// </summary>
		public void ClearToken()
		{
			this.OAuthToken = this.OAuthTokenSecret = null;
		}

		/// <summary>
		/// AccountToken の新しいインスタンスを初期化します。
		/// </summary>
		public AccountToken()
		{
		}

		/// <summary>
		/// ユーザ名を指定し AccountToken の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="name"></param>
		public AccountToken(string name)
			: this(name, default(UserID), null, null)
		{
		}

		/// <summary>
		/// ユーザ名、ユーザ ID、OAuth 認証トークンおよび OAuth 秘密認証トークンを指定し AccountToken の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="name">ユーザ名。</param>
		/// <param name="userID">ユーザ ID。</param>
		/// <param name="token">OAuth 認証トークン。</param>
		/// <param name="tokenSecret">OAuth 秘密認証トークン。</param>
		public AccountToken(string name, UserID userID, string token, string tokenSecret)
			: this()
		{
			this.Name = name;
			this.UserID = userID;
			this.OAuthToken = token;
			this.OAuthTokenSecret = tokenSecret;
		}

		/// <summary>
		/// 指定した AccountToken が現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="other">現在の AccountToken と比較する AccountToken。</param>
		/// <returns>等しいかどうか。</returns>
		public bool Equals(AccountToken other)
		{
			if (other == null)
				return false;

			return other.UserID == this.UserID
				|| other.Name == this.Name;
		}

		/// <summary>
		/// 現在の AccountToken の文字列表現を取得します。
		/// </summary>
		/// <returns>現在の AccountToken の文字列表現。</returns>
		public override string ToString()
		{
			return this.UserID == 0
				? "(すべて)"
				: this.Name ?? "(ID: " + this.UserID + ")";
		}

		/// <summary>
		/// 指定したオブジェクトが現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="obj">現在の AccountToken と比較するオブジェクト。</param>
		/// <returns>等しいかどうか。</returns>
		public override bool Equals(object obj)
		{
			return obj is AccountToken ? Equals((AccountToken)obj) : base.Equals(obj);
		}

		/// <summary>
		/// 現在の AccountToken のハッシュコードを取得します。
		/// </summary>
		/// <returns>現在の AccountToken のハッシュコード。</returns>
		public override int GetHashCode()
		{
			return this.UserID.GetHashCode();
		}
	}
}
