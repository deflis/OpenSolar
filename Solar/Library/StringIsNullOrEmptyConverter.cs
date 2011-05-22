using Ignition.Presentation;

namespace Solar
{
	class StringIsNullOrEmptyConverter : OneWayValueConverter<string, bool>
	{
		protected override bool ConvertFromSource(string value, object parameter)
		{
			return string.IsNullOrEmpty(value);
		}
	}
}
