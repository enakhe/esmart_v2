using ESMART.Presentation.Forms.Export;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Paragraph = iText.Layout.Element.Paragraph;
using Table = iText.Layout.Element.Table;

namespace ESMART.Presentation.Utils
{
    public static class ExportHelper
    {
        public static Document ExportAndPrint(DataGrid dataGrid, List<string> selectedColumns, string exportFormat, string fileName, byte[] logoBytes, string companyName, string email, string phone, string address)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var pdfPath = Path.Combine(documentsPath, $"{fileName}_{timestamp}.pdf");

            if (dataGrid.ItemsSource == null)
            {
                MessageBox.Show("No data available to export.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                using (var writer = new PdfWriter(pdfPath))
                {
                    var pdf = new PdfDocument(writer);
                    var doc = new Document(pdf);

                    return new Document(pdf);

                }
            }

            try
            {
                var dataTable = GetDataTableFromDataGrid(dataGrid, selectedColumns);

                var doc = ExportToPdf(dataTable, pdfPath, fileName, logoBytes, companyName, email, phone, address);
                return doc;
            }
            catch (Exception ex)
            {
                throw new Exception($"Export failed: {ex.Message}");
            }
        }

        private static DataTable GetDataTableFromDataGrid(DataGrid dataGrid, List<string> selectedColumns)
        {
            var dt = new DataTable();

            foreach (var col in selectedColumns)
            {
                dt.Columns.Add(col);
            }

            foreach (var item in dataGrid.ItemsSource)
            {
                var row = dt.NewRow();

                foreach (var col in selectedColumns)
                {
                    var prop = item.GetType().GetProperty(col);
                    if (prop != null)
                    {
                        row[col] = prop.GetValue(item)?.ToString() ?? string.Empty;
                    }
                }

                dt.Rows.Add(row);
            }

            return dt;
        }

        private static Document ExportToPdf(DataTable dt, string filePath, string documentTitle, byte[] logoBytes, string companyName, string email, string phone, string address)
        {
            using (var writer = new PdfWriter(filePath))
            {
                var pdf = new PdfDocument(writer);
                var doc = new Document(pdf);

                var logo = new iText.Layout.Element.Image(
                    iText.IO.Image.ImageDataFactory.Create(logoBytes)
                ).ScaleToFit(100, 100);

                var headerTable = new Table(2).UseAllAvailableWidth();

                var namePara = new Paragraph(companyName)
                    .SetFontSize(18)
                    .SimulateBold()
                    .SetFontColor(ColorConstants.BLUE);

                var detailsPara = new Paragraph()
                    .Add($"Email: {email}\n")
                    .Add($"Phone: {phone}\n")
                    .Add($"Address: {address}")
                    .SetFontSize(10);

                var title = new Paragraph(documentTitle)
                    .SetFontSize(13)
                    .SimulateBold()
                    .SetFontColor(ColorConstants.BLUE)
                    .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);

                headerTable.AddCell(new Cell().Add(logo).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                headerTable.AddCell(new Cell()
                    .Add(namePara)
                    .Add(detailsPara)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                );

                // Add header to document
                doc.Add(headerTable);
                doc.Add(new Paragraph("\n"));
                doc.Add(title);
                doc.Add(new Paragraph("\n"));

                // Create the data table
                var table = new Table(dt.Columns.Count).UseAllAvailableWidth();

                var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var cellFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Add headers with color
                foreach (DataColumn col in dt.Columns)
                {
                    table.AddHeaderCell(
                        new Cell().Add(new Paragraph(col.ColumnName)
                        .SetFont(headerFont)
                        .SetFontSize(12)
                        .SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(ColorConstants.BLUE)
                    );
                }

                // Add rows
                foreach (DataRow row in dt.Rows)
                {
                    foreach (var cell in row.ItemArray)
                    {
                        table.AddCell(
                            new Cell().Add(new Paragraph(cell?.ToString() ?? string.Empty)
                            .SetFont(cellFont)
                            .SetFontSize(10))
                            .SetBackgroundColor(ColorConstants.WHITE)
                        );
                    }
                }

                doc.Add(table);
                doc.Close();

                return doc;
            }
        }

        private static void ExportToExcel(DataTable dt, string filePath)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            worksheet.Cell(1, 1).InsertTable(dt);

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
            headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }

        private static void PrintPdf(string filePath)
        {
            try
            {
                using (var document = PdfiumViewer.PdfDocument.Load(filePath))
                {
                    using (var printDoc = document.CreatePrintDocument())
                    {
                        printDoc.PrinterSettings = new PrinterSettings();
                        printDoc.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing PDF: {ex.Message}");
            }
        }

        private static void PrintExcel(string filePath)
        {
            try
            {
                var excelApp = new Microsoft.Office.Interop.Excel.Application();
                var workbook = excelApp.Workbooks.Open(filePath);
                workbook.PrintOutEx();
                workbook.Close(false);
                excelApp.Quit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing Excel: {ex.Message}");
            }
        }
    }
}
