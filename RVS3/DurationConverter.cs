using System;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections.Generic;
using System.Windows.Markup;

namespace RVS3
{
    public class DurationConverter : MarkupExtension, IValueConverter
    {
        private static DurationConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new DurationConverter();
            }
            return _converter;
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Double? result =0.0d;
            TimeSpan? val = value as TimeSpan?;

            if (value != null && (targetType == typeof(Int32) || targetType == typeof(double)) )
            {
                
                result = val.Value.TotalMinutes;
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