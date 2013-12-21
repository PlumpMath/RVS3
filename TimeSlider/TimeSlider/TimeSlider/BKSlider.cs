using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ExtendedCollections;

namespace RobControls
{
    public class BKSlider:Slider
    {
        # region Bookmarks dependency definition
        public static readonly DependencyProperty BookmarksProperty = DependencyProperty.Register("Bookmarks", typeof(SortableObservableCollection<GenericBookmark>), typeof(BKSlider), new FrameworkPropertyMetadata(default(SortableObservableCollection<GenericBookmark>)));
        public SortableObservableCollection<GenericBookmark> Bookmarks
        {
            get
            {
                return (SortableObservableCollection<GenericBookmark>)GetValue(BookmarksProperty);
            }
            set
            {
                SetValue(BookmarksProperty, value);
            }
        }
        # endregion
        # region Length dependency definition
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(long), typeof(BKSlider), new FrameworkPropertyMetadata(default(long)));
        public long Length
        {
            get
            {
                return (long)GetValue(LengthProperty);
            }
            set
            {
                SetValue(LengthProperty, value);
            }
        }
        # endregion
        # region currentTime dependency definition
        public static readonly DependencyProperty currentTimeProperty = DependencyProperty.Register("currentTime", typeof(long), typeof(BKSlider), new FrameworkPropertyMetadata(default(long)));
        public long currentTime
        {
            get
            {
                return (long)GetValue(currentTimeProperty);
            }
            set
            {
                SetValue(currentTimeProperty, value);
            }
        }
        # endregion

        public BKSlider() : base()
        {
        }
        public static readonly RoutedEvent onBookmarkClickedEvent = EventManager.RegisterRoutedEvent("onBookmarkClicked", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<GenericBookmark>), typeof(BKSlider));
        public event RoutedPropertyChangedEventHandler<GenericBookmark> onBookmarkClicked
        {
            add { AddHandler(onBookmarkClickedEvent, value); }
            remove { RemoveHandler(onBookmarkClickedEvent, value); }
        }

        private void ButtonOnClick(object sender, RoutedEventArgs args)
        {
            Button Control = (Button)sender;
            ItemsControl LVX = DependencyObjExtensions.FindVisualParent<ItemsControl>(Control);
            //RoutedPropertyChangedEventArgs<BOOKMARK> arguments = new RoutedPropertyChangedEventArgs<int>((int)e.OldValue, (int)e.NewValue, OnVolumeChangedEvent);
        }
    }
}
