using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Ignition;
using Lunar;

namespace Solar
{
	[ContentProperty("Items")]
	class LinkContextMenuConverter : DependencyObject, IValueConverter
	{
		public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(Collection<object>), typeof(LinkContextMenuConverter), new UIPropertyMetadata(null));
		public static readonly DependencyProperty CommandHandlerProperty = DependencyProperty.Register("CommandHandler", typeof(StatusesListBoxCommandHandler), typeof(LinkContextMenuConverter), new UIPropertyMetadata(null));
		readonly LinkConverter linkConverter = new LinkConverter
		{
			MenuItems = true,
		};

		public LinkContextMenuConverter()
		{
			this.Items = new Collection<object>();
		}

		public Collection<object> Items
		{
			get
			{
				return (Collection<object>)GetValue(ItemsProperty);
			}
			set
			{
				SetValue(ItemsProperty, value);
			}
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var converted = (Status)value;
			var links = (IEnumerable<object>)linkConverter.Convert(value, typeof(IEnumerable<object>), null, culture);
			var items = this.Items == null
				? links
				: links != null && links.Any()
					? links.Append(new Separator()).Concat(this.Items.Cast<object>())
					: this.Items.Cast<object>();
			var ctx = new ContextMenu();

			ctx.Opened += (sender, e) =>
			{
				ctx.Items.Clear();

				foreach (var i in items)
				{
					if (i is Control &&
						((Control)i).Parent != null)
						((ItemsControl)((Control)i).Parent).Items.Remove(i);

					ctx.Items.Add(i);
				}
			};
			ctx.Closed += (sender, e) => ctx.Items.Clear();

			return ctx;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
