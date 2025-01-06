using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using LibraryManagementApi.Data;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Layout.Borders;
using iText.Kernel.Pdf.Canvas.Draw;
//using iText.Kernel.Colors;

namespace LibraryManagementApi.Services
{
    public class PdfService
    {
        private readonly LibraryContext _context;
        public PdfService(LibraryContext context)
        {
            _context = context;
            // _emailNotificationService = emailNotificationService;
        }
        public string GeneratePdf(string username,string phoneNumber, string email,string BookName,DateTime BorrowDate, DateTime dueDate)
        {
            // Specify the path where the PDF will be saved
            var pdfPath = Path.Combine(Path.GetTempPath(), $"{username}_BorrowedBookDetails.pdf");

            // Create a PDF document
            using (var writer = new PdfWriter(pdfPath))
            {
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new Document(pdf);
                    //var darkBlueColor = new DeviceRgb(0, 0, 139);
                    // Create a table for the logo and library name
                    var headerTable = new Table(2);
                    headerTable.SetWidth(UnitValue.CreatePercentValue(100)); // Set table width to 100%
                    headerTable.SetMarginBottom(10); // Space below the header

                    // Add logo image
                    string logoPath = "Images/logo.jpg"; // Update this path to your logo file
                    var img = new Image(ImageDataFactory.Create(logoPath))
                        .SetWidth(50) // Set the width of the logo
                        .SetHeight(50); // Set the height of the logo
                    headerTable.AddCell(new Cell().Add(img).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));

                    var libraryName = new Paragraph("SERENITY LIBRARY")
                        .SetFontSize(24)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER);
                    libraryName.Add(new Paragraph("21, Book Street,CrossCut Nagar,Chennai,TamilNadu")
                        .SetFontSize(15)
                        .SetTextAlignment(TextAlignment.CENTER));
                    headerTable.AddCell(new Cell().Add(libraryName).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER));

                    // Add the header table to the document
                    document.Add(headerTable);
                    var line = new LineSeparator(new SolidLine(1)); // Create a line with 1-point thickness
                    line.SetMarginTop(5); // Optional: Add some space above the line
                    document.Add(line);
                    document.Add(new Paragraph("\n"));

                    var ReceiptName = new Paragraph("Borrowed Book Receipt:")
                        .SetFontSize(18)
                        .SetBold()
                        //.SetFontColor(darkBlueColor)
                        .SetTextAlignment(TextAlignment.LEFT);
                    document.Add(ReceiptName);
                    // Add a space before the table
                    document.Add(new Paragraph("\n"));

                    // Create a table with 2 columns
                    var table = new Table(2);
                    table.SetWidth(UnitValue.CreatePercentValue(100)); // Set table width to 100%

                    // Add table headers
                    table.AddHeaderCell("Field").SetBold();
                    table.AddHeaderCell("Details").SetBold();

                    // Add data to the table
                    table.AddCell("Username");
                    table.AddCell(username);
                    table.AddCell("Phone Number");
                    table.AddCell(phoneNumber);
                    table.AddCell("Email");
                    table.AddCell(email);
                    table.AddCell("Book Name");
                    table.AddCell(BookName);
                    table.AddCell("Borrow Date");
                    table.AddCell(BorrowDate.ToShortDateString());
                    table.AddCell("Due Date");
                    table.AddCell(dueDate.ToShortDateString());
                    document.Add(table);
                    document.Add(new Paragraph("\n"));
                    document.Add(new Paragraph("\n"));
                    var ReceiptDetails = new Paragraph("Here are the details of the book you have borrowed.Please take a moment to review the information below")
                        .SetFontSize(12)
                        .SetTextAlignment(TextAlignment.LEFT);
                    document.Add(ReceiptDetails);
                    document.SetMargins(20, 20, 20, 20); // Set document margins

                    document.Close();
                }
            }
            return pdfPath;
        }

    }
}
