using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using HandyControl.Controls;

namespace HandyControlDemo.Data
{
    public class PropertyGridDemoModel
    {
        [Category("Category1")]
        public string String { get; set; }

        [Category("Category2")]
        public int Integer { get; set; }

        [Category("Category1"), Priority(3)]
        public bool Boolean { get; set; }

        [Category("Category1"), Priority(5), Necessary]
        public Gender Enum { get; set; }

        [Category("Category1"), Priority(4)]
        public DemoDataModel FlattenedType { get; set; }

        public HorizontalAlignment HorizontalAlignment { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        public ImageSource ImageSource { get; set; }
    }

    public enum Gender
    {
        Male,
        Female
    }
}
