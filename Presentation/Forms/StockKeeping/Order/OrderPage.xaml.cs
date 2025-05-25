using ESMART.Application.Common.Interface;
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

namespace ESMART.Presentation.Forms.StockKeeping.Order
{
    /// <summary>
    /// Interaction logic for OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly ITransactionRepository _transactionRepository;
        public OrderPage(IStockKeepingRepository stockKeepingRepository, IBookingRepository bookingRepository, IGuestRepository guestRepository, ITransactionRepository transactionRepository)
        {
            _stockKeepingRepository = stockKeepingRepository;
            _bookingRepository = bookingRepository;
            _transactionRepository = transactionRepository;
            _guestRepository = guestRepository;
            InitializeComponent();
        }

        // Add order
        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            OrderDialog orderDialog = new OrderDialog(_stockKeepingRepository, _bookingRepository, _guestRepository, _transactionRepository)
            {
                Owner = Window.GetWindow(this)
            };
            orderDialog.ShowDialog();
            MessageBox.Show("Add Order button clicked!");
        }
    }
}
