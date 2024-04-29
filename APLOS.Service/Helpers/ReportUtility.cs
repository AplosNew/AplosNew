using HtmlAgilityPack;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Employees;
using OTSBD;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace Library.Service.Helpers
{
    public class ReportUtility
    {

        public string ExcelHtmlTable(IWorksheet sheet, string Directory)
        {
            try
            {
                string filePath = Path.Combine(Directory, System.DateTime.Now.Ticks.ToString());

                sheet.SaveAsHtml(filePath, HtmlSaveOptions.Default);

                string _html = System.IO.File.ReadAllText(filePath, System.Text.Encoding.Unicode);
                System.IO.File.Delete(filePath);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(_html);

                var styles = doc.DocumentNode.SelectNodes("//style").ToList();
                var tds = doc.DocumentNode.SelectNodes("//td").ToList();




                string[] stringSeparators = new string[] { "\r\n" };
                string[] lines = styles[0].InnerText.Split(stringSeparators, StringSplitOptions.None);
                Dictionary<string, string> dicstyles = new Dictionary<string, string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("{") == false)
                        continue;

                    for (int L = 0; L < lines[i].Length; L++)
                    {
                        if (lines[i][L] == '{')
                        {
                            string k = lines[i].Substring(0, L).Replace(".", "");
                            string v = lines[i].Substring(L + 1, lines[i].Length - 1 - L - 1);

                            dicstyles.Add(k, v);
                        }
                    }
                }

                for (int i = 0; i < tds.Count; i++)
                {

                    string styleValue = "";
                    string attrclassname = "";



                    for (int j = 0; j < tds[i].Attributes.Count; j++)
                    {
                        if (tds[i].Attributes[j].Name == "class")
                            attrclassname = tds[i].Attributes[j].Value;

                        if (tds[i].Attributes[j].Name == "class")
                            tds[i].Attributes[j].Remove();
                    }
                    for (int j = 0; j < tds[i].Attributes.Count; j++)
                    {
                        if (tds[i].Attributes[j].Name == "style")
                            styleValue = tds[i].Attributes[j].Value;

                        if (tds[i].Attributes[j].Name == "style")
                            tds[i].Attributes[j].Remove();
                    }


                    if (attrclassname != "")
                    {
                        if (dicstyles.ContainsKey(attrclassname))
                        {
                            tds[i].Attributes.Add("style", dicstyles[attrclassname] + styleValue);

                        }

                    }
                }

                var body = doc.DocumentNode.SelectNodes("//body").ToList();


                return body[0].InnerHtml;
            }
            catch (Exception)
            {

                return "";
            }

        }
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<EmployeeInformation> _EmployeeInformationRepository;

        public ReportUtility()
        {
            _sqlRepository = new SqlRepository();
        }

        public void PasswordSet(ref IWorksheet sheet, string password)
        {
            sheet.Protect(password);
        }

        public void FreezePage(ref IWorksheet sheet, int FirstVisibleColumn, int FirstVisibleRow)
        {
            sheet.UsedRange[GetColumnNameForXls(FirstVisibleColumn) + "" + FirstVisibleRow].FreezePanes();
            sheet.FirstVisibleColumn = FirstVisibleColumn;
            sheet.FirstVisibleRow = FirstVisibleRow;
        }

        /// <summary>
        /// This Function get the number of the column as (int) and Return the Name of the Column as (string)
        /// ColumnNo must be greater or equal 1
        /// As for Example:
        /// 1. If the ColumnNo is equal 3 then this Function returns "C"
        /// 2. If the ColumnNo is equal 27 then this Function returns "AA"
        /// 3. If the ColumnNo is equal 53 then this Function returns "BA"
        /// </summary>
        /// <param name="ColumnNo"></param>
        /// <returns></returns>
        public string GetColumnNameForXls(int ColumnNo)
        {
            ColumnNo = ColumnNo - 1;
            if (ColumnNo < 0)
            {
                return "";
            }

            var CharVelue1 = 0;
            var CharVelue2 = 0;
            char ch1, ch2;
            string ColumnName;
            int reminder, div;

            reminder = ColumnNo % 26;
            div = ColumnNo / 26;

            if (div == 0)
            {
                CharVelue1 = 65;
                CharVelue1 = CharVelue1 + reminder;
            }
            if (div > 0)
            {
                CharVelue1 = 65;
                CharVelue2 = 65;
                CharVelue1 = CharVelue1 + div;
                CharVelue2 = CharVelue2 + reminder;
            }

            if (CharVelue2 == 0)
            {
                ch1 = (char)CharVelue1;
                ColumnName = "" + ch1;
            }
            else
            {
                CharVelue1 = CharVelue1 - 1;
                ch1 = (char)CharVelue1;
                ch2 = (char)CharVelue2;
                ColumnName = "" + ch1 + ch2;
            }

            return ColumnName;
        }

        public string NumberFormatDecimalTwo()
        {
            return "#,##0.00;(#,##0.00);* ??;@";
            // return "#,##0.00;#,##0.00";
        }
        public string NumberFormatDecimalThree()
        {
            return "#,##0.000;(#,##0.000);* ??;@";
        }
        public string NumberFormatDecimalFour()
        {
            return "#,##0.000;(#,##0.0000);* ??;@";
        }
        public string NumberFormatNegativeSignDelimeterDecimalTwo()
        {
            return "#,##0.00_);#,##0.00";
        }

        public string NumberFormatInt()
        {
            return "###0;";
        }
        public string NumberFormatIntLocal(string language)
        {
            if (language == "Bengali")
            { return "[$-5000445]#,##0;(#,##0);* ??;@"; }
            if (language == "Hindi")
            { return "[$-4000400]#,##0;(#,##0);* ??;@"; }
            else { return "#,##0;(#,##0);* ??;@"; }

        }
        public string NumberFormatDecimalLocal(string language)
        {
            if (language == "Bengali")
            { return "[$-5000445]#,##0.00;(#,##0.00);* ??;@"; }
            if (language == "Hindi")
            { return "[$-4000400]#,##0.00;(#,##0.00);* ??;@"; }
            else { return "#,##0.00;(#,##0.00);* ??;@"; }

        }
       
        public void SetSLText(ref IWorksheet sheet, int row, int col, int txt)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatInt();
            //sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);
        }
        public void SetNumberText(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt;
            //sheet.Range[row, col].CellStyle.IsFirstSymbolApostrophe = false;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            //sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);
        }
        public void SetText(ref IWorksheet sheet, int row, int col, int txt)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatInt();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
        }
        public void SetTextBorder(ref IWorksheet sheet, int row, int col, int txt)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatInt();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);

        }
        public void SetTextBorder(ref IWorksheet sheet, int row, int col, double txt)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatInt();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);

        }
        public void SetTextBorder(ref IWorksheet sheet, int row, int col, double txt, double colWIdth)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatInt();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, col].ColumnWidth = colWIdth;


        }
        public void SetTextBorder(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatInt();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);

        }
        public void SetTextBorder(ref IWorksheet sheet, int row, int col, string txt, double colWIdth)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatInt();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, col].ColumnWidth = colWIdth;


        }
        public void SetText(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt;
            // dailydaystatus
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            // sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);


        }
        public void SetTextWrapText(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt;
            // dailydaystatus
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].WrapText = true;

        }
        public void SetTextWrapText(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].HorizontalAlignment = al;
        }

        public void SetTextBdr(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);
        }

        public void SetText(ref IWorksheet sheet, int row, int col, int txt, ExcelVAlign a)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatInt();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = a;
        }

        public void SetText(ref IWorksheet sheet, int row, int col, int txt, ExcelHAlign al)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatInt();
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].HorizontalAlignment = al;
        }

        public void SetText(ref IWorksheet sheet, int row, int col, double txt)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatDecimalTwo();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);

        }
        public void SetTextDecimalThree(ref IWorksheet sheet, int row, int col, double txt)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatDecimalThree();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);

        }

        public void NumberFormatDecimalFour(ref IWorksheet sheet, int row, int col, double txt)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatDecimalFour();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);

        }

        public void SetText(ref IWorksheet sheet, int row, int col, double txt, int Number)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = clsStaticInfo.NumberFormat(Number);
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);

        }

        public void SetText(ref IWorksheet sheet, int row, int col, double txt, ExcelHAlign al)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatDecimalFour();
            sheet.Range[row, col].HorizontalAlignment = al;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
        }

        public void SetText(ref IWorksheet sheet, int row, int col, double txt, bool IsBold)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatDecimalTwo();
            sheet.Range[row, col].CellStyle.Font.Bold = IsBold;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
        }

        public void SetText(ref IWorksheet sheet, int row, int col, double txt, bool IsBold, ExcelLineStyle bs)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatDecimalTwo();
            sheet.Range[row, col].CellStyle.Font.Bold = IsBold;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(bs);
        }
        public void SetText(ref IWorksheet sheet, int row, int col, string txt, bool IsBold, ExcelLineStyle bs)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].CellStyle.Font.Bold = IsBold;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(bs);
        }
        public void SetText(ref IWorksheet sheet, int row, int col, string txt, bool IsBold)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].CellStyle.Font.Bold = IsBold;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
        }
        public void SetTextLeftAlign(ref IWorksheet sheet, int row, int col, string txt, bool IsBold, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].CellStyle.Font.Bold = IsBold;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
        }
        public void SetTextMiddle(ref IWorksheet sheet, int row, int col, string txt, bool IsBold)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].CellStyle.Font.Bold = IsBold;
            sheet.Range[row, col].CellStyle.Font.Size = 10;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
        }

        public void SetTextMiddle(ref IWorksheet sheet, int row, int col, string txt, bool IsBold, int rowHeight, int fontSize)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].CellStyle.Font.Bold = IsBold;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].CellStyle.Font.Size = fontSize;
            sheet.Range[row, col].RowHeight = rowHeight;
            sheet.Range[row, col].CellStyle.Font.FontName = "Arial Narrow";




        }

        public void SetText(ref IWorksheet sheet, int row, int col, string txt, int width, int rowheight, int fontSize)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatDecimalTwo();
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].RowHeight = rowheight;
            sheet.Range[row, col].CellStyle.Font.Size = fontSize;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);


        }
        public void SetText(ref IWorksheet sheet, int row, int col, string txt, int width, int rowheight, int colWidth, int fontSize)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatDecimalTwo();
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].RowHeight = rowheight;
            sheet.Range[row, col].ColumnWidth = colWidth;
            sheet.Range[row, col].CellStyle.Font.Size = rowheight;



        }
        public void SetText(ref IWorksheet sheet, int row, int col, string txt, bool IsBold, bool wrapText)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].WrapText = wrapText;
            sheet.Range[row, col].CellStyle.Font.Bold = IsBold;
            sheet.Range[row, col].RowHeight = 27;
        }

        public void SetFormula(ref IWorksheet sheet, int row, int col, string txt, bool isDecimal)
        {
            sheet.Range[row, col].Formula = txt;
            if (isDecimal)
                sheet.Range[row, col].NumberFormat = NumberFormatDecimalTwo();
            else
                sheet.Range[row, col].NumberFormat = NumberFormatInt();
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
        }
        public void SetFormulaDay(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Formula = txt;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
        }

        public void SetText(ref IWorksheet sheet, int row, int col, string txt, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].HorizontalAlignment = al;
        }

        public void SetTextEntity(ref IWorksheet sheet, int row, int col, string entity, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = entity;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].HorizontalAlignment = al;
        }

        public void SetText(IWorksheet sheet, int row, int col, string txt, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].HorizontalAlignment = al;
        }

        public void SetText(ref IWorksheet sheet, int row, int col, string txt, int width)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        }
        public void SetHeaderTextWB(ref IWorksheet sheet, int row, int col, string txt, int width)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);

        }
        public void SetText(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].HorizontalAlignment = al;
        }

        public void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {
            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
        }

        public void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            string NumberFormatString = "#,##0;(#,##0)";
            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
        }

        public void SetCellTextNoForamt(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
        }
        public void SetCellTextBold(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            string NumberFormatString = "#,##0.00;(#,##0.00)";
            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
        }
        public void SetCellTextBold(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
        }

        public void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Value, bool HasDecimal)
        {
            string NumberFormatString = "#,##0.00;(#,##0.00);* ??;@";//#,##0.00;(#,##0.00)
            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
        }

        public string InWord(double amount, string CurrencyId)
        {
            var _result = string.Empty;
            var _Amount = amount.ToString();
            var _TotalAmount = amount.ToString();

            var inwords = string.Empty;
            var BaseCurrency = string.Empty;
            var LargeUnit = string.Empty;
            var SmallUnit = string.Empty;
            try
            {
                var sql = "Select Id, [Description], LargeUnit, SmallUnit, InWordFormat from [SCS].[Currency] where Id='" + CurrencyId + "'";
                var dsLocal = _sqlRepository.GetDataTable(sql);
                if (dsLocal.Rows.Count > 0)
                {
                    BaseCurrency = dsLocal.Rows[0]["Description"].ToString();
                    LargeUnit = dsLocal.Rows[0][nameof(LargeUnit)].ToString();
                    SmallUnit = dsLocal.Rows[0][nameof(SmallUnit)].ToString();
                    inwords = "";
                    if (dsLocal.Rows[0]["InWordFormat"].ToString().ToUpper() == "SUBCONTINENT")
                        inwords += Helpers.InWord.SpellAmountInIndiaSubConWay(_Amount, LargeUnit, SmallUnit, "only.");
                    else if (dsLocal.Rows[0]["InWordFormat"].ToString().ToUpper() == "INTERNATIONAL")
                        inwords += Helpers.InWord.SpellAmountInIntlWay(_Amount, LargeUnit, SmallUnit, "only.");
                    else
                        inwords += Helpers.InWord.SpellAmountInIndiaSubConWay(_Amount, LargeUnit, SmallUnit, "only.");
                    _result = inwords;
                }
                else
                {
                    inwords += Helpers.InWord.SpellAmountInIndiaSubConWay(_Amount, LargeUnit, SmallUnit, "only.");
                    _result = inwords;
                }

                return _result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string InWordNew(double amount, string CurrencyId)
        {
            var _result = string.Empty;
            var _Amount = amount.ToString();
            var _TotalAmount = amount.ToString();

            var inwords = string.Empty;
            var BaseCurrency = string.Empty;
            var LargeUnit = string.Empty;
            var SmallUnit = string.Empty;
            try
            {
                var sql = "Select Id, [Description], LargeUnit, SmallUnit, InWordFormat from [SCS].[Currency] where Id='" + CurrencyId + "'";
                var dsLocal = _sqlRepository.GetDataTable(sql);
                if (dsLocal.Rows.Count > 0)
                {
                    BaseCurrency = dsLocal.Rows[0]["Description"].ToString();
                    LargeUnit = dsLocal.Rows[0][nameof(LargeUnit)].ToString();
                    SmallUnit = dsLocal.Rows[0][nameof(SmallUnit)].ToString();
                    inwords = "";
                    if (dsLocal.Rows[0]["InWordFormat"].ToString().ToUpper() == "SUBCONTINENT")
                        inwords += Helpers.InWord.SpellAmountInIndiaSubConWayNew(_Amount, LargeUnit, SmallUnit);
                    else if (dsLocal.Rows[0]["InWordFormat"].ToString().ToUpper() == "INTERNATIONAL")
                        inwords += Helpers.InWord.SpellAmountInIndiaSubConWayNew(_Amount, LargeUnit, SmallUnit);
                    else
                        inwords += Helpers.InWord.SpellAmountInIndiaSubConWayNew(_Amount, LargeUnit, SmallUnit);
                    _result = inwords;
                }
                else
                {
                    inwords += Helpers.InWord.SpellAmountInIndiaSubConWayNew(_Amount, LargeUnit, SmallUnit);
                    _result = inwords;
                }

                return _result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetWorkbook(ref ExcelEngine excelEngine, int TotalSheet)
        {
            excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            var workbook = application.Workbooks.Create(TotalSheet);
            return workbook;
        }

        #region PageSetup

        public void PageSetup(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PageSetupMarginLeft(ref sheet, xlsColumnHeader, po, identity.FullName);
        }

        public void PageSetup2(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PageSetupMarginLeft2(ref sheet, xlsColumnHeader, po, identity.FullName);
        }
        public void PageSetup3(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PageSetupMarginLeft3(ref sheet, xlsColumnHeader, po, identity.FullName);
        }

        public void PageSetupMarginLeft3(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po, string userName)
        {
            try
            {
                sheet.PageSetup.TopMargin = 0.4;
                sheet.PageSetup.BottomMargin = 0.2;
                sheet.PageSetup.PrintTitleRows = "$" + xlsColumnHeader + ":$" + xlsColumnHeader + "";
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&P" + " of " + "&N";
                //sheet.PageSetup.RightFooter = "&p";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt");
                sheet.PageSetup.LeftMargin = 0.3;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = po;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.PrintGridlines = false;
                sheet.PageSetup.CenterVertically = false;
                sheet.IsDisplayZeros = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PageSetup4(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PageSetupMarginLeft4(ref sheet, xlsColumnHeader, po, identity.FullName);
        }
        public void PageSetupMarginLeft4(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po, string userName)
        {
            try
            {
                sheet.PageSetup.TopMargin = 0.4;
                sheet.PageSetup.BottomMargin = 0.2;
                sheet.PageSetup.PrintTitleRows = "$" + xlsColumnHeader + ":$" + xlsColumnHeader + "";
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&P" + " of " + "&N";
                //sheet.PageSetup.RightFooter = "&p";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt");
                sheet.PageSetup.LeftMargin = 0.1;
                sheet.PageSetup.RightMargin = 0.1;
                sheet.PageSetup.Orientation = po;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.PrintGridlines = false;
                sheet.PageSetup.CenterVertically = false;
                sheet.IsDisplayZeros = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PageSetupMarginLeft2(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po, string userName)
        {
            try
            {
                sheet.PageSetup.TopMargin = 0.4;
                sheet.PageSetup.BottomMargin = 0.2;
                sheet.PageSetup.PrintTitleRows = "$" + xlsColumnHeader + ":$" + xlsColumnHeader + "";
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&P" + " of " + "&N";
                //sheet.PageSetup.RightFooter = "&p";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt");
                sheet.PageSetup.LeftMargin = 0.3;
                sheet.PageSetup.RightMargin = 0.3;
                sheet.PageSetup.Orientation = po;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.PrintGridlines = false;
                sheet.PageSetup.CenterVertically = false;
                sheet.IsDisplayZeros = false;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public void PageSetupMarginLeft(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po, string userName)
        {
            try
            {
                sheet.PageSetup.TopMargin = 0.5;
                sheet.PageSetup.BottomMargin = 0.6;
                sheet.PageSetup.PrintTitleRows = "$" + xlsColumnHeader + ":$" + xlsColumnHeader + "";
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&P" + " of " + "&N";
                //sheet.PageSetup.RightFooter = "&p";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt");
                sheet.PageSetup.LeftMargin = 0.75;
                sheet.PageSetup.RightMargin = 0.50;
                sheet.PageSetup.Orientation = po;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.PrintGridlines = false;
                sheet.PageSetup.CenterVertically = false;
                sheet.IsDisplayZeros = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PageSetup(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po, string userName)
        {
            try
            {
                sheet.PageSetup.TopMargin = 0.5;
                sheet.PageSetup.BottomMargin = 0.2;
                sheet.PageSetup.PrintTitleRows = "$" + xlsColumnHeader + ":$" + xlsColumnHeader + "";
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.RightFooter = "&p";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt");
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = po;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.PrintGridlines = false;
                sheet.PageSetup.CenterVertically = false;
                sheet.IsDisplayZeros = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PageSetupAuto(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po, string userName)
        {
            try
            {
                sheet.PageSetup.TopMargin = 0.5;
                sheet.PageSetup.BottomMargin = 1;
                sheet.PageSetup.PrintTitleRows = "$" + xlsColumnHeader + ":$" + xlsColumnHeader + "";
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.RightFooter = "&p";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Generated By: " + userName + "\n" + "Generated Date & Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt");
                sheet.PageSetup.LeftMargin = 0.5;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = po;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.PrintGridlines = false;
                sheet.PageSetup.CenterVertically = false;
                sheet.IsDisplayZeros = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PageAdjustableSetup(ref IWorksheet sheet, int xlsColumnHeader, int rowPrint, ExcelPageOrientation po)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PageAdjustableSetup(ref sheet, xlsColumnHeader, rowPrint, po, identity.FullName);
        }

        public void PageAdjustableSetup(ref IWorksheet sheet, int xlsColumnHeader, int rowPrint, ExcelPageOrientation po, string userName)
        {
            try
            {
                sheet.PageSetup.TopMargin = 0.5;
                sheet.PageSetup.BottomMargin = 1;
                sheet.PageSetup.PrintTitleRows = "$" + xlsColumnHeader + ":$" + xlsColumnHeader + "";
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.RightFooter = "&p";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt");
                sheet.PageSetup.LeftMargin = 0.5;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = po;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperEnvelopeC5;
                sheet.PageSetup.PrintGridlines = false;
                sheet.PageSetup.CenterVertically = false;
                sheet.IsDisplayZeros = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion PageSetup

        #region Report Header

        public void MainCompanyGroupHeader(ref IWorksheet sheet, int lastCol, string sheetHeader, string companyGroupId)
        {
            var sql = @"SELECT com.Id, com.username, com.LegalName, am.Address1, am.Address2, co.UserName AS Country, ct.UserName AS City
	                    , cm.Phone1 AS Phone, cm.Email1 AS Email, cm.Website AS Website
	                    , ar.UserName AS Area, am.Address1+', '+ar.UserName+', '+ct.UserName Address
                        , cm.Phone1+', '+cm.Email1+', '+cm.Website Contact,com.Image
                        FROM ORG.CompanyGroup AS com
                        LEFT OUTER JOIN MST.AddressMaster AS am ON am.Id = com.AddressMasterId
                        LEFT OUTER JOIN MST.ContactMaster AS cm ON cm.Id = com.ContactMasterId
                        LEFT OUTER JOIN SCS.Country AS co ON co.Id = am.CountryId
                        LEFT OUTER JOIN SCS.City AS ct ON ct.Id = am.CityId
                        LEFT OUTER JOIN SCS.Area AS ar ON ar.Id = am.AreaId
                        WHERE com.Id= '" + companyGroupId + "'";
            var companyGroup = _sqlRepository.GetDataTable(sql);
            if (companyGroup.Rows.Count == 0)
                throw new CustomException("Company Group information not found!");
            Header(sheet, lastCol, sheetHeader, companyGroup, false);
        }

        public void CompanyGroupHeader(ref IWorksheet sheet, int lastCol, string sheetHeader, string companyGroupId)
        {
            MainCompanyGroupHeader(ref sheet, lastCol, sheetHeader, companyGroupId);
        }

        public void CompanyGroupHeaderPhoenix(ref IWorksheet sheet, int lastCol, string sheetHeader, string companyGroupId)
        {
            var sql = @"SELECT COM.Id, COM.Name LegalName, Address FROM CompanyGroup AS COM WHERE COM.Id='" + companyGroupId + "'";
            var companyGroup = _sqlRepository.GetDataTable(sql);
            if (companyGroup.Rows.Count == 0)
                throw new CustomException("Company Group information not found!");
            Header(sheet, lastCol, sheetHeader, companyGroup, false);
        }

        public void CompanyHeader(ref IWorksheet sheet, int lastCol, string sheetHeader, string companyId)
        {
            try
            {
                var sql = @"SELECT COM.Id, COM.UserName, COM.LegalName, COM.WebDomain, AM.Address1, AM.Address2, CO.UserName AS Country, CT.UserName AS City, CM.Phone1 AS Phone, CM.Email1 AS Email
                        , CM.Website AS Website, AR.UserName AS Area
                        , [Address]=CASE ISNULL(AM.Address1,'') WHEN '' THEN '' ELSE AM.Address1 +', ' END+
			                        CASE ISNULL(AR.UserName,'') WHEN '' THEN '' ELSE AR.UserName +', ' END+
			                        CASE ISNULL(CT.UserName,'') WHEN '' THEN '' ELSE ct.UserName END
                        , Contact=CASE ISNULL(CM.Phone1,'') WHEN '' THEN '' ELSE CM.Phone1 +', ' END+
		                        CASE ISNULL(CM.Email1,'') WHEN '' THEN '' ELSE CM.Email1 +', ' END+
		                        CASE ISNULL(CM.Website ,'') WHEN '' THEN '' ELSE CM.Website  END
                        FROM [ORG].[Company] AS COM
                        LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=COM.AddressMasterId
                        LEFT JOIN [MST].[ContactMaster] AS CM ON CM.Id=COM.ContactMasterId
                        LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
                        LEFT JOIN [SCS].[City] AS CT ON CT.Id=AM.CityId
                        LEFT JOIN [SCS].[Area] AS AR ON AR.Id=AM.AreaId
                        WHERE COM.Id='" + companyId + "'";
                var company = _sqlRepository.GetDataTable(sql);
                if (company.Rows.Count == 0)
                    throw new CustomException("Company information not found!");
                Header(sheet, lastCol, sheetHeader, company, false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CompanyPlantHeader(ref IWorksheet sheet, int lastCol, string sheetHeader, string companyId, string plantName, string date)
        {
            try
            {
                var sql = @"SELECT COM.Id, COM.UserName, COM.LegalName, COM.WebDomain

                , AM.Address1

                , AM.Address2, CO.UserName AS Country, CT.UserName AS City, CM.Phone1 AS Phone, CM.Email1 AS Email
                        , CM.Website AS Website, AR.UserName AS Area
                        , [Address]=CASE ISNULL(AM.Address1,'') WHEN '' THEN '' ELSE AM.Address1 +', ' END+
			                        CASE ISNULL(AR.UserName,'') WHEN '' THEN '' ELSE AR.UserName +', ' END+
			                        CASE ISNULL(CT.UserName,'') WHEN '' THEN '' ELSE ct.UserName END
                        , Contact=CASE ISNULL(CM.Phone1,'') WHEN '' THEN '' ELSE CM.Phone1 +', ' END+
		                        CASE ISNULL(CM.Email1,'') WHEN '' THEN '' ELSE CM.Email1 +', ' END+
		                        CASE ISNULL(CM.Website ,'') WHEN '' THEN '' ELSE CM.Website  END
                        FROM [ORG].[Company] AS COM
							left join ORG.Plant as PL on PL.CompanyId=COM.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PL.AddressMasterId
                        LEFT JOIN [MST].[ContactMaster] AS CM ON CM.Id=COM.ContactMasterId
                        LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
                        LEFT JOIN [SCS].[City] AS CT ON CT.Id=AM.CityId
                        LEFT JOIN [SCS].[Area] AS AR ON AR.Id=AM.AreaId
                        WHERE COM.Id='" + companyId + @"'";
                var company = _sqlRepository.GetDataTable(sql);
                if (company.Rows.Count == 0)
                    throw new CustomException("Company information not found!");
                PlantHeader(sheet, lastCol, sheetHeader, company, plantName, false, date);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CompanyPlantHeader(ref IWorksheet sheet, int lastCol, string sheetHeader, string companyId, string plantId, string plantName, string date)
        {
            try
            {
                var sql = @"SELECT COM.Id, COM.UserName, COM.LegalName, COM.WebDomain, AM.Address1, AM.Address2, CO.UserName AS Country, CT.UserName AS City, CM.Phone1 AS Phone, CM.Email1 AS Email
                        , CM.Website AS Website, AR.UserName AS Area
                        , [Address]=CASE ISNULL(AM.Address1,'') WHEN '' THEN '' ELSE AM.Address1 +', ' END+
			                        CASE ISNULL(AR.UserName,'') WHEN '' THEN '' ELSE AR.UserName +', ' END+
			                        CASE ISNULL(CT.UserName,'') WHEN '' THEN '' ELSE ct.UserName END
                        , Contact=CASE ISNULL(CM.Phone1,'') WHEN '' THEN '' ELSE CM.Phone1 +', ' END+
		                        CASE ISNULL(CM.Email1,'') WHEN '' THEN '' ELSE CM.Email1 +', ' END+
		                        CASE ISNULL(CM.Website ,'') WHEN '' THEN '' ELSE CM.Website  END
                        FROM [ORG].[Company] AS COM
						left join ORG.Plant as PL on PL.CompanyId=COM.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PL.AddressMasterId
                        LEFT JOIN [MST].[ContactMaster] AS CM ON CM.Id=COM.ContactMasterId
                        LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
                        LEFT JOIN [SCS].[City] AS CT ON CT.Id=AM.CityId
                        LEFT JOIN [SCS].[Area] AS AR ON AR.Id=AM.AreaId
                        WHERE COM.Id='" + companyId + "'and PL.Id='" + plantId + @"'";
                var company = _sqlRepository.GetDataTable(sql);
                if (company.Rows.Count == 0)
                    throw new CustomException("Company information not found!");
                PlantHeader(sheet, lastCol, sheetHeader, company, plantName, false, date);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public void CompanyPlantHeader2(ref IWorksheet sheet, int lastCol, string sheetHeader, string companyId, string plantId, string plantName, string date)
        {
            try
            {
                var sql = @"SELECT COM.Id, COM.UserName, COM.LegalName, COM.WebDomain, AM.Address1, AM.Address2, CO.UserName AS Country, CT.UserName AS City, CM.Phone1 AS Phone, CM.Email1 AS Email
                        , CM.Website AS Website, AR.UserName AS Area
                        , [Address]=CASE ISNULL(AM.Address1,'') WHEN '' THEN '' ELSE AM.Address1 +', ' END+
			                        CASE ISNULL(AR.UserName,'') WHEN '' THEN '' ELSE AR.UserName +', ' END+
			                        CASE ISNULL(CT.UserName,'') WHEN '' THEN '' ELSE ct.UserName END
                        , Contact=CASE ISNULL(CM.Phone1,'') WHEN '' THEN '' ELSE CM.Phone1 +', ' END+
		                        CASE ISNULL(CM.Email1,'') WHEN '' THEN '' ELSE CM.Email1 +', ' END+
		                        CASE ISNULL(CM.Website ,'') WHEN '' THEN '' ELSE CM.Website  END
                        FROM [ORG].[Company] AS COM
						left join ORG.Plant as PL on PL.CompanyId=COM.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PL.AddressMasterId
                        LEFT JOIN [MST].[ContactMaster] AS CM ON CM.Id=COM.ContactMasterId
                        LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
                        LEFT JOIN [SCS].[City] AS CT ON CT.Id=AM.CityId
                        LEFT JOIN [SCS].[Area] AS AR ON AR.Id=AM.AreaId
                        WHERE COM.Id='" + companyId + "'and PL.Id='" + plantId + @"'";
                var company = _sqlRepository.GetDataTable(sql);
                if (company.Rows.Count == 0)
                    throw new CustomException("Company information not found!");
                PlantHeader2(sheet, lastCol, sheetHeader, company, plantName, false, date);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CompanyPlantHeaderNew(ref IWorksheet sheet, int StartCol, string sheetHeader, string companyId, string plantName, string date)
        {
            try
            {
                var sql = @"SELECT COM.Id, COM.UserName, COM.LegalName, COM.WebDomain, AM.Address1, AM.Address2, CO.UserName AS Country, CT.UserName AS City, CM.Phone1 AS Phone, CM.Email1 AS Email
                        , CM.Website AS Website, AR.UserName AS Area
                        , [Address]=CASE ISNULL(AM.Address1,'') WHEN '' THEN '' ELSE AM.Address1 +', ' END+
			                        CASE ISNULL(AR.UserName,'') WHEN '' THEN '' ELSE AR.UserName +', ' END+
			                        CASE ISNULL(CT.UserName,'') WHEN '' THEN '' ELSE ct.UserName END
                        , Contact=CASE ISNULL(CM.Phone1,'') WHEN '' THEN '' ELSE CM.Phone1 +', ' END+
		                        CASE ISNULL(CM.Email1,'') WHEN '' THEN '' ELSE CM.Email1 +', ' END+
		                        CASE ISNULL(CM.Website ,'') WHEN '' THEN '' ELSE CM.Website  END
                        FROM [ORG].[Company] AS COM
                        LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=COM.AddressMasterId
                        LEFT JOIN [MST].[ContactMaster] AS CM ON CM.Id=COM.ContactMasterId
                        LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
                        LEFT JOIN [SCS].[City] AS CT ON CT.Id=AM.CityId
                        LEFT JOIN [SCS].[Area] AS AR ON AR.Id=AM.AreaId
                        WHERE COM.Id='" + companyId + "'";
                var company = _sqlRepository.GetDataTable(sql);
                if (company.Rows.Count == 0)
                    throw new CustomException("Company information not found!");

                PlantHeaderNew(sheet, StartCol, sheetHeader, company, plantName, false, date);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PlantHeader(ref IWorksheet sheet, int lastCol, string sheetHeader, string plantId)
        {
            try
            {
                var sql = @"SELECT com.Id,com.CompanyId, com.username LegalName, '' WebDomain, am.Address1, am.Address2, co.UserName AS Country, ct.UserName AS City
                        ,'' AS Phone, '' AS Email, '' AS Website, ar.UserName AS Area
                        , am.Address1+', '+ar.UserName+', '+ct.UserName Address, '' Contact,cmp.Image CompanyImage
                        FROM ORG.Plant AS com
						left join ORG.Company cmp on cmp.Id = com.CompanyId
                        LEFT OUTER JOIN MST.AddressMaster AS am ON am.Id = com.AddressMasterId
                        LEFT OUTER JOIN SCS.Country AS co ON co.Id = am.CountryId
                        LEFT OUTER JOIN SCS.City AS ct ON ct.Id = am.CityId
                        LEFT OUTER JOIN SCS.Area AS ar ON ar.Id = am.AreaId
                        WHERE com.Id = '" + plantId + "'";
                var plant = _sqlRepository.GetDataTable(sql);
                if (plant.Rows.Count == 0)
                    throw new CustomException("Plant information not found!");
                if (lastCol > 0)
                {
                    Header(sheet, lastCol, sheetHeader, plant, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PlantHeaderPayment(ref IWorksheet sheet, int lastCol, string sheetHeaderText, string sheetHeader, string plantId)
        {
            try
            {
                var sql = @"SELECT com.Id,com.CompanyId, com.username LegalName, '' WebDomain, am.Address1, am.Address2, co.UserName AS Country, ct.UserName AS City
                        ,'' AS Phone, '' AS Email, '' AS Website, ar.UserName AS Area
                        , am.Address1+', '+ar.UserName+', '+ct.UserName Address, '' Contact,cmp.Image CompanyImage
                        FROM ORG.Plant AS com
						left join ORG.Company cmp on cmp.Id = com.CompanyId
                        LEFT OUTER JOIN MST.AddressMaster AS am ON am.Id = com.AddressMasterId
                        LEFT OUTER JOIN SCS.Country AS co ON co.Id = am.CountryId
                        LEFT OUTER JOIN SCS.City AS ct ON ct.Id = am.CityId
                        LEFT OUTER JOIN SCS.Area AS ar ON ar.Id = am.AreaId
                        WHERE com.Id = '" + plantId + "'";
                var plant = _sqlRepository.GetDataTable(sql);
                if (plant.Rows.Count == 0)
                    throw new CustomException("Plant information not found!");
                if (lastCol > 0)
                {
                    HeaderPayment(sheet, lastCol, sheetHeaderText, sheetHeader, plant, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void PlantHeaderWithOutLogo(ref IWorksheet sheet, int lastCol, string sheetHeader, string plantId)
        {
            try
            {
                var sql = @"SELECT com.Id,com.CompanyId, com.username LegalName, '' WebDomain, am.Address1, am.Address2, co.UserName AS Country, ct.UserName AS City
                        ,'' AS Phone, '' AS Email, '' AS Website, ar.UserName AS Area
                        , am.Address1+', '+ar.UserName+', '+ct.UserName Address, '' Contact
                        FROM ORG.Plant AS com
                        LEFT OUTER JOIN MST.AddressMaster AS am ON am.Id = com.AddressMasterId
                        LEFT OUTER JOIN SCS.Country AS co ON co.Id = am.CountryId
                        LEFT OUTER JOIN SCS.City AS ct ON ct.Id = am.CityId
                        LEFT OUTER JOIN SCS.Area AS ar ON ar.Id = am.AreaId
                        WHERE com.Id = '" + plantId + "'";
                var plant = _sqlRepository.GetDataTable(sql);
                if (plant.Rows.Count == 0)
                    throw new CustomException("Plant information not found!");
                if (lastCol > 0)
                {
                    HeaderWithOutLogo(sheet, lastCol, sheetHeader, plant, false);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Header(IWorksheet sheet, int lastCol, string sheetHeader, DataTable dt, bool isWithLogo)
        {
            Image companyLogo = null;
            string strPath = "";
            int additionalColumn = 1;
            //isWithLogo = false;
            if (isWithLogo)
            {
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dt.Rows[0]["CompanyImage"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                    additionalColumn = 3;
                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.1);
                            int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(3) + sheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                }
            }
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1"].RowHeight = 25;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1"].CellStyle.Font.Size = 14;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].Merge();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.Font.Bold = true;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1"].Text = dt.Rows[0]["LegalName"].ToString();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2"].RowHeight = 15;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2"].CellStyle.Font.Size = 10;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2" + ":" + GetColumnNameForXls(lastCol) + "2"].Merge();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

            var address = dt.Rows[0]["Address1"].ToString().Replace("\n", "");
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2"].Text = address;


            sheet.Range[GetColumnNameForXls(additionalColumn) + "3"].RowHeight = 15;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3"].CellStyle.Font.Size = 10;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].Merge();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.Font.Bold = true;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3"].Text = sheetHeader;
            //if (isWithLogo)
            //{
            //    var image = ResourcesPathReader.GetLogoOrImagePath() + dt.Rows[0]["Image"];
            //    if (File.Exists(image))
            //        sheet.Pictures.AddPicture(1, lastCol, 3, lastCol + 1, image);
            //}
        }
        public void HeaderPayment(IWorksheet sheet, int lastCol, string sheetHeaderText, string sheetHeader, DataTable dt, bool isWithLogo)
        {
            Image companyLogo = null;
            string strPath = "";
            int additionalColumn = 1;
            //isWithLogo = false;
            if (isWithLogo)
            {
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dt.Rows[0]["CompanyImage"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                    additionalColumn = 3;
                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.1);
                            int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(3) + sheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                }
            }
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1"].RowHeight = 25;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1"].CellStyle.Font.Size = 14;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].Merge();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.Font.Bold = true;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1"].Text = dt.Rows[0]["LegalName"].ToString();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2"].RowHeight = 15;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2"].CellStyle.Font.Size = 10;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2" + ":" + GetColumnNameForXls(lastCol) + "2"].Merge();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

            var address = dt.Rows[0]["Address1"].ToString().Replace("\n", "");
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2"].Text = address;


            sheet.Range[GetColumnNameForXls(additionalColumn) + "3"].RowHeight = 15;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3"].CellStyle.Font.Size = 10;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].Merge();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.Font.Bold = true;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3"].Text = sheetHeaderText;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "4"].Text = sheetHeader;
            //if (isWithLogo)
            //{
            //    var image = ResourcesPathReader.GetLogoOrImagePath() + dt.Rows[0]["Image"];
            //    if (File.Exists(image))
            //        sheet.Pictures.AddPicture(1, lastCol, 3, lastCol + 1, image);
            //}
        }
        public void HeaderWithOutLogo(IWorksheet sheet, int lastCol, string sheetHeader, DataTable dt, bool isWithLogo)
        {
            Image companyLogo = null;
            string strPath = "";
            int additionalColumn = 1;
            //isWithLogo = false;
            if (isWithLogo)
            {
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dt.Rows[0]["CompanyId"].ToString() + ".jpg");  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                    additionalColumn = 3;
                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.3);
                            int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(3) + sheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception)
                {
                }
            }
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1"].RowHeight = 25;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1"].CellStyle.Font.Size = 14;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].Merge();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.Font.Bold = true;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "1"].Text = dt.Rows[0]["LegalName"].ToString();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2"].RowHeight = 15;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2"].CellStyle.Font.Size = 10;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2" + ":" + GetColumnNameForXls(lastCol) + "2"].Merge();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

            var address = dt.Rows[0]["Address1"].ToString().Replace("\n", "");
            sheet.Range[GetColumnNameForXls(additionalColumn) + "2"].Text = address;


            sheet.Range[GetColumnNameForXls(additionalColumn) + "3"].RowHeight = 15;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3"].CellStyle.Font.Size = 10;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].Merge();
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.Font.Bold = true;
            sheet.Range[GetColumnNameForXls(additionalColumn) + "3"].Text = sheetHeader;
            //if (isWithLogo)
            //{
            //    var image = ResourcesPathReader.GetLogoOrImagePath() + dt.Rows[0]["Image"];
            //    if (File.Exists(image))
            //        sheet.Pictures.AddPicture(1, lastCol, 3, lastCol + 1, image);
            //}
        }


        private void PlantHeader(IWorksheet sheet, int lastCol, string sheetHeader, DataTable dt, string plantName, bool isWithLogo, string date)
        {
            sheet.Range["A1"].RowHeight = 25;
            sheet.Range["A1"].CellStyle.Font.Size = 14;
            sheet.Range["A1" + ":" + GetColumnNameForXls(lastCol) + "1"].Merge();
            sheet.Range["A1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range["A1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range["A1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.Font.Bold = true;
            sheet.Range["A1"].Text = dt.Rows[0]["LegalName"].ToString();
            sheet.Range["A2"].RowHeight = 20;
            sheet.Range["A2"].CellStyle.Font.Size = 12;
            sheet.Range["A2" + ":" + GetColumnNameForXls(lastCol) + "2"].Merge();
            sheet.Range["A2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range["A2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range["A2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.Font.Bold = true;
            sheet.Range["A2"].Text = plantName;
            sheet.Range["A3"].RowHeight = 15;
            sheet.Range["A3"].CellStyle.Font.Size = 10;
            sheet.Range["A3" + ":" + GetColumnNameForXls(lastCol) + "3"].Merge();
            sheet.Range["A3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range["A3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range["A3"].Text = dt.Rows[0]["Address"].ToString();
            sheet.Range["A4"].RowHeight = 15;
            sheet.Range["A4"].CellStyle.Font.Size = 10;
            sheet.Range["A4" + ":" + GetColumnNameForXls(lastCol) + "4"].Merge();
            sheet.Range["A4" + ":" + GetColumnNameForXls(lastCol) + "4"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range["A4" + ":" + GetColumnNameForXls(lastCol) + "4"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range["A4" + ":" + GetColumnNameForXls(lastCol) + "4"].CellStyle.Font.Bold = true;
            sheet.Range["A4"].Text = sheetHeader;
            if (date != null)
            {
                sheet.Range["A5"].CellStyle.Font.Size = 10;
                sheet.Range["A5" + ":" + GetColumnNameForXls(lastCol) + "5"].Merge();
                sheet.Range["A5" + ":" + GetColumnNameForXls(lastCol) + "5"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range["A5" + ":" + GetColumnNameForXls(lastCol) + "5"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["A5" + ":" + GetColumnNameForXls(lastCol) + "5"].CellStyle.Font.Bold = false;
                sheet.Range["A5"].Text = date;
            }
            if (isWithLogo)
            {
                var image = ResourcesPathReader.GetLogoOrImagePath() + dt.Rows[0]["Image"];
                if (File.Exists(image))
                    sheet.Pictures.AddPicture(1, lastCol, 3, lastCol + 1, image);
            }
        }


        private void PlantHeader2(IWorksheet sheet, int lastCol, string sheetHeader, DataTable dt, string plantName, bool isWithLogo, string date)
        {
            sheet.Range["A1"].RowHeight = 25;
            sheet.Range["A1"].CellStyle.Font.Size = 14;
            sheet.Range["A1" + ":" + GetColumnNameForXls(lastCol) + "1"].Merge();
            sheet.Range["A1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range["A1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range["A1" + ":" + GetColumnNameForXls(lastCol) + "1"].CellStyle.Font.Bold = true;
            sheet.Range["A1"].Text = dt.Rows[0]["LegalName"].ToString();
            sheet.Range["A2"].RowHeight = 20;
            sheet.Range["A2"].CellStyle.Font.Size = 12;
            sheet.Range["A2" + ":" + GetColumnNameForXls(lastCol) + "2"].Merge();
            sheet.Range["A2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range["A2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range["A2" + ":" + GetColumnNameForXls(lastCol) + "2"].CellStyle.Font.Bold = true;
            sheet.Range["A2"].Text = plantName;
            sheet.Range["A3"].RowHeight = 15;
            sheet.Range["A3"].CellStyle.Font.Size = 8;
            sheet.Range["A3" + ":" + GetColumnNameForXls(lastCol) + "3"].Merge();
            sheet.Range["A3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range["A3" + ":" + GetColumnNameForXls(lastCol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range["A3"].Text = dt.Rows[0]["Address"].ToString();
            sheet.Range["A4"].RowHeight = 15;
            sheet.Range["A4"].CellStyle.Font.Size = 10;
            sheet.Range["A4" + ":" + GetColumnNameForXls(lastCol) + "4"].Merge();
            sheet.Range["A4" + ":" + GetColumnNameForXls(lastCol) + "4"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range["A4" + ":" + GetColumnNameForXls(lastCol) + "4"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range["A4" + ":" + GetColumnNameForXls(lastCol) + "4"].CellStyle.Font.Bold = true;
            sheet.Range["A4"].Text = sheetHeader;
            if (date != null)
            {
                sheet.Range["A5"].CellStyle.Font.Size = 10;
                sheet.Range["A5" + ":" + GetColumnNameForXls(lastCol) + "5"].Merge();
                sheet.Range["A5" + ":" + GetColumnNameForXls(lastCol) + "5"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range["A5" + ":" + GetColumnNameForXls(lastCol) + "5"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["A5" + ":" + GetColumnNameForXls(lastCol) + "5"].CellStyle.Font.Bold = false;
                sheet.Range["A5"].Text = date;
            }
            if (isWithLogo)
            {
                var image = ResourcesPathReader.GetLogoOrImagePath() + dt.Rows[0]["Image"];
                if (File.Exists(image))
                    sheet.Pictures.AddPicture(1, lastCol, 3, lastCol + 1, image);
            }
        }


        private void PlantHeaderNew(IWorksheet sheet, int Col, string sheetHeader, DataTable dt, string plantName, bool isWithLogo, string date)
        {
            int ROW = 1;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].RowHeight = 25;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.Font.Size = 14;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.Font.Bold = true;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].Text = dt.Rows[0]["LegalName"].ToString();
            sheet.Range[ROW, Col, ROW, Col + 6].Merge();
            ROW++;
            if (string.IsNullOrEmpty(plantName) == false)
            {
                sheet[clsStaticInfo.GetxlsCol(Col) + ROW].RowHeight = 20;
                sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.Font.Size = 12;
                sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.Font.Bold = true;
                sheet[clsStaticInfo.GetxlsCol(Col) + ROW].Text = plantName;
                sheet.Range[ROW, Col, ROW, Col + 6].Merge();
                ROW++;
            }
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].RowHeight = 15;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.Font.Size = 10;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].Text = dt.Rows[0]["Address"].ToString();
            sheet.Range[ROW, Col, ROW, Col + 6].Merge();

            ROW++;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].RowHeight = 15;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.Font.Size = 10;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].CellStyle.Font.Bold = true;
            sheet[clsStaticInfo.GetxlsCol(Col) + ROW].Text = sheetHeader;
            sheet.Range[ROW, Col, ROW, Col + 6].Merge();


            try
            {
                string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dt.Rows[0]["Id"].ToString() + ".jpg");  // IDCardEng.xlsx
                Image companyLogo = Image.FromFile(strPath);
                if (companyLogo != null)
                {
                    double totalWidth = 0;
                    for (int i = 1; i < Col; i++)
                    {
                        totalWidth += sheet.GetColumnWidth(i);
                    }

                    int totalWidthPixel = (int)(totalWidth * 7.5);
                    int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(3) + sheet.GetRowHeight(3)) * 1.50);

                    companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                    IPictureShape pic = null;

                    pic = sheet.Pictures.AddPicture(1, 1, companyLogo);


                }


            }
            catch (Exception ex)
            {


            }

        }

        public void SetMasterHeaderText(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt + " : ";
            sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
        }

        public void SetMiddleAlignmentText(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].CellStyle.Font.Bold = false;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
        }

        public void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }
        public void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt, int colWidth, int fontSize)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = colWidth;
            sheet.Range[row, col].CellStyle.Font.Size = fontSize;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }

        public void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt, int width)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }
        public void SetHeaderTextBL(ref IWorksheet sheet, int row, int col, string txt, int width)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.None);
        }
        public void SetHeaderTextBL(ref IWorksheet sheet, int row, int col, string txt, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.None);

        }


        public void SetHeaderTexte(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = 25;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }
        public void SetList(ref IWorksheet sheet, int row, int col, string[] List)
        {
            try
            {
                IDataValidation validation = sheet.Range[row, col].DataValidation;
                validation.ListOfValues = List;
                //validation.ListOfValues = new string[] { "ListItem1", "ListItem2", "ListItem3" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetList(DataSet ds, string ColumnName, out string[] list)
        {
            list = new string[ds.Tables[0].Rows.Count];
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    list[i] = ds.Tables[0].Rows[i][ColumnName].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetList(ref IWorksheet sheet, int frow, int lrow, int col, string[] List)
        {
            try
            {
                IDataValidation validation = sheet.Range[frow, col, lrow, col].DataValidation;
                validation.ListOfValues = List;
                //validation.ListOfValues = new string[] { "ListItem1", "ListItem2", "ListItem3" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetList(ref IWorksheet sheet, int frow, int lrow, int col, IWorksheet sheet_source, int Col_source, int lastRowOfSource)
        {
            try
            {
                IRange irCountry = sheet_source.Range[2, Col_source, lastRowOfSource + 2, Col_source];
                IDataValidation validationCountry = sheet.Range[frow + 1, col, lrow, col].DataValidation;
                validationCountry.DataRange = irCountry;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetList(ref IWorksheet sheet, int frow, int lrow, int col, DataSet ds, string ColumnName = "UserName")
        {
            try
            {
                string[] _list;
                GetList(ds, ColumnName, out _list);
                IDataValidation validation = sheet.Range[frow, col, lrow, col].DataValidation;
                //IRange ir = sheet.Range[frow, col, lrow, col];
                validation.ListOfValues = _list;
                //validation.DataRange = ir;
                //validation.ListOfValues = new string[] { "ListItem1", "ListItem2", "ListItem3" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetHeaderDate(ref IWorksheet sheet, int row, int col, string txt, ExcelKnownColors Fontcolor)
        {
            sheet.Range[row, col].DateTime = Convert.ToDateTime(txt);
            sheet.Range[row, col].NumberFormat = "dd-MMM-yyyy";
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].CellStyle.Font.Color = Fontcolor;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }
        public void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt, ExcelKnownColors Fontcolor)
        {
            sheet.Range[row, col].Text = txt;
            //sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].CellStyle.Font.Color = Fontcolor;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }
        public void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelKnownColors color)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].CellStyle.ColorIndex = color;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }

        public void SetSignatureText(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].CellStyle.Font.Bold = false;
            sheet.Range[row, col].CellStyle.Font.Size = 8;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        }

        public void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);

        }
        public void SetMasterHeaderTextForShiftReport(ref IWorksheet sheet, int row, int col, string txt, int fontSize)
        {
            sheet.Range[row, col].Text = txt + " : ";
            sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].CellStyle.Font.Size = fontSize;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
        }

        public void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt, ExcelHAlign al, bool HasBorder)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;
            if (HasBorder)
            {
                sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
            }
        }

        public void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }

        //public void SetHeaderRangeText(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        //{
        //    int endCol = col++;
        //    sheet.Range[row, col,row, endCol].Text = txt;
        //    sheet.Range[row, col, row, endCol].ColumnWidth = width;
        //    sheet.Range[row, col, row, endCol].CellStyle.Font.Bold = true;
        //    sheet.Range[row, col, row, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //    sheet.Range[row, col, row, endCol].HorizontalAlignment = al;
        //    sheet.Range[row, col, row, endCol].BorderAround(ExcelLineStyle.Thin);
        //}

        public void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = 10;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        //public void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        //{

        //    sheet.Range[xlsRow, xlsCol].Text = Text;
        //    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //    sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //    sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);

        //}

        public string GetFormulaGrandTotal(ArrayList al, int col)
        {
            string _formula = string.Empty;
            ReportUtility ru = new ReportUtility();
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    if (_formula.Length == 0)
                    {
                        _formula = "=" + ru.GetColumnNameForXls(col) + al[i];
                    }
                    else
                    {
                        _formula += "+" + ru.GetColumnNameForXls(col) + al[i];
                    }
                }
                return _formula;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetHeadText(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        }

        public void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        public string GetMonthName(string monthValue)
        {
            string _month = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(monthValue))
                {
                    throw new Exception("Value can not be blank...");
                }
                string val = GetNumData(monthValue);
                int _monthValue = Convert.ToInt16(val);

                if (_monthValue < 1)
                {
                    throw new Exception("Minimum value can be 1...");
                }
                if (_monthValue > 12)
                {
                    throw new Exception("Maximum value can be 12...");
                }

                switch (_monthValue)
                {
                    case 1:
                        _month = "JAN";
                        break;

                    case 2:
                        _month = "FEB";
                        break;

                    case 3:
                        _month = "MAR";
                        break;

                    case 4:
                        _month = "APR";
                        break;

                    case 5:
                        _month = "MAY";
                        break;

                    case 6:
                        _month = "JUN";
                        break;

                    case 7:
                        _month = "JUL";
                        break;

                    case 8:
                        _month = "AUG";
                        break;

                    case 9:
                        _month = "SEP";
                        break;

                    case 10:
                        _month = "OCT";
                        break;

                    case 11:
                        _month = "NOV";
                        break;

                    default:
                        _month = "DEC";
                        break;
                }
                return _month;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //
            }
        }// end function
        public string GetNumData(string strNumber)
        {
            double d;
            strNumber = strNumber.Replace(",", "");
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0"; }
            else if (Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return strNumber;
            }
            else
            {
                return "0";
            }
        }// end function


        public string GetAttSum()
        {
            try
            {
                return @" TotalPresent = CASE WHEN DayStatus = 'P' and LTSystemID is null THEN 1 
                                                       WHEN DayStatus = 'WP' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'HP' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'WHP' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'HWP' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'P' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'P' and LTSystemID is not null and IsHalfDayLeave = 0 THEN 1
                                                       WHEN DayStatus = 'LVP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'LVP' and LTSystemID is not null and IsHalfDayLeave = 0   THEN 1
                                                        -- lwp n normal in both case it will b 0.5
                                                       WHEN DayStatus = 'WP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'HP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'WHP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5
                                                       WHEN DayStatus = 'HWP' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5


                                                      WHEN DayStatus = 'WP' and LTSystemID is not null and IsHalfDayLeave = 0   THEN 1
                                                       WHEN DayStatus = 'HP' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       WHEN DayStatus = 'WHP' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1
                                                       WHEN DayStatus = 'HWP' and LTSystemID is not null and IsHalfDayLeave = 0  THEN 1

                                                        --if late and half leave, rest of the day will b Present not late. not considereing 1st or 2nd half
                                                       WHEN DayStatus = 'L' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                       WHEN DayStatus = 'WL' and LTSystemID is not null and IsHalfDayLeave = 1   THEN 0.5
                                                       WHEN DayStatus = 'HL' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                       WHEN DayStatus = 'WHL' and LTSystemID is not null and IsHalfDayLeave = 1   THEN 0.5
                                                       WHEN DayStatus = 'HWL' and LTSystemID is not null and IsHalfDayLeave = 1    THEN 0.5
                                                       WHEN DayStatus = 'LVL' and LTSystemID is not null and IsHalfDayLeave = 1    THEN 0.5

                                                       WHEN DayStatus = 'RST' THEN 1

                                                       WHEN DayStatus = 'OD' THEN 1
                                                       WHEN DayStatus = 'HDP'  THEN 0.5
                                                       WHEN DayStatus = 'HDA' and LTSystemID is null THEN 0.5

                                                       ELSE 0 END,
			                            TotalLate = CASE WHEN DayStatus = 'L' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'WL' and LTSystemID is null THEN 1

                                                       WHEN DayStatus = 'HL' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'WHL' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'HWL' and LTSystemID is null THEN 1
                                                       WHEN DayStatus = 'LVL' and LTSystemID is not null and IsHalfDayLeave = 0    THEN 1                                                      

                                                       ELSE 0 END,
			                            TotalAbsent = CASE WHEN DayStatus = 'A' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'WA' and LTSystemID is null THEN 1
                                                        WHEN DayStatus = 'HA' and LTSystemID is null THEN 1
                                                       
                                                        WHEN DayStatus = 'LV'and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5

                                                        WHEN DayStatus = 'WA' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                        WHEN DayStatus = 'A' and LTSystemID is not null and IsHalfDayLeave = 1  THEN 0.5
                                                       
                                                        WHEN DayStatus = 'LVA' and LTSystemID is not null and IsHalfDayLeave = 0 THEN 1
                                                        WHEN DayStatus = 'LVA' and LTSystemID is not null and IsHalfDayLeave = 1 THEN 0.5

                                                        WHEN DayStatus = 'HDP' and LTSystemID is null THEN 0.5
                                                        WHEN DayStatus = 'HDA' THEN 0.5

                                                        ELSE 0 END,
			                            TotalLv = CASE WHEN LTSystemID is not null  and IsHalfDayLeave = 1 and IsLWP=0 THEN 0.5

                                                          WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus='LV' and IsLWP=0 THEN 1
                                                          WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus='A' and IsLWP=0  THEN 1

                                                           ELSE 0 END,

                                        TotalLWP = CASE WHEN LTSystemID is not null  and IsHalfDayLeave = 1 and DayStatus<>'LV' and IsLWP=1 THEN 0.5
                                                        WHEN LTSystemID is not null  and IsHalfDayLeave = 1 and DayStatus='LV' and IsLWP=1 THEN 0.5
                                                        WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and DayStatus<>'LV' and IsLWP=1 THEN 0
                                                        WHEN LTSystemID is not null  and IsHalfDayLeave = 0 and IsLWP=1 THEN 1
                                                        ELSE 0 END,

			                            TotalMLv = CASE WHEN DayStatus = 'MLV' THEN 1

                                                        WHEN DayStatus = 'MLVP' THEN 1

                                                        WHEN DayStatus = 'MLVL' THEN 1

                                                        WHEN DayStatus = 'WMLV' THEN 1

                                                        WHEN DayStatus = 'HMLV' THEN 1

                                                        WHEN DayStatus = 'WMLVP' THEN 1

                                                        WHEN DayStatus = 'HMLVP' THEN 1

                                                        WHEN DayStatus = 'WMLVL' THEN 1

                                                        WHEN DayStatus = 'HMLVL' THEN 1
                                                        WHEN DayStatus = 'WHMLV' THEN 1
                                                        WHEN DayStatus = 'WHMLVP' THEN 1
                                                        WHEN DayStatus = 'WHMLVL' THEN 1

                                                        WHEN DayStatus = 'HWMLV' THEN 1

                                                        WHEN DayStatus = 'HWMLVP' THEN 1

                                                        WHEN DayStatus = 'HWMLVL' THEN 1

                                                        ELSE 0 END,
                                        TotalCompAssignLv = CASE WHEN DayStatus = 'CAL' THEN 1
                                                        WHEN DayStatus = 'CALP' THEN 1

                                                        WHEN DayStatus = 'CALL' THEN 1

                                                        WHEN DayStatus = 'WCAL' THEN 1

                                                        WHEN DayStatus = 'HCAL' THEN 1

                                                        WHEN DayStatus = 'WCALP' THEN 1

                                                        WHEN DayStatus = 'HCALP' THEN 1

                                                        WHEN DayStatus = 'WCALL' THEN 1

                                                        WHEN DayStatus = 'HCALL' THEN 1
                                                        WHEN DayStatus = 'WHCAL' THEN 1
                                                        WHEN DayStatus = 'WHCALP' THEN 1
                                                        WHEN DayStatus = 'WHCALL' THEN 1

                                                        WHEN DayStatus = 'HWCAL' THEN 1

                                                        WHEN DayStatus = 'HWCALP' THEN 1

                                                        WHEN DayStatus = 'HWCALL' THEN 1

                                                        ELSE 0 END,
			                            TotalWeekOff = CASE WHEN DayStatus = 'W' THEN 1

                                                        ELSE 0 END,
			                            TotalHoliDay = CASE WHEN DayStatus = 'H' THEN 1

                                                       ELSE 0 END,
                                        TotalWeekOffHoliDay = CASE WHEN DayStatus = 'WH' THEN 1

                                                        WHEN DayStatus = 'HW' THEN 1

                                                       ELSE 0 END,";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Report Header

        public class SalaryHeadSequence
        {
            public string SalaryHeadId { get; set; }
            public string SalaryHead { get; set; }
            public string HeadType { get; set; }
            public int Sequence { get; set; }
            public int XLColIndex { get; set; }
            public string Earning { get; set; }
            public string Deduction { get; set; }
            public string HeadCategory { get; set; }
            public bool IsInt { get; set; }
            public int DecimalNo { get; set; }
            public bool IsGrossComponent { get; set; }
            public bool IsCTCComponent { get; set; }
            public bool IsNetPayEffect { get; set; }
        }
        /// <summary>
        /// For PayRegister
        /// </summary>
        public class SalaryHeadSequenceStructure
        {
            public string SalaryHeadId { get; set; }
            public string SalaryHead { get; set; }
            public string HeadType { get; set; }
            public int Sequence { get; set; }
            public int XLColIndex { get; set; }
            public string Earning { get; set; }
            public string Deduction { get; set; }
            public string HeadCategory { get; set; }
            public bool IsInt { get; set; }
            public int DecimalNo { get; set; }
            public bool IsCTCComponent { get; set; }
            public bool IsNetPayEffect { get; set; }
        }

        public string SetFormula(string f, int r)
        {
            string result = "";
            try
            {
                var x = f.Split(',');
                foreach (var item in x)
                {
                    var col = GetColumnNameForXls(Convert.ToInt32(item));
                    if (result.Length == 0)
                    {
                        result = col + r;
                    }
                    else
                    {
                        result += "+" + col + r;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string NumberFormatDecimalZero()
        {
            return "#,##0;(#,##0)";
        }

        public string NumberFormatIntWithComma()
        {
            return "#,#,#0;";
        }
        public class Param
        {
            public string CompanyGroupId { get; set; }
            public string CompanyId { get; set; }
            public string PlantId { get; set; }
            public string SheetHeader { get; set; }
            public string SheetName { get; set; }
            public string UserName { get; set; }
            //public string CompanyInfo { get; set; }
        }
        public string GetDynamicDecimalPlace(int DecimalPlace)
        {
            string v = "#,##0;(#,##0)";
            //string v= "#,##0.00;(#,##0.00)";
            try
            {
                string _zero_acum = "";
                for (int i = 0; i < DecimalPlace; i++)
                {
                    if (_zero_acum.Length == 0)
                    {
                        _zero_acum = ".0";
                    }
                    else
                    {
                        _zero_acum += "0";
                    }
                }
                v = "#,##0" + _zero_acum + ";(#,##0" + _zero_acum + ")";
                return v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetDynamicDecimalPlaceLocal(int DecimalPlace, string language)
        {
            string v = "#,##0.00;(#,##0.00)";
            try
            {
                if (language == "Bengali")
                {
                    v = "[$-5000445]#,##0;(#,##0)";
                    string _zero_acum = "";
                    for (int i = 0; i < DecimalPlace; i++)
                    {
                        if (_zero_acum.Length == 0)
                        {
                            _zero_acum = ".0";
                        }
                        else
                        {
                            _zero_acum += "0";
                        }
                    }
                    v = "[$-5000445]#,##0" + _zero_acum + ";(#,##0" + _zero_acum + ")";
                }
                else if (language == "Hindi")
                {
                    v = "[$-4000400]#,##0;(#,##0)";
                    string _zero_acum = "";
                    for (int i = 0; i < DecimalPlace; i++)
                    {
                        if (_zero_acum.Length == 0)
                        {
                            _zero_acum = ".0";
                        }
                        else
                        {
                            _zero_acum += "0";
                        }
                    }
                    v = "[$-4000400]#,##0" + _zero_acum + ";(#,##0" + _zero_acum + ")";
                }
                else
                {
                    v = "#,##0;(#,##0)";
                    string _zero_acum = "";
                    for (int i = 0; i < DecimalPlace; i++)
                    {
                        if (_zero_acum.Length == 0)
                        {
                            _zero_acum = ".0";
                        }
                        else
                        {
                            _zero_acum += "0";
                        }
                    }
                    v = "#,##0" + _zero_acum + ";(#,##0" + _zero_acum + ")";
                }
                return v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetDecimalFormatlocal(SalaryHeadSequence shs, string language)
        {
            try
            {
                var ob = new ReportUtility();
                if (shs.IsInt)
                {
                    return ob.NumberFormatIntLocal(language);
                }
                else
                {
                    return ob.GetDynamicDecimalPlaceLocal(shs.DecimalNo, language);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetDecimalFormatlocalNetPay(bool IsInt, int decimalNo, string language)
        {
            try
            {
                var ob = new ReportUtility();
                if (IsInt)
                {
                    return ob.NumberFormatIntLocal(language);
                }
                else
                {
                    return ob.GetDynamicDecimalPlaceLocal(decimalNo, language);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetDecimalFormatlocal(int DecimalNo, string language)
        {
            try
            {
                var ob = new ReportUtility();

                return ob.GetDynamicDecimalPlaceLocal(DecimalNo, language);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// for pay rgister
        /// </summary>
        /// <param name="shs"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public string GetDecimalFormatlocalStructure(SalaryHeadSequenceStructure shs, string language)
        {
            try
            {
                var ob = new ReportUtility();
                if (shs.IsInt)
                {
                    return ob.NumberFormatIntLocal(language);
                }
                else
                {
                    return ob.GetDynamicDecimalPlaceLocal(shs.DecimalNo, language);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        public void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        public void SetHeaderTextRotate(ref IWorksheet sheet, int row, int col, string txt)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = 7;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].CellStyle.Font.Size = 7;
            sheet.Range[row, col].CellStyle.ShrinkToFit = true;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[row, col].CellStyle.Rotation = 90;
        }
        public void Header(ref IWorksheet sheet, Param param, int lastcol, string SheetHeader, bool IsForCompanyGroup)
        {
            DataSet dsLocal = null;
            try
            {
                if (IsForCompanyGroup)
                {
                    GetCompanyGroup(param, out dsLocal);
                    if (dsLocal.Tables[0].Rows.Count == 0)
                    {
                        throw (new Exception("Company Group Info not found !!!"));
                    }
                }
                else
                {
                    GetCompany(param, out dsLocal);
                    if (dsLocal.Tables[0].Rows.Count == 0)
                    {
                        throw (new Exception("Company Info not found !!!"));
                    }
                }

                sheet.Range["A1"].RowHeight = 25;
                sheet.Range["A1"].CellStyle.Font.Size = 14;
                sheet.Range["A1" + ":" + GetColumnNameForXls(lastcol) + "1"].Merge();
                sheet.Range["A1" + ":" + GetColumnNameForXls(lastcol) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range["A1" + ":" + GetColumnNameForXls(lastcol) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["A1" + ":" + GetColumnNameForXls(lastcol) + "1"].CellStyle.Font.Bold = true;
                sheet.Range["A1"].Text = dsLocal.Tables[0].Rows[0]["LegalName"].ToString();
                sheet.Range["A2"].RowHeight = 15;
                sheet.Range["A2"].CellStyle.Font.Size = 10;
                sheet.Range["A2" + ":" + GetColumnNameForXls(lastcol) + "2"].Merge();
                sheet.Range["A2" + ":" + GetColumnNameForXls(lastcol) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range["A2" + ":" + GetColumnNameForXls(lastcol) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["A2"].Text = dsLocal.Tables[0].Rows[0]["Address"].ToString();
                //sheet.Range["A3"].RowHeight = 15;
                //sheet.Range["A3"].CellStyle.Font.Size = 10;
                //sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].Merge();
                //sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.Font.Bold = true;
                //sheet.Range["A3"].Text = dsLocal.Tables[0].Rows[0]["Contact"].ToString();

                sheet.Range["A1:" + GetColumnNameForXls(lastcol) + "2"].BorderAround(ExcelLineStyle.Thin);

                sheet.Range["A3"].RowHeight = 15;
                sheet.Range["A3"].CellStyle.Font.Size = 10;
                sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].Merge();
                sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.Font.Bold = true;
                sheet.Range["A3"].Text = SheetHeader;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetCompanyGroup(Param param, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"
                                    SELECT com.Id
	                                    ,com.username
	                                    ,com.LegalName
	                                    ,am.Address1
	                                    ,am.Address2
	                                    ,co.UserName AS Country
	                                    ,ct.UserName AS City
	                                    ,cm.Phone1 AS Phone
	                                    ,cm.Email1 AS Email
	                                    ,cm.Website AS Website
	                                    ,ar.UserName AS Area
                                        ,am.Address1+', '+ar.UserName+', '+ct.UserName Address
                                        ,cm.Phone1+', '+cm.Email1+', '+cm.Website Contact
                                    FROM ORG.CompanyGroup AS com
                                    LEFT OUTER JOIN MST.AddressMaster AS am ON am.Id = com.AddressMasterId
                                    LEFT OUTER JOIN MST.ContactMaster AS cm ON cm.Id = com.ContactMasterId
                                    LEFT OUTER JOIN SCS.Country AS co ON co.Id = am.CountryId
                                    LEFT OUTER JOIN SCS.City AS ct ON ct.Id = am.CityId
                                    LEFT OUTER JOIN SCS.Area AS ar ON ar.Id = am.AreaId
                                    WHERE com.Id = '" + param.CompanyGroupId + @"'
                                    ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void GetCompany(Param param, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT com.Id
	                                    ,com.username
	                                    ,com.LegalName
	                                    ,com.WebDomain
	                                    ,am.Address1
	                                    ,am.Address2
	                                    ,co.UserName AS Country
	                                    ,ct.UserName AS City
	                                    ,cm.Phone1 AS Phone
	                                    ,cm.Email1 AS Email
	                                    ,cm.Website AS Website
	                                    ,ar.UserName AS Area
                                        ,com.Image
                                        ,am.Address1+', '+ar.UserName+', '+ct.UserName Address
                                        ,cm.Phone1+', '+cm.Email1+', '+cm.Website Contact
                                    FROM ORG.Company AS com
                                    LEFT OUTER JOIN MST.AddressMaster AS am ON am.Id = com.AddressMasterId
                                    LEFT OUTER JOIN MST.ContactMaster AS cm ON cm.Id = com.ContactMasterId
                                    LEFT OUTER JOIN SCS.Country AS co ON co.Id = am.CountryId
                                    LEFT OUTER JOIN SCS.City AS ct ON ct.Id = am.CityId
                                    LEFT OUTER JOIN SCS.Area AS ar ON ar.Id = am.AreaId
                                    WHERE com.Id = '" + param.CompanyId + @"'
                                    ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void PageSetup(ref IWorksheet sheet, string UserName, int xlsColumnHeader, ExcelPageOrientation po)
        {
            try
            {
                //Setting Page Setup
                sheet.PageSetup.TopMargin = 0.5;
                sheet.PageSetup.BottomMargin = 1;
                //sheet.PageSetup.PrintTitleRows = "$1:$" + xlsColumnHeader + "";
                //sheet.PageSetup.PrintTitleRows = "$" + xlsColumnHeader + ":$" + xlsColumnHeader + "";
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.RightFooter = "&p";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + UserName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet.PageSetup.LeftMargin = 0.5;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = po;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.PrintGridlines = false;
                sheet.PageSetup.CenterVertically = false;
                sheet.IsDisplayZeros = false;
                sheet.PageSetup.Zoom = 100;
                sheet.PageSetup.PrintQuality = 600;
                sheet.PageSetup.FirstPageNumber = 1;

                //sheet.PageSetup.PrintTitleRows = "$1:$2";
                //sheet2.PageSetup.CenterHorizontally = true;
                //sheet.PageSetup.
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Header(ref IWorksheet sheet, Param param, int lastcol, string SheetHeader)
        {
            DataSet dsLocal = null;
            try
            {
                GetCompany(param, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count == 0)
                {
                    throw (new Exception("Company Info not found !!!"));
                }

                string strPath = "";
                Image companyLogo = null;
                string companyLogoName = "";
                companyLogoName = dsLocal.Tables[0].Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                try
                {

                    if (companyLogo != null)
                    {
                        double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(3) + sheet.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }

                sheet.Range["D1"].RowHeight = 25;
                sheet.Range["D1"].CellStyle.Font.Size = 14;
                sheet.Range["D1" + ":" + GetColumnNameForXls(lastcol) + "1"].Merge();
                sheet.Range["D1" + ":" + GetColumnNameForXls(lastcol) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range["D1" + ":" + GetColumnNameForXls(lastcol) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["D1" + ":" + GetColumnNameForXls(lastcol) + "1"].CellStyle.Font.Bold = true;
                sheet.Range["D1"].Text = dsLocal.Tables[0].Rows[0]["LegalName"].ToString();
                sheet.Range["D2"].RowHeight = 15;
                sheet.Range["D2"].CellStyle.Font.Size = 10;
                sheet.Range["D2" + ":" + GetColumnNameForXls(lastcol) + "2"].Merge();
                sheet.Range["D2" + ":" + GetColumnNameForXls(lastcol) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range["D2" + ":" + GetColumnNameForXls(lastcol) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["D2"].Text = dsLocal.Tables[0].Rows[0]["Address"].ToString();
                //sheet.Range["A3"].RowHeight = 15;
                //sheet.Range["A3"].CellStyle.Font.Size = 10;
                //sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].Merge();
                //sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range["A3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.Font.Bold = true;
                //sheet.Range["A3"].Text = dsLocal.Tables[0].Rows[0]["Contact"].ToString();

                //sheet.Range["D2:" + GetColumnNameForXls(lastcol) + "2"].BorderAround(ExcelLineStyle.Thin);

                sheet.Range["D3"].RowHeight = 15;
                sheet.Range["D3"].CellStyle.Font.Size = 10;
                sheet.Range["D3" + ":" + GetColumnNameForXls(lastcol) + "3"].Merge();
                sheet.Range["D3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range["D3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range["D3" + ":" + GetColumnNameForXls(lastcol) + "3"].CellStyle.Font.Bold = true;
                sheet.Range["D3"].Text = SheetHeader;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string cnDgt(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input.Replace('0', '০')
                    .Replace('1', '১')
                    .Replace('2', '২')
                    .Replace('3', '৩')
                    .Replace('4', '৪')
                    .Replace('5', '৫')
                    .Replace('6', '৬')
                    .Replace('7', '৭')
                    .Replace('8', '৮')
                    .Replace('9', '৯')
                    .Replace('.', '.');

            }
            else if (lng == "Hindi")
            {
                return input.Replace('0', '०')
                    .Replace('1', '१')
                    .Replace('2', '२')
                    .Replace('3', '३')
                    .Replace('4', '४')
                    .Replace('5', '५')
                    .Replace('6', '६')
                    .Replace('7', '७')
                    .Replace('8', '८')
                    .Replace('9', '९')
                    .Replace('.', '.');
            }
            return input;
        }
        public string ChangeMonthA(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input

                     .Replace("Jan", "জানু")
                    .Replace("Feb", "ফেব্রু")
                    .Replace("Mar", "মার্চ")
                    .Replace("Apr", "এপ্রিল")
                    .Replace("May", "মে")
                    .Replace("Jun", "জুন")
                    .Replace("Jul", "জুলাই")
                    .Replace("Aug", "আগস্ট")
                    .Replace("Sep", "সেপ্টে")
                    .Replace("Oct", "অক্টো")
                    .Replace("Nov", "নভে")
                    .Replace("Dec", "ডিসে");
            }
            else if (lng == "Hindi")
            {
                return input
                    .Replace("Jan", "जनवरी")
                    .Replace("Feb", "फरवरी")
                    .Replace("Mar", "मार्च")
                    .Replace("Apr", "अप्रैल")
                    .Replace("May", "मई")
                    .Replace("Jun", "जून")
                    .Replace("Jul", "जुलाई")
                    .Replace("Aug", "अगस्त")
                    .Replace("Sep", "सितम्बर")
                    .Replace("Oct", "अक्तूबर")
                    .Replace("Nov", "नवम्बर")
                    .Replace("Dec", "दिसम्बर");
            }
            return input;
        }
        public string GetFormatedDateA(string date, string lng)
        {
            var formateDate = string.Empty;
            var day = cnDgt(date.Substring(0, 2), lng);
            var mon = ChangeMonthA(date.Substring(3, 3), lng);
            var year = cnDgt(date.Substring(7, 4), lng);
            return formateDate = day + "-" + mon + "-" + year;
        }
        public string ChangeMonth(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input
                    .Replace("Jan", "০১")
                    .Replace("Feb", "০২")
                    .Replace("Mar", "০৩")
                    .Replace("Apr", "০৪")
                    .Replace("May", "০৫")
                    .Replace("Jun", "০৬")
                    .Replace("Jul", "০৭")
                    .Replace("Aug", "০৮")
                    .Replace("Sep", "০৯")
                    .Replace("Oct", "১০")
                    .Replace("Nov", "১১")
                    .Replace("Dec", "১২");
            }
            else if (lng == "Hindi")
            {
                return input
                    .Replace("Jan", "जनवरी")
                    .Replace("Feb", "फरवरी")
                    .Replace("Mar", "मार्च")
                    .Replace("Apr", "अप्रैल")
                    .Replace("May", "मई")
                    .Replace("Jun", "जून")
                    .Replace("Jul", "जुलाई")
                    .Replace("Aug", "अगस्त")
                    .Replace("Sep", "सितम्बर")
                    .Replace("Oct", "अक्तूबर")
                    .Replace("Nov", "नवम्बर")
                    .Replace("Dec", "दिसम्बर");
            }
            return input;
        }

        public string GetFormatedDate(string date, string lng)
        {
            var formateDate = string.Empty;
            var day = cnDgt(date.Substring(0, 2), lng);
            var mon = ChangeMonth(date.Substring(3, 3), lng);
            var year = cnDgt(date.Substring(7, 4), lng);
            return formateDate = day + "-" + mon + "-" + year;
        }




        #region RDLC Utilities
        public DataTable CompanyHeader(string companyId)
        {
            var sql = @"SELECT COM.Id, COM.UserName, COM.LegalName, COM.WebDomain, AM.Address1, AM.Address2, CO.UserName AS Country, CT.UserName AS City, CM.Phone1 AS Phone, CM.Email1 AS Email
                        , CM.Website AS Website, AR.UserName AS Area
                        , [Address]=CASE ISNULL(AM.Address1,'') WHEN '' THEN '' ELSE AM.Address1 +', ' END+
			                        CASE ISNULL(AR.UserName,'') WHEN '' THEN '' ELSE AR.UserName +', ' END+
			                        CASE ISNULL(CT.UserName,'') WHEN '' THEN '' ELSE ct.UserName END
                        , Contact=CASE ISNULL(CM.Phone1,'') WHEN '' THEN '' ELSE CM.Phone1 +', ' END+
		                        CASE ISNULL(CM.Email1,'') WHEN '' THEN '' ELSE CM.Email1 +', ' END+
		                        CASE ISNULL(CM.Website ,'') WHEN '' THEN '' ELSE CM.Website  END
                        FROM [ORG].[Company] AS COM
                        LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=COM.AddressMasterId
                        LEFT JOIN [MST].[ContactMaster] AS CM ON CM.Id=COM.ContactMasterId
                        LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
                        LEFT JOIN [SCS].[City] AS CT ON CT.Id=AM.CityId
                        LEFT JOIN [SCS].[Area] AS AR ON AR.Id=AM.AreaId
                        WHERE COM.Id='" + companyId + "'";
            return _sqlRepository.GetDataTable(sql);
        }
        #endregion
        #region Local language

        public class LabelList : BaseModel
        {
            public string LocalLabel { get; set; }
            public string DefaultLabel { get; set; }

        }

        public Dictionary<string, string> LocalLanguageLabelList(string plantId, string languageId)
        {
            try
            {
                Dictionary<string, string> dicLabel = new Dictionary<string, string>();
                var strSQL = @"select Distinct LabelName DefaultLabel,Name LocalLabel from HKP.LocalLanguage where LabelName is not null 

                and LanguageId = '" + languageId + @"'
                    
                union

                    SELECT lt.Code, LL.Name FROM HKP.LocalLanguage LL
                    JOIN LeaveType LT ON LT.Id=LL.LeaveTypeId
                    WHERE LeaveTypeId IS NOT NULL AND LanguageId = '" + languageId + @"'
                    ";

                DataTable dtLabel = _sqlRepository.GetDataTable(strSQL);

                for (int i = 0; i < dtLabel.Rows.Count; i++)
                {
                    if (dicLabel.ContainsKey(dtLabel.Rows[i]["DefaultLabel"].ToString().Trim()) == false)
                        dicLabel.Add(dtLabel.Rows[i]["DefaultLabel"].ToString().Trim(), dtLabel.Rows[i]["LocalLabel"].ToString().Trim());
                }
                return dicLabel;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public string GetLabelname(Dictionary<string, string> labelList, string Givenenum, string defaultValue)
        {
            //var labelLocal = labelList.Where(r => r.DefaultLabel.Trim().ToUpper() == Givenenum.Trim().ToUpper()).FirstOrDefault();
            //var _labelLocalLocal = labelLocal == null ? defaultValue : labelLocal.LocalLabel == null || labelLocal.LocalLabel == "" ? labelLocal.DefaultLabel : labelLocal.LocalLabel;

            string labelLocal = "";
            if (labelList.ContainsKey(Givenenum.Trim()))
            {
                labelLocal = labelList[Givenenum];
            }
            var _labelLocalLocal = string.IsNullOrEmpty(labelLocal) ? defaultValue : labelLocal;//.LocalLabel == null || labelLocal.LocalLabel == "" ? labelLocal.DefaultLabel : labelLocal.LocalLabel;


            return _labelLocalLocal;
        }


        public string LocalLanguageListSql(string plantId, string languageId, out bool isLocalLanguage)
        {
            try
            {
                DataTable DTplantWiseLocalLanguageList = null;
                var strSQL = @"SELECT ISNULL(LANG.StandardName,'English') localLanguage FROM SCS.[Language] LANG
                                      INNER join ORG.Plant Plant on plant.LanguageId = LANG.Id 
                                      WHERE LanguageId  = '" + languageId + "'";
                DTplantWiseLocalLanguageList = _sqlRepository.GetDataTable(strSQL);
                var localLanguage = "English";
                isLocalLanguage = false;

                if (DTplantWiseLocalLanguageList.Rows.Count > 0)
                {
                    isLocalLanguage = true;
                    localLanguage = DTplantWiseLocalLanguageList.Rows[0]["localLanguage"].ToString();
                }
                return localLanguage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string PlantWiseLocalLanguageListSql(string plantId, string languageId)
        {
            try
            {
                DataTable DTplantWiseLocalLanguageList = null;
                var strSQL = @"SELECT ISNULL(LANG.StandardName,'English') localLanguage FROM SCS.[Language] LANG
                                    INNER join ORG.Plant Plant on plant.LanguageId = LANG.Id 
                                    WHERE LanguageId = '" + languageId + "'";
                DTplantWiseLocalLanguageList = _sqlRepository.GetDataTable(strSQL);
                var localLanguage = "English";

                if (DTplantWiseLocalLanguageList.Rows.Count > 0)
                {
                    localLanguage = DTplantWiseLocalLanguageList.Rows[0]["localLanguage"].ToString();
                }
                return localLanguage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable payRollGroup(string payRollGroupId)
        {
            try
            {
                var strSQL = @"select * from HKP.PayrollGroup where Id = '" + payRollGroupId + @"'";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        private string Concat(string addresspart, string fullAddress)
        {
            if (addresspart.Trim() == "")
                return fullAddress;
            if (fullAddress == "")
                fullAddress = addresspart;
            else
                fullAddress += ", " + addresspart;

            return fullAddress + ".";
        }

        public string GetAddress(string addressId, string additionalAddress)
        {
            string sqlText = @"SELECT   ISNULL(AM.Address1,'') Address1
												,ISNULL(AM.Address2,'') Address2,ISNULL(AM.Address3,'') Address3
												,ISNULL(AM.Ward,'') Ward,ISNULL(AM.Village,'') Village,ISNULL(AM.Circle,'') Circle,ISNULL(AM.Thana,'') Thana, ISNULL(Area.UserName,'') Area
												,ISNULL(District.UserName,'') District
												,ISNULL(City.UserName,'') City,ISNULL(stt.UserName,'') stt,ISNULL(Country.UserName,'') Country,ISNULL(Continent.UserName,'') Continent
		                                        from MST.AddressMaster AM 
										 LEFT JOIN SCS.[State] stt  ON stt.Id = AM.StateId
                                         LEFT JOIN SCS.Country Country  ON Country.Id = AM.CountryId
                                         LEFT JOIN SCS.Continent Continent  ON Continent.Id = AM.CountryId
                                         LEFT JOIN SCS.Area Area  ON Area.Id = AM.AreaId
                                         LEFT JOIN SCS.District District  ON District.Id = AM.DistrictId
                                         LEFT JOIN SCS.City City  ON City.Id = AM.CityId
										 WHERE AM.Id = '" + addressId + @"'";

            var dtAddressMaster = _sqlRepository.GetDataTable(sqlText);
            string address = "";
            if (dtAddressMaster.Rows.Count > 0)
            {
                if (additionalAddress == "")
                {
                    address = Concat(dtAddressMaster.Rows[0]["Address1"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Address2"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Address3"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Ward"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Village"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Circle"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Thana"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Area"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["District"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["City"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Country"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Continent"].ToString(), address);
                }
                else
                {
                    address = Concat(dtAddressMaster.Rows[0]["Ward"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Village"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Circle"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Thana"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Area"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["District"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["City"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Country"].ToString(), address);
                    address = Concat(dtAddressMaster.Rows[0]["Continent"].ToString(), address);
                }
            }
            return address;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="OTConsiderOn"></param>
        /// <param name="OTHr">in min</param>
        /// <param name="OT_output"></param>
        public void GetOT(string OTConsiderOn, string OTHr, out string OT_output)
        {
            OT_output = string.Empty;
            try
            {
                string yot = string.Empty;
                if (string.IsNullOrEmpty(OTHr))
                {
                    yot = "0";
                }
                else
                {
                    yot = OTHr;
                }
                if (OTConsiderOn.ToUpper() == "HOUR MINUTE VALUE")//
                {
                    int hh = Convert.ToInt32(Math.Floor(Convert.ToDouble(bplib.clsWebLib.GetNumData(yot)))) / 60;
                    decimal mm = Convert.ToDecimal(bplib.clsWebLib.GetNumData(yot)) % 60;
                    //int mm = Convert.ToInt32(Math.Floor(Convert.ToDouble(bplib.clsWebLib.GetNumData(yot)))) % 60;
                    if (mm == 0)
                    {
                        string minute = mm.ToString("F").TrimStart();
                        minute = minute.Substring(2, minute.Length - 2);
                        OT_output = hh + ":" + minute;
                    }
                    else
                    {
                        String minute = mm.ToString();
                        if (minute.Contains("."))
                        {
                            minute = minute.Substring(0, minute.Length - 3);

                        }
                        //else
                        //{
                        OT_output = hh + ":" + minute;
                        //}
                    }


                }
                else
                {
                    double hh = Convert.ToDouble(bplib.clsWebLib.GetNumData(yot)) / 60;
                    //OT_output = hh.ToString();
                    OT_output = hh.ToString("0.##");//Muntasir


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetColFormula(ref IWorksheet sheet, int row, int col, string txt, bool IsDecimal)
        {
            sheet.Range[row, col].Formula = txt;
            if (IsDecimal)
            {
                sheet.Range[row, col].NumberFormat = NumberFormatDecimalTwo();
            }
            else
            {
                sheet.Range[row, col].NumberFormat = NumberFormatInt();
            }
            //sheet.Range[row, col].CellStyle.IsFirstSymbolApostrophe = false;
            //sheet.Range[row, col].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
            //sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, col].CellStyle.Font.Bold = false;
        }



        public static Image FixedSize(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap bmPhoto = new Bitmap(Width, Height, PixelFormat.Format32bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);
            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;
            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);
            grPhoto.Dispose();
            return bmPhoto;
        }
    }
    public class FiscalYearMonthSequence
    {
        public string MonthName { get; set; }
        public string MonthNo { get; set; }
        public string LastDayOfMonth { get; set; }
        public string MonthYear { get; set; }
        public int XLColIndex { get; set; }

    }

    public class OperationSequence
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int XLColIndex { get; set; }
    }

}