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
using System.Windows.Threading;

namespace ESMART.Presentation.Forms.StockKeeping.MenuItem
{
    /// <summary>
    /// Interaction logic for AddMenuItemDialog.xaml
    /// </summary>
    public partial class AddMenuItemDialog : Window
    {
        private bool _suppressTextChanged = false;
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private DispatcherTimer _formatTimer;
        public AddMenuItemDialog(IStockKeepingRepository stockKeepingRepository)
        {
            _stockKeepingRepository = stockKeepingRepository;
            InitializeComponent();

            stkName.Visibility = Visibility.Collapsed;
            _formatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _formatTimer.Tick += FormatTimer_Tick;
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
                var category = await _stockKeepingRepository.GetMenuItemCategoryByIdAsync((string)cmbCategory.SelectedValue);

                var menuItem = new Domain.Entities.StoreKeeping.MenuItem
                {
                    Price = decimal.Parse(txtPrice.Text.Replace(",", ""), CultureInfo.InvariantCulture),
                    MenuCategoryId = (string)cmbCategory.SelectedValue,
                    ServiceArea = Enum.Parse<ServiceArea>(cmbServiceArea.SelectedValue.ToString()!),
                    IsAvailable = isChecked,
                    Image = category.Image,
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

        private void DecimalInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressTextChanged) return;

            _formatTimer.Stop(); // restart timer
            _formatTimer.Tag = sender;
            _formatTimer.Start();
        }

        private void FormatTimer_Tick(object sender, EventArgs e)
        {
            _formatTimer.Stop();

            var textBox = _formatTimer.Tag as TextBox;
            if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text)) return;

            int caretIndex = textBox.CaretIndex;
            string unformatted = textBox.Text.Replace(",", "");

            if (decimal.TryParse(unformatted, out decimal value))
            {
                _suppressTextChanged = true;

                textBox.Text = string.Format(CultureInfo.InvariantCulture, "{0:N}", value);
                textBox.CaretIndex = Math.Min(caretIndex, textBox.Text.Length);

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
