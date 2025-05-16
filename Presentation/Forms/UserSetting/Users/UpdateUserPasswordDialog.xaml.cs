using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Data;
using Microsoft.AspNetCore.Identity;
using System.Windows;

namespace ESMART.Presentation.Forms.UserSetting.Users
{
    /// <summary>
    /// Interaction logic for UpdateUserPasswordDialog.xaml
    /// </summary>
    public partial class UpdateUserPasswordDialog : Window
    {
        private readonly IApplicationUserRoleRepository _userService;
        private readonly ApplicationUser _user;
        public UpdateUserPasswordDialog(IApplicationUserRoleRepository userService, ApplicationUser applicationUser)
        {
            _userService = userService;
            _user = applicationUser;
            InitializeComponent();
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var password = txtNewPassword.Text;

                var passwordHasher = new PasswordHasher<ApplicationUser>();
                _user.PasswordHash = passwordHasher.HashPassword(_user, password);

                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Password cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await _userService.UpdateUser(_user);

                MessageBox.Show("User password updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
