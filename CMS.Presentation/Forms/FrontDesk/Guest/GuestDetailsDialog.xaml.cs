using ESMART.Application.Common.Models;
using ESMART.Application.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Repositories.FrontDesk;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for GuestDetailsDialog.xaml
    /// </summary>
    public partial class GuestDetailsDialog : Window
    {
        private readonly string _id;
        private readonly IGuestRepository _guestRepository;
        public GuestDetailsDialog(string id, IGuestRepository guestRepository)
        {
            _id = id;
            _guestRepository = guestRepository;
            InitializeComponent();
        }

        private async Task LoadGuestDetails()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestResult = await _guestRepository.GetGuestByIdAsync(_id);
                if (!guestResult.Succeeded)
                {
                    var sb = new StringBuilder();
                    foreach (var item in guestResult.Errors)
                    {
                        sb.AppendLine(item);
                    }

                    MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    var guest = guestResult.Response;
                    var guestViewModel = new GuestViewModel()
                    {
                        Id = guest.Id,
                        GuestId = guest.GuestId,
                        FullName = $"{guest.FirstName} {guest.MiddleName} {guest.LastName}",
                        GuestImage = guest.GuestImage,
                        Email = guest.Email,
                        Gender = guest.Gender,
                        Street = guest.Street,
                        Status = guest.Status,
                        PhoneNumber = guest.PhoneNumber,
                        City = guest.City,
                        State = guest.State,
                        Country = guest.Country,
                        CreatedBy = guest.ApplicationUser?.FullName,
                        DateCreated = guest.DateCreated.ToString("ddd d MMMM, yyyy", CultureInfo.InvariantCulture),
                        DateModified = guest.DateModified.ToString("ddd d MMMM, yyyy", CultureInfo.InvariantCulture),
                    };

                    this.DataContext = guestViewModel;
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

        private async void DeleteGuest_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this guest?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await _guestRepository.DeleteGuestAsync(_id);
                    this.DialogResult = true;
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

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadGuestDetails();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
