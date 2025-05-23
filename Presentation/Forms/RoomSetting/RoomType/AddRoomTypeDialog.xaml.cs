using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ESMART.Presentation.Forms.RoomSetting.RoomType
{
    /// <summary>
    /// Interaction logic for AddRoomTypeDialog.xaml
    /// </summary>
    public partial class AddRoomTypeDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        private bool _suppressTextChanged = false;
        private DispatcherTimer _formatTimer;

        public AddRoomTypeDialog(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
            InitializeComponent();

            _formatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _formatTimer.Tick += FormatTimer_Tick;

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

        private void DecimalInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressTextChanged) return;

            _formatTimer.Stop(); // restart timer
            _formatTimer.Tag = sender;
            _formatTimer.Start();
        }

        private void FormatTimer_Tick(object sender, EventArgs e)
        {
            _formatTimer.Stop();

            var textBox = _formatTimer.Tag as TextBox;
            if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text)) return;

            int caretIndex = textBox.CaretIndex;
            string unformatted = textBox.Text.Replace(",", "");

            if (decimal.TryParse(unformatted, out decimal value))
            {
                _suppressTextChanged = true;

                textBox.Text = string.Format(CultureInfo.InvariantCulture, "{0:N}", value);
                textBox.CaretIndex = Math.Min(caretIndex, textBox.Text.Length);

                _suppressTextChanged = false;
            }
        }

    }
}
