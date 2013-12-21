using System;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

namespace RVS3
{
    public class AFilenameConverter : MarkupExtension, IValueConverter
    {


        private static AFilenameConverter _converter = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new AFilenameConverter();
            }
            return _converter;
        }
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string filename = value.ToString();
            string final_path = System.IO.Path.GetFileNameWithoutExtension(filename);

            return final_path;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
