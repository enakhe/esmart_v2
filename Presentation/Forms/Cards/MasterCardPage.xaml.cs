using ESMART.Domain.Enum;
using ESMART.Presentation.LockSDK;
using ESMART.Presentation.Session;
using OfficeOpenXml;
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
    /// Interaction logic for MasterCardPage.xaml
    /// </summary>
    public partial class MasterCardPage : Page
    {
        public MasterCardPage()
        {
            InitializeComponent();
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var st = 0;

                char[] card_snr = new char[1000];

                string validTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string endTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
                int cardFlg = 0;

                CARD_INFO cardInfo = new CARD_INFO();

                cardFlg += !(bool)chkPassageMode.IsChecked! ? 0 : 2;
                cardFlg += !(bool)chkDeadLocks.IsChecked! ? 1 : 0;
                cardFlg += !(bool)chkCancelOldCards.IsChecked! ? 4 : 0;

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

                st = LockSDKMethods.MakeMasterCard(card_snr, validTime, endTime, CARD_FLAGS.CF_CHECK_TIMESTAMP, 0);

                if (st == (int)ERROR_TYPE.OPR_OK)
                {
                    MessageBox.Show("Successfully issued master card", "", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int checkEncoder = CheckEncoder(LOCK_SETTING.LOCK_TYPE_PULMOS);
            if (checkEncoder != 1)
            {
                LockSDKMethods.CheckErr(checkEncoder);
            }
        }
    }
}
