using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using HandyControl.Data;
using HandyControl.Data.Enum;
using HandyControl.Interactivity;
using HandyControl.Tools.Extension;

namespace HandyControl.Controls
{
    [TemplatePart(Name = ElementItemsControl, Type = typeof(ItemsControl))]
    [TemplatePart(Name = ElementSearchBar, Type = typeof(SearchBar))]
    public class PropertyGrid : Control
    {
        private const string ElementItemsControl = "PART_ItemsControl";

        private const string ElementSearchBar = "PART_SearchBar";

        private ItemsControl _itemsControl;

        private ICollectionView _dataView;

        private SearchBar _searchBar;

        private string _searchKey;

        public PropertyGrid()
        {
            CommandBindings.Add(new CommandBinding(ControlCommands.SortByCategory, SortByCategory, (s, e) => e.CanExecute = ShowSortButton));
            CommandBindings.Add(new CommandBinding(ControlCommands.SortByName, SortByName, (s, e) => e.CanExecute = ShowSortButton));
            CommandBindings.Add(new CommandBinding(ControlCommands.SortByHierarchyLevel, SortByHierarchyLevel, (s, e) => e.CanExecute = ShowSortButton));
        }

        public virtual PropertyResolver PropertyResolver { get; } = new();

        public static readonly RoutedEvent SelectedObjectChangedEvent =
            EventManager.RegisterRoutedEvent("SelectedObjectChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<object>), typeof(PropertyGrid));

        public event RoutedPropertyChangedEventHandler<object> SelectedObjectChanged
        {
            add => AddHandler(SelectedObjectChangedEvent, value);
            remove => RemoveHandler(SelectedObjectChangedEvent, value);
        }

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(object), typeof(PropertyGrid), new PropertyMetadata(default, OnSelectedObjectChanged));

        private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (PropertyGrid) d;
            ctl.OnSelectedObjectChanged(e.OldValue, e.NewValue);
        }

        public object SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        protected virtual void OnSelectedObjectChanged(object oldValue, object newValue)
        {
            UpdateItems(newValue);
            RaiseEvent(new RoutedPropertyChangedEventArgs<object>(oldValue, newValue, SelectedObjectChangedEvent));
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(PropertyGrid), new PropertyMetadata(default(string)));

