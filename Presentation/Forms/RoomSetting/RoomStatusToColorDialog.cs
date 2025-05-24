using ESMART.Domain.Entities.RoomSettings;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ESMART.Presentation.Forms.RoomSetting
{
    public class RoomStatusToColorDialog : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is RoomStatus status))
                return new SolidColorBrush(Colors.Gray);

            switch (status)
            {
                case RoomStatus.Vacant:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
                case RoomStatus.Maintenance:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                case RoomStatus.Booked:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a237e"));
                case RoomStatus.Reserved:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fbbc04"));
                case RoomStatus.Dirty:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7500"));
                case RoomStatus.Debit:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
                case RoomStatus.Credit:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C853"));
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
