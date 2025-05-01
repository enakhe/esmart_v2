using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using System.Windows;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.RoomSetting
{
    /// <summary>
    /// Interaction logic for AddBuildingDialog.xaml
    /// </summary>
    public partial class AddBuildingDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        public AddBuildingDialog(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
            InitializeComponent();
        }

        private async void AddBuilding_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(txtBuildingName.Text, txtBuildingNumber.Text);
                if (isNull)
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtBuildingNumber.Text, out int buildingNumber))
                {
                    MessageBox.Show("Please enter a valid building number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var buildingName = txtBuildingName.Text;
                var buildingNo = txtBuildingNumber.Text;

                var building = new Domain.Entities.RoomSettings.Building
                {
                    Name = buildingName,
                    Number = buildingNo
                };

                await _roomRepository.AddBuilding(building);

                MessageBox.Show("Building added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void NumberOnly_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                string clipboard = Clipboard.GetText();
                if (!clipboard.All(char.IsDigit))
                {
                    e.Handled = true;
                }
            }
        }
    }
}
