#region LIB
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion LIB

namespace Library.Service.Administration.Contract
{
    #region General Contract Master
    public class GeneralContractService
    {
        private readonly SqlRepository _sqlRepository;
        public GeneralContractService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region GetFun
        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = @"select *, EI.EmployeeName from HKP.ParameterMaster PM
                    left join EmployeeInformation EI on EI.SystemId = PM.EmpSystemId 
                    where Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM HKP.GeneralContractItemMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        #region SEARCH SAVED DATA IN GRID 
        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                //string TableName = "HKP.GeneralContractItemMaster";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = "GC." + column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"select GC.*, UOM.UserName UOMName from [HKP].[GeneralContractItemMaster] GC
left join SCS.UnitOfMeasurement UOM on UOM.Id = GC.UOMId
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
                string TableNameHead = "HKP.GeneralContractItemMaster";

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

                    data["Id"] = "PM" + _Id;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {

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

                string TableName = "HKP.GeneralContractItemMaster";
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
        #endregion GetFun

        #region General Contract
        public IEnumerable<object> GetContractMaster()
        {
            try
            {
                string sql = @"select '' Id,GC.Id ContractMasterId, GC.UserName ContractMaster, GC.Code, GC.Sequence, GC.Category, GC.SubCategory,
GC.Purpose, GC.Detail, GC.Remarks, UOM.UserName UOMName
from [HKP].[GeneralContractItemMaster] GC
left join SCS.UnitOfMeasurement UOM on UOM.Id = GC.UOMId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion General Contract

        #region Contractor List
        public IEnumerable<object> GetContractorList()
        {
            try
            {
                string sql = @"";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Contractor List

        #region Checked By
        public IEnumerable<object> GetForCheckedByList()
        {
            try
            {
                string sql = @"select '' Id,ei.SystemId, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section, GDSG.UserName LegalDesignation,
                            SBC.UserName as SubSection from dbo.EmployeeInformation ei
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                                     
                            where ei.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Checked By

        #region Approved By
        public IEnumerable<object> GetForApprovedByByList()
        {
            try
            {
                string sql = @"select '' Id,ei.SystemId, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section, GDSG.UserName LegalDesignation,
                            SBC.UserName as SubSection from dbo.EmployeeInformation ei
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId                                     
                            where ei.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Approved By

        #region Entity
        public IEnumerable<object> GetEntity()
        {
            try
            {
                var sql = @"select '' Id,Id EntityId, UserName, EntityType, Code from org.Entity where Active = 1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Entity

        public IEnumerable<object> GetVendorBasedEmployee()
        {
            try
            {
                var sql = @"select '' Id,ei.SystemId, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section, GDSG.UserName LegalDesignation,
                            SBC.UserName as SubSection from dbo.EmployeeInformation ei
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                                     
                            where ei.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    #endregion General Contract Master

    #region GeneralContract
    public class ContractItemDetailService
    {
        private readonly SqlRepository _sqlRepository;
        #region constructor
        public ContractItemDetailService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion constructor

        #region SAVE
        public Dictionary<string, object> Save(Dictionary<string, object> data, List<Dictionary<string, object>> contractItemDetail, List<Dictionary<string, object>> checkby, List<Dictionary<string, object>> approveby, List<Dictionary<string, object>> entity)
        {
            try
            {
                string TableNameHead = "MST.GeneralContract";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                string __Id = "";

                #region  HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = _Id;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion HEAD

                #region ContractItemDetail
                DataSet dsContractItemDetail;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from MST.ContractItemDetail where GeneralContractId ='" + data["Id"].ToString() + "'", out dsContractItemDetail, false, "1");


                foreach (var item in contractItemDetail)
                {
                    DataView dv = new DataView(dsContractItemDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("MST.ContractItemDetail", out __Id);
                        item["Id"] = __Id;
                        item["GeneralContractId"] = data["Id"].ToString();

                        AddNewRow(dsContractItemDetail.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["GeneralContractId"] = data["Id"].ToString();

                        EditRow(drmo, item);
                    }
                }
                #endregion  ContractItemDetail

                #region CheckBy
                DataSet dsCheckBy;
                ConnectionManager.DAL.ConManager conCheckBy = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from MST.GeneralContractCheckBy where GeneralContractId ='" + data["Id"].ToString() + "'", out dsCheckBy, false, "1");

                foreach (var item in checkby)
                {
                    DataView dv = new DataView(dsCheckBy.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("MST.GeneralContractCheckBy", out __Id);
                        item["Id"] = __Id;
                        item["GeneralContractId"] = data["Id"].ToString();

                        AddNewRow(dsCheckBy.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["GeneralContractId"] = data["Id"].ToString();

                        EditRow(drmo, item);
                    }
                }
                #endregion CheckBy

                #region ApproveBy
                DataSet dsApproveBy;
                ConnectionManager.DAL.ConManager conApprovekBy = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from [MST].[GeneralContractApproveBy] where GeneralContractId ='" + data["Id"].ToString() + "'", out dsApproveBy, false, "1");
                foreach (var item in approveby)
                {
                    DataView dv = new DataView(dsApproveBy.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("MST.GeneralContractApproveBy", out __Id);
                        item["Id"] = __Id;
                        item["GeneralContractId"] = data["Id"].ToString();

                        AddNewRow(dsApproveBy.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["GeneralContractId"] = data["Id"].ToString();

                        EditRow(drmo, item);
                    }
                }
                #endregion ApproveBy

                #region Entity
                DataSet dsEntity;
                ConnectionManager.DAL.ConManager conEntity = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from [MST].[GeneralContractEntity] where GeneralContractId ='" + data["Id"].ToString() + "'", out dsEntity, false, "1");
                foreach (var item in entity)
                {
                    DataView dv = new DataView(dsEntity.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("MST.GeneralContractEntity", out __Id);
                        item["Id"] = __Id;
                        item["GeneralContractId"] = data["Id"].ToString();

                        AddNewRow(dsEntity.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["GeneralContractId"] = data["Id"].ToString();

                        EditRow(drmo, item);
                    }
                }
                #endregion Entity

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsContractItemDetail, dsCheckBy, dsApproveBy, dsEntity);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> SaveVendorEmployee(List<Dictionary<string, object>> vendoremployee, string headerId)
        {
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [MST].[GeneralContractVendorEmployee] where GeneralContractId ='" + headerId + "'", out dsMaster, false, "1");
                string _Id = "";

                foreach (var item in vendoremployee)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("GeneralContractVendorEmployee", out _Id);

                        item["Id"] = _Id;
                        item["GeneralContractId"] = headerId;
                        item["EmployeeId"] = item["SystemId"];

                        AddNewRow(dsMaster.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["EmployeeId"] = dv[0].Row["EmployeeId"].ToString();

                        EditRow(drmo, item);
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return vendoremployee;

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

        public IEnumerable<object> GetHeaderList()
        {
            try
            {
                var sql = @"select GC.Id, GC.ShortName, GC.StandardName, GC.UserName, GC.FileName, GC.PartyId, P.UserName PartyName, P.Code PartyCode from MST.GeneralContract GC
                            left join HKP.Party P on P.Id = GC.PartyId
                            order by Id ASC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetContractItemDetail(string gcId)
        {
            try
            {
                var sql = @"select CI.Id, CI.ContractMasterId, GCIM.UserName ContractMaster, CI.GeneralContractId, GC.UserName GeneralContract, 
                            CI.MinQty, CI.MaxQty, CI.AvgQty, CI.Rate,format(CI.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
                            from MST.ContractItemDetail CI
                            left join MST.GeneralContract GC on GC.Id = CI.GeneralContractId
                            left join HKP.GeneralContractItemMaster GCIM on GCIM.Id = CI.ContractMasterId
                            where GC.Id = '" + gcId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetGeneralContractEmpDetail(string gcId)
        {
            try
            {
                var sql = @"select GCV.*,ei.EmployeeCode,ei.EmployeeName,ei.DOJ,D.UserName Department,S.UserName Section,SS.UserName SubSection
                                    ,LD.UserName LegalDesignation,DD.UserName Designation
                                    from [MST].[GeneralContractVendorEmployee] GCV
                                    left join MST.GeneralContract GC on GC.Id=GCV.GeneralContractId
                                    left join EmployeeInformation ei on ei.SystemId=GCV.EmployeeId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                    left join ORG.Department D on D.Id=pr.DepartmentId
                                    left join ORG.Section S on S.Id=pr.SectionId
                                    left join ORG.SubSection SS on SS.Id=pr.SubSectionId
                                    left join HKP.LegalDesignation LD on LD.Id=ei.LegalDesignationId
                                    left join HKP.Designation DD on DD.Id=pr.DesignationID
                            where GC.Id = '" + gcId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetCheckByList(string gcId)
        {
            try
            {
                var sql = @"select CB.Id, CB.isCheck, EI.SystemId EmployeeId, EI.EmployeeCode, EI.EmployeeName, FORMAT(EI.DOJ, 'dd-MMM-yyyy')DOJ,
                            DP.UserName as Department, LDSG.StandardName as Designation, SC.UserName as Section, GDSG.UserName LegalDesignation,                           
                             SBC.UserName as SubSection, CB.GeneralContractId, GC.UserName GeneralContract
                            from MST.GeneralContractCheckBy CB
                            left join MST.GeneralContract GC on GC.Id = CB.GeneralContractId 
                            left join EmployeeInformation EI on EI.SystemId = CB.SystemId
                             LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
							where GC.Id = '" + gcId + "'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetApproveByList(string gcId)
        {
            try
            {
                var sql = @"select AB.Id, AB.isApprove, EI.SystemId EmployeeId, EI.EmployeeCode, EI.EmployeeName, FORMAT(EI.DOJ, 'dd-MMM-yyyy')DOJ,
                            DP.UserName as Department, LDSG.StandardName as Designation, SC.UserName as Section, GDSG.UserName LegalDesignation,                           
                             SBC.UserName as SubSection, AB.GeneralContractId, GC.UserName GeneralContract
                            from MST.GeneralContractApproveBy AB
                            left join MST.GeneralContract GC on GC.Id = AB.GeneralContractId 
                            left join EmployeeInformation EI on EI.SystemId = AB.SystemId
                             LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
							where GC.Id = '" + gcId + "'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSaveEntityList(string gcId)
        {
            try
            {
                var str = @"select GE.Id, GE.EntityId, E.EntityType, E.UserName, E.Code, G.Id 
                            from MST.GeneralContractEntity GE
                            left join org.Entity E on E.Id = GE.EntityId
                            left join MST.GeneralContract G on G.Id = GE.GeneralContractId
                            where G.Id = '" + gcId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
    #endregion GeneralContract

    #region COntract Entry
    public class ContractEntryService
    {
        private readonly SqlRepository _sqlRepository;
        public ContractEntryService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region SAVE
        public Dictionary<string, object> Save(Dictionary<string, object> data, List<Dictionary<string, object>> contractItemDetail)
        {
            try
            {
                string TableNameHead = "TRN.GeneralContractEntry";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                string __Id = "";

                #region FURNITURE POLICY HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = _Id;
                    data["CheckedByStatus"] = "To Be Check";
                    data["IsCancel"] = 0;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion FURNITURE POLICY HEAD

                #region Child

                DataSet dsChild;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from TRN.ContractItemEntry where GeneralContractEntryId ='" + data["Id"].ToString() + "'", out dsChild, false, "1");


                foreach (var item in contractItemDetail)
                {

                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.ContractItemEntry", out __Id);
                        item["Id"] = __Id;
                        item["GeneralContractEntryId"] = data["Id"].ToString();

                        AddNewRow(dsChild.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["GeneralContractEntryId"] = data["Id"].ToString();

                        EditRow(drmo, item);
                    }
                }

                #endregion Child

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);

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

        public Dictionary<string, object> CancelContractEntry(Dictionary<string, object> data,string Name)
        {
            try
            {
                string TableNameHead = "TRN.GeneralContractEntry";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                 
                if (dsMaster.Tables[0].Rows.Count > 0)
                { 
                    data["IsCancel"] = 1;
                    data["CancelBy"] = Name;
                    data["CancelDateTime"] = System.DateTime.Now.ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    #endregion COntract Entry

    #region ContractReport
    public class ContractReportService
    {
        SqlRepository _sqlRepository;
        public ContractReportService()
        {
            _sqlRepository = new SqlRepository();
        }

        public void GetContractTransactionExcelReport(string from, string to, string contractid, string entityid, out DataTable data)
        {
            var sql = "";
            try
            {
                if (entityid == "null" || entityid == "undefined")
                {
                    if (contractid != "null" && contractid != "undefined")
                    {
                        sql = @"select GC.UserName ContractName , FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, GCI.UserName Item, 
CIE.TransactionQuantity Quantity
, CIE.Rate, CIE.Amount, EI.EmployeeName 'CheckBy', EMP.EmployeeName 'ApproveBy', GCE.ApprovedStatus, 
GCE.CheckedByStatus , CIE.Remarks , CIE.AddedBy , format(CIE.AddedDate, 'dd-MMM-yyyy') AddedDate ,CIE.AvgQty
from TRN.GeneralContractEntry GCE
LEFT JOIN TRN.ContractItemEntry CIE on CIE.GeneralContractEntryId = GCE.Id
LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
left join EmployeeInformation EI on EI.SystemId = GCE.CheckBySystemId
left join EmployeeInformation EMP on EMP.SystemId = GCE.ApprovedById
left join mst.GeneralContract GC on GC.Id = GCE.GeneralContractId
                        where GCE.Date between '" + from + "' and '" + to + "' and GCE.GeneralContractId = '" + contractid + "'";
                    }
                    else
                    {
                        sql = @"select GC.UserName ContractName , FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, GCI.UserName Item, 
CIE.TransactionQuantity Quantity
, CIE.Rate, CIE.Amount, EI.EmployeeName 'CheckBy', EMP.EmployeeName 'ApproveBy', GCE.ApprovedStatus, 
GCE.CheckedByStatus , CIE.Remarks , CIE.AddedBy , format(CIE.AddedDate, 'dd-MMM-yyyy') AddedDate ,CIE.AvgQty
from TRN.GeneralContractEntry GCE
LEFT JOIN TRN.ContractItemEntry CIE on CIE.GeneralContractEntryId = GCE.Id
LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
left join EmployeeInformation EI on EI.SystemId = GCE.CheckBySystemId
left join EmployeeInformation EMP on EMP.SystemId = GCE.ApprovedById
left join mst.GeneralContract GC on GC.Id = GCE.GeneralContractId
                        where GCE.Date between '" + from + "' and '" + to + "' ";
                    }
                }
                if (entityid != "null" && entityid != "undefined")
                {
                    if (contractid == "null" || contractid == "undefined")
                    {
                        sql = @"select GC.UserName ContractName , FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, GCI.UserName Item, 
CIE.TransactionQuantity Quantity
, CIE.Rate, CIE.Amount, EI.EmployeeName 'CheckBy', EMP.EmployeeName 'ApproveBy', GCE.ApprovedStatus, 
GCE.CheckedByStatus , CIE.Remarks , CIE.AddedBy , format(CIE.AddedDate, 'dd-MMM-yyyy') AddedDate ,CIE.AvgQty
from TRN.GeneralContractEntry GCE
LEFT JOIN TRN.ContractItemEntry CIE on CIE.GeneralContractEntryId = GCE.Id
LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
left join EmployeeInformation EI on EI.SystemId = GCE.CheckBySystemId
left join EmployeeInformation EMP on EMP.SystemId = GCE.ApprovedById
left join mst.GeneralContract GC on GC.Id = GCE.GeneralContractId
                        where GCE.Date between '" + from + "' and '" + to + "'  and E.Id = '" + entityid + "'";
                    }
                    else
                    {
                        sql = @"select GC.UserName ContractName , FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, GCI.UserName Item, 
CIE.TransactionQuantity Quantity
, CIE.Rate, CIE.Amount, EI.EmployeeName 'CheckBy', EMP.EmployeeName 'ApproveBy', GCE.ApprovedStatus, 
GCE.CheckedByStatus , CIE.Remarks , CIE.AddedBy , format(CIE.AddedDate, 'dd-MMM-yyyy') AddedDate ,CIE.AvgQty
from TRN.GeneralContractEntry GCE
LEFT JOIN TRN.ContractItemEntry CIE on CIE.GeneralContractEntryId = GCE.Id
LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
left join EmployeeInformation EI on EI.SystemId = GCE.CheckBySystemId
left join EmployeeInformation EMP on EMP.SystemId = GCE.ApprovedById
left join mst.GeneralContract GC on GC.Id = GCE.GeneralContractId
                        where GCE.Date between '" + from + "' and '" + to + "' and GCE.GeneralContractId = '" + contractid + "' and E.Id = '" + entityid + "'";
                    }
                }

                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ContractTransactionSummaryExlReport(string from, string to, string contractid, string entityid, out DataTable data)
        {
            var sql = "";
            try
            {

                if (entityid == null || entityid == "null")
                {
                    sql = @"select  GCI.UserName Item, CIE.TransactionQuantity Quantity
                            , CIE.Amount
                            from TRN.GeneralContractEntry GCE
                            LEFT JOIN TRN.ContractItemEntry CIE on CIE.GeneralContractEntryId = GCE.Id
                            LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
                            left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
                        where GCE.Date between '" + from + "' and '" + to + "' and GCE.GeneralContractId = '" + contractid + "'";
                }
                else
                {
                    sql = @"select  GCI.UserName Item, CIE.TransactionQuantity Quantity
                            , CIE.Amount
                            from TRN.GeneralContractEntry GCE
                            LEFT JOIN TRN.ContractItemEntry CIE on CIE.GeneralContractEntryId = GCE.Id
                            LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
                            left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
                        where GCE.Date between '" + from + "' and '" + to + "' and GCE.GeneralContractId = '" + contractid + "' and E.Id = '" + entityid + "'";
                }

                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    #endregion  ContractReport

    #region GeneralContractCheckService
    public class GeneralContractCheckService
    {
        SqlRepository _sqlRepository;
        public GeneralContractCheckService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region SAVE
        public void GeneralContractChecked(string headerId, string CheckedStataus, string AuthorizedById, string CheckedReason)
        {
            try
            {
                string _sql = "Update TRN.GeneralContractEntry set CheckedByStatus='" + CheckedStataus + "',ApprovedStatus='To Be Approve', ApprovedById='" + AuthorizedById + "', CheckedReason='" + CheckedReason + "' where Id='" + headerId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GeneralContractAuth(string headerId, string ApprovedStataus, string AuthorizedById, string ApprovedReason)
        {
            try
            {
                string _sql = "Update TRN.GeneralContractEntry set ApprovedStatus='" + ApprovedStataus + "', ApprovedReason='" + ApprovedReason + "' where Id='" + headerId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SAVE

        public void GetGeneralContractCheckedHeaderData(string ContractId, out DataTable dtOrder)
        {
            try
            {
                string strSql = string.Empty;
                strSql = @"select  GCE.Id, FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, EI.EmployeeName, GC.UserName Contract,GCE.CheckedByStatus,GCE.AddedBy,EIM.EmployeeName ApprovedBy
                                    from TRN.GeneralContractEntry GCE
                                    LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
                                    LEFT JOIN MST.GeneralContract GC ON GC.Id = GCE.GeneralContractId
                                    left join EmployeeInformation EI on EI.SystemId = GCE.CheckBySystemId
                                    left join EmployeeInformation EIM on EIM.SystemId = GCE.ApprovedById
                                    where GCE.Id='" + ContractId + "'";

                dtOrder = _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function
        public void GetGeneralContractCheckedDetailsData(string ContractId, out DataTable dtDetail)
        {
            try
            {
                string strSql = string.Empty;
                strSql = @"select CIE.*, GCI.UserName Item
                                    from TRN.ContractItemEntry CIE
                                    left join TRN.GeneralContractEntry GCE on GCE.Id = CIE.GeneralContractEntryId
                                    left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
                                    where GCE.Id='" + ContractId + "'";

                dtDetail = _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function

    }
    #endregion GeneralContractCheckService
}
