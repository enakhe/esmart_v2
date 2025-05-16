using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Transaction;
using System.Windows;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.Setting.FinancialSetting
{
    /// <summary>
    /// Interaction logic for AddBankAccountDialog.xaml
    /// </summary>
    public partial class AddBankAccountDialog : Window
    {
        private readonly ITransactionRepository _transactionRepository;
        public AddBankAccountDialog(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
            InitializeComponent();
        }

        private async void AddRoomButton_Click(object sender, RoutedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtBankAccountName.Text, txtBankAccountNumber.Text, txtBankName.Text);

            if (!isNull)
            {
                var bankAccount = new BankAccount()
                {
                    BankAccountName = txtBankAccountName.Text,
                    BankAccountNumber = txtBankAccountNumber.Text,
                    BankName = txtBankName.Text,
                };

                await _transactionRepository.AddBankAccountAsync(bankAccount);

                MessageBox.Show("Bank account added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
