using System;
using System.IO;
using System.Net;
using Codeplex.Data;

namespace Lunar
{
	/// <summary>
	/// WebException に受信内容を追加したものです。
	/// </summary>
	public class ContentedWebException : WebException
	{
		/// <summary>
		/// 受信内容を取得します。
		/// </summary>
		public string ContentText
		{
			get;
			set;
		}

		/// <summary>
		/// ContentedWebException クラスの新しいインスタンスを、指定したエラー メッセージ、入れ子になった例外、ステータス、および応答を使用して初期化します。
		/// </summary>
		/// <param name="message">エラー メッセージのテキスト。</param>
		/// <param name="innerException">入れ子になった例外。</param>
		/// <param name="status">System.Net.WebExceptionStatus 値の 1 つ。</param>
		/// <param name="response">リモート ホストからの応答を格納する System.Net.WebResponse インスタンス。</param>
		public ContentedWebException(string message, Exception innerException, WebExceptionStatus status, WebResponse response)
			: base(message, innerException, status, response)
		{
		}

		/// <summary>
		/// 指定された WebException を基に ContentedWebException の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="ex">基になる WebException。</param>
		/// <returns>新しい ContentedWebException。</returns>
		public static WebException Create(WebException ex)
		{
			var res = (HttpWebResponse)ex.Response;

			if (res == null)
				return ex;

			using (var sr = new StreamReader(res.GetResponseStream()))
			{
				var content = sr.ReadToEnd();
				var message = ex.Message;

				if (res.StatusCode == HttpStatusCode.ServiceUnavailable)
					message = "Twitter is over capacity.";
				else if (res.ContentType.StartsWith("application/json"))
				{
					dynamic json = DynamicJson.Parse(content);

					if (json.error())
						message = json.error;
				}

				return new ContentedWebException(message, ex, ex.Status, ex.Response)
				{
					ContentText = content,
				};
			}
		}
	}
}
