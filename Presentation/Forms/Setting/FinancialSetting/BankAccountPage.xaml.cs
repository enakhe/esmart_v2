using ESMART.Application.Common.Interface;
using ESMART.Infrastructure.Repositories.RoomSetting;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }
    }
}
