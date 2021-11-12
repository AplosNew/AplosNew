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
using bplib;

namespace Library.HumanResource.NewAttendanceProcess

{
    public class OTConfirmationProcessService
    {

        ISqlRepository _sqlRepository;
        public OTConfirmationProcessService()
        {
            _sqlRepository = new SqlRepository();
        }


        public IEnumerable<object> getDayTypes()
        {
            try
            {
                var str = @"Select * from dbo.DayType";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public object getFilters()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var str = @"Select p.Id as PlantId , p.UserName as Plant, e.Id as EntityId , e.UserName as Entity
                            from Org.Entity e 
                            left join org.Plant p on p.Id = e.PlantId where p.CompanyId = '"+identity.CompanyId+@"'
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getGridData(string Week, string FromDate, string ToDate, string OTConfirmationValue, string OTLimit, string Process, string ProcessValue, string DayStatus
 , string DSApp, Dictionary<string, string> Parameters)
        {
            try
            {
                string OTConfirm = "";
                if(clsWebLib.RetValidLen(OTConfirmationValue).ToString() != "" && clsWebLib.RetValidLen(OTConfirmationValue).ToString() != "2")
                {
                    OTConfirm = "and IsOTComfirm = " + OTConfirmationValue;
                }

                string isDayStatus = "";
                if (clsWebLib.RetValidLen(DSApp).ToString() != "" && clsWebLib.RetValidLen(DSApp).ToString() != "2") 
                {
                    isDayStatus = "and isLock =" + DSApp;
                }

                string ProcessFil = "";
                if(clsWebLib.RetValidLen(Process).ToString() != "" && clsWebLib.RetValidLen(ProcessValue).ToString() == "")
                {
                    throw new Exception("Please Enter The Process Filter Value!!");
                }

                if (clsWebLib.RetValidLen(Process).ToString() == "" && clsWebLib.RetValidLen(ProcessValue).ToString() != "")
                {
                    throw new Exception("Please Enter The Process Filter Selection!!");
                }

                if(clsWebLib.RetValidLen(Process).ToString() != "" && clsWebLib.RetValidLen(ProcessValue).ToString() != "")
                {
                    ProcessFil = " and "+Process + ProcessValue;
                }

                string DaySt = "";
                if(clsWebLib.RetValidLen(DayStatus).ToString() != "" )
                {
                    DaySt = "and a.DayStatus = '"+DayStatus+"'";
                }

                var str = @"select a.EmpSystemID,e.EmployeeCode,a.DayStatus,a.WorkDate,e.PlantId,p.UserName as Plant,
                            a.InTime,a.OutTime,a.ProcessedOT,isnull((a.ProcessedOT*dt.OTMultiplingFactor),'0') as TargetOT,
                            isnull(PreallocatedOTHr*60,'0') as PlanOT,dt.DayLimit,a.IsOTComfirm,a.StandardOT,
                            --- Week Data
                            WeekLimit= case when a.OTWeek='1' then (select dt.Week1Limit)
                            when a.OTWeek='2' then (select dt.Week2Limit)
                            when a.OTWeek='3' then (select dt.Week3Limit)
                            when a.OTWeek='4' then (select dt.Week4Limit) end,
                            a.OTYear,a.OTMonth,a.OTWeek,
                            d.UserName as Department,s.UserName as Section,ss.UserName AS SubSection,l.UserName as Designation 
                            from AttdnProcessData a left join employeeinformation e on a.EmpSystemID=e.SystemId
                            left join org.Plant p on p.Id=e.PlantId
                            left join mst.DesignationMasterLegalDesignation ddm on
                            ddm.LegalDesignationId = e.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
                            left join DayStatusPlantChild dc on dc.EmpTypeId=dm.EmployeeCategoryId
                            and dc.PlantId=e.PlantId
                            left join DayStatusHeader dh on dh.Id=dc.headerId
                            left join DayTypeWithValues dt on dt.HeaderId=dh.Id
                            left join org.Section s on s.Id=e.SectionId
                            left join ORG.SubSection ss on ss.Id=e.SubSectionId
                            left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId
                            left join org.Department d on d.Id=e.DepartmentId
                            left join PreallocatedOT pot on (pot.PlantID=e.PlantId and pot.WorkDate between '2021-11-1'
                            and '2021-11-08') and ISNULL(ExtendTheDayLimit,'')! =''
                            where a.WorkDate between '2021-11-1' and '2021-11-08' and IsOTEntitled=1
                            and dt.DayType=a.DayStatus 
                            "+OTConfirm+@" "+isDayStatus+@"
                            "+ProcessFil+@" "+DaySt+@"
                            and OTWeek="+Week+@"
                            and a.WorkDate between '"+FromDate+@"' and '"+ToDate+@"'
                            and p.Id in ("+ Parameters["PlantId"] + ") ";

                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}