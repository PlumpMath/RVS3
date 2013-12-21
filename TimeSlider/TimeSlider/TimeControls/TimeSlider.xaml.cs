using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtendedCollections;
namespace RobControls
{
	/// <summary>
	/// Interaction logic for MainControl.xaml
	/// </summary>
	public partial class TimeSlider
	{
        # region bookmarks dependency definition
        public static readonly DependencyProperty bookmarksProperty = DependencyProperty.Register("bookmarks", typeof(SortableObservableCollection<GenericBookmark>), typeof(TimeSlider), new FrameworkPropertyMetadata(default(SortableObservableCollection<GenericBookmark>)));
        public SortableObservableCollection<GenericBookmark> bookmarks
        {
            get
            {
                return (SortableObservableCollection<GenericBookmark>)GetValue(bookmarksProperty);
            }
            set
            {
                SetValue(bookmarksProperty, value);
            }
        }
        # endregion
        # region Length dependency definition
        public static readonly DependencyProperty lengthProperty = DependencyProperty.Register("length", typeof(double), typeof(TimeSlider), new FrameworkPropertyMetadata(default(double)));
        public double length
        {
            get
            {
                return (double)GetValue(lengthProperty);
            }
            set
            {
                SetValue(lengthProperty, value);
            }
        }
        # endregion
        # region currentTime dependency definition
        public static readonly DependencyProperty currentTimeProperty = DependencyProperty.Register("currentTime", typeof(double), typeof(TimeSlider), new FrameworkPropertyMetadata(default(double)));
        public double currentTime
        {
            get
            {
                return (double)GetValue(currentTimeProperty);
            }
            set
            {
                SetValue(currentTimeProperty, value);
            }
        }
        # endregion
		public TimeSlider()
		{
			this.InitializeComponent();
		}
        public static readonly RoutedEvent OnValueChangedEvent = EventManager.RegisterRoutedEvent("OnValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(TimeSlider));
        public event RoutedPropertyChangedEventHandler<double> OnValueChanged
        {
            add { AddHandler(OnValueChangedEvent, value); }
            remove { RemoveHandler(OnValueChangedEvent, value); }
        }

        private void BKSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RoutedPropertyChangedEventArgs<double> args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, OnValueChangedEvent);
        }

        public static readonly RoutedEvent OnDeleteBookmarkEvent = EventManager.RegisterRoutedEvent("OnDeleteBookmark", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<GenericBookmark>), typeof(TimeSlider));
        public event RoutedPropertyChangedEventHandler<GenericBookmark> OnDeleteBookmark
        {
            add { AddHandler(OnDeleteBookmarkEvent, value); }
            remove { RemoveHandler(OnDeleteBookmarkEvent, value); }
        }

        private void BKSub_OnDeleteBookmark(object sender, RoutedPropertyChangedEventArgs<GenericBookmark> e)
        {
            RoutedPropertyChangedEventArgs<GenericBookmark> args = new RoutedPropertyChangedEventArgs<GenericBookmark>(null, e.NewValue, OnDeleteBookmarkEvent);
            RaiseEvent(args);
        }
	}
}