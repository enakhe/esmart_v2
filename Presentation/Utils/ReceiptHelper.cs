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

namespace ESMART.Presentation.Utils
{
    public class ReceiptHelper
    {
        public FlowDocument GenerateTransactionItemReceiptDocument(TransactionItem transaction, string hotelName, string hotelAddress, string hotelPhoneNo)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(20),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12
            };

            // Header
            doc.Blocks.Add(new Paragraph(new Run(hotelName))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });

            doc.Blocks.Add(new Paragraph(new Run($"{hotelAddress}"))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
            });

            doc.Blocks.Add(new Paragraph(new Run($"{hotelPhoneNo}"))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
            });

            doc.Blocks.Add(new Paragraph(new Run($"Date: {transaction.DateAdded:G}"))
            {
                TextAlignment = TextAlignment.Left,
                FontWeight = FontWeights.Bold,
            });

            doc.Blocks.Add(new Paragraph(new Run($"Service ID: {transaction.ServiceId}"))
            {
                TextAlignment = TextAlignment.Left,
                FontWeight = FontWeights.Bold,
            });

            doc.Blocks.Add(new Paragraph(new Run(new string('-', 46))));

            // Table for items
            var table = new Table();
            table.Columns.Add(new TableColumn { Width = new GridLength(90) }); // Name
            table.Columns.Add(new TableColumn { Width = new GridLength(60) });  // Qty
            table.Columns.Add(new TableColumn { Width = new GridLength(80) });  // Price
            table.Columns.Add(new TableColumn { Width = new GridLength(80) });  // Total

            var headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Item"))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Type"))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Price"))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Total"))));
            var header = new TableRowGroup();
            header.Rows.Add(headerRow);
            table.RowGroups.Add(header);

            var body = new TableRowGroup();

            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(transaction.Category.ToString()))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(transaction.Type.ToString()))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(transaction.Amount.ToString("N2")))));

            row.Cells.Add(new TableCell(new Paragraph(new Run(transaction.Amount.ToString("N2")))));
            body.Rows.Add(row);

            table.RowGroups.Add(body);
            doc.Blocks.Add(table);

            doc.Blocks.Add(new Paragraph(new Run(new string('-', 46))));

            // Totals
            doc.Blocks.Add(new Paragraph(new Run($"Total: ₦{transaction.Amount}")) { TextAlignment = TextAlignment.Right });

            // Footer
            doc.Blocks.Add(new Paragraph(new Run("Thank you for your purchase!"))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 0)
            });

            return doc;
        }

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
