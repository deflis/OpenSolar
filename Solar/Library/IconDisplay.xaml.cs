using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Solar
{
#pragma warning disable 1591

	/// <summary>
	/// IconDisplay.xaml の相互作用ロジック
	/// </summary>
	partial class IconDisplay : UserControl
	{
		ImageSource image;
		Uri imageUri;
		internal static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(IconDisplay), new UIPropertyMetadata(null, OnSourceChanged));
		static readonly UriBitmapConverter conv = new UriBitmapConverter();

		internal Uri Source
		{
			get
			{
				return (Uri)GetValue(SourceProperty);
			}
			set
			{
				SetValue(SourceProperty, value);
			}
		}

		/// <summary>
		/// IconDisplay の新しいインスタンスを初期化します。
		/// </summary>
		public IconDisplay()
		{
			InitializeComponent();
            this.CacheMode = new BitmapCache(1);
		}

		void GetImage()
		{
			imageUri = this.Source;

			if (imageUri == null)
				this.image = null;
			else
                conv.ConvertAsync(imageUri, this, img => { image = (BitmapImage)img; this.InvalidateVisual(); });
                
		}

		static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var self = (IconDisplay)d;

			self.GetImage();
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (image != null)
			{
				var container = new Rect
				(
					this.Padding.Left,
					this.Padding.Top,
					this.ActualWidth - this.Padding.Right - this.Padding.Left,
					this.ActualHeight - this.Padding.Bottom - this.Padding.Top
				);
				var horizontal = image.Width > image.Height;
				var sz = new Size
				(
					horizontal ? container.Height / (image.Height / image.Width) : container.Width,
					horizontal ? container.Height : container.Width / (image.Width / image.Height)
				);
				var rect = new Rect
				(
					container.Left + container.Width / 2 - sz.Width / 2,
					container.Top + container.Height / 2 - sz.Height / 2,
					sz.Width,
					sz.Height
				);

				drawingContext.PushClip(new RectangleGeometry(container));
				drawingContext.DrawImage(image, rect);
				drawingContext.Pop();
			}
		}
	}

#pragma warning restore 1591
}
