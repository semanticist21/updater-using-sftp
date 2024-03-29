﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Updater.Converter
{
    internal class BooleanInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool result)
            {
                return !result;
            }
            else if (value is string stringValue)
            {
                if (stringValue.Equals("true") || stringValue.Equals("True"))
                {
                    return true;
                }
                else if (stringValue.Equals("false") || stringValue.Equals("False"))
                {
                    return false;
                }
                else return DependencyProperty.UnsetValue;
            }
            else return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool result)
            {
                return !result;
            }
            else return DependencyProperty.UnsetValue;
        }
    }
}
