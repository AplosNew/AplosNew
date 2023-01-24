using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectionManager;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Service.Extension;
using Library.Service.EmployeeServices;

namespace HRService
{
    public class clsDataContext
    {


        public void SaveDataSets(params System.Data.DataSet[] dsRef)
        {

            clsConnectionManager objCon = null;
            try
            {
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveData(ref dsRef[i]);
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }

        } // End Function
        public void sampleSearch(string fromDate, string todate, string entityID, string strKey, out System.Data.DataSet dsRef)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            string strfromdate = "";
            string strtodate = "";
            try
            {
                if (strKey == "")
                {
                    strSQL = @"SELECT SystemID, refNo, left(replace(upper(convert(varchar,InvoiceDate,113)),' ','-'),11) as InvoiceDate, 
                                Narration, RefID,  ISNULL(Amount,0) AS Amount
                                FROM Voucher 
                                WHERE " + strKey + " AND entityID='" + entityID + @"'
                                ORDER BY refNo";

                }

                else
                {
                    strSQL = @"SELECT SystemID, refNo, left(replace(upper(convert(varchar,InvoiceDate,113)),' ','-'),11) as InvoiceDate, 
                                Narration, RefID,  ISNULL(Amount,0) AS Amount
                                FROM Voucher 
                                WHERE " + strKey + " AND entityID='" + entityID + @"'
                                ORDER BY refNo";

                }

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();


            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        public void sampleData(string strEntityID, string refNo, out System.Data.DataSet dsRef)
        {
            string strSQL;
            clsConnectionManager objCon;
            try
            {
                strSQL = @"SELECT V.refNo, ii.LotNo, ii.ItemID, IC.ItemDescription, ItemCode,
                            ISNULL(II.Rate,0) AS Rate, ISNULL(II.Quantity,0) AS Quantity, 
                            SUM(ISNULL(II.UpQuantity,0)) AS UpQuantity, SUM(ISNULL(II.DownQuantity,0)) AS DownQuantity 
                            FROM Voucher AS V
                            LEFT OUTER JOIN InventoryItems AS II ON V.SystemID=II.VoucherSystemID
                            LEFT OUTER JOIN ItemChild AS IC ON II.ItemID=IC.SystemID
                            WHERE refNo='" + refNo + @"' AND V.EntityID='" + strEntityID + @"'
                            GROUP BY V.refNo, ii.LotNo, ii.ItemID, IC.ItemDescription, ItemCode,
                            II.Rate, Quantity";


                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void sampleDelete(string strEntityID, string strItemID)
        {
            clsConnectionManager objCon = null;
            try
            {
                objCon = new clsConnectionManager();

                objCon.BeginTransaction();

                objCon.executeQuery("DELETE FROM ItemChild WHERE EntityID = '" + strEntityID + "' AND SystemID = '" + strItemID + "'");

                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {

                objCon = null;
            }
        }//End of function


        public void getCompanyList(out List<CompanyList> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<CompanyList>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT c.id as CompanyID, c.CODE, c.UserName AS  CompanyName, c.UserName AS ShortName, c.UserName AS FullName, c.UserName AS Title,
                       am.Address1, am.Address2, am.Address3, am.Phone,
                       am.Email
                  FROM ORG.Company c
                  LEFT OUTER JOIN mst.AddressMaster AS am ON am.Id=c.AddressMasterId";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new CompanyList
                    {
                        CompanyID = dsRef.Tables[0].Rows[i]["CompanyID"].ToString(),
                        CODE = dsRef.Tables[0].Rows[i]["CODE"].ToString(),
                        CompanyName = dsRef.Tables[0].Rows[i]["CompanyName"].ToString(),
                        ShortName = dsRef.Tables[0].Rows[i]["ShortName"].ToString(),
                        FullName = dsRef.Tables[0].Rows[i]["FullName"].ToString(),
                        Title = dsRef.Tables[0].Rows[i]["Title"].ToString(),
                        Address1 = dsRef.Tables[0].Rows[i]["Address1"].ToString(),
                        Address2 = dsRef.Tables[0].Rows[i]["Address2"].ToString(),
                        Address3 = dsRef.Tables[0].Rows[i]["Address3"].ToString(),
                        Phone = dsRef.Tables[0].Rows[i]["Phone"].ToString(),
                        Email = dsRef.Tables[0].Rows[i]["Email"].ToString(),
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        // Written by Nitesh
        #region Written By Nitesh
        public void getWorkcenter(out List<WorkCenterList> DataList , string processid)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<WorkCenterList>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select WM.StandardName Text, WM.Id Value from SCS.WorkCenterMaster WM                          
                            where WM.ProcessId = '" + processid + "'order by Text";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new WorkCenterList
                    {
                        Text = dsRef.Tables[0].Rows[i]["Text"].ToString(),
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void getDepartment(out List<DepartmentList> DataList, string detentionid)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<DepartmentList>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct DMD.DepartmentId Value, D.UserName Text from org.Department D
                        left join dbo.DetentionMasterDepartment DMD on DMD.DepartmentId = D.Id
                        left join dbo.DetentionMaster DM on DM.Id = DMD.DetentionMasterId
                        where DM.DetentionTypeId = '" + detentionid+ "'order by Text";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new DepartmentList
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Text = dsRef.Tables[0].Rows[i]["Text"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void getAllDepartment(out List<AllDepartmentList> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<AllDepartmentList>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id Value, UserName Text from ORG.Department";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new AllDepartmentList
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Text = dsRef.Tables[0].Rows[i]["Text"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }


        #region MyAppIcon Default
        public void getmyappicon(out List<DefaultMyAppIconList> DataList, string userid, string Iconid)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<DefaultMyAppIconList>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select AR.Id RoleId, AR.Name Role, ARD.IconId, ARD.ModuleId,  ARM.EmployeeId, U.FullName,
 U.UserId , AR.Active
from 
SEC.AppRole AR
left join SEC.AppRoleDetail ARD on ARD.RoleId = AR.Id
left join SEC.AppRoleMapping ARM on ARM.RoleId = AR.Id
left join SEC.[User] U on U.Id = ARM.UserId
left join dbo.MobileAppIcon MA on MA.Id = ARD.ModuleId
left join dbo.MobileAppModule MAM on MAM.Id = MA.ModuleId
where FullName != 'null'  and U.UserId = '" + userid + "' and IconId = '" + Iconid + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new DefaultMyAppIconList
                    {
                        RoleId = dsRef.Tables[0].Rows[i]["RoleId"].ToString(),
                        ModuleId = dsRef.Tables[0].Rows[i]["ModuleId"].ToString(),
                        IconID = dsRef.Tables[0].Rows[i]["IconID"].ToString(),
                        Role = dsRef.Tables[0].Rows[i]["Role"].ToString(),
                        EmployeeId = dsRef.Tables[0].Rows[i]["EmployeeId"].ToString(),
                        FullName = dsRef.Tables[0].Rows[i]["FullName"].ToString(),
                        UserID = dsRef.Tables[0].Rows[i]["UserID"].ToString(),
                        Active = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["Active"]),
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void getModuleaccess(out List<DefaultMyAppIconList> DataList, string userid, string Moduleid)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<DefaultMyAppIconList>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select AR.Id RoleId, AR.Name Role, ARD.IconId, ARD.ModuleId,  ARM.EmployeeId, U.FullName,
 U.UserId , AR.Active
from 
SEC.AppRole AR
left join SEC.AppRoleDetail ARD on ARD.RoleId = AR.Id
left join SEC.AppRoleMapping ARM on ARM.RoleId = AR.Id
left join SEC.[User] U on U.Id = ARM.UserId
left join dbo.MobileAppIcon MA on MA.Id = ARD.ModuleId
left join dbo.MobileAppModule MAM on MAM.Id = MA.ModuleId
where FullName != 'null'  and U.UserId = '" + userid + "' and ARD.ModuleId = '" + Moduleid + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new DefaultMyAppIconList
                    {
                        RoleId = dsRef.Tables[0].Rows[i]["RoleId"].ToString(),
                        ModuleId = dsRef.Tables[0].Rows[i]["ModuleId"].ToString(),
                        IconID = dsRef.Tables[0].Rows[i]["IconID"].ToString(),
                        Role = dsRef.Tables[0].Rows[i]["Role"].ToString(),
                        EmployeeId = dsRef.Tables[0].Rows[i]["EmployeeId"].ToString(),
                        FullName = dsRef.Tables[0].Rows[i]["FullName"].ToString(),
                        UserID = dsRef.Tables[0].Rows[i]["UserID"].ToString(),
                        Active = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["Active"]),
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        #endregion MyAppIcon Default

        public void getDetentionType(out List<DetentionTypeList> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<DetentionTypeList>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct DT.UserName As DetentionType, DT.Id As DetentionTypeId from DetentionMasterDepartment DD
                        left join DetentionMaster DM ON DM.Id=DD.DetentionMasterId
                        left join hkp.DetentionType DT ON DT.id=DM.DetentionTypeId
                        order by UserName";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new DetentionTypeList
                    {
                        DetentionType = dsRef.Tables[0].Rows[i]["DetentionType"].ToString(),
                        DetentionTypeId = dsRef.Tables[0].Rows[i]["DetentionTypeId"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        public void GetQualification(out List<QualificationList> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<QualificationList>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id, StandardName from HKP.QualificationMaster";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new QualificationList
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        StandardName = dsRef.Tables[0].Rows[i]["StandardName"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        public void GetDetentionResponsible(out List<DetentionResponsiblePersonList> DataList, string detentiontypeid)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<DetentionResponsiblePersonList>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct E.SystemId as ResponsiblePersonId, E.CellPhnNo ,E.EmployeeCode,E.EmployeeName as ResponsiblePerson,DEP.UserName AS Department,S.UserName as Section,
                           SS.UserName as SubSection,DEG.UserName AS [LegalDesignation]
                           --CAST (CASE WHEN DLRP.Id IS NULL THEN 0 ELSE 1 END AS bit) chk, DLRP.isActive
                           from DetentionMasterResponsible DR
                           left join EmployeeInformation AS E ON E.SystemId=DR.ResponsibleMasterId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=E.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.id=E.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=E.SectionId
                            LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
                            --Left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.ResponsiblePersonId = E.SystemId
                            left join dbo.DetentionMaster DM on DM.Id = DR.DetentionMasterId
                            left join hkp.DetentionType DT on DT.Id = DM.DetentionTypeId
                            where DT.Id = '"+ detentiontypeid +"'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new DetentionResponsiblePersonList
                    {
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        CellPhnNo = dsRef.Tables[0].Rows[i]["CellPhnNo"].ToString(),
                        EmployeeCode = dsRef.Tables[0].Rows[i]["EmployeeCode"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),
                        Section = dsRef.Tables[0].Rows[i]["Section"].ToString(),
                        SubSection = dsRef.Tables[0].Rows[i]["SubSection"].ToString(),
                        LegalDesignation = dsRef.Tables[0].Rows[i]["LegalDesignation"].ToString(),
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }


        public void GetIssueByNo(out List<DetentionIssueByNo> DataList, string EmployeeId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<DetentionIssueByNo>();

            System.Data.DataSet dsRef;
            try
            {
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strSQL = @"select E.CellPhnNo IssueByNo from EmployeeInformation E
                                where E.SystemId = '" + EmployeeId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new DetentionIssueByNo
                    {
                        IssueByNo = dsRef.Tables[0].Rows[i]["IssueByNo"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        // for myapp default list
        public void GetMyAppDefault(out List<MyAppDefaultlist> DataList, string IconName)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<MyAppDefaultlist>();

            System.Data.DataSet dsRef;
            try
            {
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strSQL = @"select * from dbo.MyAppDefalt where IconName = '" + IconName + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new MyAppDefaultlist
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        MenuName = dsRef.Tables[0].Rows[i]["MenuName"].ToString(),
                        IconName = dsRef.Tables[0].Rows[i]["IconName"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetDetentionLogGrid(out List<DetentionLogGridList> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<DetentionLogGridList>();

            System.Data.DataSet dsRef;
            try
            {
                #region cmnt


                strSQL = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,DL.AddedDate, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate, DL.UpdateRemarks,
                            HK.UserName Process,  HK.Id ProcessId, DL.AddedBy, DL.AddedDate, DL.AddedFromIP
                            , DP.UserName Department, DL.DepartmentId,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id  and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							 STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
							left join HKP.Process HK on HK.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                            where isClose = 0";
                #endregion cmnt
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new DetentionLogGridList
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        WorkCenter = dsRef.Tables[0].Rows[i]["WorkCenter"].ToString(),
                        DetentionType = dsRef.Tables[0].Rows[i]["DetentionType"].ToString(),
                        LoginTime = dsRef.Tables[0].Rows[i]["LoginTime"].ToString(),
                        IssueByNo = dsRef.Tables[0].Rows[i]["IssueByNo"].ToString(),
                        ResponsiblePersonName = dsRef.Tables[0].Rows[i]["ResponsiblePersonName"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        ContactNo = dsRef.Tables[0].Rows[i]["ContactNo"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        WorkCenterId = dsRef.Tables[0].Rows[i]["WorkCenterId"].ToString(),
                        DetentionTypeId = dsRef.Tables[0].Rows[i]["DetentionTypeId"].ToString(),
                        isClose = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["isClose"]),
                        isUpdate = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["isUpdate"]),
                        Process = dsRef.Tables[0].Rows[i]["Process"].ToString(),
                        ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                        AddedFromIP = dsRef.Tables[0].Rows[i]["AddedFromIP"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DLRPId = dsRef.Tables[0].Rows[i]["DLRPId"].ToString(),
                        Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),
                        DepartmentId = dsRef.Tables[0].Rows[i]["DepartmentId"].ToString(),
                        UpdateRemarks = dsRef.Tables[0].Rows[i]["UpdateRemarks"].ToString(),
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        #region Aman
        public void GetDetentionLogDetail(out List<GetDetentionclose> DataList , string from, string to, string departmentId, string detentionTypeId)
        {
            clsConnectionManager objCon = null;
                string strSQL = "";
                DataList = new List<GetDetentionclose>();

                System.Data.DataSet dsRef;
                try
                {
                #region cmnt

                
                    strSQL = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,  DL.UpdateRemarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime), 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DepartmentId = '" + departmentId + @"'
								and DL.DetentionTypeId = '" + detentionTypeId + "' and  DL.isClose = 1";
                
                    #endregion cmnt
                    objCon = new clsConnectionManager();
                    objCon.BeginTransaction();
                    objCon.getDataSet(strSQL, out dsRef);
                    objCon.CommitTransaction();
                    for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                    {
                        DataList.Add(new GetDetentionclose
                        {
                            Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                            WorkCenter = dsRef.Tables[0].Rows[i]["WorkCenter"].ToString(),
                            DetentionType = dsRef.Tables[0].Rows[i]["DetentionType"].ToString(),
                            LoginTime = dsRef.Tables[0].Rows[i]["LoginTime"].ToString(),
                            IssueByNo = dsRef.Tables[0].Rows[i]["IssueByNo"].ToString(),
                            ResponsiblePersonName = dsRef.Tables[0].Rows[i]["ResponsiblePersonName"].ToString(),
                            ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                            ContactNo = dsRef.Tables[0].Rows[i]["ContactNo"].ToString(),
                            Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                            WorkCenterId = dsRef.Tables[0].Rows[i]["WorkCenterId"].ToString(),
                            DetentionTypeId = dsRef.Tables[0].Rows[i]["DetentionTypeId"].ToString(),
                            isClose = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["isClose"]),
                            isUpdate = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["isUpdate"]),
                            Process = dsRef.Tables[0].Rows[i]["Process"].ToString(),
                            ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                            AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                            AddedFromIP = dsRef.Tables[0].Rows[i]["AddedFromIP"].ToString(),
                            AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                            AddedTime = dsRef.Tables[0].Rows[i]["AddedTime"].ToString(),
                            LogoutDate = dsRef.Tables[0].Rows[i]["LogoutDate"].ToString(),
                            LogoutTime = dsRef.Tables[0].Rows[i]["LogoutTime"].ToString(),
                            Duration = dsRef.Tables[0].Rows[i]["Duration"].ToString(),
                            DLRPId = dsRef.Tables[0].Rows[i]["DLRPId"].ToString(),
                            Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),
                            DepartmentId = dsRef.Tables[0].Rows[i]["DepartmentId"].ToString(),
                            UpdateRemarks = dsRef.Tables[0].Rows[i]["UpdateRemarks"].ToString(),
                        });
                    }

                }
                catch (System.Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    objCon = null;
                }
            
        }

        public void GetDetentionLogDetailfromto(out List<GetDetentionclose> DataList, string from, string to)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<GetDetentionclose>();

            System.Data.DataSet dsRef;
            try
            {
                #region cmnt
                strSQL = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,  DL.UpdateRemarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime), 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59'and DL.isClose = 1";

                #endregion cmnt
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new GetDetentionclose
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        WorkCenter = dsRef.Tables[0].Rows[i]["WorkCenter"].ToString(),
                        DetentionType = dsRef.Tables[0].Rows[i]["DetentionType"].ToString(),
                        LoginTime = dsRef.Tables[0].Rows[i]["LoginTime"].ToString(),
                        IssueByNo = dsRef.Tables[0].Rows[i]["IssueByNo"].ToString(),
                        ResponsiblePersonName = dsRef.Tables[0].Rows[i]["ResponsiblePersonName"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        ContactNo = dsRef.Tables[0].Rows[i]["ContactNo"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        WorkCenterId = dsRef.Tables[0].Rows[i]["WorkCenterId"].ToString(),
                        DetentionTypeId = dsRef.Tables[0].Rows[i]["DetentionTypeId"].ToString(),
                        isClose = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["isClose"]),
                        isUpdate = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["isUpdate"]),
                        Process = dsRef.Tables[0].Rows[i]["Process"].ToString(),
                        ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                        AddedFromIP = dsRef.Tables[0].Rows[i]["AddedFromIP"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        AddedTime = dsRef.Tables[0].Rows[i]["AddedTime"].ToString(),
                        LogoutDate = dsRef.Tables[0].Rows[i]["LogoutDate"].ToString(),
                        LogoutTime = dsRef.Tables[0].Rows[i]["LogoutTime"].ToString(),
                        Duration = dsRef.Tables[0].Rows[i]["Duration"].ToString(),
                        DLRPId = dsRef.Tables[0].Rows[i]["DLRPId"].ToString(),
                        Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),
                        DepartmentId = dsRef.Tables[0].Rows[i]["DepartmentId"].ToString(),
                        UpdateRemarks = dsRef.Tables[0].Rows[i]["UpdateRemarks"].ToString(),
                    });
                }

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }

        }


        public void GetDetentionLogDetailfromtodepartment(out List<GetDetentionclose> DataList, string from, string to, string departmentId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<GetDetentionclose>();

            System.Data.DataSet dsRef;
            try
            {
                #region cmnt
                strSQL = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,  DL.UpdateRemarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime), 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DepartmentId = '" + departmentId + @"' and  DL.isClose = 1";

                #endregion cmnt
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new GetDetentionclose
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        WorkCenter = dsRef.Tables[0].Rows[i]["WorkCenter"].ToString(),
                        DetentionType = dsRef.Tables[0].Rows[i]["DetentionType"].ToString(),
                        LoginTime = dsRef.Tables[0].Rows[i]["LoginTime"].ToString(),
                        IssueByNo = dsRef.Tables[0].Rows[i]["IssueByNo"].ToString(),
                        ResponsiblePersonName = dsRef.Tables[0].Rows[i]["ResponsiblePersonName"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        ContactNo = dsRef.Tables[0].Rows[i]["ContactNo"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        WorkCenterId = dsRef.Tables[0].Rows[i]["WorkCenterId"].ToString(),
                        DetentionTypeId = dsRef.Tables[0].Rows[i]["DetentionTypeId"].ToString(),
                        isClose = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["isClose"]),
                        isUpdate = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["isUpdate"]),
                        Process = dsRef.Tables[0].Rows[i]["Process"].ToString(),
                        ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                        AddedFromIP = dsRef.Tables[0].Rows[i]["AddedFromIP"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        AddedTime = dsRef.Tables[0].Rows[i]["AddedTime"].ToString(),
                        LogoutDate = dsRef.Tables[0].Rows[i]["LogoutDate"].ToString(),
                        LogoutTime = dsRef.Tables[0].Rows[i]["LogoutTime"].ToString(),
                        Duration = dsRef.Tables[0].Rows[i]["Duration"].ToString(),
                        DLRPId = dsRef.Tables[0].Rows[i]["DLRPId"].ToString(),
                        Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),
                        DepartmentId = dsRef.Tables[0].Rows[i]["DepartmentId"].ToString(),
                        UpdateRemarks = dsRef.Tables[0].Rows[i]["UpdateRemarks"].ToString(),
                    });
                }

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }

        }

        public void GetDetentionLogDetailfromtodetention(out List<GetDetentionclose> DataList, string from, string to, string detentionTypeId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<GetDetentionclose>();

            System.Data.DataSet dsRef;
            try
            {
                #region cmnt
                strSQL = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,  DL.UpdateRemarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime), 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59'  and DL.DetentionTypeId = '" + detentionTypeId + "' and  DL.isClose = 1";
                #endregion cmnt
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new GetDetentionclose
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        WorkCenter = dsRef.Tables[0].Rows[i]["WorkCenter"].ToString(),
                        DetentionType = dsRef.Tables[0].Rows[i]["DetentionType"].ToString(),
                        LoginTime = dsRef.Tables[0].Rows[i]["LoginTime"].ToString(),
                        IssueByNo = dsRef.Tables[0].Rows[i]["IssueByNo"].ToString(),
                        ResponsiblePersonName = dsRef.Tables[0].Rows[i]["ResponsiblePersonName"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        ContactNo = dsRef.Tables[0].Rows[i]["ContactNo"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        WorkCenterId = dsRef.Tables[0].Rows[i]["WorkCenterId"].ToString(),
                        DetentionTypeId = dsRef.Tables[0].Rows[i]["DetentionTypeId"].ToString(),
                        isClose = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["isClose"]),
                        isUpdate = bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[i]["isUpdate"]),
                        Process = dsRef.Tables[0].Rows[i]["Process"].ToString(),
                        ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                        AddedFromIP = dsRef.Tables[0].Rows[i]["AddedFromIP"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        AddedTime = dsRef.Tables[0].Rows[i]["AddedTime"].ToString(),
                        LogoutDate = dsRef.Tables[0].Rows[i]["LogoutDate"].ToString(),
                        LogoutTime = dsRef.Tables[0].Rows[i]["LogoutTime"].ToString(),
                        Duration = dsRef.Tables[0].Rows[i]["Duration"].ToString(),
                        DLRPId = dsRef.Tables[0].Rows[i]["DLRPId"].ToString(),
                        Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),
                        DepartmentId = dsRef.Tables[0].Rows[i]["DepartmentId"].ToString(),
                        UpdateRemarks = dsRef.Tables[0].Rows[i]["UpdateRemarks"].ToString(),
                    });
                }

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }

        }

        public void GetTodayAssignedTask(out List<Tasks> DataList, string UserId, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select tm.Id, TaskDescription , CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where tm.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy' and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = '" + Date + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),
                       
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }


        public void GetTaskChats(out List<ChatTask> DataList, string Id)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<ChatTask>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select tc.Id, tc.TaskManagerMasterId,CreatedById, CommentText , ei.EmployeeName , ei.EmpPicPath from dbo.TaskComments As tc  
left join dbo.EmployeeInformation As ei on tc.CreatedById = ei.SystemId where tc.TaskManagerMasterId = '"+Id+"'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new ChatTask
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskManagerMasterId = dsRef.Tables[0].Rows[i]["TaskManagerMasterId"].ToString(),
                        CreatedById = dsRef.Tables[0].Rows[i]["CreatedById"].ToString(),
                        CommentText = dsRef.Tables[0].Rows[i]["CommentText"].ToString(),
                        EmployeeName = dsRef.Tables[0].Rows[i]["EmployeeName"].ToString(),
                        EmpPicPath = dsRef.Tables[0].Rows[i]["EmpPicPath"].ToString(),
                       
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }


        public void GetTaskAssignedDetail(out List<AssignTaskDatals> DataList, string Id)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<AssignTaskDatals>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Id,TaskManagerMasterId,AuthorizationType,ResponsiblePersonId , ei.FirstName As  EmployeeName, ei.EmpPicPath from dbo.TaskAudit as ta
left join dbo.EmployeeInformation As ei on ta.ResponsiblePersonId = ei.SystemId  where TaskManagerMasterId =  '" + Id + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new AssignTaskDatals
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskManagerMasterId = dsRef.Tables[0].Rows[i]["TaskManagerMasterId"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        EmployeeName = dsRef.Tables[0].Rows[i]["EmployeeName"].ToString(),
                        EmpPicPath = dsRef.Tables[0].Rows[i]["EmpPicPath"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetOverDueAssignedTask(out List<Tasks> DataList, string UserId, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select tm.Id, TaskDescription , CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId, format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where tm.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy' and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate < '" + Date + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetNextWeakAssignedTask(out List<Tasks> DataList, string UserId, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select tm.Id, TaskDescription , CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId, format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where tm.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy' and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = DATEADD(day, 7, '" + Date + "')";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetFutureAssignedTask(out List<Tasks> DataList, string UserId, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select tm.Id, TaskDescription , CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId, format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where tm.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy' and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate =  DATEADD(day, 7, '" + Date + "')";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }




         public void GetTodayCreateTask(out List<Tasks> DataList, string UserId, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select tm.Id, TaskDescription , CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where tm.CurrentStatus <> 'Closed' and ta.AuthorizationType = 'CreatedBy' and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = '" + Date + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),
                       
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }


        public void GetOverDueCreateTask(out List<Tasks> DataList, string UserId, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select tm.Id, TaskDescription , CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId, format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where tm.CurrentStatus <> 'Closed' and ta.AuthorizationType = 'CreatedBy' and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate < '" + Date + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetNextWeakCreateTask(out List<Tasks> DataList, string UserId, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select tm.Id, TaskDescription , CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId, format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where tm.CurrentStatus <> 'Closed' and ta.AuthorizationType = 'CreatedBy' and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = DATEADD(day, 7, '" + Date + "')";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetFutureCreateTask(out List<Tasks> DataList, string UserId, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select tm.Id, TaskDescription , CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId, format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where tm.CurrentStatus <> 'Closed' and ta.AuthorizationType = 'CreatedBy' and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate =  DATEADD(day, 7, '" + Date + "')";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }


        public void GetOnTimeTaskCreation(out List<closeTask> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<closeTask>();



            System.Data.DataSet dsRef;
            try
            {



                strSQL = @"select tm.Id, TaskDescription ,format(ClosingDate,'dd-MM-yy') As ClosingDate, ClosedBy, CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where  ta.AuthorizationType = 'CreatedBy' and tm.ClosingDate <= ta.DueDate  and tm.ClosedBy = '" + UserId + "'";



                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new closeTask
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        ClosedBy = dsRef.Tables[0].Rows[i]["ClosedBy"].ToString(),
                        ClosingDate = dsRef.Tables[0].Rows[i]["ClosingDate"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),



                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }


        public void GetOnTimeTaskAssigned(out List<closeTask> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<closeTask>();



            System.Data.DataSet dsRef;
            try
            {



                strSQL = @"select tm.Id, TaskDescription ,format(ClosingDate,'dd-MM-yy') As ClosingDate, ClosedBy, CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where  ta.AuthorizationType <> 'CreatedBy' and tm.ClosingDate <= ta.DueDate  and tm.ClosedBy = '" + UserId + "'";



                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new closeTask
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        ClosedBy = dsRef.Tables[0].Rows[i]["ClosedBy"].ToString(),
                        ClosingDate = dsRef.Tables[0].Rows[i]["ClosingDate"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),



                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }




        public void GetLateTaskAssigned(out List<closeTask> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<closeTask>();



            System.Data.DataSet dsRef;
            try
            {



                strSQL = @"select tm.Id, TaskDescription ,format(ClosingDate,'dd-MM-yy') As ClosingDate, ClosedBy, CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where  ta.AuthorizationType <> 'CreatedBy' and tm.ClosingDate > ta.DueDate  and tm.ClosedBy = '" + UserId + "'";



                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new closeTask
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        ClosedBy = dsRef.Tables[0].Rows[i]["ClosedBy"].ToString(),
                        ClosingDate = dsRef.Tables[0].Rows[i]["ClosingDate"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),



                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetLateTaskCreation(out List<closeTask> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<closeTask>();



            System.Data.DataSet dsRef;
            try
            {



                strSQL = @"select tm.Id, TaskDescription ,format(ClosingDate,'dd-MM-yy') As ClosingDate, ClosedBy, CurrentStatus,  TaskDetailDescription , ta.AuthorizationType, ResponsiblePersonId,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate,
format(ta.DueDate,'dd-MM-yy') as DueDate , format(ta.AddedDate, 'dd-MM-yy') as AddedDate from dbo.TaskManagerMaster As tm
left Join dbo.TaskAudit As ta on ta.TaskManagerMasterId = tm.Id 
where  ta.AuthorizationType = 'CreatedBy' and tm.ClosingDate > ta.DueDate  and tm.ClosedBy = '" + UserId + "'";



                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new closeTask
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        ClosedBy = dsRef.Tables[0].Rows[i]["ClosedBy"].ToString(),
                        ClosingDate = dsRef.Tables[0].Rows[i]["ClosingDate"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),



                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetCloseTask(out List<ActiveTask> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<ActiveTask>();



            System.Data.DataSet dsRef;
            try
            {



                strSQL = @"select 'OnTimeTaskAssigned' As Dated, Count(tm.Id) As Counted  from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where tm.ClosingDate <= ta.DueDate and ta.AuthorizationType <> 
'CreatedBy' and tm.ClosedBy = '" + UserId + @"'

 


Union All
select 'LateTaskAssigned' As Dated,  Count(tm.Id) As Counted from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where tm.ClosingDate > ta.DueDate and ta.AuthorizationType <> 
'CreatedBy' and tm.ClosedBy = '" + UserId + @"'

 

Union All
select 'OnTimeTaskCreation' As Dated, Count(tm.Id) As Counted  from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where tm.ClosingDate <= ta.DueDate and ta.AuthorizationType = 
'CreatedBy' and tm.ClosedBy = '" + UserId + @"'

 


Union All
select 'LateTaskCreation' As Dated,  Count(tm.Id) As Counted from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where tm.ClosingDate > ta.DueDate and ta.AuthorizationType = 
'CreatedBy' and tm.ClosedBy = '" + UserId + @"'";



                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new ActiveTask
                    {
                        Dated = dsRef.Tables[0].Rows[i]["Dated"].ToString(),
                        Counted = dsRef.Tables[0].Rows[i]["Counted"].ToString(),



                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        #endregion Aman
        public void GetProcess(out List<Process> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Process>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select distinct WM.ProcessId Value, P.UserName Text from SCS.WorkCenterMaster WM
                            left join HKP.Process P on P.Id = WM.ProcessId";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Process
                    {
                         Value= dsRef.Tables[0].Rows[i]["Value"].ToString(),
                         Text = dsRef.Tables[0].Rows[i]["Text"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetActiveTask(out List<ActiveTask> DataList , string UserId, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<ActiveTask>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select 'TodayCreation' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType = 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = '" + Date + @"'

Union All
select 'FutureCreation' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType = 'CreatedBy'
and ta.ResponsiblePersonId = '"+UserId+"' and ta.DueDate > DATEADD(day, 7, '"+Date+@"')

Union All
select 'OverDueCreation' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType = 'CreatedBy'
and ta.ResponsiblePersonId = '"+UserId+"' and ta.DueDate < '"+Date+@"'

Union All

select 'NextWeekCreation' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType = 'CreatedBy'
and ta.ResponsiblePersonId = '"+UserId+"' and ta.DueDate = DATEADD(day, 7, '"+Date+@"')

Union All
select 'TodayAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '"+UserId+"' and ta.DueDate = '"+Date+@"'

Union All
select 'FutureAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '"+UserId+"' and ta.DueDate > DATEADD(day, 7, '"+Date+@"')

Union All

select 'OverDueAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '"+UserId+"' and ta.DueDate < '"+Date+@"'

Union All

select 'NextWeekAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '"+UserId+"' and ta.DueDate = DATEADD(day, 7, '"+Date+"')";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new ActiveTask
                    {
                        Dated = dsRef.Tables[0].Rows[i]["Dated"].ToString(),
                        Counted = dsRef.Tables[0].Rows[i]["Counted"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }



        public void GetCloseTask(out List<ActiveTask> DataList, string UserId, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<ActiveTask>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select 'TodayCreation' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus = 'Closed' and ta.AuthorizationType = 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = '" + Date + @"'

Union All
select 'FutureCreation' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus = 'Closed' and ta.AuthorizationType = 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate > DATEADD(day, 7, '" + Date + @"')

Union All
select 'OverDueCreation' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus = 'Closed' and ta.AuthorizationType = 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate < '" + Date + @"'

Union All

select 'NextWeekCreation' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus = 'Closed' and ta.AuthorizationType = 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = DATEADD(day, 7, '" + Date + @"')

Union All
select 'TodayAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus = 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = '" + Date + @"'

Union All
select 'FutureAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus = 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate > DATEADD(day, 7, '" + Date + @"')

Union All

select 'OverDueAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus = 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate < '" + Date + @"'

Union All

select 'NextWeekAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus = 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = DATEADD(day, 7, '" + Date + "')";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new ActiveTask
                    {
                        Dated = dsRef.Tables[0].Rows[i]["Dated"].ToString(),
                        Counted = dsRef.Tables[0].Rows[i]["Counted"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }


        public string PostCreateDetention(IEnumerable<CreateDetentionList> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.DetentionLog";

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
                        dr["isClose"] = false;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


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
                        dr["isClose"] = false;

                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();

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

      


        // Detention Log Out

        #endregion Written By Nitesh
        // Written by Nitesh end

        public void getEmployeeInfo(string EmployeeCode, string CompanyID, out List<EmployeeInfo> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<EmployeeInfo>();
            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT ei.SystemID,ei.GroupID,ei.CompanyID,ei.PlantID, ei.EmployeeCode, ei.CardNumber, ei.EmployeeName, ei.EmpType,
       ei.EmploymentType,   
       left(replace(upper(convert(varchar,ei.DOB,113)),' ','-'),11) DOB,
       left(replace(upper(convert(varchar,ei.DOJ,113)),' ','-'),11) DOJ,
       left(replace(upper(convert(varchar,ei.DOS,113)),' ','-'),11) DOS,
       ei.EmployeeStatus, ei.NationalID, ei.CitizenID, ei.PresentAddress1 PresentAddress,
       ei.ParmanentAddress1 ParmanentAddress,p.UserName PlantName,d.UserName DivisionName,d2.UserName DepartmentName,s.UserName SectionName,ss.UserName SubSectionName,dg.UserName DesignationGroupName,d3.UserName DesignationName,
       SAL.MinYear,SAL.MinMonth,
      
       jl.JobLocation,i.EmpImage, i.ImgType
  FROM EmployeeInformation ei
LEFT OUTER JOIN JobLocation jl ON jl.SystemID=ei.JobLocationID
LEFT OUTER JOIN EmployeeImage I ON i.EmpSystemID=ei.SystemID
LEFT OUTER JOIN ORG.Plant p ON p.Id=ei.PlantID
LEFT OUTER JOIN ORG.Division d ON d.Id=ei.DivisionID
LEFT OUTER JOIN ORG.Department d2 ON d2.Id=ei.DepartmentID
LEFT OUTER JOIN ORG.Section s ON s.Id=ei.SectionID
LEFT OUTER JOIN ORG.SubSection ss ON ss.Id=ei.SubSectionID
LEFT OUTER JOIN HKP.DesignationGroup dg ON dg.Id=ei.DesignationGroupID
LEFT OUTER JOIN HKP.Designation d3 ON d3.Id=ei.DesignationSystemID
LEFT OUTER JOIN (SELECT C.EmpInfoSystemID,MIN(spm.YearNo) AS MinYear,MIN(spm.MonthNo) AS MinMonth
                   FROM SalaryProcChild C
                   LEFT OUTER JOIN SalaryProcMaster spm ON spm.SystemID=c.SlrProcMstSystemID
                    GROUP BY C.EmpInfoSystemID) AS SAL ON SAL.EmpInfoSystemID=ei.SystemID

WHERE ei.EmployeeCode='" + EmployeeCode + "' AND ei.CompanyID='" + CompanyID + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new EmployeeInfo
                    {
                        SystemID = dsRef.Tables[0].Rows[i]["SystemID"].ToString(),
                        GroupID = dsRef.Tables[0].Rows[i]["GroupID"].ToString(),
                        CompanyID = dsRef.Tables[0].Rows[i]["CompanyID"].ToString(),
                        PlantID = dsRef.Tables[0].Rows[i]["PlantID"].ToString(),
                        EmployeeCode = dsRef.Tables[0].Rows[i]["EmployeeCode"].ToString(),
                        CardNumber = dsRef.Tables[0].Rows[i]["CardNumber"].ToString(),
                        EmployeeName = dsRef.Tables[0].Rows[i]["EmployeeName"].ToString(),
                        EmpType = dsRef.Tables[0].Rows[i]["EmpType"].ToString(),
                        EmploymentType = dsRef.Tables[0].Rows[i]["EmploymentType"].ToString(),
                        DOB = dsRef.Tables[0].Rows[i]["DOB"].ToString(),
                        DOJ = dsRef.Tables[0].Rows[i]["DOJ"].ToString(),
                        DOS = dsRef.Tables[0].Rows[i]["DOS"].ToString(),
                        EmployeeStatus = dsRef.Tables[0].Rows[i]["EmployeeStatus"].ToString(),
                        NationalID = dsRef.Tables[0].Rows[i]["NationalID"].ToString(),
                        CitizenID = dsRef.Tables[0].Rows[i]["CitizenID"].ToString(),
                        PresentAddress = dsRef.Tables[0].Rows[i]["PresentAddress"].ToString(),
                        ParmanentAddress = dsRef.Tables[0].Rows[i]["ParmanentAddress"].ToString(),
                        PlantName = dsRef.Tables[0].Rows[i]["PlantName"].ToString(),
                        DivisionName = dsRef.Tables[0].Rows[i]["DivisionName"].ToString(),
                        DepartmentName = dsRef.Tables[0].Rows[i]["DepartmentName"].ToString(),
                        SectionName = dsRef.Tables[0].Rows[i]["SectionName"].ToString(),
                        SubSectionName = dsRef.Tables[0].Rows[i]["SubSectionName"].ToString(),
                        DesignationGroupName = dsRef.Tables[0].Rows[i]["DesignationGroupName"].ToString(),
                        DesignationName = dsRef.Tables[0].Rows[i]["DesignationName"].ToString(),
                        JobLocation = dsRef.Tables[0].Rows[i]["JobLocation"].ToString(),

                        MinMonth = (int)clsStdLib.dbl(dsRef.Tables[0].Rows[i]["MinMonth"].ToString()),
                        MinYear = (int)clsStdLib.dbl(dsRef.Tables[0].Rows[i]["MinYear"].ToString()),

                        EmpImage = dsRef.Tables[0].Rows[i]["EmpImage"],
                        ImgType = dsRef.Tables[0].Rows[i]["ImgType"].ToString(),
                    });
                    DataList[i].EmpImage = new byte[] { 0 };
                    if (dsRef.Tables[0].Rows[i]["EmpImage"].GetType() != typeof(System.DBNull))
                        DataList[i].EmpImage = dsRef.Tables[0].Rows[i]["EmpImage"];
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        public void getSalaryInformation(string EmpInfoSystemID, int Month, int Year, out List<SalaryInformation> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<SalaryInformation>();
            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT Spc.SystemID, spc.EmpInfoSystemID, spm.SalaryProcID, 
   left(replace(upper(convert(varchar,spm.FromDate,113)),' ','-'),11) FromDate,
   left(replace(upper(convert(varchar,spm.ToDate,113)),' ','-'),11) ToDate, 
   left(replace(upper(convert(varchar,spm.SalaryProcDate,113)),' ','-'),11) SalaryProcDate,
spm.MonthNo,
spm.YearNo,sh.[Description] AS SalaryHead,sh.HeadType, spc.DisbusmentAmount AS DisbursementAmount,c.Name  AS DisbursementCurrency,
CASE WHEN isnull(spc.IsDisbursed,0)=0 THEN 'NO' ELSE 'YES' END AS isDisbursed
  FROM SalaryProcMaster spm
LEFT OUTER JOIN SalaryProcChild spc ON spm.SystemID=spc.SlrProcMstSystemID
LEFT OUTER JOIN SalaryHead sh ON sh.SalaryHeadID=spc.SalaryHeadID
LEFT OUTER JOIN SCS.currency c ON c.Id=spc.DisbusmentCurrencyID

WHERE spc.EmpInfoSystemID='" + EmpInfoSystemID + "' AND isnull(spc.IsNetPayEffect,0)=1 AND  spm.MonthNo=" + Month + " AND spm.YearNo=" + Year + @"
ORDER BY sh.HeadType DESC, sh.SalaryHead ASC";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new SalaryInformation
                    {
                        SystemID = dsRef.Tables[0].Rows[i]["SystemID"].ToString(),
                        EmpInfoSystemID = dsRef.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(),
                        SalaryProcID = dsRef.Tables[0].Rows[i]["SalaryProcID"].ToString(),
                        FromDate = dsRef.Tables[0].Rows[i]["FromDate"].ToString(),
                        ToDate = dsRef.Tables[0].Rows[i]["ToDate"].ToString(),
                        SalaryProcDate = dsRef.Tables[0].Rows[i]["SalaryProcDate"].ToString(),
                        MonthNo = (int)clsStdLib.dbl(dsRef.Tables[0].Rows[i]["MonthNo"].ToString()),
                        YearNo = (int)clsStdLib.dbl(dsRef.Tables[0].Rows[i]["YearNo"].ToString()),
                        SalaryHead = dsRef.Tables[0].Rows[i]["SalaryHead"].ToString(),
                        HeadType = dsRef.Tables[0].Rows[i]["HeadType"].ToString(),
                        DisbursementAmount = clsStdLib.dbl(dsRef.Tables[0].Rows[i]["DisbursementAmount"].ToString()),
                        DisbursementCurrency = dsRef.Tables[0].Rows[i]["DisbursementCurrency"].ToString(),
                        isDisbursed = dsRef.Tables[0].Rows[i]["isDisbursed"].ToString()
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        public void getAttendanceInformation(string EmpSystemID, string FromDate, String ToDate, out List<AttendanceInformation> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<AttendanceInformation>();
            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT apd.EmpSystemID, apd.WorkDate,sd.ShiftDefinationName,apd.InTime,
                            apd.OutTime, apd.DayStatus
                            FROM AttdnProcessData apd 
                            left outer join ShiftDefination sd ON sd.SystemID=apd.ShiftSystemID

                            WHERE apd.EmpSystemID='" + EmpSystemID + "' AND WorkDate Between '" + FromDate + "' AND '" + ToDate + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new AttendanceInformation
                    {
                        EmpSystemID = dsRef.Tables[0].Rows[i]["EmpSystemID"].ToString(),
                        WorkDate = dsRef.Tables[0].Rows[i]["WorkDate"].ToString(),
                        ShiftDefinationName = dsRef.Tables[0].Rows[i]["ShiftDefinationName"].ToString(),
                        InTime = dsRef.Tables[0].Rows[i]["InTime"].ToString(),
                        OutTime = dsRef.Tables[0].Rows[i]["OutTime"].ToString(),
                        DayStatus = dsRef.Tables[0].Rows[i]["DayStatus"].ToString(),
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        public void getSalaryStructure(string EmpInfoSystemID, out List<SalaryStructure> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<SalaryStructure>();
            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT si.SystemID, sim.EmpInfoSystemID,
left(replace(upper(convert(varchar,sim.EffectiveDate,113)),' ','-'),11) EffectiveDate,
CASE WHEN isnull(sim.IsApproved,0)=0 THEN 'NO' ELSE 'YES' END AS IsApproved,
 sh.[Description] AS SalaryHead,sh.HeadType, si.DefineAmount,c.Name  AS Currency

FROM SalaryInfoDefine si 
LEFT OUTER JOIN SalaryInfoDefineMaster AS sim ON sim.SystemID=si.SalaryID
LEFT OUTER JOIN SalaryHead sh ON sh.SalaryHeadID=si.SalaryHeadID
LEFT OUTER JOIN SCS.currency c ON c.Id=si.AmtDefinitionCurrencyID

WHERE sim.EmpInfoSystemID='" + EmpInfoSystemID + "' AND sim.EffectiveDate IN (SELECT MAX(EffectiveDate) FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID='" + EmpInfoSystemID + "')";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new SalaryStructure
                    {
                        SystemID = dsRef.Tables[0].Rows[i]["SystemID"].ToString(),
                        EmpInfoSystemID = dsRef.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(),
                        EffectiveDate = dsRef.Tables[0].Rows[i]["EffectiveDate"].ToString(),
                        IsApproved = dsRef.Tables[0].Rows[i]["IsApproved"].ToString(),
                        SalaryHead = dsRef.Tables[0].Rows[i]["SalaryHead"].ToString(),
                        HeadType = dsRef.Tables[0].Rows[i]["HeadType"].ToString(),
                        DefineAmount = (int)clsStdLib.dbl(dsRef.Tables[0].Rows[i]["DefineAmount"].ToString()),
                        Currency = dsRef.Tables[0].Rows[i]["Currency"].ToString(),
                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void getUnreadNotifications(string EmpInfoSystemID, out List<ServerNotifications> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<ServerNotifications>();
            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT N.SystemID, N.EmpInfoSystemID,apd.WorkDate,apd.InTime, apd.OutTime, apd.DayStatus,n.EventType,
                        spm.MonthNo, spm.YearNo,N.EventDate,N.EventRaisedBy
                        FROM [dbo].[EmployeeNotifications] N
                        LEFT OUTER JOIN SalaryProcMaster spm ON spm.SystemID=n.EventSourceTableSystemID

                        LEFT OUTER JOIN AttdnProcessData apd ON apd.EmpSystemID=n.EmpInfoSystemID AND 
                        REPLACE(CONVERT(CHAR(11), apd.WorkDate, 106), ' ', '-')=REPLACE(CONVERT(CHAR(11), n.WorkDate, 106), ' ', '-')

                        WHERE EmpInfoSystemID='" + EmpInfoSystemID + "' AND isnull(IsDelivered,0)=0 ORDER BY N.EventDate DESC";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();

                DataRow drLocal;
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new ServerNotifications
                    {
                        SystemID = dsRef.Tables[0].Rows[i]["SystemID"].ToString(),
                        EmpInfoSystemID = dsRef.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(),
                        WorkDate = dsRef.Tables[0].Rows[i]["WorkDate"].ToString(),
                        InTime = dsRef.Tables[0].Rows[i]["InTime"].ToString(),
                        OutTime = dsRef.Tables[0].Rows[i]["OutTime"].ToString(),
                        DayStatus = dsRef.Tables[0].Rows[i]["DayStatus"].ToString(),
                        EventType = dsRef.Tables[0].Rows[i]["EventType"].ToString(),
                        MonthNo = (int)clsStdLib.dbl(dsRef.Tables[0].Rows[i]["MonthNo"].ToString()),
                        YearNo = (int)clsStdLib.dbl(dsRef.Tables[0].Rows[i]["YearNo"].ToString()),
                        EventDate = Convert.ToDateTime(dsRef.Tables[0].Rows[i]["EventDate"].ToString()),
                        EventRaisedBy = dsRef.Tables[0].Rows[i]["EventRaisedBy"].ToString(),
                    });


                }


                strSQL = @"SELECT *
                        FROM [dbo].[EmployeeNotifications] N
                       
                        WHERE N.EmpInfoSystemID='" + EmpInfoSystemID + "' AND isnull(N.IsDelivered,0)=0 ";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    drLocal = dsRef.Tables[0].Rows[i];
                    drLocal.BeginEdit();
                    drLocal["IsDelivered"] = true;
                    drLocal.EndEdit();
                }
                SaveDataSets(dsRef);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        public void getNotifications(string EmpInfoSystemID, string lastDateTime, int RecordCount, out List<ServerNotifications> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<ServerNotifications>();
            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT TOP " + RecordCount.ToString() + " N.SystemID, N.EmpInfoSystemID,apd.WorkDate,apd.InTime, apd.OutTime, apd.DayStatus,n.EventType, " + @"
                        spm.MonthNo, spm.YearNo,N.EventDate,N.EventRaisedBy
                        FROM [dbo].[EmployeeNotifications] N
                        LEFT OUTER JOIN SalaryProcMaster spm ON spm.SystemID=n.EventSourceTableSystemID

                        LEFT OUTER JOIN AttdnProcessData apd ON apd.EmpSystemID=n.EmpInfoSystemID AND 
                        REPLACE(CONVERT(CHAR(11), apd.WorkDate, 106), ' ', '-')=REPLACE(CONVERT(CHAR(11), n.WorkDate, 106), ' ', '-')

                        WHERE EmpInfoSystemID='" + EmpInfoSystemID + "' AND N.EventDate<'" + lastDateTime + "'  ORDER BY N.EventDate DESC";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();


                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new ServerNotifications
                    {
                        SystemID = dsRef.Tables[0].Rows[i]["SystemID"].ToString(),
                        EmpInfoSystemID = dsRef.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(),
                        WorkDate = dsRef.Tables[0].Rows[i]["WorkDate"].ToString(),
                        InTime = dsRef.Tables[0].Rows[i]["InTime"].ToString(),
                        OutTime = dsRef.Tables[0].Rows[i]["OutTime"].ToString(),
                        DayStatus = dsRef.Tables[0].Rows[i]["DayStatus"].ToString(),
                        EventType = dsRef.Tables[0].Rows[i]["EventType"].ToString(),
                        MonthNo = (int)clsStdLib.dbl(dsRef.Tables[0].Rows[i]["MonthNo"].ToString()),
                        YearNo = (int)clsStdLib.dbl(dsRef.Tables[0].Rows[i]["YearNo"].ToString()),
                        EventDate = Convert.ToDateTime(dsRef.Tables[0].Rows[i]["EventDate"].ToString()),
                        EventRaisedBy = dsRef.Tables[0].Rows[i]["EventRaisedBy"].ToString(),
                    });
                }

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public bool login(string EmployeeCode, string PIN, string CompanyID)
        {

            clsConnectionManager objCon = null;
            string strSQL = "";

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT * FROM EmployeeInformation ei WHERE ei.EmployeeCode='"
                        + EmployeeCode + "' AND ei.CompanyID='" + CompanyID + "' AND ei.employeeStatus='Active'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();

                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    strSQL = @"SELECT * FROM EmployeeMobileNotification ei WHERE ei.EMPSystemID='" + dsRef.Tables[0].Rows[0]["SystemID"].ToString() + "' AND ei.PINNo='" + PIN + "'";
                    strSQL = @"SELECT * FROM [HKP].[EmployeeMobileAppsAuthorization] ei WHERE ei.EmployeeId='" + dsRef.Tables[0].Rows[0]["SystemID"].ToString() + "' AND ei.PINNo=" + PIN + "";
                    objCon = new clsConnectionManager();
                    objCon.BeginTransaction();
                    objCon.getDataSet(strSQL, out dsRef);
                    objCon.CommitTransaction();
                    if (dsRef.Tables[0].Rows.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }



        }


        public void getUserUnSentNotifications(string EventType, out DataSet dsRef)
        {

            clsConnectionManager objCon = null;
            string strSQL = "";


            try
            {
                strSQL = @"SELECT apd.* FROM EmployeeNotifications en 
INNER JOIN AttdnProcessData apd ON apd.EmpSystemID=en.EmpInfoSystemID
  AND REPLACE(CONVERT(CHAR(11), en.WorkDate, 106), ' ', '-')=REPLACE(CONVERT(CHAR(11), apd.WorkDate, 106), ' ', '-')
 WHERE en.EventType='" + EventType + "' AND isnull(en.IsDelivered,0)=0 AND en.WorkDate='" + System.DateTime.Now.ToString(clsStdLib.dateFormat) + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();


            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }



        }
        public void getUserUnSentSalaryNotifications(string EventType, out DataSet dsRef)
        {

            clsConnectionManager objCon = null;
            string strSQL = "";


            try
            {
                strSQL = @"SELECT spm.SystemID,en.EmpInfoSystemID,spm.MonthNo, spm.YearNo
                            FROM EmployeeNotifications en 
                            LEFT OUTER JOIN SalaryProcMaster spm ON spm.SystemID=en.EventSourceTableSystemID
                            WHERE en.EventType='" + EventType + "' AND isnull(en.IsDelivered,0)=0";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();


            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }



        }
        #region Written By Aman
        #region AllTaskList

        public void GetMyCreationActive(out List<Tasks> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Tm.Id, AuthorizationType, tm.TaskDescription,tm.TaskDetailDescription, 
tm.CurrentStatus, ResponsiblePersonId,format(ta.AddedDate, 'dd-MM-yy')AddedDate,format(ta.DueDate,'dd-MM-yy') as DueDate,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where ta.AuthorizationType = 
'CreatedBy' and tm.CurrentStatus <> 'Closed' and ta.ResponsiblePersonId = '" + UserId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetMyTaskActive(out List<Tasks> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Tm.Id, AuthorizationType, tm.TaskDescription,tm.TaskDetailDescription, 
tm.CurrentStatus, ResponsiblePersonId,format(ta.AddedDate, 'dd-MM-yy')AddedDate,format(ta.DueDate,'dd-MM-yy') as DueDate,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where ta.AuthorizationType = 
'AssignTo' and tm.CurrentStatus <> 'Closed' and ta.ResponsiblePersonId = '" + UserId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetTocheckActive(out List<Tasks> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Tm.Id, AuthorizationType, tm.TaskDescription,tm.TaskDetailDescription, 
tm.CurrentStatus, ResponsiblePersonId,format(ta.AddedDate, 'dd-MM-yy')AddedDate,format(ta.DueDate,'dd-MM-yy') as DueDate,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where ta.AuthorizationType = 
'CheckBy' and tm.CurrentStatus <> 'Closed' and ta.ResponsiblePersonId = '" + UserId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetTocrosscheckActive(out List<Tasks> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Tm.Id, AuthorizationType, tm.TaskDescription,tm.TaskDetailDescription, 
tm.CurrentStatus, ResponsiblePersonId,format(ta.AddedDate, 'dd-MM-yy')AddedDate,format(ta.DueDate,'dd-MM-yy') as DueDate,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where ta.AuthorizationType = 
'CrossCheckBy' and tm.CurrentStatus <> 'Closed' and ta.ResponsiblePersonId = '" + UserId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetToapprovedActive(out List<Tasks> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Tm.Id, AuthorizationType, tm.TaskDescription,tm.TaskDetailDescription, 
tm.CurrentStatus, ResponsiblePersonId,format(ta.AddedDate, 'dd-MM-yy')AddedDate,format(ta.DueDate,'dd-MM-yy') as DueDate,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where ta.AuthorizationType = 
'ApproveBy' and tm.CurrentStatus <> 'Closed' and ta.ResponsiblePersonId = '" + UserId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }



        public void GetMyCreationClose(out List<Tasks> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Tm.Id, AuthorizationType, tm.TaskDescription,tm.TaskDetailDescription, 
tm.CurrentStatus, ResponsiblePersonId,format(ta.AddedDate, 'dd-MM-yy')AddedDate,format(ta.DueDate,'dd-MM-yy') as DueDate,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where ta.AuthorizationType = 
'CreatedBy' and tm.CurrentStatus = 'Closed' and ta.ResponsiblePersonId = '" + UserId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetMyTaskClose(out List<Tasks> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Tm.Id, AuthorizationType, tm.TaskDescription,tm.TaskDetailDescription, 
tm.CurrentStatus, ResponsiblePersonId,format(ta.AddedDate, 'dd-MM-yy')AddedDate,format(ta.DueDate,'dd-MM-yy') as DueDate,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where ta.AuthorizationType = 
'AssignTo' and tm.CurrentStatus = 'Closed' and ta.ResponsiblePersonId = '" + UserId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetTocheckClose(out List<Tasks> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Tm.Id, AuthorizationType, tm.TaskDescription,tm.TaskDetailDescription, 
tm.CurrentStatus, ResponsiblePersonId,format(ta.AddedDate, 'dd-MM-yy')AddedDate,format(ta.DueDate,'dd-MM-yy') as DueDate,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where ta.AuthorizationType = 
'CheckBy' and tm.CurrentStatus = 'Closed' and ta.ResponsiblePersonId = '" + UserId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetTocrosscheckClose(out List<Tasks> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Tm.Id, AuthorizationType, tm.TaskDescription,tm.TaskDetailDescription, 
tm.CurrentStatus, ResponsiblePersonId,format(ta.AddedDate, 'dd-MM-yy')AddedDate,format(ta.DueDate,'dd-MM-yy') as DueDate,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where ta.AuthorizationType = 
'CrossCheckBy' and tm.CurrentStatus = 'Closed' and ta.ResponsiblePersonId = '" + UserId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetToapprovedClose(out List<Tasks> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Tasks>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select Tm.Id, AuthorizationType, tm.TaskDescription,tm.TaskDetailDescription, 
tm.CurrentStatus, ResponsiblePersonId,format(ta.AddedDate, 'dd-MM-yy')AddedDate,format(ta.DueDate,'dd-MM-yy') as DueDate,format(ta.CommitmentDate,'dd-MM-yy') as CommitmentDate from dbo.TaskManagerMaster As tm
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  where ta.AuthorizationType = 
'ApproveBy' and tm.CurrentStatus = 'Closed' and ta.ResponsiblePersonId = '" + UserId + "'";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Tasks
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        TaskDescription = dsRef.Tables[0].Rows[i]["TaskDescription"].ToString(),
                        CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                        TaskDetailDescription = dsRef.Tables[0].Rows[i]["TaskDetailDescription"].ToString(),
                        AuthorizationType = dsRef.Tables[0].Rows[i]["AuthorizationType"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        DueDate = dsRef.Tables[0].Rows[i]["DueDate"].ToString(),
                        CommitmentDate = dsRef.Tables[0].Rows[i]["CommitmentDate"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        #endregion AllTaskList

        #region Deshboard
        public void GetDeshboard(out List<Default> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"Select 'RequisitionCount' AS Text, count(MRM.Id) Value
                        FROM[TRN].[MaterialRequsitionMaster] MRM
                        Where MRM.CheckedByStatus <> 'Checked' AND MRM.CheckedByStatus <>'Hold' and MRM.CheckedByStatus <> 'Reject' 
                        AND MRM.CheckedBy='" + UserId + @"'
Union All
 Select 'POCount' AS Text, count(IR.Id) Value
                       FROM [TRN].[PurchaseOrder] AS IR
                        Where IR.CheckedByStatus <> 'Checked' AND IR.CheckedByStatus <>'Hold' and IR.CheckedByStatus <> 'Reject' 
                        AND IR.CheckedBy='" + UserId + @"'
Union All
Select 'GRNCount' AS Text, count(IR.Id) Value
                        from trn.InventoryReceive AS IR
                        Where IR.CheckedByStatus <> 'Checked' AND IR.CheckedByStatus <>'Hold' and IR.CheckedByStatus <> 'Reject' 
                        AND IR.CheckedBy='" + UserId + @"'
Union All
Select 'ServicePOCount' AS Text, count(IR.Id) Value
                        from trn.ServicePOMaster AS IR
                        Where IR.CheckedByStatus <> 'Checked' AND IR.CheckedByStatus <>'Hold' and IR.CheckedByStatus <> 'Reject' 
                        AND IR.CheckedBy='" + UserId + @"'
Union All
Select 'ServiceCount' AS Text, count(IR.Id) Value
                        from trn.ServiceAcknowledgementMaster AS IR
                        Where IR.CheckedByStatus <> 'Checked' AND IR.CheckedByStatus <>'Hold' and IR.CheckedByStatus <> 'Reject' 
                        AND IR.CheckedBy='" + UserId + @"'


Union All
select 'AdvanceCount' AS Text, Count(EmpSystemId) As Value  from TRN.EmployeeAdvanceRequisition where IsPost  =
0 and EmpSystemId = '" + UserId + @"'

Union All
select 'ExpenseCount' AS Text , Count(EmployeeId) As Value from TRN.ExpenseBooking where VoucherId Is Null and EmployeeId = '" + UserId + @"'

Union All
select 'IssueCount' AS Text, Count(EmployeeId) As Value from TRN.InventoryIssue Where  VoucherId Is Null and EmployeeId = '" + UserId + @"'

Union All
select 'LeaveCount' AS Name , Count(SystemID) As Value from dbo.LeaveTransaction   WHERE  IsNull(IsApproved,0) = 0
                             AND ISNULL(SystemID,'')<> ''
                             AND IsCancel=0
                             AND FirstApprovingStatus = 0  AND FirstApprovingAuthority = '" + UserId + @"'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Text = dsRef.Tables[0].Rows[i]["Text"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        #endregion Deshboard
        #endregion Written By Aman

    }


    public class ServerNotifications
    {
        public string SystemID { get; set; } = "";
        public string EmpInfoSystemID { get; set; } = "";
        public string WorkDate { get; set; } = "";
        public string InTime { get; set; } = "";
        public string OutTime { get; set; } = "";
        public string DayStatus { get; set; } = "";
        public string EventType { get; set; } = "";
        public int MonthNo { get; set; } = 0;
        public int YearNo { get; set; } = 0;
        public DateTime EventDate { get; set; } = System.DateTime.Now;
        public string EventRaisedBy { get; set; } = "";
    }

    public class CompanyList
    {
        public string CompanyID { get; set; } = "";
        public string CODE { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string ShortName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Address1 { get; set; } = "";
        public string Address2 { get; set; } = "";
        public string Address3 { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
    }
    public class EmployeeInfo
    {
        public string SystemID { get; set; } = "";
        public string GroupID { get; set; } = "";
        public string CompanyID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string CardNumber { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string EmpType { get; set; } = "";
        public string EmploymentType { get; set; } = "";
        public string DOB { get; set; } = "";
        public string DOJ { get; set; } = "";
        public string DOS { get; set; } = "";
        public string EmployeeStatus { get; set; } = "";
        public string NationalID { get; set; } = "";
        public string CitizenID { get; set; } = "";
        public string PresentAddress { get; set; } = "";
        public string ParmanentAddress { get; set; } = "";
        public string PlantName { get; set; } = "";
        public string DivisionName { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string SectionName { get; set; } = "";

        public int MinYear { get; set; } = 0;
        public int MinMonth { get; set; } = 0;

        public string SubSectionName { get; set; } = "";
        public string DesignationGroupName { get; set; } = "";
        public string DesignationName { get; set; } = "";
        public string JobLocation { get; set; } = "";
        public object EmpImage { get; set; } = null;
        public string ImageLocation { get; set; } = "";
        public string ImgType { get; set; } = "";
    }
    public class SalaryInformation
    {
        public string SystemID { get; set; } = "";
        public string EmpInfoSystemID { get; set; } = "";
        public string SalaryProcID { get; set; } = "";
        public string FromDate { get; set; } = "";
        public string ToDate { get; set; } = "";
        public string SalaryProcDate { get; set; } = "";
        public int MonthNo { get; set; } = 0;
        public int YearNo { get; set; } = 0;
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public double DisbursementAmount { get; set; } = 0;
        public string DisbursementCurrency { get; set; } = "";
        public string isDisbursed { get; set; } = "";
    }
    public class SalaryStructure
    {

        public string SystemID { get; set; } = "";
        public string EmpInfoSystemID { get; set; } = "";
        public string EffectiveDate { get; set; } = "";
        public string IsApproved { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public double DefineAmount { get; set; } = 0;
        public string Currency { get; set; } = "";
    }

    public class AttendanceInformation
    {
        public string EmpSystemID { get; set; } = "";
        public string WorkDate { get; set; } = "";
        public string ShiftDefinationName { get; set; } = "";
        public string InTime { get; set; } = "";
        public string OutTime { get; set; } = "";
        public string DayStatus { get; set; } = "";
    }

    public class MyAppDefaultlist
    {
        public string Id { get; set; } 
        public string MenuName { get; set; } 
        public string IconName { get; set; } 
    }
    #region Written by Nitesh


    public class WorkCenterList
    {
        public string Text { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class QualificationList
    {
        public string Id { get; set; } = "";
        public string StandardName { get; set; } = "";
    }

    public class DetentionTypeList
    {
        public string DetentionTypeId { get; set; } = "";
        public string DetentionType { get; set; } = "";
    }

    public class DetentionResponsiblePersonList
    {
        public string ResponsiblePersonId { get; set; }
        public string CellPhnNo { get; set; }
        public string EmployeeCode { get; set; }
        public string ResponsiblePerson { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string SubSection { get; set; }
        public string LegalDesignation { get; set; }
    }

    public class DefaultMyAppIconList
    {
        public string RoleId { get; set; }
        public string ModuleId { get; set; }
        public string IconID { get; set; }
        public string Role { get; set; }
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string UserID { get; set; }
        public bool Active { get; set; }
    }

    public class DetentionIssueByNo
    {
        public string IssueByNo { get; set; }
    }

    public class Process
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }
    public class DepartmentList
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class AllDepartmentList
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class DetentionLogGridList
    {

        public string Id { get; set; }
        public string WorkCenter { get; set; }
        public string DetentionType { get; set; }
        public string LoginTime { get; set; }
        public string IssueByNo { get; set; }
        public string ResponsiblePersonName { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string ContactNo { get; set; }
        public string Remarks { get; set; }
        public string WorkCenterId { get; set; }
        public string DetentionTypeId { get; set; }
        public bool isClose { get; set; }
        public bool isUpdate { get; set; }
        public string Process { get; set; }
        public string ProcessId { get; set; }
        public string AddedBy { get; set; }
        public string AddedFromIP { get; set; }
        public string AddedDate { get; set; }
        public string DLRPId { get; set; }
        public string Department { get; set; }
        public string DepartmentId { get; set; }
        public string UpdateRemarks { get; set; }
        
    }

    public class GetDetentionLog
    {

        public string Id { get; set; }
        public string WorkCenter { get; set; }
        public string DetentionType { get; set; }
        public string LoginTime { get; set; }
        public string IssueByNo { get; set; }
        public string ResponsiblePersonName { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string ContactNo { get; set; }
        public string Remarks { get; set; }
        public string WorkCenterId { get; set; }
        public string DetentionTypeId { get; set; }
        public bool isClose { get; set; }
        public string MachineMaster { get; set; }
        public string LogoutTime { get; set; }
        public string MachineMasterId { get; set; }
        public string AddedBy { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }
        public string AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
    }
    public class GetDetentionclose
    {
        public string Id { get; set; }
        public string WorkCenter { get; set; }
        public string DetentionType { get; set; }
        public string AddedTime { get; set; }
        public string LoginTime { get; set; }
        public string IssueByNo { get; set; }
        public string ResponsiblePersonName { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string ContactNo { get; set; }
        public string Remarks { get; set; }
        public string WorkCenterId { get; set; }
        public string DetentionTypeId { get; set; }
        public bool isClose { get; set; }
        public bool isUpdate { get; set; }
        public string Process { get; set; }
        public string ProcessId { get; set; }
        public string AddedBy { get; set; }
        public string AddedFromIP { get; set; }
        public string AddedDate { get; set; }
        public string LogoutDate { get; set; }
        public string LogoutTime { get; set; }
        public string Duration { get; set; }
        public string UpdateRemarks { get; set; }
        public string DLRPId { get; set; }
        public string Department { get; set; }
        public string DepartmentId { get; set; }
    }
    public class CreateDetentionList
    {
        public string Id { get; set; }
        public string WorkCenterId { get; set; }
        public string DetentionTypeId { get; set; }
        public string MachineMasterId { get; set; }
        public string IssueByNo { get; set; }
        public DateTime LogoutTime { get; set; } = System.DateTime.Now;
        public bool isClose { get; set; }
        public string Remarks { get; set; }
        public string AddedBy { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
    #endregion Written by Nitesh

    #region WrittenBy Aman
    public class ActiveTask
    {
        public string Dated { get; set; }
        public string Counted { get; set; }
    }

    public class ChatTask
    {
        public string Id { get; set; }
        public string TaskManagerMasterId { get; set; }
        public string CreatedById { get; set; }
        public string CommentText { get; set; }
        public string EmployeeName { get; set; }
        public string EmpPicPath { get; set; }
       
    }

    public class AssignTaskDatals
    {
        public string Id { get; set; }
        public string TaskManagerMasterId { get; set; }
        public string AuthorizationType { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string EmployeeName { get; set; }
        public string EmpPicPath { get; set; }

    }
    public class Tasks
    {
        public string Id { get; set; }
        public string TaskDescription { get; set; }
        public string CurrentStatus { get; set; }
        public string TaskDetailDescription { get; set; }
        public string AuthorizationType { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string AddedDate { get; set; }
        public string DueDate { get; set; }
        public string CommitmentDate { get; set; }
    }
    #endregion WrittenBy Aman

    public class closeTask
    {
        public string Id { get; set; }
        public string TaskDescription { get; set; }
        public string CurrentStatus { get; set; }
        public string TaskDetailDescription { get; set; }
        public string ClosingDate { get; set; }
        public string ClosedBy { get; set; }
        public string AuthorizationType { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string AddedDate { get; set; }
        public string DueDate { get; set; }
        public string CommitmentDate { get; set; }
    }

    public class Default
    {
        public string Text { get; set; } = "";
        public string Value { get; set; } = "";
    }


}
