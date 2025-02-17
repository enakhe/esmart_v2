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

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    public partial class GuestPage : Page
    {
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand LoginCommand { get; }
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
    }
}
