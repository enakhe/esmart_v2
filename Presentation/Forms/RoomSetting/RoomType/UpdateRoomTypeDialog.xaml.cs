using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.RoomSetting.RoomType
{
    /// <summary>
    /// Interaction logic for UpdateRoomTypeDialog.xaml
    /// </summary>
    public partial class UpdateRoomTypeDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        private readonly Domain.Entities.RoomSettings.RoomType _roomType;
        private bool _suppressTextChanged = false;

        public UpdateRoomTypeDialog(IRoomRepository roomRepository, Domain.Entities.RoomSettings.RoomType roomType)
        {
            _roomRepository = roomRepository;
            _roomType = roomType;
            InitializeComponent();
        }

        private void LoadRoomType()
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;

                txtRoomTypeName.Text = _roomType.Name;
                txtRoomTypeRate.Text = _roomType.Rate?.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void UpdateRoomType_Click(object sender, RoutedEventArgs e)
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

                _roomType.Name = roomTypeName;
                _roomType.Rate = roomTypeRate;
                _roomType.DateModified = DateTime.Now;

                await _roomRepository.UpdateRoomType(_roomType);

                MessageBox.Show("Room type updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void Window_Activated(object sender, EventArgs e)
        {
            LoadRoomType();
        }
    }
}
