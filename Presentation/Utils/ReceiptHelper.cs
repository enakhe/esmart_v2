using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using System.Transactions;
using ESMART.Domain.Entities.Transaction;
using System.Windows.Media;
using System.Windows.Controls;
using ESMART.Presentation.Forms.Export;
using static MaterialDesignThemes.Wpf.Theme;
using ESMART.Domain.Entities.Configuration;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using ESMART.Application.Common.Models;

namespace ESMART.Presentation.Utils
{
    public class ReceiptHelper
    {

        public FlowDocument GeneratePreviewFlowDocument(ExportResult result, Hotel hotel, System.Windows.Controls.DataGrid dataGrid, string title)
        {
            var doc = new FlowDocument
            {
                PageWidth = 793.7, // A4 width at 96 DPI
                PageHeight = 1122.5,
                PagePadding = new Thickness(50),
                ColumnWidth = double.PositiveInfinity,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            };

            // ======= HOTEL LOGO ==========
            if (hotel.LogoUrl != null && hotel.LogoUrl.Length > 0)
            {
                var image = new Image
                {
                    Width = 100, // Adjust size as needed
                    Height = 100,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = LoadImage(hotel.LogoUrl)
                };

                var logoContainer = new BlockUIContainer(image)
                {
                    Margin = new Thickness(0, 0, 0, 10),
                    TextAlignment = TextAlignment.Center
                };

                doc.Blocks.Add(logoContainer);
            }

            doc.Blocks.Add(new Paragraph(new Run(hotel.Name))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            doc.Blocks.Add(new Paragraph(new Run($"Address: {hotel.Address}")));
            doc.Blocks.Add(new Paragraph(new Run($"Email: {hotel.Email} | Phone: {hotel.PhoneNumber}")));
            doc.Blocks.Add(new Paragraph(new Run($"Date: {DateTime.Now:dd MMM yyyy}")));
            doc.Blocks.Add(new Paragraph(new Run(""))); // Spacer

            if (!String.IsNullOrEmpty(title))
            {
                // ======= HEADER ==========
                doc.Blocks.Add(new Paragraph(new Run(title))
                {
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 5)
                });
            }

            doc.Blocks.Add(new Paragraph(new Run(""))); // Spacer

            var table = new Table
            {
                CellSpacing = 0,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5)
            };

            int columnCount = result.SelectedColumns.Count;

            // Make all columns evenly spaced across A4 width (minus padding)
            double columnWidth = (doc.PageWidth - doc.PagePadding.Left - doc.PagePadding.Right) / columnCount;
            for (int i = 0; i < columnCount; i++)
            {
                table.Columns.Add(new TableColumn { Width = new GridLength(columnWidth) });
            }

            var rowGroup = new TableRowGroup();

            // Header row
            var headerRow = new TableRow();
            foreach (var col in result.SelectedColumns)
            {
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run(col))
                {
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(4),
                    TextAlignment = TextAlignment.Center
                })
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5),
                    Background = Brushes.LightGray
                });
            }
            rowGroup.Rows.Add(headerRow);

            // Data rows
            foreach (var item in dataGrid.ItemsSource)
            {
                var row = new TableRow();
                foreach (var col in result.SelectedColumns)
                {
                    var prop = item.GetType().GetProperty(col);
                    var value = prop?.GetValue(item)?.ToString() ?? "";

                    var cell = new TableCell(new Paragraph(new Run(value))
                    {
                        Margin = new Thickness(4),
                        //TextWrapping = TextWrapping.Wrap
                    })
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.25)
                    };

                    row.Cells.Add(cell);
                }
                rowGroup.Rows.Add(row);
            }

            // check if there is a column named Amount and calculate the total and display it in the document
            if (result.SelectedColumns.Contains("Amount"))
            {
                var totalRow = new TableRow();
                totalRow.Cells.Add(new TableCell(new Paragraph(new Run("Total"))
                {
                    FontWeight = FontWeights.UltraBold,
                    Margin = new Thickness(4),
                    TextAlignment = TextAlignment.Right
                })
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5),
                    Background = Brushes.LightGray
                });
                decimal totalAmount = 0;
                foreach (var item in dataGrid.ItemsSource)
                {
                    var prop = item.GetType().GetProperty("Amount");
                    if (prop != null && prop.GetValue(item) is string amount)
                    {
                        totalAmount += decimal.Parse(amount);
                    }
                }
                totalRow.Cells.Add(new TableCell(new Paragraph(new Run($" ₦{totalAmount.ToString("N2")}"))
                {
                    Margin = new Thickness(4),
                    TextAlignment = TextAlignment.Right,
                    FontWeight = FontWeights.UltraBold,

                })
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5),
                    Background = Brushes.LightGray
                });
                rowGroup.Rows.Add(totalRow);
            }

            table.RowGroups.Add(rowGroup);
            doc.Blocks.Add(table);

            return doc;
        }

        public static void PrintReceipt(FlowDocument doc)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = doc;
                pd.PrintDocument(idpSource.DocumentPaginator, "Receipt Print");
            }
        }

        private ImageSource LoadImage(byte[] imageData)
        {
            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
            }
            return bitmap;
        }

        public static void PrintFlowDocument(FlowDocument document)
        {
            var pd = new PrintDialog();

            if (pd.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = document;
                document.PageHeight = 1122.5;
                document.PageWidth = 793.7;
                document.PagePadding = new Thickness(50);
                document.ColumnWidth = double.PositiveInfinity;

                pd.PrintDocument(idpSource.DocumentPaginator, "Hotel Document Print");
            }
        }

        public static void SaveFlowDocumentToFile(FlowDocument document, string fileName)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fullPath = Path.Combine(documentsPath, $"{fileName}.xps");

            // Ensure the document is laid out for A4
            document.PageHeight = 1122.5;
            document.PageWidth = 793.7;
            document.PagePadding = new Thickness(50);
            document.ColumnWidth = double.PositiveInfinity;

            // Save as XPS
            using var xpsDocument = new XpsDocument(fullPath, FileAccess.Write);
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            writer.Write(((IDocumentPaginatorSource)document).DocumentPaginator);

            MessageBox.Show($"Document saved to {fullPath}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
