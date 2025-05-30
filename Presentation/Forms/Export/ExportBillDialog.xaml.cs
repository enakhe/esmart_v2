using ESMART.Application.Common.Interface;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Presentation.Forms.Receipt;
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

        private readonly DataGrid _dataGrid;
        private readonly DataGrid? _serviceTable;
        private readonly DataGrid? _paymentTable;
        private readonly string? _extraTable;
        private readonly string? _title;
        private readonly decimal? _totalAmount;
        private readonly decimal? _totalServiceCharge;
        private readonly decimal? _totalVAT;
        private readonly decimal? _totalService;
        private readonly decimal? _totalDiscount;
        private readonly decimal? _totalTAmount;
        private readonly decimal? _amountPaid;
        private readonly List<string>? _nestedSelectedColumns;
        private readonly Domain.Entities.FrontDesk.Booking _booking;
        private readonly ReceiptExport? _receiptExport;
        private readonly System.Printing.PageOrientation _pageOrientation;
        private readonly IHotelSettingsService _hotelSettingsService;

        public ExportBillDialog(IEnumerable<string> columnNames, DataGrid dataGrid, IHotelSettingsService hotelSettingsService, Domain.Entities.FrontDesk.Booking booking, string? extraTable, List<string>? nestedSelectedColumns = null, string? title = null, decimal? totalAmount = null, ReceiptExport? receiptExport = null, System.Printing.PageOrientation pageOrientation = default, decimal? totalVAT = null, decimal? totalService = null, decimal? totalDiscount = null, decimal? totalTAmount = null, DataGrid? serviceTable = null, decimal? totalServiceCharge = null, decimal? amountPaid = null, DataGrid? paymentTable = null)
        {
            InitializeComponent();

            ColumnOptions = [.. columnNames.Select(c => new ColumnOption { ColumnName = c, IsSelected = true })];
            _dataGrid = dataGrid;
            _booking = booking;
            _extraTable = extraTable;
            _totalAmount = totalAmount;
            _title = title;
            _pageOrientation = pageOrientation;
            _totalVAT = totalVAT;
            _receiptExport = receiptExport;
            _nestedSelectedColumns = nestedSelectedColumns;
            _hotelSettingsService = hotelSettingsService;
            DataContext = this;

            Loaded += DisableMinimizeButton;
            _totalService = totalService;
            _totalDiscount = totalDiscount;
            _totalTAmount = totalTAmount;
            _serviceTable = serviceTable;
            _totalServiceCharge = totalServiceCharge;
            _amountPaid = amountPaid;
            _paymentTable = paymentTable;
        }

        public ExportResult GetResult()
        {
            return new ExportResult
            {
                SelectedColumns = [.. ColumnOptions.Where(c => (bool)c.IsSelected!).Select(c => c.ColumnName?.Replace(" ", ""))],
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
                    var printer = new PrintHelper();

                    var doc = printer.GenerateBillFlowDoc(result, hotel, _booking, _dataGrid, _extraTable, _nestedSelectedColumns, _title, _totalAmount, _totalVAT, _totalDiscount, _totalService, _totalTAmount, _totalServiceCharge, _amountPaid, _receiptExport, _serviceTable, _paymentTable);
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
            PrintAndSave(doc, txtFileName.Text, _pageOrientation);

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

        public void PrintAndSave(FlowDocument document, string fileName, System.Printing.PageOrientation orientation)
        {
            PrintHelper.PrintFlowDocument(document, orientation);
            PrintHelper.SaveFlowDocumentToFile(document, fileName);
        }
    }

    public class ExportResult2
    {
        public List<string>? SelectedColumns { get; set; }
        public List<string>? SelectedColumns2 { get; set; }
        public string? ExportFormat { get; set; }
        public string? FileName { get; set; }
    }
}
