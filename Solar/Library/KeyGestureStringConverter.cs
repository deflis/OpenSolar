using System.Globalization;
using System.Windows.Input;
using Ignition.Presentation;

namespace Solar
{
	class KeyGestureStringConverter : OneWayValueConverter<KeyGesture, string>
	{
		protected override string ConvertFromSource(KeyGesture value, object parameter)
		{
			if (value == null)
				return null;
			else
				return value.GetDisplayStringForCulture(CultureInfo.CurrentUICulture);
		}
	}
}
