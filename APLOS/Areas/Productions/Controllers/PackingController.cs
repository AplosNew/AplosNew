#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using Library.OrderManagement.Production;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.Data;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Aplos.Areas.Commercial.Controllers;
using System.Drawing;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class PackingController : BaseController
    {
        PackingData det = new PackingData();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public PackingController(ISqlRepository R)
        {
            _sqlRepository = R;
            det = new PackingData();
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string ToDate, string FromDate, string type, string group, string column, string value)
        {
            try
            {
                var jj = det.GetData(ToDate, FromDate, type, group, column, value);
                var jsondata = Json(new { Error = false, DATA = jj }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult getClickData(string productCode, string poid)
        {
            return Json(det.getClickData(poid, productCode), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getPurposeCategory()
        {
            try
            {
                return Json(det.getPurposeCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpPost]
        public ActionResult GetEntity()
        {
            try
            {
                return Json(det.GetEntity(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getpackingGridOne(string productCode)
        {
            try
            {
                return Json(det.getpackingGridOne(productCode), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getMOwithCustomers(string Customers)
        {
            try
            {
                return Json(det.getMOwithCustomers(Customers), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [Authorize, HttpGet]
        public ActionResult getSOfromProduct(string column, string value)
        {
            try
            {
                return Json(det.getSOfromProduct(column, value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpGet]
        public ActionResult getCustomers()
        {
            try
            {
                return Json(det.getCustomers(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getEmployees()
        {
            try
            {
                return Json(det.getEmployees(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getByWhomEmployees()
        {
            try
            {
                return Json(det.getByWhomEmployees(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getStorageLoc()
        {
            try
            {
                return Json(det.getStorageLoc(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getEntity()
        {
            try
            {
                return Json(det.getEntity(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult getSoFromCustomer(string customer)
        {
            try
            {
                return Json(det.getSoFromCustomer(customer), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult getPoLotReference(string productCode, string toDispatch, string PO, string FromDate, string ToDate)
        {
            try
            {
                return Json(det.getPoLotReference(productCode, toDispatch, PO, FromDate, ToDate), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult getCartonsDetails(string LotNo, string ProductCode, string PO)
        {
            try
            {
                var kk = det.getCartonsDetails(LotNo, ProductCode, PO, out List<Dictionary<string, object>> dts);
                return Json(new { Data = kk, Inactive = dts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult getPackingList()
        {
            try
            {
                var kk = det.getPackingList();
                return Json(new { Data = kk }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [Authorize, HttpGet]
        public ActionResult PackingList(string PackingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            GetPackingListReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, PackingId);

            return View();
        }

        public void GetPackingListReport(string companyGroupId, string companyId, string plantId, string UserId, string PackingId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "PackingList" + plantId + ".docx";

            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            WordDocument document = new WordDocument(File, FormatType.Docx);

            try
            {
                WSection section = document.Sections[0];

                DataTable dsOrderMaster;
                DataTable dsTermsAndCondition;

                dsOrderMaster = PackingListSQL(PackingId);
                dsTermsAndCondition = TermsAndConditionSQL(dsOrderMaster.Rows[0]["ContractId"].ToString());

                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makePackingListService(companyGroupId, companyId, plantId, PackingId, document, dsOrderMaster);   // {materialItems}
                var TermsAndCondition = makeTermsAndCondition(companyGroupId, companyId, plantId, PackingId, document, dsTermsAndCondition);   // {materialItems}

                //document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                ////document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                //document.Replace("{TotalInWords}", ru.InWord((MaterialTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);


                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        //ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Tables[0].Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                        document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "PackingListReport" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;

            }

            document.Close();
        }
        public DataTable PackingListSQL(string PackingId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT  P.PackingId, moi.Id MasterOrderItemID,c.Id as ContractId,mm.UserName MaterialDescription,mma.StandardName as Article,h.Code as HSNCode
                                ,PLC.LCRef,Format(PLC.LCDate,'dd-MMM-yyyy') LCDate,B.UserName as IssueingBank,AM.Address1 IssueingBankAddress,sc.GWeight,sc.NetWeight, sc.NoOfPackages

                                ,CartonSerialNo = (Select Stuff((Select distinct ','+isc.RefNo
                                from dbo.ItemScanChild isc 
								left join trn.POLotReference pol on pol.Id = isc.PackingId
							    left join trn.PackingLineItem pli on pli.PackingLineItemId = pol.PackingLineItemId
                                where isc.NetWeight=sc.NetWeight and isc.GWeight=sc.GWeight and pli.PackingId = '"+ PackingId + @"'
                                for xml path('')
                                ),1,1,''))

                                ,sc.TotalQtyNetWeight,sc.GrossWeight,sc.ProductCode, sc.LotNo,FORMAT(p.AddedDate,'dd-MMM-yyyy') PackingDate,
                                u.UserName as UoM,pbt.UserName as ConsigneeBilltoName,pst.UserName as ConsigneeShiptoName,pst.UserName as AcceptedBy,c.InvoicingByAddress as ConsigneeBillToAddress,c.DeliveryByAddress as ConsigneeShipToAddress,cu.Code as CurrencyName,cu.Id CurrencyId,
                                c.ContractNo,FORMAT(c.AddedDate,'dd-MMM-yyyy') AddedDate,PT.UserName PaymentTerm
                              ,SP.SalesId InvoiceNo,FORMAT(S.InvoiceDate,'dd-MMM-yyyy') InvoiceDate
                                from trn.Packing as p 
                                LEFT JOIN TRN.PackingLineItem pli on pli.PackingId=p.PackingId
                                LEFT JOIN TRN.POLotReference plr on plr.PackingLineItemId= pli.PackingLineItemId
                                LEFT JOIN TRN.SalesOrder as so on so.Id=pli.SOId
                                LEFT JOIN TRN.MasterOrderItem as moi on moi.id=so.MasterOrderItemId
                                LEFT JOIN dbo.[contract] as c on c.id = moi.contractId
								
                                LEFT JOIN dbo.PurchaseLC PLC on PLC.ContractId=C.Id						
                                LEFT JOIN  MST.BankMaster IB on IB.Id=PLC.OpeningBankMasterId
                                LEFT JOIN  HKP.Bank B on B.Id=IB.BankId
                                LEFT JOIN MST.AddressMaster AM on AM.Id = B.AddressMasterId
                                
                                LEFT JOIN HKP.Party as pc on pc.Id=c.CustomerId
                                LEFT JOIN HKP.PartyPlant as pbt on pbt.Id=c.InvoicingPartyPlantId
                                LEFT JOIN HKP.PartyPlant as pst on pst.Id=c.DeliveryPartyPlantId
                                LEFT JOIN MST.Destination DS ON DS.Id=SO.DestinationId
                                LEFT JOIN MST.MaterialMaster as mm on mm.Id=moi.MaterialMasterId
                                LEFT JOIN HKP.HSNCode as h on h.Id=mm.HSNCodeId
                                LEFT JOIN MST.MaterialMasterArticle as mma on mma.MaterialMasterId=mm.Id AND MOI.ArticleId=MMA.Id
                                LEFT JOIN TRN.MasterOrder as mo on mo.id=moi.MasterOrderId
                                LEFT JOIN SCS.UnitOfMeasurement as u on u.Id=mo.TotalQtyUOMId
                                LEFT JOIN SCS.Currency as cu on cu.Id=mo.CurrencyId
                                LEFT JOIN MST.PaymentTerm PT ON PT.Id=MO.PaymentTermId
								
                                LEFT JOIN 
                                (
                                SELECT sc.netWeight,sc.GWeight,
                                Count(sc.RefNo) as NoOfPackages, sc.ProductCode ,sc.POId , sc.LotNo
                                ,(sc.NetWeight * Count(sc.RefNo)) as TotalQtyNetWeight,(sc.GWeight * Count(sc.RefNo)) as GrossWeight
                                FROM dbo.ItemScanChild sc 
								LEFT JOIN TRN.POLotReference pol on pol.Id = sc.PackingId
							    LEFT JOIN TRN.PackingLineItem pli on pli.PackingLineItemId = pol.PackingLineItemId
							    WHERE pli.PackingId = '" + PackingId + @"' and sc.IsDespatch = 0
                                GROUP BY  sc.ProductCode ,sc.POId , sc.LotNo,sc.netWeight,sc.GWeight
                                ) as sc on sc.LotNo = plr.LotNo and sc.ProductCode = plr.ProductCode and sc.POId = plr.PONo

                                LEFT JOIN dbo.SalesPacking SP on SP.PackingId=p.PackingId
								LEFT JOIN TRN.Sales S on S.Id=SP.SalesId

                                where P.PackingId ='" + PackingId + @"'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable TermsAndConditionSQL(string ContractId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT ROW_NUMBER() OVER(ORDER BY TC.Sequence) RoWNo,
                        tc.Description as TermsAndConditions from dbo.ContractTermsAndConditions as ctc
                        left outer join hkp.TermsAndConditions as tc on tc.Id=ctc.TermsAndConditionsId
                        where ctc.ContractId='" + ContractId + "' Order By TC.Sequence ";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }


        public double makePackingListService(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{MaterialDescription}";

            DataTable sales, materialTax;

            int LasColumnIndex = 10;

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Sl#");
            range.ApplyCharacterFormat(FontBold);
            int colSrNo = COL; COL++;
            wTable.Rows[ROW].Cells[colSrNo].Width = 35;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description Of Goods");
            range.ApplyCharacterFormat(FontBold);
            int colDescriptionOfGoods = COL; COL++;
            wTable.Rows[ROW].Cells[colDescriptionOfGoods].Width = 110;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 35;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Product Code");
            range.ApplyCharacterFormat(FontBold);
            int colProductCode = COL; COL++;
            wTable.Rows[ROW].Cells[colProductCode].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PR/LOT No");
            range.ApplyCharacterFormat(FontBold);
            int colPRLotNo = COL; COL++;
            wTable.Rows[ROW].Cells[colPRLotNo].Width = 59;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Avg Net Weight/Package");
            range.ApplyCharacterFormat(FontBold);
            int colAvgNetWeight = COL; COL++;
            wTable.Rows[ROW].Cells[colAvgNetWeight].Width = 47;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Avg Gross Weight/Package");
            range.ApplyCharacterFormat(FontBold);
            int colcolAvgGrossWeight = COL; COL++;
            wTable.Rows[ROW].Cells[colcolAvgGrossWeight].Width = 47;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("No OF Package");
            range.ApplyCharacterFormat(FontBold);
            int colNoOFPackage = COL; COL++;
            wTable.Rows[ROW].Cells[colNoOFPackage].Width = 48;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Carton Serial No");
            range.ApplyCharacterFormat(FontBold);
            int colCartonSerialNo = COL; COL++;
            wTable.Rows[ROW].Cells[colCartonSerialNo].Width = 55;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Quantity Net Weight In" + "(" + "" + dsOrderMaster.Rows[0]["UoM"].ToString() + "" + ")" + " ");
            range.ApplyCharacterFormat(FontBold);
            int colTotalQty = COL; COL++;
            wTable.Rows[ROW].Cells[colTotalQty].Width = 58;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Gross Weight In " + "(" + "" + dsOrderMaster.Rows[0]["UoM"].ToString() + "" + ")" + " ");
            range.ApplyCharacterFormat(FontBold);
            int colGrossWeight = COL;
            wTable.Rows[ROW].Cells[colGrossWeight].Width = 45;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            int PreviousNo = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                //
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }

                TROW.Cells[colSrNo].AddParagraph().AppendText(sl.ToString());
                TROW.Cells[colDescriptionOfGoods].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialDescription"].ToString());
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colProductCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ProductCode"].ToString());
                TROW.Cells[colPRLotNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["LotNo"].ToString());
                TROW.Cells[colAvgNetWeight].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["NetWeight"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colcolAvgGrossWeight].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["GWeight"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colNoOFPackage].AddParagraph().AppendText(dsOrderMaster.Rows[i]["NoOfPackages"].ToString());
                //TROW.Cells[colCartonSerialNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CartonSerialNo"].ToString());
                if (i==0)
                {
                    if(Convert.ToInt32(dsOrderMaster.Rows[i]["NoOfPackages"].ToString())==1)
                    TROW.Cells[colCartonSerialNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["NoOfPackages"].ToString());
                    else
                        TROW.Cells[colCartonSerialNo].AddParagraph().AppendText(1 + "-" + dsOrderMaster.Rows[i]["NoOfPackages"].ToString());
                    PreviousNo += Convert.ToInt32(dsOrderMaster.Rows[i]["NoOfPackages"].ToString());

                }
                else
                {
                    int LastPkg = Convert.ToInt32(dsOrderMaster.Rows[i]["NoOfPackages"].ToString()) + PreviousNo;
                    TROW.Cells[colCartonSerialNo].AddParagraph().AppendText(PreviousNo + 1 +"-"+ LastPkg);
                    PreviousNo += Convert.ToInt32(dsOrderMaster.Rows[i]["NoOfPackages"].ToString());

                }


                TROW.Cells[colTotalQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TotalQtyNetWeight"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colGrossWeight].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["GrossWeight"].ToString()).ToString("#,##0.00"));

                //totalValue += clsStdLib.dbl(sales.Rows[i]["TrnAmount"].ToString());
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            range.ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                //|| dicTaxes.ContainsValue(C)
                if (C == colDescriptionOfGoods || C == colHSN || C == colProductCode || C == colPRLotNo || C == colAvgNetWeight || C == colcolAvgGrossWeight || C == colCartonSerialNo)
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
            }
            #endregion Totals
            ROW++;
            #region Sub Total
            //double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(Amount)", "").ToString());
            #endregion Total
            ROW++;


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            //for (int R = 0; R < wTable.Rows.Count; R++)
            //{
            //    WTableRow TROW = wTable.Rows[R];
            //    TROW.Cells[0].Width = 30;
            //    if (dv.Count < 3)
            //        TROW.Cells[0].Width = 30 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

            //    for (int CE = 0; CE < TROW.Cells.Count; CE++)
            //    {
            //        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
            //        {
            //            item.ApplyStyle("MyStyle");
            //        }
            //    }
            //}


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return 0;
        }

        public double makeTermsAndCondition(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsTermsAndCondition)
        {
            string replaceString = "{TermsAndCondition}";


            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("TERMS OF DELIVERY AND PAYMENT");
            range.ApplyCharacterFormat(FontBold);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 250;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsTermsAndCondition.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsTermsAndCondition.Rows[i]["RoWNo"].ToString() + "." + dsTermsAndCondition.Rows[i]["TermsAndConditions"].ToString());

            }
            ROW++;

            #region Total
            //int TotalRow = ROW;
            //wTable.AddRow();
            //WTableRow _TROW = wTable.LastRow;

            //range.ApplyCharacterFormat(FontBold);
            #endregion Total
            ROW++;
            #region paragrpath formats

            IWParagraphStyle myStyle = document.AddParagraphStyle("ServiceStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            #endregion paragrpath formats

            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;
            ROW++;
            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return 0;
        }




        [HttpPost]
        public JsonResult CreateAll(Dictionary<string, object> Packingdata, Dictionary<string, object> PackingLineItemdata, Dictionary<string, object> POLotRefData, List<Dictionary<string, object>> Cartons, List<Dictionary<string, object>> POLotCollection, int lastIndex)
        {
            try
            {
                det.CreateAll(Packingdata, PackingLineItemdata, POLotRefData, Cartons, POLotCollection, lastIndex, out int ll);
                return Json(new { Error = false, Data = Packingdata, lastIndex = ll, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult getgridPacking()
        {
            try
            {

                return Json(det.getgridPacking(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult openPackingLineItemModal(string PackingId)
        {
            try
            {

                return Json(det.getPackingLineItemModal(PackingId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult openPOLotRefGridModal(string PackingLineItemId)
        {
            try
            {
                return Json(det.getPOLotRefGridModal(PackingLineItemId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(string PackingId)
        {

            try
            {
                var workbook = GetFilterData(PackingId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "LoadingPlanReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IWorkbook GetFilterData(string PackingId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            sheet.Name = "Loading Plan Report";




            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable mainHeader = det.getMainHeadersReport(PackingId);
            DataTable data = det.GetReportData(PackingId);
            DataTable packageDetail = det.getPackageDetailReport(PackingId);
            #region Main Headers

            SetHeaderTextTop(ref sheet, ROW, COL, "Customer", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["Customer"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Packing Id", 13, ExcelHAlign.HAlignLeft);

            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["PackingId"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Packing Creation Date", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["Date"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Inactive Date", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["InactiveDate"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "By Whom", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["ByWhom"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            COL = 1;
            ROW++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Despatch Responsible Person Id", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["DRespPerson"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Storage Location", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["StorageLoc"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Entity", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["Entity"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Remarks (If Any)", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["Remarks"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            COL = 1;
            ROW += 2;
            #endregion


            sheet.Range[ROW, COL].Text = "ITEM DETAIL ";
            sheet.Range[ROW, COL].ColumnWidth = 13;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW += 2;
            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Packing Line Id", 13, ExcelHAlign.HAlignCenter);
            int ColPLID = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Master Order No", 13, ExcelHAlign.HAlignCenter);
            int ColMONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Item Id", 13, ExcelHAlign.HAlignCenter);
            int ColItemId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO Id", 13, ExcelHAlign.HAlignCenter);
            int ColSoId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material", 13, ExcelHAlign.HAlignCenter);
            int ColMaterial = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 13, ExcelHAlign.HAlignCenter);
            int ColArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Code", 13, ExcelHAlign.HAlignCenter);
            int ColProductCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PO No", 15, ExcelHAlign.HAlignCenter);
            int ColPONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Lot No", 13, ExcelHAlign.HAlignCenter);
            int ColLotNo = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Plan Qty", 13, ExcelHAlign.HAlignCenter);
            int ColPlanQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO PO No", 13, ExcelHAlign.HAlignCenter);
            int ColSoPoNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Stock Qty", 13, ExcelHAlign.HAlignCenter);
            int ColStockQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "No Of Packages", 13, ExcelHAlign.HAlignCenter);
            int ColPackages = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColPLID].Text = data.Rows[i]["PackingLineItemId"].ToString();
                sheet[ROW, ColMONo].Text = data.Rows[i]["MasterOrderNo"].ToString();
                sheet[ROW, ColItemId].Text = data.Rows[i]["ItemId"].ToString();
                sheet[ROW, ColSoId].Text = data.Rows[i]["SoId"].ToString();
                sheet[ROW, ColMaterial].Text = data.Rows[i]["Material"].ToString();
                sheet[ROW, ColArticle].Text = data.Rows[i]["Article"].ToString();
                sheet[ROW, ColProductCode].Text = data.Rows[i]["ProductCode"].ToString();
                sheet[ROW, ColPONo].Text = data.Rows[i]["PONo"].ToString();
                sheet[ROW, ColLotNo].Text = data.Rows[i]["LotNo"].ToString();
                sheet[ROW, ColPlanQty].Text = data.Rows[i]["PlanQty"].ToString();
                sheet[ROW, ColSoPoNo].Text = data.Rows[i]["SoPoNo"].ToString();
                sheet[ROW, ColStockQty].Text = data.Rows[i]["StockQty"].ToString();
                sheet[ROW, ColPackages].Text = data.Rows[i]["NoOfPackages"].ToString();



                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;
            #region Package Detail
            COL = 1;
            sheet.Range[ROW, COL].Text = "PACKAGE DETAIL ";
            sheet.Range[ROW, COL].ColumnWidth = 13;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW += 2;
            COL = 1;
            report.SetHeaderText(ref sheet, ROW, COL, "Packing Line Id", 13, ExcelHAlign.HAlignCenter);
            int ColPkLineId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Cartons", 13, ExcelHAlign.HAlignCenter);
            sheet.Range[ROW, 2, ROW, 12].Merge();
            int ColCartons = COL;
            ROW++;
            COL = 12;
            sheet.Range[ROW, 2, ROW, COL].Merge();

            for (int i = 0; i < packageDetail.Rows.Count; i++)
            {
                sheet[ROW, ColPkLineId].Text = packageDetail.Rows[i]["PackingLineItemId"].ToString();

                sheet[ROW, ColPkLineId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColCartons].Text = packageDetail.Rows[i]["Cartons"].ToString();
                sheet.Range[ROW, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }
            ROW++;
            #endregion
            endRow = ROW - 1;
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Loading Plan Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }
        private void SetTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {

            sheet.Range[row, col - 1, row, col].Text = txt;
            sheet.Range[row, col - 1, row, col].Merge();
            sheet.Range[row, col - 1, row, col].ColumnWidth = width;
            sheet.Range[row, col - 1, row, col].HorizontalAlignment = al;
            sheet.Range[row, col - 1, row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;

        }
        private void SetText(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {

            sheet.Range[row, col, row, col].Text = txt;
            sheet.Range[row, col, row, col].ColumnWidth = width;
            sheet.Range[row, col, row, col].HorizontalAlignment = al;
            sheet.Range[row, col, row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;

        }


        [HttpPost, Authorize]
        public ActionResult GetStockReport(string ToDate, string FromDate, string type, string group, string column, string value)
        {

            try
            {
                var workbook = GetStockReportForm( ToDate,  FromDate,  type,  group,  column,  value);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "StockReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        private IWorkbook GetStockReportForm(string ToDate, string FromDate, string type, string group, string column, string value)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = det.GetStockData(ToDate, FromDate, type, group, column, value);


            var sheet = workbook.Worksheets[0];
            sheet.Name = "Stock Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
            sheet.Range[ROW, COL].ColumnWidth = 13;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW += 2;

            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Assigned", 13, ExcelHAlign.HAlignCenter);
            int ColAssigned = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Code", 13, ExcelHAlign.HAlignCenter);
            int ColProdCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Production Order", 13, ExcelHAlign.HAlignCenter);
            int ColPO = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Lot No", 13, ExcelHAlign.HAlignCenter);
            int ColLotNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "From Date", 13, ExcelHAlign.HAlignCenter);
            int ColFD = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "From Period", 13, ExcelHAlign.HAlignCenter);
            int ColFP = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Upto Date", 13, ExcelHAlign.HAlignCenter);
            int ColUD = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Despatch", 15, ExcelHAlign.HAlignCenter);
            int ColDespatch = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Planned Qty", 13, ExcelHAlign.HAlignCenter);
            int ColPlannedQty = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Booked Qty", 13, ExcelHAlign.HAlignCenter);
            int ColcolBookedQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Stock Qty", 13, ExcelHAlign.HAlignCenter);
            int ColStckQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO Qty", 13, ExcelHAlign.HAlignCenter);
            int ColSoQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "NO Of SO", 13, ExcelHAlign.HAlignCenter);
            int ColNoOfSO = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ItemId", 13, ExcelHAlign.HAlignCenter);
            int ColItemId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Item Article", 13, ExcelHAlign.HAlignCenter);
            int ColItemArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product", 13, ExcelHAlign.HAlignCenter);
            int ColProd = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Master Order No", 13, ExcelHAlign.HAlignCenter);
            int ColMasterOrderNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Customer", 13, ExcelHAlign.HAlignCenter);
            int ColCustomer = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColAssigned].Text = data.Rows[i]["Assigned"].ToString();
                sheet[ROW, ColProdCode].Text = data.Rows[i]["ProductCode"].ToString();
                sheet[ROW, ColPO].Text = data.Rows[i]["PO"].ToString();
                sheet[ROW, ColLotNo].Text = data.Rows[i]["LotNo"].ToString();
                sheet[ROW, ColFD].Text = data.Rows[i]["fd"].ToString();
                sheet[ROW, ColFP].Text = data.Rows[i]["fp"].ToString();
                sheet[ROW, ColUD].Text = data.Rows[i]["ud"].ToString();
                sheet[ROW, ColDespatch].Text = data.Rows[i]["Despatch"].ToString();
                sheet[ROW, ColPlannedQty].Text = data.Rows[i]["PlannedQty"].ToString();
                sheet[ROW, ColcolBookedQty].Text = data.Rows[i]["BookedQty"].ToString();
                sheet[ROW, ColStckQty].Text = data.Rows[i]["StockQty"].ToString();
                sheet[ROW, ColSoQty].Text = data.Rows[i]["SoQty"].ToString();
                sheet[ROW, ColNoOfSO].Text = data.Rows[i]["NoOfSo"].ToString();
                sheet[ROW, ColItemId].Text = data.Rows[i]["ItemId"].ToString();
                sheet[ROW, ColItemArticle].Text = data.Rows[i]["ItemArticle"].ToString();
                sheet[ROW, ColProd].Text = data.Rows[i]["Product"].ToString();
                sheet[ROW, ColMasterOrderNo].Text = data.Rows[i]["MasterOrderNo"].ToString();
                sheet[ROW, ColCustomer].Text = data.Rows[i]["Customer"].ToString();



                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;
           
            endRow = ROW - 1;
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Stock Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
    }   
}