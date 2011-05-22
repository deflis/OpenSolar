using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;
using Ignition;
using Lunar;
using Solar.Models;

namespace Solar
{
#pragma warning disable 1591

	class LinkConverter : IValueConverter
	{
		public static readonly Regex UriRegex = new Regex(@"https?://[-_.!~*'()a-zA-Z0-9;/?:\@&=+\$,%#]+", RegexOptions.Compiled);
		public static readonly Regex HashRegex = new Regex(@"#[_a-zA-Z0-9]+", RegexOptions.Compiled);
		public static readonly Regex LinkRegex = new Regex(@"(?<url>" + UriRegex.ToString() + @")|(?<user>@[_a-zA-Z0-9]+)|(?<hash>" + HashRegex.ToString() + @")", RegexOptions.Compiled);
		const int MaxResolvedUriCache = 500;

		public static OrderedDictionary<Uri, string> ResolvedUriCache
		{
			get;
			private set;
		}

		public bool MenuItems
		{
			get;
			set;
		}

		static LinkConverter()
		{
			ResolvedUriCache = new OrderedDictionary<Uri, string>();
		}

		public class LinkInfo
		{
			public Status Status
			{
				get;
				private set;
			}

			public string Link
			{
				get;
				private set;
			}

			public LinkInfo(Status status, string link)
			{
				this.Status = status;
				this.Link = link;
			}
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			var converted = (Status)value;
			var text = converted.Text;

			if (this.MenuItems)
				return text == null
					? Enumerable.Empty<MenuItem>()
					: LinkRegex.Matches(text).Cast<Match>().Select(_ =>
					{
						var mi = new MenuItem
						{
							Header = _.Value.Replace("_", "__"),
						};
						string commandName = null;

						if (_.Groups["user"].Success)
						{
							mi.CommandParameter = new LinkInfo(converted, _.Value.Substring(1));
							commandName = "UserDetailsCommand";
						}
						else if (_.Groups["hash"].Success)
						{
							mi.CommandParameter = new LinkInfo(converted, _.Value);
							commandName = "SearchCommand";
						}
						else if (_.Groups["url"].Success)
						{
							try
							{
								var uri = new Uri(_.Value);

								mi.CommandParameter = uri;
								ExpandUri(mi.Dispatcher, u => mi.ToolTip = u, uri);
							}
							catch
							{
								mi.CommandParameter = new Uri("about:blank");
							}

							commandName = "OpenUriCommand";
						}

						mi.SetResourceReference(MenuItem.CommandProperty, commandName);

						return mi;
					});

			var rt = new TextBlock();
			var idx = 0;

			if (string.IsNullOrEmpty(text))
				return rt;

			var matches = LinkRegex.Matches(text);

			if (matches.Count > 0)
			{
				foreach (Match i in matches)
				{
					if (idx != i.Index)
						rt.Inlines.Add(text.Substring(idx, i.Index - idx));

					var link = new Hyperlink(new Run(i.Value))
					{
						Focusable = false,
					};
					string commandName = null;

					if (i.Groups["url"].Success && Uri.IsWellFormedUriString(i.Value.TrimEnd('%'), UriKind.Absolute))
					{
						var uri = new Uri(i.Value);

						link.CommandParameter = uri;
						commandName = "OpenUriCommand";

						ExpandUri(link.Dispatcher, _ => link.ToolTip = _, uri);
					}
					else if (i.Groups["user"].Success)
					{
						link.CommandParameter = new LinkInfo(converted, i.Value.Substring(1));
						commandName = "UserDetailsCommand";
					}
					else if (i.Groups["hash"].Success)
					{
						link.CommandParameter = new LinkInfo(converted, i.Value);
						commandName = "SearchCommand";
					}

					if (commandName != null)
						link.SetResourceReference(Hyperlink.CommandProperty, commandName);

					rt.Inlines.Add(link);

					idx = i.Index + i.Length;
				}

				if (idx != text.Length)
					rt.Inlines.Add(text.Substring(idx));
			}
			else
				rt.Text = text;

			return rt;
		}

		static void ExpandUri(Dispatcher dispatcher, Action<string> setLink, Uri uri)
		{
			try
			{
				Task.Factory.StartNew(() =>
				{
					lock (ResolvedUriCache)
						if (ResolvedUriCache.ContainsKey(uri))
							dispatcher.BeginInvoke(setLink, DispatcherPriority.Background, ResolvedUriCache[uri]);
						else
						{
							var resolver = Client.Instance.UrlExpanders.FirstOrDefault(_ => _.GetVariable("IsShort")(uri));

							if (resolver != null)
								using (new ProgressBlock("短縮 URL を解決しています..."))
									lock (uri.AbsoluteUri)
										try
										{
											var rt = (string)resolver.GetVariable("Expand")(uri);

											if (rt == null)
												return;

											dispatcher.BeginInvoke(setLink, DispatcherPriority.Background, ResolvedUriCache[uri] = rt);

											if (ResolvedUriCache.Count > MaxResolvedUriCache)
												ResolvedUriCache.RemoveAt(0);
										}
										catch (Exception ex)
										{
											App.Log(ex);
										}
						}
				}, TaskCreationOptions.LongRunning);
			}
			catch (Exception ex)
			{
				App.Log(ex);
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

#pragma warning restore 1591
}
