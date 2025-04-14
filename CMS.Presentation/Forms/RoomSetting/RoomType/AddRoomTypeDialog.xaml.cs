using ESMART.Application.Common.Utils;
using ESMART.Application.Interface;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.RoomSetting.RoomType
{
    /// <summary>
    /// Interaction logic for AddRoomTypeDialog.xaml
    /// </summary>
    public partial class AddRoomTypeDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        private bool _suppressTextChanged = false;
        public AddRoomTypeDialog(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
            InitializeComponent();
        }

        private async void AddRoomType_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(txtRoomTypeName.Text, txtRoomTypeRate.Text);

                if (isNull)
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtRoomTypeRate.Text.Replace(",", ""), out decimal roomTypeRate))
                {
                    MessageBox.Show("Please enter a valid rate.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoaderOverlay.Visibility = Visibility.Visible;

                var roomTypeName = txtRoomTypeName.Text;
                var roomTypeRateText = txtRoomTypeRate.Text;

                var roomType = new Domain.Entities.RoomSettings.RoomType
                {
                    Name = roomTypeName,
                    Rate = roomTypeRate
                };

                await _roomRepository.AddRoomTypeAsync(roomType);
                MessageBox.Show("Room type added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        // Allow only numbers and one decimal point
        private void DecimalInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d.]");
        }

        // Prevent user from entering multiple decimal points
        private void DecimalInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressTextChanged) return;

            var textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox?.Text)) return;

            if (decimal.TryParse(textBox.Text.Replace(",", ""), out decimal value))
            {
                _suppressTextChanged = true;
                textBox.Text = string.Format(CultureInfo.InvariantCulture, "{0:N}", value);
                textBox.CaretIndex = textBox.Text.Length;
                _suppressTextChanged = false;
            }
        }

        // Allow navigation keys (backspace, delete, arrows)
        private void DecimalInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = !(e.Key >= Key.D0 && e.Key <= Key.D9 ||
                          e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 ||
                          e.Key == Key.Back || e.Key == Key.Delete ||
                          e.Key == Key.Left || e.Key == Key.Right ||
                          e.Key == Key.Decimal || e.Key == Key.OemPeriod);
        }
    }
}
