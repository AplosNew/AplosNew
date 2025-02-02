using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Service.Payrolls.SalaryProcess
{
   public class clsMaternityBenefit
    {
        ISqlRepository _sqlRepository;
        public clsMaternityBenefit(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        public clsMaternityBenefit()
        {

        }
        public void SaveMaster(MaternityBenefitMaster _MaternityBenefitMaster, decimal TotalWorkingDays,decimal TotalPayable)
        {
            DataSet dsMaster = null;
            try
            {               
              
                SaveMaternityBenefitMaster(_MaternityBenefitMaster,  TotalWorkingDays, TotalPayable, out dsMaster);
                clsStaticInfo obj = new clsStaticInfo();

                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveMasterAndDetail(MaternityBenefitMaster _MaternityBenefitMaster, IEnumerable<MaternityBenefitDetail> _MaternityBenefitDetailList,decimal TotalWorkingDays,decimal TotalPayable)
        {
            DataSet dsMaster = null;
            DataSet dsDetail = null;
            try
            {
                SaveMaternityBenefitMaster(_MaternityBenefitMaster,  TotalWorkingDays, TotalPayable, out dsMaster);
                SaveMaternityBenefitDetail(_MaternityBenefitMaster, _MaternityBenefitDetailList, TotalWorkingDays, out dsDetail);
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSetsAndDelete(_MaternityBenefitMaster.EmpSystemId, _MaternityBenefitMaster.LeaveTransactionId, dsMaster, dsDetail);
                //obj.SaveDataSets(dsMaster, dsDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveMasterAndDetailForAfter(MaternityBenefitMaster _MaternityBenefitMaster)
        {
            DataSet dsMaster = null;
          
            try
            {
                SaveMaternityBenefitMasterForAfter(_MaternityBenefitMaster, out dsMaster);             
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetMaster(string empid, string leavetransactionid, string userid, string ip,out MaternityBenefitMaster mbm)
        {
            mbm = null;
            DataSet dsLocal = null;
            try
            {
                //get leaveinfo
                //create ob
                GetLeaveInfo(leavetransactionid, empid, out dsLocal);
                if(dsLocal.Tables[0].Rows.Count>0)
                {
                    mbm = new MaternityBenefitMaster();
                    mbm.AddedBy = userid;
                    mbm.AddedFromIP = ip;
                    mbm.AfterAmount = 0;
                    //  mbm.AfterPaymentDate = ;
                    mbm.BeforeAmount = 0;
                    //mbm.BeforePaymentDate = null;
                    mbm.EmpSystemId = empid;
                    mbm.IsPaidAfter = false;
                    mbm.IsPaidBefore = false;
                    mbm.LeaveDays =Convert.ToDecimal(dsLocal.Tables[0].Rows[0]["LeaveDays"].ToString());
                    mbm.LeaveTransactionId = leavetransactionid;
                    mbm.PlantId = dsLocal.Tables[0].Rows[0]["PlantId"].ToString();
                    mbm.UpdatedBy = userid;
                    //mbm.UpdatedDate = null;
                    mbm.UpdatedFromIP = ip;
                    mbm.WageRate = 0;
                }
                else
                {
                    throw new Exception("No leave found...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetDetail(string empid, string leavetransactionid, string userid, string ip,out IEnumerable<MaternityBenefitDetail> dList)
        {
            dList = null;
            try
            {
                //sal innfo
                //

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public void xSaveMasterAndDetail(string empid, string leavetransactionid, string userid,string ip)
        //{
        //    DataSet dsMaster = null;
        //    DataSet dsDetail = null;
        //    MaternityBenefitMaster mbm = null;
        //    IEnumerable<MaternityBenefitDetail> dList = null;
        //    try
        //    {
        //        GetMaster(empid, leavetransactionid, userid, ip,out mbm);
        //        GetDetail(empid, leavetransactionid, userid, ip,out dList);
        //        SaveMaternityBenefitMaster(mbm, out dsMaster);
        //        SaveMaternityBenefitDetail(mbm, dList, out dsDetail);
        //        clsStaticInfo obj = new clsStaticInfo();
        //        obj.SaveDataSets(dsMaster, dsDetail);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        void SaveMaternityBenefitMaster(MaternityBenefitMaster _MaternityBenefitMaster, decimal TotalWorkingDays,decimal TotalPayable, out DataSet dsMaster)
        {
            dsMaster = null;
            try
            {
                MaternityBenefitMaster(_MaternityBenefitMaster.Id, out dsMaster);
                _MaternityBenefitMaster.TotalWorkingDays = TotalWorkingDays;
                _MaternityBenefitMaster.TotalPayable = TotalPayable;
                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "Id='" + _MaternityBenefitMaster.Id + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "MaternityBenefitMaster", out sID);
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    _MaternityBenefitMaster.Id = "BM" + sID;
                    _MaternityBenefitMaster.AddedDate = System.DateTime.Now;
                    _MaternityBenefitMaster.UpdatedDate = System.DateTime.Now;

                    foreach (PropertyInfo prop in _MaternityBenefitMaster.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(_MaternityBenefitMaster, null));
                    }

                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();
                    _MaternityBenefitMaster.UpdatedDate = System.DateTime.Now;
                    foreach (PropertyInfo prop in _MaternityBenefitMaster.GetType().GetProperties())
                    {
                        if (prop.Name == nameof(_MaternityBenefitMaster.IsPaidBefore) || prop.Name == nameof(_MaternityBenefitMaster.BeforePaymentDate))
                        {
                            SetRowValue(ref dr, prop.Name, prop.GetValue(_MaternityBenefitMaster, null));
                        }
                        SetRowValue(ref dr, _MaternityBenefitMaster.UpdatedDate);
                        SetRowValue(ref dr, _MaternityBenefitMaster.UpdatedFromIP);
                        SetRowValue(ref dr, _MaternityBenefitMaster.UpdatedBy);
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


        void SaveMaternityBenefitMasterForAfter(MaternityBenefitMaster _MaternityBenefitMaster,out DataSet dsMaster)
        {
            dsMaster=null;
            try
            {
                MaternityBenefitMaster(_MaternityBenefitMaster.Id, out dsMaster);

                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "Id='" + _MaternityBenefitMaster.Id + "' ";
                if (dvMaster.Count == 0)
                {
                   
                }
                else
                {
                    #region edit                    
                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();
                    _MaternityBenefitMaster.UpdatedDate = System.DateTime.Now;
                    foreach (PropertyInfo prop in _MaternityBenefitMaster.GetType().GetProperties())
                    {
                        if (prop.Name == nameof(_MaternityBenefitMaster.IsPaidBefore) || prop.Name == nameof(_MaternityBenefitMaster.BeforePaymentDate) || prop.Name==nameof(_MaternityBenefitMaster.IsPaidAfter)||prop.Name==nameof(_MaternityBenefitMaster.AfterPaymentDate))
                        {
                            SetRowValue(ref dr, prop.Name, prop.GetValue(_MaternityBenefitMaster, null));
                        }
                        
                    }                    
                        SetRowValue(ref dr, nameof(_MaternityBenefitMaster.UpdatedDate), _MaternityBenefitMaster.UpdatedDate);
                        SetRowValue(ref dr, nameof(_MaternityBenefitMaster.UpdatedFromIP), _MaternityBenefitMaster.UpdatedFromIP);
                        SetRowValue(ref dr, nameof(_MaternityBenefitMaster.UpdatedBy), _MaternityBenefitMaster.UpdatedBy);                  
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
        void SaveMaternityBenefitDetail(MaternityBenefitMaster master, IEnumerable<MaternityBenefitDetail> _MaternityBenefitDetailList,decimal TotalWorkingDays, out DataSet dsDetail)
        {
            dsDetail = null;
            string _pk = string.Empty;
            try
            {
                MaternityBenefitDetail(master.Id, out dsDetail);
                string sID = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "MaternityBenefitDetail", out sID);
                int Count = 0;
                foreach (var detailItem in _MaternityBenefitDetailList)
                {
                   // detailItem.TotalWorkingDays = TotalWorkingDays;
                    Count++;
                       DataView dvDetail = new DataView(dsDetail.Tables[0]);
                    dvDetail.RowFilter = "MaternityBenefitMasterId='"+ detailItem.MaternityBenefitMasterId + "'";
                    if (dvDetail.Count == 0)
                    {
                        #region add
                        _pk = "BD" + sID+"-"+Count;
                        DataRow dr = dsDetail.Tables[0].NewRow();

                        //detailItem.Id = _pk;
                        //detailItem.AddedDate = System.DateTime.Now;
                        //detailItem.UpdatedDate = System.DateTime.Now;
                        //detailItem.MaternityBenefitMasterId = master.Id;
                        //detailItem.AddedBy = master.AddedBy;
                        //detailItem.AddedFromIP = master.AddedFromIP;

                        //foreach (PropertyInfo prop in detailItem.GetType().GetProperties())
                        //{
                        //    SetRowValue(ref dr, prop.Name, prop.GetValue(detailItem, null));
                        //}





                        dr["Id"] = _pk;
                        dr["MaternityBenefitMasterId"] = master.Id;
                        dr["SalaryProcessMasterId"] = detailItem.SalaryProcessMasterId;
                        dr["YearNo"] = detailItem.YearNo;
                        dr["MonthNo"] = detailItem.MonthNo;
                        dr["WorkingDays"] = detailItem.WorkingDays;
                        dr["StructureAmount"] = detailItem.TotalGross;
                        dr["EarnedAmount"] = detailItem.NetPay;
                        dr["BonusAmount"] = detailItem.BonusAmount;
                        dr["EncashAmount"] = detailItem.EncashAmount;
                        dr["OtherAmount"] = detailItem.OtherAmount;
                        dr["AddedBy"] = master.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now;
                        dr["AddedFromIP"] = master.AddedFromIP;
                        dr["UpdatedBy"] = master.AddedBy;
                        dr["UpdatedDate"] = System.DateTime.Now;
                        dr["UpdatedFromIP"] = master.AddedFromIP;
                        dr["TotalEarnedAmount"] = detailItem.TotalEarnedAmount;

                        dsDetail.Tables[0].Rows.Add(dr);
                        #endregion
                    }
                    else
                    {
                        DataRow dr = dvDetail[0].Row;
                        dr.BeginEdit();
                        dr["MaternityBenefitMasterId"] = master.Id;
                        dr["SalaryProcessMasterId"] = detailItem.SalaryProcessMasterId;
                        dr["YearNo"] = detailItem.YearNo;
                        dr["MonthNo"] = detailItem.MonthNo;
                        dr["WorkingDays"] = detailItem.WorkingDays;
                        dr["StructureAmount"] = detailItem.TotalGross;
                        dr["EarnedAmount"] = detailItem.NetPay;
                        dr["BonusAmount"] = detailItem.BonusAmount;
                        dr["EncashAmount"] = detailItem.EncashAmount;
                        dr["OtherAmount"] = detailItem.OtherAmount;
                        dr["UpdatedBy"] = master.AddedBy;
                        dr["UpdatedDate"] = System.DateTime.Now;
                        dr["UpdatedFromIP"] = master.AddedFromIP;
                        dr["TotalEarnedAmount"] = detailItem.TotalEarnedAmount;
                        dr.EndEdit();
                    }




                    dvDetail.RowFilter = null;
                }//for each
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        string GetMonthList(string LeaveStartDate)
        {
            string WC = " Where ";
            try
            {
                var v = Convert.ToDateTime(LeaveStartDate).AddMonths(-3);
                WC += "(YearNo=" + v.ToString("yyyy") + " and MonthNo=" + v.ToString("MM") + ")";
                var v2 = Convert.ToDateTime(v).AddMonths(1);
                WC += " or (YearNo=" + v2.ToString("yyyy") + " and MonthNo=" + v2.ToString("MM") + ")";
                var v3 = Convert.ToDateTime(v2).AddMonths(1);
                WC += " or (YearNo=" + v3.ToString("yyyy") + " and MonthNo=" + v3.ToString("MM") + ")";
                return WC;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string GetPreviousMonthList(string LeaveStartDate)
        {
            string WC = " Where ";
            try
            {
                var v = Convert.ToDateTime(LeaveStartDate).AddMonths(-1);
                WC += "(YearNo=" + v.ToString("yyyy") + " and MonthNo=" + v.ToString("MM") + ")";
                
                return WC;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> xxShowSalaryInfo(string EmpSystemId, string LeaveStartDate)
        {
            try
            {
                string WC = string.Empty;
                WC = GetMonthList(LeaveStartDate);
              var sql = @"select 
                            YearNo,MonthNo,str(YearNo)+'-'+[MonthName] [MonthName],SalaryProcessMasterId
                            ,isnull(StructureAmount,0) StructureAmount,isnull(EarnedAmount,0) EarnedAmount,WorkingDays
                            ,EffectiveDate
                            ,isnull(BonusAmount,0) BonusAmount
                            ,TotalEarnedAmount=isnull(EarnedAmount,0)+isnull(BonusAmount,0)+isnull(OtherAmount,0)
                            ,EncashAmount,OtherAmount
                             from (--x
                            select m.MonthNo,m.YearNo
                            ,DateName( month , DateAdd( month , m.MonthNo , -1 )) [MonthName]
                            ,h.SalaryHead,c.EntryAmount StructureAmount,c.DisbusmentAmount 
                            --,att.TotalProcDate,att.TotalAbsent,att.TotalHoliDay,att.TotalLWP,att.TotalWeekOff
                            ,WorkingDays=att.TotalProcDate-att.TotalAbsent-att.TotalHoliDay-att.TotalLWP-att.TotalWeekOff
                            ,b.BonusAmount,b.EffectiveDate
                            ,c.SlrProcMstSystemID SalaryProcessMasterId,convert(decimal,0) OtherAmount,convert(decimal,0) EncashAmount
                            ,tg.DisbusmentAmount TotalGross
                            ,np.DisbusmentAmount EarnedAmount
                            from SalaryProcChild c
                            inner join (select * from SalaryHead where HeadCategory in( 'Gross')) h on h.SalaryHeadID=c.SalaryHeadID
                            left join
                            (
                            select * from SalaryProcChild where
                            SalaryHeadID in (select SalaryHeadID from SalaryHead where HeadCategory in( 'TOTAL GROSS'))
                            )tg on c.SlrProcMstSystemID=tg.SlrProcMstSystemID and tg.empinfosystemid='" + EmpSystemId + @"'  

                            left join
                            (
                            select * from SalaryProcChild where
                            SalaryHeadID in (select SalaryHeadID from SalaryHead where HeadCategory in( 'Net Payable'))
                            )np on c.SlrProcMstSystemID=np.SlrProcMstSystemID and np.empinfosystemid='" + EmpSystemId + @"'  

                            left join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
                            left join SalaryProceAttdnData att on att.SlrProcMstSystemID=m.SystemID and att.EmpSystemID='" + EmpSystemId + @"'                           
                            left join (select c.*,m.EffectiveDate from BonusPaymentActual c
                            left join [BonusPaymentActualMaster] m on m.SystemID=c.BnsMstSystemID
                            )b on b.EmpSystemID='" + EmpSystemId + @"' and month(b.EffectiveDate)=m.MonthNo
                            where c.SlrProcMstSystemID in (
                            select SystemID from SalaryProcMaster " + WC + @"
                            )
                            and c.EmpInfoSystemID='" + EmpSystemId + @"'
                            ) x";

                var data = _sqlRepository.GetDataCollection(sql);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> ShowSalaryInfo(string EmpSystemId, string LeaveStartDate)
        {
            try
            {
                string WC = string.Empty;
                WC = GetPreviousMonthList(LeaveStartDate);
                var sql = @"select
                            YearNo,MonthNo,str(YearNo)+'-'+[MonthName] [MonthName],SalaryProcessMasterId
                            ,isnull(StructureAmount,0) Gross
                            from (--x
                            select m.MonthNo,m.YearNo
                            ,DateName( month , DateAdd( month , m.MonthNo , -1 )) [MonthName]
                            ,c.EntryAmount StructureAmount,c.DisbusmentAmount 
                            ,c.SlrProcMstSystemID SalaryProcessMasterId
	                      
                            from
                            (
                            select * from SalaryProcChild where
                            SalaryHeadID in (select SalaryHeadID from SalaryHead where HeadCategory in( 'Gross'))
                            )c                           
                           
                            left join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
                            left join SalaryProceAttdnData att on att.SlrProcMstSystemID=m.SystemID and att.EmpSystemID='" + EmpSystemId + @"'
                            left join (select c.*,m.EffectiveDate from BonusPaymentActual c left join [BonusPaymentActualMaster] m on m.SystemID=c.BnsMstSystemID
                            )b on b.EmpSystemID='" + EmpSystemId + @"'  and month(b.EffectiveDate)=m.MonthNo and YEAR(b.EffectiveDate)=m.YearNo
                            
                            where
                            c.SlrProcMstSystemID in (
                            select SystemID from SalaryProcMaster " + WC + @"
                            )
                            and c.EmpInfoSystemID='" + EmpSystemId + @"'
                            ) x";

                var data = _sqlRepository.GetDataCollection(sql);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<Dictionary<string, object>> ShowSalaryInfoAfter(string EmpSystemId, string LeaveTransactionId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select  convert(varchar,mbd.YearNo) +'-'+ DateName(  MONTH , DateAdd( month , mbd.MonthNo, -1))[MonthName]
                            ,mbd.WorkingDays,mbd.StructureAmount,mbd.EarnedAmount,mbd.BonusAmount,mbd.EncashAmount,mbd.OtherAmount     
                            ,(mbd.EarnedAmount+mbd.OtherAmount+mbd.BonusAmount)Total 
	                        ,mbd.TotalEarnedAmount
                                from [dbo].[MaternityBenefitDetail] mbd
                                left join MaternityBenefitMaster mbm on mbm.Id= mbd.MaternityBenefitMasterId
                                left join SalaryProcChild spc on spc.SlrProcMstSystemID=mbm.EmpSystemId 
                                left join SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
                                left join SalaryHead h on h.SalaryHeadID=spc.SalaryHeadID
                                WHERE mbm.EmpSystemId='" + EmpSystemId + @"' and mbm.PlantId='"+identity.PlantId+ @"' AND mbm.LeaveTransactionId='"+ LeaveTransactionId + @"'
                                order by mbd.MonthNo";                           
                var data = _sqlRepository.GetDataCollection(sql);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<Dictionary<string, object>> GetLavelValue(string EmpSystemId, string LeaveTransactionId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select WorkingDays,mbm.AdditionalAmount,mbm.AdditionalAmountAfter,mbm.AdditionalAmountBefore,mbm.AfterAmount,mbm.BeforeAmount,mbm.Remark,mbm.WageRate ,mbm.TotalWorkingDays 
                                ,mbm.TotalPayable
                            ,mbm.Id,mbm.IsPaidAfter,mbm.IsPaidBefore,FORMAT(mbm.AfterPaymentDate,'dd-MMM-yyyy')AfterPaymentDate
                            ,FORMAT(mbm.BeforePaymentDate,'dd-MMM-yyyy')BeforePaymentDate
                                from  MaternityBenefitMaster mbm 
								left join(select sum(WorkingDays) WorkingDays
                            --,SUM(BonusAmount+EarnedAmount+OtherAmount) TotalAmount
                            ,MaternityBenefitMasterId from [dbo].[MaternityBenefitDetail]
								group by MaternityBenefitMasterId
								) mbd on mbm.Id= mbd.MaternityBenefitMasterId                              
                                WHERE mbm.EmpSystemId='" + EmpSystemId+ @"' and  mbm.PlantId='"+ identity.PlantId+ @"' and mbm.LeaveTransactionId='"+LeaveTransactionId+@"'";

                var data = _sqlRepository.GetDataCollection(sql);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void SetRowValue(ref DataRow dr,string Field,object v)
        {
            try
            {
                if(v is null)
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
        void SetRowValue(ref DataRow dr, object v)
        {
            try
            {
                dr[nameof(v)] = v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void MaternityBenefitMaster(string pk, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[MaternityBenefitMaster] WHERE id  = '" + pk + @"'";

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
        void MaternityBenefitDetail(string MaternityBenefitMasterId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                                FROM [dbo].[MaternityBenefitDetail] WHERE MaternityBenefitMasterId  = '" + MaternityBenefitMasterId + @"'";

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
        void GetLeaveInfo(string leavePK,string EmpSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                                declare @emp varchar(30)
                                set @emp='" + EmpSystemId + @"'
                                SELECT  EI.SystemId
                                        ,EI.EmployeeCode
                                        ,EI.EmployeeName
                                        , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                                        , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                                        , DG.UserName GivenDesignation
                                        , DP.UserName Department
                                        , DSG.UserName LegalDesignation
		                                ,s.UserName Section
		                                ,ss.UserName Subsection
		                                ,ll.UserName Line
		                                ,format(t.fromdate,'dd-MMM-yyyy') LeaveStartDate
		                                ,t.LeaveDays,ei.PlantId
		                                ,mp.IsBefore,mp.BeforePercentage,mp.IsAfter,mp.AfterPercentage
		                                ,x.TotalDays,x.TotalEarn,Rate=isnull(x.TotalEarn,0)/isnull(x.TotalDays,0)
		                                ,TotlaEarning=(isnull(x.TotalEarn,0)/isnull(x.TotalDays,0))*t.LeaveDays
		                                ,BeforePercentageAmount=((isnull(x.TotalEarn,0)/isnull(x.TotalDays,0))*t.LeaveDays)*mp.BeforePercentage/100
		                                ,AfterPercentageAmount=((isnull(x.TotalEarn,0)/isnull(x.TotalDays,0))*t.LeaveDays)*mp.AfterPercentage/100

                                        FROM dbo.Employeeinformation EI
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                        LEFT JOIN HKP.LegalDesignation DSG ON ei.LegalDesignationId=DSG.Id
                                        LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                        LEFT JOIN ORG.Department DP on DP.Id=pr.DepartmentId							
                                        LEFT JOIN org.Section s ON s.id=pr.SectionId
                                        LEFT JOIN org.SubSection ss ON ss.Id=pr.SubSectionId
		                                left join org.Line ll on ll.id=mb.LineId
		                                left join (select * FROM LeaveTransaction where SystemID ='" + leavePK + @"') t on t.EmpSystemID=ei.SystemId
		                                left join mst.MaternityLeavePolicy mp on mp.id=t.MaternityLeavePolicyId		                                
		                                where ei.SystemId =@emp";

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

    public class MaternityBenefitMaster
    {        	
		 public string Id { get; set; }
         public string LeaveTransactionId { get; set; }
         public string PlantId { get; set; }
         public string EmpSystemId { get; set; }
         public decimal WageRate { get; set; }
         public decimal LeaveDays { get; set; }
         public decimal BeforeAmount { get; set; }
         public DateTime? BeforePaymentDate { get; set; }
         public bool IsPaidBefore { get; set; }
         
         public decimal AfterAmount { get; set; }
        public decimal TotalWorkingDays { get; set; }
        public decimal TotalPayable { get; set; }
        public decimal AdditionalAmount { get; set; }
         public decimal AdditionalAmountBefore { get; set; }
         public decimal AdditionalAmountAfter { get; set; }
         public decimal Deduction { get; set; }
         public string Remark { get; set; }
         public DateTime? AfterPaymentDate { get; set; }
         public bool IsPaidAfter { get; set; }
         [NeverUpdate]
         public string AddedBy { get; set; }
         [NeverUpdate]
         public DateTime AddedDate { get; set; }
         [NeverUpdate]
         public string AddedFromIP { get; set; }
         public string UpdatedBy { get; set; }
         public DateTime? UpdatedDate { get; set; }
         public string UpdatedFromIP { get; set; }

       

        
    }
    public class MaternityBenefitDetail
    {
        public string Id { get; set; }
        public string MaternityBenefitMasterId { get; set; }
        public string SalaryProcessMasterId { get; set; }
        public int YearNo { get; set; }
        public int MonthNo { get; set; }
        public decimal Advance { get; set; }
        public decimal WorkingDays { get; set; }
        public decimal StructureAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal EncashAmount { get; set; }
        public decimal OtherAmount { get; set; }
        public decimal TotalEarnedAmount { get; set; }
        public decimal EarnedAmount { get; set; }
        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        public decimal TotalGross { get; set; }
        public decimal NetPay { get; set; }

        //public decimal TotalAmount
        //{
        //    get
        //    {
        //        return this.BonusAmount+this.EncashAmount+this.EarnedAmount+this.OtherAmount;
        //    }
        //}
    }
}
