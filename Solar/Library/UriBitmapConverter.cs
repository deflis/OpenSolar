using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ignition;
using Ignition.Presentation;

namespace Solar
{
#pragma warning disable 1591

	class UriBitmapConverter : OneWayValueConverter<Uri, BitmapImage>
	{
		static readonly ConcurrentDictionary<Uri, CacheValue> images = new ConcurrentDictionary<Uri, CacheValue>();
		const int MaxImages = 2000;

		public static int CacheCount
		{
			get
			{
				return images.Count;
			}
		}

		static UriBitmapConverter()
		{
			Directory.CreateDirectory(".imageCache");
		}

		public static void Clean()
		{
			var items = images.Where(_ => _.Value.LastReference < DateTime.Now - TimeSpan.FromHours(1));

			while (items.Count() > MaxImages)
				images.Remove(items.Aggregate((x, y) => x.Value.ReferenceCount > y.Value.ReferenceCount ? y : x).Key);
		}

		public static void Clear()
		{
			images.Clear();
		}

		protected override BitmapImage ConvertFromSource(Uri value, object parameter)
		{
			Clean();

			if (value == null)
				return null;
			else if (images.ContainsKey(value))
				return images[value].Value;

			try
			{
				MemoryStream ms;
				lock (value)
				{
					if (File.Exists(GetCachePath(value)))
					{
						ms = File.OpenRead(GetCachePath(value)).Freeze();
					}
					else
					{
						using (var wc = new WebClient
						{
							Headers =
							{
								{ HttpRequestHeader.UserAgent, "Solar" },
							},
						})
						using (var ns = wc.OpenRead(value))
							ms = ns.Freeze();
					}
					using (ms)
					{
						File.WriteAllBytes(GetCachePath(value), ms.ToArray());
						var rt = new BitmapImage();

						rt.BeginInit();
						rt.StreamSource = ms;
						rt.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
						rt.CacheOption = BitmapCacheOption.OnLoad;
						rt.EndInit();
						RenderOptions.SetBitmapScalingMode(rt, BitmapScalingMode.NearestNeighbor);

						if (rt.CanFreeze)
							rt.Freeze();

						return images.AddOrUpdate(value, new CacheValue(rt), (_, oldValue) => new CacheValue(rt)).Value;
					}
				}
			}
			catch (Exception ex)
			{
				App.Log(ex);

				return null;
			}
		}

		private static String GetCachePath(Uri uri)
		{
			return @".imageCache\" + (uri.Host + uri.LocalPath).Replace('/', '.');
		}

		class CacheValue
		{
			readonly BitmapImage value;

			public CacheValue(BitmapImage value)
			{
				this.value = value;
			}

			public int ReferenceCount
			{
				get;
				private set;
			}

			public DateTime LastReference
			{
				get;
				private set;
			}

			public BitmapImage Value
			{
				get
				{
					this.ReferenceCount++;
					this.LastReference = DateTime.Now;

					return value;
				}
			}
		}
	}

#pragma warning restore 1591
}
