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

        public IEnumerable<object> getMedicinePurposeCategory(List<string>medincinepurpose)
        {
            try
            {
                var sql = "";
                for (int i = 0; i < medincinepurpose.Count; i++)
                {
                     sql = @"select Category Text, Id from HKP.MedicinePurpose MP
                            where MP.Id in('" + medincinepurpose[i].ToString() + "')";
                }
                
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getUOM()
        {
            try
            {
                var sql = @"select Id, StandardName, UserName, IsComercialUnit, Description, Remarks from scs.UnitOfMeasurement";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> searchUOM(string column, string value)
        {
            try
            {
                string TableName = "HKP.MedicineMaster";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"select Id, StandardName, UserName, IsComercialUnit, Description, Remarks from scs.UnitOfMeasurement 
                               where " + strkey + "";
                return _sqlRepository.GetDataCollection(sql, null);
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

                string sql = @"Select PM.Sequence, PM.Code, PM.ShortName, PM.StandardName, PM.Id, PM.UserName,  
                                PM.IsActive, PM.MinStockQty, U.StandardName UOMName, U.Id UOMId, PM.Remarks, 

                            STUFF((
                            SELECT ',' + p.UserName

                            FROM HKP.MedicineMasterPurpose pp
                            left join hkp.MedicinePurpose p on p.Id = pp.MedicinePurposeId
                            where pp.MedicineMasterId = pm.Id
                            FOR XML PATH('')

                            ),1,1,'') AS MedicinePurpose


                            from HKP.MedicineMaster PM
							left join SCS.UnitOfMeasurement U on U.Id = PM.UOMId
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

        public IEnumerable<object> GetCategory()
        {
            try
            {
                var sql = "select Id Value, UserName Text from HKP.MedicineCategory where  IsActive = 1";
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

                string sql = @"SELECT MP.*, MC.UserName MedicineCategory FROM HKP.MedicinePurpose MP
                                LEFT JOIN HKP.MedicineCategory MC on MC.Id = MP.MedicineCategoryId
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

    #region Medicine Category
    public class MedicineCategoryService
    {
        private readonly SqlRepository _sqlRepository;
        #region constructor
        public MedicineCategoryService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion constructor

        #region GET
        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = "select * from HKP.MedicineCategory where Id = '" + Id + "' ";
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
            DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM HKP.MedicineCategory");
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
                string TableName = "HKP.MedicineCategory";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT MC.* FROM HKP.MedicineCategory MC
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
                string TableNameHead = "HKP.MedicineCategory";

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

                string TableName = "HKP.MedicineCategory";
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
    #endregion Medicine Category

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
                var str = @"Select PM.Sequence, PM.Code, PM.ShortName, PM.StandardName, PM.Id, PM.UserName,  
                                PM.IsActive, PM.MinStockQty, U.StandardName UOMName, U.Id UOMId, PM.Remarks, 

                            STUFF((
                            SELECT ',' + p.UserName

                            FROM HKP.MedicineMasterPurpose pp
                            left join hkp.MedicinePurpose p on p.Id = pp.MedicinePurposeId
                            where pp.MedicineMasterId = pm.Id
                            FOR XML PATH('')

                            ),1,1,'') AS MedicinePurpose


                            from HKP.MedicineMaster PM
							left join SCS.UnitOfMeasurement U on U.Id = PM.UOMId
							where PM.IsActive = 1
							order by PM.UserName";
                //var str = @"Select Id Value, UserName Text from HKP.MedicineMaster";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPlant()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
               
                var str = @"select Id Value, StandardName Text from ORG.Plant where Id = '"+ identity.PlantId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> getMedicineReceipt()
        {
            try
            {
               /* var str = @"select MR.Id, M.Id MedicineMaster, M.StandardName Medicine, MR.InvoiceNumber, FORMAT(MR.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, 
FORMAT(MRC.ExpiryDate, 'dd-MMM-yyyy')ExpiryDate, MRC.Quantity, MRC.Rate, MRC.Amount, P.UserName PartyName,  P.Code PartyCode,
MRC.Id MedicineReceiptChildId, MR.Id MedicineReceiptId, MR.PlantId, PL.StandardName PlantName
from TRN.MedicineReceiptChild MRC
left join TRN.MedicineReceipt MR on MR.Id = MRC.MedicineReceiptId
left join HKP.MedicineMaster M on M.Id = MRC.MedicineMasterId
left join HKP.Party P on P.Id = mR.PartyId
left join ORG.Plant PL on PL.Id = MR.PartyId
order by MRC.ExpiryDate";*/

                var str = @"select isnull(sum(MRC.Amount),0)Amount, MR.PartyId, P.UserName PartyName, P.Code PartyCode, MR.InvoiceNumber
, FORMAT(MR.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, MR.Id, MR.PlantId, PL.StandardName PlantName , MR.IsActive
from TRN.MedicineReceiptChild MRC
left join TRN.MedicineReceipt MR on MR.Id = MRC.MedicineReceiptId
left join HKP.Party P on P.Id = MR.PartyId
left join ORG.Plant PL on PL.Id = MR.PlantId
where MR.IsActive = 1
Group By MR.PartyId, P.UserName, P.Code, MR.InvoiceNumber, MR.InvoiceDate, MR.Id, MR.PlantId, PL.StandardName, MR.IsActive
";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetChildValue(string masterId)
        {
            try
            {
                var str = @"select ROW_NUMBER() OVER(ORDER BY MR.Id) SrNo, MRC.Id , M.Id MedicineMasterId, M.StandardName UserName, MR.InvoiceNumber, FORMAT(MR.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, 
FORMAT(MRC.ExpiryDate, 'dd-MMM-yyyy')ExpiryDate, MRC.Quantity, MRC.Rate, MRC.Amount, P.UserName PartyName,  P.Code PartyCode,
MRC.Id MedicineReceiptChildId, MR.Id MedicineReceiptId, MR.PlantId, PL.StandardName PlantName, IsOpeningQty
from TRN.MedicineReceiptChild MRC
left join TRN.MedicineReceipt MR on MR.Id = MRC.MedicineReceiptId
left join HKP.MedicineMaster M on M.Id = MRC.MedicineMasterId
left join HKP.Party P on P.Id = mR.PartyId
left join ORG.Plant PL on PL.Id = MR.PartyId
where MRC.MedicineReceiptId = '" + masterId + @"'
--order by MRC.ExpiryDate";

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

        #region Update
        public Dictionary<string, object> Update(Dictionary<string, object> data, List<Dictionary<string, object>> medicinelist, string partyId)
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
                    //_Id = data["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion MEDICINE RECEIPT HEAD

                #region MedicineMasterPurpose child

                DataSet dsMedicineReceiptChild;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from TRN.MedicineReceiptChild where MedicineReceiptId ='" + data["Id"].ToString() + "'", out dsMedicineReceiptChild, false, "1");

                //while (dsMedicineReceiptChild.Tables[0].DefaultView.Count > 0)
                //{
                //    dsMedicineReceiptChild.Tables[0].DefaultView[0].Delete();
                //}
                int count = 0;
                foreach (var item in medicinelist)
                {
                    DataView dv = new DataView(dsMedicineReceiptChild.Tables[0]);
                    dv.RowFilter = "Id='" + item["MedicineReceiptChildId"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = data["Id"].ToString() + '-' + count++;
                        item["MedicineReceiptId"] = data["Id"].ToString();
                        
                        AddNewRow(dsMedicineReceiptChild.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["MedicineReceiptId"] = data["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #region comment
                /* for (int i = 0; i < medicinelist.Count; i++)
                 {
                     DataRow dr = dsMedicineReceiptChild.Tables[0].NewRow();
                     if (dsMedicineReceiptChild.Tables[0].Rows.Count == 0)
                     {
                         dr["Id"] = data["Id"].ToString() + '-' + i.ToString();
                         dr["MedicineReceiptId"] = data["Id"].ToString();
                         dr["MedicineMasterId"] = medicinelist[i]["MedicineMasterId"].ToString();

                         dr["ExpiryDate"] = medicinelist[i]["ExpiryDate"].ToString();
                         dr["Quantity"] = medicinelist[i]["Quantity"].ToString();
                         dr["Amount"] = medicinelist[i]["Amount"].ToString();
                         dr["Rate"] = medicinelist[i]["Rate"].ToString();
                         dr["AddedBy"] = identity.Name;
                         dr["AddedDate"] = System.DateTime.Now.ToString();
                         dr["AddedFromIP"] = identity.IPAddress;
                     }
                     else
                     {
                         dr["MedicineReceiptId"] = data["Id"].ToString();
                         dr["MedicineMasterId"] = medicinelist[i]["MedicineMasterId"].ToString();

                         dr["ExpiryDate"] = medicinelist[i]["ExpiryDate"].ToString();
                         dr["Quantity"] = medicinelist[i]["Quantity"].ToString();
                         dr["Amount"] = medicinelist[i]["Amount"].ToString();
                         dr["Rate"] = medicinelist[i]["Rate"].ToString();
                         dr["UpdatedBy"] = identity.Name;
                         dr["UpdatedDate"] = System.DateTime.Now.ToString();
                         dr["UpdatedFromIP"] = identity.IPAddress;
                     }
                     dsMedicineReceiptChild.Tables[0].Rows.Add(dr);
                 }*/
                #endregion comment

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
        #endregion Update

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
        public DataTable GetMedicineReceiptReport(string headerid)
        {
            string strSQL;
            try
            {

                strSQL = @"select MR.Id, M.Id MedicineMaster, M.StandardName Medicine, MR.InvoiceNumber, FORMAT(MR.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, 
                            FORMAT(MRC.ExpiryDate, 'dd-MMM-yyyy')ExpiryDate, MRC.Quantity, MRC.Rate, MRC.Amount, P.UserName PartyName,  P.Code PartyCode,
                            MRC.Id MedicineReceiptChildId, MR.Id MedicineReceiptId, MR.PlantId, PL.StandardName PlantName,

                            STUFF((Select ',' + MP.UserName
                            from HKP.MedicinePurpose MP
                            left join HKP.MedicineMasterPurpose MMP on MMP.MedicinePurposeId = MP.Id 
                            where M.Id = MMP.MedicineMasterId
                            FOR XML PATH('')),1,1,'') Purpose

                            from TRN.MedicineReceiptChild MRC
                            left join TRN.MedicineReceipt MR on MR.Id = MRC.MedicineReceiptId
                            left join HKP.MedicineMaster M on M.Id = MRC.MedicineMasterId
                            left join HKP.Party P on P.Id = MR.PartyId
                            left join ORG.Plant PL on PL.Id = MR.PlantId
                            where MR.Id = '"+headerid+"' order by MRC.ExpiryDate";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            
        }

        public void GetAllInvoiceDataPrint(string from, string to, out DataTable data)
        {
            try
            {
                var sql = @"select isnull(sum(MRC.Amount),0)Amount, MR.PartyId, P.UserName PartyName, P.Code PartyCode, MR.InvoiceNumber
                            , FORMAT(MR.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate, MR.Id, MR.PlantId, PL.StandardName PlantName 
                            from TRN.MedicineReceiptChild MRC
                            left join TRN.MedicineReceipt MR on MR.Id = MRC.MedicineReceiptId
                            left join HKP.Party P on P.Id = MR.PartyId
                            left join ORG.Plant PL on PL.Id = MR.PlantId
                            Group By MR.PartyId, P.UserName, P.Code, MR.InvoiceNumber, MR.InvoiceDate, 
                            MR.Id, MR.PlantId, PL.StandardName";
                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {

                throw;
            }
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
                    strkey = "PM." + column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"Select PM.Sequence, PM.Code, PM.ShortName, PM.StandardName, PM.Id, PM.UserName,  
                                PM.IsActive, PM.MinStockQty, U.StandardName UOMName, U.Id UOMId, PM.Remarks, 

                            STUFF((
                            SELECT ',' + p.UserName

                            FROM HKP.MedicineMasterPurpose pp
                            left join hkp.MedicinePurpose p on p.Id = pp.MedicinePurposeId
                            where pp.MedicineMasterId = pm.Id
                            FOR XML PATH('')

                            ),1,1,'') AS MedicinePurpose


                            from HKP.MedicineMaster PM
							left join SCS.UnitOfMeasurement U on U.Id = PM.UOMId
                                where " + strkey + "order by Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SEARCH SAVED DATA IN GRID

        #region Delete
        public string RemoveParticular(string id)
        {
            try
            {

                string TableName = "TRN.MedicineReceiptChild";
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
        #endregion

    }
    #endregion Medicine Receipt

    #region Medical Log
    public class MedicalLogServce
    {
        private readonly SqlRepository _sqlRepository;
        #region Const
        public MedicalLogServce()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Const

        #region GET OP
        public IEnumerable<object> GetMedicineList()
        {
            try
            {
                
                var sql = @"select distinct MM.Id, MM.StandardName Medicine, MM.Remarks, isnull(MR.Receipt,0)-isnull(ESM.Issue,0) Stock  
                            from hkp.MedicineMaster MM
                            left join (select mm.Id,sum(MRC.Quantity) Receipt from hkp.MedicineMaster MM
                            left join trn.MedicineReceiptChild MRC on MRC.MedicineMasterId = MM.id group by mm.Id) MR on MR.Id = MM.Id
                            left join (SELECT MM.Id, SUM(ESM.Quantity) Issue FROM HKP.MedicineMaster MM
                            LEFT JOIN TRN.MedicineReceiptChild MRC ON MRC.MedicineMasterId = MM.Id
                            LEFT JOIN TRN.EmployeeSicknessMedicines ESM ON ESM.MedicineReceiptChildId = MRC.Id
                            group by mm.id) ESM on ESM.Id = MM.Id
                            WHERE (isnull(MR.Receipt,0)-isnull(ESM.Issue,0))>0";
               
                
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMedicineByReceipt(string medicinemasterId)
        {
            try
            {
                var sql = @"SELECT '' Id, MRC.Id MedicineReceiptChildId, MM.UserName Medicine,isnull(MR.Receipt,0)-isnull(ESM.Issue,0) Stock, FORMAT(MRC.ExpiryDate, 'dd-MMM-yyyy')ExpiryDate
                            FROM TRN.MedicineReceiptChild MRC 
                            LEFT JOIN hkp.MedicineMaster MM ON MM.Id=MRC.MedicineMasterId
                            left join (select mm.Id,sum(MRC.Quantity) Receipt from hkp.MedicineMaster MM
                            left join trn.MedicineReceiptChild MRC on MRC.MedicineMasterId = MM.id group by mm.Id) MR on MR.Id = MM.Id
                            left join (SELECT MM.Id, SUM(ESM.Quantity) Issue FROM HKP.MedicineMaster MM
                            LEFT JOIN TRN.MedicineReceiptChild MRC ON MRC.MedicineMasterId = MM.Id
                            LEFT JOIN TRN.EmployeeSicknessMedicines ESM ON ESM.MedicineReceiptChildId = MRC.Id
                            Group By mm.id) ESM on ESM.Id = MM.Id
                        where MRC.Quantity is not null and MM.Id = '" + medicinemasterId + "'order by MRC.ExpiryDate";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSicknessType()
        {
            try
            {
                var sql = @"select '' Id, MP.Id MedicinePurposeId, MP.UserName Sickness, MC.UserName Category, MP.Remarks PurposeRemarks,
						MC.Remarks CategoryRemarks
						from HKP.MedicinePurpose MP
						left join HKP.MedicineCategory MC on MC.Id = MP.MedicineCategoryId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmployee()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string str = @"select EMP.EmployeeCode as Code, EMP.SystemId, EMP.EmployeeName, SC.UserName as Section, SBC.UserName SubSection,
                                LDSG.UserName Designation, EMP.DOJ,
                                GDSG.UserName as GivenDesignation, UN.UserName as Entity
                                from EmployeeInformation EMP
                                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                                where EMP.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region SEARCH SAVED DATA IN GRID 
        public IEnumerable<object> getSearchSicknessData(string column, string value)
        {
            try
            {
                string TableName = "HKP.MedicinePurpose";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"select UserName Purpose, Category Sickness from HKP.MedicinePurpose 
                               where " + strkey + "";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SEARCH SAVED DATA IN GRID

       
        public IEnumerable<object> MedicallogGridView()
        {
            try
            {
                var SQL = @"select Top 5000 ML.Id, FORMAT(ML.Date, 'dd-MMM-yyyy')[Date], 
EMP.EmployeeCode, EMP.EmployeeName, ML.Remarks,  ML.NoOfVisits, FORMAT(ML.Time, 'hh:mm tt')Time,
STUFF((select ', ' + MC.UserName
from TRN.EmployeeSickness ES
LEFT join HKP.MedicinePurpose MP on MP.Id = ES.MedicinePurposeId
LEFT JOIN HKP.MedicineCategory MC ON MC.Id = MP.MedicineCategoryId
where ES.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Sickness,

STUFF((Select ',' + MP.UserName
from HKP.MedicineCategory MC
left join HKP.MedicinePurpose MP on MP.MedicineCategoryId = MC.Id
left join TRN.EmployeeSickness ES on ES.MedicinePurposeId = MP.Id
where ES.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Purpose,

STUFF((Select ', ' +  CONVERT(VARCHAR(20),ESM.Quantity)
from TRN.EmployeeSicknessMedicines ESM
LEFT JOIN TRN.MedicineReceiptChild MRC on MRC.Id = ESM.MedicineReceiptChildId
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
where ESM.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Quantity
from TRN.MedicalLog ML
left join EmployeeInformation EMP ON EMP.SystemId = ML.EmployeeSystemId
--INNER JOIN TRN.EmployeeSicknessMedicines x on x.MedicalLogId = ML.Id
GROUP BY ML.Id, ML.Date, EMP.EmployeeCode, EMP.EmployeeName, ML.Remarks, ML.NoOfVisits, ML.Time
order by  ml.Date  desc
";

                return _sqlRepository.GetDataCollection(SQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMedicineChildForUpdate(string masterId)
        {
            try
            {
                string sql = @"select ESM.Id, ML.Id MedicalLogId, MM.UserName Medicine, ESM.Quantity, ESM.NoOfDays, 
ESM.Remarks, MRC.Id MedicineReceiptChildId
from TRN.EmployeeSicknessMedicines ESM
left join TRN.MedicalLog ML on ML.Id =  ESM.MedicalLogId
left join TRN.MedicineReceiptChild MRC on MRC.Id = ESM.MedicineReceiptChildId
left join HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
where ESM.MedicalLogId = '" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSicknessChildForUpdate(string masterId)
        {
            try
            {
                string sql = @"select ES.Id, ML.Id MedicalLogId, MC.UserName Category, MP.UserName Sickness, 
MP.Id MedicinePurposeId, ES.Remarks
from TRN.EmployeeSickness ES
left join TRN.MedicalLog ML on ML.Id = ES.MedicalLogId
left join HKP.MedicinePurpose MP on MP.Id = ES.MedicinePurposeId
left join HKP.MedicineCategory MC on MC.Id = MP.MedicineCategoryId
where ES.MedicalLogId = '" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion GET OP

        #region Visit Count
        public IEnumerable<object> CountEmpVisits(string empsystemCode)
        {
            //DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(NoOfVisits),0) AS NoOfVisits FROM TRN.MedicalLog where EmployeeSystemId = '" + empsystemCode + "'");
            //if (dt.Rows.Count > 0)
            //    return clsStaticInfo.dbl(dt.Rows[0]["NoOfVisits"].ToString()) + 1;

            //return 1;
            try
            {
                var sql = @"SELECT isnull(Max(NoOfVisits),0) AS NoOfVisits FROM TRN.MedicalLog where EmployeeSystemId = '"+ empsystemCode + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }
        #endregion Visit Count

        #region SAVE
        public Dictionary<string, object> Save(Dictionary<string, object> data, List<Dictionary<string, object>> medicinepurposelist, List<Dictionary<string, object>> medicinelist, string empSystemId)
        {
            try
            {
                string TableNameHead = "TRN.MedicalLog";

                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region MEDICAL LOG HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = "ML" + _Id;
                    data["EmployeeSystemId"] = empSystemId;
                    
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    //_Id = data["Id"].ToString();
                    data["EmployeeSystemId"] = empSystemId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion MEDICAL LOG HEAD

                #region sickness child

                DataSet dsMedicinePurposeChild;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from TRN.EmployeeSickness where MedicalLogId ='" + data["Id"].ToString() + "'", out dsMedicinePurposeChild, false, "1");

                int count = 0;
                if (medicinepurposelist != null)
                {
                    foreach (var item in medicinepurposelist)
                    {
                        DataView dv = new DataView(dsMedicinePurposeChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        
                        if (dv.Count == 0)
                        {
                            item["Id"] = data["Id"].ToString() + '-' + count++;
                            item["MedicalLogId"] =  data["Id"];
                           
                            AddNewRow(dsMedicinePurposeChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #region commnet 1
                /*for (int i = 0; i < medicinepurposelist.Count; i++)
                {
                   
                    if (dsMedicinePurposeChild.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMedicinePurposeChild.Tables[0].NewRow();
                        dr["Id"] = data["Id"].ToString() + '-' + i.ToString();
                        dr["MedicalLogId"] = data["Id"].ToString();
                        dr["MedicinePurposeId"] = medicinepurposelist[i]["Id"];
                        dr["Remarks"] = medicinepurposelist[i]["Remarks"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMedicinePurposeChild.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsMedicinePurposeChild.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["MedicalLogId"] = data["Id"].ToString();
                        dr["MedicinePurposeId"] = medicinepurposelist[i]["Id"];
                        dr["Remarks"] = medicinepurposelist[i]["Remarks"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();

                        
                    }

                }*/
                #endregion commnet 1

                #endregion sickness child

                #region SAVE MEDICINE CHILD
                DataSet dsMedicineChild;
                ConnectionManager.DAL.ConManager Medicineconn = new ConnectionManager.DAL.ConManager("1");
                Medicineconn.OpenDataSetThroughAdapter("select * from TRN.EmployeeSicknessMedicines where MedicalLogId ='" + data["Id"].ToString() + "'", out dsMedicineChild, false, "1");

                count = 0;
                if (medicinelist != null)
                {
                    foreach (var item in medicinelist)
                    {
                        DataView dv = new DataView(dsMedicineChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = data["Id"].ToString() + '-' + count++;
                            item["MedicalLogId"] = data["Id"];

                            AddNewRow(dsMedicineChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #region comment 2
                /* for (int i = 0; i < medicinelist.Count; i++)
                 {
                     if (dsMedicineChild.Tables[0].Rows.Count == 0)
                      {
                          DataRow dr = dsMedicineChild.Tables[0].NewRow();
                          dr["Id"] = data["Id"].ToString() + '-' + i.ToString();
                          dr["MedicineReceiptChildId"] = medicinelist[i]["Id"].ToString();
                          dr["MedicalLogId"] = data["Id"].ToString();
                          dr["Quantity"] = medicinelist[i]["Quantity"];
                          dr["NoOfDays"] = medicinelist[i]["NoOfDays"];
                          dr["Remarks"] = medicinelist[i]["Remarks"];
                          dr["AddedBy"] = identity.Name;
                          dr["AddedDate"] = System.DateTime.Now.ToString();
                          dr["AddedFromIP"] = identity.IPAddress;
                          dsMedicineChild.Tables[0].Rows.Add(dr);
                      }
                      else
                      {
                          DataRow dr = dsMedicineChild.Tables[0].DefaultView[0].Row;
                          dr.BeginEdit();
                          //dr["Id"] = data["Id"].ToString() + i.ToString();
                          dr["MedicineReceiptChildId"] = medicinelist[i]["Id"].ToString();
                          dr["MedicalLogId"] = data["Id"].ToString();
                          dr["Quantity"] = medicinelist[i]["Quantity"];
                          dr["Quantity"] = medicinelist[i]["Quantity"];
                          dr["NoOfDays"] = medicinelist[i]["NoOfDays"];
                          dr["Remarks"] = medicinelist[i]["Remarks"];
                          dr["UpdatedBy"] = identity.Name;
                          dr["UpdatedDate"] = System.DateTime.Now.ToString();
                          dr["UpdatedFromIP"] = identity.IPAddress;

                          dr.EndEdit();


                      }

                 }*/
                #endregion comment 2

                #endregion SAVE MEDICINE CHILD
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMedicinePurposeChild, dsMedicineChild);

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

        #region GET SEQUENCE
        public double CountEmployeeVisiting(string empSytemId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(NoOfVisits),1) AS NoOfVisits FROM TRN.MedicalLog where EmployeeSystemId = '"+ empSytemId + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["NoOfVisits"].ToString()) + 1;

            return 1;
        }
        #endregion GET SEQUENCE

        public IEnumerable<object> getSearchedEmployee(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey =  column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"select EMP.EmployeeCode as Code, EMP.SystemId, EMP.EmployeeName, SC.UserName as Section, SBC.UserName SubSection,
                                LDSG.UserName Designation, FORMAT(EMP.DOJ,'dd-MMM-yyyy')DOJ,
                                GDSG.UserName as GivenDesignation, UN.UserName as Entity
                                from EmployeeInformation EMP
                                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                --left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                                where " + strkey + " and EMP.EmployeeStatus='Active' ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    #endregion Medical Log

    #region Medical Log Report
    public class MedicalLogReportService
    {
        private readonly SqlRepository _sqlRepository;
        public MedicalLogReportService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region GET OP
        public IEnumerable<object> GetMedicinePopUp()
        {
            try
            {
                var sql = @"select distinct MM.UserName, MRC.MedicineMasterId, MC.UserName Category,
STUFF((select ',' + P.UserName 
FROM HKP.MedicineMasterPurpose pp
                            left join hkp.MedicinePurpose p on p.Id = pp.MedicinePurposeId
                            where pp.MedicineMasterId = MM.Id
                            FOR XML PATH('')

                            ),1,1,'') AS MedicinePurpose

from TRN.MedicineReceiptChild MRC
left join HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
left join hkp.MedicineMasterPurpose X on X.MedicineMasterId = MM.Id
left join HKP.MedicinePurpose Y on Y.Id = X.MedicinePurposeId
left join HKP.MedicineCategory MC on MC.Id = Y.MedicineCategoryId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMedicalLogEmployee()
        {
            try
            {
                
                string str = @"select distinct ML.EmployeeSystemId
,EMP.EmployeeCode as Code, EMP.SystemId, EMP.EmployeeName, SC.UserName as Section, SBC.UserName SubSection,
LDSG.UserName Designation, EMP.DOJ, GDSG.UserName as GivenDesignation, UN.UserName as Entity                               
from TRN.MedicalLog ML
left join EmployeeInformation EMP on EMP.SystemId = ML.EmployeeSystemId
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion GET OP

        #region Grid View Query
        public IEnumerable<object> medicallogGridView(string from, string to, string empSystemId)
        {
            var SQL = "";
            try
            {
                if (empSystemId != null)
                {
                    SQL = @"select distinct x.NoOfDays [Days], ML.Id, FORMAT(ML.Date, 'dd-MMM-yyyy')[Date], EMP.EmployeeCode, 
DP.UserName Department, SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation,
UN.UserName Entity, ML.NoOfVisits
,EMP.EmployeeName, ML.Remarks, GDSG.UserName GivenDesignation, x.Quantity,
STUFF((select ', ' + MC.UserName
from TRN.EmployeeSickness ES
LEFT join HKP.MedicinePurpose MP on MP.Id = ES.MedicinePurposeId
LEFT JOIN HKP.MedicineCategory MC ON MC.Id = MP.MedicineCategoryId
where ES.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Sickness,

STUFF((Select ', ' + MM.UserName
from TRN.EmployeeSicknessMedicines ESM
--LEFT JOIN TRN.MedicineReceipt MR on MR.Id = ESM.MedicineReceiptChildId
LEFT JOIN TRN.MedicineReceiptChild MRC on MRC.Id = ESM.MedicineReceiptChildId
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
where ESM.MedicalLogId = ML.Id

FOR XML PATH('')),1,1,'') Medicines,
STUFF((Select ', ' +  CONVERT(VARCHAR(20),ESM.Quantity)
from TRN.EmployeeSicknessMedicines ESM
--LEFT JOIN TRN.MedicineReceipt MR on MR.Id = ESM.MedicineReceiptChildId
LEFT JOIN TRN.MedicineReceiptChild MRC on MRC.Id = ESM.MedicineReceiptChildId
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
where ESM.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Quantity

,STUFF((SELECT ',' + MP.UserName  FROM HKP.MedicinePurpose MP
left join TRN.EmployeeSickness ES on ES.MedicinePurposeId = MP.Id
left join TRN.MedicalLog ML on ML.Id = ES.MedicalLogId
left join EmployeeInformation EI on EI.SystemId = ML.EmployeeSystemId
where EI.SystemId = EMP.SystemId
FOR XML PATH('')),1,1,'') Purpose

, ML.AddedBy
from TRN.MedicalLog ML
INNER JOIN TRN.EmployeeSicknessMedicines x on x.MedicalLogId = ML.Id
left join EmployeeInformation EMP ON EMP.SystemId = ML.EmployeeSystemId
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
left join hkp.EmployeeCategory EC on x.Id=dm.EmployeeCategoryId
where ML.[Date] between '" + from + "' and '" + to + "' and EMP.SystemId = '" + empSystemId + "' and EMP.EmployeeStatus = 'Active'" +
"GROUP BY ML.Id, ML.Date, EMP.EmployeeCode, EMP.EmployeeName, ML.Remarks, x.NoOfDays, DP.UserName, SC.UserName, SBC.UserName, LDSG.UserName, UN.UserName, GDSG.UserName ,ML.AddedBy, EMP.SystemId, ML.NoOfVisits";
                }
                else
                {
                    SQL = @"select distinct x.NoOfDays [Days], ML.Id, FORMAT(ML.Date, 'dd-MMM-yyyy')[Date], EMP.EmployeeCode, 
DP.UserName Department, SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation,
UN.UserName Entity, ML.NoOfVisits
,EMP.EmployeeName, ML.Remarks, GDSG.UserName GivenDesignation,
STUFF((select ', ' + MC.UserName
from TRN.EmployeeSickness ES
LEFT join HKP.MedicinePurpose MP on MP.Id = ES.MedicinePurposeId
LEFT JOIN HKP.MedicineCategory MC ON MC.Id = MP.MedicineCategoryId
where ES.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Sickness,

STUFF((Select ', ' + MM.UserName
from TRN.EmployeeSicknessMedicines ESM
--LEFT JOIN TRN.MedicineReceipt MR on MR.Id = ESM.MedicineReceiptChildId
LEFT JOIN TRN.MedicineReceiptChild MRC on MRC.Id = ESM.MedicineReceiptChildId
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
where ESM.MedicalLogId = ML.Id

FOR XML PATH('')),1,1,'') Medicines,
STUFF((Select ', ' +  CONVERT(VARCHAR(20),ESM.Quantity)
from TRN.EmployeeSicknessMedicines ESM
--LEFT JOIN TRN.MedicineReceipt MR on MR.Id = ESM.MedicineReceiptChildId
LEFT JOIN TRN.MedicineReceiptChild MRC on MRC.Id = ESM.MedicineReceiptChildId
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
where ESM.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Quantity

,STUFF((SELECT ',' + MP.UserName  FROM HKP.MedicinePurpose MP
left join TRN.EmployeeSickness ES on ES.MedicinePurposeId = MP.Id
left join TRN.MedicalLog ML on ML.Id = ES.MedicalLogId
left join EmployeeInformation EI on EI.SystemId = ML.EmployeeSystemId
where EI.SystemId = EMP.SystemId
FOR XML PATH('')),1,1,'') Purpose

,ML.AddedBy
from TRN.MedicalLog ML
INNER JOIN TRN.EmployeeSicknessMedicines x on x.MedicalLogId = ML.Id
left join EmployeeInformation EMP ON EMP.SystemId = ML.EmployeeSystemId
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
left join hkp.EmployeeCategory EC on x.Id=dm.EmployeeCategoryId
where ML.[Date] between '" + from + "' and '" + to + "' and EMP.EmployeeStatus = 'Active'" +
"GROUP BY ML.Id, ML.Date, EMP.EmployeeCode, EMP.EmployeeName, ML.Remarks, x.NoOfDays, DP.UserName, SC.UserName, SBC.UserName, LDSG.UserName, UN.UserName, GDSG.UserName ,ML.AddedBy, EMP.SystemId, ML.NoOfVisits";
                }


                return _sqlRepository.GetDataCollection(SQL);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        
        public IEnumerable<object> GetMedinceStockGrid(string fromDate, string toDate)
        {
            try
            {
                string sql = @"DECLARE @MedicineMasterId VARCHAR(20)='MM17';

SELECT X.Medicine, (SELECT MAX(ExpiryDate) FROM TRN.MedicineReceiptChild WHERE MedicineMasterId=X.MedicineMasterId  )ExpiryDate
,SUM(ISNULL(X.[Opening Quantity],0)) OpeningQuantity,SUM(ISNULL(X.TrnsReceivedQty,0)) TrnsReceivedQty,SUM(ISNULL(X.TrnsIssueQty,0)) TrnsIssueQty
,ClosingQty=SUM(ISNULL(X.[Opening Quantity],0)) + SUM(ISNULL(X.TrnsReceivedQty,0)) - SUM(ISNULL(X.TrnsIssueQty,0)) 
FROM (select MRC.MedicineMasterId,MM.UserName Medicine, [Opening Quantity]  = isnull(MRC.Quantity,0),0 TrnsReceivedQty,0 TrnsIssueQty
from TRN.MedicineReceipt MR
LEFT JOIN TRN.MedicineReceiptChild MRC ON MRC.MedicineReceiptId = MR.Id
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
left join (select ISnull(SUM(Quantity),0)IssueQty, MedicineMasterId 
from TRN.EmployeeSicknessMedicines WHERE CONVERT(date,AddedDate)<='19-NOV-2022' GROUP BY MedicineMasterId) ESM on ESM.MedicineMasterId = MRC.MedicineMasterId
WHERE CONVERT(date,MRC.AddedDate)<'" + fromDate + @"'
GROUP BY MRC.MedicineMasterId,MM.UserName, MRC.Quantity

UNION ALL
select MRC.MedicineMasterId,MM.UserName Medicine, 0 [Opening Quantity] ,SUM(isnull(MRC.Quantity,0)) TrnsReceivedQty,0 TrnsIssueQty
from TRN.MedicineReceipt MR
LEFT JOIN TRN.MedicineReceiptChild MRC ON MRC.MedicineReceiptId = MR.Id
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
WHERE CONVERT(date,MRC.AddedDate) BETWEEN '" + fromDate + "' AND '" + toDate + @"'
GROUP BY MRC.MedicineMasterId,MM.UserName

UNION ALL
select ESM.MedicineMasterId,MM.UserName Medicine, 0 [Opening Quantity] ,0 TrnsReceivedQty,SUM(isnull(ESM.Quantity,0)) TrnsIssueQty
from TRN.EmployeeSicknessMedicines  ESM
LEFT JOIN HKP.MedicineMaster MM on MM.Id = ESM.MedicineMasterId
WHERE CONVERT(date,ESM.AddedDate) BETWEEN '" + fromDate + "' AND '" + toDate + @"'
GROUP BY ESM.MedicineMasterId,MM.UserName)X
GROUP BY X.Medicine,X.MedicineMasterId";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion Grid View Query

        #region Excel View Query
        public DataTable medicallogExcelView(string from, string to, string empSystemId)
        {
            var SQL = "";
            try
            {
                if (empSystemId != null)
                {
                    SQL = @"select distinct x.NoOfDays [Days], ML.Id, FORMAT(ML.Date, 'dd-MMM-yyyy')[Date], EMP.EmployeeCode, 
DP.UserName Department, SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation,
UN.UserName Entity, ML.NoOfVisits
,EMP.EmployeeName, ML.Remarks, GDSG.UserName GivenDesignation, x.Quantity,
STUFF((select ', ' + MC.UserName
from TRN.EmployeeSickness ES
LEFT join HKP.MedicinePurpose MP on MP.Id = ES.MedicinePurposeId
LEFT JOIN HKP.MedicineCategory MC ON MC.Id = MP.MedicineCategoryId
where ES.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Sickness,

STUFF((Select ', ' + MM.UserName
from TRN.EmployeeSicknessMedicines ESM
--LEFT JOIN TRN.MedicineReceipt MR on MR.Id = ESM.MedicineReceiptChildId
LEFT JOIN TRN.MedicineReceiptChild MRC on MRC.Id = ESM.MedicineReceiptChildId
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
where ESM.MedicalLogId = ML.Id

FOR XML PATH('')),1,1,'') Medicines,

STUFF((Select ', ' +  CONVERT(VARCHAR(20),ESM.Quantity)
from TRN.EmployeeSicknessMedicines ESM
--LEFT JOIN TRN.MedicineReceipt MR on MR.Id = ESM.MedicineReceiptChildId
LEFT JOIN TRN.MedicineReceiptChild MRC on MRC.Id = ESM.MedicineReceiptChildId
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
where ESM.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Quantity

,STUFF((SELECT ',' + MP.UserName  FROM HKP.MedicinePurpose MP
left join TRN.EmployeeSickness ES on ES.MedicinePurposeId = MP.Id
left join TRN.MedicalLog ML on ML.Id = ES.MedicalLogId
left join EmployeeInformation EI on EI.SystemId = ML.EmployeeSystemId
where EI.SystemId = EMP.SystemId
FOR XML PATH('')),1,1,'') Purpose

, ML.AddedBy
from TRN.MedicalLog ML
INNER JOIN TRN.EmployeeSicknessMedicines x on x.MedicalLogId = ML.Id
left join EmployeeInformation EMP ON EMP.SystemId = ML.EmployeeSystemId
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
left join hkp.EmployeeCategory EC on x.Id=dm.EmployeeCategoryId
where ML.[Date] between '" + from + "' and '" + to + "' and EMP.SystemId = '" + empSystemId + "' and EMP.EmployeeStatus = 'Active'" +
"GROUP BY ML.Id, ML.Date, EMP.EmployeeCode, EMP.EmployeeName, ML.Remarks, x.NoOfDays, DP.UserName, SC.UserName, SBC.UserName, LDSG.UserName, UN.UserName, GDSG.UserName, ML.AddedBy, EMP.SystemId, ML.NoOfVisits";
                }
                else
                {
                    SQL = @"select distinct x.NoOfDays [Days], ML.Id, FORMAT(ML.Date, 'dd-MMM-yyyy')[Date], EMP.EmployeeCode, 
DP.UserName Department, SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation,
UN.UserName Entity, ML.NoOfVisits
,EMP.EmployeeName, ML.Remarks, GDSG.UserName GivenDesignation,
STUFF((select ', ' + MC.UserName
from TRN.EmployeeSickness ES
LEFT join HKP.MedicinePurpose MP on MP.Id = ES.MedicinePurposeId
LEFT JOIN HKP.MedicineCategory MC ON MC.Id = MP.MedicineCategoryId
where ES.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Sickness,

STUFF((Select ', ' + MM.UserName
from TRN.EmployeeSicknessMedicines ESM
--LEFT JOIN TRN.MedicineReceipt MR on MR.Id = ESM.MedicineReceiptChildId
LEFT JOIN TRN.MedicineReceiptChild MRC on MRC.Id = ESM.MedicineReceiptChildId
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
where ESM.MedicalLogId = ML.Id

FOR XML PATH('')),1,1,'') Medicines,
STUFF((Select ', ' +  CONVERT(VARCHAR(20),ESM.Quantity)
from TRN.EmployeeSicknessMedicines ESM
--LEFT JOIN TRN.MedicineReceipt MR on MR.Id = ESM.MedicineReceiptChildId
LEFT JOIN TRN.MedicineReceiptChild MRC on MRC.Id = ESM.MedicineReceiptChildId
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
where ESM.MedicalLogId = ML.Id
FOR XML PATH('')),1,1,'') Quantity

,STUFF((SELECT ',' + MP.UserName  FROM HKP.MedicinePurpose MP
left join TRN.EmployeeSickness ES on ES.MedicinePurposeId = MP.Id
left join TRN.MedicalLog ML on ML.Id = ES.MedicalLogId
left join EmployeeInformation EI on EI.SystemId = ML.EmployeeSystemId
where EI.SystemId = EMP.SystemId
FOR XML PATH('')),1,1,'') Purpose

, ML.AddedBy
from TRN.MedicalLog ML
INNER JOIN TRN.EmployeeSicknessMedicines x on x.MedicalLogId = ML.Id
left join EmployeeInformation EMP ON EMP.SystemId = ML.EmployeeSystemId
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
left join hkp.EmployeeCategory EC on x.Id=dm.EmployeeCategoryId
where ML.[Date] between '" + from + "' and '" + to + "' and EMP.EmployeeStatus = 'Active'" +
    "GROUP BY ML.Id, ML.Date, EMP.EmployeeCode, EMP.EmployeeName, ML.Remarks, x.NoOfDays, DP.UserName, SC.UserName, SBC.UserName, LDSG.UserName, UN.UserName, GDSG.UserName, ML.AddedBy, EMP.SystemId, ML.NoOfVisits";
                }
                return _sqlRepository.GetDataTable(SQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable medicineStockExcelView(string fromDate ,string toDate)
        {
            try
            {
                //string mdid = "";
                //if (!string.IsNullOrEmpty(medicineId)|| medicineId !="null")
                //{
                //    mdid = "and ";
                //}
                string sql = @"DECLARE @MedicineMasterId VARCHAR(20)='MM17';

SELECT X.Medicine,SUM(ISNULL(X.[Opening Quantity],0)) OpeningQuantity,SUM(ISNULL(X.TrnsReceivedQty,0)) TrnsReceivedQty,SUM(ISNULL(X.TrnsIssueQty,0)) TrnsIssueQty
,ClosingQty=SUM(ISNULL(X.[Opening Quantity],0)) + SUM(ISNULL(X.TrnsReceivedQty,0)) - SUM(ISNULL(X.TrnsIssueQty,0)) 
,(SELECT MAX(ExpiryDate) FROM TRN.MedicineReceiptChild WHERE MedicineMasterId=X.MedicineMasterId  )ExpiryDate
FROM (select MRC.MedicineMasterId,MM.UserName Medicine, [Opening Quantity] =SUM(isnull(MRC.Quantity,0)) - SUM(isnull(ESM.IssueQty,0)),0 TrnsReceivedQty,0 TrnsIssueQty
from TRN.MedicineReceipt MR
LEFT JOIN TRN.MedicineReceiptChild MRC ON MRC.MedicineReceiptId = MR.Id
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
left join (select ISnull(SUM(Quantity),0)IssueQty, MedicineMasterId 
from TRN.EmployeeSicknessMedicines WHERE CONVERT(date,AddedDate)<='19-NOV-2022' GROUP BY MedicineMasterId) ESM on ESM.MedicineMasterId = MRC.MedicineMasterId
WHERE CONVERT(date,MRC.AddedDate)<'"+ fromDate + @"'
GROUP BY MRC.MedicineMasterId,MM.UserName

UNION ALL
select MRC.MedicineMasterId,MM.UserName Medicine, 0 [Opening Quantity] ,SUM(isnull(MRC.Quantity,0)) TrnsReceivedQty,0 TrnsIssueQty
from TRN.MedicineReceipt MR
LEFT JOIN TRN.MedicineReceiptChild MRC ON MRC.MedicineReceiptId = MR.Id
LEFT JOIN HKP.MedicineMaster MM on MM.Id = MRC.MedicineMasterId
WHERE CONVERT(date,MRC.AddedDate) BETWEEN '"+ fromDate + "' AND '"+ toDate + @"'
GROUP BY MRC.MedicineMasterId,MM.UserName

UNION ALL
select ESM.MedicineMasterId,MM.UserName Medicine, 0 [Opening Quantity] ,0 TrnsReceivedQty,SUM(isnull(ESM.Quantity,0)) TrnsIssueQty
from TRN.EmployeeSicknessMedicines  ESM
LEFT JOIN HKP.MedicineMaster MM on MM.Id = ESM.MedicineMasterId
WHERE CONVERT(date,ESM.AddedDate) BETWEEN '" + fromDate + "' AND '" + toDate + @"'
GROUP BY ESM.MedicineMasterId,MM.UserName)X
GROUP BY X.Medicine,X.MedicineMasterId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion Excel View Query

       

    }
    #endregion Medical Log Report
}

