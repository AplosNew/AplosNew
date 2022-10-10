using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee
{
    #region Medicine Master
    public class MedicineMasterService
    {
        private readonly SqlRepository _sqlRepository;
        #region constructor
        public MedicineMasterService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion constructor

        #region GET
        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = @"Select Sequence, Code, Id, UserName, Category, SubCategory, Rate, IsActive Remarks, 
                            STUFF((
                            SELECT ',' + p.UserName

                            FROM HKP.MedicineMasterPurpose pp
                            left join hkp.MedicinePurpose p on p.Id = pp.MedicinePurposeId
                            where pp.MedicineMasterId = pm.Id
                            FOR XML PATH('')

                            ),1,1,'') AS MedicinePurpose


                            from HKP.MedicineMaster pm where Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getMedicinePurpose()
        {
            try
            {
                var sql = @"select Id Value, StandardName Text from HKP.MedicinePurpose";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion GET

        #region GET SEQUENCE
        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM HKP.MedicineMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        #endregion GET SEQUENCE

        #region SEARCH SAVED DATA IN GRID 
        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string TableName = "HKP.MedicineMaster";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"Select Sequence, Code, ShortName, StandardName, Id, UserName, Category, SubCategory, Rate, IsActive Remarks, 
                            STUFF((
                            SELECT ',' + p.UserName

                            FROM HKP.MedicineMasterPurpose pp
                            left join hkp.MedicinePurpose p on p.Id = pp.MedicinePurposeId
                            where pp.MedicineMasterId = pm.Id
                            FOR XML PATH('')

                            ),1,1,'') AS MedicinePurpose


                            from HKP.MedicineMaster PM
                                where " + strkey + "order by Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SEARCH SAVED DATA IN GRID

        #region SAVE
        public Dictionary<string, object> Save(Dictionary<string, object> data, List<string> medicinepurpose)
        {
            try
            {
                string TableNameHead = "HKP.MedicineMaster";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where StandardName='" + data["StandardName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Standard Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region Medicine HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = "MM" + _Id;
                    //dsMaster.Tables[0].Rows.Add(dr);

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion Medicine POLICY HEAD

                #region MedicineMasterPurpose child

                DataSet dsMedicinePurposeChild;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from HKP.MedicineMasterPurpose where MedicineMasterId ='" + data["Id"].ToString() + "'", out dsMedicinePurposeChild, false, "1");

                while (dsMedicinePurposeChild.Tables[0].DefaultView.Count > 0)
                {
                    dsMedicinePurposeChild.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < medicinepurpose.Count; i++)
                {
                    DataRow dr = dsMedicinePurposeChild.Tables[0].NewRow();
                    dr["Id"] = data["Id"].ToString() + i.ToString();
                    dr["MedicineMasterId"] = data["Id"].ToString();
                    dr["MedicinePurposeId"] = medicinepurpose[i].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMedicinePurposeChild.Tables[0].Rows.Add(dr);
                }

                #endregion MedicineMasterPurpose child

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMedicinePurposeChild);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SAVE

        #region SAVEPURPOSE
        public Dictionary<string, object> SavePurpose(Dictionary<string, object> data, string medicineMasterId)
        {
            try
            {
                string TableNameHead = "HKP.MedicinePurpose";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where MedicineMasterId='" + medicineMasterId + "'", out dsMaster, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = "Mp" + _Id;
                    data["MedicineMasterId"] = "MP" + _Id;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["MedicineMasterId"] = "MP" + _Id;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion FURNITURE POLICY HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SAVE

        #region DELETE
        public string Delete(string id)
        {
            try
            {

                string TableName = "HKP.MedicineMaster";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }
        #endregion DELETE

        #region CREATE AND EDIT DEFAULT COLUMN
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion CREATE AND EDIT DEFAULT COLUMN
    }
    #endregion Medicine Master

    #region Medicine Purpose
    public class MedicinePurposeService
    {
        private readonly SqlRepository _sqlRepository;
        #region constructor
        public MedicinePurposeService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion constructor

        #region GET
        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = "select * from HKP.MedicinePurpose where Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion GET

        #region GET SEQUENCE
        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM HKP.MedicinePurpose");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        #endregion GET SEQUENCE

        #region SEARCH SAVED DATA IN GRID 
        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string TableName = "HKP.MedicinePurpose";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT MP.* FROM HKP.MedicinePurpose MP
                                where " + strkey + "order by Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SEARCH SAVED DATA IN GRID

        #region SAVE
        public Dictionary<string, object> Save(Dictionary<string, object> data)
        {
            try
            {
                string TableNameHead = "HKP.MedicinePurpose";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where StandardName='" + data["StandardName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Standard Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = "MP" + _Id;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion FURNITURE POLICY HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SAVE

        #region DELETE
        public string Delete(string id)
        {
            try
            {

                string TableName = "HKP.MedicinePurpose";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }
        #endregion DELETE

        #region CREATE AND EDIT DEFAULT COLUMN
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion CREATE AND EDIT DEFAULT COLUMN
    }
    #endregion Medicine Purpose

    #region Medicine Receipt
    public class MedicineReceiptService
    {
        private readonly SqlRepository _sqlRepository;

        #region CONSTR
        public MedicineReceiptService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion CONSTR

        #region GET
        public IEnumerable<object> getMedicineData()
        {
            try
            {
                var str = @"select M.Id, M.Code, M.Sequence, M.UserName Medicine, M.Category, 
                            M.SubCategory, M.Remarks from HKP.MedicineMaster M";
                //var str = @"Select Id Value, UserName Text from HKP.MedicineMaster";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getMedicineReceipt()
        {
            try
            {
                var str = @"select M.Category, M.Id MedicineMaster, M.StandardName Medicine, MR.InvoiceNumber, FORMAT(MR.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, 
FORMAT(MRC.ExpiryDate, 'dd-MMM-yyyy')ExpiryDate, MRC.Quantity, MRC.Rate, MRC.Amount, P.UserName Party,
MRC.Id MedicineReceiptChildId, MR.Id MedicineReceiptId
from TRN.MedicineReceiptChild MRC
left join TRN.MedicineReceipt MR on MR.Id = MRC.MedicineReceiptId
left join HKP.MedicineMaster M on M.Id = MRC.MedicineMasterId
left join HKP.Party P on P.Id = mR.PartyId
order by MRC.ExpiryDate";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion GET

        #region SAVE
        public Dictionary<string, object> SaveHeader(Dictionary<string, object> data, List<Dictionary<string, object>> medicinelist, string partyId)
        {
            try
            {
                string TableNameHead = "TRN.MedicineReceipt";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region MEDICINE RECEIPT HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = "MR" + _Id;
                    data["PartyId"] = partyId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion MEDICINE RECEIPT HEAD

                #region MedicineMasterPurpose child

                DataSet dsMedicineReceiptChild;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from TRN.MedicineReceiptChild where MedicineReceiptId ='" + data["Id"].ToString() + "'", out dsMedicineReceiptChild, false, "1");

                while (dsMedicineReceiptChild.Tables[0].DefaultView.Count > 0)
                {
                    dsMedicineReceiptChild.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < medicinelist.Count; i++)
                {
                    DataRow dr = dsMedicineReceiptChild.Tables[0].NewRow();
                    dr["Id"] = data["Id"].ToString() + '-' + i.ToString();
                    dr["MedicineReceiptId"] = data["Id"].ToString();
                    dr["MedicineMasterId"] = medicinelist[i]["Id"].ToString();

                    dr["ExpiryDate"] = medicinelist[i]["ExpiryDate"].ToString();
                    dr["Quantity"] = medicinelist[i]["Quantity"].ToString();
                    dr["Amount"] = medicinelist[i]["Amount"].ToString();
                    dr["Rate"] = medicinelist[i]["Rate"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMedicineReceiptChild.Tables[0].Rows.Add(dr);
                }

                #endregion MedicineMasterPurpose child


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMedicineReceiptChild);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SAVE

        #region CREATE AND EDIT DEFAULT COLUMN
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion CREATE AND EDIT DEFAULT COLUMN

        #region PRINT PDF
        public DataTable loadOrderMaster(string medicinereceiptId)
        {
            string strSQL;
            try
            {

                strSQL = @"select MR.Id MedicineReceiptId, M.Category, M.Id MedicineMaster, M.StandardName Medicine, MR.InvoiceNumber, FORMAT(MR.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, 
FORMAT(MRC.ExpiryDate, 'dd-MMM-yyyy')ExpiryDate, MRC.Quantity, MRC.Rate, MRC.Amount, P.UserName Part,
MRC.Id MedicineReceiptChildId
from TRN.MedicineReceipt MR
LEFT JOIN TRN.MedicineReceiptChild MRC ON MRC.MedicineReceiptId = mr.Id
LEFT JOIN HKP.MedicineMaster M on M.Id = MRC.MedicineMasterId
left join HKP.Party P on P.Id = MR.PartyId
where MR.Id = '" + medicinereceiptId + "' order by MRC.ExpiryDate";
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

      
        public void GePurchaseOrderReport(string companyGroupId, string companyId, string plantId, string userId, string purchaseOrderId)
        {
            ReportUtility ru = new ReportUtility();
            var fileName = "";
            var strPath = "";
            var File = "";
            fileName = "PurchaseOrder" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);

            //Gets the paragraph at index 1
            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];
                //var DiscountAmount = "";

                DataTable dsOrderMaster, dsServiceItems, dsTermsAndCondition;
                dsOrderMaster = loadOrderMaster(purchaseOrderId);//sql
               
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                invoicePartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dsOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);
                vendorPartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);
                document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);
                document.Replace("{SpecialInstruction}", dsOrderMaster.Rows[0]["SpecialInstruction"].ToString(), false, false);
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
              
                List<string> strReplace = new List<string>();
                
                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }

                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);
                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                //Syncfusion.Pdf.PdfDocument pdfDocument = converter.ConvertToPDF(document);
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "PurchaseOrder" + purchaseOrderId;
                //Saves the PDF file 
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //Closes the instance of document objects
            document.Close();
        }
        #endregion PRINT PDF

        #region SEARCH SAVED DATA IN GRID 
        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string TableName = "HKP.MedicineMaster";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"Select Sequence, Code, ShortName, StandardName, Id, UserName Medicine, Category, SubCategory, Rate, IsActive Remarks, 
                            STUFF((
                            SELECT ',' + p.UserName

                            FROM HKP.MedicineMasterPurpose pp
                            left join hkp.MedicinePurpose p on p.Id = pp.MedicinePurposeId
                            where pp.MedicineMasterId = pm.Id
                            FOR XML PATH('')

                            ),1,1,'') AS MedicinePurpose


                            from HKP.MedicineMaster PM
                                where " + strkey + "order by Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SEARCH SAVED DATA IN GRID

    }
    #endregion Medicine Receipt
}

