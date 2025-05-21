#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.StoreKeeping;
using ESMART.Domain.ViewModels.StoreKepping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ESMART.Presentation.Forms.StockKeeping.MenuItemRecipe
{
    /// <summary>
    /// Interaction logic for AddMenuItemRecipeDialog.xaml
    /// </summary>
    public partial class AddMenuItemRecipeDialog : Window
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private readonly Domain.Entities.StoreKeeping.MenuItem _menuItem;
        private readonly ObservableCollection<MenuItemRecipeDataGrid> inventoryItems;
        public AddMenuItemRecipeDialog(IStockKeepingRepository stockKeepingRepository, Domain.Entities.StoreKeeping.MenuItem menuItem)
        {
            _stockKeepingRepository = stockKeepingRepository;
            _menuItem = menuItem;
            InitializeComponent();

            inventoryItems = new ObservableCollection<MenuItemRecipeDataGrid>();

        }

        private async Task LoadInventoryItem()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var inventoryItems = await _stockKeepingRepository.GetAllInventoryItemsAsync();
                if (inventoryItems != null)
                {
                    cmbInventory.ItemsSource = inventoryItems;
                    cmbInventory.DisplayMemberPath = "Name";
                    cmbInventory.SelectedValuePath = "Id";
                }
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInventoryItem();
        }

        private void cmbInventory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbInventory.SelectedItem is null)
            {
                return;
            }

            if (cmbInventory.SelectedItem is InventoryViewModel selectedInventoryItem)
            {
                txtUnit.Text = selectedInventoryItem.UnitOfMeasure.ToString();
            }
        }

        private async void AddInvetoryToTable_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (cmbInventory.SelectedItem is null || String.IsNullOrEmpty(txtQuantity.Text))
                {
                    MessageBox.Show("Please add necessary fields", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var inventoryItemViewModel = (InventoryViewModel)cmbInventory.SelectedItem;
                var inventoryItem = await _stockKeepingRepository.GetInventoryItemByIdAsync(inventoryItemViewModel.Id);

                if (inventoryItem != null)
                {
                    var menuItemRecipe = new MenuItemRecipeDataGrid
                    {
                        Id = inventoryItem.Id,
                        Name = inventoryItem.Name,
                        UnitOfMeasure = inventoryItem.UnitOfMeasure.ToString(),
                        Quantity = decimal.Parse(txtQuantity.Text),
                    };

                    inventoryItems.Add(menuItemRecipe);
                    MenuItemRecipeDataGrid.ItemsSource = inventoryItems;

                    cmbInventory.SelectedItem = null;
                    txtUnit.Text = "";
                    txtQuantity.Text = "";
                }
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

        private static bool IsTextNumeric(string text)
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

            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                if (!decimal.TryParse(newText, out _))
                {
                    e.Handled = true;
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                foreach(var inventory in inventoryItems)
                {
                    var menuItemRecipe = new Domain.Entities.StoreKeeping.MenuItemRecipe()
                    {
                        InventoryItemId = inventory.Id,
                        MenuItemId = _menuItem.Id,
                        Quantity = inventory.Quantity,
                        UpdatedAt = DateTime.Now
                    };

                    await _stockKeepingRepository.AddMenuItemRecipeAsync(menuItemRecipe);
                }

                MessageBox.Show("Menu item recipe added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }

    public class MenuItemRecipeDataGrid
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Quantity { get; set; }
    }
}
