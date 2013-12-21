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

namespace RobControls
{
    /// <summary>
    /// Logique d'interaction pour TimeDisplay.xaml
    /// </summary>
    public partial class TimeDisplay : UserControl
    {
        # region Length dependency definition
        public static readonly DependencyProperty lengthProperty = DependencyProperty.Register("length", typeof(double), typeof(TimeDisplay), new FrameworkPropertyMetadata(default(double)));
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
        public static readonly DependencyProperty currentTimeProperty = DependencyProperty.Register("currentTime", typeof(double), typeof(TimeDisplay), new FrameworkPropertyMetadata(default(double)));
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
        public TimeDisplay()
        {
            InitializeComponent();
        }
    }
}
