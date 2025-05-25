using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.StoreKeeping;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace ESMART.Presentation.Forms.StockKeeping.MenuCategory
{
    /// <summary>
    /// Interaction logic for AddMenuCategoryDialog.xaml
    /// </summary>
    public partial class AddMenuCategoryDialog : Window
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        public AddMenuCategoryDialog(IStockKeepingRepository stockKeepingRepository)
        {
            _stockKeepingRepository = stockKeepingRepository;
            InitializeComponent();
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

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                bool areFieldsEmpty = Helper.AreAnyNullOrEmpty(txtName.Text, cmbServiceArea.Text);

                if (areFieldsEmpty)
                {
                    MessageBox.Show("Please fill all fields", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool isChecked = (bool)chkIsAvailable.IsChecked!;

                var menyCategory = new Domain.Entities.StoreKeeping.MenuCategory
                {
                    Name = txtName.Text,
                    ServiceArea = Enum.Parse<ServiceArea>(cmbServiceArea.SelectedValue.ToString()!),
                    IsActive = isChecked,
                    CreatedAt = DateTime.Now,
                    Description = txtDescription.Text
                };

                await _stockKeepingRepository.AddMenuItemCategoryAsync(menyCategory);

                MessageBox.Show("Menu category added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServiceArea();
        }
    }
}
