using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Presentation.Session;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.RoomSetting.Room
{
    /// <summary>
    /// Interaction logic for AddRoomDialog.xaml
    /// </summary>
    public partial class AddRoomDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        private bool _suppressTextChanged = false;

        public AddRoomDialog(IRoomRepository roomRepository)
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

                if (buildings != null)
                {
                    cmbBuilding.ItemsSource = buildings;
                    cmbBuilding.DisplayMemberPath = "Name";
                    cmbBuilding.SelectedValuePath = "Id";
                }


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

        public async Task LoadFloors()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                bool isNull = cmbBuilding.SelectedItem == null || Helper.AreAnyNullOrEmpty(cmbBuilding.SelectedItem.ToString()!);

                if (!isNull)
                {
                    var floors = await _roomRepository.GetFloorsByBuilding(cmbBuilding.SelectedValue.ToString()!);
                    if (floors != null)
                    {
                        cmbFloor.ItemsSource = floors;
                        cmbFloor.DisplayMemberPath = "Number";
                        cmbFloor.SelectedValuePath = "Id";
                    }
                }
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

        public async Task LoadArea()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var areas = await _roomRepository.GetAllAreas();

                if (areas != null)
                {
                    cmbArea.ItemsSource = areas;
                    cmbArea.DisplayMemberPath = "Name";
                    cmbArea.SelectedValuePath = "Id";
                }


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

        public async Task LoadRoomType()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var roomTypes = await _roomRepository.GetAllRoomTypes();
                if (roomTypes != null)
                {
                    cmbRoomType.ItemsSource = roomTypes;
                    cmbRoomType.DisplayMemberPath = "Name";
                    cmbRoomType.SelectedValuePath = "Id";
                }

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

        private async void AddRoomButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
                bool isNull = Helper.AreAnyNullOrEmpty(txtRoomNumber.Text, txtRoomRate.Text, cmbBuilding.SelectedValue.ToString()!, cmbFloor.SelectedValue.ToString()!, cmbArea.SelectedValue.ToString()!, cmbRoomType.SelectedValue.ToString()!);

                if (isNull)
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtRoomRate.Text.Replace(",", ""), out decimal roomRate))
                {
                    MessageBox.Show("Please enter a valid rate.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var foundRoom = await _roomRepository.GetRoomByNumber(txtRoomNumber.Text);

                if (foundRoom != null)
                {
                    MessageBox.Show("A room already exists with that number.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var roomNumber = txtRoomNumber.Text;
                var buildingId = cmbBuilding.SelectedValue.ToString();
                var floorId = cmbFloor.SelectedValue.ToString();
                var areaId = cmbArea.SelectedValue.ToString();
                var roomTypeId = cmbRoomType.SelectedValue.ToString();

                var room = new Domain.Entities.RoomSettings.Room
                {
                    Number = roomNumber,
                    Rate = roomRate,
                    BuildingId = buildingId,
                    FloorId = floorId,
                    AreaId = areaId,
                    RoomTypeId = roomTypeId,
                    CreatedBy = AuthSession.CurrentUser?.Id,
                    Status = Domain.Entities.RoomSettings.RoomStatus.Vacant,
                    UpdatedBy = AuthSession.CurrentUser?.Id,
                };

                await _roomRepository.AddRoom(room);
                MessageBox.Show("Room added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
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
            await LoadArea();
            await LoadRoomType();
        }

        private async void cmbBuilding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadFloors();
        }

        private async void cmbRoomType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isNull = cmbRoomType.SelectedItem == null || Helper.AreAnyNullOrEmpty(cmbRoomType.SelectedValue.ToString()!);
            if (!isNull)
            {
                var result = await _roomRepository.GetRoomTypeById(cmbRoomType.SelectedValue.ToString()!);

                if (result == null)
                {
                    MessageBox.Show("Room type not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

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
                    MessageBox.Show("Room type not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var roomType = result.Response;
                txtRoomRate.Text = roomType.Rate.ToString();
            }
        }

        private void EditButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtRoomRate.IsEnabled = true;
        }
    }
}
