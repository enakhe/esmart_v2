using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.Enum;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Repositories.Transaction;
using ESMART.Infrastructure.Repositories.Verification;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.Verification;
using ESMART.Presentation.LockSDK;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
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

namespace ESMART.Presentation.Forms.Home
{
    /// <summary>
    /// Interaction logic for CreateCardDialog.xaml
    /// </summary>
    public partial class CreateCardDialog : Window
    {
        private readonly Domain.Entities.RoomSettings.Room _room;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;
        private readonly GuestAccountService _guestAccountService;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITransactionRepository _transactionRepository;


        public CreateCardDialog(Domain.Entities.RoomSettings.Room room, IHotelSettingsService hotelSettingsService, IVerificationCodeService verificationCodeService, IApplicationUserRoleRepository applicationUserRoleRepository, GuestAccountService guestAccountService, IBookingRepository bookingRepository, ITransactionRepository transactionRepository)
        {
            _room = room;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
            _verificationCodeService = verificationCodeService;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            _guestAccountService = guestAccountService;
            _bookingRepository = bookingRepository;
            _transactionRepository = transactionRepository;
        }

        private void LoadDefaultSetting()
        {
            txtRoom.Text = _room.Number;

            dtpCheckIn.DisplayDateStart = DateTime.Today;
            dtpCheckIn.SelectedDate = DateTime.Now;
            dtpCheckOut.SelectedDate = DateTime.Now.AddDays(1);
        }

        public void LoadPaymentMethod()
        {
            try
            {
                var method = Enum.GetValues<PaymentMethod>()
                    .Cast<PaymentMethod>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbPaymentMethod.ItemsSource = method;
                cmbPaymentMethod.DisplayMemberPath = "Name";
                cmbPaymentMethod.SelectedValuePath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadBankAccount()
        {
            try
            {
                var accountNumber = await _transactionRepository.GetAllBankAccountAsync();

                cmbAccountNumber.ItemsSource = accountNumber;
                cmbAccountNumber.DisplayMemberPath = "BankAccountNumber";
                cmbAccountNumber.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dtpCheckOut_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpCheckOut.SelectedDate != null && dtpCheckIn.SelectedDate != null)
            {
                if (dtpCheckOut.SelectedDate <= dtpCheckIn.SelectedDate)
                {
                    dtpCheckOut.SelectedDate = dtpCheckIn.SelectedDate.Value.AddDays(1);
                }

                var days = (dtpCheckOut.SelectedDate.Value.Date - dtpCheckIn.SelectedDate.Value.Date).Days;
                txtDays.Text = days.ToString();
            }
        }

        private void dtpCheckIn_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpCheckIn.SelectedDate != null)
            {
                dtpCheckOut.DisplayDateStart = dtpCheckIn.SelectedDate.Value.AddDays(1);

                if (dtpCheckOut.SelectedDate == null || dtpCheckOut.SelectedDate <= dtpCheckIn.SelectedDate)
                {
                    dtpCheckOut.SelectedDate = dtpCheckIn.SelectedDate.Value.AddDays(1);
                }

                var days = (dtpCheckOut.SelectedDate.Value.Date - dtpCheckIn.SelectedDate.Value.Date).Days;
                txtDays.Text = days.ToString();
            }
        }

        private void txtDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((int.TryParse(txtDays.Text, out int days) || days > 1) && dtpCheckIn.SelectedDate != null)
            {
                var checkOut = dtpCheckIn.SelectedDate.Value.AddDays(int.Parse(txtDays.Text));
                if (checkOut > dtpCheckIn.SelectedDate)
                {
                    dtpCheckOut.SelectedDate = checkOut;
                }
            }
        }

        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
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

        private void RecycleCard_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void IssueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;