        public string Description
        {
            get => (string) GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty MaxTitleWidthProperty = DependencyProperty.Register(
            "MaxTitleWidth", typeof(double), typeof(PropertyGrid), new PropertyMetadata(ValueBoxes.Double0Box));

        public double MaxTitleWidth
        {
            get => (double) GetValue(MaxTitleWidthProperty);
            set => SetValue(MaxTitleWidthProperty, value);
        }

        public static readonly DependencyProperty MinTitleWidthProperty = DependencyProperty.Register(
            "MinTitleWidth", typeof(double), typeof(PropertyGrid), new PropertyMetadata(ValueBoxes.Double0Box));

        public double MinTitleWidth
        {
            get => (double) GetValue(MinTitleWidthProperty);
            set => SetValue(MinTitleWidthProperty, value);
        }

        public static readonly DependencyProperty ShowSortButtonProperty = DependencyProperty.Register(
            "ShowSortButton", typeof(bool), typeof(PropertyGrid), new PropertyMetadata(ValueBoxes.TrueBox));

        public bool ShowSortButton
        {
            get => (bool) GetValue(ShowSortButtonProperty);
            set => SetValue(ShowSortButtonProperty, value);
        }

        public static readonly DependencyProperty ShowSearchBarProperty = DependencyProperty.Register(
            "ShowSearchBar", typeof(bool), typeof(PropertyGrid), new PropertyMetadata(ValueBoxes.TrueBox));

        public bool ShowSearchBar
        {
            get => (bool) GetValue(ShowSearchBarProperty);
            set => SetValue(ShowSearchBarProperty, value);
        }

        public static readonly DependencyProperty FlattenChildPropertiesProperty = DependencyProperty.Register(
         "FlattenChildProperties", typeof(Flattening), typeof(PropertyGrid), new PropertyMetadata(Flattening.Off));

        public Flattening FlattenChildProperties
        {
            get => (Flattening) GetValue(FlattenChildPropertiesProperty);
            set => SetValue(FlattenChildPropertiesProperty, value);
        }

        public static readonly DependencyProperty DefaultSortingProperty = DependencyProperty.Register(
         "DefaultSorting", typeof(SortingMode), typeof(PropertyGrid), new PropertyMetadata(SortingMode.Category));

        public SortingMode DefaultSorting
        {
            get => (SortingMode) GetValue(DefaultSortingProperty);
            set => SetValue(DefaultSortingProperty, value);
        }

        public static readonly DependencyProperty GroupHeaderTemplateProperty = DependencyProperty.Register(
         nameof(GroupHeaderTemplate), typeof(DataTemplate), typeof(PropertyGrid), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate GroupHeaderTemplate
        {
            get => (DataTemplate) GetValue(GroupHeaderTemplateProperty);
            set => SetValue(GroupHeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty GroupHeaderMarginProperty = DependencyProperty.Register(
         nameof(GroupHeaderMargin), typeof(Thickness), typeof(PropertyGrid),
         new FrameworkPropertyMetadata(new Thickness(0, 0, 0, 6), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Thickness GroupHeaderMargin
        {
            get => (Thickness) GetValue(GroupHeaderMarginProperty);
            set => SetValue(GroupHeaderMarginProperty, value);
        }

        public static readonly DependencyProperty SortByPriorityProperty = DependencyProperty.Register(
         "SortByPriority", typeof(bool), typeof(PropertyGrid), new PropertyMetadata(ValueBoxes.TrueBox));

        public bool SortByPriority
        {
            get => (bool) GetValue(SortByPriorityProperty);
            set => SetValue(SortByPriorityProperty, value);
        }

        public override void OnApplyTemplate()
        {
            if (_searchBar != null)
            {
                _searchBar.SearchStarted -= SearchBar_SearchStarted;
            }

            base.OnApplyTemplate();

            _itemsControl = GetTemplateChild(ElementItemsControl) as ItemsControl;
            _searchBar = GetTemplateChild(ElementSearchBar) as SearchBar;

            if (_searchBar != null)
            {
                _searchBar.SearchStarted += SearchBar_SearchStarted;
            }

            UpdateItems(SelectedObject);
        }

        /// <summary>
        /// Algorithmic helper class to temporarily link parent data to a PropertyDescriptorCollection
        /// </summary>
        private class ParentPropertyDescriptorCollection
        {
            public ParentPropertyDescriptorCollection(PropertyDescriptorCollection properties, object parent, string category)
            {
                Properties      = properties;
                Category        = category;
                ParentComponent = parent;
            }

            public PropertyDescriptorCollection Properties      { get; }
            public string                       Category        { get; }
            public object                       ParentComponent { get; }
        }

        private IEnumerable<PropertyItem> FlattenUnknownProperties(ParentPropertyDescriptorCollection collection, int hierarchyLevel) =>
            FlattenUnknownProperties(collection.Properties, collection.ParentComponent, collection.Category, hierarchyLevel);

        private IEnumerable<PropertyItem> FlattenUnknownProperties(PropertyDescriptorCollection propertiesToFlatten, object component, string parentCategory, int hierarchyLevel = 1)
        {
            var browsableProperties = propertiesToFlatten.OfType<PropertyDescriptor>()
                                                         .Where(item => PropertyResolver.ResolveIsBrowsable(item)).ToList();

            var knownProperties = browsableProperties.Where(item => PropertyResolver.IsKnownEditorType(item.PropertyType) || PropertyResolver.HasEditorType(item))
                                                     .Select(item => CreatePropertyItem(item, component, parentCategory, hierarchyLevel))
                                                     .Do(item => item.InitElement());

            var unknownPropertiesCollections = browsableProperties.Where(item => !PropertyResolver.IsKnownEditorType(item.PropertyType) && !PropertyResolver.HasEditorType(item))
                                                                  .Select(item => GetCategorizedChildProperties(item, component));

            return unknownPropertiesCollections.Select(coll => FlattenUnknownProperties(coll, hierarchyLevel + 1))
                                               .Aggregate(knownProperties, (current, flattenedChildProperties) => current.Concat(flattenedChildProperties));
        }

        private ParentPropertyDescriptorCollection GetCategorizedChildProperties(PropertyDescriptor parentItem, object parentComponent)
        {
            string category = null;
            switch (FlattenChildProperties)
            {
                case Flattening.ParentCategory:
                    category = PropertyResolver.ResolveCategory(parentItem);
                    break;
                case Flattening.ParentNameAsCategory:
                    category = parentItem.DisplayName;
                    break;
            }
            return new ParentPropertyDescriptorCollection(parentItem.GetChildProperties(), parentItem.GetValue(parentComponent), category);
        }

        private void UpdateItems(object obj)
        {
            if (obj == null || _itemsControl == null) return;

            if (FlattenChildProperties == Flattening.Off)
            {
                _dataView = CollectionViewSource.GetDefaultView(TypeDescriptor.GetProperties(obj.GetType())
                                                                              .OfType<PropertyDescriptor>()
                                                                              .Where(item => PropertyResolver.ResolveIsBrowsable(item))
                                                                              .Select(item => CreatePropertyItem(item, obj, null, 0))
                                                                              .Do(item => item.InitElement()));
            }
            else
            {
                _dataView = CollectionViewSource.GetDefaultView(FlattenUnknownProperties(TypeDescriptor.GetProperties(obj.GetType()), obj, null));
            }

            switch (DefaultSorting)
            {
                case SortingMode.Name:
                    SortByName(null, null);
                    break;
                case SortingMode.Hierarchy:
                    SortByHierarchyLevel(null, null);
                    break;
                default:
                    SortByCategory(null, null);
                    break;
            }

            _itemsControl.ItemsSource = _dataView;
        }

        private void SortByCategory(object sender, ExecutedRoutedEventArgs e)
        {
            if (_dataView == null) return;

            using (_dataView.DeferRefresh())
            {
                _dataView.GroupDescriptions.Clear();
                _dataView.SortDescriptions.Clear();
                if (SortByPriority)
                {
                    _dataView.SortDescriptions.Add(new SortDescription(PropertyItem.PriorityProperty.Name, ListSortDirection.Descending));
                }
                _dataView.SortDescriptions.Add(new SortDescription(PropertyItem.CategoryProperty.Name, ListSortDirection.Ascending));
                _dataView.SortDescriptions.Add(new SortDescription(PropertyItem.DisplayNameProperty.Name, ListSortDirection.Ascending));
                _dataView.GroupDescriptions.Add(new PropertyGroupDescription(PropertyItem.CategoryProperty.Name));
            }
        }

        private void SortByName(object sender, ExecutedRoutedEventArgs e)
        {
            if (_dataView == null) return;

            using (_dataView.DeferRefresh())
            {
                _dataView.GroupDescriptions.Clear();
                _dataView.SortDescriptions.Clear();
                if (SortByPriority)
                {
                    _dataView.SortDescriptions.Add(new SortDescription(PropertyItem.PriorityProperty.Name, ListSortDirection.Descending));
                }
                _dataView.SortDescriptions.Add(new SortDescription(PropertyItem.PropertyNameProperty.Name, ListSortDirection.Ascending));
            }
        }

        private void SortByHierarchyLevel(object sender, ExecutedRoutedEventArgs e)
        {
            if (_dataView == null) return;

            using (_dataView.DeferRefresh())
            {
                _dataView.GroupDescriptions.Clear();
                _dataView.SortDescriptions.Clear();
                if (SortByPriority)
                {
                    _dataView.SortDescriptions.Add(new SortDescription(PropertyItem.PriorityProperty.Name, ListSortDirection.Descending));
                }
                _dataView.SortDescriptions.Add(new SortDescription(PropertyItem.HierarchyLevelProperty.Name, ListSortDirection.Ascending));
                _dataView.GroupDescriptions.Add(new PropertyGroupDescription(PropertyItem.CategoryProperty.Name));
            }
        }

        private void SearchBar_SearchStarted(object sender, FunctionEventArgs<string> e)
        {
            if (_dataView == null) return;

            _searchKey = e.Info.ToLower();
            if (string.IsNullOrEmpty(_searchKey))
            {
                foreach (UIElement item in _dataView)
                {
                    item.Show();
                }
            }
            else
            {
                foreach (PropertyItem item in _dataView)
                {
                    item.Show(item.PropertyName.ToLower().Contains(_searchKey) || item.DisplayName.ToLower().Contains(_searchKey));
                }
            }
        }

        protected virtual PropertyItem CreatePropertyItem(PropertyDescriptor propertyDescriptor, object component, string category, int hierarchyLevel) =>
            new PropertyItem
            {
                Category         = category ?? PropertyResolver.ResolveCategory(propertyDescriptor),
                DisplayName      = PropertyResolver.ResolveDisplayName(propertyDescriptor),
                Description      = PropertyResolver.ResolveDescription(propertyDescriptor),
                IsReadOnly       = PropertyResolver.ResolveIsReadOnly(propertyDescriptor),
                DefaultValue     = PropertyResolver.ResolveDefaultValue(propertyDescriptor),
                Editor           = PropertyResolver.ResolveEditor(propertyDescriptor),
                HierarchyLevel   = PropertyResolver.ResolveHierarchyLevel(propertyDescriptor) ?? hierarchyLevel,
                Priority         = PropertyResolver.ResolvePriority(propertyDescriptor),
                IsNecessary      = PropertyResolver.ResolveIsNecessary(propertyDescriptor),
                Value            = component,
                PropertyName     = propertyDescriptor.Name,
                PropertyType     = propertyDescriptor.PropertyType,
                PropertyTypeName = $"{propertyDescriptor.PropertyType.Namespace}.{propertyDescriptor.PropertyType.Name}"
            };

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            TitleElement.SetTitleWidth(this, new GridLength(Math.Max(MinTitleWidth, Math.Min(MaxTitleWidth, ActualWidth / 3))));
        }
    }
}
