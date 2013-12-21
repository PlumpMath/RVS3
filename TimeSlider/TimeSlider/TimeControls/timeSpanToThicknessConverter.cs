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
            if (values[0] != null && values[1] != null && values[2] != null && (double)values[1]!=0)
            {
                double _time = (double)values[0];
                double _totaltime = (double)values[1];
                double _width = (double)values[2]-10;
                double _val = (_time / _totaltime * _width)-4;
                result = new Thickness(_val, 0, 0, 8);
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
