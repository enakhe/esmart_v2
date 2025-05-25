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

namespace ESMART.Presentation.Forms.StockKeeping.MenuCategory
{
    /// <summary>
    /// Interaction logic for UpdateMenuCategoryDialog.xaml
    /// </summary>
    public partial class UpdateMenuCategoryDialog : Window
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private readonly Domain.Entities.StoreKeeping.MenuCategory _menuCategory;
        public UpdateMenuCategoryDialog(IStockKeepingRepository stockKeepingRepository, Domain.Entities.StoreKeeping.MenuCategory menuCategory)
        {
            _stockKeepingRepository = stockKeepingRepository;
            _menuCategory = menuCategory;
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

        public async Task LoadCategory()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var category = await _stockKeepingRepository.GetMenuItemCategoryByIdAsync(_menuCategory.Id);
                if (category != null)
                {
                    txtName.Text = category.Name;
                    cmbServiceArea.SelectedValue = category.ServiceArea.ToString();
                    chkIsAvailable.IsChecked = category.IsActive;
                    txtDescription.Text = category.Description;
                }
                else
                {
                    MessageBox.Show("Category not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServiceArea();
            await LoadCategory();
        }
    }
}