                bool isNull = Helper.AreAnyNullOrEmpty(txtGuestName.Text, txtRoom.Text, txtDays.Text);
                if (isNull && cmbPaymentMethod.SelectedValue == null && cmbAccountNumber.SelectedValue == null)
                {
                    MessageBox.Show("Please insert all required fields.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var activeUser = AuthSession.CurrentUser;
                var hotel = await _hotelSettingsService.GetHotelInformation();
                //var roomBooking = await _guestAccountService.GetRoomBookingByRoomIdAsync(_room.Id);

                var lockSetting = await _hotelSettingsService.GetSettingsByCategoryAsync("Operation Settings");
                if (lockSetting != null)
                {
                    var lockType = lockSetting.FirstOrDefault(x => x.Key == "LockType")?.Value;
                    if (lockType == "MIFI")
                    {
                        var isVerifyPayment = await _hotelSettingsService.GetSettingAsync("VerifyTransaction");
                        if (isVerifyPayment != null)
                        {
                            var value = isVerifyPayment.Value;
                            if (value != null && value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                            {
                                await VerifyPayment(hotel, activeUser);
                            }
                            else
                            {
                                IssueCardForMIFI();
                            }
                        }
                        
                    }
                    else if (lockType == "RFID")
                    {
                        MessageBox.Show("RFID card issuing is not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (lockType == "PULMOS")
                    {
                        var isVerifyPayment = await _hotelSettingsService.GetSettingAsync("VerifyTransaction");
                        if (isVerifyPayment != null)
                        {
                            var value = isVerifyPayment.Value;
                            if (value != null && value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                            {
                                await VerifyPayment(hotel, activeUser);
                            }
                            else
                            {
                                IssueCardForMIFI();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid lock type selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void IssueCardForMIFI()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                char[] card_snr = new char[100];
                string roomno = $"{_room.Building?.Number}.{_room.Floor?.Number}.{_room.Number}";
                string intime = dtpCheckIn.SelectedDate!.Value.ToString("yyyy-MM-dd HH:mm:ss");
                String outtime = dtpCheckOut.SelectedDate!.Value.ToString("yyyy-MM-dd HH:mm:ss");

                CARD_FLAGS iflags = CARD_FLAGS.CF_CHECK_TIMESTAMP;

                if (LockSDKHeaders.PreparedIssue(card_snr) == false)
                    return;
                var st = LockSDKMethods.MakeGuestCard(card_snr, roomno, _room.Area.Number, "", intime, outtime, iflags);

                if (st == 1)
                {
                    string cardSnr = new string(card_snr);
                    MessageBox.Show($"Successfully issued card: {cardSnr}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    LockSDKMethods.CheckErr(st);
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDefaultSetting();
            LoadPaymentMethod();
            await LoadBankAccount();
            int checkEncoder = CheckEncoder(LOCK_SETTING.LOCK_TYPE_PULMOS);
            if (checkEncoder != 1)
            {
                LockSDKMethods.CheckErr(checkEncoder);
            }
        }

        private async Task VerifyPayment(Hotel hotel, ApplicationUser activeUser)
        {
            //var booking = await _guestAccountService.GetBookingByIdAsync(bookingId);

            var verificationCode = new VerificationCode
            {
                Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                ServiceId = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 7)),
                ApplicationUserId = AuthSession.CurrentUser?.Id
            };

            await _verificationCodeService.AddCode(verificationCode);
            var accountNumber = ((BankAccount)cmbAccountNumber.SelectedItem).BankAccountNumber;
            var accountName = ((BankAccount)cmbAccountNumber.SelectedItem).BankAccountName;
            var bankName = ((BankAccount)cmbAccountNumber.SelectedItem).BankName;

            var response = await SenderHelper.SendEmailOTP(
                        hotel.Email,
                        "Card Creation OTP Generated Successfully",
                        "otp_template",
                        new EmailOTPVariable
                        {
                            OTP = verificationCode.Code,
                            accountNumber = $"{accountNumber} ({accountName}) | {bankName}",
                            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            guestName = $"{txtGuestName.Text} (Room: {txtRoom.Text})",
                            hotel = hotel.Name,
                            noOfNight = txtDays.Text,
                            paymentMethod = cmbPaymentMethod.SelectedValue != null
                                ? Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!).ToString()
                                : string.Empty,
                            receptionist = activeUser.FullName,
                            receptionistContact = activeUser.PhoneNumber,
                            service = "Booking",
                        }
                    );

            await SenderHelper.SendEmailOTP(
                        hotel.BackupEmail,
                        "Card Creation OTP Generated Successfully",
                        "otp_template",
                        new EmailOTPVariable
                        {
                            OTP = verificationCode.Code,
                            accountNumber = $"{accountNumber} ({accountName}) | {bankName}",
                            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            guestName = $"{txtGuestName.Text} (Room: {txtRoom.Text})",
                            hotel = hotel.Name,
                            noOfNight = txtDays.Text,
                            paymentMethod = cmbPaymentMethod.SelectedValue != null
                                ? Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!).ToString()
                                : string.Empty,
                            receptionist = activeUser.FullName,
                            receptionistContact = activeUser.PhoneNumber,
                            service = "Booking",
                        }
                    );

            if (response.IsSuccessStatusCode)
            {
                var verifyPaymentWindow = new VerifyPaymentWindow(
                    _verificationCodeService,
                    _hotelSettingsService,
                    _bookingRepository,
                    _transactionRepository,
                    verificationCode.ServiceId,
                    decimal.Parse("0.0"),
                    _applicationUserRoleRepository
                );

                if (verifyPaymentWindow.ShowDialog() == true)
                {
                    IssueCardForMIFI();
                    this.DialogResult = true;
                }
                else
                {
                    await _verificationCodeService.DeleteAsync(verificationCode.Id);
                }
            }
            else
            {
                MessageBox.Show(
                    "Error Sending OTP",
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
    }
}
