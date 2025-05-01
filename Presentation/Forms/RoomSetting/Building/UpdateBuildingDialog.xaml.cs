using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.RoomSetting.Building
{
    /// <summary>
    /// Interaction logic for EditBuildingDialog.xaml
    /// </summary>
    public partial class EditBuildingDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        private readonly Domain.Entities.RoomSettings.Building _building;

        public EditBuildingDialog(IRoomRepository roomRepository, Domain.Entities.RoomSettings.Building building)
        {
            _roomRepository = roomRepository;
            _building = building;
            InitializeComponent();
        }

        private void LoadBuilding()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                txtBuildingName.Text = _building.Name;
                txtBuildingNumber.Text = _building.Number?.ToString();
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

        private async void UpdateBuilding_Click(object sender, RoutedEventArgs e)
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

                var result = await _roomRepository.GetBuildingById(_building.Id);
                if (!result.Succeeded)
                {
                    var sb = new StringBuilder();
                    foreach (var item in result.Errors)
                    {
                        sb.AppendLine(item);
                    }
                    MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var building = result.Response;

                if (building == null)
                {
                    MessageBox.Show("Building not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                building.Name = buildingName;
                building.Number = buildingNo;
                building.DateModified = DateTime.Now;

                await _roomRepository.UpdateBuilding(building);
                this.DialogResult = true;
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

        private void Window_Activated(object sender, EventArgs e)
        {
            LoadBuilding();
        }
    }
}
