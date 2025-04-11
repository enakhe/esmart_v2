using ESMART.Application.Interface;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Presentation.Forms.FrontDesk.Guest;
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

namespace ESMART.Presentation.Forms.RoomSetting
{
    /// <summary>
    /// Interaction logic for RoomSettingPage.xaml
    /// </summary>
    public partial class RoomSettingPage : Page
    {
        private readonly IRoomRepository _roomRepository;
        public RoomSettingPage(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
            InitializeComponent();
        }

        public async Task LoadBuilding()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var buildings = await _roomRepository.GetAllBuildings();
                BuildingDataGrid.ItemsSource = buildings;
                txtBuildingCount.Text = buildings.Count.ToString();
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

        private async void AddBuildingButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            AddBuildingDialog addBuilding = serviceProvider.GetRequiredService<AddBuildingDialog>();
            if (addBuilding.ShowDialog() == true)
            {
               await LoadBuilding();
            }
        }

        private async void BuildingDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBuilding();
        }
    }
}
