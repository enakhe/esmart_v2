using ESMART.Application.Common.Utils;
using ESMART.Application.Interface;
using ESMART.Domain.Entities.RoomSettings;
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
    /// Interaction logic for UpdateRoomDialog.xaml
    /// </summary>
    public partial class UpdateRoomDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        private readonly Domain.Entities.RoomSettings.Room _room;
        private bool _suppressTextChanged = false;

        public UpdateRoomDialog(IRoomRepository roomRepository, Domain.Entities.RoomSettings.Room room)
        {
            _roomRepository = roomRepository;
            _room = room;
            InitializeComponent();
        }

        private void LoadRoom()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                txtRoomNumber.Text = _room.Number;
                txtRoomRate.Text = _room.Rate.ToString();

                cmbStatus.SelectedValue = _room.Status;
                cmbBuilding.SelectedValue = _room.BuildingId;
                cmbFloor.SelectedValue = _room.FloorId;
                cmbArea.SelectedValue = _room.AreaId;
                cmbRoomType.SelectedValue = _room.RoomTypeId;
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

        public async Task LoadFloors()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(cmbBuilding.SelectedValue.ToString());

                if (!isNull)
                {
                    var floors = await _roomRepository.GetFloorsByBuilding(cmbBuilding.SelectedValue.ToString());
                    if (floors == null || floors.Count == 0)
                    {
                        MessageBox.Show("No floors found. Please add a floor first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
                    }

                    cmbFloor.ItemsSource = floors;
                    cmbFloor.DisplayMemberPath = "Number";
                    cmbFloor.SelectedValuePath = "Id";

                    cmbFloor.SelectedValue = _room.FloorId;
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

                if (areas == null || areas.Count == 0)
                {
                    MessageBox.Show("No areas found. Please add a building first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }

                cmbArea.ItemsSource = areas;
                cmbArea.DisplayMemberPath = "Name";
                cmbArea.SelectedValuePath = "Id";
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
                if (roomTypes == null || roomTypes.Count == 0)
                {
                    MessageBox.Show("No room types found. Please add a room type first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
                cmbRoomType.ItemsSource = roomTypes;
                cmbRoomType.DisplayMemberPath = "Name";
                cmbRoomType.SelectedValuePath = "Id";
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

        public void LoadStatus()
        {
            try
            {
                var status = Enum.GetValues(typeof(RoomStatus))
                    .Cast<RoomStatus>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbStatus.ItemsSource = status;
                cmbStatus.DisplayMemberPath = "Name";
                cmbStatus.SelectedValuePath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdateRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(txtRoomNumber.Text, txtRoomRate.Text,
                    cmbBuilding.SelectedValue.ToString(), cmbFloor.SelectedValue.ToString(),
                    cmbArea.SelectedValue.ToString(), cmbRoomType.SelectedValue.ToString(), cmbStatus.SelectedValue.ToString());

                if (isNull)
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(txtRoomNumber.Text, out int floorNumber))
                {
                    MessageBox.Show("Please enter a valid floor number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoaderOverlay.Visibility = Visibility.Visible;
                var roomNo = txtRoomNumber.Text.Trim();
                var roomRate = decimal.Parse(txtRoomRate.Text.Trim(), CultureInfo.InvariantCulture);
                var buildingId = cmbBuilding.SelectedValue.ToString();
                var floorId = cmbFloor.SelectedValue.ToString();
                var areaId = cmbArea.SelectedValue.ToString();
                var roomTypeId = cmbRoomType.SelectedValue.ToString();

                _room.Number = roomNo;
                _room.Rate = roomRate;
                _room.BuildingId = buildingId;
                _room.FloorId = floorId;
                _room.AreaId = areaId;
                _room.RoomTypeId = roomTypeId;
                _room.Status = (RoomStatus)Enum.Parse(typeof(RoomStatus), cmbStatus.SelectedValue.ToString());
                _room.DateModified = DateTime.Now;
                _room.UpdatedBy = AuthSession.CurrentUser?.Id;

                var result = await _roomRepository.UpdateRoom(_room);

                if (result == null)
                {
                    MessageBox.Show("Room not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

        private async void cmbBuilding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadFloors();
        }

        private async void cmbRoomType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(cmbRoomType.SelectedValue.ToString()!);
            if (!isNull)
            {
                var selectedId = cmbRoomType.SelectedValue.ToString();

                var result = await _roomRepository.GetRoomTypeById(selectedId!);

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

        private async void Window_Activated(object sender, EventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                await LoadBuilding();
                await LoadArea();
                await LoadRoomType();
                LoadStatus();
                LoadRoom();
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

        private void EditButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtRoomRate.IsEnabled = true;
        }
    }
}
