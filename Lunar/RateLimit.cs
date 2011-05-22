using System;

namespace Lunar
{
	/// <summary>
	/// API 制限情報を提供します。
	/// </summary>
	public struct RateLimit : IEquatable<RateLimit>
	{
		/// <summary>
		/// 最大実行数を取得します。
		/// </summary>
		public int Limit
		{
			get;
			private set;
		}

		/// <summary>
		/// 現在の残実行数を取得します。
		/// </summary>
		public int Remaining
		{
			get;
			private set;
		}

		/// <summary>
		/// 実行制限のリセット時刻を取得します。
		/// </summary>
		public DateTime Reset
		{
			get;
			private set;
		}

		/// <summary>
		/// この実行制限が適用されるアカウントを取得します。
		/// </summary>
		public AccountToken Account
		{
			get;
			private set;
		}

		/// <summary>
		/// アカウント、残実行数、最大実行数およびリセット時刻を指定し、RateLimit の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="account">アカウント。</param>
		/// <param name="remaining">残実行数。</param>
		/// <param name="limit">最大実行数。</param>
		/// <param name="reset">リセット時刻。</param>
		public RateLimit(AccountToken account, int remaining, int limit, DateTime reset)
			: this()
		{
			this.Account = account;
			this.Remaining = remaining;
			this.Limit = limit;
			this.Reset = reset;
		}

		/// <summary>
		/// 指定した RateLimit が現在のインスタンスと等しいかどうか判断します。
		/// </summary>
		/// <param name="other">現在の RateLimit と比較する RateLimit。</param>
		/// <returns>等しいかどうか。</returns>
		public bool Equals(RateLimit other)
		{
			return other.Account == null && this.Account == null
				|| other.Account != null && other.Account.Equals(this.Account)
				|| this.Account != null && this.Account.Equals(other.Account);
		}

		/// <summary>
		/// 現在の RateLimit のハッシュコードを取得します。
		/// </summary>
		/// <returns>現在の RateLimit のハッシュコード。</returns>
		public override int GetHashCode()
		{
			return this.Account == null ? 0 : this.Account.GetHashCode();
		}
	}
}
