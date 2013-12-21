using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ExtendedCollections;


namespace RobControls
{
    public class GenericBookmark : DependencyObject
    {
        # region bookmarkTime dependency definition
        public static readonly DependencyProperty bookmarkTimeProperty = DependencyProperty.Register("bookmarkTime", typeof(double), typeof(GenericBookmark), new FrameworkPropertyMetadata(default(double)));
        public double bookmarkTime
        {
            get
            {
                return (double)GetValue(bookmarkTimeProperty);
            }
            set
            {
                SetValue(bookmarkTimeProperty, value);
            }
        }
        # endregion

        public GenericBookmark(double _time)
        {
            bookmarkTime = _time;
        }
    }
}
