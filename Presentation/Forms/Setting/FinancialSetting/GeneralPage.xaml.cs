using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Configuration;
using ESMART.Presentation.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.Setting.FinancialSetting
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class General : Page
    {
        private readonly IHotelSettingsService _hotelSettingsService;
        public General(IHotelSettingsService hotelSettingsService)
        {
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        private void LoadCurrencyOption()
        {
            var currencies = new List<CurrencyOption>
            {
                new CurrencyOption { Symbol = "₦", Code = "NGN" },
                new CurrencyOption { Symbol = "$", Code = "USD" },
                new CurrencyOption { Symbol = "€", Code = "EUR" },
                new CurrencyOption { Symbol = "£", Code = "GBP" },
                new CurrencyOption { Symbol = "¥", Code = "JPY" },
                new CurrencyOption { Symbol = "₵", Code = "GHS" },
                new CurrencyOption { Symbol = "R", Code = "ZAR" },
            };

            cmbCurrency.ItemsSource = currencies;
            cmbCurrency.DisplayMemberPath = "Display";
            cmbCurrency.SelectedValuePath = "Code";
        }

        private async Task LoadFiancialData()
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;
                var financialSettings = await _hotelSettingsService.GetSettingsByCategoryAsync("Financial Settings");
                if (financialSettings != null)
                {
                    foreach (var setting in financialSettings)
                    {
                        switch (setting.Key)
                        {
                            case "VAT":
                                txtVAT.Text = setting.Value;
                                break;
                            case "ServiceCharge":
                                txtServicCharge.Text = setting.Value;
                                break;
                            case "Discount":
                                txtDiscount.Text = setting.Value;
                                break;
                            case "CurrencySymbol":
                                cmbCurrency.SelectedValue = setting.Value;
                                break;
                            case "RefundPercent":
                                txtRefundPercent.Text = setting.Value;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading financial data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;

                var vat = txtVAT.Text.Replace("%", "").Trim();
                var serviceCharge = txtServicCharge.Text.Replace("%", "").Trim();
                var discount = txtDiscount.Text;
                var currencySymbol = cmbCurrency.SelectedValue.ToString();
                var refundPercent = txtRefundPercent.Text.Replace("%", "").Trim();

                var vatSetting = await _hotelSettingsService.GetSettingAsync("VAT");
                var serviceChargeSetting = await _hotelSettingsService.GetSettingAsync("ServiceCharge");
                var discountSetting = await _hotelSettingsService.GetSettingAsync("Discount");
                var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");
                var refundSetting = await _hotelSettingsService.GetSettingAsync("RefundPercent");

                if (vatSetting == null || serviceChargeSetting == null || discountSetting == null || currencySetting == null || refundSetting == null)
                {
                    MessageBox.Show("One or more settings not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                vatSetting.Value = vat;
                serviceChargeSetting.Value = serviceCharge;
                discountSetting.Value = discount;
                currencySetting.Value = currencySymbol!;
                refundSetting.Value = refundPercent;

                await _hotelSettingsService.UpdateSettingAsync(vatSetting);
                await _hotelSettingsService.UpdateSettingAsync(serviceChargeSetting);
                await _hotelSettingsService.UpdateSettingAsync(discountSetting);
                await _hotelSettingsService.UpdateSettingAsync(currencySetting);
                await _hotelSettingsService.UpdateSettingAsync(refundSetting);

                MessageBox.Show("Financial settings updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadFiancialData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving financial settings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        // VAT
        private void VatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputFormatter.AllowDecimalOnly(sender, e);
        }

        private void VatTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.FormatAsPercentageOnLostFocus(sender, e);
        }

        private void VatTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.StripPercentageOnGotFocus(sender, e);
        }


        // Service Charge
        private void ServiceChargeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputFormatter.AllowDecimalOnly(sender, e);
        }

        private void ServiceChargeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.FormatAsPercentageOnLostFocus(sender, e);
        }

        private void ServiceChargeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.StripPercentageOnGotFocus(sender, e);
        }


        // Discount
        private void DiscountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputFormatter.AllowDecimalOnly(sender, e);
        }

        private void DiscountTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.FormatAsDecimalOnLostFocus(sender, e);
        }

        private void DiscountTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.StripPercentageOnGotFocus(sender, e);
        }

        private void txtDiscount_GotFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.StripPercentageOnGotFocus(sender, e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrencyOption();
            await LoadFiancialData();
        }
    }
}
