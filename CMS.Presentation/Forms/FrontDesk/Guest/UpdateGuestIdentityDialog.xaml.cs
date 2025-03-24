using ESMART.Application.Common.Utils;
using ESMART.Application.Interface;
using ESMART.Domain.Entities.FrontDesk;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
                var result = await _guestRepository.GetGuestIdentityByGuestIdAsync(_guestId);
                if (result.Succeeded)
                {
                    cbIdType.Text = result.Response.IdType;
                    txtIdNumber.Text = result.Response.IdNumber;
                    if (result.Response.IdentificationDocumentFront != null && result.Response.IdentificationDocumentFront.Length > 0)
                    {
                        using (var ms = new MemoryStream(result.Response.IdentificationDocumentFront))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                            frontImg.Source = bitmap;
                        }
                    }

                    if (result.Response.IdentificationDocumentBack != null && result.Response.IdentificationDocumentBack.Length > 0)
                    {
                        using (var ms = new MemoryStream(result.Response.IdentificationDocumentBack))
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
                    var result = await _guestRepository.GetGuestIdentityByGuestIdAsync(_guestId);
                    Domain.Entities.FrontDesk.GuestIdentity guestIdentity = result.Response;
                    if (result.Succeeded)
                    {
                        guestIdentity.IdType = idType;
                        guestIdentity.IdNumber = idNumber;
                        guestIdentity.IdentificationDocumentFront = ImageSourceToByteArray(frontImg.Source);
                        guestIdentity.IdentificationDocumentBack = ImageSourceToByteArray(backImg.Source);

                        var updateResult = await _guestRepository.UpdateGuestIdentityAsync(guestIdentity);
                        if (updateResult.Succeeded)
                        {
                            MessageBox.Show("Guest updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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