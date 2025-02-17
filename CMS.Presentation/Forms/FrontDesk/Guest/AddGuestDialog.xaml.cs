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
using Microsoft.Win32;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ESMART.Application.Common;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Application.Interface;
using ESMART.Presentation.Session;

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

        private void UploadMugShot_Click(object sender, RoutedEventArgs e)
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

                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(firstName, middleName, lastName, email, phoneNumber, gender, city, state, country);
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
                        Status = "inactive",
                        GuestImage = profileImageBytes,
                        IsTrashed = false,
                        CreatedBy = AuthSession.CurrentUser.Id
                    };
                    await _guestRepository.AddGuestAsync(guest);
                    MessageBox.Show("Guest added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please enter all required fields",
                                    "Invalid",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
        }
            catch(Exception ex)
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
    }
}
