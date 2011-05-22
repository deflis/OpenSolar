using System;
using System.Threading.Tasks;
using System.Windows;
using Ignition;
using Ignition.Presentation;

namespace Solar
{
	/// <summary>
	/// サムネイルつきアドレスを表します。
	/// </summary>
	public class ThumbnailedUri : NotifyObject
	{
		Func<Uri, Uri> getThumbnail;

		/// <summary>
		/// 元のアドレスを取得します。
		/// </summary>
		public Uri Original
		{
			get;
			private set;
		}

		/// <summary>
		/// サムネイルのアドレスを取得します。
		/// </summary>
		public Uri Thumbnail
		{
			get
			{
				var rt = GetValue(() => this.Thumbnail);

				if (getThumbnail != null && rt == null)
					Task.Factory.StartNew(() =>
					{
						using (new ProgressBlock("サムネイルを解決しています..."))
							try
							{
								this.Thumbnail = getThumbnail(this.Original);
							}
							catch (Exception ex)
							{
								App.Log(ex);
								App.Current.Dispatcher.Invoke((Action)(() => MessageBoxEx.Show(ex.ToString(), ex.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error)));
							}
					});

				return rt;
			}
			private set
			{
				SetValue(() => this.Thumbnail, value);
			}
		}

		/// <summary>
		/// アドレスを指定し ThumbnailedUri の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="original">元のアドレス。このアドレスは、サムネイルのアドレスでもあります。</param>
		public ThumbnailedUri(Uri original)
			: this(original, original)
		{
		}

		/// <summary>
		/// 元のアドレスおよびサムネイルのアドレスを指定し ThumbnailedUri の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="original">元のアドレス。</param>
		/// <param name="thumbnail">サムネイルのアドレス。</param>
		public ThumbnailedUri(Uri original, Uri thumbnail)
		{
			this.Original = original;
			this.Thumbnail = thumbnail;
		}

		/// <summary>
		/// 元のアドレスおよびサムネイルアドレスの取得処理を指定し ThumbnailedUri の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="original">元のアドレス。</param>
		/// <param name="getThumbnail">サムネイルアドレスの取得処理。</param>
		public ThumbnailedUri(Uri original, Func<Uri, Uri> getThumbnail)
		{
			this.Original = original;
			this.getThumbnail = getThumbnail;
		}
	}
}
