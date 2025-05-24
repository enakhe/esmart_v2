#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Presentation.Utils;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;

namespace ESMART.Presentation.Forms.Export
{
    /// <summary>
    /// Interaction logic for ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : Window
    {
        public bool IsPdf { get; set; } = true;
        public bool IsExcel { get; set; }
        public ObservableCollection<ColumnOption> ColumnOptions { get; set; }
        private readonly DataGrid _dataGrid;
        private readonly IHotelSettingsService _hotelSettingsService;

        public ExportDialog(IEnumerable<string> columnNames, DataGrid dataGrid, IHotelSettingsService hotelSettingsService)
        {
            InitializeComponent();
            ColumnOptions = [.. columnNames.Select(c => new ColumnOption { ColumnName = c, IsSelected = true })];
            _dataGrid = dataGrid;
            _hotelSettingsService = hotelSettingsService;
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

                    var doc = printer.GeneratePreviewFlowDocument(result, hotel, _dataGrid, txtFileName.Text);
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

    public class ColumnOption
    {
        public string ColumnName { get; set; }
        public bool IsSelected { get; set; } = true;
    }

    public class ExportResult
    {
        public List<string> SelectedColumns { get; set; }
        public string ExportFormat { get; set; }
        public string FileName { get; set; }
    }
}
