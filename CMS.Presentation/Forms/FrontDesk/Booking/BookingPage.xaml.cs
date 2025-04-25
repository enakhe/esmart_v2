using ESMART.Application.Common.Interface;
using ESMART.Application.Interface;
using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.Verification;
using ESMART.Presentation.Forms.Verification;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using Microsoft.Extensions.DependencyInjection;
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

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for BookingPage.xaml
    /// </summary>
    public partial class BookingPage : Page
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IGuestRepository _guestRepository;
        private readonly IRoomRepository _roomRepository;
        public BookingPage(IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService, IHotelSettingsService hotelSettingsService, IRoomRepository roomRepository, IGuestRepository guestRepository)
        {
            _bookingRepository = bookingRepository;
            _verificationCodeService = verificationCodeService;
            _hotelSettingsService = hotelSettingsService;
            _roomRepository = roomRepository;
            _guestRepository = guestRepository;
            InitializeComponent();
        }

        private async Task LoadBooking()
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;
                var allBooking = await _bookingRepository.GetAllBookingsAsync();

                if (allBooking != null)
                {
                    BookingDataGrid.ItemsSource = allBooking;
                    txtBookingCount.Text = allBooking.Count.ToString();
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

        private async void AddSingleBooking_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            AddBookingDialog addBookingDialog = serviceProvider.GetRequiredService<AddBookingDialog>();
            if (addBookingDialog.ShowDialog() == true)
            {
                await LoadBooking();
            }
        }

        private async void IssueCard_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedBooking = (BookingViewModel)BookingDataGrid.SelectedItem;
                    if (selectedBooking.Id != null)
                    {
                        var hotel = await _hotelSettingsService.GetHotelInformation();
                        var result = await _bookingRepository.GetBookingById(selectedBooking.Id);
                        if (result != null)
                        {
                            var booking = result.Response;
                            if (booking != null)
                            {
                                if (booking.Status != Domain.Enum.PaymentStatus.Completed)
                                {
                                    var verificationCode = new VerificationCode()
                                    {
                                        Code = booking.BookingId,
                                        BookingId = booking.Id,
                                        IssuedBy = AuthSession.CurrentUser?.Id
                                    };

                                    await _verificationCodeService.AddCode(verificationCode);

                                    if(hotel != null)
                                    {
                                        var response = await SenderHelper.SendOtp(hotel, booking, booking.Guest, "Booking", verificationCode.Code, booking.TotalAmount);

                                        if (response.IsSuccessStatusCode)
                                        {
                                            MessageBox.Show("Kindly verify booking payment", "Code resent", MessageBoxButton.OK, MessageBoxImage.Information);

                                            VerifyPaymentWindow verifyPaymentWindow = new(_verificationCodeService, _hotelSettingsService, _bookingRepository, booking);
                                            if (verifyPaymentWindow.ShowDialog() == true)
                                            {
                                                IssueCardDialog issueCardDialog = new IssueCardDialog(_bookingRepository, booking);
                                                if (issueCardDialog.ShowDialog() == true)
                                                {
                                                    await LoadBooking();
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    IssueCardDialog issueCardDialog = new IssueCardDialog(_bookingRepository, booking);
                                    if (issueCardDialog.ShowDialog() == true)
                                    {
                                        await LoadBooking();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a booking before issuing card.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async void ExtendStay_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedBooking = (BookingViewModel)BookingDataGrid.SelectedItem;
                    if (selectedBooking.Id != null)
                    {
                        var hotel = await _hotelSettingsService.GetHotelInformation();
                        var result = await _bookingRepository.GetBookingById(selectedBooking.Id);
                        if (result != null)
                        {
                            var booking = result.Response;
                            if (booking != null)
                            {
                                if (booking.Status != Domain.Enum.PaymentStatus.Completed)
                                {
                                    var verificationCode = new VerificationCode()
                                    {
                                        Code = booking.BookingId,
                                        BookingId = booking.Id,
                                        IssuedBy = AuthSession.CurrentUser?.Id
                                    };

                                    await _verificationCodeService.AddCode(verificationCode);

                                    if (hotel != null)
                                    {
                                        var response = await SenderHelper.SendOtp(hotel, booking, booking.Guest, "Booking", verificationCode.Code, booking.TotalAmount);

                                        if (response.IsSuccessStatusCode)
                                        {
                                            MessageBox.Show("Kindly verify booking payment", "Code resent", MessageBoxButton.OK, MessageBoxImage.Information);

                                            VerifyPaymentWindow verifyPaymentWindow = new(_verificationCodeService, _hotelSettingsService, _bookingRepository, booking);
                                            if (verifyPaymentWindow.ShowDialog() == true)
                                            {
                                                ExtendStayDialog extendStayDialog = new ExtendStayDialog(_guestRepository, _roomRepository, _hotelSettingsService, _bookingRepository, _verificationCodeService, booking);
                                                if (extendStayDialog.ShowDialog() == true)
                                                {
                                                    await LoadBooking();
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ExtendStayDialog extendStayDialog = new ExtendStayDialog(_guestRepository, _roomRepository, _hotelSettingsService, _bookingRepository, _verificationCodeService, booking);
                                    if (extendStayDialog.ShowDialog() == true)
                                    {
                                        await LoadBooking();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a booking before issuing card.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedBooking = (BookingViewModel)BookingDataGrid.SelectedItem;

                    if (selectedBooking.Id != null)
                    {
                        MessageBoxResult messageResult = MessageBox.Show("Are you sure you want to delete this booking?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (messageResult == MessageBoxResult.Yes)
                        {
                            LoaderOverlay.Visibility = Visibility.Visible;
                            var result = await _bookingRepository.DeleteBooking(selectedBooking.Id);

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

                            await LoadBooking();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a booking before deleting.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBooking();
        }
    }
}
