using ESMART.Presentation.Forms.Home;
using System.Windows;

namespace ESMART.Presentation.Forms
{
    public partial class Dashboard : Window
    {
        private bool _isLoading;
        public Dashboard()
        {
            InitializeComponent();
            MainFrame.Navigate(new IndexPage());
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                LoaderGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new IndexPage());
        }
    }
}
