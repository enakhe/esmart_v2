using ESMART.Application.Common.Interface;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.Setting.OperationalSetting
{
    /// <summary>
    /// Interaction logic for OperationSettingPage.xaml
    /// </summary>
    public partial class OperationSettingPage : Page
    {
        private readonly IHotelSettingsService _hotelSettingsService;
        public OperationSettingPage(IHotelSettingsService hotelSettingsService)
        {
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        private async Task LoadLockSetting()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var operationSettings = await _hotelSettingsService.GetSettingsByCategoryAsync("Operation Settings");
                if (operationSettings != null)
                {
                    foreach (var setting in operationSettings)
                    {
                        switch (setting.Key)
                        {
                            case "LockType":
                                cmbLockType.Text = setting.Value;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading lock type settings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var lockType = cmbLockType.Text;

                var setting = await _hotelSettingsService.GetSettingAsync("LockType");

                if (setting != null)
                {
                    setting.Value = lockType;

                    var result = await _hotelSettingsService.UpdateSettingAsync(setting);

                    if (result)
                    {
                        MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to save settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving settings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLockSetting();
        }
    }
}
