using System.Windows;
using Ignition.Presentation;

namespace Solar
{
	class NegativeBooleanToVisibilityConverter : ValueConverter<bool, Visibility>
	{
		protected override Visibility ConvertFromSource(bool value, object parameter)
		{
			return value ? Visibility.Collapsed : Visibility.Visible;
		}

		protected override bool ConvertToSource(Visibility value, object parameter)
		{
			return value != Visibility.Visible;
		}
	}
}
