#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Presentation.Session;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    public partial class AddGuestDialog : Window
    {
        private string profilePictureImage;
        private readonly IGuestRepository _guestRepository;
        public AddGuestDialog(IGuestRepository guestRepository)
        {
            _guestRepository = guestRepository;
            InitializeComponent();
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

        private async void Button_Click(object sender, RoutedEventArgs e)
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

                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(firstName, lastName, phoneNumber, gender, city, state, country);
                if (!areFieldsEmpty)
                {
                    byte[] profileImageBytes = null;
                    if (!string.IsNullOrEmpty(profilePictureImage))
                    {
                        profileImageBytes = File.ReadAllBytes(profilePictureImage);
                    }

                    var guest = new Domain.Entities.FrontDesk.Guest
                    {
                        GuestId = "GU" + Guid.NewGuid().ToString().Split("-")[0].ToUpper().Substring(0, 3),
                        FirstName = firstName.ToUpper(),
                        MiddleName = middleName.ToUpper(),
                        LastName = lastName.ToUpper(),
                        Email = email,
                        PhoneNumber = phoneNumber,
                        Gender = gender,
                        Street = street,
                        City = city,
                        State = state,
                        Country = country,
                        Status = "Inactive",
                        GuestImage = profileImageBytes,
                        IsTrashed = false,
                        ApplicationUserId = AuthSession.CurrentUser?.Id
                    };

                    await _guestRepository.AddGuestAsync(guest);

                    MessageBox.Show("Guest added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    AddGuestIdentityDialog addGuestIdentityDialog = new AddGuestIdentityDialog(guest, _guestRepository);
                    addGuestIdentityDialog.ShowDialog();
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please enter all required fields",
                                    "Invalid",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
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

        private void ButtonCancle_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
