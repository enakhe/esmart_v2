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
using ESMART.Presentation.Forms.Receipt;
using DocumentFormat.OpenXml.Office2016.Excel;
using ESMART.Domain.Entities.FrontDesk;
using System.Text.RegularExpressions;

namespace ESMART.Presentation.Utils
{
    public class ReceiptHelper
    {

        public FlowDocument GeneratePreviewFlowDocument(ExportResult result, Hotel hotel, DataGrid dataGrid, string title, string? nestedCollectionPropertyName = null)
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
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            };

            // ===== Logo =====
            if (hotel.LogoUrl?.Length > 0)
            {
                var image = new Image
                {
                    Width = 110,
                    Height = 110,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = LoadImage(hotel.LogoUrl)
                };

                doc.Blocks.Add(new BlockUIContainer(image) { Margin = new Thickness(0, 0, 0, 11) });
            }

            // ===== Header Info =====
            doc.Blocks.Add(new Paragraph(new Run(hotel.Name)) { FontSize = 20, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            doc.Blocks.Add(new Paragraph(new Run($"Address: {hotel.Address}")));
            doc.Blocks.Add(new Paragraph(new Run($"Email: {hotel.Email} | Phone: {hotel.PhoneNumber}")));
            doc.Blocks.Add(new Paragraph(new Run($"Date: {DateTime.Now:dd MMM yyyy}")));
            doc.Blocks.Add(new Paragraph(new Run(""))); // spacer

            if (!string.IsNullOrEmpty(title))
            {
                doc.Blocks.Add(new Paragraph(new Run(title)) { FontSize = 18, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            }

            doc.Blocks.Add(new Paragraph(new Run(""))); // spacer

            foreach (var parentItem in dataGrid.ItemsSource)
            {
                // ==== Main (Parent) Table for the Order ====
                var parentTable = new Table { CellSpacing = 0 };
                foreach (var _ in result.SelectedColumns!)
                    parentTable.Columns.Add(new TableColumn { Width = GridLength.Auto });

                var parentGroup = new TableRowGroup();
                var parentHeaderRow = new TableRow();
                foreach (var col in result.SelectedColumns)
                {
                    parentHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run(col)))
                    {
                        FontWeight = FontWeights.Bold,
                        Background = Brushes.Black,
                        BorderBrush = Brushes.Black,
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0.5),
                        Padding = new Thickness(4)
                    });
                }
                parentGroup.Rows.Add(parentHeaderRow);

                var parentDataRow = new TableRow();
                foreach (var col in result.SelectedColumns)
                {
                    var prop = parentItem.GetType().GetProperty(col);
                    var value = prop?.GetValue(parentItem)?.ToString() ?? "";
                    parentDataRow.Cells.Add(new TableCell(new Paragraph(new Run(value)))
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5),
                        Padding = new Thickness(4),
                        TextAlignment = TextAlignment.Center,
                    });
                }
                parentGroup.Rows.Add(parentDataRow);
                parentTable.RowGroups.Add(parentGroup);
                doc.Blocks.Add(parentTable);

                // ==== Nested Table (e.g. OrderItems) ====
                if (!string.IsNullOrEmpty(nestedCollectionPropertyName))
                {
                    var nestedProp = parentItem.GetType().GetProperty(nestedCollectionPropertyName);
                    if (nestedProp != null)
                    {
                        var nestedItems = nestedProp.GetValue(parentItem) as IEnumerable<object>;
                        if (nestedItems != null && nestedItems.Any())
                        {
                            doc.Blocks.Add(new Paragraph(new Run("Details:"))
                            {
                                FontWeight = FontWeights.Bold,
                                Margin = new Thickness(0, 11, 0, 5)
                            });

                            var nestedTable = new Table { CellSpacing = 0 };
                            var nestedProps = nestedItems.First().GetType().GetProperties();
                            foreach (var _ in nestedProps)
                                nestedTable.Columns.Add(new TableColumn { Width = GridLength.Auto });

                            var nestedGroup = new TableRowGroup();
                            var nestedHeaderRow = new TableRow();
                            foreach (var prop in nestedProps)
                            {
                                nestedHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run(prop.Name)))
                                {
                                    FontWeight = FontWeights.Bold,
                                    Background = Brushes.LightGray,
                                    BorderBrush = Brushes.Black,
                                    BorderThickness = new Thickness(0.5),
                                    Padding = new Thickness(4)
                                });
                            }
                            nestedGroup.Rows.Add(nestedHeaderRow);

                            foreach (var item in nestedItems)
                            {
                                var row = new TableRow();
                                foreach (var prop in nestedProps)
                                {
                                    var value = prop.GetValue(item)?.ToString() ?? "";
                                    row.Cells.Add(new TableCell(new Paragraph(new Run(value)))
                                    {
                                        BorderBrush = Brushes.Black,
                                        BorderThickness = new Thickness(0.5),
                                        Padding = new Thickness(4),
                                        TextAlignment = TextAlignment.Center,
                                    });
                                }
                                nestedGroup.Rows.Add(row);
                            }

                            nestedTable.RowGroups.Add(nestedGroup);
                            doc.Blocks.Add(nestedTable);
                        }
                    }
                }

                // Optional spacing between orders
                doc.Blocks.Add(new Paragraph(new Run(" ")) { Margin = new Thickness(0, 11, 0, 11) });
            }

            return doc;
        }


        public FlowDocument GenerateBillFlowDoc(ExportResult result, Hotel hotel, Domain.Entities.FrontDesk.Booking booking, DataGrid dataGrid, string? nestedCollectionPropertyName = null, List<string>? nestedSelectedColumns = null, string? title = null, decimal? totalAmount = 0, decimal? totalTax = 0, decimal? totalDiscount = 0, decimal? totalCharge = 0, decimal? totalTAmount = 0, decimal? totalServiceCharge = 0, decimal? amountPaid = 0, ReceiptExport? receiptExport = null, DataGrid? serVicdFeeDataGrid = null, DataGrid? paymentDataGrid = null)
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
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            };

            // ======= HOTEL LOGO ==========
            if (hotel.LogoUrl != null && hotel.LogoUrl.Length > 0)
            {
                var image = new Image
                {
                    Width = 110, // Adjust size as needed
                    Height = 110,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = LoadImage(hotel.LogoUrl)
                };

                var logoContainer = new BlockUIContainer(image)
                {
                    Margin = new Thickness(0, 0, 0, 11),
                    TextAlignment = TextAlignment.Center
                };

                doc.Blocks.Add(logoContainer);
            }

            doc.Blocks.Add(new Paragraph(new Run(hotel.Name))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 11)
            });

            doc.Blocks.Add(new Paragraph(new Run($"Address: {hotel.Address}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run($"Email: {hotel.Email} | Phone: {hotel.PhoneNumber}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run($"Date: {DateTime.Now:dd MMM yyyy}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run(""))); // Spacer

            Table infoTable = new Table();
            infoTable.Columns.Add(new TableColumn());
            infoTable.Columns.Add(new TableColumn());

            // Give infoTable a a4 paper width
            infoTable.Columns[0].Width = new GridLength((A4PortraitWidth / 2) + 40);
            infoTable.Columns[1].Width = new GridLength((A4PortraitWidth / 2) + 40);

            // Create a row
            TableRow row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run($"Check In\n")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left,
            });

            row.Cells.Add(new TableCell(new Paragraph(new Run($"Check In: {booking.CheckIn:dd MMM yyyy h:mm tt}")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            // Add another row
            TableRow row2 = new TableRow();
            row2.Cells.Add(new TableCell(new Paragraph(new Run($"Booking Number: {booking.BookingId}\n")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            row2.Cells.Add(new TableCell(new Paragraph(new Run($"Check Out: {booking.CheckOut:dd MMM yyyy h:mm tt}")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            TableRow row3 = new TableRow();
            row3.Cells.Add(new TableCell(new Paragraph(new Run($"Guest Name: {booking.Guest.FullName}\n")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            row3.Cells.Add(new TableCell(new Paragraph(new Run($"No of Rooms: {1}")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            TableRow row4 = new TableRow();
            row4.Cells.Add(new TableCell(new Paragraph(new Run($"Phone Number: {booking.Guest.PhoneNumber}\n")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            row4.Cells.Add(new TableCell(new Paragraph(new Run($"Email: {booking.Guest.Email}")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            TableRow row5 = new TableRow();
            row5.Cells.Add(new TableCell(new Paragraph(new Run($"Address: {booking.Guest.Street}, {booking.Guest.City}, {booking.Guest.State}, {booking.Guest.Country}\n")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            if (totalAmount != null)
            {
                row5.Cells.Add(new TableCell(new Paragraph(new Run($"Total Amount: ₦ {totalAmount:N2} + Tax ₦ {totalCharge:N2} {(totalServiceCharge != 0 ? $"+ ₦{(totalServiceCharge):N2}" : "")}")))
                {
                    FontSize = 11,
                    TextAlignment = TextAlignment.Left
                });
            }

            // Add rows to table
            TableRowGroup rowGroup = new TableRowGroup();
            rowGroup.Rows.Add(row);
            rowGroup.Rows.Add(row2);
            rowGroup.Rows.Add(row3);
            rowGroup.Rows.Add(row4);
            rowGroup.Rows.Add(row5);

            if (receiptExport != null)
            {
                TableRow row6 = new TableRow();
                row6.Cells.Add(new TableCell(new Paragraph(new Run($"Receipt Number: {receiptExport.ReceiptNo}")))
                {
                    FontSize = 11,
                    TextAlignment = TextAlignment.Left
                });

                row6.Cells.Add(new TableCell(new Paragraph(new Run($"Cashier: {receiptExport.Cashier}")))
                {
                    FontSize = 11,
                    TextAlignment = TextAlignment.Left
                });

                rowGroup.Rows.Add(row6);
            }

            infoTable.RowGroups.Add(rowGroup);

            // Add table to the document
            doc.Blocks.Add(infoTable);

            if (title != null)
            {
                doc.Blocks.Add(new Paragraph(new Run(title))
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center
                });
            }

            foreach (var parentItem in dataGrid.ItemsSource)
            {
                // ==== Main (Parent) Table for the Order ====
                var parentTable = new Table { CellSpacing = 0 };
                foreach (var _ in result.SelectedColumns!)
                    parentTable.Columns.Add(new TableColumn { Width = GridLength.Auto });

                var parentGroup = new TableRowGroup();
                var parentHeaderRow = new TableRow();
                foreach (var col in result.SelectedColumns)
                {
                    parentHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run(SplitCamelCase(col))))
                    {
                        FontWeight = FontWeights.Bold,
                        Background = Brushes.Black,
                        BorderBrush = Brushes.Black,
                        Foreground = Brushes.White,
                        TextAlignment = TextAlignment.Center,
                        BorderThickness = new Thickness(0.5),
                        Padding = new Thickness(4)
                    });
                }
                parentGroup.Rows.Add(parentHeaderRow);

                var parentDataRow = new TableRow();
                foreach (var col in result.SelectedColumns)
                {
                    var prop = parentItem.GetType().GetProperty(col);
                    var value = prop?.GetValue(parentItem);
                    string cellValue = value is decimal decVal ? "₦ " + decVal.ToString("N2") : value?.ToString() ?? "";

                    parentDataRow.Cells.Add(new TableCell(new Paragraph(new Run(cellValue)))
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5),
                        Padding = new Thickness(4),
                        TextAlignment = TextAlignment.Center,
                    });
                }
                parentGroup.Rows.Add(parentDataRow);
                parentTable.RowGroups.Add(parentGroup);
                doc.Blocks.Add(parentTable);

                // ==== Nested Table (e.g. OrderItems) ====
                if (!string.IsNullOrEmpty(nestedCollectionPropertyName))
                {
                    var nestedProp = parentItem.GetType().GetProperty(nestedCollectionPropertyName);
                    if (nestedProp != null)
                    {
                        var nestedItems = nestedProp.GetValue(parentItem) as IEnumerable<object>;
                        if (nestedItems != null && nestedItems.Any())
                        {
                            doc.Blocks.Add(new Paragraph(new Run("Account Statement"))
                            {
                                FontWeight = FontWeights.Bold,
                                FontSize = 13,
                                Margin = new Thickness(0, 11, 0, 5),
                                TextAlignment = TextAlignment.Center
                            });

                            var nestedTable = new Table { CellSpacing = 0 };
                            var nestedItemType = nestedItems.First().GetType();
                            var nestedProps = nestedItemType.GetProperties();

                            var filteredNestedProps = nestedSelectedColumns != null && nestedSelectedColumns.Any()
                                ? nestedProps.Where(p => nestedSelectedColumns.Contains(p.Name)).ToList()
                                : nestedProps.ToList();

                            foreach (var _ in filteredNestedProps)
                                nestedTable.Columns.Add(new TableColumn { Width = GridLength.Auto });

                            var nestedGroup = new TableRowGroup();
                            var nestedHeaderRow = new TableRow();
                            foreach (var prop in filteredNestedProps)
                            {
                                nestedHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run(SplitCamelCase(prop.Name))))
                                {
                                    FontWeight = FontWeights.Bold,
                                    Background = Brushes.LightGray,
                                    TextAlignment = TextAlignment.Center,
                                    BorderBrush = Brushes.Black,
                                    BorderThickness = new Thickness(0.5),
                                    Padding = new Thickness(4)
                                });
                            }
                            nestedGroup.Rows.Add(nestedHeaderRow);

                            // calculated the total of the nested items bill posts
                            if (nestedSelectedColumns != null && nestedSelectedColumns.Contains("BillPost"))
                            {
                                decimal nestedTotal = nestedItems.Sum(item =>
                                {
                                    var amountProp = item.GetType().GetProperty("BillPost");
                                    return amountProp != null ? (decimal)amountProp.GetValue(item)! : 0;
                                });
                                doc.Blocks.Add(new Paragraph(new Run($"Guest: {booking.Guest.FullName} - {booking.Room.RoomType.Name} ({booking.Room.Number}): ₦ {nestedTotal:N2}"))
                                {
                                    FontSize = 14,
                                    FontWeight = FontWeights.Bold,
                                    TextAlignment = TextAlignment.Right
                                });
                            }

                            foreach (var item in nestedItems)
                            {
                                var roww = new TableRow();
                                foreach (var prop in filteredNestedProps)
                                {
                                    var value = prop.GetValue(item);
                                    string cellValue;
                                    if (value is decimal decVal)
                                    {
                                        cellValue = "₦ " + decVal.ToString("N2");
                                    }
                                    else if (value is DateTime dtVal)
                                    {
                                        cellValue = dtVal.ToString("MM/dd/yy");
                                    }
                                    else
                                    {
                                        cellValue = value?.ToString() ?? "";
                                    }
                                    roww.Cells.Add(new TableCell(new Paragraph(new Run(cellValue)))
                                    {
                                        BorderBrush = Brushes.Black,
                                        BorderThickness = new Thickness(0.5),
                                        Padding = new Thickness(4),
                                        TextAlignment = TextAlignment.Center,
                                    });
                                }
                                nestedGroup.Rows.Add(roww);
                            }

                            nestedTable.RowGroups.Add(nestedGroup);
                            doc.Blocks.Add(nestedTable);
                        }
                    }
                }

                // Optional spacing between orders
                doc.Blocks.Add(new Paragraph(new Run(" ")) { Margin = new Thickness(0, 5, 0, 5) });
            }

            if (serVicdFeeDataGrid != null && serVicdFeeDataGrid.ItemsSource != null && serVicdFeeDataGrid.ItemsSource.Cast<object>().Any())
            {
                doc.Blocks.Add(new Paragraph(new Run("Other Service Charges"))
                {
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    Margin = new Thickness(0, 11, 0, 5),
                    TextAlignment = TextAlignment.Center
                });

                // add another table just like the nesteddatagrid with the same columns
                var serviceTable = new Table { CellSpacing = 0 };

                foreach (var _ in nestedSelectedColumns!)
                    serviceTable.Columns.Add(new TableColumn { Width = GridLength.Auto });

                var serviceGroup = new TableRowGroup();
                var serviceHeaderRow = new TableRow();

                foreach (var col in nestedSelectedColumns)
                {
                    serviceHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run(SplitCamelCase(col))))
                    {
                        FontWeight = FontWeights.Bold,
                        Background = Brushes.LightGray,
                        TextAlignment = TextAlignment.Center,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5),
                        Padding = new Thickness(4)
                    });
                }

                serviceGroup.Rows.Add(serviceHeaderRow);

                foreach (var serviceItem in serVicdFeeDataGrid.ItemsSource)
                {
                    var serviceRow = new TableRow();
                    foreach (var col in nestedSelectedColumns)
                    {
                        var prop = serviceItem.GetType().GetProperty(col);
                        var value = prop?.GetValue(serviceItem);
                        string cellValue;
                        if (value is decimal decVal)
                        {
                            cellValue = "₦ " + decVal.ToString("N2");
                        }
                        else if (value is DateTime dtVal)
                        {
                            cellValue = dtVal.ToString("MM/dd/yy");
                        }
                        else
                        {
                            cellValue = value?.ToString() ?? "";
                        }
                        serviceRow.Cells.Add(new TableCell(new Paragraph(new Run(cellValue)))
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.5),
                            Padding = new Thickness(4),
                            TextAlignment = TextAlignment.Center,
                        });
                    }
                    serviceGroup.Rows.Add(serviceRow);
                }

                serviceTable.RowGroups.Add(serviceGroup);
                doc.Blocks.Add(serviceTable);
            }

            // Add paymentDataGrid like we did for serVicdFeeDataGrid
            if (paymentDataGrid != null && paymentDataGrid.ItemsSource != null && paymentDataGrid.ItemsSource.Cast<object>().Any())
            {
                doc.Blocks.Add(new Paragraph(new Run("Payment History"))
                {
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    Margin = new Thickness(0, 11, 0, 5),
                    TextAlignment = TextAlignment.Center
                });
                // add another table just like the nesteddatagrid with the same columns
                var paymentTable = new Table { CellSpacing = 0 };
                foreach (var _ in nestedSelectedColumns!)
                    paymentTable.Columns.Add(new TableColumn { Width = GridLength.Auto });
                var paymentGroup = new TableRowGroup();
                var paymentHeaderRow = new TableRow();
                foreach (var col in nestedSelectedColumns)
                {
                    paymentHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run(SplitCamelCase(col))))
                    {
                        FontWeight = FontWeights.Bold,
                        Background = Brushes.LightGray,
                        TextAlignment = TextAlignment.Center,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5),
                        Padding = new Thickness(4)
                    });
                }
                paymentGroup.Rows.Add(paymentHeaderRow);
                foreach (var paymentItem in paymentDataGrid.ItemsSource)
                {
                    var paymentRow = new TableRow();
                    foreach (var col in nestedSelectedColumns)
                    {
                        var prop = paymentItem.GetType().GetProperty(col);
                        var value = prop?.GetValue(paymentItem);
                        string cellValue;
                        if (value is decimal decVal)
                        {
                            cellValue = "₦ " + decVal.ToString("N2");
                        }
                        else if (value is DateTime dtVal)
                        {
                            cellValue = dtVal.ToString("MM/dd/yy");
                        }
                        else
                        {
                            cellValue = value?.ToString() ?? "";
                        }
                        paymentRow.Cells.Add(new TableCell(new Paragraph(new Run(cellValue)))
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.5),
                            Padding = new Thickness(4),
                            TextAlignment = TextAlignment.Center,
                        });
                    }
                    paymentGroup.Rows.Add(paymentRow);
                }

                // Create a new row and add the total amount for the paymentdatagrid
                var totalPaymentRow = new TableRow();
                totalPaymentRow.Cells.Add(new TableCell(new Paragraph(new Run("Total Payment:")))
                {
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Right,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(4)
                });

                totalPaymentRow.Cells.Add(new TableCell(new Paragraph(new Run($"₦ {paymentDataGrid.ItemsSource.Cast<TransactionItemViewModel>().Sum(p => p.BillPost):N2}")))
                {
                    TextAlignment = TextAlignment.Center,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(4)
                });
                paymentGroup.Rows.Add(totalPaymentRow);

                paymentTable.RowGroups.Add(paymentGroup);
                doc.Blocks.Add(paymentTable);
            }

            doc.Blocks.Add(new Paragraph(new Run($"Total for the period {booking.CheckIn:dd/MM/yy} to {booking.CheckOut:dd/MM/yy} ₦ {totalAmount:N2}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Right
            });

            doc.Blocks.Add(new Paragraph(new Run($"Total Discount: ₦ {totalDiscount:N2}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Right
            });

            doc.Blocks.Add(new Paragraph(new Run($"Tax: ₦ {totalCharge:N2}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Right
            });

            doc.Blocks.Add(new Paragraph(new Run($"Other Service Charges: ₦ {totalServiceCharge:N2}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Right
            });


            doc.Blocks.Add(new Paragraph(new Run($"Total Amount ₦ {totalTAmount:N2}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Right
            });

            doc.Blocks.Add(new Paragraph(new Run($"Amount Paid ₦ {amountPaid:N2}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Right
            });

            if(totalAmount > amountPaid)
            {
                doc.Blocks.Add(new Paragraph(new Run($"Amount to be received ₦ {(totalTAmount - amountPaid):N2}"))
                {
                    FontSize = 11,
                    TextAlignment = TextAlignment.Right
                });
            }
            else
            {
                doc.Blocks.Add(new Paragraph(new Run($"Amount to be refunded ₦ {(amountPaid - totalAmount):N2}"))
                {
                    FontSize = 11,
                    TextAlignment = TextAlignment.Right
                });
            }

            doc.Blocks.Add(new Paragraph(new Run("")));

            doc.Blocks.Add(new Paragraph(new Run($"Guest Signature"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });
            doc.Blocks.Add(new Paragraph(new Run($"{booking.Guest.FullName}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });
            doc.Blocks.Add(new Paragraph(new Run($"{DateTime.Now:dd MMM yyyy h:mm tt}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            return doc;
        }

        public FlowDocument GenerateBillFlowDoc(Hotel hotel, Booking booking)
        {
            const double A4PortraitWidth = 793.7;
            const double A4PortraitHeight = 1122.5;

            var doc = new FlowDocument
            {
                PageWidth = A4PortraitHeight,
                PageHeight = A4PortraitWidth,
                PagePadding = new Thickness(50),
                ColumnWidth = double.PositiveInfinity,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            };

            // ======= HOTEL LOGO ==========
            if (hotel.LogoUrl != null && hotel.LogoUrl.Length > 0)
            {
                var image = new Image
                {
                    Width = 110,
                    Height = 110,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = LoadImage(hotel.LogoUrl)
                };

                var logoContainer = new BlockUIContainer(image)
                {
                    Margin = new Thickness(0, 0, 0, 11),
                    TextAlignment = TextAlignment.Center
                };

                doc.Blocks.Add(logoContainer);
            }

            doc.Blocks.Add(new Paragraph(new Run(hotel.Name))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 11)
            });

            doc.Blocks.Add(new Paragraph(new Run($"Address: {hotel.Address}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run($"Email: {hotel.Email} | Phone: {hotel.PhoneNumber}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run("")));

            Table infoTable = new Table();

            infoTable.Columns.Add(new TableColumn());
            infoTable.Columns.Add(new TableColumn());

            infoTable.Columns[0].Width = new GridLength(A4PortraitWidth);
            infoTable.Columns[1].Width = new GridLength(A4PortraitWidth);

            // Create a row
            TableRow row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run($"Check In\n")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left,
            });

            row.Cells.Add(new TableCell(new Paragraph(new Run($"Check In: {booking.CheckIn:dd MMM yyyy h:mm tt}")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            TableRow row2 = new TableRow();
            row2.Cells.Add(new TableCell(new Paragraph(new Run($"Booking Number: {booking.BookingId}\n")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            row2.Cells.Add(new TableCell(new Paragraph(new Run($"Check Out: {booking.CheckOut:dd MMM yyyy h:mm tt}")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            TableRow row3 = new TableRow();
            row3.Cells.Add(new TableCell(new Paragraph(new Run($"Guest Name: {booking.Guest.FullName}\n")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            row3.Cells.Add(new TableCell(new Paragraph(new Run($"No of Rooms: {1}")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            TableRow row4 = new TableRow();
            row4.Cells.Add(new TableCell(new Paragraph(new Run($"Phone Number: {booking.Guest.PhoneNumber}\n")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            row4.Cells.Add(new TableCell(new Paragraph(new Run($"Email: {booking.Guest.Email}")))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            });

            TableRow row5 = new TableRow();
            row5.Cells.Add(new TableCell(new Paragraph(new Run($"Address: {booking.Guest.Street}, {booking.Guest.City}, {booking.Guest.State}, {booking.Guest.Country}\n")))
            {
                FontSize = 11,
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
            doc.Blocks.Add(infoTable);

            doc.Blocks.Add(new Paragraph(new Run("")));

            doc.Blocks.Add(new Paragraph(new Run("Booking Invoice List (Invoice Settled)"))
            {
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            return doc;
        }

        public FlowDocument PrintGuestInformation(Domain.Entities.FrontDesk.Guest guest, Hotel hotel)
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
                FontSize = 11,
                TextAlignment = TextAlignment.Left
            };

            // ======= HOTEL LOGO ==========
            if (hotel.LogoUrl != null && hotel.LogoUrl.Length > 0)
            {
                var image = new Image
                {
                    Width = 110, // Adjust size as needed
                    Height = 110,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = LoadImage(hotel.LogoUrl)
                };

                var logoContainer = new BlockUIContainer(image)
                {
                    Margin = new Thickness(0, 0, 0, 11),
                    TextAlignment = TextAlignment.Center
                };

                doc.Blocks.Add(logoContainer);
            }

            doc.Blocks.Add(new Paragraph(new Run(hotel.Name))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 11)
            });

            doc.Blocks.Add(new Paragraph(new Run($"Address: {hotel.Address}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run($"Email: {hotel.Email} | Phone: {hotel.PhoneNumber}"))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run("")));

            if (guest.GuestImage != null && guest.GuestImage.Length > 0)
            {
                var image = new Image
                {
                    Width = 150, // Adjust size as needed
                    Height = 150,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = LoadImage(guest.GuestImage)
                };

                var guestImageContaner = new BlockUIContainer(image)
                {
                    Margin = new Thickness(0, 0, 0, 11),
                    TextAlignment = TextAlignment.Center
                };

                doc.Blocks.Add(guestImageContaner);
            }

            doc.Blocks.Add(new Paragraph(new Run($"Personal Information"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Left
            });

            doc.Blocks.Add(new Paragraph(new Run($"Name: {guest.FullName} "))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            doc.Blocks.Add(new Paragraph(new Run($"Email: {guest.Email} "))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            doc.Blocks.Add(new Paragraph(new Run($"Phone Number: {guest.PhoneNumber} "))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            doc.Blocks.Add(new Paragraph(new Run($"Gender: {guest.Gender} "))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            doc.Blocks.Add(new Paragraph(new Run($"Address: {guest.Street} {guest.City} {guest.Country}"))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            doc.Blocks.Add(new Paragraph(new Run($"No of Bookings: {guest.Bookings.Count:N0}"))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            doc.Blocks.Add(new Paragraph(new Run($"Account created by: {guest.ApplicationUser.FullName}"))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            doc.Blocks.Add(new Paragraph(new Run("")));

            doc.Blocks.Add(new Paragraph(new Run($"Means of Identification"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Left
            });


            doc.Blocks.Add(new Paragraph(new Run($"ID Type: {guest.GuestIdentity.IdType}"))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            doc.Blocks.Add(new Paragraph(new Run($"ID Number: {guest.GuestIdentity.IdNumber}"))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            });

            if (guest.GuestIdentity.Document != null && guest.GuestIdentity.Document.Length > 0)
            {
                var image = new Image
                {
                    Width = 200, // Adjust size as needed
                    Height = 200,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = LoadImage(guest.GuestIdentity.Document)
                };

                var idContainer = new BlockUIContainer(image)
                {
                    Margin = new Thickness(0, 0, 0, 11),
                    TextAlignment = TextAlignment.Center
                };

                doc.Blocks.Add(idContainer);
            }

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

        public static void PrintFlowDocument(FlowDocument document, System.Printing.PageOrientation orientation)
        {
            var printDialog = new PrintDialog();

            // Set printer orientation to landscape
            printDialog.PrintTicket.PageOrientation = orientation;

            if (printDialog.ShowDialog() == true)
            {
                if (orientation == System.Printing.PageOrientation.Landscape)
                {
                    // Set the FlowDocument to A4 landscape dimensions (1122.5 x 793.7 at 96 DPI)
                    document.PageWidth = 1122.5;  // A4 Landscape width
                    document.PageHeight = 793.7;  // A4 Landscape height
                }
                else
                {
                    // Set the FlowDocument to A4 portrait dimensions (793.7 x 1122.5 at 96 DPI)
                    document.PageWidth = 793.7;   // A4 Portrait width
                    document.PageHeight = 1122.5; // A4 Portrait height
                }

                document.PagePadding = new Thickness(50);
                document.ColumnWidth = double.PositiveInfinity;

                // Set the paginator to match the landscape size
                IDocumentPaginatorSource idpSource = document;
                var paginator = idpSource.DocumentPaginator;
                paginator.PageSize = new Size(document.PageWidth, document.PageHeight);

                printDialog.PrintDocument(paginator, "Hotel Document Print - Landscape");
            }
        }

        private string SplitCamelCase(string input)
        {
            return Regex.Replace(input, "(?<!^)([A-Z])", " $1");
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
