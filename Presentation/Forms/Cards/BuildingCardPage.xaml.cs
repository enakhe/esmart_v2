using ESMART.Application.Common.Interface;
using ESMART.Domain.Enum;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Presentation.LockSDK;
using ESMART.Presentation.Session;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.Cards
{
    /// <summary>
    /// Interaction logic for BuildingCardPage.xaml
    /// </summary>
    public partial class BuildingCardPage : Page
    {
        string computerName = Environment.MachineName;
        private readonly IRoomRepository _roomRepository;
        private readonly ICardRepository _cardRepository;
        public BuildingCardPage(IRoomRepository roomRepository, ICardRepository cardRepository)
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

        private static int OpenPort(int port)
        {
            var st = LockSDKHeaders.LS_OpenPort(port);
            return st;
        }

        public static int CheckEncoder(LOCK_SETTING lockSetting)
        {
            Int16 locktype = (short)lockSetting;
            var st = LockSDKHeaders.TP_Configuration(locktype);
            return st;
        }

        public async Task<StringBuilder> GetAuthCardFromDB()
        {
            var AuthCard = await _cardRepository.GetAuthorizationCardByComputerName(computerName);
            if (AuthCard != null)
            {
                return new StringBuilder(AuthCard.AuthId);
            }

            return new StringBuilder("");
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int checkEncoder = CheckEncoder(LOCK_SETTING.LOCK_TYPE_PULMOS);
            if (checkEncoder != 1)
            {
                LockSDKMethods.CheckErr(checkEncoder);
            }

            await LoadBuilding();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var st = 0;
                char[] card_snr = new char[1000];

                string validTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string endTime = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                int cardFlg = 0;

                if(cmbBuilding.SelectedValue == null)
                {
                    MessageBox.Show("Failed to issue card: please select a building", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                string buidlingId = cmbBuilding.SelectedValue?.ToString()!;
                var building = await _roomRepository.GetBuildingById(buidlingId);

                CARD_INFO cardInfo = new CARD_INFO();

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
                else
                {
                    byte[] cbuf = new byte[10000];
                    cardInfo = new CARD_INFO();
                    int result = LockSDKHeaders.LS_GetCardInformation(ref cardInfo, 0, 0, IntPtr.Zero);
                }

                st = LockSDKMethods.MakeBuildingCard(card_snr, building.Response?.Number!, validTime, endTime, 1, 0);

                if (st == (int)ERROR_TYPE.OPR_OK)
                {
                    MessageBox.Show("Successfully issued building card", "", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
