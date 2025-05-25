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
using ESMART.Domain.Entities.Configuration;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using ESMART.Application.Common.Models;
using ESMART.Domain.ViewModels.Transaction;

namespace ESMART.Presentation.Utils
{
    public class ReceiptHelper
    {

        public FlowDocument GeneratePreviewFlowDocument(ExportResult result, Hotel hotel, DataGrid dataGrid, string title)
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

        public FlowDocument GenerateBillFlowDoc(ExportResult result, Hotel hotel, Domain.Entities.FrontDesk.Booking booking, DataGrid dataGrid, List<TransactionItemViewModel> transactionItem)
        {
            const double A4PortraitWidth = 793.7;
            const double A4PortraitHeight = 1122.5;

            var doc = new FlowDocument
            {
                PageWidth = A4PortraitHeight, // Landscape width
                PageHeight = A4PortraitWidth, // Landscape height
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

            doc.Blocks.Add(new Paragraph(new Run($"Address: {hotel.Address}"))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run($"Email: {hotel.Email} | Phone: {hotel.PhoneNumber}"))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run($"Date: {DateTime.Now:dd MMM yyyy}"))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run(""))); // Spacer

            Table infoTable = new Table();
            infoTable.Columns.Add(new TableColumn());
            infoTable.Columns.Add(new TableColumn());

            // Optional: Set column widths
            infoTable.Columns[0].Width = new GridLength(A4PortraitWidth);
            infoTable.Columns[1].Width = new GridLength(A4PortraitWidth);

