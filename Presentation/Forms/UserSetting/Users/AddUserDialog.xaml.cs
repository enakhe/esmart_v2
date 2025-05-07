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
    /// Interaction logic for AddUSerDialog.xaml
    /// </summary>
    public partial class AddUserDialog : Window
    {
        private readonly IApplicationUserRoleRepository _userService;
        public AddUserDialog(IApplicationUserRoleRepository userService)
        {
            _userService = userService;
            InitializeComponent();
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
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
                string password = txtPassword.Text;


                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(firstName, lastName, email, phoneNumber, userName, password, role);
                if (!areFieldsEmpty)
                {
                    var selectedRole = await _userService.GetRoleById(role);

                    var user = new ApplicationUser
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        MiddleName = middleName,
                        Email = email,
                        PhoneNumber = phoneNumber,
                        UserName = userName,
                        PasswordHash = password,
                        RoleId = selectedRole.Id
                    };

                    await _userService.AddUser(user, password);
                    await _userService.AssignRoleToUser(user, selectedRole);

                    selectedRole.NoOfUser += 1;
                    await _userService.UpdateRole(selectedRole);

                    MessageBox.Show($"User '{userName}' added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please enter a valid username and password.");
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

        private void txtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtEmail.Text);
            if (isNull)
            {
                txtEmail.BorderBrush = new SolidColorBrush(Colors.Red);
                txtEmail.ToolTip = "Email cannot be empty";
            }
            else
            {
                var random = new Random();
                var randomNumber = random.Next(1000, 9999);
                var password = $"Default${randomNumber}";
                txtPassword.Text = password;
            }
        }
    }
}
