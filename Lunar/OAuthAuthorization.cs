using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Lunar
{
	/// <summary>
	/// OAuth 認証を提供します。
	/// </summary>
	public class OAuthAuthorization : IDisposable
	{
		static readonly Random r = new Random();
		readonly WebClient client = new WebClient
		{
			Encoding = Encoding.UTF8,
		};
		readonly Regex uriEncodeConversionRegex = new Regex("%[0-9A-F]{2}", RegexOptions.Compiled);
		readonly MatchEvaluator uriEncodeConversionRegexMatch = _ => _.Value.ToLower();

		string requestToken;
		string requestTokenSecret;

		/// <summary>
		/// アカウント情報を取得または設定します。
		/// </summary>
		public AccountToken Token
		{
			get;
			set;
		}

		/// <summary>
		/// Consumer key を取得します。
		/// </summary>
		public string ConsumerKey
		{
			get;
			private set;
		}

		/// <summary>
		/// Consumer secret を取得します。
		/// </summary>
		public string ConsumerSecret
		{
			get;
			private set;
		}

		static OAuthAuthorization()
		{
			ServicePointManager.Expect100Continue = false;
		}

		/// <summary>
		/// アカウントを指定し OAuthAuthorization の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="token">アカウント。</param>
		public OAuthAuthorization(AccountToken token)
			: this("6Mp7uucDrPfErH1qLiu0bQ", "I2BvMSk5DIpFFdYpMpLvqA0ien8soagmw701gLOtkIk", token)
		{
		}

		/// <summary>
		/// OAuthAuthorization の新しいインスタンスを初期化します。
		/// </summary>
		public OAuthAuthorization()
			: this(null)
		{
		}

		/// <summary>
		/// Consumer key と Consumer secret を指定し OAuthAuthorization の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="consumerKey">Consumer key。</param>
		/// <param name="consumerSecret">Consumer secret。</param>
		public OAuthAuthorization(string consumerKey, string consumerSecret)
			: this(consumerKey, consumerSecret, null)
		{
		}

		/// <summary>
		/// Consumer key、 Consumer secret およびアカウントを指定し OAuthAuthorization の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="consumerKey">Consumer key。</param>
		/// <param name="consumerSecret">Consumer secret。</param>
		/// <param name="token">アカウント。</param>
		public OAuthAuthorization(string consumerKey, string consumerSecret, AccountToken token)
		{
			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
			this.Token = token;
		}

		/// <summary>
		/// 認証のためのアドレスを取得します。
		/// </summary>
		/// <returns>認証のためのアドレス。</returns>
		public Uri GetAuthorizationUri()
		{
			var parameters = CreateParameters();

			parameters.Add("oauth_signature", CreateSignature("POST", TwitterUriBuilder.OAuth.RequestToken(), parameters));

			try
			{
				var res = UploadString(TwitterUriBuilder.OAuth.RequestToken(), parameters);

				requestToken = res["oauth_token"];
				requestTokenSecret = res["oauth_token_secret"];

				return TwitterUriBuilder.OAuth.Authorize(requestToken);
			}
			catch (WebException ex)
			{
				throw new OAuthUnauthorizedException(ex);
			}
		}

		/// <summary>
		/// 指定した PIN で認証します。
		/// </summary>
		/// <param name="pin">PIN。</param>
		/// <returns>認証されたアカウント。</returns>
		public AccountToken Authenticate(string pin)
		{
			var parameters = CreateParameters(requestToken);

			parameters.Add("oauth_signature", CreateSignature("POST", TwitterUriBuilder.OAuth.RequestToken(), parameters, requestTokenSecret));
			parameters.Add("oauth_verifier", pin);

			try
			{
				var res = UploadString(TwitterUriBuilder.OAuth.AccessToken(), parameters);

				if (this.Token == null)
					this.Token = new AccountToken();

				this.Token.Name = res["screen_name"];
				this.Token.UserID = (UserID)long.Parse(res["user_id"]);
				this.Token.OAuthToken = res["oauth_token"];
				this.Token.OAuthTokenSecret = res["oauth_token_secret"];

				return this.Token;
			}
			catch (WebException ex)
			{
				throw new OAuthUnauthorizedException(ex);
			}
		}

		Dictionary<string, string> UploadString(Uri uri, Dictionary<string, string> parameters)
		{
			return client.UploadString(uri.AbsoluteUri, string.Join("&", parameters.Where(_ => !string.IsNullOrEmpty(_.Value)).OrderBy(_ => _.Key).Select(_ => _.Key + "=" + EscapeDataString(_.Value))))
						 .Split('&')
						 .Select(_ => _.Split('='))
						 .ToDictionary(_ => _.First(), _ => Uri.UnescapeDataString(_.Last()));
		}

		Dictionary<string, string> DownloadString(Uri uri, Dictionary<string, string> parameters)
		{
			return client.DownloadString(uri.AbsoluteUri + "?" + string.Join("&", parameters.Where(_ => !string.IsNullOrEmpty(_.Value)).OrderBy(_ => _.Key).Select(_ => _.Key + "=" + EscapeDataString(_.Value))))
						 .Split('&')
						 .Select(_ => _.Split('='))
						 .ToDictionary(_ => _.First(), _ => Uri.UnescapeDataString(_.Last()));
		}

		Dictionary<string, string> CreateParameters(string token = null)
		{
			return new Dictionary<string, string>
			{
				{ "oauth_consumer_key", this.ConsumerKey },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", CreateTimestamp().ToString() },
				{ "oauth_nonce", CreateOnce() },
				{ "oauth_version", "1.0" },
				{ "oauth_token", token },
			};
		}

		string CreateOnce()
		{
			const string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

			return string.Join(null, Enumerable.Range(0, 8)
											   .Select(_ => letters[r.Next(letters.Length)]));
		}

		long CreateTimestamp()
		{
			return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
		}

		/// <summary>
		/// 文字列をエスケープ表現に変換します。( ) * ' ! もエスケープします。
		/// </summary>
		/// <param name="s">文字列。</param>
		/// <returns>エスケープされた文字列。</returns>
		public static string EscapeDataString(string s)
		{
			return Uri.EscapeDataString(s)
					  .Replace("(", Uri.HexEscape('('))
					  .Replace(")", Uri.HexEscape(')'))
					  .Replace("*", Uri.HexEscape('*'))
					  .Replace("'", Uri.HexEscape('\''))
					  .Replace("!", Uri.HexEscape('!'));
		}

		/// <summary>
		/// 指定したメソッド、アドレスおよびクエリの OAuth シグネチャおよびパラメータを取得します。
		/// </summary>
		/// <param name="httpMethod">メソッド。</param>
		/// <param name="uri">アドレス。</param>
		/// <param name="query">クエリ。</param>
		/// <returns>OAuth シグネチャおよびパラメータ。</returns>
		public string CreateParameters(string httpMethod, Uri uri, string query = null)
		{
			var oAuthParameters = CreateParameters(this.Token.OAuthToken);
			var parameters = new Dictionary<string, string>(oAuthParameters);
			var uriString = uri.GetLeftPart(UriPartial.Path);

			if (string.IsNullOrEmpty(query))
				query = uri.Query;

			foreach (var i in query.TrimStart('?').Split('&').Select(_ => _.Split('=')))
				parameters.Add(i.First(), i.Last());

			oAuthParameters.Add("oauth_signature", CreateSignature(httpMethod, uri, parameters));

			return string.Join("&", oAuthParameters.Select(_ => _.Key + "=" + ConvertUrlEncode(EscapeDataString(_.Value))));
		}

		string ConvertUrlEncode(string query)
		{
			return uriEncodeConversionRegex.Replace(query, uriEncodeConversionRegexMatch);
		}

		string CreateSignature(string httpMethod, Uri uri, Dictionary<string, string> parameters)
		{
			return CreateSignature(httpMethod, uri, parameters, this.Token != null && this.Token.IsAuthorized ? this.Token.OAuthTokenSecret : string.Empty);
		}

		string CreateSignature(string httpMethod, Uri uri, Dictionary<string, string> parameters, string tokenSecret)
		{
			using (var hmacsha1 = new HMACSHA1
			{
				Key = Encoding.ASCII.GetBytes(EscapeDataString(ConsumerSecret) + '&' + EscapeDataString(tokenSecret)),
			})
			{
				var q = string.Join("&", parameters.Where(_ => !string.IsNullOrEmpty(_.Value)).OrderBy(_ => _.Key).Select(_ => _.Key + "=" + _.Value));
				var str = string.Join("&", new[]
				{
				    httpMethod,
				    EscapeDataString(uri.GetLeftPart(UriPartial.Path)),
				    EscapeDataString(q),
				});

				return Convert.ToBase64String(hmacsha1.ComputeHash(Encoding.ASCII.GetBytes(str)));
			}
		}

		/// <summary>
		/// OAuthAuthorization によって使用されているすべてのリソースを開放します。
		/// </summary>
		public void Dispose()
		{
			client.Dispose();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// dtor
		/// </summary>
		~OAuthAuthorization()
		{
			Dispose();
		}
	}
}
