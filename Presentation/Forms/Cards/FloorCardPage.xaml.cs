using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Enum;
using ESMART.Presentation.LockSDK;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.Cards
{
    /// <summary>
    /// Interaction logic for FloorCardPage.xaml
    /// </summary>
    public partial class FloorCardPage : Page
    {
        string computerName = Environment.MachineName;
        private readonly IRoomRepository _roomRepository;
        private readonly ICardRepository _cardRepository;

        public FloorCardPage(IRoomRepository roomRepository, ICardRepository cardRepository)
        {
            _roomRepository = roomRepository;
            _cardRepository = cardRepository;
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
                        listFloors.ItemsSource = floors.Select(f => f.Number);
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

        public string ConvertSelectedItemsToString(ListBox checkedListBox)
        {
            var selectedItems = checkedListBox.SelectedItems;
            List<string> codes = new List<string>();

            foreach (var item in selectedItems)
            {
                string itemText = item.ToString()!;
                int startIndex = itemText.IndexOf('(') + 1;
                int endIndex = itemText.IndexOf(')');

                if (startIndex > 0 && endIndex > startIndex)
                {
                    string code = itemText.Substring(startIndex, endIndex - startIndex);
                    codes.Add(code);
                }
            }

            string result = string.Join(".", codes);
            return result;
        }

        public string GetSelectedCodes(ListBox checkedListBox)
        {
            List<string> codes = new List<string>();
            foreach (var item in checkedListBox.SelectedItems)
            {
                codes.Add(item.ToString()!);
            }

            if (codes.Count > 0)
            {
                return string.Join(".", codes);
            }
            else
            {
                return string.Empty;
            }
        }

        public async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var st = 0;

                char[] card_snr = new char[1000];

                var building = await _roomRepository.GetBuildingById(cmbBuilding.SelectedValue.ToString()!);


                string validTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string endTime = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");

                int cardFlg = 0;
                int buildingList = int.Parse(building.Response?.Number!);
                string floorList = GetSelectedCodes(listFloors);

                cardFlg += (bool)chkPassageMode.IsChecked! ? 2 : 0;
                cardFlg += (bool)chkDeadLocks.IsChecked! ? 1 : 0;
                cardFlg += (bool)chkCancelOldCards.IsChecked! ? 4 : 0;

                st = LockSDKHeaders.LS_ReadRom(card_snr);
                if (LockSDKMethods.PreparedIssue(card_snr) == false)
                    return;

                st = LockSDKMethods.ReadCard(card_snr);

                if (st != (int)ERROR_TYPE.OPR_OK)
                {
                    LockSDKMethods.CheckErr(st);
                    return;
                }

                st = LockSDKMethods.MakeFloorCard(card_snr, buildingList, floorList, DateTime.Now.ToString("HH:mm"), DateTime.Now.AddMinutes(30).ToString("HH:mm"), validTime, endTime, CARD_FLAGS.CF_CHECK_TIMESTAMP, 0);

                if (st == (int)ERROR_TYPE.OPR_OK)
                {
                    MessageBox.Show("Successfully issued florr card", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (st == (int)ERROR_TYPE.PORT_IN_USED)
                {
                    MessageBox.Show("Failed to issue card: Port is already in use", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Failed to issue card, error code: {st}", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBuilding();
        }

        private async void cmbBuilding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadFloors();
        }
    }
}
