#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Presentation.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.Export
{
    /// <summary>
    /// Interaction logic for ExportBillDialog.xaml
    /// </summary>
    public partial class ExportBillDialog : Window
    {
        public bool IsPdf { get; set; } = true;
        public bool IsExcel { get; set; }
        public ObservableCollection<ColumnOption> ColumnOptions { get; set; }
        public ObservableCollection<ColumnOption> ColumnOptions2 { get; set; }

        private readonly DataGrid _dataGrid;
        private readonly Domain.Entities.FrontDesk.Booking _booking;
        private readonly List<TransactionItemViewModel> _transactionItemViewModels;
        private readonly IHotelSettingsService _hotelSettingsService;

        public ExportBillDialog(IEnumerable<string> columnNames, DataGrid dataGrid, IHotelSettingsService hotelSettingsService, Domain.Entities.FrontDesk.Booking booking, List<TransactionItemViewModel> transactionItemViewModels)
        {
            InitializeComponent();

            ColumnOptions = [.. columnNames.Select(c => new ColumnOption { ColumnName = c, IsSelected = true })];
            _dataGrid = dataGrid;
            _booking = booking;
            _hotelSettingsService = hotelSettingsService;
            _transactionItemViewModels = transactionItemViewModels;
            DataContext = this;

            Loaded += DisableMinimizeButton;
        }

        public ExportResult GetResult()
        {
            return new ExportResult
            {
                SelectedColumns = [.. ColumnOptions.Where(c => c.IsSelected).Select(c => c.ColumnName.Replace(" ", ""))],
                ExportFormat = IsPdf ? "PDF" : "Excel",
                FileName = txtFileName.Text.Trim()
            };
        }

        public async Task<FlowDocument> LoadPreview()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var result = GetResult();
                var hotel = await _hotelSettingsService.GetHotelInformation();

                if (hotel != null)
                {
                    var printer = new ReceiptHelper();

                    var doc = printer.GenerateBillFlowDoc(result, hotel, _booking, _dataGrid, _transactionItemViewModels);
                    docViewer.Document = doc;

                    return doc;
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

            return docViewer.Document;
        }

        private async void ApplySelectionButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadPreview();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFileName.Text))
            {
                MessageBox.Show("Please enter a file name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            var doc = await LoadPreview();
            PrintAndSave(doc, txtFileName.Text);

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPreview();
        }

        private void DisableMinimizeButton(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MINIMIZEBOX);
        }

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x00020000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private async void txtFileName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            await LoadPreview();
        }

        public void PrintAndSave(FlowDocument document, string fileName)
        {
            ReceiptHelper.PrintFlowDocument(document);
            ReceiptHelper.SaveFlowDocumentToFile(document, fileName);
        }
    }

    public class ExportResult2
    {
        public List<string> SelectedColumns { get; set; }
        public List<string> SelectedColumns2 { get; set; }
        public string ExportFormat { get; set; }
        public string FileName { get; set; }
    }
}
