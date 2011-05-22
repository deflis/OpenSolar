using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Solar
{
	class DropDownButton : ToggleButton
	{
		public DropDownButton()
		{
			this.SetBinding(ToggleButton.IsCheckedProperty, new Binding("DropDownMenu.IsOpen")
			{
				Source = this,
			});
		}

		public ContextMenu DropDownMenu
		{
			get
			{
				return (ContextMenu)GetValue(DropDownMenuProperty);
			}
			set
			{
				SetValue(DropDownMenuProperty, value);
			}
		}

		public static readonly DependencyProperty DropDownMenuProperty =
			DependencyProperty.Register("DropDownMenu", typeof(ContextMenu), typeof(DropDownButton), new UIPropertyMetadata(null));

		protected override void OnClick()
		{
			if (this.DropDownMenu != null)
			{
				this.DropDownMenu.PlacementTarget = this;
				this.DropDownMenu.Placement = PlacementMode.Bottom;
			}

			base.OnClick();
		}
	}
}
