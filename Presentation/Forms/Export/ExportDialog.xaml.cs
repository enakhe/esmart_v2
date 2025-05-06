#nullable disable

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

        public ExportDialog(IEnumerable<string> columnNames)
        {
            InitializeComponent();
            ColumnOptions = [.. columnNames.Select(c => new ColumnOption { ColumnName = c, IsSelected = true })];
            DataContext = this;
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFileName.Text))
            {
                MessageBox.Show("Please enter a file name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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
