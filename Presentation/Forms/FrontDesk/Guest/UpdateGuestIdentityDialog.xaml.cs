#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    public partial class UpdateGuestIdentityDialog : Window
    {
        private readonly string _guestId;
        private readonly IGuestRepository _guestRepository;
        private string frontDocument;
        private string backDocument;

        public UpdateGuestIdentityDialog(string guest, IGuestRepository guestRepository)
        {
            _guestId = guest;
            _guestRepository = guestRepository;
            InitializeComponent();
        }

        public byte[] ImageSourceToByteArray(ImageSource imageSource)
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

        private void UploadBack_Click(object sender, RoutedEventArgs e)
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
                    backDocument = openFileDialog.FileName;
                    backImg.Source = new BitmapImage(new Uri(backDocument));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void LoadGuestIdentity()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestIdentity = await _guestRepository.GetGuestIdentityByGuestIdAsync(_guestId);
                if (guestIdentity != null)
                {
                    cbIdType.Text = guestIdentity.IdType;
                    txtIdNumber.Text = guestIdentity.IdNumber;
                    if (guestIdentity.IdentificationDocumentFront != null && guestIdentity.IdentificationDocumentFront.Length > 0)
                    {
                        using (var ms = new MemoryStream(guestIdentity.IdentificationDocumentFront))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                            frontImg.Source = bitmap;
                        }
                    }

                    if (guestIdentity.IdentificationDocumentBack != null && guestIdentity.IdentificationDocumentBack.Length > 0)
                    {
                        using (var ms = new MemoryStream(guestIdentity.IdentificationDocumentBack))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                            backImg.Source = bitmap;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Guest identity not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void UpdateGuestIdentity_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGuestIdentity();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                string idType = cbIdType.Text;
                string idNumber = txtIdNumber.Text;

                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(idNumber, idType);
                if (!areFieldsEmpty)
                {
                    var guestIdentity = await _guestRepository.GetGuestIdentityByGuestIdAsync(_guestId);
                    if (guestIdentity != null)
                    {
                        guestIdentity.IdType = idType;
                        guestIdentity.IdNumber = idNumber;
                        guestIdentity.IdentificationDocumentFront = ImageSourceToByteArray(frontImg.Source);
                        guestIdentity.IdentificationDocumentBack = ImageSourceToByteArray(backImg.Source);

                        await _guestRepository.UpdateGuestIdentityAsync(guestIdentity);
                        MessageBox.Show("Guest identity information updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
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