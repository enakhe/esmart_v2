using DocumentFormat.OpenXml.Spreadsheet;
using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Data;
using Microsoft.AspNetCore.Identity;
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
using System.Windows.Shapes;

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
