using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ignition;
using Ignition.Presentation;
using Lunar;

namespace Solar
{
	/// <summary>
	/// 画像サムネイル解決コンバータです。
	/// </summary>
	public class ImageUriResolver : OneWayValueConverter<Status, IEnumerable<ThumbnailedUri>>
	{
		static readonly Lazy<List<Func<Uri, ThumbnailedUri>>> resolvers = new Lazy<List<Func<Uri, ThumbnailedUri>>>(() => new List<Func<Uri, ThumbnailedUri>>
		{
			ConvertTwitPic,
		});

		/// <summary>
		/// リゾルバの一覧を取得します。
		/// </summary>
		public static List<Func<Uri, ThumbnailedUri>> Resolvers
		{
			get
			{
				return resolvers.Value;
			}
		}

		/// <summary>
		/// アドレスをサムネイル付きアドレスに変換します。
		/// </summary>
		/// <param name="value">アドレス。</param>
		/// <param name="parameter">使用されません。</param>
		/// <returns>サムネイル付きアドレス。</returns>
		protected override IEnumerable<ThumbnailedUri> ConvertFromSource(Status value, object parameter)
		{
			if (value == null)
				return null;
			else
				return LinkConverter.LinkRegex
									.Matches(value.Text ?? string.Empty).Cast<Match>()
									.Select(_ => _.Value)
									.Where(_ => _ != null && Uri.IsWellFormedUriString(_, UriKind.Absolute))
									.Select(_ => new Uri(_))
									.Select(_ => LinkConverter.ResolvedUriCache.ContainsKey(_) && Uri.IsWellFormedUriString(LinkConverter.ResolvedUriCache[_], UriKind.Absolute) ? new Uri(LinkConverter.ResolvedUriCache[_]) : _)
									.Select(Convert)
									.Where(_ => _ != null);
		}

		static ThumbnailedUri Convert(Uri uri)
		{
			return Resolvers.Aggregate((ThumbnailedUri)null, (x, y) => x ?? y(uri))
				?? ConvertStandard(uri);
		}

		static ThumbnailedUri ConvertTwitPic(Uri uri)
		{
			if (uri.Host == "twitpic.com" && uri.Segments.Length == 2)
				return new ThumbnailedUri(uri, new Uri("http://twitpic.com/show/mini" + uri.AbsolutePath));
			else
				return null;
		}

		static ThumbnailedUri ConvertStandard(Uri uri)
		{
			if (uri.AbsolutePath.EndsWith(StringComparison.OrdinalIgnoreCase, ".png", ".jpg", ".gif"))
				return new ThumbnailedUri(uri);
			else
				return null;
		}
	}
}
