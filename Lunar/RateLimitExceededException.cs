using System;

namespace Lunar
{
	/// <summary>
	/// API 実行制限に達したときにスローされる例外。
	/// </summary>
	public class RateLimitExceededException : ServiceException
	{
		/// <summary>
		/// API 制限情報を指定し RateLimitExceededException の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="rateLimit">基になる API 制限情報。</param>
		public RateLimitExceededException(RateLimit rateLimit)
			: base("API 実行制限に達しました。" + rateLimit.Reset.ToString("HH:mm") + " までお待ちください。")
		{
			this.RateLimit = rateLimit;
		}

		/// <summary>
		/// API 制限情報を取得します。
		/// </summary>
		public RateLimit RateLimit
		{
			get;
			private set;
		}
	}
}
