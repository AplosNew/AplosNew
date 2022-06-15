using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;

namespace Library.HumanResource.Employee
{
    #region FuguaiTransactionService
    public class FuguaiTransactionService
    {
        private readonly SqlRepository _sqlRepository;
        public FuguaiTransactionService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getEntity()
        {
            try 
            {
                var sql = @"select e.Id as Value, e.UserName as Text from ORG.Entity e";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getObservedBy(string user)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                user = identity.UserId;
                //var sql = @"select s.Id as Value, s.FullName as Text  from SEC.[user] s where s.Id = '"+ user + "'";
                var sql = @"select s.Id, s.UserId, s.FullName as UserName,s.Email  from SEC.[user] s where Active = '1'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getCategory() 
        {
            try
            {
                var sql = @"select distinct z.Category as Text from hkp.ZoneMaster z";
               
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getTag(string categoryText)
        {
            try
            {
                var sql = @"select z.Id as Value, z.UserName as Text from hkp.ZoneMaster z where z.Category = '"+ categoryText + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDepartment()
        {
            try
            {
                var sql = @"select d.Id as Value, d.UserName as Text from org.Department d";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getSubCategory(string categoryText, string FuguaiId)
        {
            try
            {
                var sql = @"select distinct z.SubCategory as Text from hkp.ZoneMaster z 
                where z.Category = '"+ categoryText + "' and z.Id = '"+ FuguaiId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getResponsiblePerson(string DepartmentId)
        {
            try
            {
                var sql = @"select e.SystemId, e.EmployeeName, e.DOJ, e.EmployeeCode from dbo.EmployeeInformation e
                            left join org.Department d on d.Id = e.DepartmentId
                            where d.Id = '" + DepartmentId + "' and e.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getProcess(string EntityId)
        {
            try
            {
                var sql = @"select p.Id as Value, p.UserName as Text from hkp.Process p
                            left join hkp.EntityProcessTag ept  on ept.ProcessId = p.Id
                            left join org.Entity e on e.Id = ept.EntityId
                            where e.Id = '"+ EntityId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getMachine()
        {
            try
            {
                /*var sql = @"select mm.Id as Value, mm.UserName as Text from dbo.MachineMasterProcess msp
                            left join MST.MachineMaster mm on mm.Id = msp.MachineMasterId
                            left join hkp.Process p on p.Id = msp.ProcessId
                            where p.Id = '"+ processId + "'";*/
                var sql = @"select mm.Id as Value, mm.UserName as Text from MST.MachineMaster mm";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getMachineRef(string mmId)
        {
            try
            {
                var sql = @"select mm.Id as Value, mm.ProductionMachineQty as Text from MST.MachineMaster mm where mm.Id = '" + mmId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> Save(Dictionary<string, object> data, string ObservedById, string ResponsiblePersonId)
        {
            try
            {
                //Master Table -PMSMaster
                
                if (data.ContainsValue("ObservedById") == data.ContainsValue("ResponsiblePersonId"))
                {
                    data["TagColor"] = "White";
                }
                else
                {
                    data["TagColor"] = "Red";
                }
                string TableName = "TRN.FuguaiTransaction";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "FT" + _Id;
                    data["ObservedById"] = ObservedById;
                    data["ResponsiblePersonId"] = ResponsiblePersonId;
                    data["Date"] = DateTime.Now.ToString("dd-MMM-yyyy");
                    data["Time"] = DateTime.Now.ToString("h:mm:ss");
                    //data["ObservedById"] = identity.FullName;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["ObservedById"] = ObservedById;
                    data["ResponsiblePersonId"] = ResponsiblePersonId;
                    data["Date"] = DateTime.Now.ToString("dd-MMM-yyyy");
                    data["Time"] = DateTime.Now.ToString("h:mm:ss");

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
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
    #endregion FuguaiTransactionService

    //Fuguai Report Service
    #region FuguaiReportService
    public class FuguaiReportService
    {
        private readonly SqlRepository _sqlRepository;
        public FuguaiReportService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getByWhom()
        {
            var sql = @"Select distinct s.FullName as Text from TRN.FuguaiTransaction ft 
                        left join SEC.[user] s on s.Id = ft.ObservedById";
            //var sql = @"Select ObservedById as Text from TRN.FuguaiTransaction ft where where ft.Date between '"+ From + "' and '"+To+"'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> getResponsiblePerson()
        {
            var sql = @"select e.EmployeeName as Text from trn.FuguaiTransaction ft
                        left join dbo.EmployeeInformation e on e.SystemId = ft.ResponsiblePersonId";
           
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> getCategory()
        {
            var sql = @"select distinct ft.ZoneCategory as Text from TRN.FuguaiTransaction ft";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> getFuguai(string categoryText)
        {
            var sql = @"select distinct z.UserName as Text from hkp.ZoneMaster z  
                        left join trn.FuguaiTransaction ft on ft.ZoneMasterId = z.Id
                        where ft.ZoneCategory = '"+ categoryText + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> getFinalStatus(string categoryText)
        {
            var sql = @"select distinct ft.FinalStatus as Text from trn.FuguaiTransaction ft 
                        left join hkp.ZoneMaster z on z.Id = ft.ZoneMasterId
                        where z.Category = '" + categoryText + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> getFuguaiTransaction(string SystemId, string ObservedById)
        {
            var sql = @"select ft.Id, cast(ft.Date as Date) as Date, CONVERT(varchar(5),ft.[Date],108) Time, et.UserName as Entity, e.EmployeeName as ResponsiblePerson, s.FullName as ObservedBy, ft.ZoneCategory as Category,
                        z.UserName as Tag, ft.Detail, ft.PriorityLevel, z.SubCategory, d.UserName as Department, p.UserName as Process,
                        cast(ft.TargetDate as Date) as TargetDate, ft.StoryPoint, ft.Remarks, ft.CurrentStatus,
                        mm.UserName as Machine, mm.ProductionMachineQty as MachineReference, ft.FinalStatus, ft.TagColor
                        from TRN.FuguaiTransaction ft
                        left join dbo.EmployeeInformation e on e.SystemId = ft.ResponsiblePersonId
                        left join SEC.[user] s on s.Id = ft.ObservedById
                        left join hkp.ZoneMaster z on z.Id = ft.ZoneMasterId
                        left join org.Entity et on et.Id = ft.EntityId
                        left join org.Department d on d.Id = ft.ResponsibleDepartmentId
                        left join MST.MachineMaster mm on mm.Id = ft.MachineMasterId
                        left join hkp.Process p on p.Id = ft.ProcessId
                        where e.EmployeeName = '" + SystemId + "' and s.FullName = '" + ObservedById + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> viewByDate(string FromDate, string ToDate, string FinalStatus)
        {
            if (FinalStatus == null)
            {
                string sql = @"select ft.Id, cast(ft.Date as Date) as Date, CONVERT(varchar(5),ft.[Date],108) Time, et.UserName as Entity, e.EmployeeName as ResponsiblePerson, s.FullName as ObservedBy, ft.ZoneCategory as Category,
                        z.UserName as Tag, ft.Detail, ft.PriorityLevel, z.SubCategory, d.UserName as Department, p.UserName as Process,
                        cast(ft.TargetDate as Date) as TargetDate, ft.StoryPoint, ft.Remarks, ft.CurrentStatus,
                        mm.UserName as Machine, mm.ProductionMachineQty as MachineReference, ft.FinalStatus, ft.TagColor
                        from TRN.FuguaiTransaction ft
                        left join dbo.EmployeeInformation e on e.SystemId = ft.ResponsiblePersonId
                        left join SEC.[user] s on s.Id = ft.ObservedById
                        left join hkp.ZoneMaster z on z.Id = ft.ZoneMasterId
                        left join org.Entity et on et.Id = ft.EntityId
                        left join org.Department d on d.Id = ft.ResponsibleDepartmentId
                        left join MST.MachineMaster mm on mm.Id = ft.MachineMasterId
                        left join hkp.Process p on p.Id = ft.ProcessId
                        where ft.Date between'" + FromDate + "' and '" + ToDate + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            else
            {
                string sql = @"select ft.Id, cast(ft.Date as Date) as Date, CONVERT(varchar(5),ft.[Date],108) Time, et.UserName as Entity, e.EmployeeName as ResponsiblePerson, s.FullName as ObservedBy, ft.ZoneCategory as Category,
                        z.UserName as Tag, ft.Detail, ft.PriorityLevel, z.SubCategory, d.UserName as Department, p.UserName as Process,
                        cast(ft.TargetDate as Date) as TargetDate, ft.StoryPoint, ft.Remarks, ft.CurrentStatus,
                        mm.UserName as Machine, mm.ProductionMachineQty as MachineReference, ft.FinalStatus, ft.TagColor
                        from TRN.FuguaiTransaction ft
                        left join dbo.EmployeeInformation e on e.SystemId = ft.ResponsiblePersonId
                        left join SEC.[user] s on s.Id = ft.ObservedById
                        left join hkp.ZoneMaster z on z.Id = ft.ZoneMasterId
                        left join org.Entity et on et.Id = ft.EntityId
                        left join org.Department d on d.Id = ft.ResponsibleDepartmentId
                        left join MST.MachineMaster mm on mm.Id = ft.MachineMasterId
                        left join hkp.Process p on p.Id = ft.ProcessId
                        where ft.Date between'" + FromDate + "' and '" + ToDate + "' and ft.FinalStatus = '" + FinalStatus + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
        }

        public DataTable GetReport(string FromDate, string ToDate, string FinalStatus)
        {
            try
            {
                if (FinalStatus == null)
                {
                    string sql = @"select ft.Id, cast(ft.Date as Date) as Date, CONVERT(varchar(5),ft.[Date],108) Time, et.UserName as Entity, e.EmployeeName as ResponsiblePerson, s.FullName as ObservedBy, ft.ZoneCategory as Category,
                        z.UserName as Tag, ft.Detail, ft.PriorityLevel, z.SubCategory, d.UserName as Department, p.UserName as Process,
                        cast(ft.TargetDate as Date) as TargetDate, ft.StoryPoint, ft.Remarks, ft.CurrentStatus,
                        mm.UserName as Machine, mm.ProductionMachineQty as MachineReference, ft.FinalStatus, ft.TagColor
                        from TRN.FuguaiTransaction ft
                        left join dbo.EmployeeInformation e on e.SystemId = ft.ResponsiblePersonId
                        left join SEC.[user] s on s.Id = ft.ObservedById
                        left join hkp.ZoneMaster z on z.Id = ft.ZoneMasterId
                        left join org.Entity et on et.Id = ft.EntityId
                        left join org.Department d on d.Id = ft.ResponsibleDepartmentId
                        left join MST.MachineMaster mm on mm.Id = ft.MachineMasterId
                        left join hkp.Process p on p.Id = ft.ProcessId
                        where ft.Date >= '" + FromDate + "' and ft.Date <= '" + ToDate + "'";
                    return _sqlRepository.GetDataTable(sql);
                }
                else
                {


                    string sql = @"select ft.Id, cast(ft.Date as Date) as Date, CONVERT(varchar(5),ft.[Date],108) Time, et.UserName as Entity, e.EmployeeName as ResponsiblePerson, s.FullName as ObservedBy, ft.ZoneCategory as Category,
                        z.UserName as Tag, ft.Detail, ft.PriorityLevel, z.SubCategory, d.UserName as Department, p.UserName as Process,
                        cast(ft.TargetDate as Date) as TargetDate, ft.StoryPoint, ft.Remarks, ft.CurrentStatus,
                        mm.UserName as Machine, mm.ProductionMachineQty as MachineReference, ft.FinalStatus, ft.TagColor
                        from TRN.FuguaiTransaction ft
                        left join dbo.EmployeeInformation e on e.SystemId = ft.ResponsiblePersonId
                        left join SEC.[user] s on s.Id = ft.ObservedById
                        left join hkp.ZoneMaster z on z.Id = ft.ZoneMasterId
                        left join org.Entity et on et.Id = ft.EntityId
                        left join org.Department d on d.Id = ft.ResponsibleDepartmentId
                        left join MST.MachineMaster mm on mm.Id = ft.MachineMasterId
                        left join hkp.Process p on p.Id = ft.ProcessId
                        where ft.Date between'" + FromDate + "' and '" + ToDate + "' and ft.FinalStatus = '" + FinalStatus + "'";
                    return _sqlRepository.GetDataTable(sql);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    #endregion FuguaiReportService
}
