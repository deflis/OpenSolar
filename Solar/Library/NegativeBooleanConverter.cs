using Ignition.Presentation;

namespace Solar
{
	class NegativeBooleanConverter : ValueConverter<bool, bool>
	{
		protected override bool ConvertFromSource(bool value, object parameter)
		{
			return !value;
		}

		protected override bool ConvertToSource(bool value, object parameter)
		{
			return !value;
		}
	}
}
