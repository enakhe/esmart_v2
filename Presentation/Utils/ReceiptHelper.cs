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

        public static void PrintReceipt(FlowDocument doc)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = doc;
                pd.PrintDocument(idpSource.DocumentPaginator, "Receipt Print");
            }
        }
    }
}
