using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    public class TransactionTypeColorConversion : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is not TransactionType status)
                return new SolidColorBrush(Colors.Gray);

            return status switch
            {
                //TransactionType.Credit => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EAD16")),
                //TransactionType.Debit => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000")),
                TransactionType.Adjustment => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a237e")),
                TransactionType.Refund => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fbbc04")),
                _ => new SolidColorBrush(Colors.LightGray),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
