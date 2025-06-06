﻿using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Data;
using System.Windows;

namespace ESMART.Presentation.Forms.UserSetting.Roles
{
    /// <summary>
    /// Interaction logic for UpdateRoleDialog.xaml
    /// </summary>
    public partial class UpdateRoleDialog : Window
    {
        private readonly ApplicationRole _role;
        private readonly IApplicationUserRoleRepository _applicationRoleService;
        public UpdateRoleDialog(ApplicationRole role, IApplicationUserRoleRepository applicationRoleService)
        {
            _role = role;
            _applicationRoleService = applicationRoleService;
            InitializeComponent();
        }

        public void LoadData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                txtName.Text = _role.Name;
                txtDescription.Text = _role.Description;
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

        private void Window_Activated(object sender, EventArgs e)
        {
            LoadData();
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                string roleName = txtName.Text;
                if (!string.IsNullOrEmpty(roleName))
                {
                    _role.Name = roleName;
                    _role.Description = txtDescription.Text;

                    await _applicationRoleService.UpdateRole(_role);

                    MessageBox.Show($"Role '{roleName}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
