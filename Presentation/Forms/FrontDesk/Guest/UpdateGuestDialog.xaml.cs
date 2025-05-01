using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Presentation.Session;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    public partial class UpdateGuestDialog : Window
    {
        private readonly string _id;
        private readonly IGuestRepository _guestRepository;
        private string profilePictureImage;

        public UpdateGuestDialog(string id, IGuestRepository guestRepository)
        {
            _id = id;
            _guestRepository = guestRepository;
            InitializeComponent();
        }

        public byte[]? ImageSourceToByteArray(ImageSource imageSource)
        {
            if (imageSource is BitmapSource bitmapSource)
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    return ms.ToArray();
                }
            }
            return null;
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
                    imgProfileImg.Source = new BitmapImage(new Uri(profilePictureImage));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                string firstName = txtFirstName.Text;
                string middleName = txtMiddleName.Text;
                string lastName = txtLastName.Text;
                string email = txtEmail.Text;
                string phoneNumber = txtPhoneNumber.Text;
                string gender = cbGender.Text;

                string street = txtStreet.Text;
                string city = txtCity.Text;
                string state = txtState.Text;
                string country = txtCountry.Text;

                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(firstName, middleName, lastName, phoneNumber, gender, city, state, country);
                if (!areFieldsEmpty)
                {
                    var result = await _guestRepository.GetGuestByIdAsync(_id);
                    Domain.Entities.FrontDesk.Guest guest = result.Response;
                    guest.FirstName = firstName.ToUpper();
                    guest.MiddleName = middleName.ToUpper();
                    guest.LastName = lastName.ToUpper();
                    guest.Email = email;
                    guest.PhoneNumber = phoneNumber;
                    guest.Gender = gender;
                    guest.Street = street;
                    guest.City = city;
                    guest.State = state;
                    guest.Country = country;
                    guest.GuestImage = ImageSourceToByteArray(imgProfileImg.Source);
                    guest.DateModified = DateTime.Now;
                    guest.UpdatedBy = AuthSession.CurrentUser?.Id;

                    var updateResult = await _guestRepository.UpdateGuestAsync(guest);
                    if (updateResult.Succeeded)
                    {
                        MessageBox.Show("Guest updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        UpdateGuestIdentityDialog updateGuestIdentityDialog = new UpdateGuestIdentityDialog(guest.Id, _guestRepository);
                        updateGuestIdentityDialog.ShowDialog();
                        this.DialogResult = true;
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        foreach (var item in updateResult.Errors)
                        {
                            sb.AppendLine(item);
                        }

                        MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter all required fields",
                                    "Invalid",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
