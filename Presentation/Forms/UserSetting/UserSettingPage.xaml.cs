using ESMART.Application.Common.Interface;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.UserSetting.Roles;
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

namespace ESMART.Presentation.Forms.UserSetting
{
    /// <summary>
    /// Interaction logic for UserSettingPage.xaml
    /// </summary>
    public partial class UserSettingPage : Page
    {
        private readonly IApplicationRole _applicationRoleService;
        public UserSettingPage(IApplicationRole applicationRoleService)
        {
            _applicationRoleService = applicationRoleService;
            InitializeComponent();
        }

        public async Task LoadData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var allRoles = await _applicationRoleService.GetAllRoles();
                if (allRoles != null)
                {
                    RoleDataGrid.ItemsSource = allRoles;
                    txtRoleCount.Text = allRoles.Count.ToString();
                }
                else
                {
                    MessageBox.Show("No roles found", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
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

        public async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            AddRoleDialog addRoleDialog = serviceProvider.GetRequiredService<AddRoleDialog>();
            if (addRoleDialog.ShowDialog() == true)
            {
                await LoadData();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }
    }
}
