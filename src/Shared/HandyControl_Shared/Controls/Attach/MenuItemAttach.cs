using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HandyControl.Controls
{
    public class MenuItemAttach
    {
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.RegisterAttached("GroupName", typeof(string), typeof(MenuItemAttach),
                                                new PropertyMetadata(string.Empty, GroupNamePropertyChanged));

        [AttachedPropertyBrowsableForType(typeof(MenuItem))]
        public static string GetGroupName(DependencyObject obj)
        {
            return (string) obj.GetValue(GroupNameProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(MenuItem))]
        public static void SetGroupName(DependencyObject obj, string value)
        {
            obj.SetValue(GroupNameProperty, value);
        }

        private static void GroupNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MenuItem menuItem)
            {
                var newGroupName = e.NewValue.ToString();
                var oldGroupName = e.OldValue.ToString();

                if (string.IsNullOrWhiteSpace(newGroupName))
                {
                    menuItem.Checked -= MenuItem_Checked;
                    menuItem.Click -= MenuItem_Click;

                }
                else if (string.IsNullOrWhiteSpace(oldGroupName))
                {
                    // When the oldGroupName is null or white space,
                    // it means we had removed the Checked event handler or never add, so we need add one.
                    menuItem.Checked += MenuItem_Checked;

                    // The same to Checked event.
                    menuItem.Click += MenuItem_Click;
                }
            }
        }

        private static void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is MenuItem parent)
            {
                var groupName = GetGroupName(menuItem);

                foreach (var item in parent.Items.OfType<MenuItem>().Where(m => m != menuItem &&
                                                                                m.IsCheckable &&
                                                                                GetGroupName(m) == groupName))
                {
                    item.IsChecked = false;
                }
            }
        }

        private static void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // prevent uncheck when click the checked menu item
            if (e.OriginalSource is MenuItem menuItem && !menuItem.IsChecked)
            {
                menuItem.IsChecked = true;
            }
        }
    }
}
