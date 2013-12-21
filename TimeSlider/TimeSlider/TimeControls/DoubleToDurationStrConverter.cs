using System;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections.Generic;
using System.Windows.Markup;

namespace RobControls
{
    public class DoubleToDurationStrConverter : MarkupExtension, IValueConverter
    {
        private static DoubleToDurationStrConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new DoubleToDurationStrConverter();
            }
            return _converter;
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = "";
            

            if (value != null && (targetType == typeof(string)))
            {
                double val = (double)value;
                TimeSpan time = TimeSpan.FromMinutes(val);
                string formattedTimeSpan = string.Format("{0:D1}h: {1:D2}m: {2:D2}s", time.Hours, time.Minutes, time.Seconds);

                result = formattedTimeSpan;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan? result = null;
            
            if (value != null && (targetType == typeof(TimeSpan) ))
            {
                Double val = (Double)value;
                result = TimeSpan.FromMinutes(val);
            }

            return result;
        }
    }
}