using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ESMART.Presentation.Forms.FrontDesk.Reservation
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Colors.Gray);

            string status = value.ToString()!;
            switch (status)
            {
                case "Active":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EAD16")); // Light Green
                case "Inactive":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000")); // Light Red
                default:
                    return new SolidColorBrush(Colors.LightGray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
