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
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;


#endregion Using

namespace Aplos.Areas.EmployeeServices.Controllers
{
    public class EmployeeServiceBookingController : BaseController
    {
        string TableName = "dbo.EmpServiceData";



        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public EmployeeServiceBookingController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor




        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + "  "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getservices()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,Service AS Text FROM dbo.EmpServiceType order by Service"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getcategory(string serviceid, string CatId)
        {
            if (CatId == "null")
            {
                return Json(_sqlRepository.GetDataCollection("select Id as Value,Category as Text from dbo.EmpServiceCategory where EmpServiceTypeId='" + serviceid + "' order by Category "), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(_sqlRepository.GetDataCollection("select Id as Value,Category as Text from dbo.EmpServiceCategory where EmpServiceTypeId='" + serviceid + "' and Id='" + CatId + "' order by Category "), JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpGet]
        public JsonResult getuom(string serviceid)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct uom.Id as Value,uom.UserName as Text from SCS.UnitOfMeasurement uom left join dbo.EmpServiceType est on uom.Id=est.UOMId left join dbo.EmpServiceCategory esc on est.Id=esc.EmpServiceTypeId left join dbo.EmpServiceData esd on esc.Id=esd.EmployeeServiceCategoryId where est.Id='" + serviceid + "'  "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public JsonResult getform(string CategoryId, string ServicesId)
        {
            return Json(_sqlRepository.GetDataCollection("select est.Form from dbo.EmpServiceType est left join dbo.EmpServiceCategory esc on est.Id=esc.EmpServiceTypeId where esc.Id='" + CategoryId + "' and esc.EmpServiceTypeId='" + ServicesId + "' "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getshift()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_sqlRepository.GetDataCollection("SELECT SystemID as Value,UserName AS Text FROM dbo.ShiftDefination where GroupID='" + identity.CompanyGroupId + @"' order by UserName"), JsonRequestBehavior.AllowGet);
            //     return Json(_sqlRepository.GetDataCollection("SELECT SystemID as Value,UserName AS Text FROM dbo.ShiftDefination where PlantID='"+ identity.PlantId + @"' order by UserName"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSelectedShift(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT sd.SystemID as Value,sd.UserName AS Text FROM dbo.ShiftDefination sd 
                           left join dbo.EmpServiceData esd on sd.SystemID=esd.ShiftId where esd.Id='" + Id + @"' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.EmpServiceData where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value , string FromDate , string ToDate)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string ddDates = "1=1";
            if (FromDate != "" && ToDate != "")
            {
                ddDates =" Date between '" + FromDate + @"' and '" + ToDate + @"'";
            }


            //string sql = @"select distinct esd.*, FORMAT(esd.Date,'dd-MMM-yyyy') as EmpServiceDate,CONVERT(varchar(5),esd.[Time],108)[GetTime],ei.SystemId,ei.EmployeeCode
            //                                                        ,ei.EmployeeName as EmpName,ei.EmployeeStatus,sd.UserName as ShiftName,esc.Category,est.Form,est.Service as ServiceName
            //                                                        ,est.Id as EmployeeServicesId,uom.UserName as UOM,uom.Id as UOMId 
            //                                                         from dbo.EmpServiceData esd
            //                                                        left join dbo.EmployeeInformation ei on ei.SystemId=esd.EmployeeId
            //					left join dbo.ShiftDefination sd on sd.SystemID=esd.ShiftId
            //					left join dbo.EmpServiceCategory esc on esc.Id=esd.EmployeeServiceCategoryId
            //					left join dbo.EmpServiceType est on est.Id=esc.EmpServiceTypeId
            //					left join SCS.UnitOfMeasurement uom on uom.Id=est.UOMId
            //				    WHERE " + strkey + " and sd.GroupID='" + identity.CompanyGroupId + @"' and Date between '"+FromDate+@"' and '"+ToDate+@"'
            //                                                        order by Date desc ";

            //string sql = @"Select distinct  esd.Date,FORMAT(esd.Date,'dd-MMM-yyyy') as EmpServiceDate,CONVERT(varchar(5),esd.[Time],108)[GetTime], ShiftId , EmployeeServiceCategoryId , es.Id as  EmployeeServicesId,
            //                es.Service as ServiceName, sd.UserName as ShiftName, ec.Category
            //                from dbo.EmpServiceData esd
            //                left join dbo.EmpServiceCategory  ec on ec.Id = esd.EmployeeServiceCategoryId
            //                left join dbo.EmpServiceType es on es.Id = ec.EmpServiceTypeId
            //                left join dbo.ShiftDefination sd on sd.SystemID = esd.ShiftId
            //                WHERE " + strkey + " and sd.GroupID='" + identity.CompanyGroupId + @"' and "+ddDates+@"
            //                order by Date desc ";

            string sql = @"Select EMP.EmployeeCode, EMP.EmployeeName, UN.UserName Entity, DP.UserName Department, LDSG.UserName 'Legal Designation', ShiftId
, EmployeeServiceCategoryId , es.Id as  EmployeeServicesId,  es.Service as ServiceName, ec.Category, esd.Date,FORMAT(esd.Date,'dd-MMM-yyyy') as EmpServiceDate
,CONVERT(varchar(5),esd.[Time],108)[GetTime], esd.Quantity, esd.AddedBy, sd.UserName as ShiftName, ESD.Amount							
                            from dbo.EmpServiceData esd
                            left join dbo.EmpServiceCategory  ec on ec.Id = esd.EmployeeServiceCategoryId
                            left join dbo.EmpServiceType es on es.Id = ec.EmpServiceTypeId
                            left join dbo.ShiftDefination sd on sd.SystemID = esd.ShiftId
							left join EmployeeInformation EMP ON EMP.SystemId = esd.EmployeeId
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=DM.DesignationGroupId
 WHERE " + strkey + " and sd.GroupID='" + identity.CompanyGroupId + @"' and " + ddDates + @"
 order by Date desc";
            var jsondata = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;

            return jsondata;
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmpServiceData", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                if (data["IsProcessed"].ToString() == "False")
                {
                    DataSet dsMaster;


                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    if (clsStaticInfo.nullrecorder(data["EmployeeId"].ToString()) == "")
                        throw new Exception("Select employee");

                    //con.OpenDataSetThroughAdapter("select * from " + TableName + " where Service='" + data["Service"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                    //if (dsMaster.Tables[0].Rows.Count > 0)
                    //    throw new Exception("Same Service already exists!!!");


                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                    string _Id = "";

                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0 && data["Id"] == null)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        //data["Id"] = "ESD" + _Id;
                        data["Id"] = "ESD" + GetPK();
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data update

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);

                    return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
                }
                else
                {
                    throw new Exception("Now You cannot Edit And Delete");
                }

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

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
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
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
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpPost, Authorize]
        public JsonResult getgriddatatoshow(string ServicesId, string CategoryId, string Date , string Time, string EmployeeCode)
        {
            string sql = @"select top 100 * from (select distinct esd.*,FORMAT(esd.Date,'dd-MMM-yyyy') as EmpServiceDate,CONVERT(varchar(5),esd.[Time],108)[GetGridTime],ei.SystemId,ei.EmployeeCode,ei.EmployeeName as EmpName,ei.EmployeeStatus,sd.UserName as Shift,esc.Category,est.Form,est.Service,est.Id as EmployeeServicesId,uom.UserName as Text,uom.Id as UOMId from dbo.EmpServiceData esd
                                                                    left join dbo.EmployeeInformation ei on ei.SystemId=esd.EmployeeId
																	left join dbo.ShiftDefination sd on sd.SystemID=esd.ShiftId
																	left join dbo.EmpServiceCategory esc on esc.Id=esd.EmployeeServiceCategoryId
																	left join dbo.EmpServiceType est on est.Id=esc.EmpServiceTypeId
																	left join SCS.UnitOfMeasurement uom on uom.Id=est.UOMId
																	where EI.EmployeeCode = '"+ EmployeeCode + "' and Date='" + Date + @"' 
                                                                    --and EmployeeServiceCategoryId='" + CategoryId + "' and est.Id='" + ServicesId + "' and CONVERT(varchar(5),esd.[Time],108) = '"+Time+@"'
                                                                    ) AS TEMP order by Time desc";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult getduplicatedata(string ServicesId, string CategoryId, string Date, string EmployeeId)
        {
            string sql = @"select top 100 * from (select distinct esd.*,ei.SystemId,ei.EmployeeCode,ei.EmployeeName as EmpName,ei.EmployeeStatus,sd.UserName as Shift,esc.Category,est.Form,est.Service,est.Id as EmployeeServicesId,uom.UserName as Text,uom.Id as UOMId from dbo.EmpServiceData esd
                                                                    left join dbo.EmployeeInformation ei on ei.SystemId=esd.EmployeeId
																	left join dbo.ShiftDefination sd on sd.SystemID=esd.ShiftId
																	left join dbo.EmpServiceCategory esc on esc.Id=esd.EmployeeServiceCategoryId
																	left join dbo.EmpServiceType est on est.Id=esc.EmpServiceTypeId
																	left join SCS.UnitOfMeasurement uom on uom.Id=est.UOMId
																	where Date='" + Date + "' and EmployeeServiceCategoryId='" + CategoryId + "' and est.Id='" + ServicesId + "' and esd.EmployeeId='" + EmployeeId + "') AS TEMP order by Time desc";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DelQuantity(string Id)
        {
            try
            {
                string sql = @" delete from dbo.EmpServiceData where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Quantity Form deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult DelAmount(string Id)
        {
            try
            {
                string sql = @" delete from dbo.EmpServiceData where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Amount Form deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult DelReading(string Id)
        {
            try
            {
                string sql = @" delete from dbo.EmpServiceData where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Reading Form deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Employee field
        [HttpPost, Authorize]
        public ActionResult LoadAllEmpDetailsForSelection(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"
                        SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            EMP.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' --and emp.CompanyId='" + identity.CompanyId + @"' 
                              and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                   AND isnull(Emp.SystemID,'') not in (select isnull(EmployeeId,'') from dbo.EmpServiceData where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

    }

}