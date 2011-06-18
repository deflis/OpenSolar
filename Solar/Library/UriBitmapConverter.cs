using System;
using System.Collections.Concurrent;
using System.IO;
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

        static UriBitmapConverter()
        {
            Directory.CreateDirectory(".imageCache");
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
                        File.WriteAllBytes(GetCachePath(value), ms.ToArray());
                    }
                    using (ms)
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
                        MemoryStream ms;
                        lock (value)
                        {
                            bool exist;
                            if (exist = File.Exists(GetCachePath(value)))
                            {
                                File.SetLastWriteTime(GetCachePath(value), DateTime.Now);
                                ms = File.OpenRead(GetCachePath(value)).Freeze();
                            }
                            else
                            {
                                using (var wc = new WebClient
                                {
                                    Headers =
                                    {
                                        { HttpRequestHeader.UserAgent, "Solar" },
                                    }
                                })
                                using (var ns = wc.OpenRead(value))
                                {
                                    ms = ns.Freeze();
                                }
                            }
                            using (ms)
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

                                if (!exist)
                                {
                                    var arr = ms.ToArray();
                                    try
                                    {
                                        File.WriteAllBytes(GetCachePath(value), arr);
                                    }
                                    catch (IOException)
                                    {
                                    }
                                }
                            }
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

        protected static System.Security.Cryptography.SHA1CryptoServiceProvider sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
        static readonly ConcurrentDictionary<Uri, String> cachePath = new ConcurrentDictionary<Uri, String>();
        protected static String GetCachePath(Uri uri)
        {
            if (cachePath.ContainsKey(uri))
                return cachePath[uri];

            var split_path = uri.AbsolutePath.Split("/");

            var path = @".imageCache\" + (split_path.Length > 2 ?( split_path.Reverse().Skip(1).First() + "-") : "") + BitConverter.ToString(sha1.ComputeHash(System.Text.Encoding.Default.GetBytes(uri.Host + uri.AbsoluteUri))).Replace("-", "") + "." + uri.AbsolutePath.Split(".").Last();
            
            cachePath.Add(uri, path);
            return path;
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
