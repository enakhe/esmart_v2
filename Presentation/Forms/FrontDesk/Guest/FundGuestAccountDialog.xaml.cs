using DocumentFormat.OpenXml.Bibliography;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Infrastructure.Repositories.Transaction;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using System.Windows.Threading;

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    /// <summary>
    /// Interaction logic for FundGuestAccountDialog.xaml
    /// </summary>
    public partial class FundGuestAccountDialog : Window
    {
        private DispatcherTimer _formatTimer;
        private bool _suppressTextChanged = false;
        private Domain.Entities.FrontDesk.Guest _guest;
        private readonly IGuestRepository _guestRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly GuestAccountService _guestAccountService;
        private readonly ITransactionRepository _transactionRepository;
        private IServiceProvider _serviceProvider;
        public FundGuestAccountDialog(Domain.Entities.FrontDesk.Guest guest, IGuestRepository guestRepository, IBookingRepository bookingRepository, ITransactionRepository transactionRepository, GuestAccountService gustAccountService)
        {
            InitializeComponent();
            InitializeServices();
            _guestRepository = guestRepository;
            _bookingRepository = bookingRepository;
            _guestAccountService = gustAccountService;

            _formatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _formatTimer.Tick += FormatTimer_Tick;

            _guest = guest;

            txtAmount.Text = "0.00"; // Initialize with a default value
            _transactionRepository = transactionRepository;
        }

        private void InitializeServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services, configuration);
            _serviceProvider = services.BuildServiceProvider();
        }

        private async Task LoadGuestAccount()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestAccount = await _guestAccountService.GetAccountAsync(_guest.Id);
                if (guestAccount != null)
                {
                    txtAmount.CaretIndex = txtAmount.Text.Length; 
                    txtAmount.SelectionStart = txtAmount.Text.Length;
                    txtAmount.SelectionLength = 0;

                    chkResBar.IsChecked = guestAccount.AllowBarAndRes;
                    chkLaundry.IsChecked = guestAccount.AllowLaundry;
                }
                else
                {
                    txtAmount.Text = "0.00";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the guest account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void DecimalInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressTextChanged) return;

            _formatTimer.Stop(); // restart timer
            _formatTimer.Tag = sender;
            _formatTimer.Start();
        }

        private void FormatTimer_Tick(object sender, EventArgs e)
        {
            _formatTimer.Stop();

            var textBox = _formatTimer.Tag as TextBox;
            if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text)) return;

            int caretIndex = textBox.CaretIndex;
            string unformatted = textBox.Text.Replace(",", "");

            if (decimal.TryParse(unformatted, out decimal value))
            {
                _suppressTextChanged = true;

                textBox.Text = string.Format(CultureInfo.InvariantCulture, "{0:N}", value);
                textBox.CaretIndex = Math.Min(caretIndex, textBox.Text.Length);

                _suppressTextChanged = false;
            }
        }

        public void LoadPaymentMethod()
        {
            try
            {
                var method = Enum.GetValues<PaymentMethod>()
                    .Cast<PaymentMethod>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbPaymentMethod.ItemsSource = method;
                cmbPaymentMethod.DisplayMemberPath = "Name";
                cmbPaymentMethod.SelectedValuePath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadPaymentType()
        {
            try
            {
                var method = Enum.GetValues<PaymentType>()
                    .Cast<PaymentType>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbPaymentMethod.ItemsSource = method;
                cmbPaymentMethod.DisplayMemberPath = "Name";
                cmbPaymentMethod.SelectedValuePath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadBankAccount()
        {
            try
            {
                var accountNumber = await _transactionRepository.GetAllBankAccountAsync();

                cmbAccountNumber.ItemsSource = accountNumber;
                cmbAccountNumber.DisplayMemberPath = "BankAccountNumber";
                cmbAccountNumber.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void FundAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(txtAmount.Text, ((BankAccount)cmbAccountNumber.SelectedItem).Id, cmbPaymentMethod.SelectedValue.ToString()!, cmbTopUpType.SelectedValue.ToString()!);
                if (isNull)
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtAmount.Text.Replace(",", ""), out decimal amount))
                {
                    MessageBox.Show("Please enter a valid amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoaderOverlay.Visibility = Visibility.Visible;

                var paymentMethod = Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!);
                var bankAccountId = cmbAccountNumber.SelectedValue.ToString();
                var paymentType = Enum.Parse<PaymentType>(cmbTopUpType.SelectedValue.ToString()!);
                var userId = AuthSession.CurrentUser.Id;

                await _guestAccountService.ToUpAsync(_guest.Id, amount, paymentMethod, bankAccountId!, userId, paymentType);

                LoaderOverlay.Visibility = Visibility.Collapsed;
                MessageBox.Show("Account saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
                MessageBox.Show($"An error occurred while funding the account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtGuest.Text = _guest.FullName;
            await LoadGuestAccount();
            LoadPaymentMethod();
            await LoadBankAccount();
        }
    }
}
