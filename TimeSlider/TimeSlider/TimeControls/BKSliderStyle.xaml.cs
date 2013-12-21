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


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem control = (MenuItem)e.Source;
            ItemsPresenter IP = DependencyObjExtensions.FindVisualParent<ItemsPresenter>(control);
            GenericBookmark gb = (GenericBookmark)IP.DataContext;
            Button BB = ((ContextMenu)control.Parent).PlacementTarget as Button;
            BKSlider BK = DependencyObjExtensions.TryFindParent<BKSlider>(BB);
            RoutedPropertyChangedEventArgs<GenericBookmark> args = new RoutedPropertyChangedEventArgs<GenericBookmark>(null, gb, BKSlider.OnDeleteBookmarkEvent);
            BK.RaiseEvent(args);
        }
    }
}
