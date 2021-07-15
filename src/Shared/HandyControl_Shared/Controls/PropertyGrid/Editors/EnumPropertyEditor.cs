using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace HandyControl.Controls
{
    public class EnumPropertyEditor : PropertyEditorBase
    {
        public override FrameworkElement CreateElement(PropertyItem propertyItem) =>
            new System.Windows.Controls.ComboBox
            {
                IsEnabled   = !propertyItem.IsReadOnly,
                ItemsSource = EnumPropertyEditor.GetEnumDescription(propertyItem.PropertyType)
            };

        public override DependencyProperty GetDependencyProperty() => Selector.SelectedValueProperty;

        protected override IValueConverter GetConverter(PropertyItem propertyItem) => new EnumDescriptionConverter(propertyItem.PropertyType);

        private static IEnumerable<string> GetEnumDescription(Type enumType)
        {
            var values    = Enum.GetValues(enumType);
            var enumDescs = new List<string>(values.Length);

            enumDescs.AddRange(from object value in values
                               select enumType.GetField(value.ToString())
                               into field
                               let attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                               select (attributes.Length == 0 ? field.Name : ((DescriptionAttribute) attributes[0]).Description));

            return enumDescs;
        }

        private class EnumDescriptionConverter : IValueConverter
        {
            private readonly List<KeyValuePair<Enum, string>> cache;

            public EnumDescriptionConverter(Type enumType)
            {
                var values = Enum.GetValues(enumType);

                cache = new List<KeyValuePair<Enum, string>>(values.Length);

                foreach (var value in values)
                {
                    var field      = enumType.GetField(value.ToString());
                    var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    var fieldValue = (attributes.Length == 0 ? field.Name : ((DescriptionAttribute) attributes[0]).Description);

                    cache.Add(new KeyValuePair<Enum, string>((Enum) value, fieldValue));
                }
            }

            object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
                cache.FirstOrDefault(x => x.Key.Equals((Enum) value)).Value;

            object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
                cache.FirstOrDefault(x => x.Value == (string) value).Key;
        }
    }
}
