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
        public RoomSettingPage()
        {
            InitializeComponent();
        }

        private void AddBuildingButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            AddBuildingDialog addBuilding = serviceProvider.GetRequiredService<AddBuildingDialog>();
            if (addBuilding.ShowDialog() == true)
            {

            }
        }
    }
}
