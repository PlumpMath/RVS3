using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ExtendedCollections;

namespace RobControls
{
    public partial class BKSliderStyle
    {
        private void ButtonOnClick(object sender, RoutedEventArgs args)
        {
            Button Control = (Button)sender;
            ItemsControl LVX = DependencyObjExtensions.FindVisualParent<ItemsControl>(Control);
            //RoutedPropertyChangedEventArgs<BOOKMARK> arguments = new RoutedPropertyChangedEventArgs<int>((int)e.OldValue, (int)e.NewValue, OnVolumeChangedEvent);
        }
    }
}
