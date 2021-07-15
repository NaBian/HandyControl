using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace HandyControl.Tools.Converter
{
    public class BooleanArr2VisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return Visibility.Collapsed;
            }

            var arr = new List<bool>();
            foreach (var item in values)
            {
                if (item is bool boolValue)
                {
                    arr.Add(boolValue);
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            if (parameter is string useAny && useAny == "UseAny")
            {
                return arr.Any(item => item) ? Visibility.Visible : Visibility.Collapsed;
            }
            return arr.All(item => item) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
