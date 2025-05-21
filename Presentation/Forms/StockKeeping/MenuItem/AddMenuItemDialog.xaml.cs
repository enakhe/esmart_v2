using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.StoreKeeping;
using ESMART.Domain.Enum;
using ESMART.Presentation.Forms.StockKeeping.MenuItemRecipe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.StockKeeping.MenuItem
{
    /// <summary>
    /// Interaction logic for AddMenuItemDialog.xaml
    /// </summary>
    public partial class AddMenuItemDialog : Window
    {
        private bool _suppressTextChanged = false;
        private readonly IStockKeepingRepository _stockKeepingRepository;
        public AddMenuItemDialog(IStockKeepingRepository stockKeepingRepository)
        {
            _stockKeepingRepository = stockKeepingRepository;
            InitializeComponent();

            stkName.Visibility = Visibility.Collapsed;
        }

        public void LoadServiceArea()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var area = Enum.GetValues<ServiceArea>()
                    .Cast<ServiceArea>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbServiceArea.ItemsSource = area;
                cmbServiceArea.DisplayMemberPath = "Name";
                cmbServiceArea.SelectedValuePath = "Name";
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

        // Load inventory item to cmbName Combobox
        private async Task LoadInventoryItem()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var inventoryItems = await _stockKeepingRepository.GetAllInventoryItemsAsync();
                if (inventoryItems != null)
                {
                    cmbName.ItemsSource = inventoryItems;
                    cmbName.DisplayMemberPath = "Name";
                    cmbName.SelectedValuePath = "Id";
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

        private async Task LoadMenuItemCategory()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var menuCategory = await _stockKeepingRepository.GetMenuItemCategoriesAsync();
                if (menuCategory != null)
                {
                    cmbCategory.ItemsSource = menuCategory;
                    cmbCategory.DisplayMemberPath = "Name";
                    cmbCategory.SelectedValuePath = "Id";
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

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(txtPrice.Text, cmbCategory.Text, cmbServiceArea.Text);

                if (areFieldsEmpty)
                {
                    MessageBox.Show("Please fill all fields", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text.Replace(",", ""), out decimal price))
                {
                    MessageBox.Show("Invalid price format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (price <= 0)
                {
                    MessageBox.Show("Price must be greater than zero", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool isChecked = (bool)chkIsAvailable.IsChecked!;

                var menuItem = new Domain.Entities.StoreKeeping.MenuItem
                {
                    Price = decimal.Parse(txtPrice.Text.Replace(",", ""), CultureInfo.InvariantCulture),
                    MenuCategoryId = (string)cmbCategory.SelectedValue,
                    ServiceArea = Enum.Parse<ServiceArea>(cmbServiceArea.SelectedValue.ToString()!),
                    IsAvailable = isChecked,
                    IsDirectStock = (bool)chkIsDirectStock.IsChecked!,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                if (menuItem.IsDirectStock)
                {
                    menuItem.Name = cmbName.Text;
                    menuItem.InventoryItemID = (string)cmbName.SelectedValue;
                }
                else
                {
                    menuItem.Name = txtName.Text;
                }

                    await _stockKeepingRepository.AddMenuItemAsync(menuItem);

                if(!menuItem.IsDirectStock)
                {
                    AddMenuItemRecipeDialog addMenuItemRecipeDialog = new(_stockKeepingRepository, menuItem)
                    {
                        Owner = this
                    };

                    addMenuItemRecipeDialog.ShowDialog();
                }

                MessageBox.Show("Menu item added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

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

        private void DecimalInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d.]");
        }

        private void DecimalInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = !(e.Key >= Key.D0 && e.Key <= Key.D9 ||
                          e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 ||
                          e.Key == Key.Back || e.Key == Key.Delete ||
                          e.Key == Key.Left || e.Key == Key.Right ||
                          e.Key == Key.Decimal || e.Key == Key.OemPeriod);
        }

        private void DecimalInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressTextChanged) return;

            var textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox?.Text)) return;

            if (decimal.TryParse(textBox.Text.Replace(",", ""), out decimal value))
            {
                _suppressTextChanged = true;
                textBox.Text = string.Format(CultureInfo.InvariantCulture, "{0:N}", value);
                textBox.CaretIndex = textBox.Text.Length;
                _suppressTextChanged = false;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void chkIsDirectStock_Checked(object sender, RoutedEventArgs e)
        {
            stkCmbName.Visibility = Visibility.Visible;

            if (stkName != null)
                stkName.Visibility = Visibility.Collapsed;
        }

        private void chkIsDirectStock_Unchecked(object sender, RoutedEventArgs e)
        {
            stkCmbName.Visibility = Visibility.Collapsed;

            stkName.Visibility = Visibility.Visible;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServiceArea();
            await LoadMenuItemCategory();
            await LoadInventoryItem();
        }
    }
}
