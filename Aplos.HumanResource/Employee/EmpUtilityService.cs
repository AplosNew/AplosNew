using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.HumanResource.NewAttendanceProcess;

namespace Library.Service.EmployeeServices
{
    public class EmpUtilityService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public EmpUtilityService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        public string Create(IEnumerable<PhysicalVerifyModel> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.OTfromApp";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                int i = 0;
                foreach (PhysicalVerifyModel item in DataToSave)
                {
                    con.OpenDataSetThroughAdapter("select * from dbo.OTfromApp where EmpSystemId='" + item.EmpSystemId + "'and WorkDate='" + item.WorkDate + "'", out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);

                        dr["Id"] = "OT" + _Id;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["Remarks"] = item.Remarks;
                        dr["OThour"] = item.OThour;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["WorkDate"] = item.WorkDate;
                        dr["IsConfirmed"] = false;
                      
                        dsMaster.Tables[0].Rows.Add(dr);

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);

                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["Remarks"] = item.Remarks;
                        dr["OThour"] = item.OThour;
                        dr["IsConfirmed"] = false;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["WorkDate"] = item.WorkDate;
                     
                        dr.EndEdit();
                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                    }
                    i++;
                }
                return i.ToString();


            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        #region CREATE By Nitesh
        public string CreateDetentionLog(IEnumerable<CreateDetentionList> DataToSave)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "TRN.DetentionLog";
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                int i = 0;

                foreach (CreateDetentionList item in DataToSave)
                {
                    con.OpenDataSetThroughAdapter("select * from TRN.DetentionLog where Id='" + item.Id + "'", out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);

                        dr["Id"] = "DL" + _Id;
                        dr["WorkCenterId"] = item.WorkCenterId;
                        dr["DetentionTypeId"] = item.DetentionTypeId;
                        dr["MachineMasterId"] = item.MachineMasterId;
                        dr["IssueByNo"] = item.IssueByNo;
                        dr["Remarks"] = item.Remarks;
                        //dr["AddedBy"] = By;
                        dr["isClose"] = false;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        //dr["AddedFromIP"] = Ip;


                        dsMaster.Tables[0].Rows.Add(dr);

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);

                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["WorkCenterId"] = item.WorkCenterId;
                        dr["DetentionTypeId"] = item.DetentionTypeId;
                        dr["MachineMasterId"] = item.MachineMasterId;
                        dr["IssueByNo"] = item.IssueByNo;
                        dr["Remarks"] = item.Remarks;
                        //dr["UpdatedBy"] = updatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        //dr["UpdatedFromIP"] = updatedFrom;
                        dr.EndEdit();
                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                    }
                    i++;
                }
                return i.ToString();


            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion CREATE By Nitesh

        public string CreateOT(IEnumerable<PhysicalVerifyModel> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.OTfromApp";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<PhysicalVerifyModel> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.OTfromApp where EmpSystemId='" + items[0].EmpSystemId + "' and WorkDate='" + items[0].WorkDate + "'", out dsMaster, false, "1");

                foreach (PhysicalVerifyModel item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);

                        dr["Id"] = "OT" + _Id;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["Remarks"] = item.Remarks;
                        dr["IsConfirmed"] = false;
                        dr["OThour"] = item.OThour;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["WorkDate"] = item.WorkDate;

                        dsMaster.Tables[0].Rows.Add(dr);


                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["Remarks"] = item.Remarks;
                        dr["OThour"] = item.OThour;
                        dr["IsConfirmed"] = false;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["WorkDate"] = item.WorkDate;

                        dr.EndEdit();
                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                return MasterId;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public IEnumerable<object> GetEmpInfo(string Code)
        {
            try
            {
                var Sql = @"select emp.SystemId as EmpId,emp.EmployeeName as Name,apd.WorkDate,emp.EmployeeCode,emp.EmploymentType,mb.Code AS BudgetCode,mb.Id as BudgetCodeId,
                        dx.StandardName as Department,emp.EmployeeGroupSystemID,dx.Id as DepartmentId,CONVERT(VARCHAR(12),emp.DOJ,107) as DOJ,l.Id as LegalDesignationId,l.StandardName as LegalDesignation,
                        d.StandardName as Designation,
                               d.Id as DesignationId , sd.ShiftDefinationName Shift , SC.StandardName Section,SBC.StandardName SubSection
							   ,LL.UserName Line
                                from dbo.AttdnProcessData apd
								 left join dbo.EmployeeInformation emp on emp.SystemId = apd.EmpSystemID 
								left join hkp.Designation d on d.Id=emp.DesignationSystemID
								left join org.Department dx on dx.Id=emp.DepartmentId
					LEFT JOIN MST.ManpowerBudget MB ON apd.BudgetId = MB.Id
					left join hkp.LegalDesignation l on l.Id=emp.LegalDesignationId
					left join ORG.Section SC on SC.Id = EMP.SectionId
left join ORG.SubSection SBC on SBC.Id = EMP.SubSectionId
left join ShiftDefination sd on sd.systemid = MB.shiftdefinationid
left join ORG.Position POS on POS.Id = MB.PositionId
left join org.Department DP on DP.Id= POS.DepartmentId
LEFT JOIN hkp.Designation DSG on DSG.id = POS.DesignationId
left join mst.DesignationMaster DM on DM.DesignationId = POS.DesignationId
left join HKP.EmployeeCategory EC on EC.Id = Dm.EmployeeCategoryId
left join org.Line LL on LL.Id	= MB.LineId
                                where emp.EmployeeStatus = 'Active' and EmployeeCode='" + Code + "' and apd.WorkDate = convert(date, getdate())";

                       return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmpCode(string GpId, string CompId, string PlantId)
        {
            try
            {
                var sql = "SELECT EmployeeCode as Code,SystemID as Value,EmployeeName as Text,CellPhnNo FROM dbo.EmployeeInformation " +
                    "where EmployeeStatus = 'Active' and GroupID='" + GpId + "' and CompanyId='" + CompId + "' and PlantId='" + PlantId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetBudgetData(string GpId, string CompId, string PlantId, string Code)
        {
            try
            {
                var sql = @"SELECT PMB.Id as BudgetId, PMB.Code, PMB.EntityId, ERD.UserName AS EntityName, PMB.PositionId, PRD.UserName AS PositionName,PRD.Code PositionCode
                , ERD.Code EntityCode, PMB.IsOTEntitled, PMB.PayrollGroupId, PMB.WorkGroupId, PRD.IsDirect , ERD.PlantId
                , (SELECT UserName FROM[ORG].[Plant] WHERE Id = ERD.PlantId) AS[Plant], ERD.DivisionId
                , (SELECT UserName FROM[ORG].[Division] WHERE Id = ERD.DivisionId) AS[Division], ERD.SubDivisionId
                , (SELECT UserName FROM[ORG].[SubDivision] WHERE Id = ERD.SubDivisionId) AS[SubDivision],ERD.UnitId
                , (SELECT UserName FROM[ORG].[Unit] WHERE Id = ERD.UnitId) AS[Unit], PRD.DepartmentId
                , (SELECT UserName FROM[ORG].[Department] WHERE Id = PRD.DepartmentId) AS[Department], PRD.SectionId
                , (SELECT UserName FROM[ORG].[Section] WHERE Id = PRD.SectionId) AS[Section], PRD.SubSectionId
                , (SELECT UserName FROM[ORG].[SubSection] WHERE Id = PRD.SubSectionId) AS[Subsection], PMB.LineId
                , (SELECT UserName FROM[ORG].[Line] WHERE Id = PMB.LineId) AS[Line] , PMB.ShiftDefinationId
                , (SELECT UserName FROM[dbo].[ShiftDefination] WHERE SystemID = PMB.ShiftDefinationId) AS[ShiftDefination] , PRD.DesignationId
                , (SELECT UserName FROM[HKP].[Designation] WHERE Id = PRD.DesignationId) AS[Designation]
                FROM[MST].[ManpowerBudget] AS PMB INNER JOIN ORG.Entity AS ERD ON PMB.EntityId = ERD.Id
                INNER JOIN ORG.Position AS PRD ON PMB.PositionId = PRD.Id
                WHERE PMB.Active = 1 AND PMB.Archive = 0 AND ERD.CompanyGroupId = '" + GpId + "' AND ERD.CompanyId = '" + CompId + "' AND ERD.PlantId = '" + PlantId + "'AND ERD.Active = 1 AND ERD.Archive = 0 AND PMB.Code='" + Code + "'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SaveDatax(string EmpId, string BudgetId, string LId, string GId, string By, string Ip, string Id)
        {
            try
            {
                DataSet dsRef;
                string TableName = "dbo.EmployeeBudgetCodeHistory";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Id + "'", out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    string _Id = "";

                    DataRow dr = dsRef.Tables[0].NewRow();
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    dr["Id"] = _Id;
                    dr["EmpSystemId"] = EmpId;
                    dr["BudgetId"] = BudgetId;
                    dr["GivenDesignationId"] = GId;
                    dr["LegalDesignationId"] = LId;
                    dr["AddedBy"] = By;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = Ip;

                    dsRef.Tables[0].Rows.Add(dr);
                                       
                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);
                
                string MasterId = dsRef.Tables[0].Rows[0]["Id"].ToString();
                return MasterId;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public string SaveData(List<AttdnManualData> DataToSave)
        {
            try
            {
                List<AttdnManualData> items = DataToSave.ToList();

                DataSet dsRef;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from dbo.AttndManualDataFromApp where EmpSystemID='" + items[0].EmpSystemID + "' and WorkDate='" + items[0].WorkDate + "'";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {

                    DataRow dr = dsRef.Tables[0].NewRow();

                    dr["GroupID"] = items[0].GroupID;
                    dr["EmpSystemID"] = items[0].EmpSystemID;
                    dr["WorkDate"] = items[0].WorkDate;
                    dr["DayStatus"] = items[0].DayStatus;
                    dr["ShiftSystemId"] = items[0].ShiftSystemId;

                    if (items[0].InTime != null)
                    {
                        dr["InTime"] = items[0].InTime;
                    }
                    else
                    {
                        dr["InTime"] = DBNull.Value;
                    }
                    if (items[0].OutTime != null)
                    {
                        dr["OutTime"] = items[0].OutTime;
                    }
                    else
                    {
                        dr["OutTime"] = DBNull.Value;
                    }

                    dr["AddedBy"] = items[0].AddedBy;
                    dr["DateAdded"] = DateTime.Now.ToString();


                    dsRef.Tables[0].Rows.Add(dr);

                }
                else
                {

                    DataRow dr = dsRef.Tables[0].Rows[0];
                    dr.BeginEdit();

                    dr["GroupID"] = items[0].GroupID;
                    dr["DayStatus"] = items[0].DayStatus;
                    dr["ShiftSystemId"] = items[0].ShiftSystemId;

                    if (items[0].InTime != null)
                    {
                        dr["InTime"] = items[0].InTime;
                    }
                    else
                    {
                        dr["InTime"] = DBNull.Value;
                    }
                    if (items[0].OutTime != null)
                    {
                        dr["OutTime"] = items[0].OutTime;
                    }
                    else
                    {
                        dr["OutTime"] = DBNull.Value;
                    }


                    dr["UpdatedBy"] = items[0].AddedBy;
                    dr["DateUpdated"] = DateTime.Now.ToString();

                    dr.EndEdit();

                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);

                return "true";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            finally
            {

            }
        }

        public string SaveManualOT(IEnumerable<AttendanceProcessNewProcess> DataToSave)
        {
            try
            {
                int i = 0;
                DataSet ManualOTData;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
               
                List<AttendanceProcessNewProcess> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.AttdnProcessData where RowId='"+items[0].RowId+"'", out ManualOTData, false, "1");

                if (ManualOTData.Tables[0].Rows.Count > 0)
                {
                    ManualOTData.Tables[0].Rows[0].BeginEdit();
                    ManualOTData.Tables[0].Rows[0]["ManualOt"] = items[0].ManualOt;
                    ManualOTData.Tables[0].Rows[0]["IsLock"] = false;
                    ManualOTData.Tables[0].Rows[0]["LockedBy"] = DBNull.Value;
                    ManualOTData.Tables[0].Rows[0]["LockedDate"] = DBNull.Value;
                    ManualOTData.Tables[0].Rows[0]["ManualByWhom"] = items[0].AddedBy;
                    ManualOTData.Tables[0].Rows[0]["ManualEntryTime"] = DateTime.Now;
                    ManualOTData.Tables[0].Rows[0]["ManualFlag"] = true;
                    ManualOTData.Tables[0].Rows[0]["OTComfirmBy"] = DBNull.Value;
                    ManualOTData.Tables[0].Rows[0]["DateOTComfirm"] = DBNull.Value;
                    ManualOTData.Tables[0].Rows[0]["IsOTComfirm"] = false;

                    #region OT Columns 
                    ManualOTData.Tables[0].Rows[0]["TargetOT"] = DBNull.Value;
                    ManualOTData.Tables[0].Rows[0]["PlanOT"] = DBNull.Value;
                    ManualOTData.Tables[0].Rows[0]["AppliedOTLimit"] = DBNull.Value;
                    ManualOTData.Tables[0].Rows[0]["AllowedOTLimit"] = DBNull.Value;
                    ManualOTData.Tables[0].Rows[0]["StandardOT"] = DBNull.Value;
                    ManualOTData.Tables[0].Rows[0]["AdditionalOt"] = DBNull.Value;
                    #endregion

                    ManualOTData.Tables[0].Rows[0].EndEdit();
                    i = 1;

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(ManualOTData);
                if (i == 1)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }



        public string Createx(IEnumerable<Updatebudgetcode> DataToSave, string Empsystemid) 
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.AttdnProcessData";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<Updatebudgetcode> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where WorkDate = convert(date, getdate())  and EmpSystemID ='" + Empsystemid + "'", out dsMaster, false, "1");


                foreach (Updatebudgetcode item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["BudgetId"] = item.BudgetId;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["DateUpdated"] = System.DateTime.Now.ToString();

                        dr.EndEdit();

                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                // string MasterId = SaveDatax(items[0].EmpSystemID, "");

               // string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
              //  return MasterId;
                return "true";                     

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        // For Test 

        public string Creatextest(IEnumerable<EmployeeInformationViewModel> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.EmployeeInformation";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<EmployeeInformationViewModel> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where SystemId='" + items[0].SystemId + "'", out dsMaster, false, "1");


                foreach (EmployeeInformationViewModel item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["BudgetCode"] = item.BudgetCode;
                        dr["PositionID"] = item.PositionID;
                        dr["DepartmentId"] = item.DepartmentId;
                        dr["DivisionId"] = item.DivisionId;
                        dr["SectionId"] = item.SectionId;
                        dr["SubSectionId"] = item.SubSectionId;
                        dr["UnitId"] = item.UnitId;
                        if (item.LineId != null)
                        {
                            dr["LineId"] = item.LineId;
                        }
                        dr["DesignationSystemID"] = item.DesignationSystemID;
                        dr["DesignationGroupId"] = item.DesignationGroupId;
                        if (item.EmployeeGroupSystemID != null)
                        {
                            dr["EmployeeGroupSystemID"] = item.EmployeeGroupSystemID;
                        }
                        dr["EmploymentType"] = item.EmploymentType;
                        dr["SubDivisionId"] = item.SubDivisionId;
                        dr["AddedBy"] = item.AddedBy;
                        dr["DateAdded"] = System.DateTime.Now.ToString();
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["DateUpdated"] = System.DateTime.Now.ToString();

                        dr.EndEdit();

                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                 string MasterId = SaveDatax(items[0].SystemId, items[0].prevbudgetCode, items[0].LegalDesignationId, items[0].prevdesgnId, items[0].AddedBy, items[0].AddedFromIP, "");
                return MasterId;
               // return "true"; //Stopping It for Now 


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> GetBudgetCode(string GpId, string CompId,string PlntId)
        {
            try
            {
                var sql = @"SELECT distinct M.Code FROM MST.ManpowerBudget M
                        join org.Entity en on en.Id = M.EntityId
                        where m.Active='1' AND M.Archive='0' and M.CompanyId='"+CompId+"' AND M.CompanyGroupId='"+GpId+"' and en.PlantId = '"+PlntId+"'";
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> GetDesignationGroup(string DesgId)
        {
            try
            {
                var sql = @"SELECT * FROM mst.DesignationMaster WHERE DesignationId='" + DesgId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string UpdateInLive(string EmpId,string WkDate,string Shift)
        {
            try
            {
                DataSet dsRef;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from EmpDateWiseShiftAssign where WorkDate='" + WkDate + "' and EmpSystemID='" + EmpId + "'";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                {

                    DataRow dr = dsRef.Tables[0].Rows[0];
                    dr.BeginEdit();

                    dr["ShiftSystemID"] = Shift;
                    dr["ManualShiftId"] = Shift;
                    dr["UpdatedBy"] = "FromApp";
                    dr["DateUpdated"] = DateTime.Now.ToString();

                    dr.EndEdit();
                }
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);

                string MasterId = dsRef.Tables[0].Rows[0]["UpdatedBy"].ToString();
                return MasterId;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public IEnumerable<object> GetROEmp(string BudgetId,string FromDate,string ToDate)
        {
            try
            {
               
                var sql = @"select distinct P.RowId,d.Category,format(p.WorkDate,'yyyy-MM-dd') as WorkDate,
                emp.SystemId as EmpId,p.PlantId,emp.EmployeeCode,emp.EmployeeName,
                p.DayStatus,dp.UserName as Department,s.UserName as SubSection,
                ss.UserName as Section,p.OTHr,mb.ROBudgetCode,
                sh.UserName as Shift,sh.SystemID as ShiftId,p.InTime,p.OutTime,
				p.ShiftInTime,p.ShiftOutTime,
                p.IsLock from
				dbo.AttdnProcessData p left join EmployeeInformation emp on 
				p.EmpSystemID =emp.SystemId
                left join DayType d on d.DayType=p.DayStatus			    
				left join mst.ManpowerBudget mb on emp.BudgetCode=mb.Id
                left join org.Department dp on dp.Id=emp.DepartmentId
                left join Org.SubSection s on s.Id=emp.SubSectionId
                left join org.section ss on ss.Id=emp.SectionId			    
			    left join dbo.ShiftDefination sh on sh.SystemID=p.ShiftSystemID
				where emp.EmployeeStatus = 'Active' and ROBudgetCode='" + BudgetId+@"' and ISNULL(p.DayStatus,'')!='' and
				p.WorkDate between '"+FromDate+@"' and '"+ToDate+@"'
                order by emp.SystemId,WorkDate";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetOTEmpCode(string GpId, string CompId, string PlantId, string Date)
        {
            try
            {
                var sql = @"SELECT distinct EmployeeCode as Code,SystemID as Value FROM dbo.EmployeeInformation emp
                join hkp.LegalDesignation d on d.Id=emp.LegalDesignationId
                join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId =d.Id
                 join scs.DesignationMasterConfiguration dm on dm.DesignationMasterId=dd.DesignationMasterId
                and dm.PlantId=emp.PlantId
                where emp.EmployeeStatus = 'Active' and emp.GroupID='" + GpId + "'and dm.IsOTEntitled='1' and emp.CompanyId='" + CompId + "' and " +
                "emp.PlantId='" + PlantId + "' and emp.DOJ<='" + Date + "' and (emp.dos is null or emp.dos>='" + Date + "')";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPREmp(string BudgetId, string FromDate,string ToDate)
        {
            try
            {

                var sql = @"select distinct P.RowId,d.Category,format(p.WorkDate,'yyyy-MM-dd') as WorkDate,
                emp.SystemId as EmpId,p.PlantId,emp.EmployeeCode,emp.EmployeeName,
                p.DayStatus,dp.UserName as Department,s.UserName as SubSection,
                ss.UserName as Section,p.OTHr,mb.PRBudgetCode,
                sh.UserName as Shift,sh.SystemID as ShiftId,p.InTime,p.OutTime,
				p.ShiftInTime,p.ShiftOutTime,
                p.IsLock from
				dbo.AttdnProcessData p left join EmployeeInformation emp on 
				p.EmpSystemID =emp.SystemId
                left join DayType d on d.DayType=p.DayStatus			    
				left join mst.ManpowerBudget mb on emp.BudgetCode=mb.Id
                left join org.Department dp on dp.Id=emp.DepartmentId
                left join Org.SubSection s on s.Id=emp.SubSectionId
                left join org.section ss on ss.Id=emp.SectionId			    
			    left join dbo.ShiftDefination sh on sh.SystemID=p.ShiftSystemID
				where emp.EmployeeStatus = 'Active' and PRBudgetCode='" + BudgetId+ @"' and ISNULL(p.DayStatus,'')!='' and
				p.WorkDate between '"+FromDate+"' and '"+ToDate+"' order by emp.SystemId,WorkDate";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSeniorBudgetCode(string EmpId)
        {
            try
            {
                var Sql = @"select Id as BudgetId,Code as BudgetCode from mst.ManpowerBudget mb
                left join EmployeeInformation emp on 
                emp.BudgetCode=mb.Id where emp.EmployeeStatus = 'Active' and emp.SystemId='" + EmpId + "'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public IEnumerable<object> GetDepartment()
        {
            try
            {
                var sql = @"select Id as Value,UserName as Text from Org.Department order by Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public IEnumerable<object> GetSection()
        {
            try
            {
                var sql = @"select Id as Value,UserName as Text from org.Section order by Sequence";    return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public IEnumerable<object> GetSubSection()
        {
            try
            {
                var sql = @"select Id as Value,UserName as Text from org.SubSection order by Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public IEnumerable<object> GetAttndLock(string PlantId, string Date)
        {
            try
            {
                var sql = @"select * from PlantWiseAttendanceLock where PlantId='"+PlantId+"' and LockedDate='"+Date+"' and IsActive='1' ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAttndUnLock(string EmpId, string Date)
        {
            try
            {
                var sql = @"select * from ExceptionEmployeeAttendanceUnlock where 
                    EmpSystemId='"+EmpId+"'and WorkDate='"+Date+"' and IsActive='1' ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetUpdOTEmpCode(string GpId, string CompId, string PlantId, string Date, string DepId, string SId, string SsId)
        {
            try
            {
                var sql = @"SELECT distinct EmployeeCode as Code,SystemID as Value FROM dbo.EmployeeInformation emp
                join hkp.LegalDesignation d on d.Id=emp.LegalDesignationId
                join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId =d.Id
				join mst.ManpowerBudget mb on mb.Id=emp.BudgetCode
				join org.Position p on p.Id=mb.PositionId
                join org.Department dp on dp.Id=p.DepartmentId
                join org.Section s on s.Id=p.SectionId
                join org.SubSection ss on ss.Id=p.SubSectionId
                join scs.DesignationMasterConfiguration dm on dm.DesignationMasterId=dd.DesignationMasterId
                and dm.PlantId=emp.PlantId
                where emp.EmployeeStatus = 'Active' and emp.GroupID='" + GpId+"'and dm.IsOTEntitled='1' and emp.CompanyId='"+CompId+"' and" +
                " emp.PlantId='"+PlantId+"' and dp.Id='"+DepId+"' and s.Id='"+SId+"' and ss.Id='"+SsId+"'" +
                " and emp.DOJ<='"+Date+"' and (emp.dos is null or emp.dos>='"+Date+"')";
                
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAttndStatus(string EmpId, string Date)
        {
            try
            {
                var sql = @"select DayStatus,t.Category from AttdnProcessData s 
                left join EmployeeInformation emp on emp.SystemId=s.EmpSystemID
                 left join DayType t on t.DayType=s.DayStatus
                 where emp.EmployeeStatus = 'Active' and emp.SystemId='" + EmpId+"' and s.WorkDate='"+Date+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Shift & Attnd
        public IEnumerable<object> GetShiftAttnd(string EmpId, string Date)
        {
            try
            {
                var sql = @"select distinct p.ShiftSystemID as ShiftId,p.InTime,p.DayStatus,e.EmployeeName,p.RowId,
                p.OutTime,d.UserName as Shift,OverStay,AllotedOT as OTHour,
				t.Category from 
				dbo.AttdnProcessData p
                left join dbo.ShiftDefination d on d.SystemID=p.ShiftSystemID
				left join DayType t on t.DayType=p.DayStatus
				left join dbo.EmployeeInformation e on e.SystemId=p.EmpSystemID
				left join OTPerMinutePolicy ot on ot.PlantId=e.PlantId
                where e.EmployeeStatus = 'Active' and WorkDate='" + Date+"' and EmpSystemID='"+EmpId+@"'
				and ot.OverstayOrEarlyOut=p.OverStay";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
  
        public IEnumerable<object> GetShiftTimings(string PlantId)
        {
            try
            {
                var sql = @"SELECT distinct SystemID as Value,UserName AS Text,
                ShiftDefinationDescription as Timings,InTime,OutTime  
                FROM [dbo].[ShiftDefination] where isnull(PlantID,'')='" + PlantId+ "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPartShift(string PlantId,string Id)
        {
            try
            {
                var sql = @"SELECT distinct SystemID as Value,UserName AS Text,
                ShiftDefinationDescription as Timings,InTime,OutTime  
                FROM [dbo].[ShiftDefination] where isnull(PlantID,'')='"+PlantId+"' and SystemID='"+Id+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> CheckOTEligible(string EmpId)
        {
            try
            {
                var sql = @"SELECT distinct EmployeeCode as Code,SystemID as Value,EmployeeName,dm.IsOTEntitled FROM dbo.EmployeeInformation emp
                join hkp.LegalDesignation d on d.Id=emp.LegalDesignationId
                join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId =d.Id
				join mst.ManpowerBudget mb on mb.Id=emp.BudgetCode
				join org.Position p on p.Id=mb.PositionId
                join org.Department dp on dp.Id=p.DepartmentId
                join org.Section s on s.Id=p.SectionId
                join org.SubSection ss on ss.Id=p.SubSectionId
                join scs.DesignationMasterConfiguration dm on dm.DesignationMasterId=dd.DesignationMasterId
                and dm.PlantId=emp.PlantId
                where emp.EmployeeStatus = 'Active' and emp.SystemId='" + EmpId+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> EmpCompCode(string CGId, string CId)
        {
            try
            {
                var _sql = @"SELECT EmployeeCode as Code,SystemID as Value,EmployeeName AS Text
                    FROM dbo.EmployeeInformation where EmployeeStatus = 'Active'
                    AND EmpType!='Guest' and GroupID= '" + CGId + "' and CompanyId='" + CId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetuserInfo(string UserId)
        {
            try
            {
                var sql = @"select distinct Id as Value,UserId as Text from [SEC].[User] where UserId='" + UserId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public IEnumerable<object> GetOTId(string EmpId, string Date)
        {
            try
            {
                var sql = @"select Id,EmpSystemId,WorkDate from dbo.OTfromApp 
                where EmpSystemId='"+EmpId+"' and WorkDate='"+Date+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Validations

        public IEnumerable<object> CheckOD(string EmpId, string Date)
        {
            try
            {
                var sql = @"SELECT * FROM [dbo].[EmployeeOnDuty] WHERE EmpSystemId='"+EmpId+"'" +
                    " AND (FromDate='"+Date+"'OR ToDate='"+Date+"')";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
      
        public IEnumerable<object> CheckSalaryLock(string EmpId, string Month,string Year)
        {
            try
            {
                var sql = @"select * from SalaryProcMaster SPM
left join SalaryProcChild SPC on SPC.SlrProcMstSystemID = SPM.SystemID
where SPC.EmpInfoSystemID = '" + EmpId+ "' and SPM.MonthNo = '" + Month+ "' and SPM.YearNo = '" + Year+ "' and isnull(SPM.SalaryView,0) = 1";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       

    }

    public class BudgetCodeHistory
    {
        public string Id { get; set; }
        public string EmpSystemId { get; set; }
        public string LegalDesignationId { get; set; }
        public string AddedBy { get; set; }
        public string GivenDesignationId { get; set; }
        public string BudgetId { get; set; }
        public string AddedFromIP { get; set; }
        public string AddedDate { get; set; }
    }
    
    public class EmployeeInformationViewModel
    {
        public string SystemId { get; set; } 
        public string LegalDesignationId { get; set; }
        public string prevdesgnId { get; set; }
        public string EmploymentType { get; set; }
        public string GivenDesignationId { get; set; }
        public string LineId { get; set; }
        public string AddedFromIP { get; set; }
        public string EmployeeGroupSystemID { get; set; }
        public string UnitId { get; set; }
        public string PositionID { get; set; }
        public string DepartmentId { get; set; }
        public string BudgetCode { get; set; }
        public string SubSectionId { get; set; }
        public string DivisionId { get; set; }
        public string prevbudgetCode { get; set; }
        public string SubDivisionId { get; set; }
        public string SectionId { get; set; }
        public string AddedBy { get; set; }
        public string DesignationGroupId { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string DesignationSystemID { get; set; }
      
    }

    public class Updatebudgetcode
    {
        public string EmpSystemID { get; set; }
        public string BudgetId { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

    }



    public class PhysicalVerifyModel
    {
        public string AddedBy { get; set; }
        public string BudgetCode { get; set; }
        public string AddedFromIP { get; set; }
        public string InOutParam { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string EmpSystemId { get; set; }      
        public string OThour { get; set; }
        public string Remarks { get; set; }
        public string WorkDate { get; set; }
        public string Id { get; set; }
    }
    
    public class AttdnManualData
    {
        public string AddedBy { get; set; }
        public DateTime? DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string EmpSystemID { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string ShiftSystemId { get; set; }
        public string GroupID { get; set; }
        public string WorkDate { get; set; }
        public string DayStatus { get; set; }
       
    }

    public class CreateDetentionList
    {
        public string Id { get; set; }
        public string WorkCenterId { get; set; }
        public string DetentionTypeId { get; set; }
        public string MachineMasterId { get; set; }
        public string IssueByNo { get; set; }
        public string LogoutTime { get; set; } 
        public bool isClose { get; set; }
        public string Remarks { get; set; }
        public string AddedBy { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdateFromIP { get; set; }
        public string AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
    }

}
