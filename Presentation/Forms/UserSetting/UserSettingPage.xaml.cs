using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.ViewModels.Data;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Repositories.RoomSetting;
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

        public async Task LoadRoleData()
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
                await LoadRoleData();
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedRole = (ApplicationRoleViewModel)RoleDataGrid.SelectedItem;

                if (selectedRole.Id != null)
                { 
                    var role = await _applicationRoleService.GetRoleById(selectedRole.Id);
                    UpdateRoleDialog updateRoleDialog = new UpdateRoleDialog(role, _applicationRoleService);
                    if (updateRoleDialog.ShowDialog() == true)
                    {
                        await LoadRoleData();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedRole = (ApplicationRoleViewModel)RoleDataGrid.SelectedItem;
                if (selectedRole.Id != null)
                {
                    var role = await _applicationRoleService.GetRoleById(selectedRole.Id);
                    if (role != null)
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete the role '{role.Name}'?", "Delete Role", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            await _applicationRoleService.DeleteRole(role);
                            await LoadRoleData();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async Task LoadMetrics()
        {
            try
            {
                var roleCount = await _applicationRoleService.GetAllRoles();

                txtRoleCount.Text = roleCount.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMetrics();
        }

        private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl)
            {
                var selectedTab = tabControl.SelectedItem as TabItem;

                if (selectedTab == tbRole)
                {
                    await LoadRoleData();
                }

                await LoadMetrics();
            }
        }
    }
}