            // Create a row
            TableRow row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run($"Check In\n")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left,
            });

            row.Cells.Add(new TableCell(new Paragraph(new Run($"Check In: {booking.CheckIn:dd MMM yyyy h:mm tt}")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            // Add another row
            TableRow row2 = new TableRow();
            row2.Cells.Add(new TableCell(new Paragraph(new Run($"Booking Number: {booking.BookingId}\n")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            row2.Cells.Add(new TableCell(new Paragraph(new Run($"Check Out: {booking.CheckOut:dd MMM yyyy h:mm tt}")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            TableRow row3 = new TableRow();
            row3.Cells.Add(new TableCell(new Paragraph(new Run($"Guest Name: {booking.Guest.FullName}\n")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            row3.Cells.Add(new TableCell(new Paragraph(new Run($"No of Rooms: {1}")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            TableRow row4 = new TableRow();
            row4.Cells.Add(new TableCell(new Paragraph(new Run($"Phone Number: {booking.Guest.PhoneNumber}\n")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            row4.Cells.Add(new TableCell(new Paragraph(new Run($"Email: {booking.Guest.Email}")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            TableRow row5 = new TableRow();
            row5.Cells.Add(new TableCell(new Paragraph(new Run($"Address: {booking.Guest.Street}, {booking.Guest.City}, {booking.Guest.State}, {booking.Guest.Country}\n")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            row5.Cells.Add(new TableCell(new Paragraph(new Run($"Total Amount: {booking.TotalAmount:N2}")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            // Add rows to table
            TableRowGroup rowGroup = new TableRowGroup();
            rowGroup.Rows.Add(row);
            rowGroup.Rows.Add(row2);
            rowGroup.Rows.Add(row3);
            rowGroup.Rows.Add(row4);
            rowGroup.Rows.Add(row5);


            infoTable.RowGroups.Add(rowGroup);

            // Add table to the document
            doc.Blocks.Add(infoTable);

            doc.Blocks.Add(new Paragraph(new Run("")));

            doc.Blocks.Add(new Paragraph(new Run($"Booking Invoice List (Invoices Settled)"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run("")));

            int columnCount = result.SelectedColumns.Count;
            double columnWidth = (doc.PageWidth - doc.PagePadding.Left - doc.PagePadding.Right) / columnCount;

            // Assuming transaction items collection is on booking.TransactionItems
            var allTransactionItems = transactionItem;

            foreach (var transaction in dataGrid.ItemsSource)
            {
                // --- Transaction table for the current transaction ---
                var transactionTable = new Table
                {
                    CellSpacing = 0,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5),
                    Margin = new Thickness(0, 10, 0, 5)
                };

                transactionTable.Columns.Clear();
                for (int i = 0; i < columnCount; i++)
                    transactionTable.Columns.Add(new TableColumn { Width = new GridLength(columnWidth) });

                var transactionRowGroup = new TableRowGroup();

                // Add header row - optional: add only once before loop for performance
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
                transactionRowGroup.Rows.Add(headerRow);

                // Add transaction data row
                var transactionRow = new TableRow();
                foreach (var col in result.SelectedColumns)
                {
                    var prop = transaction.GetType().GetProperty(col);
                    var value = prop?.GetValue(transaction);
                    string cellValue = value is decimal decVal ? "₦ " + decVal.ToString("N2") : value?.ToString() ?? "";

                    transactionRow.Cells.Add(new TableCell(new Paragraph(new Run(cellValue))
                    {
                        Margin = new Thickness(4),
                        TextAlignment = TextAlignment.Center
                    })
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.25)
                    });
                }
                transactionRowGroup.Rows.Add(transactionRow);
                transactionTable.RowGroups.Add(transactionRowGroup);
                doc.Blocks.Add(transactionTable);

                // --- Nested transaction items table for this transaction ---
                var transactionIdProp = transaction.GetType().GetProperty("BookingId") ?? transaction.GetType().GetProperty("TransactionId");
                if (transactionIdProp == null)
                    continue;

                var transactionId = transactionIdProp.GetValue(transaction);

                var filteredItems = allTransactionItems?.Where(item =>
                {
                    var itemTransactionIdProp = item.GetType().GetProperty("BookingId") ?? item.GetType().GetProperty("TransactionId");
                    if (itemTransactionIdProp == null) return false;
                    var itemTransactionId = itemTransactionIdProp.GetValue(item);
                    return itemTransactionId != null && itemTransactionId.Equals(transactionId);
                }).ToList();

                if (filteredItems == null || filteredItems.Count == 0)
                    continue;

                var itemsTable = new Table
                {
                    CellSpacing = 0,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5),
                    Margin = new Thickness(20, 0, 0, 10) // Indent for clarity
                };

                var itemProperties = filteredItems.First().GetType().GetProperties();

                itemsTable.Columns.Clear();
                foreach (var prop in itemProperties)
                {
                    itemsTable.Columns.Add(new TableColumn { Width = new GridLength(columnWidth) });
                }

                var itemRowGroup = new TableRowGroup();

                // Header for transaction items
                var itemHeaderRow = new TableRow();
                foreach (var prop in itemProperties)
                {
                    itemHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run(prop.Name))
                    {
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(4),
                        TextAlignment = TextAlignment.Center,
                        FontSize = 11
                    })
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5),
                        Background = Brushes.LightGray
                    });
                }
                itemRowGroup.Rows.Add(itemHeaderRow);

                // Rows for transaction items
                foreach (var item in filteredItems)
                {
                    var itemRow = new TableRow();
                    foreach (var prop in itemProperties)
                    {
                        var value = prop.GetValue(item);
                        string cellValue = value is decimal decVal ? "₦ " + decVal.ToString("N2") : value?.ToString() ?? "";

                        itemRow.Cells.Add(new TableCell(new Paragraph(new Run(cellValue))
                        {
                            Margin = new Thickness(4),
                            TextAlignment = TextAlignment.Center,
                            FontSize = 10.5
                        })
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.25)
                        });
                    }
                    itemRowGroup.Rows.Add(itemRow);
                }

                itemsTable.RowGroups.Add(itemRowGroup);
                doc.Blocks.Add(itemsTable);
            }

            // Account Charges Heading
            doc.Blocks.Add(new Paragraph(new Run("\nAccount Charges (Guest Wallet Debits)"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run($"Guest: {booking.Guest.FullName} - {booking.Room.RoomType.Name}"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run(""))); // spacer

            // Placeholder for further account transactions table if needed
            var accTable = new Table
            {
                CellSpacing = 0,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5)
            };

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
            var printDialog = new PrintDialog();

            // Set printer orientation to landscape
            printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

            if (printDialog.ShowDialog() == true)
            {
                // Set the FlowDocument to A4 landscape dimensions (1122.5 x 793.7 at 96 DPI)
                document.PageWidth = 1122.5;  // A4 Landscape width
                document.PageHeight = 793.7;  // A4 Landscape height
                document.PagePadding = new Thickness(50);
                document.ColumnWidth = double.PositiveInfinity;

                // Set the paginator to match the landscape size
                IDocumentPaginatorSource idpSource = document;
                var paginator = idpSource.DocumentPaginator;
                paginator.PageSize = new Size(document.PageWidth, document.PageHeight);

                printDialog.PrintDocument(paginator, "Hotel Document Print - Landscape");
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
