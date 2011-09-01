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
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Solar
{
#pragma warning disable 1591

    class UriBitmapConverter : OneWayValueConverter<Uri, BitmapImage>
    {
        static readonly ConcurrentDictionary<Uri, CacheValue> images = new ConcurrentDictionary<Uri, CacheValue>();
        const int MaxImages = 500;

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
                    images.AsParallel()
                        .OrderByDescending(x => x.Value.ReferenceCount)
                        .ThenByDescending(_ => _.Value.LastReference)
                        .Take(images.Count - MaxImages)
                        .ForAll(x => images.Remove(x.Key));
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
            lock (images)
            {
                if (value == null)
                    return null;
                else if (images.ContainsKey(value))
                    return images[value].Value;
            }

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
                return;
            }
            else if (images.ContainsKey(value))
            {
                func(images[value].Value);
                return;
            }

            Clean();

            try
            {
                if (File.Exists(GetCachePath(value)))
                {
                    lock (value)
                    {
                        File.SetLastWriteTime(GetCachePath(value), DateTime.Now);
                        ((Func<string, FileStream>)File.OpenRead).ToAsync()(GetCachePath(value))
                            .ObserveOn(Scheduler.NewThread)
                            .SelectMany(_ => Observable.Using(() => _, fs => new[] { fs.Freeze() }.ToObservable()))
                            .Do(ms => { lock (value) window.Dispatcher.Invoke(func, CreateImage(ms, value)); })
                            .Subscribe(ms => ms.Dispose(), ex => App.Log(ex), () => { });
                    }
                }
                else
                {
                    lock (value)
                    {
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(value);
                        req.UserAgent = "Solar";
                        req.Timeout = 30000;
                        Observable.FromAsyncPattern<WebResponse>(req.BeginGetResponse, req.EndGetResponse)()
                            .ObserveOn(Scheduler.NewThread)
                            .SelectMany(res => Observable.Using(() => res.GetResponseStream(), ns => new[] { ns.Freeze() }.ToObservable()))
                            .Do(ms => { lock (value) if (!File.Exists(GetCachePath(value))) File.WriteAllBytes(GetCachePath(value), ms.ToArray()); })
                            .Do(ms => { lock (value) window.Dispatcher.Invoke(func, CreateImage(ms, value)); })
                            .Subscribe(ms => ms.Dispose(), ex => App.Log(ex), () => { });
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log(ex);
                return;
            }
        }

        BitmapImage CreateImage(Stream ms, Uri value)
        {
            var rt = new BitmapImage();

            rt.BeginInit();
            rt.StreamSource = ms;
            rt.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            rt.CacheOption = BitmapCacheOption.OnLoad;
            rt.DecodePixelWidth = 48;
            rt.DecodePixelHeight = 48;
            rt.EndInit();
            RenderOptions.SetBitmapScalingMode(rt, BitmapScalingMode.NearestNeighbor);

            if (rt.CanFreeze)
                rt.Freeze();

            return images.AddOrUpdate(value, new CacheValue(rt), (_, oldValue) => new CacheValue(rt)).Value;
        }


        protected static System.Security.Cryptography.SHA1CryptoServiceProvider sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
        static readonly ConcurrentDictionary<Uri, String> cachePath = new ConcurrentDictionary<Uri, String>();
        protected static String GetCachePath(Uri uri)
        {
            if (cachePath.ContainsKey(uri))
                return cachePath[uri];

            var split_path = uri.AbsolutePath.Split("/");

            var path = @".imageCache\" + (split_path.Length > 2 ? (split_path.Reverse().Skip(1).First() + "-") : "") + BitConverter.ToString(sha1.ComputeHash(System.Text.Encoding.Default.GetBytes(uri.Host + uri.AbsoluteUri))).Replace("-", "") + "." + uri.AbsolutePath.Split(".").Last().Replace("/", "-");

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
