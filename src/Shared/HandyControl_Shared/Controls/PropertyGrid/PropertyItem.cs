using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Data;

namespace HandyControl.Controls
{
    public class PropertyItem : ListBoxItem
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(PropertyItem), new PropertyMetadata(default(object)));

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

        public string DisplayName
        {
            get => (string) GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

        public string PropertyName
        {
            get => (string) GetValue(PropertyNameProperty);
            set => SetValue(PropertyNameProperty, value);
        }

        public static readonly DependencyProperty PropertyTypeProperty =
            DependencyProperty.Register("PropertyType", typeof(Type), typeof(PropertyItem), new PropertyMetadata(default(Type)));

        public Type PropertyType
        {
            get => (Type) GetValue(PropertyTypeProperty);
            set => SetValue(PropertyTypeProperty, value);
        }

        public static readonly DependencyProperty PropertyTypeNameProperty =
            DependencyProperty.Register("PropertyTypeName", typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

        public string PropertyTypeName
        {
            get => (string) GetValue(PropertyTypeNameProperty);
            set => SetValue(PropertyTypeNameProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

        public string Description
        {
            get => (string) GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(PropertyItem), new PropertyMetadata(ValueBoxes.FalseBox));

        public bool IsReadOnly
        {
            get => (bool) GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, ValueBoxes.BooleanBox(value));
        }

        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(object), typeof(PropertyItem), new PropertyMetadata(default(object)));

        public object DefaultValue
        {
            get => GetValue(DefaultValueProperty);
            set => SetValue(DefaultValueProperty, value);
        }

        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

        public string Category
        {
            get => (string) GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        public static readonly DependencyProperty EditorProperty =
            DependencyProperty.Register("Editor", typeof(PropertyEditorBase), typeof(PropertyItem),
                                        new PropertyMetadata(default(PropertyEditorBase)));

        public PropertyEditorBase Editor
        {
            get => (PropertyEditorBase) GetValue(EditorProperty);
            set => SetValue(EditorProperty, value);
        }

        public static readonly DependencyProperty EditorElementProperty =
            DependencyProperty.Register("EditorElement", typeof(FrameworkElement), typeof(PropertyItem),
                                        new PropertyMetadata(default(FrameworkElement)));

        public FrameworkElement EditorElement
        {
            get => (FrameworkElement) GetValue(EditorElementProperty);
            set => SetValue(EditorElementProperty, value);
        }

        public static readonly DependencyProperty IsExpandedEnabledProperty =
            DependencyProperty.Register("IsExpandedEnabled", typeof(bool), typeof(PropertyItem), new PropertyMetadata(ValueBoxes.FalseBox));

        public bool IsExpandedEnabled
        {
            get => (bool) GetValue(IsExpandedEnabledProperty);
            set => SetValue(IsExpandedEnabledProperty, ValueBoxes.BooleanBox(value));
        }

        public static readonly DependencyProperty HierarchyLevelProperty =
            DependencyProperty.Register("HierarchyLevel", typeof(int?), typeof(PropertyItem), new PropertyMetadata(default(int?)));

        public int? HierarchyLevel
        {
            get => (int?) GetValue(HierarchyLevelProperty);
            set => SetValue(HierarchyLevelProperty, value);
        }

        public static readonly DependencyProperty PriorityProperty =
            DependencyProperty.Register("Priority", typeof(int), typeof(PropertyItem), new PropertyMetadata(default(int)));

        public int Priority
        {
            get => (int) GetValue(PriorityProperty);
            set => SetValue(PriorityProperty, value);
        }

        public static readonly DependencyProperty IsNecessaryProperty =
            DependencyProperty.Register("IsNecessary", typeof(bool), typeof(PropertyItem), new PropertyMetadata(ValueBoxes.FalseBox));

        public bool IsNecessary
        {
            get => (bool) GetValue(IsNecessaryProperty);
            set => SetValue(IsNecessaryProperty, ValueBoxes.BooleanBox(value));
        }

        public PropertyDescriptor PropertyDescriptor { get; set; }

        public virtual void InitElement()
        {
            if (Editor == null) return;
            EditorElement = Editor.CreateElement(this);
            Editor.CreateBinding(this, EditorElement);

            if (!IsNecessary)
            {
                return;
            }

            InfoElement.SetNecessary(this, true);
            TitleElement.SetTitle(EditorElement, DisplayName);
            TitleElement.SetTitlePlacement(EditorElement, TitlePlacementType.Left);
        }
    }
}
