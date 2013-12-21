using System;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Interop;
using System.IO;
using System.ComponentModel;

namespace RobControls
{

    public class timeSpanToThicknessConverter : MarkupExtension, IMultiValueConverter
    {


        private static timeSpanToThicknessConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new timeSpanToThicknessConverter();
            }
            return _converter;
        }
        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Thickness result = new Thickness();
            if (values[0] != null && values[1] != null && values[2] != null && values[3] !=null)
            {
                TimeSpan _time = (TimeSpan)values[0];
                TimeSpan _totaltime = (TimeSpan)values[1];
                double _width = (double)values[2];
                PositionEnum _direction = (PositionEnum)values[3];
                List<Double> marge = new List<double>();
                double _val = _totaltime.TotalMilliseconds / _time.TotalMilliseconds * _width;
                foreach (PositionEnum _pos in Enum.GetValues(typeof(PositionEnum)))
                {
                    if ((_direction & _pos) == _pos)
                    {
                        marge.Add(_val);
                    }
                    else
                    {
                        marge.Add(0.0d);
                    }
                }
                result = new Thickness(marge[0], marge[1], marge[2], marge[3]); ;
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
