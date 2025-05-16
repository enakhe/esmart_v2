using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Transaction;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.Setting.FinancialSetting
{
    /// <summary>
    /// Interaction logic for BankAccountPage.xaml
    /// </summary>
    public partial class BankAccountPage : Page
    {
        private readonly ITransactionRepository _transactionRepository;
        public BankAccountPage(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
            InitializeComponent();
        }

        private async Task LoadData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var bankAccounts = await _transactionRepository.GetAllBankAccountAsync();
                BankAccountDataGrid.ItemsSource = bankAccounts;
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addBankAccountDialog = new AddBankAccountDialog(_transactionRepository);
            if (addBankAccountDialog.ShowDialog() == true)
            {
                await LoadData();
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedBuilding = (BankAccount)BankAccountDataGrid.SelectedItem;

                if (selectedBuilding.Id != null)
                {
                    var bankAccount = await _transactionRepository.GetBankAccountById(selectedBuilding.Id);

                    var updateBankAccountt = new UpdateBankAccount(bankAccount, _transactionRepository);
                    if (updateBankAccountt.ShowDialog() == true)
                    {
                        await LoadData();
                    }
                }
                else
                {
                    MessageBox.Show("Please select an account before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void DeleteBankAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedBuilding = (BankAccount)BankAccountDataGrid.SelectedItem;

                    if (selectedBuilding.Id != null)
                    {
                        MessageBoxResult messageResult = MessageBox.Show("Are you sure you want to delete this bank account?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (messageResult == MessageBoxResult.Yes)
                        {
                            LoaderOverlay.Visibility = Visibility.Visible;
                            var bankAccount = await _transactionRepository.GetBankAccountById(selectedBuilding.Id);
                            await _transactionRepository.DeleteBankAccountAsync(bankAccount);

                            await LoadData();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select an account before deleting.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }
    }
}
