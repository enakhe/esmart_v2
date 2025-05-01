using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ESMART.Presentation.Forms.Setting.SystemSetup
{
    /// <summary>
    /// Interaction logic for HotelInformationPage.xaml
    /// </summary>
    public partial class HotelInformationPage : Page
    {
        private readonly IHotelSettingsService _hotelSettingsService;
        private string profilePictureImage;
        public HotelInformationPage(IHotelSettingsService hotelSettingsService)
        {
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        private async Task LoadHotelInformation()
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;
                var hotel = await _hotelSettingsService.GetHotelInformation();

                if (hotel != null)
                {
                    txtHotelName.Text = hotel.Name;
                    txtAddress.Text = hotel.Address;
                    txtPhoneNumber.Text = hotel.PhoneNumber;
                    txtEmail.Text = hotel.Email;

                    if (hotel.LogoUrl != null)
                    {
                        using (var ms = new MemoryStream(hotel.LogoUrl))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = ms;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            imgLogo.Source = bitmap;
                        }
                    }
                    else
                    {
                        imgLogo.Source = null;
                    }
                }
                else
                {
                    MessageBox.Show("Hotel information not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading hotel information: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Select Image",
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    profilePictureImage = openFileDialog.FileName;
                    imgLogo.Source = new BitmapImage(new Uri(profilePictureImage));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                string hotelName = txtHotelName.Text;
                string address = txtAddress.Text;
                string phoneNumber = txtPhoneNumber.Text;
                string email = txtEmail.Text;
                byte[] logoImage = null;

                if (!string.IsNullOrEmpty(profilePictureImage))
                {
                    logoImage = File.ReadAllBytes(profilePictureImage);
                }

                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(hotelName, address, phoneNumber, email);
                if (areFieldsEmpty)
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var hotel = await _hotelSettingsService.GetHotelInformation();

                if (hotel != null)
                {
                    hotel.Name = hotelName;
                    hotel.Address = address;
                    hotel.PhoneNumber = phoneNumber;
                    hotel.Email = email;
                    hotel.LogoUrl = logoImage;
                }

                var result = await _hotelSettingsService.UpdateHotelInformation(hotel!);
                MessageBox.Show("Hotel information updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadHotelInformation();
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadHotelInformation();
        }
    }
}
