using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Data;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    public partial class GuestPage : Page
    {
        private readonly IGuestRepository _guestRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IApplicationUserRoleRepository _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private IServiceProvider _serviceProvider;

        public GuestPage(IGuestRepository guestRepository, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService, IApplicationUserRoleRepository userService, UserManager<ApplicationUser> userManager)
        {
            _guestRepository = guestRepository;
            _transactionRepository = transactionRepository;
            _userService = userService;
            _userManager = userManager;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
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

        public async Task LoadGuests()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guests = await _guestRepository.GetAllGuestsAsync();
                GuestDataGrid.ItemsSource = guests;
                txtGuestCount.Text = guests.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void AddGuest_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            AddGuestDialog addGuestDialog = _serviceProvider.GetRequiredService<AddGuestDialog>();
            if (addGuestDialog.ShowDialog() == true)
            {
                await LoadGuests();
            }
        }

        private async void SearchGuest_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var searchText = txtSearch.Text.Trim();
                if (string.IsNullOrEmpty(searchText) || searchText == "Search")
                {
                    await LoadGuests();
                    return;
                }
                var guests = await _guestRepository.SearchGuestAsync(searchText);
                GuestDataGrid.ItemsSource = guests;
                txtGuestCount.Text = guests.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void GuestDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGuests();
        }

        private void EditGuest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedGuest = (Domain.Entities.FrontDesk.Guest)GuestDataGrid.SelectedItem;

                if (selectedGuest.Id != null)
                {
                    UpdateGuestDialog updateGuestDialog = new UpdateGuestDialog(selectedGuest.Id, _guestRepository);
                    if (updateGuestDialog.ShowDialog() == true)
                    {
                        _ = LoadGuests();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void ViewGuest_Click(Object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedGuest = (Domain.Entities.FrontDesk.Guest)GuestDataGrid.SelectedItem;
                if (selectedGuest.Id != null)
                {
                    GuestDetailsDialog viewGuestDialog = new GuestDetailsDialog(selectedGuest.Id, _guestRepository, _transactionRepository, _hotelSettingsService);
                    if (viewGuestDialog.ShowDialog() == true)
                    {
                        await LoadGuests();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before viewing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void DeleteGuest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var userId = AuthSession.CurrentUser?.Id;

                if (userId != null)
                {
                    var user = await _userService.GetUserById(userId);
                    if (user != null)
                    {
                        bool isAdmin = await _userManager.IsInRoleAsync(user, DefaultRoles.Administrator.ToString()) ||
                                        await _userManager.IsInRoleAsync(user, DefaultRoles.Admin.ToString()) ||
                                        await _userManager.IsInRoleAsync(user, DefaultRoles.Manager.ToString());
                        if (!isAdmin)
                        {
                            MessageBox.Show("You are not authorized to perform this action", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            var selectedGuest = (Domain.Entities.FrontDesk.Guest)GuestDataGrid.SelectedItem;
                            if (selectedGuest.Id != null)
                            {
                                LoaderOverlay.Visibility = Visibility.Visible;
                                try
                                {
                                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this guest?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                    if (result == MessageBoxResult.Yes)
                                    {
                                        await _guestRepository.DeleteGuestAsync(selectedGuest.Id);
                                        await LoadGuests();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                }
                                finally
                                {
                                    LoaderOverlay.Visibility = Visibility.Collapsed;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please select a guest before deleting.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }

            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = GuestDataGrid.Columns
                    .Where(c => c.Header != null)
                    .Select(c => c.Header.ToString())
                    .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                    .ToList();

                var optionsWindow = new ExportDialog(columnNames);
                var result = optionsWindow.ShowDialog();

                if (result == true)
                {
                    var exportResult = optionsWindow.GetResult();
                    var hotel = await _hotelSettingsService.GetHotelInformation();

                    if (exportResult.SelectedColumns.Count == 0)
                    {
                        MessageBox.Show("Please select at least one column to export.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        if (hotel != null)
                        {
                            ExportHelper.ExportAndPrint(GuestDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Search")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = Brushes.Black;
            }
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search";
                txtSearch.Foreground = Brushes.Gray;
            }
        }
    }
}
