using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ESMART.Presentation.Forms.Reports
{
    public class OverStayedBookingColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Colors.Gray);

            bool status = (bool)value;
            return status switch
            {
                true => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000")),// Light Red
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111827")),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
