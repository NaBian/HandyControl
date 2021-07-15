using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using HandyControl.Controls;

namespace HandyControl.Controls
{

    public abstract class PropertyEditorBase : DependencyObject
    {
        public abstract FrameworkElement CreateElement(PropertyItem propertyItem);

        public virtual void CreateBinding(PropertyItem propertyItem, DependencyObject element)
        {
            BindingOperations.SetBinding(element, GetDependencyProperty(),
                                         new Binding($"{propertyItem.PropertyName}")
                                         {
                                             Source              = propertyItem.Value,
                                             Mode                = GetBindingMode(propertyItem),
                                             UpdateSourceTrigger = GetUpdateSourceTrigger(propertyItem),
                                             Converter           = GetConverter(propertyItem)
                                         });
        }

        public abstract DependencyProperty GetDependencyProperty();

        public virtual BindingMode GetBindingMode(PropertyItem propertyItem) => propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;

        public virtual UpdateSourceTrigger GetUpdateSourceTrigger(PropertyItem propertyItem) => UpdateSourceTrigger.PropertyChanged;

        protected virtual IValueConverter GetConverter(PropertyItem propertyItem) => null;
    }
}
