using System.Globalization;
using System.Windows.Data;

namespace ESMART.Presentation.Forms.RoomSetting
{
    public class HeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double originalHeight = (double)value;
            double offset = 150;

            return Math.Max(0, originalHeight - offset);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
