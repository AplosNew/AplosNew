using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.ViewModel.Organizations;
using OTSBD;
using SetINOUT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;
using TBS;

namespace Library.Service.Attendances
{
    public class clsShiftInfo
    {
        ISqlRepository _sqlRepository;
        public clsShiftInfo(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        public IEnumerable<ComboModel> LoadYearCbo(string plantId)
        {
            try
            {
                var sql = @"SELECT Id, YearNo FROM dbo.YearlyCalendar WHERE PlantId='" + plantId + "'";
                return _sqlRepository.GetCombo(sql, "Id", "YearNo");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<ComboModel> GetFixedShift(string plantId)
        {
            try
            {
                var sql = @"select Systemid,ShiftDefinationDescription from ShiftDefination where PlantID='" + plantId + "' and IsActive=1";
                return _sqlRepository.GetCombo(sql, "Systemid", "ShiftDefinationDescription");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<ComboModel> GetRosterMaster(string plantId)
        {
            try
            {
                var sql = @"select Systemid,ShiftRosterName from ShiftRosterMaster where PlantID='" + plantId + "'";
                return _sqlRepository.GetCombo(sql, "Systemid", "ShiftRosterName");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        #region Department Group ---------------------

        public string SaveDepartmentMasterAndDetails(DepartmentGroup master, string DepartmentGroupId, List<DepartmentGroupDetails> DepartmentIdList)
        {
            DataSet dsMaster = null;
            DataSet dsDetails = null;
            DepartmentGroupId = string.Empty;

            try
            {
                SaveDepartmentMaster(master, DepartmentGroupId, out dsMaster);

                clsStaticInfo obj = new clsStaticInfo();
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    DepartmentGroupId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                }
                SaveDepartmentDetails(DepartmentIdList, DepartmentGroupId, out dsDetails);

                obj.SaveDataSets(dsMaster, dsDetails);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DepartmentGroupId;
        }

        void SaveDepartmentMaster(DepartmentGroup master, string DepartmentGroupId, out DataSet dsMaster)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
                DepartmentMaster(master.Id, out dsMaster);

                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "Id='" + master.Id + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DepartmentGroup", out sID);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    master.Id = "DM" + sID;
                    foreach (PropertyInfo prop in master.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(master, null));
                    }
                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in master.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(master, null));
                    }
                    dr.EndEdit();
                    #endregion
                }
                dvMaster.RowFilter = null;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        void SaveDepartmentDetails(List<DepartmentGroupDetails> DepartmentIdList, string DepartmentGroupId, out DataSet dsDetails)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsDetails = null;
            DeleteDepartmentDetails(DepartmentGroupId, out dsDetails);
            DepartmentDetails(DepartmentGroupId, out dsDetails);
            try
            {
                if (DepartmentIdList != null)
                {
                    for (int i = 0; i < DepartmentIdList.Count; i++)
                    {
                        DataView dvMaster = new DataView(dsDetails.Tables[0]);
                        dvMaster.RowFilter = "DepartmentGroupId='"+ DepartmentGroupId+@"' and DepartmentId='" + DepartmentIdList[i].Id + "' ";
                        if (dvMaster.Count == 0)
                        {
                            #region add

                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DepartmentGroupDetails", out sID);

                            DataRow dr = dsDetails.Tables[0].NewRow();
                            //DepartmentIdList[i].Id = "DG" + sID;
                            //foreach (PropertyInfo prop in DepartmentIdList[i].GetType().GetProperties())
                            //{
                            //    SetRowValue(ref dr, prop.Name, prop.GetValue(DepartmentIdList[i], null));
                            //}
                            dr["Id"] = "DG" + sID; ;
                            dr["DepartmentGroupId"] = DepartmentGroupId;
                            dr["DepartmentId"] = DepartmentIdList[i].Id;


                            dsDetails.Tables[0].Rows.Add(dr);
                            #endregion
                        }
                        else
                        {
                            #region edit

                            DataRow dr = dvMaster[0].Row;
                            dr.BeginEdit();

                            //foreach (PropertyInfo prop in dsDetails.GetType().GetProperties())
                            //{
                            //    SetRowValue(ref dr, prop.Name, prop.GetValue(dsDetails, null));
                            //}
                            dr["DepartmentGroupId"] = DepartmentGroupId;
                            dr["DepartmentId"] = DepartmentIdList[i].Id;
                            dr.EndEdit();
                            #endregion
                        }
                        dvMaster.RowFilter = null;

                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        void SetRowValue(ref DataRow dr, string Field, object v)
        {
            try
            {
                if (v is null)
                {
                    dr[Field] = DBNull.Value;
                }
                else
                {
                    dr[Field] = v;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void DepartmentMaster(string Id, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM  DepartmentGroup where ID='" + Id + @"' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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

        void DepartmentDetails(string DepartmentGroupId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM  DepartmentGroupDetails where DepartmentGroupId='" + DepartmentGroupId + @"' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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

        void DeleteDepartmentDetails(string DepartmentGroupId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"delete FROM  DepartmentGroupDetails where DepartmentGroupId='" + DepartmentGroupId + @"' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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


        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(Contract), out sID);
            return sID;
        }

        #endregion





        public IEnumerable<ComboModel> GetRosterShift(string plantId, string rosterId)
        {
            try
            {
                var sql = @"select Systemid,ShiftDefinationDescription from ShiftDefination where PlantID='" + plantId + "' and IsActive=1 AND SystemID IN (Select ShiftDefinationID from ShiftRosterChild Where SRMasterSystemID='" + rosterId + "')";
                return _sqlRepository.GetCombo(sql, "Systemid", "ShiftDefinationDescription");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> ShiftDefinationSearch(string sGroupID, string sPlantID)
        {
            string strSql = "";
            try
            {
                strSql = @"SELECT SystemID ShiftDefinationID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, SequenceNo ShiftSequence, CONVERT(VARCHAR(10), InTime, 108) AS InTime,
                                        InTimeStartMargin, LateMargin, AbsentEndMargin, CONVERT(VARCHAR(10), OutTime, 108) AS OutTime,
                                        OutTimeEndMargin, OTStartTime, CONVERT(VARCHAR(10), BreakStratTime, 108) AS BreakStratTime,
                                        CONVERT(VARCHAR(10), BreakEndTime, 108) AS BreakEndTime, BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
                                FROM ShiftDefination WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' Order By ShiftDefinationName";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//end of function

        public List<Dictionary<string, object>> GetEmpInfo(string companyGroupId, string plantId, string EffectiveDate, string criteria)
        {
            string fromDate = "01-" + Convert.ToDateTime(EffectiveDate).ToString("MMM") + "-" + Convert.ToDateTime(EffectiveDate).ToString("yyyy");
            string toDate = Convert.ToDateTime(fromDate).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

            try
            {
                string wcManual = "";
                string Apjoin = "";
                if (criteria == "MANUALOUTTUIME")
                {
                    wcManual = " AND AP.IsOTEntitled = 1  AND AP.IsManualOutTime = 1";
                    Apjoin = @"INNER JOIN AttdnProcessData AP ON AP.EmpSystemID = E.SystemId
                        INNER JOIN AttdnManualData MA ON AP.EmpSystemID = MA.EmpSystemID AND AP.WorkDate = MA.WorkDate";
                }
                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var OrgStrList = OrgStructureList(companyGroupId);
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";

                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = EN." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = EN." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = PO." + item.ColumnName + "Id\n";
                    }
                }
                var cmdText = @"SELECT    * fROM(  SELECT   dISTINCT        [CheckBoxSelect] = Convert(bit, 'false'),
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    , EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus , e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,E.PlantId
								    ,x.EffectiveDate
								    ,x.ShiftSystem
								    ,shiftd.ShiftDefinationName
								    ,X.SingleDayShift

                                    FROM EmployeeInformation e

                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    LEFT OUTER JOIN ORG.Department edept on edept.id=PO.DepartmentId
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=PO.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=PO.SubDivisionId
                                    LEFT OUTER JOIN ORG.Section es on es.id=PO.SectionId
                                    LEFT OUTER JOIN ORG.SubSection ess on ess.id=PO.SubSectionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=EN.UnitId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId
                                    
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    " + Apjoin + @"
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId

                                  		-------SUB-----
									left join ( select ESA.EmpSystemID,ESA.FixSystemID,ESA.IsSingleDayShift,format(ESA.EffectiveDate,'dd-MMM-yyyy')as EffectiveDate
									,ShiftSystem= case when isnull(esa.FixSystemID,'')=''  then RosterStartShiftID else FixSystemID end 
									,SingleDayShift= CASE WHEN esa.IsSingleDayShift=1 THEN 'Yes' else 'No' 
									end From EmployeeShiftAssign AS ESA
									INNER JOIN (select max(EffectiveDate)AS MaxEffectiveDate,EmpSystemID From EmployeeShiftAssign WHERE IsSingleDayShift=0 group by EmpSystemID)
									 AS Y ON Y.EmpSystemID =ESA.EmpSystemID AND ESA.EffectiveDate=Y.MaxEffectiveDate 
                                    WHERE  ESA.IsSingleDayShift=0 
                                        ) as x on x.EmpSystemID=e.SystemId 
										left join ShiftDefination shiftd on shiftd.SystemID=x.ShiftSystem
									    ------SUB-------

                                  WHERE DOJ<='" + toDate + @"' AND (DOS is null OR DOS>= '" + fromDate + "') and e.plantId='" + plantId + @"' and e.GroupID='" + companyGroupId + @"' " + wcManual + @"
                                     ) DD ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

                return _sqlRepository.GetDataCollection(cmdText);
                
            }
            catch (Exception)
            {
                throw;
            }
        }//end of function

        public List<Dictionary<string, object>> GetDepartmentInfo(string companyGroupId, string plantId)
        {
            try
            {
                var cmdText = @"  select *,
                                    Actives=  case when Active=1 then 'Yes' else 'No' end 
                            from [ORG].[Department]  ";

                return _sqlRepository.GetDataCollection(cmdText);

            }
            catch (Exception)
            {
                throw;
            }
        }//end of function

        public List<Dictionary<string, object>> GetDepartmentInfoEdit(string Id,string CompanyId, string plantId)
        {
            try
            {
                var cmdText = @"   select d.*, Actives=  case when d.Active=1 then 'Yes' else 'No' end 
                                     from [ORG].[Department]  d
                                     LEFT JOIN DepartmentGroupDetails DGD ON DGD.DepartmentId=d.Id
                                     LEFT JOIN DepartmentGroup dg ON dg.Id=DGD.DepartmentGroupId
                                     WHERE dg.Id='"+Id+ @"' and dg.CompanyId='" + CompanyId + @"' ";

                return _sqlRepository.GetDataCollection(cmdText);

            }
            catch (Exception)
            {
                throw;
            }
        }//end of function


        public List<Dictionary<string, object>> GetDepartmenthkp(string CompanyId)
        {
            try
            {
                var cmdText = @"  select UserName,ShortName,StandardName,Id,Description,Remarks,CompanyId,Active,Code,Sequence
                                    ,Actives = case when Active=1 then 'Yes' else 'No' end
                                    from DepartmentGroup where CompanyId='" + CompanyId + @"'   ";

                return _sqlRepository.GetDataCollection(cmdText);

            }
            catch (Exception)
            {
                throw;
            }
        }//end of function

        public IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId)
        {
            try
            {
                var strSQL = @" SELECT DISTINCT u.StandardName ColumnName,IsNULL(e.RType,'position') as Rtype,e.Sequence eSequence,p.Sequence pSequence from (
                                SELECT DISTINCT StandardName from [ORG].[StructureRelationship] as ee where CompanyGroupId='" + CompanyGroupId + @"' and RType = 'Entity' union
                                SELECT DISTINCT StandardName from [ORG].[StructureRelationship] as pp where CompanyGroupId='" + CompanyGroupId + @"' and RType = 'position' ) u
                                LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Entity' ) e on e.StandardName = u.StandardName
                                LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Position' ) p on p.StandardName = u.StandardName";
                return _sqlRepository.GetModelCollection<OrgStructureListViewModel>(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SaveData()
        {
            #region DataSet Declare

            DataSet dsShiftAttdnLoack = null;
            DataSet dsShiftMaxDate = null;

            DataSet dsChkMaxDate = null;

            DataSet dsShiftDay = null;

            DataSet dsShiftAssign = null;
            DataTable dtShiftAssign = null;
            DataRow drShiftAssign = null;
            DataView dvShiftAssign = null;

            DataSet dsWeekOffByDay_tbd = null;
            DataTable dtWeekOffByDay = null;
            DataRow drWeekOffByDay = null;
            DataView dvWeekOffByDay = null;

            //clsTax objTxGrEmp = null;

            clsStaticInfo objApp = null;
            clsEmployeeLoad objEmpLoad = null;

            #endregion

            bool DATA_OK = false;
            string sEffectiveDate = "";
            string sPervEffDt = "";
            string effectiveDate = "";
            string sMaxEffectiveDate = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");

            try
            {
                string lblEmpSystemId = string.Empty;
                string lblShiftAssSystemID = string.Empty;
                string plantid = string.Empty;
                bool radFixShift = false;
                bool radRosterShift = false;
                string txtEmpCode = string.Empty;
                string ddlEmpFixShift = string.Empty;
                string ddlEmpRosterShift = string.Empty;
                string ddlRosterStartShift = string.Empty;
                string txtEmpFixShiftEffectiveDate = string.Empty;
                string txtEmpRosterShiftEffectiveDate = string.Empty;
                string lblEmpDateOJ = string.Empty;
                string LOGIN_GROUP_ID = string.Empty;
                bool IsEdit = false;

                objApp = new clsStaticInfo();
                objEmpLoad = new clsEmployeeLoad();
                //objTxGrEmp = new clsTax();
                clsAttendance.AttendanceProcessAplos objAttdn = new clsAttendance.AttendanceProcessAplos();

                #region DataSet

                objEmpLoad.SaveEmployeeShiftAssign(lblEmpSystemId, lblShiftAssSystemID, out dsShiftAssign);
                dtShiftAssign = dsShiftAssign.Tables[0];
                dvShiftAssign = new DataView();
                dvShiftAssign.Table = dtShiftAssign;
                dvShiftAssign.RowFilter = "SystemID = '" + lblShiftAssSystemID + "'";

                //if (radFixShift.Checked == true)
                //{
                //    objEmpLoad.SaveEmployeeWeekOffByDay(lblEmpSystemId, lblWorkOffSystemID.Text.Trim(), out dsWeekOffByDay);
                //    dtWeekOffByDay = dsWeekOffByDay.Tables[0];
                //    dvWeekOffByDay = new DataView();
                //    dvWeekOffByDay.Table = dtWeekOffByDay;
                //    dvWeekOffByDay.RowFilter = "SystemID = '" + lblWorkOffSystemID.Text.Trim() + "'";
                //}

                #endregion DataSet

                if (DATA_OK == false)
                {
                    #region Validation

                    if (string.IsNullOrEmpty(plantid) == true)
                    {
                        Exception ex = new Exception("Select Plant First...");
                        throw (ex);
                    }

                    if (txtEmpCode == "" || txtEmpCode.Length > 30)
                    {
                        Exception ex = new Exception("Define the Employee Code...(Max length allowed 30)");
                        throw (ex);
                    }

                    #region Shift

                    if (radFixShift == false && radRosterShift == false)
                    {
                        Exception ex = new Exception("Please select any one Shift type (Fixed/Roster)...");
                        throw (ex);
                    }

                    if (radFixShift == true)
                    {
                        #region Fix Shift

                        if (ddlEmpFixShift == "")
                        {
                            Exception ex = new Exception("Please Define Fix Shift...");
                            throw (ex);
                        }
                        if ((txtEmpFixShiftEffectiveDate == "") || (bplib.clsWebLib.IsDateOK(txtEmpFixShiftEffectiveDate) == false))
                        {
                            Exception ex = new Exception("Please Define the Fix-Shift Effective Date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                            throw (ex);
                        }

                        if (txtEmpFixShiftEffectiveDate != "")
                        {
                            DateTime dtDOJ = bplib.clsWebLib.DateData_DBToApp(lblEmpDateOJ, bplib.clsWebLib.DB_DATE_FORMAT);
                            DateTime dtFSED = bplib.clsWebLib.DateData_DBToApp(txtEmpFixShiftEffectiveDate, bplib.clsWebLib.DB_DATE_FORMAT);
                            TimeSpan ts4 = dtDOJ - dtFSED;
                            int days4 = ts4.Days;
                            if (days4 > 0)
                            {
                                Exception ex = new Exception("Please check Fix-Shift Effective Date, cannot less than DOJ Date...");
                                throw (ex);
                            }
                        }

                        #region commented
                        //if (radAlignWithCC.Checked == false && radIndividualWeekOff.Checked == false)
                        //{
                        //    Exception ex = new Exception("Please select any one Week Off Day type (Company Calander(CC)/Individual)...");
                        //    throw (ex);
                        //}
                        //if (radIndividualWeekOff.Checked == true)
                        //{
                        //    if (string.IsNullOrEmpty(ddlFstWeekOffDay.SelectedValue.Trim()) == true)
                        //    {
                        //        ddlFstWeekOffDay.Focus();
                        //        Exception ex = new Exception("Please Select 1st Week Off Day...");
                        //        throw (ex);
                        //    }
                        //    if (string.IsNullOrEmpty(ddlFstWeekOffDay.SelectedValue.Trim()) == false)
                        //    {
                        //        if (radFstHalf.Checked == false && radFstFull.Checked == false)
                        //        {
                        //            Exception ex = new Exception("Please select any one Week Off type (Full/Half)...");
                        //            throw (ex);
                        //        }
                        //        if (radFstHalf.Checked == true && string.IsNullOrEmpty(ddlFstHalfLengthType.Text.Trim()) == true)
                        //        {
                        //            ddlFstHalfLengthType.Focus();
                        //            Exception ex = new Exception("Please select day length type...");
                        //            throw (ex);
                        //        }
                        //    }
                        //    if (string.IsNullOrEmpty(ddlSndWeekOffDay.SelectedValue.Trim()) == false)
                        //    {
                        //        if (radSndHalf.Checked == false && radSndFull.Checked == false)
                        //        {
                        //            Exception ex = new Exception("Please select any one Week Off type (Full/Half)...");
                        //            throw (ex);
                        //        }
                        //        if (radSndHalf.Checked == true && string.IsNullOrEmpty(ddlSndHalfLengthType.Text.Trim()) == true)
                        //        {
                        //            ddlSndHalfLengthType.Focus();
                        //            Exception ex = new Exception("Please select day length type...");
                        //            throw (ex);
                        //        }
                        //    }
                        //    if ((string.IsNullOrEmpty(ddlFstWeekOffDay.SelectedValue.Trim()) == false) & (string.IsNullOrEmpty(ddlSndWeekOffDay.SelectedValue.Trim()) == false) & (ddlFstWeekOffDay.SelectedValue.Trim() == ddlSndWeekOffDay.SelectedValue.Trim()))
                        //    {
                        //        Exception ex = new Exception("1st Week Off Day and 2nd Week Off Day can not be same...");
                        //        throw (ex);
                        //    }
                        //} 
                        #endregion

                        sEffectiveDate = txtEmpFixShiftEffectiveDate;

                        #endregion Fix Shift
                    }
                    if (radRosterShift == true)
                    {
                        #region Roster Shift

                        if (ddlEmpRosterShift == "")
                        {
                            Exception ex = new Exception("Please Define Roster Shift...");
                            throw (ex);
                        }
                        if ((txtEmpRosterShiftEffectiveDate == "") || (bplib.clsWebLib.IsDateOK(txtEmpRosterShiftEffectiveDate) == false))
                        {
                            Exception ex = new Exception("Please Define the Roster Shift Effective Date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                            throw (ex);
                        }
                        if (txtEmpRosterShiftEffectiveDate != "")
                        {
                            DateTime dtDOJ = bplib.clsWebLib.DateData_DBToApp(lblEmpDateOJ, bplib.clsWebLib.DB_DATE_FORMAT);
                            DateTime dtRSED = bplib.clsWebLib.DateData_DBToApp(txtEmpRosterShiftEffectiveDate, bplib.clsWebLib.DB_DATE_FORMAT);
                            TimeSpan ts4 = dtDOJ - dtRSED;
                            int days4 = ts4.Days;
                            if (days4 > 0)
                            {
                                Exception ex = new Exception("Please check Roster-Shift Effective Date, cannot less than DOJ Date...");
                                throw (ex);
                            }
                        }
                        if (string.IsNullOrEmpty(ddlRosterStartShift) == true)
                        {
                            Exception ex = new Exception("Please Select Roster Start Shift...");
                            throw (ex);
                        }
                        //if (string.IsNullOrEmpty(txtStartFromDay.Text.Trim()) == true)
                        //{
                        //    txtStartFromDay.Focus();
                        //    Exception ex = new Exception("Please Select Roster Shift Start From Day...");
                        //    throw (ex);
                        //}
                        objEmpLoad.CheckShiftRosterChild(LOGIN_GROUP_ID, plantid, ddlEmpRosterShift, ddlRosterStartShift, out dsShiftDay);
                        if (dsShiftDay.Tables[0].Rows.Count > 0)
                        {
                            //if (bplib.clsWebLib.IsNumeric(txtStartFromDay.Text.Trim()) == false)
                            //{
                            //    txtStartFromDay.Focus();
                            //    Exception ex = new Exception("Please Enter Numeric data Only");
                            //    throw (ex);
                            //}
                            //if (Convert.ToBoolean(dsShiftDay.Tables[0].Rows[0]["IsDaysLengthShiftRoster"].ToString()) == true)
                            //{
                            //    if (string.IsNullOrEmpty(txtStartFromDay.Text.Trim()) == true)
                            //    {
                            //        txtStartFromDay.Text = "1";
                            //    }
                            //    if ((Convert.ToInt32(dsShiftDay.Tables[0].Rows[0]["DaysLengthShiftRoster"].ToString()) - Convert.ToInt32(txtStartFromDay.Text)) < 0)
                            //    {
                            //        txtStartFromDay.Focus();
                            //        Exception ex = new Exception("Please check shift Start From Day, cannot more than Day Lenght...");
                            //        throw (ex);
                            //    }
                            //}
                        }

                        sEffectiveDate = txtEmpRosterShiftEffectiveDate;

                        #endregion Roster Shift
                    }
                    if (IsEdit)
                    {
                        if (dvShiftAssign.Count > 0)
                        {
                            DateTime dtPervEffDt = bplib.clsWebLib.DateData_DBToApp(dvShiftAssign[0].Row["EffectiveDate"].ToString().Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                            DateTime dtEfftDt = bplib.clsWebLib.DateData_DBToApp(sEffectiveDate.Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                            TimeSpan ts4 = dtEfftDt - dtPervEffDt;
                            int days4 = ts4.Days;
                            if (days4 > 0)
                            {
                                Exception ex = new Exception("Unable to Edit Effective Date, cannot be more than Last Effective Date...");
                                throw (ex);
                            }
                            sPervEffDt = dtPervEffDt.ToString("dd-MMM-yyyy");
                        }
                    }

                    #endregion Shift

                    if (objEmpLoad.CheckShiftAssignIsAttdnLock(lblEmpSystemId, sEffectiveDate.Trim(), out dsShiftAttdnLoack) == false)
                    {
                        Exception ex = new Exception("Unable to Change/AddNew Shift Assignment before Date " + Convert.ToDateTime(dsShiftAttdnLoack.Tables[0].Rows[0]["WorkDate"].ToString()).ToString("dd-MMM-yyyy") + ", attendance already processed. For Change/AddNew Shift Assignment, require to delete all attendance information of this employee after effective date...");
                        throw (ex);
                    }

                    if (objEmpLoad.CheckShiftAssignEffictiveDateIsGrtOthers(lblEmpSystemId, sEffectiveDate.Trim(), lblShiftAssSystemID, out dsChkMaxDate) == false)
                    {
                        Exception ex = new Exception("Unable to Change/AddNew Shift Assignment before Date " + Convert.ToDateTime(dsChkMaxDate.Tables[0].Rows[0]["EffectiveDate"].ToString()).ToString("dd-MMM-yyyy") + ", Another Shift had already Assiged in this date...");
                        throw (ex);
                    }

                    DataSet dsJL = null;
                    objEmpLoad.GetJobLocationMaxDate(lblEmpSystemId, sEffectiveDate, out dsJL);
                    if (dsJL.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Shift effective Date [" + sEffectiveDate + "] can not be less than Joblocation Effective Date [" + dsJL.Tables[0].Rows[0]["ED"].ToString() + "] ");
                    }
                    var ep = "'" + lblEmpSystemId + "'";
                    // lock validation
                    AttendanceProcessAplos ob = new AttendanceProcessAplos();
                    ob.LockValidation(plantid, sEffectiveDate, sEffectiveDate, ep);


                    DATA_OK = true;

                    #endregion
                    if (radFixShift)
                    {
                        effectiveDate = txtEmpFixShiftEffectiveDate;
                    }
                    else
                    {
                        effectiveDate = txtEmpRosterShiftEffectiveDate;

                    }

                    objEmpLoad.GetEmployeeJobLocation(lblEmpSystemId, effectiveDate);
                }
                if (DATA_OK == true)
                {
                    objEmpLoad.GetMaxDateOfShiftAssign(lblEmpSystemId, sEffectiveDate.Trim(), out dsShiftMaxDate);
                    if (dsShiftMaxDate.Tables[0].Rows.Count > 0)
                    {
                        if (string.IsNullOrEmpty(dsShiftMaxDate.Tables[0].Rows[0]["MaxWorkDate"].ToString().Trim()) == false)
                        {
                            sMaxEffectiveDate = Convert.ToDateTime(dsShiftMaxDate.Tables[0].Rows[0]["MaxWorkDate"].ToString()).ToString("dd-MMM-yyyy");
                        }
                    }

                    objEmpLoad.DeleteEmpDateWiseShiftAssign(lblEmpSystemId, sEffectiveDate.Trim());

                    #region NEW ID GENERATE

                    string strShiftAssSystemID = "";
                    if (lblShiftAssSystemID == "")
                    {
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_SHIFT_ASSIGN", out strShiftAssSystemID);
                        strShiftAssSystemID = "SA" + "-" + strShiftAssSystemID;
                        lblShiftAssSystemID = strShiftAssSystemID.ToString();
                    }

                    //string strWorkOffSystemID = "";
                    //if (lblWorkOffSystemID.Text == "" & radFixShift.Checked == true)
                    //{
                    //    bplib.clsGenID objGenID = new bplib.clsGenID();
                    //    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_WEEKOFF_BYDAY", out strWorkOffSystemID);
                    //    strWorkOffSystemID = "EW" + "-" + strWorkOffSystemID;
                    //    lblWorkOffSystemID.Text = strWorkOffSystemID.ToString();
                    //}

                    #endregion End ID Generate

                    #region Employee Shift Assign

                    ShiftAssignEmp s = new ShiftAssignEmp();
                    s.SystemID = lblShiftAssSystemID;
                    s.AddedBy = "";
                    s.DateAdded = DateTime.Now;
                    s.DateUpdated = DateTime.Now;
                    s.EffectiveDate = Convert.ToDateTime(effectiveDate);
                    s.EmpSystemID = lblEmpSystemId;
                    s.FixSystemID = ddlEmpFixShift;
                    s.IsFix = radFixShift;
                    //s.IsRoster = radRosterShift;
                    s.RosterStartShiftID = ddlRosterStartShift;
                    s.RosterSystemID = ddlEmpRosterShift;
                    s.UpdatedBy = "";

                    var obj = new clsStaticInfo();
                    if (dvShiftAssign.Count == 0)
                    {// Add new block                        
                        drShiftAssign = dtShiftAssign.NewRow();
                        UpdateEmployeeShiftAssignDataRow("ADDNEW", s, ref drShiftAssign);
                        dtShiftAssign.Rows.Add(drShiftAssign);
                    }
                    else
                    {//edit block                        
                        drShiftAssign = dvShiftAssign[0].Row;
                        drShiftAssign.BeginEdit();
                        UpdateEmployeeShiftAssignDataRow("EDIT", s, ref drShiftAssign);
                        drShiftAssign.EndEdit();
                    }
                    dvShiftAssign.RowFilter = null;

                    #endregion Employee Shift Assign

                    #region Employee Week Off By Day
                    string pk_WO = string.Empty;
                    //if (radFixShift == true)
                    //{
                    //    if (dvWeekOffByDay.Count == 0)
                    //    {// Add new block
                    //        pk_WO = lblWorkOffSystemID.Text;
                    //        drWeekOffByDay = dtWeekOffByDay.NewRow();
                    //        UpdateEmployeeWeekOffByDayDataRow("ADDNEW", ref drWeekOffByDay);
                    //        dtWeekOffByDay.Rows.Add(drWeekOffByDay);
                    //    }
                    //    else
                    //    {//edit block
                    //        drWeekOffByDay = dvWeekOffByDay[0].Row;
                    //        pk_WO = drWeekOffByDay["SystemID"].ToString();
                    //        drWeekOffByDay.BeginEdit();
                    //        UpdateEmployeeWeekOffByDayDataRow("EDIT", ref drWeekOffByDay);
                    //        drWeekOffByDay.EndEdit();
                    //    }
                    //    dvWeekOffByDay.RowFilter = null;
                    //}

                    #endregion Employee Week Off By Day
                    //DeleteWeekOff(ref dsWeekOffByDay_tbd, lblEmpSystemId, sEffectiveDate.Trim(), pk_WO);
                    objApp.SaveDataSets(dsShiftAssign, dsWeekOffByDay_tbd);

                    ShiftProcess(sEffectiveDate, sMaxEffectiveDate, sPervEffDt);

                    DateTime FromDate = Convert.ToDateTime(sEffectiveDate.Trim());
                    DateTime ToDate = Convert.ToDateTime(sMaxEffectiveDate.Trim());
                    while (FromDate <= ToDate)
                    {
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                        objAttdn.SaveTotal(plantid, FromDate.ToString("dd-MMM-yyyy"), lblEmpSystemId, false);
                        FromDate = FromDate.AddDays(1);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objApp = null;
            }
        }//End Function
        private void UpdateEmployeeShiftAssignDataRow(string OPN_FLAG, ShiftAssignEmp s, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = s.SystemID;
                    drLocal["EmpSystemID"] = s.EmpSystemID;

                    drLocal["AddedBy"] = s.AddedBy;
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["IsFix"] = s.IsFix;
                drLocal["IsRoster"] = s.IsRoster;
                drLocal["IsSingleDayShift"] = false;
                if (s.IsFix)
                {
                    drLocal["EffectiveDate"] = s.EffectiveDate;
                    drLocal["FixSystemID"] = s.FixSystemID;

                    drLocal["RosterSystemID"] = DBNull.Value;
                    drLocal["RosterStartShiftID"] = DBNull.Value;
                    drLocal["StartFromDay"] = DBNull.Value;
                }
                if (s.IsRoster)
                {
                    drLocal["EffectiveDate"] = s.EffectiveDate;
                    drLocal["RosterSystemID"] = s.RosterSystemID;
                    drLocal["RosterStartShiftID"] = s.RosterStartShiftID;
                    drLocal["StartFromDay"] = 0;

                    drLocal["FixSystemID"] = DBNull.Value;
                }

                drLocal["UpdatedBy"] = "";
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
        void ShiftProcess(string OPN_FLAG, string a, string s) { }
        public void SaveDataBulk(ShiftAssignEmp ss,bool CheckBox)
        {
            #region DataSet Declare

            //DataSet dsShiftAttdnLoack = null;
            //DataSet dsShiftMaxDate = null;
            //DataSet dsChkMaxDate = null;
            DataSet dsEmployeeInfo = null;

            DataSet dsShiftAssign = null;
            DataTable dtShiftAssign = null;
            DataRow drShiftAssign = null;
            DataView dvShiftAssign = null;

            clsStaticInfo objApp = null;
            clsEmployeeLoad objEmpLoad = null;

            #endregion

            bool DATA_OK = false;
            //string sEffectiveDate = "";
            //string sPervEffDt = "";
            //string effectiveDate = "";
            //string sMaxEffectiveDate = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
            DataSet dsHRsetting = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsSetInOut obj = new clsSetInOut();
                objApp = new clsStaticInfo();
                objEmpLoad = new clsEmployeeLoad();
                clsAttendance.AttendanceProcessAplos objAttdn = new clsAttendance.AttendanceProcessAplos();
                TBS.ShiftProcess spr = new TBS.ShiftProcess();
                FixedShiftProcess spf = new FixedShiftProcess();

                #region DataSet

                EmployeeShiftAssign_for_Save(ss.EmpSystemIDs, ss.EffectiveDate.ToString("dd-MMM-yyyy"), out dsShiftAssign);
                dtShiftAssign = dsShiftAssign.Tables[0];
                dvShiftAssign = new DataView();
                dvShiftAssign.Table = dtShiftAssign;
                //dvShiftAssign.RowFilter = "SystemID = '" + lblShiftAssSystemID + "'";
                GetEmployeeInfo(ss.EmpSystemIDs, out dsEmployeeInfo);//need to work

                #endregion DataSet

                if (DATA_OK == false)//need to work
                {

                    #region Validation

                    #region Shift

                    if (ss.IsFix == false && ss.IsRoster == false)
                    {
                        Exception ex = new Exception("Please select any one Shift type (Fixed/Roster)...");
                        throw (ex);
                    }

                    if ((bplib.clsWebLib.IsDateOK(ss.EffectiveDate.ToString("dd-MMM-yyyy")) == false))
                    {
                        Exception ex = new Exception("Please Define Effective Date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                        throw (ex);
                    }

                    if (ss.IsFix == true)//
                    {
                        if (ss.FixSystemID == null)
                        {
                            Exception ex = new Exception("Please Select Fix Shift...");
                            throw (ex);
                        }
                    }
                    if (ss.IsRoster == true)
                    {
                        if (ss.RosterSystemID.Length == 0)
                        {
                            Exception ex = new Exception("Please Select Roster Shift...");
                            throw (ex);
                        }
                        if (ss.RosterStartShiftID.Length == 0)
                        {
                            Exception ex = new Exception("Please Select Roster Start Shift...");
                            throw (ex);
                        }
                    }

                    #endregion Shift

                    #region valida
                    //if (objEmpLoad.CheckShiftAssignIsAttdnLock(lblEmpSystemId, sEffectiveDate.Trim(), out dsShiftAttdnLoack) == false)
                    //{
                    //    Exception ex = new Exception("Unable to Change/AddNew Shift Assignment before Date " + Convert.ToDateTime(dsShiftAttdnLoack.Tables[0].Rows[0]["WorkDate"].ToString()).ToString("dd-MMM-yyyy") + ", attendance already processed. For Change/AddNew Shift Assignment, require to delete all attendance information of this employee after effective date...");
                    //    throw (ex);
                    //}

                    //if (objEmpLoad.CheckShiftAssignEffictiveDateIsGrtOthers(lblEmpSystemId, sEffectiveDate.Trim(), lblShiftAssSystemID, out dsChkMaxDate) == false)
                    //{
                    //    Exception ex = new Exception("Unable to Change/AddNew Shift Assignment before Date " + Convert.ToDateTime(dsChkMaxDate.Tables[0].Rows[0]["EffectiveDate"].ToString()).ToString("dd-MMM-yyyy") + ", Another Shift had already Assiged in this date...");
                    //    throw (ex);
                    //}

                    //DataSet dsJL = null;
                    //objEmpLoad.GetJobLocationMaxDate(lblEmpSystemId, sEffectiveDate, out dsJL);
                    //if (dsJL.Tables[0].Rows.Count > 0)
                    //{
                    //    throw new Exception("Shift effective Date [" + sEffectiveDate + "] can not be less than Joblocation Effective Date [" + dsJL.Tables[0].Rows[0]["ED"].ToString() + "] ");
                    //}
                    #endregion

                    for (int i = 0; i < dsEmployeeInfo.Tables[0].Rows.Count; i++)
                    {
                        string _empid = dsEmployeeInfo.Tables[0].Rows[i]["SystemID"].ToString();
                        string _EmployeeCode = dsEmployeeInfo.Tables[0].Rows[i]["EmployeeCode"].ToString();
                        string _DOJ = dsEmployeeInfo.Tables[0].Rows[i]["DOJ"].ToString();
                        string _COD = dsEmployeeInfo.Tables[0].Rows[i]["COD"].ToString();
                        string _DOS = dsEmployeeInfo.Tables[0].Rows[i]["DOS"].ToString();
                        string _SAD = dsEmployeeInfo.Tables[0].Rows[i]["SAD"].ToString();
                        string _JLD = dsEmployeeInfo.Tables[0].Rows[i]["JLD"].ToString();

                        if (string.IsNullOrEmpty(_JLD))
                        {
                            throw new Exception("Joblocation not found for Employee [" + _EmployeeCode + "]");
                        }

                        if (string.IsNullOrEmpty(_SAD) == false)
                        {
                            //if (Convert.ToDateTime(_SAD) == ss.EffectiveDate)
                            //{
                            //    throw new Exception("Already assigned shift has same effective Date [" + _SAD + "] for Employee [" + _EmployeeCode + "]");
                            //}

                            if (Convert.ToDateTime(_SAD) > ss.EffectiveDate)
                            {
                                throw new Exception("Already assigned shift has greater effective Date [" + _SAD + "] for Employee [" + _EmployeeCode + "]");
                            }
                        }

                        if (Convert.ToDateTime(_DOJ) > ss.EffectiveDate)
                        {
                            throw new Exception("EffectiveDate can not be less than DOJ [" + _DOJ + "] for Employee [" + _EmployeeCode + "]");
                        }

                        if (Convert.ToDateTime(_COD) > ss.EffectiveDate)
                        {
                            throw new Exception("EffectiveDate can not be less than 'Cut-off-Date' [" + _COD + "] for Employee [" + _EmployeeCode + "]");
                        }

                        if (Convert.ToDateTime(_JLD) > ss.EffectiveDate)
                        {
                            throw new Exception("EffectiveDate can not be less than 'Joblocation assigned Date' [" + _JLD + "] for Employee [" + _EmployeeCode + "]");
                        }

                        if (string.IsNullOrEmpty(_DOS) == false)
                        {
                            if (Convert.ToDateTime(_DOS) < ss.EffectiveDate)
                            {
                                throw new Exception("Separation Date can not be less than 'EffectiveDate Date' [" + ss.EffectiveDate + "] for Employee [" + _EmployeeCode + "]");
                            }
                        }

                        //if (string.IsNullOrEmpty(_SAD) == false && Convert.ToDateTime(_SAD) == ss.EffectiveDate)
                        //{
                        //    throw new Exception("Shift already assigned on the same date for Employee [" + _EmployeeCode + "]");
                        //}
                    }//for

                    //lock validation
                    AttendanceProcessAplos ob = new AttendanceProcessAplos();
                    ob.LockValidation(ss.PlantId, ss.EffectiveDate.ToString("dd-MMM-yyyy"), DateTime.Now.ToString("dd-MMM-yyyy"), ss.EmpSystemIDs);//need to work

                    GetHRsettinng(identity.PlantId, out dsHRsetting);

                    DATA_OK = true;

                    #endregion
                    //GetEmployeeJobLocation(ss.EmpSystemIDs, effectiveDate);//need to work
                }
                if (DATA_OK == true)
                {
                    //GetMaxDateOfShiftAssign(ss.EmpSystemIDs, sEffectiveDate.Trim(), out dsShiftMaxDate);//need to work
                    //if (dsShiftMaxDate.Tables[0].Rows.Count > 0)
                    //{
                    //    if (string.IsNullOrEmpty(dsShiftMaxDate.Tables[0].Rows[0]["MaxWorkDate"].ToString().Trim()) == false)
                    //    {
                    //        sMaxEffectiveDate = Convert.ToDateTime(dsShiftMaxDate.Tables[0].Rows[0]["MaxWorkDate"].ToString()).ToString("dd-MMM-yyyy");
                    //    }
                    //}

                    DeleteEmpDateWiseShiftAssign(ss.EmpSystemIDs, ss.EffectiveDate.ToString("dd-MMM-yyyy"));

                    #region NEW ID GENERATE

                    string _PK = "";
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_SHIFT_ASSIGN", out _PK);

                    #endregion End ID Generate

                    #region Employee Shift Assign
                    int _Count = 0;
                    for (int i = 0; i < dsEmployeeInfo.Tables[0].Rows.Count; i++)
                    {
                        string _empid = dsEmployeeInfo.Tables[0].Rows[i]["SystemID"].ToString();
                        string _systemid = "";
                        ShiftAssignEmp s_ob = new ShiftAssignEmp();
                        s_ob.SystemID = _systemid;
                        s_ob.DateAdded = DateTime.Now;
                        s_ob.DateUpdated = DateTime.Now;
                        s_ob.EffectiveDate = ss.EffectiveDate;
                        s_ob.EmpSystemID = _empid;
                        s_ob.FixSystemID = ss.FixSystemID;
                        s_ob.IsFix = ss.IsFix;
                       // s_ob.IsRoster = ss.IsRoster;
                        s_ob.RosterStartShiftID = ss.RosterStartShiftID;
                        s_ob.RosterSystemID = ss.RosterSystemID;
                        s_ob.UpdatedBy = ss.UpdatedBy;
                        s_ob.AddedBy = ss.AddedBy;
                        dvShiftAssign.RowFilter = "EmpSystemID='" + _empid + "' and EffectiveDate='" + ss.EffectiveDate + "'";
                        if (dvShiftAssign.Count > 0)
                        {
                            s_ob.SystemID = dvShiftAssign[0]["SystemID"].ToString();
                        }
                        dvShiftAssign.RowFilter = null;

                        dvShiftAssign.RowFilter = "SystemId='" + s_ob.SystemID + "'";
                        if (dvShiftAssign.Count == 0)
                        {// Add new block
                            _Count++;
                            s_ob.SystemID = "SA" + _PK + "_" + _Count;
                            drShiftAssign = dtShiftAssign.NewRow();
                            UpdateEmployeeShiftAssignDataRow("ADDNEW", s_ob, ref drShiftAssign);
                            dtShiftAssign.Rows.Add(drShiftAssign);
                        }
                        else
                        {//edit block                        
                            drShiftAssign = dvShiftAssign[0].Row;
                            drShiftAssign.BeginEdit();
                            UpdateEmployeeShiftAssignDataRow("EDIT", s_ob, ref drShiftAssign);
                            drShiftAssign.EndEdit();
                        }
                        dvShiftAssign.RowFilter = null;
                    }

                    #endregion Employee Shift Assign

                    SaveDataSets(dsShiftAssign);


                    if (dsHRsetting.Tables[0].Rows.Count > 0)
                    {
                        DateTime FromDateR = Convert.ToDateTime(ss.EffectiveDate);
                        DateTime ToDateR = DateTime.Now;
                        while (FromDateR <= ToDateR)
                        {

                            obj.SetRawINOUTonShiftAssignment(identity.PlantId, identity.CompanyGroupId, FromDateR.ToString("dd-MMM-yyyy"), ss.EmpSystemIDs);
                            FromDateR = FromDateR.AddDays(1);
                        }
                    }//hr




                    //ShiftProcess(sEffectiveDate, sMaxEffectiveDate, sPervEffDt); 
                    if (CheckBox == true)
                    {
                        DateTime FromDate = Convert.ToDateTime(ss.EffectiveDate);
                        DateTime ToDate = DateTime.Now;
                        while (FromDate <= ToDate)
                        {
                            //------------process shift
                            //spf.ShiftProcess(ss.PlantId, FromDate.ToString("dd-MMM-yyyy"),ss.GroupId,ss.EmpSystemIDs);
                            //spr.ShiftProcessStart(ss.PlantId, FromDate.ToString("dd-MMM-yyyy"), ss.GroupId, ss.EmpSystemIDs);
                            //------------
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                            objAttdn.SaveTotal(ss.PlantId, FromDate.ToString("dd-MMM-yyyy"), ss.EmpSystemIDs, false, true);
                            FromDate = FromDate.AddDays(1);
                        }
                    }
                   
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objApp = null;
            }
        }//End Function
        public void SaveDataSets(params DataSet[] dsRef)
        {
            //throw new Exception("test");string empid,string WorkDate,
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                //objCon.ExecuteNonQueryWrapper(" update EmpDateWiseShiftAssign set ToReprocess='Yes' where EmpSystemId='" + empid + @"' and WorkDate>='" + WorkDate + @"' ", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                        if (dsRef[i].Tables.Count > 0)
                            objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                    i++;
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exp)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void EmployeeShiftAssign_for_Save(string empids, string effectivedate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeShiftAssign
                                    WHERE EmpSystemID in (" + empids + @") 
                                            AND EffectiveDate = '" + effectivedate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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
        public void DeleteEmpDateWiseShiftAssign(string strEmpIDs, string strEffectDate)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM dbo.EmpDateWiseShiftAssign WHERE EmpSystemID in (" + strEmpIDs + @") AND WorkDate >= '" + strEffectDate + @"' ", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void GetEmployeeJobLocation(string empSystemIds, string effectiveDate)
        {
            DataSet dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT JobLcSystemID FROM EmpDateWiseJobLocation WHERE EmpSystemID in (" + empSystemIds + ") AND EffectiveDate<='" + effectiveDate + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("This Employee has no job location before Effective date :'" + effectiveDate + "'.");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function---
        public void GetEmployeeInfo(string empSystemIds, out DataSet dsRef)
        {
            dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select e.SystemID,e.EmployeeCode
                                --,e.doj,e.dos,c.CutOffDate COD,a.ed SAD,j.ed JLD
                                ,format(e.doj,'dd-MMM-yyyy') DOJ
                                ,format(e.dos,'dd-MMM-yyyy') DOS
                                ,format(c.CutOffDate,'dd-MMM-yyyy') COD
                                ,format(a.ed,'dd-MMM-yyyy') SAD
                                ,format(j.ed,'dd-MMM-yyyy') JLD

                                from EmployeeInformation e
                                left join scs.OpeningBalanceCutOffDate c on c.PlantId=e.PlantId and c.ModuleName='HR'
                                left join (select max(effectivedate) ed,EmpSystemID from EmployeeShiftAssign WHERE IsSingleDayShift=0 group by EmpSystemID) a on a.EmpSystemID=e.SystemId
                                left join (select max(effectivedate) ed,EmpSystemID from EmpDateWiseJobLocation group by EmpSystemID) j on j.EmpSystemID=e.SystemId
                                WHERE e.SystemID in (" + empSystemIds + ") ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function---
        public bool GetMaxDateOfShiftAssign(string strEmpIDs, string strEffectDate, out System.Data.DataSet dsRef)
        {
            //System.Data.DataSet dsRef = null;
            string strSQl;
            bool blnStatus = false;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQl = @"SELECT MAX(WorkDate) MaxWorkDate,EmpSystemID FROM dbo.EmpDateWiseShiftAssign
                            WHERE EmpSystemID in (" + strEmpIDs + @") AND WorkDate >= '" + strEffectDate + @"'
                                Group by EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void GetHRsettinng(string plantid,out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from PlantWiseHRMSSetting where PlantID='"+plantid+ "' and isnull(ShiftBasedPunchFlag,0)=1";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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
    }

    public class ShiftAssignEmp
    {
        public string SystemID { get; set; }
        public string EmpSystemID { get; set; }
        public string EmpSystemIDs { get; set; }
        public string PlantId { get; set; }
        public string GroupId { get; set; }
        public string AddedBy { get; set; }
        public DateTime? DateAdded { get; set; }
        public bool IsFix { get; set; }
        //public bool IsRoster { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string FixSystemID { get; set; }
        public string RosterSystemID { get; set; }
        public string RosterStartShiftID { get; set; }
        public string StartFromDay { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        public bool IsRoster   // property
        {
            get { return !IsFix; }   // get method
            //set { name = value; }  // set method
        }
    }
    public class EmpList
    {
        public string EmpSystemId { get; set; }
        public string DOJ { get; set; }
        public string COD { get; set; }
        public string DOS { get; set; }
        public string ShiftAssignedEffectiveDate { get; set; }
        public string JoblocationEffectiveDate { get; set; }
    }

    public class DepartmentGroup
    {
        public string Id { get; set; }
        public string Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public bool Active { get; set; }
        public string CompanyId { get; set; }
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime? AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        [NeverUpdate]
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class DepartmentGroupDetails
    {
        public string Id { get; set; }
        public string DepartmentGroupId { get; set; }
        public string DepartmentId { get; set; }

    }

}
