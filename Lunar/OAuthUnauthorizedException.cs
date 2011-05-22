using System.Net;

namespace Lunar
{
	/// <summary>
	/// OAuth の認証に失敗したときにスローされる例外。
	/// </summary>
	public class OAuthUnauthorizedException : ContentedWebException
	{
		/// <summary>
		/// 基になる WebException を指定し OAuthUnauthorizedException の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="ex">基になる WebException。</param>
		public OAuthUnauthorizedException(WebException ex)
			: base("OAuth の認証に失敗しました: " + ex.Message, ex, ex.Status, ex.Response)
		{
		}
	}
}
