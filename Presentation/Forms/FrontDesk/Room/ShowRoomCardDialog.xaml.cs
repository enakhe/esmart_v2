using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.Enum;
using ESMART.Presentation.LockSDK;
using ESMART.Presentation.Session;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
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

namespace ESMART.Presentation.Forms.FrontDesk.Room
{
    /// <summary>
    /// Interaction logic for ShowRoomCardDialog.xaml
    /// </summary>
    public partial class ShowRoomCardDialog : Window
    {
        string computerName = Environment.MachineName;
        private readonly ICardRepository _cardRepository;
        private readonly Domain.Entities.RoomSettings.Room _room;
        public ShowRoomCardDialog(ICardRepository cardRepository, Domain.Entities.RoomSettings.Room room)
        {
            _cardRepository = cardRepository;
            _room = room;
            InitializeComponent();
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

        private void LoadCardDetails()
        {
            char[] card_snr = new char[100];

            int st = LockSDKMethods.ReadCard(card_snr);
            if (st != (int)ERROR_TYPE.OPR_OK)
            {

                txtCardNo.Text = "";
                txtLockNo.Text = "";
                txtRoom.Text = "";
            }
            else
            {

                CARD_INFO cardInfo = new CARD_INFO();
                byte[] cbuf = new byte[10000];
                cardInfo = new CARD_INFO();
                int result = LockSDKHeaders.LS_GetCardInformation(ref cardInfo, 0, 0, IntPtr.Zero);

                if (result == (int)ERROR_TYPE.OPR_OK)
                {
                    var roomNo = Helper.ByteArrayToString(cardInfo.RoomList);
                    bool isNull = Helper.AreAnyNullOrEmpty(roomNo);

                    if (isNull)
                    {
                        txtCardNo.Text = "";
                        txtLockNo.Text = "";
                        txtRoom.Text = "";
                    }
                    else
                    {
                        MakeCardType cardType = Helper.GetCardType(cardInfo.CardType);

                        txtRoom.Text = _room.Number;
                        txtCardNo.Text = Helper.ByteArrayToString(cardInfo.CardNo);
                        txtLockNo.Text = $"1.1.{_room.Number}";
                    }
                }
            }
        }

        private void IssueButton_Click(object sender, EventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                char[] card_snr = new char[100];
                var st = 0;
                string roomno = $"{_room.Building.Number}.{_room.Floor.Number}.{_room.Number}";

                string intime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                String outtime = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
                CARD_FLAGS iflags = CARD_FLAGS.CF_OPEN_ONCE;

                if (LockSDKHeaders.PreparedIssue(card_snr) == false)
                    return;
                st = LockSDKMethods.MakeGuestCard(card_snr, roomno, "", "", intime, outtime, iflags);

                if (st == (int)ERROR_TYPE.OPR_OK)
                {
                    MessageBox.Show("Successfully issued card", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadCardDetails();
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

        private void RecycleCard_Click(object sender, EventArgs e)
        {
            RecycleCardFunc();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

        private void Window_Activated(object sender, EventArgs e)
        {
            LoadCardDetails();
        }
    }
}
