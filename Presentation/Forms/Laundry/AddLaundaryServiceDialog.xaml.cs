using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Laundry;
using ESMART.Domain.Enum;
using ESMART.Infrastructure.Services;
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
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.Laundry
{
    /// <summary>
    /// Interaction logic for AddLaundaryServiceDialog.xaml
    /// </summary>
    public partial class AddLaundaryServiceDialog : Window
    {
        private readonly GuestAccountService _guestAccountService;
        private readonly Domain.Entities.Laundry.Laundry _laundry;
        public AddLaundaryServiceDialog(GuestAccountService guestAccountService, Domain.Entities.Laundry.Laundry laundry)
        {
            _guestAccountService = guestAccountService;
            _laundry = laundry;
            InitializeComponent();
        }

        private void CancleButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public void LoadLaundary()
        {
            txtName.Text = _laundry.Description;
            txtPressing.Text = _laundry.PressingPrice.ToString();
            txtProduction.Text = _laundry.LaundryPrice.ToString();
            cmbCategory.SelectedItem = _laundry.Category;
        }

        public void LoadPaymentMethod()
        {
            try
            {
                var method = Enum.GetValues<LaundaryCategory>()
                    .Cast<LaundaryCategory>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbCategory.ItemsSource = method;
                cmbCategory.DisplayMemberPath = "Name";
                cmbCategory.SelectedValuePath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var itemName = txtName.Text;
                var category = cmbCategory.SelectedValue.ToString()!;
                var laundaryProduction = txtProduction.Text;
                var pressing = txtPressing.Text;

                bool isNull = Helper.AreAnyNullOrEmpty(itemName, category, laundaryProduction, pressing);

                if(!isNull)
                {
                    if (_laundry == null)
                    {
                        var laundaryItemdto = new LaundryServiceDto()
                        {
                            Category = Enum.Parse<LaundaryCategory>(cmbCategory.SelectedValue.ToString()!),
                            Description = itemName,
                            LaundryPrice = decimal.Parse(laundaryProduction),
                            PressingPrice = decimal.Parse(pressing)
                        };

                        await _guestAccountService.AddLaundaryServiceItemAsync(laundaryItemdto);
                        MessageBox.Show("Successfully save laundary servcice", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                    }
                    else
                    {
                        _laundry.Description = itemName;
                        _laundry.Category = Enum.Parse<LaundaryCategory>(cmbCategory.SelectedValue.ToString()!);
                        _laundry.PressingPrice = decimal.Parse(pressing);
                        _laundry.LaundryPrice = decimal.Parse(pressing);

                        await _guestAccountService.UpdateLaundaryService(_laundry);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured when adding laundary service. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ",")
            {
                e.Handled = true;
                return;
            }

            if (!IsTextNumeric(e.Text))
            {
                e.Handled = true;
                return;
            }

            TextBox? textBox = sender as TextBox;
            if (textBox != null)
            {
                string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                if (!decimal.TryParse(newText, out _))
                {
                    e.Handled = true;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPaymentMethod();

            if(_laundry != null)
            {
                LoadLaundary();
            }
        }
    }
}
