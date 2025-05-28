#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    public partial class AddGuestIdentityDialog : Window
    {
        private readonly string _guestId;
        private readonly IGuestRepository _guestRepository;
        private string frontDocument;

        public AddGuestIdentityDialog(string guestId, IGuestRepository guestRepository)
        {
            _guestId = guestId;
            _guestRepository = guestRepository;
            InitializeComponent();
        }

        private void UploadFront_Click(object sender, RoutedEventArgs e)
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
                    frontDocument = openFileDialog.FileName;
                    frontImg.Source = new BitmapImage(new Uri(frontDocument));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void SaveRecord_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                string idType = cbIdType.Text;
                string idNumber = txtIdNumber.Text;

                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(idNumber, idType);
                if (!areFieldsEmpty)
                {
                    byte[] idDocumentFront = null;
                    if (!string.IsNullOrEmpty(frontDocument))
                    {
                        idDocumentFront = File.ReadAllBytes(frontDocument);
                    }

                    var guestIdentity = new Domain.Entities.FrontDesk.GuestIdentity
                    {
                        Document = idDocumentFront,
                        IdNumber = idNumber,
                        IdType = idType,
                        GuestId = _guestId
                    };

                    await _guestRepository.AddGuestIdentityAsync(guestIdentity);
                    MessageBox.Show("Guest identity information added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
