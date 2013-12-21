using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ExtendedCollections;

namespace RVS3
{
    public class RECENTFILE :DependencyObject
    {
        # region movieFilename dependency definition
        public static readonly DependencyProperty movieFilenameProperty = DependencyProperty.Register("movieFilename", typeof(string), typeof(RECENTFILE), new FrameworkPropertyMetadata(default(string)));
        public string movieFilename
        {
            get
            {
                return (string)GetValue(movieFilenameProperty);
            }
            set
            {
                SetValue(movieFilenameProperty, value);
            }
        }
        # endregion
        # region movieFullFilename dependency definition
        public static readonly DependencyProperty movieFullFilenameProperty = DependencyProperty.Register("movieFullFilename", typeof(string), typeof(RECENTFILE), new FrameworkPropertyMetadata(default(string)));
        public string movieFullFilename
        {
            get
            {
                return (string)GetValue(movieFullFilenameProperty);
            }
            set
            {
                SetValue(movieFullFilenameProperty, value);
            }
        }
        # endregion

        public RECENTFILE(string f, string ff)
        {
            movieFilename = f;
            movieFullFilename = ff;
        }
    }
}
