using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Updater.Converter
{
    internal class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string stringValue) return DependencyProperty.UnsetValue;
            stringValue = stringValue.ToLower();

            return stringValue switch
            {
                "true" => true,
                "false" => false,
                _ => DependencyProperty.UnsetValue,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (boolValue) return "true";
                else return "false";
            }
            else return "false";
        }
    }
}
