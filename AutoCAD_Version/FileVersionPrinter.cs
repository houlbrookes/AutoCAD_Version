using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;

namespace AutoCAD_Version
{
    /// <summary>
    /// Prints a list of FileVersion records to a chosen printer
    /// </summary>
    public class FileVersionPrinter : IDisposable
    {
        // First line to display a file version
        const int STARTING_LINE = 6;
        // How much padding the box has
        const float BOX_PADDING = 5F;
        // How many lines down to start the box
        const float HEADER_PADDING_LINES = 3.5F;
        // How many lines to subtract from the box size
        const float FOOTER_PADDING_LINES = 4.5F;

        // Font to write the file version info and page footer
        Font bodyFont;
        // Font for the title block
        Font headingFont;
        // Font for the headings
        Font bodyUnderlineFont;
        // Copy of the FileVersion data to print
        ObservableCollection<FileVersion> FileList { get; set; }
        // Folder path to display in the header
        string AutoCADpath { get; set; }
        // How many pages have been printed
        int PagesSoFar = 0;

        /// <summary>
        /// Constructor for this Class
        /// </summary>
        /// <param name="fileList"></param>
        /// <param name="autoCADpath"></param>
        public FileVersionPrinter(ObservableCollection<FileVersion> fileList, string autoCADpath)
        {
            FileList = fileList;
            AutoCADpath = autoCADpath;
            bodyFont = new Font("Arial", 10);
            bodyUnderlineFont = new Font("Arial", 10, FontStyle.Underline);
            headingFont = new Font("Arial", 14, FontStyle.Bold | FontStyle.Underline);
        }

        // Left, Right and Center formatting
        enum Formatting
        {
            Left, Right, Centre
        }

        // Font being used
        enum FontType
        {
            Normal, NormalUnderline, Heading
        }

        /// <summary>
        /// Print a list of files and versions
        /// </summary>
        /// <param name="path">Folder containing the files</param>
        /// <param name="files">List of files and their versions</param>
        /// <returns>(true, "") if printed successfull else (false, errormessage)</returns>
        public (bool Success, string ErrorMessage) Print()
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(PrintPageHandler);

            var printDialog = new System.Windows.Forms.PrintDialog()
            {
                Document = pd,
                UseEXDialog = true,
            };

            var result = printDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    try
                    {
                        pd.Print();
                        return (Success: true, "Document Printed Successfully");
                    }
                    catch (Exception ex)
                    {
                        return (Success: false, ErrorMessage: ex.Message);
                    }
                case System.Windows.Forms.DialogResult.Cancel:
                    return (Success: false, ErrorMessage: "Print Cancelled");
                default: return (Success: false, ErrorMessage: $"{result}");
            }
        }

        /// <summary>
        /// Calculate the number of printable lines on a page
        /// using the body font.  This can be used to calculate the
        /// number of files that can be displayed per page
        /// </summary>
        /// <param name="graphics">Graphics from Print Document</param>
        /// <param name="marginBoundsHeight">Printable Height of Page</param>
        /// <returns></returns>
        private int LinesPerPage(Graphics graphics, int marginBoundsHeight)
        {
            return (int)Math.Floor(marginBoundsHeight / bodyFont.GetHeight(graphics));
        }

        /// <summary>
        /// Print Page Event from the Print Dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void PrintPageHandler(object sender, PrintPageEventArgs ev)
        {

            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            float rightMargin = ev.MarginBounds.Right;

            // Calculate the number of lines per page.
            int linesPerPage = (int)Math.Floor(ev.MarginBounds.Height / bodyFont.GetHeight(ev.Graphics));
            var filesPerPage = linesPerPage - STARTING_LINE;
            var totalPages = (int)Math.Ceiling(FileList.Count / (float)filesPerPage);

            DrawBox();

            PrintHeader();

            int count = STARTING_LINE - 1;

            var rowsToSkip = PagesSoFar * filesPerPage;

            // Print a line for every file assigned to this page
            foreach (var pair in FileList.Skip(rowsToSkip).Take(filesPerPage))
            {
                PrintLine(count, pair.Filename, Formatting.Left);
                PrintLine(count, pair.Version, Formatting.Right);
                count += 1;
            }

            // Printed the Next Page
            PagesSoFar += 1;

            PrintFooter();

            ev.HasMorePages = PagesSoFar < Math.Ceiling(FileList.Count / (float)filesPerPage);

            return; // return statement to indicate that there is no more code after this point
            
            #region Local Functions Declarations

            // Draw a box around the page area
            void DrawBox()
            {
                // Print a box around the page
                Pen blackPen = new Pen(Color.Black, 1);

                var lineHeight = bodyFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawRectangle(blackPen,
                    ev.MarginBounds.Left - BOX_PADDING,
                    ev.MarginBounds.Top + lineHeight * HEADER_PADDING_LINES,
                    ev.MarginBounds.Right - ev.MarginBounds.Left + 2 * BOX_PADDING,
                    ev.MarginBounds.Bottom - ev.MarginBounds.Top - lineHeight * FOOTER_PADDING_LINES);
            }

            // Prints a string at a given row with given formatting
            void PrintLine(int row, string lineToPrint, Formatting format = Formatting.Left, FontType theFont = FontType.Normal)
            {
                Font fontToUse = null;
                switch (theFont)
                {
                    case FontType.Normal:
                        fontToUse = bodyFont;
                        break;

                    case FontType.Heading:
                        fontToUse = headingFont;
                        break;

                    case FontType.NormalUnderline:
                        fontToUse = bodyUnderlineFont;
                        break;
                }

                var yOffset = topMargin + (row * fontToUse.GetHeight(ev.Graphics));
                var stringLength = ev.Graphics.MeasureString(lineToPrint, fontToUse).Width;
                Single xOffset = 0;
                switch (format)
                {
                    case Formatting.Left:
                        xOffset = leftMargin;
                        break;
                    case Formatting.Right:
                        xOffset = rightMargin - stringLength;
                        break;
                    case Formatting.Centre:
                        xOffset = leftMargin + (rightMargin - leftMargin - stringLength) / 2;
                        break;
                }

                ev.Graphics.DrawString(lineToPrint, fontToUse, Brushes.Black, xOffset, yOffset, new StringFormat());
            }

            // Prints the header
            void PrintHeader()
            {
                PrintLine(0, "AutoCAD Version Report", Formatting.Centre, FontType.Heading);
                PrintLine(2, "Folder: " + AutoCADpath, Formatting.Left, FontType.Normal);
                PrintLine(4, "Filename", Formatting.Left, FontType.NormalUnderline);
                PrintLine(4, "Version", Formatting.Right, FontType.NormalUnderline);
            }

            // Prints the footer
            void PrintFooter()
            {
                PrintLine((int)linesPerPage, string.Format("Page {0} of {1}", PagesSoFar, totalPages), Formatting.Left);
                PrintLine((int)linesPerPage, string.Format("{0:dd MMM yyyy} {1:HH:mm}", DateTime.Now, DateTime.Now), Formatting.Right);
            }

            #endregion
        }

        // the code below is boilerplate code autogenerated by
        // visual studio

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    headingFont.Dispose();
                    bodyFont.Dispose();
                    bodyUnderlineFont.Dispose();
                }

                disposedValue = true;
            }
        }

        ~FileVersionPrinter()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
