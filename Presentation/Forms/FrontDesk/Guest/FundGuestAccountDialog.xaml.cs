using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Infrastructure.Repositories.Transaction;
using ESMART.Presentation.Session;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly ITransactionRepository _transactionRepository;
        public FundGuestAccountDialog(Domain.Entities.FrontDesk.Guest guest, IGuestRepository guestRepository, IBookingRepository bookingRepository, ITransactionRepository transactionRepository)
        {
            InitializeComponent();
            _guestRepository = guestRepository;
            _bookingRepository = bookingRepository;

            _formatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _formatTimer.Tick += FormatTimer_Tick;

            _guest = guest;

            txtAmount.Text = "0.00"; // Initialize with a default value
            _transactionRepository = transactionRepository;
        }

        private async Task LoadGuestAccount()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestAccount = await _guestRepository.GetGuestAccountByGuestIdAsync(_guest.Id);
                if (guestAccount != null)
                {
                    txtAmount.Text = guestAccount.FundedBalance.ToString("N", CultureInfo.InvariantCulture);
                    txtAmount.CaretIndex = txtAmount.Text.Length; // Set caret to the end of the text
                    txtAmount.SelectionStart = txtAmount.Text.Length; // Ensure caret is at the end
                    txtAmount.SelectionLength = 0; // Clear any selection

                    chkResBar.IsChecked = guestAccount.AllowBarAndRes;
                    chkLaundry.IsChecked = guestAccount.AllowLaundry;

                }
                else
                {
                    txtAmount.Text = "0.00"; // Default value if no account found
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
                bool isNull = Helper.AreAnyNullOrEmpty(txtAmount.Text);
                if (isNull)
                {
                    MessageBox.Show("Please enter an amount to fund.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtAmount.Text.Replace(",", ""), out decimal amount))
                {
                    MessageBox.Show("Please enter a valid amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoaderOverlay.Visibility = Visibility.Visible;

                // save the guest account if the amount is higher then current amount and alo create one if the guest has no account
                var existingAccount = await _guestRepository.GetGuestAccountByGuestIdAsync(_guest.Id);
                var activeBooking = await _bookingRepository.GetBookingByGuestId(_guest.Id);

                // Create or update the guest account
                if (existingAccount != null)
                {
                    if (amount > existingAccount.FundedBalance)
                    {
                        existingAccount.FundedBalance += amount;
                        existingAccount.LastFunded = DateTime.Now;
                    }

                    existingAccount.AllowBarAndRes = chkResBar.IsChecked ?? true;
                    existingAccount.AllowLaundry = chkLaundry.IsChecked ?? true;
                    await _guestRepository.UpdateGuestAccountAsync(existingAccount);
                }
                else
                {
                    existingAccount = new GuestAccount
                    {
                        GuestId = _guest.Id,
                        FundedBalance = amount,
                        LastFunded = DateTime.Now,
                        AllowBarAndRes = chkResBar.IsChecked ?? true,
                        AllowLaundry = chkLaundry.IsChecked ?? true
                    };

                    await _guestRepository.FundAccountAsync(existingAccount);
                }

                var guestTransaction = new GuestTransaction
                {
                    GuestId = _guest.Id,
                    Amount = amount,
                    PaymentMethod = (PaymentMethod)cmbPaymentMethod.SelectedValue,
                    BankAccountId = cmbAccountNumber.SelectedValue?.ToString(),
                    TransactionType = TransactionType.Credit,
                    Description = $"Account funded with ₦{amount:N2}",
                    Date = DateTime.Now
                };
                await _guestRepository.AddGuestTransactionAsync(guestTransaction);

                await CreateTransactionItem(activeBooking?.BookingId!, amount, false);

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

        private async Task CreateTransactionItem(string bookingId, decimal amount, bool isUnPaid)
        {
            try
            {
                var transaction = await _transactionRepository.GetByBookingIdAsync(bookingId);
                if (transaction != null)
                {
                    var transactionItem = new TransactionItem()
                    {
                        Id = Guid.NewGuid().ToString(),
                        TransactionId = transaction.Id,
                        Amount = amount,
                        Description = $"Advance payment is {amount} {cmbPaymentMethod.Text}",
                        DateAdded = DateTime.Now,
                        BankAccount = ((BankAccount)cmbAccountNumber.SelectedItem).BankAccountNumber,
                        Invoice = transaction.InvoiceNumber,
                        Category = Category.Deposit,
                        ServiceId = transaction.BookingId,
                        Type = TransactionType.Payment,
                        Status = TransactionStatus.Paid,
                        Discount = 0,
                        TaxAmount = 0,
                        ServiceCharge = 0,
                        TotalAmount = amount,
                        ApplicationUserId = AuthSession.CurrentUser?.Id
                    };

                    if (!isUnPaid)
                    {
                        transactionItem.Status = TransactionStatus.Paid;
                    }

                    await _transactionRepository.AddTransactionItemAsync(transactionItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
