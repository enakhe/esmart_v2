using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Presentation.Utils;
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

namespace ESMART.Presentation.Forms.Setting.Licence
{
    /// <summary>
    /// Interaction logic for LicenceDialog.xaml
    /// </summary>
    public partial class LicenceDialog : Window
    {
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly ILicenceRepository _licenceRepository;
        public LicenceDialog(IHotelSettingsService hotelSettingsService, ILicenceRepository licenceRepository)
        {
            _hotelSettingsService = hotelSettingsService;
            _licenceRepository = licenceRepository;
            InitializeComponent();
        }

        private async Task IsEligibleForFreeTrial()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var freeTrialSettings = await _hotelSettingsService.GetSettingAsync("FreeTrial");
                if(freeTrialSettings != null)
                {
                    var value = freeTrialSettings.Value;
                    if (value != null && value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FreeTrialButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        FreeTrialButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while checking eligibility for free trial: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void txtLicenceKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = txtLicenceKey.Text.Replace("-", "");

            if(text.Length > 24)
            {
                text = text.Substring(0, 24);
            }

            var formattedText = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && i % 6 == 0)
                {
                    formattedText.Append('-');
                }
                formattedText.Append(text[i]);
            }

            txtLicenceKey.TextChanged -= txtLicenceKey_TextChanged;
            txtLicenceKey.Text = formattedText.ToString();
            txtLicenceKey.SelectionStart = txtLicenceKey.Text.Length;
            txtLicenceKey.TextChanged += txtLicenceKey_TextChanged;
        }

        private async void FreeTrialButton_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            var hotel = await _hotelSettingsService.GetHotelInformation();

            if (hotel != null)
            {
                var hotelName = "Free Trial" + random.Next(100000, 200000);
                string productKey = LicenceHelper.GenerateProductKey(hotelName, DateTime.Now.AddDays(7));

                if (productKey != null)
                {
                    txtHotelName.Text = hotelName;
                    txtLicenceKey.Text = productKey;

                    txtHotelName.IsReadOnly = true;
                    txtLicenceKey.IsReadOnly = true;
                }
            }
            else
            {
                MessageBox.Show("Hotel information not found. Please set up hotel information first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(txtHotelName.Text, txtLicenceKey.Text);
                if (isNull)
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (txtLicenceKey.Text.Length != 27)
                    {
                        MessageBox.Show("Invalid product key", "Invalid product key", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                    }
                    else
                    {
                        bool isValid = LicenceHelper.ValidateProductKey(txtHotelName.Text, txtLicenceKey.Text);
                        if(isValid)
                        {

                            string expDatStr = LicenceHelper.GetExpirationDate(txtLicenceKey.Text)!;

                            DateTime.TryParseExact(expDatStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime expirationDate);

                            if (txtHotelName.Text != "Default Hotel Name")
                            {
                                var licenceInfo = new Domain.Entities.Configuration.LicenceInformation
                                {
                                    LicenceKey = txtLicenceKey.Text,
                                    ExpiryDate = expirationDate,
                                    HotelName = txtHotelName.Text,
                                    CreatedDate = DateTime.Now,
                                    LastUpdatedDate = DateTime.Now
                                };

                                await _licenceRepository.AddLicenceAsync(licenceInfo);
                                SecureFileHelper.SaveSecureFile(txtHotelName.Text, txtLicenceKey.Text, expirationDate);

                                this.DialogResult = true;

                            }
                            else
                            {
                                var freeTrialSettings = await _hotelSettingsService.GetSettingAsync("FreeTrial");
                                if (freeTrialSettings != null)
                                {
                                    freeTrialSettings.Value = "False";
                                    await _hotelSettingsService.UpdateSettingAsync(freeTrialSettings);
                                }

                                this.DialogResult = true;
                            }

                             MessageBox.Show("Product Key was successfully validated", "Success", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Invalid product key", "Invalid product key", MessageBoxButton.OK,                                                     MessageBoxImage.Warning);
                        }

                           
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving license information: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            await IsEligibleForFreeTrial();
        }
    }
}
