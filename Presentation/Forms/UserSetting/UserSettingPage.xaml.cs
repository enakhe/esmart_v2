using ESMART.Application.Common.Interface;
using ESMART.Domain.ViewModels.Data;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.UserSetting.Roles;
using ESMART.Presentation.Forms.UserSetting.Users;
using ESMART.Presentation.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.UserSetting
{
    /// <summary>
    /// Interaction logic for UserSettingPage.xaml
    /// </summary>
    public partial class UserSettingPage : Page
    {
        private readonly IApplicationUserRoleRepository _applicationRoleService;
        private readonly IHotelSettingsService _hotelSettingsService;
        private IServiceProvider _serviceProvider;
        public UserSettingPage(IApplicationUserRoleRepository applicationRoleService, IHotelSettingsService hotelSettingsService)
        {
            _applicationRoleService = applicationRoleService;
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

        // Load User Data
        private async Task LoadUserData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var allUsers = await _applicationRoleService.GetAllUsers();
                if (allUsers != null)
                {
                    UserDataGrid.ItemsSource = allUsers;
                    txtUserCount.Text = allUsers.Count.ToString();
                }
                else
                {
                    MessageBox.Show("No users found", "Error", MessageBoxButton.OK,
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
            InitializeServices();

            AddRoleDialog addRoleDialog = _serviceProvider.GetRequiredService<AddRoleDialog>();
            if (addRoleDialog.ShowDialog() == true)
            {
                await LoadRoleData();
            }
        }

        public async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            AddUserDialog addUserDialog = _serviceProvider.GetRequiredService<AddUserDialog>();
            if (addUserDialog.ShowDialog() == true)
            {
                await LoadUserData();
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

        // Update user
        private async void UpdateUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedUser = (ApplicationUserViewModel)UserDataGrid.SelectedItem;
                if (selectedUser.Id != null)
                {
                    var user = await _applicationRoleService.GetUserById(selectedUser.Id);
                    UpdateUserDialog updateUserDialog = new UpdateUserDialog(user, _applicationRoleService);
                    if (updateUserDialog.ShowDialog() == true)
                    {
                        await LoadUserData();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        //Update password
        private async void UpdatePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedUser = (ApplicationUserViewModel)UserDataGrid.SelectedItem;
                if (selectedUser.Id != null)
                {
                    var user = await _applicationRoleService.GetUserById(selectedUser.Id);
                    UpdateUserPasswordDialog updatePasswordDialog = new UpdateUserPasswordDialog(_applicationRoleService, user);
                    if (updatePasswordDialog.ShowDialog() == true)
                    {
                        await LoadUserData();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // Delete User
        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedUser = (ApplicationUserViewModel)UserDataGrid.SelectedItem;
                if (selectedUser.Id != null)
                {
                    var user = await _applicationRoleService.GetUserById(selectedUser.Id);
                    if (user != null)
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete the user '{user.FullName}'?", "Delete User", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            await _applicationRoleService.DeleteUser(user);
                            await LoadUserData();
                        }
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
                var allUsers = await _applicationRoleService.GetAllUsers();

                txtRoleCount.Text = roleCount.Count.ToString();
                txtUserCount.Text = allUsers.Count.ToString();
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
                else if (selectedTab == tbUsers)
                {
                    await LoadUserData();
                }
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = UserDataGrid.Columns
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
                            ExportHelper.ExportAndPrint(UserDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
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
    }
}
