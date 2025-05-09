#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.Enum;
using ESMART.Presentation.LockSDK;
using ESMART.Presentation.Session;
using OfficeOpenXml;
using System.Windows;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for IssueCardDialog.xaml
    /// </summary>
    public partial class IssueCardDialog : Window
    {
        private IBookingRepository _bookingRepository;
        private IGuestRepository _guestRepository;
        private IHotelSettingsService _hotelSettingsService;
        private Domain.Entities.FrontDesk.Booking _booking;
        public IssueCardDialog(IBookingRepository bookingRepository, IGuestRepository guestRepository, Domain.Entities.FrontDesk.Booking booking, IHotelSettingsService hotelSettingsService)
        {
            _bookingRepository = bookingRepository;
            _hotelSettingsService = hotelSettingsService;
            _guestRepository = guestRepository;
            _booking = booking;
            InitializeComponent();
        }

        private async Task LoadBookingData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var bookingDetails = await _bookingRepository.GetBookingByIdViewModel(_booking.Id);
                if (bookingDetails != null)
                {
                    this.DataContext = bookingDetails;
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

        private void RecycleCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var st = LockSDKMethods.RecycleCard();
                if (st == 1)
                {
                    MessageBox.Show("Successfully recycled card", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
                var lockSetting = await _hotelSettingsService.GetSettingsByCategoryAsync("Operation Settings");
                if (lockSetting != null)
                {
                    var lockType = lockSetting.FirstOrDefault(x => x.Key == "LockType")?.Value;
                    if (lockType == "MIFI")
                    {
                        await IssueCardForMIFI();
                    }
                    else if (lockType == "RFID")
                    {
                        MessageBox.Show("RFID card issuing is not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (lockType == "PULMOS")
                    {
                        await IssueCardForMIFI();
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
        }

        private async Task IssueCardForMIFI()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                char[] card_snr = new char[100];
                string roomno = $"{_booking.Room?.Building?.Number}.{_booking.Room?.Floor?.Number}.{_booking.Room?.Number}";
                string intime = _booking.CheckIn.ToString("yyyy-MM-dd HH:mm:ss");
                String outtime = _booking.CheckOut.ToString("yyyy-MM-dd HH:mm:ss");

                CARD_FLAGS iflags = CARD_FLAGS.CF_CHECK_TIMESTAMP;

                if (LockSDKHeaders.PreparedIssue(card_snr) == false)
                    return;
                var st = LockSDKMethods.MakeGuestCard(card_snr, roomno, _booking.Room.Area.Number, "", intime, outtime, iflags);

                if (st == 1)
                {
                    _booking.Guest.Status = "Active";
                    await _guestRepository.UpdateGuestAsync(_booking.Guest);
                    string cardSnr = new string(card_snr);
                    MessageBox.Show($"Successfully issued card: {cardSnr}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    LockSDKMethods.CheckErr(st);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        //private async Task IssueCardForRF()
        //{
        //    LoaderOverlay.Visibility = Visibility.Visible;
        //    try
        //    {
        //        byte[] carddata = new byte[128];
        //        int i, st;
        //        int dlscoid;
        //        byte cardno;
        //        byte dai;
        //        byte llock;
        //        string datastr = "";
        //        string lockstr, EDatestr;
        //        byte[] cardbuf = new byte[128];
        //        char[] lockno = new char[6];
        //        char[] EDate = new char[10];

        //        lockstr = $"{_booking.Room.Building.Number.Substring(1)}0{_booking.Room.Floor.Number}{_booking.Room.Number.Substring(1)}";
        //        for (i = 0; i < 6; i++)
        //            lockno[i] = Convert.ToChar(lockstr.Substring(i, 1));

        //        EDatestr = _booking.CheckIn.ToString("yyMMdd") + _booking.CheckOut.ToString("HHmm");
        //        for (i = 0; i < 10; i++)
        //            EDate[i] = Convert.ToChar(EDatestr.Substring(i, 1));

        //        dlscoid = int.Parse(GetAuthCardFromDB());
        //        cardno = 0;
        //        dai = 1;
        //        llock = (byte)1;

        //        st = LockSDKHeaders.WriteGuestCardA(dlscoid, cardno, dai, llock, EDate, lockno, cardbuf);
        //        Thread.Sleep(400);
        //        if (st == 0)
        //        {
        //            LockSDKHeaders.Buzzer(50);
        //            for (i = 0; i < 32; i++)
        //            {
        //                datastr = datastr + ((char)carddata[i]).ToString();
        //            }
        //            MessageBox.Show("Guest card created successfully!");
        //        }
        //        else
        //        {
        //            MessageBox.Show("Failed to create guest card, return value: " + st.ToString());
        //        }

        //        if (st == 0)
        //        {
        //            CARD_INFO cardInfo = new CARD_INFO();
        //            byte[] cbuf = new byte[10000];
        //            cardInfo = new CARD_INFO();
        //            CompanyInformation foundCompany = _sytemSetupController.GetCompanyInfo();

        //            string cardNoString = FormHelper.ByteArrayToString(cardInfo.CardNo);
        //            MakeCardType cardType = FormHelper.GetCardType(cardInfo.CardType);

        //            GuestCard guestCard = new GuestCard()
        //            {
        //                Id = booking.Id,
        //                CardNo = cardNoString,
        //                CardType = FormHelper.FormatEnumName(cardType),
        //                IssueTime = DateTime.Now,
        //                RefundTime = txtOutTime.Value,
        //                IssuedBy = AuthSession.CurrentUser.Id,
        //                ApplicationUser = _userController.GetApplicationUserById(AuthSession.CurrentUser.Id),
        //                CanOpenDeadLocks = true,
        //                PassageMode = false,
        //                DateCreated = DateTime.Now,
        //                DateModified = DateTime.Now,
        //            };
        //            _cardController.AddGuestCard(guestCard);
        //            string guestCardString = $"Id = {guestCard.Id}\n" +
        //                     $"Card No = {guestCard.CardNo}\n" +
        //                     $"Card Type = {guestCard.CardType}\n" +
        //                     $"Room = {booking.Room.RoomNo}\n" +
        //                     $"Issue Time = {guestCard.IssueTime}\n" +
        //                     $"Refund Time = {guestCard.RefundTime}\n" +
        //                     $"Issued By = {guestCard.IssuedBy}\n" +
        //                     $"Application User = {guestCard.ApplicationUser?.FullName}\n" +
        //                     $"Date Created = {guestCard.DateCreated}\n" +
        //                     $"Date Modified = {guestCard.DateModified}";

        //            if (foundCompany != null)
        //            {
        //                if (foundCompany.Email != null)
        //                {
        //                    await EmailHelper.SendEmail(foundCompany.Email, "Booking Card Created", guestCardString);
        //                }
        //            }
        //            this.DialogResult = DialogResult.OK;
        //            this.Close();

        //            MessageBox.Show("Successfully issued card", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        else if (st == (int)ERROR_TYPE.PORT_IN_USED)
        //        {
        //            MessageBox.Show("Failed to issue card: Port is already in use.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        else
        //        {
        //            MessageBox.Show($"Failed to issue card, error code: {st}", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    finally
        //    {
        //        LoaderOverlay.Visibility = Visibility.Collapsed;
        //    }
        //}

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadBookingData();
        }
    }
}
