using ESMART.Application.Common.Interface;
using ESMART.Application.Interface;
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
using System.Collections.ObjectModel;
using ESMART.Domain.Entities.FrontDesk;


namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    public partial class GuestPage : Page
    {
        private readonly IGuestRepository _guestRepository;
        public GuestPage(IGuestRepository guestRepository)
        {
            _guestRepository = guestRepository;
            InitializeComponent();
        }

        public async Task LoadGuests()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guests = await _guestRepository.GetAllGuestsAsync();
                GuestDataGrid.ItemsSource = guests;
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
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            AddGuestDialog addGuestDialog = serviceProvider.GetRequiredService<AddGuestDialog>();
            if (addGuestDialog.ShowDialog() == true)
            {
                await LoadGuests();
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
                var selectedGuest = (Domain.ViewModels.FrontDesk.GuestViewModel)GuestDataGrid.SelectedItem;

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
    }
}
