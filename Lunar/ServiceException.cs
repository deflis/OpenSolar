using System;

namespace Lunar
{
	/// <summary>
	/// 外部サービスの例外を表します。
	/// </summary>
	public class ServiceException : Exception
	{
		/// <summary>
		/// ServiceException の新しいインスタンスを初期化します。
		/// </summary>
		public ServiceException()
		{
		}

		/// <summary>
		/// メッセージを指定し ServiceException の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="message">例外メッセージ。</param>
		public ServiceException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// メッセージと基になる例外を指定し ServiceException の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="message">例外メッセ維持。</param>
		/// <param name="innerException">基になる例外。</param>
		public ServiceException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
