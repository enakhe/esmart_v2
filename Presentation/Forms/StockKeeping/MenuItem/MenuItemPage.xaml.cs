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

namespace ESMART.Presentation.Forms.StockKeeping.MenuItem
{
    /// <summary>
    /// Interaction logic for MenuItemPage.xaml
    /// </summary>
    public partial class MenuItemPage : Page
    {
        public MenuItemPage()
        {
            InitializeComponent();
        }

        public void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var addMenuItemDialog = new AddMenuItemDialog();
            addMenuItemDialog.ShowDialog();
        }
    }
}
