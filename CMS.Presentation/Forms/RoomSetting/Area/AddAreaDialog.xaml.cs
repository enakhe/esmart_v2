using ESMART.Application.Common.Utils;
using ESMART.Application.Interface;
using System.Windows;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.RoomSetting.Area
{
    /// <summary>
    /// Interaction logic for AddAreaDialog.xaml
    /// </summary>
    public partial class AddAreaDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        public AddAreaDialog(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
            InitializeComponent();
        }

        private async void AddArea_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(txtAreaName.Text, txtAreaNumber.Text);

                if (isNull)
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtAreaNumber.Text, out int areaNumber))
                {
                    MessageBox.Show("Please enter a valid area number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoaderOverlay.Visibility = Visibility.Visible;

                var areaName = txtAreaName.Text;
                var areaNo = txtAreaNumber.Text;

                var area = new Domain.Entities.RoomSettings.Area
                {
                    Name = areaName,
                    Number = areaNo
                };

                await _roomRepository.AddArea(area);

                MessageBox.Show("Area added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }
    }
}
