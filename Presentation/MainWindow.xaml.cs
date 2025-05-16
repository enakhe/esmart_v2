using ESMART.Application.Common.Utils;
using ESMART.Application.UseCases.Data;
using ESMART.Presentation.Forms;
using ESMART.Presentation.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IdentityUseCases _identityUseCases;
        private IServiceProvider _serviceProvider;
        public MainWindow(IdentityUseCases identityUseCases)
        {
            _identityUseCases = identityUseCases;
            InitializeComponent();
        }

        private void InitializeServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services, configuration);
            _serviceProvider = services.BuildServiceProvider();
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
                        AuthSession.CurrentUser = result.Response;

                        InitializeServices();

                        Dashboard dashboard = _serviceProvider.GetRequiredService<Dashboard>();
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

        private void chkPassword_Checked(object sender, RoutedEventArgs e)
        {
            passwordTextBox.Text = txtPassword.Password;
            PasswordTextPanel.Visibility = Visibility.Visible;
            PasswordPanel.Visibility = Visibility.Collapsed;
        }

        private void chkPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = passwordTextBox.Text;
            PasswordTextPanel.Visibility = Visibility.Collapsed;
            PasswordPanel.Visibility = Visibility.Visible;
        }

        private void passwordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPassword.Password = passwordTextBox.Text;
        }
    }
}