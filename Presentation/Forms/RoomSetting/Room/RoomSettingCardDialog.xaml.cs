using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Enum;
using ESMART.Presentation.LockSDK;
using System.Text;
using System.Windows;

namespace ESMART.Presentation.Forms.RoomSetting.Room
{
    /// <summary>
    /// Interaction logic for RoomSettingCardDialog.xaml
    /// </summary>
    public partial class RoomSettingCardDialog : Window
    {
        string computerName = Environment.MachineName;
        private readonly IRoomRepository _roomRepository;
        private readonly Domain.Entities.RoomSettings.Room _room;
        private readonly ICardRepository _cardRepository;
        public RoomSettingCardDialog(IRoomRepository roomRepository, ICardRepository cardRepository, Domain.Entities.RoomSettings.Room room)
        {
            _room = room;
            _roomRepository = roomRepository;
            _cardRepository = cardRepository;
            InitializeComponent();
        }

        public async Task LoadAreas()
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

        public async Task<StringBuilder> GetAuthCardFromDB()
        {
            var AuthCard = await _cardRepository.GetAuthorizationCardByComputerName(computerName);
            if (AuthCard != null)
            {
                return new StringBuilder(AuthCard.AuthId);
            }

            return new StringBuilder("");
        }

        private static int OpenPort(LOCK_SETTING lockSetting)
        {
            Int16 locktype = (short)lockSetting;
            var st = LockSDKHeaders.LS_OpenPort(locktype);
            return st;
        }

        public static int CheckEncoder(LOCK_SETTING lockSetting)
        {
            Int16 locktype = (short)lockSetting;
            var st = LockSDKHeaders.TP_Configuration(locktype);
            return st;
        }

        private static void RecycleCardFunc()
        {
            StringBuilder card_snr = new StringBuilder();
            CARD_INFO cardInfo = new CARD_INFO();
            byte[] cbuf = new byte[10000];
            cardInfo = new CARD_INFO();
            int result = LockSDKHeaders.LS_GetCardInformation(ref cardInfo, 0, 0, IntPtr.Zero);

            var cardNo = Helper.ByteArrayToString(cardInfo.CardNo);

            int st = LockSDKHeaders.TP_CancelCard(card_snr);
            if (st == 1)
            {
                MessageBox.Show("Successfully recycled card", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                LockSDKMethods.CheckErr(st);
            }
        }

        private void RecycleCard_Click(object sender, EventArgs e)
        {
            RecycleCardFunc();
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadAreas();

            int checkEncoder = CheckEncoder(LOCK_SETTING.LOCK_TYPE_PULMOS);
            if (checkEncoder != 1)
            {
                LockSDKMethods.CheckErr(checkEncoder);
            }

            int openPort = OpenPort(LOCK_SETTING.LOCK_TYPE_PULMOS);
            if (openPort != 1)
            {
                LockSDKMethods.CheckErr(openPort);
            }

            StringBuilder authCard = await GetAuthCardFromDB();
            string fnp = "1011899778569788";
            StringBuilder clientData = authCard;

            int systemIni = LockSDKMethods.SystemInitialization(fnp, clientData);
            if (systemIni != 1)
            {
                LockSDKMethods.CheckErr(systemIni);
                return;
            }
        }

        private async void IssueButton_Click(object sender, EventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                StringBuilder card_snr = new StringBuilder(100);
                string roomno = $"{_room.Building.Number}.{_room.Floor.Number}.{_room.Number}";
                int buildingno = int.Parse(_room.Building.Number);
                int floorno = int.Parse(_room.Floor.Number);

                var areaId = cmbArea.SelectedValue.ToString();
                var area = await _roomRepository.GetAreaById(areaId!);
                int areano1 = int.Parse(area.Response?.Number.ToString()!);

                int areano2 = 0;
                ROOM_TYPE roomtype = ROOM_TYPE.RT_COMMON_ROOMS;
                LOCK_SETTING lockSetting = LOCK_SETTING.LS_ALL_REPLACE;
                int replaceNo = 0;
                var st = 0;

                st = LockSDKMethods.MakeRoomSettingsCard(card_snr, buildingno, floorno, roomno, roomtype, areano1, areano2, lockSetting, replaceNo);

                if (st == (int)ERROR_TYPE.OPR_OK)
                {
                    MessageBox.Show("Successfully issued card", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else if (st == (int)ERROR_TYPE.PORT_IN_USED)
                {
                    MessageBox.Show("Failed to issue card: Port is already in use.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Failed to issue card, error code: {st}", "", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
