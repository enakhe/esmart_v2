using ESMART.Application.Common.Utils;
using ESMART.Application.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.RoomSetting.Floor
{
    public partial class AddFloorDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        public AddFloorDialog(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
            InitializeComponent();
        }

        public async Task LoadBuilding()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var buildings = await _roomRepository.GetAllBuildings();

                if (buildings == null || buildings.Count == 0)
                {
                    MessageBox.Show("No buildings found. Please add a building first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }

                cmbBuilding.ItemsSource = buildings;
                cmbBuilding.DisplayMemberPath = "Name";
                cmbBuilding.SelectedValuePath = "Id";
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

        private async void AddFloorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
                bool isNull = Helper.AreAnyNullOrEmpty(txtFloorName.Text, txtFloorNumber.Text, cmbBuilding.SelectedValue.ToString());
                if (isNull)
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtFloorNumber.Text, out int floorNumber))
                {
                    MessageBox.Show("Please enter a valid floor number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var floorName = txtFloorName.Text;
                var floorNo = txtFloorNumber.Text;
                var buildingId = cmbBuilding.SelectedValue.ToString();

                var floor = new Domain.Entities.RoomSettings.Floor
                {
                    Name = floorName,
                    Number = floorNo,
                    BuildingId = buildingId
                };

                var result = await _roomRepository.AddFloor(floor);
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

                MessageBox.Show("Floor added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadBuilding();
        }
    }
}
