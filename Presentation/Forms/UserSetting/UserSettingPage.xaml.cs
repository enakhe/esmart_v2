using ESMART.Application.Common.Interface;
using ESMART.Domain.ViewModels.Data;
using ESMART.Presentation.Forms.UserSetting.Roles;
using ESMART.Presentation.Forms.UserSetting.Users;
using Microsoft.Extensions.DependencyInjection;
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
        public UserSettingPage(IApplicationUserRoleRepository applicationRoleService)
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
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            AddRoleDialog addRoleDialog = serviceProvider.GetRequiredService<AddRoleDialog>();
            if (addRoleDialog.ShowDialog() == true)
            {
                await LoadRoleData();
            }
        }

        public async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            AddUserDialog addUserDialog = serviceProvider.GetRequiredService<AddUserDialog>();
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
    }
}
