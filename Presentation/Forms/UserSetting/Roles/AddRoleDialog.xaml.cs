using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Data;
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

namespace ESMART.Presentation.Forms.UserSetting.Roles
{
    /// <summary>
    /// Interaction logic for AddRoleDialog.xaml
    /// </summary>
    public partial class AddRoleDialog : Window
    {
        private readonly IApplicationRole _applicationRoleService;
        public AddRoleDialog(IApplicationRole applicationRoleService)
        {
            _applicationRoleService = applicationRoleService;
            InitializeComponent();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                string roleName = txtName.Text;
                if (!string.IsNullOrEmpty(roleName))
                {
                    var role = new ApplicationRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = roleName,
                        Description = txtDescription.Text,
                    };

                    await _applicationRoleService.AddRole(role);

                    MessageBox.Show($"Role '{roleName}' added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please enter a valid role name.");
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
    }
}
