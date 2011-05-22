using System.Collections.ObjectModel;
using Ignition;
using Ignition.Presentation;

namespace Solar.Dialogs
{
	class MultiLineConverter : ValueConverter<Collection<string>, string>
	{
		protected override string ConvertFromSource(Collection<string> value, object parameter)
		{
			return string.Join("\r\n", value);
		}

		protected override Collection<string> ConvertToSource(string value, object parameter)
		{
			return value.Split("\r\n").ToCollection();
		}
	}
}
