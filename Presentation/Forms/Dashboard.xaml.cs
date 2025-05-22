using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Data;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Presentation.Forms.Cards;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.FrontDesk.Reservation;
using ESMART.Presentation.Forms.FrontDesk.Room;
using ESMART.Presentation.Forms.Home;
using ESMART.Presentation.Forms.Reports;
using ESMART.Presentation.Forms.RoomSetting;
using ESMART.Presentation.Forms.Setting;
using ESMART.Presentation.Forms.StockKeeping.Inventory;
using ESMART.Presentation.Forms.StockKeeping.MenuItem;
using ESMART.Presentation.Forms.UserSetting;
using ESMART.Presentation.Session;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace ESMART.Presentation.Forms
{
    public partial class Dashboard : Window
    {
        private bool _isLoading;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IApplicationUserRoleRepository _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBackupRepository _backupRepository;
        private IServiceProvider _serviceProvider;

        public Dashboard(IHotelSettingsService hotelSettingsService, IApplicationUserRoleRepository userService, UserManager<ApplicationUser> userManager, IBackupRepository backupRepository)
        {
            _hotelSettingsService = hotelSettingsService;
            _userService = userService;
            _userManager = userManager;
            _backupRepository = backupRepository;
            InitializeComponent();


            ShowDefaultHome();
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

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                LoaderGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async Task LoadData()
        {
            try
            {
                var hotel = await _hotelSettingsService.GetHotelInformation();
                if (hotel != null)
                {
                    txtHotelName.Text = hotel.Name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadHomePage()
        {
            InitializeServices();

            IndexPage indexPage = _serviceProvider.GetRequiredService<IndexPage>();

            MainFrame.Navigate(indexPage);
        }

        private void LoadStockKeeperPage()
        {
            InitializeServices();
            StockKeepingIndexPage storeKeepingPage = _serviceProvider.GetRequiredService<StockKeepingIndexPage>();
            MainFrame.Navigate(storeKeepingPage);
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            LoadHomePage();
        }

        private void GuestButton_Click(Object sender, RoutedEventArgs e)
        {
            InitializeServices();

            GuestPage guestPage = _serviceProvider.GetRequiredService<GuestPage>();

            MainFrame.Navigate(guestPage);
        }

        private void BookingButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            BookingPage bookingPage = _serviceProvider.GetRequiredService<BookingPage>();

            MainFrame.Navigate(bookingPage);
        }

        private void RoomSettingButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            RoomSettingPage roomSettingPage = _serviceProvider.GetRequiredService<RoomSettingPage>();

            MainFrame.Navigate(roomSettingPage);
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            SettingDialog settingPage = _serviceProvider.GetRequiredService<SettingDialog>();
            settingPage.ShowDialog();
        }

        private void UserSettingButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            UserSettingPage userSettingPage = _serviceProvider.GetRequiredService<UserSettingPage>();
            MainFrame.Navigate(userSettingPage);
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            ReportPage reportPage = _serviceProvider.GetRequiredService<ReportPage>();
            MainFrame.Navigate(reportPage);
        }

        private void ReservationButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            ReservationPage reservationPage = _serviceProvider.GetRequiredService<ReservationPage>();
            MainFrame.Navigate(reservationPage);
        }

        //Navigate to Card setting page
        private void CarSettingButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            CardPage cardPage = _serviceProvider.GetRequiredService<CardPage>();
            MainFrame.Navigate(cardPage);
        }

        private void RoomButton_Click(Object sender, RoutedEventArgs e)
        {
            InitializeServices();

            RoomPage roomPage = _serviceProvider.GetRequiredService<RoomPage>();
            MainFrame.Navigate(roomPage);
        }

        private void StockKeepingButton_Click(Object sender, RoutedEventArgs e)
        {
            LoadStockKeeperPage();
        }

        private void MenuItemButton_Click(Object sender, RoutedEventArgs e)
        {
            InitializeServices();
            MenuItemPage menuItemPage = _serviceProvider.GetRequiredService<MenuItemPage>();
            MainFrame.Navigate(menuItemPage);
        }

        private void InventoryItemButton_Click(Object sender, RoutedEventArgs e)
        {
            InitializeServices();
            InventoryItemPage inventoryItemPage = _serviceProvider.GetRequiredService<InventoryItemPage>();
            MainFrame.Navigate(inventoryItemPage);
        }

        private void OpenSidebar_Click(object sender, RoutedEventArgs e)
        {
            if (this.sideBar.Width < 200)
                this.sideBar.Width = 200;
            else
                this.sideBar.Width = 50;
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            await ApplyAuthorization();
            await LoadData();
        }

        private async Task ApplyAuthorization()
        {
            try
            {
                var userId = AuthSession.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId)) return;

                var user = await _userService.GetUserById(userId);
                if (user == null) return;

                var roles = await _userManager.GetRolesAsync(user);
                var roleSet = roles.ToHashSet();

                // Define visibility logic
                bool isSuperAdmin = roleSet.Contains(DefaultRoles.Administrator.ToString()) ||
                                    roleSet.Contains(DefaultRoles.Admin.ToString());

                bool isStoreKeeper = roleSet.Contains(DefaultRoles.StoreKeeper.ToString());
                bool isFrontDesk = roleSet.Contains(DefaultRoles.Receptionist.ToString());
                bool isAdmin = roleSet.Contains(DefaultRoles.Admin.ToString());

                // Apply visibility
                AdminControls.Visibility = isSuperAdmin ? Visibility.Visible : Visibility.Collapsed;
                SettingButton.Visibility = isSuperAdmin ? Visibility.Collapsed : Visibility.Visible;
                //StoreKeepingControls.Visibility = (isSuperAdmin || isStoreKeeper) ? Visibility.Visible : Visibility.Collapsed;
                FrontDeskControls.Visibility = (isSuperAdmin || isFrontDesk) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void ShowDefaultHome()
        {
            try
            {
                var userId = AuthSession.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId)) return;

                var user = await _userService.GetUserById(userId);
                if (user == null) return;

                var roles = await _userManager.GetRolesAsync(user);
                var roleSet = roles.ToHashSet();

                bool isSuperAdmin = roleSet.Contains(DefaultRoles.Administrator.ToString()) ||
                                    roleSet.Contains(DefaultRoles.Admin.ToString());

                bool isStoreKeeper = roleSet.Contains(DefaultRoles.StoreKeeper.ToString());
                bool isFrontDesk = roleSet.Contains(DefaultRoles.Receptionist.ToString());

                if (isFrontDesk)
                {
                    LoadHomePage();
                }
                else if (isStoreKeeper)
                {
                    LoadStockKeeperPage();
                }
                else if (isSuperAdmin)
                {
                    LoadHomePage();
                }
                else
                {
                    MessageBox.Show("You do not have permission to access this application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AppSessionManager.LogoutToLogin();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderGrid.Visibility = Visibility.Visible;
            try
            {
                var response = MessageBox.Show("Are you sure you want to create a backup?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (response == MessageBoxResult.Yes)
                {
                    var backupFile = BackupRepository.CreateBackup();

                    var userBackUp = await _backupRepository.GetBackupSettingsAsync();
                    userBackUp.LastBackup = DateTime.Now;
                    await _backupRepository.UpdateBackupSettingsAsync(userBackUp);

                    MessageBox.Show("Backup created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoaderGrid.Visibility = Visibility.Visible;

                    var hotel = await _hotelSettingsService.GetHotelInformation();
                    if (hotel != null)
                    {
                        var zippedFile = BackupRepository.ZipFiles(backupFile);
                        var result = await BackupRepository.UploadBackupAsync(zippedFile, hotel.Name);

                        if (result.Success)
                        {
                            MessageBox.Show($"Backup uploaded to cloud successfully: {result.DownloadLink}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Failed to upload backup", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoaderGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void LogOutButton_Click(Object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to log out?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                AppSessionManager.LogoutToLogin();
            }
        }
    }
}
