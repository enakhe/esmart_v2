using ESMART.Infrastructure.Services;
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

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    /// <summary>
    /// Interaction logic for GuestSettngsDialog.xaml
    /// </summary>
    public partial class GuestSettngsDialog : Window
    {
        private Domain.Entities.FrontDesk.Guest _guest;
        private readonly GuestAccountService _guestAccountService;


        public GuestSettngsDialog(Domain.Entities.FrontDesk.Guest guest, GuestAccountService guestAccountService)
        {
            InitializeComponent();
            _guest = guest;
            _guestAccountService = guestAccountService;
        }

        private async Task LoadGuestAccount()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestAccount = await _guestAccountService.GetAccountAsync(_guest.Id);
                if (guestAccount != null)
                {
                    chkResBar.IsChecked = guestAccount.AllowBarAndRes;
                    chkLaundry.IsChecked = guestAccount.AllowLaundry;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the guest account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var allowBarAndRes = (bool)chkResBar.IsChecked!;
                var allowLaundry = (bool)chkLaundry.IsChecked!;

                await _guestAccountService.UpdateGuestAccountPreference(_guest.Id, allowBarAndRes, allowLaundry);
                MessageBox.Show("Account updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating the account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGuestAccount();
        }
    }
}
