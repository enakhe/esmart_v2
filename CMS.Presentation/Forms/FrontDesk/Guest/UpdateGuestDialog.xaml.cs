using ESMART.Application.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Infrastructure.Repositories.FrontDesk;
using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class UpdateGuestDialog : Window
    {
        private readonly string _id;
        private readonly IGuestRepository _guestRepository;
        public UpdateGuestDialog(string id, IGuestRepository guestRepository)
        {
            _id = id;
            _guestRepository = guestRepository;
            InitializeComponent();
        }

        private async void LoadGuest()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var result = await _guestRepository.GetGuestByIdAsync(_id);
                if (result.Succeeded)
                {
                    txtFirstName.Text = result.Response.FirstName;
                    txtLastName.Text = result.Response.LastName;
                    txtMiddleName.Text = result.Response.MiddleName;
                    txtEmail.Text = result.Response.Email;
                    txtPhoneNumber.Text = result.Response.PhoneNumber;
                    cbGender.Text = result.Response.Gender;

                    txtStreet.Text = result.Response.Street;
                    txtCity.Text = result.Response.City;
                    txtState.Text = result.Response.State;
                    txtCountry.Text = result.Response.Country;
                    if (result.Response.GuestImage != null && result.Response.GuestImage.Length > 0)
                    {
                        using (var ms = new MemoryStream(result.Response.GuestImage))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                            imgProfileImg.Source = bitmap;
                        }
                    }
                }
                else
                {
                    var sb = new StringBuilder();
                    foreach (var item in result.Errors)
                    {
                        sb.AppendLine(item);
                    }

                    MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGuest();
        }
    }
}
