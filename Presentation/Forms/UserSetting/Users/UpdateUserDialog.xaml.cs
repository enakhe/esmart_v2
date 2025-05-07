using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.ViewModels.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ESMART.Presentation.Forms.UserSetting.Users
{
    /// <summary>
    /// Interaction logic for UpdateUserDialog.xaml
    /// </summary>
    public partial class UpdateUserDialog : Window
    {
        private readonly IApplicationUserRoleRepository _userService;
        private readonly ApplicationUser _user;
        public UpdateUserDialog(ApplicationUser user, IApplicationUserRoleRepository userService)
        {
            _userService = userService;
            _user = user;
            InitializeComponent();
        }

        private void LoadUserData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                txtFirstName.Text = _user.FirstName;
                txtLastName.Text = _user.LastName;
                txtMiddleName.Text = _user.MiddleName;
                txtUserName.Text = _user.UserName;
                txtEmail.Text = _user.Email;
                txtPhoneNumber.Text = _user.PhoneNumber;
                cmbRole.SelectedValue = _user.RoleId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadRole()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var role = await _userService.GetAllRoles();

                if (role != null)
                {
                    cmbRole.ItemsSource = role;
                    cmbRole.DisplayMemberPath = "Name";
                    cmbRole.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading roles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                string role = ((ApplicationRoleViewModel)cmbRole.SelectedItem).Id;
                string firstName = txtFirstName.Text;
                string lastName = txtLastName.Text;
                string middleName = txtMiddleName.Text;
                string email = txtEmail.Text;
                string phoneNumber = txtPhoneNumber.Text;
                string userName = txtUserName.Text;

                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(firstName, lastName, email, phoneNumber, userName, role);
                if (!areFieldsEmpty)
                {
                    var selectedRole = await _userService.GetRoleById(role);

                    _user.FirstName = firstName;
                    _user.LastName = lastName;
                    _user.MiddleName = middleName;
                    _user.Email = email;
                    _user.PhoneNumber = phoneNumber;
                    _user.UserName = userName;
                    _user.RoleId = selectedRole.Id;

                    await _userService.UpdateUser(_user);
                    await _userService.UpdateUserRole(_user, selectedRole);

                    MessageBox.Show($"User '{_user.FullName}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please fill in all fields.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
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

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadRole();
            LoadUserData();
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtEmail.Text);
            if (isNull)
            {
                txtEmail.BorderBrush = new SolidColorBrush(Colors.Red);
                txtEmail.ToolTip = "Email cannot be empty";
            }
            else
            {
                txtUserName.Text = txtEmail.Text;
            }
        }
    }
}
