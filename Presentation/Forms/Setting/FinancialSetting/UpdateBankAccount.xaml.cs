using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Transaction;
using System.Windows;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.Setting.FinancialSetting
{
    /// <summary>
    /// Interaction logic for UpdateBankAccount.xaml
    /// </summary>
    public partial class UpdateBankAccount : Window
    {
        private readonly BankAccount _bankAccount;
        private readonly ITransactionRepository _transactionRepository;
        public UpdateBankAccount(BankAccount bankAccount, ITransactionRepository transactionRepository)
        {
            _bankAccount = bankAccount;
            _transactionRepository = transactionRepository;
            InitializeComponent();
        }

        public void LoadData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                txtBankAccountName.Text = _bankAccount.BankAccountName;
                txtBankAccountNumber.Text = _bankAccount.BankAccountNumber;
                txtBankName.Text = _bankAccount.BankName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void AddRoomButton_Click(object sender, RoutedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtBankAccountName.Text, txtBankAccountNumber.Text, txtBankName.Text);

            if (!isNull)
            {
                _bankAccount.BankName = txtBankName.Text;
                _bankAccount.BankAccountNumber = txtBankAccountNumber.Text;
                _bankAccount.BankAccountName = txtBankAccountName.Text;

                await _transactionRepository.UpdateBankAccountAsync(_bankAccount);

                MessageBox.Show("Bank account added updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}
