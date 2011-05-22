using System;
using Ignition.Presentation;

namespace Solar
{
	class DateTimeConverter : OneWayValueConverter<DateTime, string>
	{
		protected override string ConvertFromSource(DateTime value, object parameter)
		{
			if (value.DayOfYear == DateTime.Now.DayOfYear)
				return value.ToString("HH:mm:ss");
			else if (value.Year == DateTime.Now.Year)
				return value.ToString("MM/dd HH:mm:ss");
			else
				return value.ToString("yy/MM/dd HH:mm:ss");
		}
	}
}
