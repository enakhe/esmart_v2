using ESMART.Application.Common.Utils;
using ESMART.Application.UseCases.Data;
using ESMART.Presentation.Forms;
using System.Text;
using System.Windows;

namespace ESMART.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IdentityUseCases _identityUseCases;
        public MainWindow(IdentityUseCases identityUseCases)
        {
            _identityUseCases = identityUseCases;
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                string email = txtUsername.Text;
                string password = txtPassword.Password;

                bool areCredentialsEmpty = Helper.AreAnyNullOrEmpty(email, password);
                if (!areCredentialsEmpty)
                {
                    var result = await _identityUseCases.LoginUser(email, password);
                    if (!result.Succeeded)
                    {
                        var sb = new StringBuilder();
                        foreach (var item in result.Errors)
                        {
                            sb.AppendLine(item);
                        }

                        MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        Dashboard dashboard = new Dashboard();
                        dashboard.Show();
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Please enter all required fields",
                                    "Invalid",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }

        }
    }
}