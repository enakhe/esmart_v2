using ESMART.Application.Common.Interface;
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
    /// Interaction logic for UpdateInventoryDialog.xaml
    /// </summary>
    public partial class UpdateInventoryDialog : Window
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private readonly Domain.Entities.StoreKeeping.InventoryItem _inventoryItem;
        public UpdateInventoryDialog(IStockKeepingRepository stockKeepingRepository, Domain.Entities.StoreKeeping.InventoryItem inventoryItem)
        {
            _stockKeepingRepository = stockKeepingRepository;
            _inventoryItem = inventoryItem;
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
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Please enter a valid name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (cmbMeasurement.SelectedItem == null)
                {
                    MessageBox.Show("Please select a unit of measurement.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _inventoryItem.Name = txtName.Text.Trim();
                _inventoryItem.UnitOfMeasure = (InventoryUnit)cmbMeasurement.SelectedValue;
                await _stockKeepingRepository.UpdateInventoryItemAsync(_inventoryItem);
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
            txtName.Text = _inventoryItem.Name;
            txtQuantity.Text = _inventoryItem.Quantity.ToString();
            txtDescription.Text = _inventoryItem.Description;
            cmbMeasurement.SelectedValue = _inventoryItem.UnitOfMeasure;
            chkIsActive.IsChecked = _inventoryItem.IsActive;
            txtReorderLevel.Text = _inventoryItem.ReorderLevel.ToString();
            txtReorderQuantity.Text = _inventoryItem.ReorderQuantity.ToString();
            txtName.Focus();
            txtName.SelectAll();

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
