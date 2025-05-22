using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.ViewModels.Transaction
{
    public class TransactionPageViewModel
    {
        public ObservableCollection<TransactionViewModel> Transactions { get; set; } = new();
    }
}
