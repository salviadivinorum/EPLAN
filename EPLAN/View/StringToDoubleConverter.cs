using System;
using System.Globalization;
using System.Windows.Data;

namespace EPLAN.View
{
	[ValueConversion(typeof(object), typeof(string))]
	internal class StringToDoubleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}

			if (double.TryParse(value as string, out double number))
			{
				return number;
			}
			return (double)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}

			string strValue = value.ToString();
			if (double.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double numberValue))
			{
				return numberValue;
			}
			return null;
		}
	}
}
