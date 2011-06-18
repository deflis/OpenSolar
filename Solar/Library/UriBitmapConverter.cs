using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ignition;
using Ignition.Presentation;
using System.Windows.Threading;
using System.Threading.Tasks;

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

		public static void Clean()
		{
            if (images.Count > MaxImages)
            {
                lock (images)
                {
                    var items = images.AsParallel().Where(_ => _.Value.LastReference < DateTime.Now - TimeSpan.FromHours(1)).OrderByDescending(x => x.Value.ReferenceCount);
                   items.Take(items.Count() - MaxImages).ForAll(x => images.Remove(x.Key));
                }
                GC.Collect();
            }
		}

		public static void Clear()
		{
			images.Clear();
		}

		protected override BitmapImage ConvertFromSource(Uri value, object parameter)
		{
			if (value == null)
				return null;
			else if (images.ContainsKey(value))
                return images[value].Value;

            Clean();


			try
			{
				lock (value)
					using (var wc = new WebClient
					{
						Headers =
					    {
					        { HttpRequestHeader.UserAgent, "Solar" },
					    },
					})
					using (var ns = wc.OpenRead(value))
					using (var ms = ns.Freeze())
					{
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
			catch (Exception ex)
			{
				App.Log(ex);

				return null;
			}
		}

        public void ConvertAsync(Uri value, DispatcherObject window, Action<BitmapImage> func)
        {
            if (value == null)
            {
                func(null);
                return;
            }
            else if (images.ContainsKey(value))
            {
                func(images[value].Value);
                return;
            }

            Clean();

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        lock (value)
                            using (var wc = new WebClient
                            {
                                Headers =
                                {
                                    { HttpRequestHeader.UserAgent, "Solar" },
                                }
                            })
                            using (var ns = wc.OpenRead(value))
                            using (var ms = ns.Freeze())
                            {
                                var rt = new BitmapImage();

                                rt.BeginInit();
                                rt.StreamSource = ms;
                                rt.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                                rt.CacheOption = BitmapCacheOption.OnLoad;
                                rt.EndInit();
                                RenderOptions.SetBitmapScalingMode(rt, BitmapScalingMode.NearestNeighbor);

                                if (rt.CanFreeze)
                                    rt.Freeze();

                                var image = images.AddOrUpdate(value, new CacheValue(rt), (_, oldValue) => new CacheValue(rt)).Value;
                                window.Dispatcher.BeginInvoke((Action)(() => func(image)), DispatcherPriority.Background);
                                return;
                            }
                    }
                    catch (Exception ex)
                    {
                        App.Log(ex);
                        window.Dispatcher.BeginInvoke((Action)(() => func(null)), DispatcherPriority.Background);
                        return;
                    }
                });
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
