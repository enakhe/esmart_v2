using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.StoreKeeping;
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

namespace ESMART.Presentation.Forms.StockKeeping.Inventory
{
    /// <summary>
    /// Interaction logic for AddInventoryDialog.xaml
    /// </summary>
    public partial class AddInventoryDialog : Window
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        public AddInventoryDialog(IStockKeepingRepository stockKeepingRepository)
        {
            _stockKeepingRepository = stockKeepingRepository;
            InitializeComponent();
        }

        private void LoadUnitOfMeasurement()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var area = Enum.GetValues<InventoryUnit>()
                    .Cast<InventoryUnit>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbMeasurement.ItemsSource = area;
                cmbMeasurement.DisplayMemberPath = "Name";
                cmbMeasurement.SelectedValuePath = "Name";
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(txtName.Text, txtQuantity.Text, cmbMeasurement.Text, txtReorderLevel.Text, txtReorderQuantity.Text);

                if (areFieldsEmpty)
                {
                    MessageBox.Show("Please fill all fields", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!decimal.TryParse(txtQuantity.Text.Replace(",", ""), out decimal quantity))
                {
                    MessageBox.Show("Invalid quantity format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool isChecked = (bool)chkIsActive.IsChecked!;
                var inventory = new InventoryItem
                {
                    Name = txtName.Text,
                    Description = txtDescription.Text,
                    Quantity = quantity,
                    UnitOfMeasure = Enum.Parse<InventoryUnit>(cmbMeasurement.Text),
                    ReorderLevel = decimal.Parse(txtReorderLevel.Text),
                    ReorderQuantity = decimal.Parse(txtReorderQuantity.Text),
                    IsActive = isChecked
                };

                await _stockKeepingRepository.AddInventoryItemAsync(inventory);
                MessageBox.Show("Inventory item added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUnitOfMeasurement();
        }

        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        // Format text input number with comma
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
    }
}
