using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    public class ExpanderHeaderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string guestName = values[0]?.ToString()!;
            string room = values[1]?.ToString()!;
            string roomType = values[2]?.ToString()!;

            return $"{guestName} | {room} | {roomType}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
