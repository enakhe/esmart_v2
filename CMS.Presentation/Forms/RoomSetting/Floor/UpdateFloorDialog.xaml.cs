using ESMART.Application.Common.Utils;
using ESMART.Application.Interface;
using ESMART.Domain.Entities.RoomSettings;
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
    /// <summary>
    /// Interaction logic for UpdateFloorDialog.xaml
    /// </summary>
    public partial class UpdateFloorDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        private readonly Domain.Entities.RoomSettings.Floor _floor;
        public UpdateFloorDialog(IRoomRepository roomRepository, Domain.Entities.RoomSettings.Floor floor)
        {
            _roomRepository = roomRepository;
            _floor = floor;
            InitializeComponent();
        }

        private void LoadFloor()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                txtFloorName.Text = _floor.Name;
                txtFloorNumber.Text = _floor.Number?.ToString();
                cmbBuilding.SelectedValue = _floor.BuildingId;
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

        private async void UpdateFloor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(txtFloorName.Text, txtFloorNumber.Text);
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
                
                LoaderOverlay.Visibility = Visibility.Visible;

                var floorName = txtFloorName.Text;
                var floorNo = txtFloorNumber.Text;
                var buildingId = cmbBuilding.SelectedValue; 

                var result = await _roomRepository.GetFloorById(_floor.Id);
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

                if (result.Response == null)
                {
                    MessageBox.Show("Floor not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var floor = result.Response;
                floor.Name = floorName;
                floor.Number = floorNo;
                floor.BuildingId = buildingId.ToString();
                floor.DateModified = DateTime.Now;

                var updateResult = await _roomRepository.UpdateFloor(floor);
                if (!updateResult.Succeeded)
                {
                    var sb = new StringBuilder();
                    foreach (var item in updateResult.Errors)
                    {
                        sb.AppendLine(item);
                    }
                    MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Floor updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            LoadFloor();
            await LoadBuilding();
        }
    }
}
