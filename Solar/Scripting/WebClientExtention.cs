using System;
using System.Linq;
using System.Net;
using Ignition;
using IronPython.Runtime;
using Microsoft.Scripting.Runtime;

[assembly: ExtensionTypeAttribute(typeof(WebClient), typeof(Solar.Scripting.WebClientExtention))]
namespace Solar.Scripting
{
	/// <summary>
	/// System.Net.WebClient の IronPython 向け拡張メソッドを提供します。
	/// </summary>
	public static class WebClientExtention
	{
		/// <summary>
		/// POST メソッドを使用して、指定したリソースに指定した辞書をアップロードします。
		/// </summary>
		/// <param name="self">WebClient インスタンス。</param>
		/// <param name="address">コレクションを受信するリソースの URI。</param>
		/// <param name="dict">リソースに送信する dict。</param>
		/// <returns>サーバーが送信した応答を格納している文字列。</returns>
		public static string UploadDict(this WebClient self, Uri address, PythonDictionary dict)
		{
			return self.UploadDict(address.AbsoluteUri, dict);
		}

		/// <summary>
		/// 指定したメソッドを使用して、指定したリソースに指定した辞書をアップロードします。
		/// </summary>
		/// <param name="self">WebClient インスタンス。</param>
		/// <param name="address">コレクションを受信するリソースの URI。</param>
		/// <param name="method">リソースに文字列を送信するために使用する HTTP メソッド。</param>
		/// <param name="dict">リソースに送信する dict。</param>
		/// <returns>サーバーが送信した応答を格納している文字列。</returns>
		public static string UploadDict(this WebClient self, Uri address, string method, PythonDictionary dict)
		{
			return self.UploadDict(address.AbsoluteUri, method, dict);
		}

		/// <summary>
		/// POST メソッドを使用して、指定したリソースに指定した辞書をアップロードします。
		/// </summary>
		/// <param name="self">WebClient インスタンス。</param>
		/// <param name="address">コレクションを受信するリソースの URI。</param>
		/// <param name="dict">リソースに送信する dict。</param>
		/// <returns>サーバーが送信した応答を格納している文字列。</returns>
		public static string UploadDict(this WebClient self, string address, PythonDictionary dict)
		{
			return self.UploadDict(address, null, dict);
		}

		/// <summary>
		/// 指定したメソッドを使用して、指定したリソースに指定した辞書をアップロードします。
		/// </summary>
		/// <param name="self">WebClient インスタンス。</param>
		/// <param name="address">コレクションを受信するリソースの URI。</param>
		/// <param name="method">リソースに文字列を送信するために使用する HTTP メソッド。</param>
		/// <param name="dict">リソースに送信する dict。</param>
		/// <returns>サーバーが送信した応答を格納している文字列。</returns>
		public static string UploadDict(this WebClient self, string address, string method, PythonDictionary dict)
		{
			self.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=UTF-8";

			return self.UploadString(address, method ?? "POST", dict.Select(_ => Uri.EscapeDataString((_.Key ?? "").ToString()) + "=" + Uri.EscapeDataString((_.Value ?? "").ToString())).Join("&"));
		}
	}
}
