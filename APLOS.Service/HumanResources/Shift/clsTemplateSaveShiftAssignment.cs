using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;


namespace Library.Service.HumanResources.Shift
{
   public class clsTemplateSaveShiftAssignment
    {
        public void GetEmployeeShiftAssign(string pks, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from EmployeeShiftAssign where EmpSystemID in (" + pks + ")";
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
        private void UpdateEmployeeInformationDataRow(string groupid, string companyid, string plantid,string user, string OPN_FLAG, string systemid,  EmployeeShiftUploadTemplate ep, ref DataRow drLocal)
        {
            try
            {
                string _plantid = plantid;
                string _groupid = groupid;
                string _companyid = companyid;
                //clsValidation clsValidation = new clsValidation();
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = systemid;                    
                    drLocal["AddedBy"] = user;
                    drLocal["DateAdded"] = DateTime.Now;
                }
                //var ReligionID = GetPK(ep.Religion);
                //if (ReligionID == string.Empty)
                //{
                //    drLocal["ReligionID"] = DBNull.Value;
                //}
                //else
                //{
                //    drLocal["ReligionID"] = ReligionID;
                //}
                drLocal["EmpSystemID"] = ep.SystemId;
                if (ep.IsRoster == string.Empty || ep.IsRoster=="NO"|| ep.IsRoster==null)
                {
                    drLocal["IsRoster"] = false;
                    drLocal["IsFix"] = true;
                    var FixSystemID = GetPK(ep.ShiftSystemId);
                    drLocal["FixSystemID"] = FixSystemID;
                    drLocal["RosterSystemID"] = DBNull.Value;
                    drLocal["RosterStartShiftID"] = DBNull.Value;
                }
                else
                {
                    drLocal["IsRoster"] = true;
                    drLocal["IsFix"] = false;
                    drLocal["FixSystemID"] = DBNull.Value;
                    var RosterSystemID = GetPK(ep.RosterSystemID);
                    var RosterStartShiftID = GetPK(ep.RosterStartShiftID);
                    drLocal["RosterSystemID"] = RosterSystemID;
                    drLocal["RosterStartShiftID"] = RosterStartShiftID;
                }                
                drLocal["EffectiveDate"] = ep.EffectiveDate; 
                drLocal["IsSingleDayShift"] = false;
                drLocal["UpdatedBy"] = user;
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //
            }
        }//End Function
        public static bool GetBool(string inputData)
        {

            try
            {
                if (string.IsNullOrEmpty(inputData) == true)
                {
                    return false;
                }                
                else if (string.Compare(inputData.Trim(), "NO", true) == 0)
                {
                    return false;
                }
                else if (string.IsNullOrEmpty(inputData.Trim()) == true)
                {
                    return false;
                }
                else if (string.Compare(inputData.Trim(), "0", true) == 0)
                {
                    return false;
                }
                else if (string.Compare(inputData.Trim(), "FALSE", true) == 0)
                {
                    return false;
                }
                else if (Convert.ToDouble(bplib.clsWebLib.GetNumData(inputData.Trim())) < 0)
                    return false;


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        } // End Function
        string GetPK(string colvalue)
        {
            string r = string.Empty;
            string token = "_#";
            try
            {
                //var k = colvalue;
                if (colvalue != null)
                {
                    var _index = colvalue.IndexOf(token);
                    if (_index != -1)
                    {
                        r = colvalue.Substring(_index + token.Length).Trim().Replace("\n", "").Replace("\r", "");
                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void CheckField(string L, string FieldName)
        {
            try
            {
                if (string.IsNullOrEmpty(L))
                {
                    throw new Exception(FieldName + " can not be blank...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void getCutOffDate(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" SELECT [Id]     
                              ,[PlantId]
                              ,[ModuleName]
                              ,[CutOffDate]     
                          FROM [SCS].[OpeningBalanceCutOffDate] where PlantId='" + PlantId + "' and ModuleName='HR'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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


        public void getDOJ(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT SystemId,FORMAT(DOJ,'dd-MMM-yyyy') DOJ FROM EmployeeInformation where PlantId='" + PlantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetShiftByJoblocation(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" select p.id PlantId,p.UserName Plant
                                ,j.SystemID JoblocationId ,j.SystemID JobLocation
                                ,s.SystemID ShiftSystemId,s.UserName ShiftName
                                 from ShiftDefination s
                                left join org.Plant p on p.id=s.PlantID
                                left join JobLocation j on j.PlantID=p.Id
                                where p.Id='" + PlantId+@"'
                                order by p.id";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        void GetSystemId(string plantid,out string  syspad)
        {
            syspad = string.Empty;
            try
            {               
                string _seed = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_SHIFT_ASSIGN", out _seed);
                //syspad = GetPadding(_seed);
                syspad = _seed;

                //string Prefix = null;
                //string _seed2 = string.Empty;
                //GetPlantPrefix(plantid, out Prefix);
                //objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), plantid + "EMP_BASIC", out _seed2);
                //pad = GetPadding(_seed2);
                //pad = Prefix + DateTime.Now.ToString("yy") + pad;

                
                //string _seed_jl = string.Empty;
                //objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_JOB_LOC", out _seed_jl);
                //_jlpk = GetPadding(_seed_jl);
                //_jlpk = DateTime.Now.ToString("yy") + _jlpk;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetBudgetCodeInfo(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"select b.id,b.Code,en.UnitId
                                ,p.DivisionId,p.SubDivisionId,p.DesignationId
                                ,p.DepartmentId,p.SectionId,p.SubSectionId,p.IsDirect,p.PaymentLink
                                from mst.ManpowerBudget b
                                left join org.Position p on p.id=b.PositionId
                                left join org.Entity en on en.id=b.EntityId
                                where en.PlantId='" + PlantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        //void GetDic(DataSet ds,dictonary<str>)

        public void SaveData(string groupid, string companyid, string plantid,string user, List<EmployeeShiftUploadTemplate> epList)
        {
            DataSet dsCOD = null;
            DataSet dsDOJ = null;
            DataSet dsShiftandJoblocation = null;
            DataSet dsShiftAssign_Save = null;
            DateTime COD = DateTime.Now;
            try
            {
                #region Validation on Required fields
                getDOJ(plantid, out dsDOJ);
                getCutOffDate(plantid, out dsCOD);
                GetShiftByJoblocation(plantid, out dsShiftandJoblocation);
                DataView dvShiftJob = new DataView(dsShiftandJoblocation.Tables[0]);
                //COD                    
                if (dsCOD.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("No cutt-of-Date is defined for this plant...");
                }
                else
                {
                    COD = Convert.ToDateTime(dsCOD.Tables[0].Rows[0]["CutOffDate"].ToString());
                }

                string _empids = string.Empty;
                foreach (var item in epList)
                {
                    if (_empids.Length == 0)
                    {
                        _empids = "'" + item.SystemId + "'";
                    }
                    else
                    {
                        _empids += ",'" + item.SystemId + "'";
                    }

                    
                }

                GetEmployeeShiftAssign(_empids, out dsShiftAssign_Save);
                DataView dvValidation = new DataView(dsShiftAssign_Save.Tables[0]);
                DataView dvDOJ = new DataView(dsDOJ.Tables[0]);

                foreach (var item in epList)
                {
                    CheckField(item.SystemId, "EmpSystemID");
                    CheckField(item.JobLocation, "EffectiveDate");




                    dvDOJ.RowFilter = "SystemId='" + item.SystemId + "'";
                    if (dvDOJ.Count > 0)
                    {
                        DateTime DOJ = Convert.ToDateTime(dvDOJ[0]["DOJ"]);
                        if (DOJ>COD)
                        {
                            //if (Convert.ToDateTime(item.EffectiveDate) < DOJ)
                            //{
                            //    throw new Exception("Employee [" + item.EmployeeCode + "] has smaller Effective Date [" + item.EffectiveDate + "] than DOJ [" + DOJ.ToString("dd-MMM-yyyy") + "]");
                            //}
                            item.EffectiveDate = DOJ.ToString("dd-MMM-yyyy");

                        }
                        else
                        {
                            //if (Convert.ToDateTime(item.EffectiveDate) < COD)
                            //{
                            //    throw new Exception("Employee [" + item.EmployeeCode + "] has smaller Effective Date [" + item.EffectiveDate + "] than COD [" + COD.ToString("dd-MMM-yyyy") + "]");
                            //}
                            item.EffectiveDate = COD.ToString("dd-MMM-yyyy");
                        }


                    }
                    else
                    {
                        throw new Exception("Employee [" + item.EmployeeCode + "] has No DOJ.");

                    }
                    dvDOJ.RowFilter = null;





                   
                    dvValidation.RowFilter = "EmpSystemID='" + item.SystemId + "' and EffectiveDate>'"+item.EffectiveDate+ "' and IsSingleDayShift=0";
                    if(dvValidation.Count>0)
                    {
                        throw new Exception("Employee ["+item.EmployeeCode+"] has greater Effective Date ["+item.EffectiveDate+"]");
                    }
                    dvValidation.RowFilter = null;

                    if (!bplib.clsWebLib.GetBoolData(item.IsRoster))
                    {
                        dvShiftJob.RowFilter = "JoblocationId='" + item.JobLocation + "' and ShiftSystemId='" + GetPK(item.ShiftSystemId) + "' ";
                        if (dvShiftJob.Count == 0)
                        {
                            throw new Exception("Employee [" + item.EmployeeCode + "] is assigned with wrong shift ....");
                        }
                        dvShiftJob.RowFilter = null;
                    }
                    else
                    {
                        dvShiftJob.RowFilter = "JoblocationId='" + item.JobLocation + "' and ShiftSystemId='" + GetPK(item.RosterStartShiftID) + "' ";
                        if (dvShiftJob.Count == 0)
                        {
                            throw new Exception("Employee [" + item.EmployeeCode + "] is assigned with wrong shift ....");
                        }
                        dvShiftJob.RowFilter = null;

                    }


                }

                
               
                

                #endregion
                //DataSet dsShiftAssignPKlist = null;
              
                //GetEmployeeShiftAssign_max(_empids, out dsShiftAssignPKlist);

                //for (int i = 0; i < dsShiftAssignPKlist.Tables[0].Rows.Count; i++)                
                //{
                //    string pk= dsShiftAssignPKlist.Tables[0].Rows[i]["EmpSystemID"].ToString();
                //    if (_shift_ass_pk.Length == 0)
                //    {
                //        _shift_ass_pk = "'" + pk + "'";
                //    }
                //    else
                //    {
                //        _shift_ass_pk += ",'" + pk + "'";

                //    }
                //}

                string syspad = string.Empty;
                string _systemid = string.Empty;
                DataView dvEmpShiftAss = new DataView(dsShiftAssign_Save.Tables[0]);
                GetSystemId(plantid,out syspad);

                int _count = 0;
                for (int i = 0; i < epList.Count; i++)
                {
                    var ep = epList[i];
                    _count++;
                    _systemid ="XS"+ syspad + "_" + _count;                
                    dvEmpShiftAss.RowFilter = " EmpSystemID ='"+ep.SystemId+"' AND EffectiveDate = '" + ep.EffectiveDate + @"'";
                    if (dvEmpShiftAss.Count == 0)
                    {
                        DataRow dr = dsShiftAssign_Save.Tables[0].NewRow();
                        UpdateEmployeeInformationDataRow(groupid, companyid, plantid, user,"ADDNEW", _systemid, ep, ref dr);
                        dsShiftAssign_Save.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dvEmpShiftAss[0].Row;
                        dr.BeginEdit();
                        UpdateEmployeeInformationDataRow(groupid, companyid, plantid, user, "EDIT", _systemid, ep, ref dr);
                        dr.EndEdit();
                    }
                    dvEmpShiftAss.RowFilter = null;
                }//for
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsShiftAssign_Save);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//EOF
    }
} 
