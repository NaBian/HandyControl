﻿using System.Windows;

namespace HandyControlDemo.Views
{
    public partial class SplitButtonDemoCtl
    {
        public SplitButtonDemoCtl()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("123");
        }
    }
}
