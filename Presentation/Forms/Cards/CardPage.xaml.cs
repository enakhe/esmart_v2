using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Enum;
using ESMART.Presentation.Forms.Home;
using ESMART.Presentation.LockSDK;
using ESMART.Presentation.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
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
    /// Interaction logic for CardPage.xaml
    /// </summary>
    public partial class CardPage : Page
    {
        private readonly string computerName = Environment.MachineName;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly ICardRepository _cardRepository;
        private IServiceProvider _serviceProvider;

        public CardPage(IHotelSettingsService hotelSettingsService, ICardRepository cardRepository)
        {
            _hotelSettingsService = hotelSettingsService;
            _cardRepository = cardRepository;
            InitializeComponent();

            LoadMasterCard();
        }

        private void InitializeServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services, configuration);
            _serviceProvider = services.BuildServiceProvider();
        }

        private async void OpenPortButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var lockSetting = await _hotelSettingsService.GetSettingsByCategoryAsync("Operation Settings");

                if (lockSetting != null)
                {
                    var lockType = lockSetting.FirstOrDefault(x => x.Key == "LockType")?.Value;
                    if (lockType == "MIFI")
                    {
                        OpenPortFunc((int)LOCK_SETTING.LOCK_TYPE_MIFI);
                    }
                    else if (lockType == "RFID")
                    {
                        MessageBox.Show("RFID card issuing is not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (lockType == "PULMOS")
                    {
                        OpenPortFunc((int)LOCK_SETTING.LOCK_TYPE_PULMOS);
                    }
                }
                else
                {
                    MessageBox.Show("Lock settings not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async void ReadAuthCardButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var lockSetting = await _hotelSettingsService.GetSettingsByCategoryAsync("Operation Settings");

                if (lockSetting != null)
                {
                    var lockType = lockSetting.FirstOrDefault(x => x.Key == "LockType")?.Value;
                    if (lockType == "MIFI")
                    {
                        await ReadAuthCardFunc();
                    }
                    else if (lockType == "RFID")
                    {
                        MessageBox.Show("RFID card issuing is not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (lockType == "PULMOS")
                    {
                        await ReadAuthCardFunc();
                    }
                }
                else
                {
                    MessageBox.Show("Lock settings not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async void UnlockButtonsButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var lockSetting = await _hotelSettingsService.GetSettingsByCategoryAsync("Operation Settings");

                if (lockSetting != null)
                {
                    var lockType = lockSetting.FirstOrDefault(x => x.Key == "LockType")?.Value;
                    if (lockType == "MIFI")
                    {
                        UnlockButtonsFunc();
                    }
                    else if (lockType == "RFID")
                    {
                        MessageBox.Show("RFID card issuing is not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (lockType == "PULMOS")
                    {
                        UnlockButtonsFunc();
                    }
                }
                else
                {
                    MessageBox.Show("Lock settings not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async void RecycleCardButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var lockSetting = await _hotelSettingsService.GetSettingsByCategoryAsync("Operation Settings");

                if (lockSetting != null)
                {
                    var lockType = lockSetting.FirstOrDefault(x => x.Key == "LockType")?.Value;
                    if (lockType == "MIFI")
                    {
                        RecycleCardFunc();
                    }
                    else if (lockType == "RFID")
                    {
                        MessageBox.Show("RFID card issuing is not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (lockType == "PULMOS")
                    {
                        RecycleCardFunc();
                    }
                }
                else
                {
                    MessageBox.Show("Lock settings not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            AuthorizationCard AuthCard = await _cardRepository.GetAuthorizationCardByComputerName(computerName);
            if (AuthCard != null)
            {
                return new StringBuilder(AuthCard.AuthId);
            }

            return new StringBuilder("");
        }

        private void OpenPortFunc(int port)
        {
            int openPort = LockSDKMethods.OpenPort(port);
            if (openPort == 1)
            {
                AuthCardBtn.IsEnabled = true;
            }
            else
            {
                LockSDKMethods.CheckErr(openPort);
            }
        }

        private async Task ReadAuthCardFunc()
        {
            char[] card_snr = new char[20];
            string fnp = "1011899778569788";
            StringBuilder clientData = new StringBuilder(100);

            int clientDataResult = LockSDKMethods.ReadAuthCard(fnp, clientData, card_snr);
            if (clientDataResult != 1)
            {
                LockSDKMethods.CheckErr(clientDataResult);
            }
            else
            {
                UnlockButtons.IsEnabled = true;
                AUTHDATA.Text = clientData.ToString();

                bool isNull = Helper.AreAnyNullOrEmpty(clientData.ToString(), computerName);
                if (isNull == false)
                {
                    var authorizationCard = new AuthorizationCard()
                    {
                        AuthId = clientData.ToString(),
                        ComputerName = computerName,
                        DateAdded = DateTime.Now,
                        DateUpdated = DateTime.Now
                    };
                    await _cardRepository.AddAuthCard(authorizationCard);
                    MessageBox.Show("Successfully saved auth card", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Unable to save auth card to database, loaded auth card", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void UnlockButtonsFunc()
        {
            string fnp = "1011899778569788";
            StringBuilder clientData = new StringBuilder(AUTHDATA.Text);

            int systemIni = LockSDKMethods.SystemInitialization(fnp, clientData);
            if (systemIni != 1)
            {
                LockSDKMethods.CheckErr(systemIni);
                return;
            }
            else
            {
                btnMasterCard.IsEnabled = true;
                btnRecycle.IsEnabled = true;
                btnBuildingCard.IsEnabled = true;
                btnFloorCard.IsEnabled = true;
            }
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


        private async void DefaultSettingLoad(LOCK_SETTING lockTypeSetting)
        {
            Int16 locktype = (short)lockTypeSetting;
            AUTHDATA.IsEnabled = false;

            int checkEncoder = LockSDKMethods.CheckEncoder(locktype);
            if (checkEncoder != 1)
            {
                OpenPort.IsEnabled = false;
                LockSDKMethods.CheckErr(checkEncoder);
            }

            StringBuilder authId = await GetAuthCardFromDB();
            if (authId != null)
            {
                bool isNull = Helper.AreAnyNullOrEmpty(authId.ToString());
                if (isNull == false)
                {
                    AUTHDATA.Text = authId.ToString();
                    UnlockButtons.IsEnabled = true;
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var lockSetting = await _hotelSettingsService.GetSettingsByCategoryAsync("Operation Settings");

                if (lockSetting != null)
                {
                    var lockType = lockSetting.FirstOrDefault(x => x.Key == "LockType")?.Value;
                    if (lockType == "MIFI")
                    {
                        DefaultSettingLoad(LOCK_SETTING.LOCK_TYPE_MIFI);
                    }
                    else if (lockType == "RFID")
                    {
                        MessageBox.Show("RFID card issuing is not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (lockType == "PULMOS")
                    {
                        DefaultSettingLoad(LOCK_SETTING.LOCK_TYPE_PULMOS);
                    }
                }
                else
                {
                    MessageBox.Show("Lock settings not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void BuildingCardButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            BuildingCardPage buildingCardPage = _serviceProvider.GetRequiredService<BuildingCardPage>();

            MainFrame.Navigate(buildingCardPage);
        }

        private void FloorCardButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            FloorCardPage floorCardPage = _serviceProvider.GetRequiredService<FloorCardPage>();

            MainFrame.Navigate(floorCardPage);
        }

        private void MasterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMasterCard();
        }

        private void LoadMasterCard()
        {
            InitializeServices();

            MasterCardPage masterCardPage = _serviceProvider.GetRequiredService<MasterCardPage>();

            MainFrame.Navigate(masterCardPage);
        }
    }
}
