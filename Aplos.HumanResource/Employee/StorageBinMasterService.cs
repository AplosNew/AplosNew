using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.HumanResource.Employee
{
  public  class StorageBinMasterService
    {
        private readonly SqlRepository _sqlRepository;
        public StorageBinMasterService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = "sb."+column + " like '%" + value + "%'";
                    

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //string sql = @"SELECT sb.Id, sb.UserName, ms.Id as StorageLocationId, ms.UserName as StorageLocation, e.SystemId as ResponsiblePersonId, e.EmployeeName as EmployeeName, sb.StorageSubLocation,
                //            sb.AreaRackCode, sb.ColumnNo, sb.RowNo, sb.BinCode, sb.BinReference, sb.UserName, sb.CapacityValue,
                //            sb.AccessType, sb.UserLocationType, sb.Remarks
                //            FROM MST.StorageBinMaster sb
                //            left join hkp.MaterialStorage ms on ms.Id = sb.StorageLocation
                //            left join dbo.EmployeeInformation e on e.SystemId = sb.ResponsiblePersonId";

                string sql = @"SELECT sb.Id as Id, sb.UserName, ms.Id as StorageLocationId, ms.UserName as StorageLocation, e.SystemId as ResponsiblePersonId, e.EmployeeName as EmployeeName, sb.StorageSubLocation,
                            sb.AreaRackCode, sb.ColumnNo, sb.RowNo, sb.BinCode, sb.BinReference, sb.UserName, sb.CapacityValue,
                            sb.AccessType, sb.UserLocationType, sb.Remarks
                            FROM MST.StorageBinMaster sb
                            left join hkp.MaterialStorage ms on ms.Id = sb.StorageLocation
                            left join dbo.EmployeeInformation e on e.SystemId = sb.ResponsiblePersonId
                            WHERE " + strkey + " order by sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getResponsiblePerson()
        {
            try
            {
                var sql = @"select ei.SystemId, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, 
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section,
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
                            left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                            left join SalaryRuleMaster SRM on srm.systemid = ei.salaryrulemastersystemid
                            left join ResidenceGroup RG on RG.Id = ei.ResidenceGroupId
                            left join TransportGroup TG on TG.Id = ei.TransportGroupId          
                            where ei.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getStorageLocation()
        {
            try
            {
                //var sql = @"select ms.Id as Value, ms.UserName as Text from HKP.MaterialStorage ms 
                //            where ms.Active = '1' ORDER BY Text ASC";
                var sql = @"select m.Id, m.UserName, p.UserName as Plant, c.UserName as Company, cg.UserName as CompanyGroup from hkp.MaterialStorage m
                            left join org.Plant p on p.Id = m.PlantId
                            left join org.Company c on c.Id = p.CompanyId
                            left join org.CompanyGroup cg on cg.Id = c.CompanyGroupId
                            where m.Active = '1' ORDER BY m.UserName ASC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getResponsiblePersonId(string ResponsiblePersonId)
        {
            try
            {
                
                var sql = @"select e.EmployeeName as ResponsiblePerson from MST.StorageBinMaster sb
                            left join dbo.EmployeeInformation e on e.SystemId = sb.ResponsiblePersonId
                            where sb.ResponsiblePersonId = '"+ ResponsiblePersonId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getStorageLocationId(string StorageLocation)
        {
            try
            {

                var sql = @"select distinct m.UserName as StorageLocation from MST.StorageBinMaster sb
                            left join hkp.MaterialStorage m on m.Id = sb.StorageLocation
                            where sb.StorageLocation = '" + StorageLocation + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> Save(Dictionary<string, object> datas, string ResponsiblePersonId, string StorageLocation)
        {
            try
            {
                //Master Table - PMSMaster
                string TableName = "MST.StorageBinMaster";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                // Validate Unique User Name
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id <> '" + datas["Id"] + "' and UserName='" + datas["UserName"].ToString() + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //{
                //    throw new Exception("Same UserName is already there!!");
                //}

                
                // Validate Unique Bin Code
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id <> '" + datas["Id"] + "' and BinCode='" + datas["BinCode"].ToString() + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Bin Code is already there!!");
                }

                // Validate Unique Bin Ref
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id <> '" + datas["Id"] + "' and BinReference='" + datas["BinReference"].ToString() + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Bin Reference is already there!!");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + datas["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    datas["Id"] = "SB" + _Id;
                   // datas["StorageLocation"] = StorageLocationId;
                    datas["ResponsiblePersonId"] = ResponsiblePersonId;
                    datas["StorageLocation"] = StorageLocation;
                    AddNewRow(dsMaster.Tables[0], datas);
                }
                else
                {
                    _Id = datas["Id"].ToString();
                    datas["ResponsiblePersonId"] = ResponsiblePersonId;
                    datas["StorageLocation"] = StorageLocation;

                    EditRow(dsMaster.Tables[0].Rows[0], datas);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(string id)
        {
            try
            {
                string TableName = "MST.StorageBinMaster";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

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
            dr["AddedDate"] = DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

    }
}
