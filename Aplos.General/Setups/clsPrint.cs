using Library.Data.Sql;
using System;

using System.Drawing;
using System.Drawing.Printing;
using System.Text;


namespace Library.General.Setups
{
   
    public class clsPrint
    {
        SqlRepository _sqlRepository = new SqlRepository();
        string _userid = ""; string _processid = ""; int _maxduration = 0;
        public clsPrint()
        {
            
        }

        public void Print()
        {
            var doc = new PrintDocument();
            var paperSize = new PaperSize("Custom", 520, 820);
            doc.DefaultPageSettings.PaperSize = paperSize;
            
            doc.PrintPage += new PrintPageEventHandler(ProvideContent);
            
            doc.Print();
        }

        public void ProvideContent(object sender, PrintPageEventArgs e)
        {

            const int FIRST_COL_PAD = 5;
            const int SECOND_COL_PAD = 7;
            const int THIRD_COL_PAD = 8;

            var sb = new StringBuilder();

            //replace with item.Branch

            sb.AppendLine(" ");
            sb.AppendLine(("TAX INVOICE:  "));
            sb.AppendLine(" ");
            sb.AppendLine(("ACCRA GHANA").PadLeft(20));
            sb.AppendLine(("Vat Reg. No.:  "));
            sb.AppendLine(("TEL: 0204355610 & 0204355608 "));
            sb.AppendLine(" ");
            sb.Append(("DATE").PadRight(8));
            sb.AppendLine(": " + DateTime.Now);
            sb.Append(("CASHIER").PadRight(8));
            sb.AppendLine((": "));
            sb.AppendLine(" ");
            //sb.AppendLine("=".PadRight(35,'='));
            sb.Append(("ITEM").PadLeft(15));
            sb.Append(("QTY").PadLeft(FIRST_COL_PAD));
            sb.Append(("PRICE").PadLeft(SECOND_COL_PAD));
            sb.AppendLine(("GH?").PadLeft(THIRD_COL_PAD));
            sb.AppendLine("-".PadRight(60, '-'));



            sb.AppendLine("-".PadRight(60, '-'));
            sb.Append("Sub Total:".PadLeft(13 + FIRST_COL_PAD + SECOND_COL_PAD));

            sb.AppendLine("VAT @ 17.50%: ".PadLeft(13 + FIRST_COL_PAD + SECOND_COL_PAD));
            sb.AppendLine("=".PadRight(50, '='));
            sb.AppendLine("Bill Total:".PadLeft(15 + FIRST_COL_PAD + SECOND_COL_PAD));

            sb.AppendLine("=".PadRight(50, '='));


            var printText = new PrintText(sb.ToString(), new Font(System.Drawing.FontFamily.GenericMonospace, 9, System.Drawing.FontStyle.Bold));
            Graphics graphics = e.Graphics;
            int startX = 0;
            int startY = 0;
            int Offset = 20;

            graphics.DrawString(printText.Text, new Font(System.Drawing.FontFamily.GenericMonospace, 9, System.Drawing.FontStyle.Bold),
                                new SolidBrush(System.Drawing.Color.Black), startX, startY + Offset);
            Offset = Offset + 20;
        }

    }

    public class PrintText
    {
        public PrintText(string text, Font font) : this(text, font, new StringFormat()) { }

        public PrintText(string text, Font font, StringFormat stringFormat)
        {
            Text = text;
            Font = font;
            StringFormat = stringFormat;
        }

        public string Text { get; set; }

        public Font Font { get; set; }

        /// <summary> Default is horizontal string formatting </summary>
        public StringFormat StringFormat { get; set; }
    }
}
