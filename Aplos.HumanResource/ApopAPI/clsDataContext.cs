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
using Library.Core;
using Library.Data.Sql;

namespace HRService
{
    public class clsDataContext
    {
        ISqlRepository _sqlRepository;
        public clsDataContext()
        {
            _sqlRepository = new SqlRepository();
        }

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

        public void getEmployeedetails(out List<EmployeeInfo> DataList, string EmployeeSysId)
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

WHERE ei.SystemId='" + EmployeeSysId + "'";
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
        public void getWorkcenter(out List<WorkCenterList> DataList, string processid)
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
                        where DM.DetentionTypeId = '" + detentionid + "'order by Text";
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
                            where DT.Id = '" + detentiontypeid + "'";
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
        public void GetDetentionLogDetail(out List<GetDetentionclose> DataList, string from, string to, string departmentId, string detentionTypeId)
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
left join dbo.EmployeeInformation As ei on tc.CreatedById = ei.SystemId where tc.TaskManagerMasterId = '" + Id + "'";

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

        public void GetResponsible(out List<Process> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Process>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select EmployeeCode as Value, EmployeeName As Text  from EmployeeInformation ";

                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Process
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

        public void GetActiveTask(out List<ActiveTask> DataList, string UserId, string Date)
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
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate > DATEADD(day, 7, '" + Date + @"')

Union All
select 'OverDueCreation' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType = 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate < '" + Date + @"'

Union All

select 'NextWeekCreation' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType = 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = DATEADD(day, 7, '" + Date + @"')

Union All
select 'TodayAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate = '" + Date + @"'

Union All
select 'FutureAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate > DATEADD(day, 7, '" + Date + @"')

Union All

select 'OverDueAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy'
and ta.ResponsiblePersonId = '" + UserId + "' and ta.DueDate < '" + Date + @"'

Union All

select 'NextWeekAssigned' As Dated, Count(ei.Id) Counted from dbo.TaskAudit As ta
LEFT JOIN dbo.TaskManagerMaster As ei on ta.TaskManagerMasterId = ei.Id  where ei.CurrentStatus <> 'Closed' and ta.AuthorizationType <> 'CreatedBy'
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


        // production service

        public string PostProductionService(IEnumerable<ProcessService> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.ProductionService";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<ProcessService> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.ProductionService where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (ProcessService item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);




                        dr["Id"] = "PS" + _Id;
                        dr["ProductionDate"] = item.ProductionDate;
                        dr["EntityId"] = item.EntityID;
                        dr["ProcessId"] = item.ProcessID;
                        dr["ShiftId"] = item.ShiftID;
                        dr["ResponsiblePerson"] = item.ResponsiblePerson;
                        dr["Entity"] = item.Entity;
                        dr["Process"] = item.Process;
                        dr["Shift"] = item.shift;
                        dr["ResponsiblePersonID"] = item.ResponsiblePersonID;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["EntityId"] = item.EntityID;
                        dr["ProcessId"] = item.ProcessID;
                        dr["ShiftId"] = item.ShiftID;
                        dr["ResponsiblePerson"] = item.ResponsiblePerson;
                        dr["Entity"] = item.Entity;
                        dr["Process"] = item.Process;
                        dr["Shift"] = item.shift;
                        dr["ResponsiblePersonID"] = item.ResponsiblePersonID;


                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;


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

        public string PostProductionServiceChild(IEnumerable<ProcessServiceChild> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.ProductionServiceChild";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                int i = 0;
                foreach (ProcessServiceChild item in DataToSave)
                {
                    con.OpenDataSetThroughAdapter("select * from dbo.ProductionServiceChild where Id='" + item.Id + "'", out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);


                        dr["Id"] = _Id;
                        dr["ProductionServiceId"] = item.ProductionServiceId;
                        dr["WorkcenterMasterId"] = item.WorkcenterMasterId;
                        dr["PO"] = item.PO;
                        dr["Value"] = item.Value;
                        dr["Remarks"] = item.Remarks;
                        dr["Detention1"] = item.Detention1;
                        dr["Detention1Time"] = item.Detention1Time;
                        dr["Detention2"] = item.Detention2;
                        dr["Detention2Time"] = item.Detention2Time;
                        dr["Detention3"] = item.Detention3;
                        dr["Detention3Time"] = item.Detention3Time;
                        dr["Detention4"] = item.Detention4;
                        dr["Detention4Time"] = item.Detention4Time;

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

                        dr["ProductionServiceId"] = item.ProductionServiceId;
                        dr["WorkcenterMasterId"] = item.WorkcenterMasterId;
                        dr["PO"] = item.PO;
                        dr["Value"] = item.Value;
                        dr["Remarks"] = item.Remarks;
                        dr["Detention1"] = item.Detention1;
                        dr["Detention1Time"] = item.Detention1Time;
                        dr["Detention2"] = item.Detention2;
                        dr["Detention2Time"] = item.Detention2Time;
                        dr["Detention3"] = item.Detention3;
                        dr["Detention3Time"] = item.Detention3Time;
                        dr["Detention4"] = item.Detention4;
                        dr["Detention4Time"] = item.Detention4Time;


                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;


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


        public string PostProductionServiceParameter(IEnumerable<ProcessServiceParameter> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.ProductionServiceParameterValue";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                int i = 0;
                foreach (ProcessServiceParameter item in DataToSave)
                {
                    con.OpenDataSetThroughAdapter("select * from TRN.ProductionServiceParameterValue where Id='" + item.Id + "'", out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);


                        dr["Id"] = _Id;
                        dr["ProductionServiceId"] = item.ProductionServiceId;
                        dr["StandardName"] = item.StandardName;
                        dr["Production100"] = item.Production100;
                        dr["Efficiency"] = item.Efficiency;
                        dr["Speed"] = item.Speed;
                        dr["ProductionShouldBe"] = item.ProductionShouldBe;
                        dr["TPI"] = item.TPI;
                        dr["NoOfSpindle"] = item.NoOfSpindle;
                        dr["MachineHank"] = item.MachineHank;
                        dr["Wrapping"] = item.Wrapping;
                        dr["ProductionActual"] = item.ProductionActual;
                        dr["DetentionInMin"] = item.DetentionInMin;
                        dr["ActualEfficiency"] = item.ActualEfficiency;
                        dr["Utilization"] = item.Utilization;
                        dr["AllottedManpower"] = item.AllottedManpower;
                        dr["ProdCapacityPerManpower"] = item.ProdCapacityPerManpower;
                        dr["WorkingHours"] = item.WorkingHours;

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

                        dr["ProductionServiceId"] = item.ProductionServiceId;
                        dr["StandardName"] = item.StandardName;
                        dr["Production100"] = item.Production100;
                        dr["Efficiency"] = item.Efficiency;
                        dr["Speed"] = item.Speed;
                        dr["ProductionShouldBe"] = item.ProductionShouldBe;
                        dr["TPI"] = item.TPI;
                        dr["NoOfSpindle"] = item.NoOfSpindle;
                        dr["MachineHank"] = item.MachineHank;
                        dr["Wrapping"] = item.Wrapping;
                        dr["ProductionActual"] = item.ProductionActual;
                        dr["DetentionInMin"] = item.DetentionInMin;
                        dr["ActualEfficiency"] = item.ActualEfficiency;
                        dr["Utilization"] = item.Utilization;
                        dr["AllottedManpower"] = item.AllottedManpower;
                        dr["ProdCapacityPerManpower"] = item.ProdCapacityPerManpower;
                        dr["WorkingHours"] = item.WorkingHours;


                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;


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
left join dbo.TaskAudit As ta on tm.Id = ta.TaskManagerMasterId  
where ta.AuthorizationType = 'AssignTo' and tm.CurrentStatus <> 'Closed' AND isnull(ta.isDone,0)=0
AND tm.TaskType IN ('ToDo','TNA','Issue')  and ta.ResponsiblePersonId = '" + UserId + "'";

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
'AssignTo' and tm.CurrentStatus = 'Closed' AND isnull(ta.isDone,0)=0 AND tm.TaskType IN ('ToDo','TNA','Issue') and ta.ResponsiblePersonId = '" + UserId + "'";

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
        public void GetDeshboard(out List<Default2> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"Select 'RequisitionCount' AS Name, count(x.Id) Value
                        FROM(select * from [TRN].[MaterialRequsitionMaster] MRM
                        Where MRM.CheckedByStatus = 'For Checking'
and  MRM.CheckedBy = '" + UserId + @"' 
union All
select * from [TRN].[MaterialRequsitionMaster] MRM
                        Where MRM.AuthorizedByStatus = 'For Approval' 
and MRM.AuthorizedBy = '" + UserId + @"') x
Union All
 Select 'POCount' AS Name, count(x.Id) Value
                       FROM( select * from [TRN].[PurchaseOrder] AS IR
                        Where IR.CheckedByStatus = 'For Checking'
and  IR.CheckedBy = '" + UserId + @"'
union All
select * from [TRN].[PurchaseOrder] AS IR
                        Where IR.AuthorizedByStatus = 'For Approval'
and IR.AuthorizedBy = '" + UserId + @"') x
Union All
Select 'GRNCount' AS Name, count(x.Id) Value
                        from( Select * from  trn.InventoryReceive AS IR
                        Where IR.CheckedByStatus = 'For Checking' 
and  IR.CheckedBy = '" + UserId + @"'
union All
 Select * from  trn.InventoryReceive AS IR
                        Where IR.AuthorizedByStatus = 'For Approval'
and IR.AuthorizedBy = '" + UserId + @"') x
Union All
Select 'ServicePOCount' AS Name, count(x.Id) Value
                        from  (select * from trn.ServicePOMaster AS IR
                         Where IR.CheckedByStatus = 'For Checking'
and  IR.CheckedBy = '" + UserId + @"' 
union All
Select * from  trn.ServicePOMaster AS IR
                        Where IR.ApprovedByStatus = 'For Approval'
and IR.ApprovedBy = '" + UserId + @"') x
Union All
Select 'ServiceAckCount' AS Name, count(x.Id) Value
                        from ( select * from trn.ServiceAcknowledgementMaster AS IR
                        Where IR.CheckedByStatus = 'For Checking'
and  IR.CheckedBy = '" + UserId + @"'
union All
Select * from  trn.ServiceAcknowledgementMaster AS IR
                        Where IR.ApprovedByStatus = 'For Approval'
and IR.ApprovedBy = '" + UserId + @"') x


Union All
select 'AdvanceCount' AS Name, Count(x.EmpSystemId) As Value  from(select * from 
TRN.EmployeeAdvanceRequisition where ApprovalStatus = 'ToBeApproved'
and  ApprovedBy = '" + UserId + @"' 
union All 
select * from 
TRN.EmployeeAdvanceRequisition where ApprovalStatus = 'ToBeChecked'
and  ApprovedBy = '" + UserId + @"') x
Union All
select 'ExpenseCount' AS Name , Count(EB.Id) As Value from  TRN.ExpenseBooking as EB 
left Join TRN.ExpenseBookingApprovalHistory as EBA  on EBA.ExpenseBookingId = EB.Id
where VoucherId Is Null and EB.ResponsiblePersonId = '" + UserId + @"' and EBA.ApprovalStatus = 'For Approval'

Union All
select 'IssueCount' AS Name, Count(x.Id) As Value from ( select * from TRN.IssueRequestMaster Where CheckedByStatus = 'For Checking'
and  CheckedBy = '" + UserId + @"' 
union All
select * from [TRN].[IssueRequestMaster]
                        Where AuthorizedByStatus = 'For Approval'
and AuthorizedBy = '" + UserId + @"') x

Union All 
select 'TaskCount' As Text, Count(ta.Id) As Value from dbo.TaskAudit as ta
left Join dbo.TaskManagerMaster as tm on tm.Id = ta.TaskManagerMasterId
where AuthorizationType <> 'CreatedBy' and isRead <> 1  and tm.CurrentStatus <> 'Closed' and ResponsiblePersonId = '" + UserId + @"'

Union All
select 'GatePassCount' As Text, COUNT(x.Id) As Value from ( select * from TRN.GatePassMaster Where GatePassStatus <> 'NonReturnable' and CheckedByStatus = 'For Checking'
and  CheckedBy = '" + UserId + @"' 
union All
select * from [TRN].[GatePassMaster]
                        Where ApprovedByStatus = 'For Approval' and GatePassStatus <> 'NonReturnable' 
and ApprovedBy = '" + UserId + @"') x

Union All
select 'LeaveCount' AS Name , Count(SystemID) As Value from dbo.LeaveTransaction   WHERE  IsNull(IsApproved,0) = 0
                             AND ISNULL(SystemID,'')<> ''
                             AND IsCancel=0
                             AND FirstApprovingStatus = 0  AND FirstApprovingAuthority = '" + UserId + @"'

Union All 
select 'QualityAction' , Count(distinct QC.Id) Value from TRN.QualityControlDetails QCD
left join TRN.QualityControl QC on QC.Id=QCD.QCId
left join ORG.Entity E on E.Id=QC.EntityId
left join hkp.Process P on P.Id=QC.ProcessId 
left join MST.QualityManagementMaster QMM on QMM.Id=QC.IssueId
left join EmployeeInformation EI on EI.SystemId=QC.ProductionInchargeId
left join TRN.ProductionOrder PO on PO.Id=QC.ProductionOrderId
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
where QCD.Status not in ('Close','Complete') and PS.UserName in ('Running','To Close') and 
QCD.GradeId in (select Id from MST.QualityGradeDetails where ActionApplicable=1)   and ResponsiblePersonId = '" + UserId + @"'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetEmployee(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select EmployeeCode As Value,EmployeeName As Name from EmployeeInformation Where EmployeeStatus = 'Active'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetEmployeeInColumn(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select CONCAT(EmployeeCode, '       ' , EmployeeName) as Name  , SystemId as Value from EmployeeInformation Where EmployeeStatus = 'Active'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetEmployeeSystem(out List<Default3> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default3>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select EmployeeCode As Value,EmployeeName As Name , SystemId from EmployeeInformation Where EmployeeStatus = 'Active'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default3
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),
                        SystemId = dsRef.Tables[0].Rows[i]["SystemId"].ToString(),

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

        public void GetReason(out List<Default2> DataList, string ProcessId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select dm.Id as Value, dm.DetentionUserName As Name from DetentionMasterProcess DMP
left join DetentionMaster  As dm on dm.Id = DMP.DetentionMasterId
where ProcessId = '" + ProcessId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetPODetail(out List<PODetail> DataList, string POId, string ProcessId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<PODetail>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select po.Id As POId , pos.StandardName As StandardName , ps.ProductionBookingLevel As BookingLevel  from TRN.ProductionOrder As po
left join HKP.ProductionStatus As pos   on po.ProductionStatusId = pos.Id 
left join trn.ProductionOrderProcessSet As ps on ps.ProductionOrderId = po.Id
where   po.Id = '" + POId + "' and ps.ProcessId = '" + ProcessId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new PODetail
                    {
                        POId = dsRef.Tables[0].Rows[i]["POId"].ToString(),
                        StandardName = dsRef.Tables[0].Rows[i]["StandardName"].ToString(),
                        BookingLevel = dsRef.Tables[0].Rows[i]["BookingLevel"].ToString(),

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

        public void GetWorkCenterId(out List<Default2> DataList, string WorkCenter)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select id As Value, UserName As Name from Scs.WorkCenterMaster where UserName = '" + WorkCenter + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetSalesReturnId(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id As Name, SalesId As Value from TRN.SalesReturn";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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


        public void GetCustomerName(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id As Value, StandardName As Name from HKP.Party Order By Sequence";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetShift(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct apd.ShiftSystemID Value,sd.ShiftDefinationName Name from AttdnProcessData apd
left join ShiftDefination sd on sd.systemid = apd.ShiftSystemID 
where  WorkDate = DATEADD(day, -1, CAST(GETDATE() AS date))
";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetProductionStatus(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id As Value, UserName As Name from HKP.ProductionStatus";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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


        public void GetTransactionQty(out List<Default2> DataList, string SalesReturnId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"Select SalesId As Value, TransactionQty As Name from TRN.SalesReturnDetail where SalesReturnId = '" + SalesReturnId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetPoStatusWise(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id As Name , ProductionStatusId As Value from TRN.ProductionOrder";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetPoStatusWiseNew(out List<Default2> DataList, string StatusId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id As Name , ProductionStatusId As Value from TRN.ProductionOrder where ProductionStatusId = '" + StatusId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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


        public void GetCartonBookedQty(out List<Weight> DataList, string SalesId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Weight>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Count(refno)CartonQty,
                isnull(Floor(Sum(ReturnNetWeight)),0)BookedQty from itemscanchild where Booked = 0  and SalesId = '" + SalesId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Weight
                    {
                        CartonQty = dsRef.Tables[0].Rows[i]["CartonQty"].ToString(),
                        BookedQty = dsRef.Tables[0].Rows[i]["BookedQty"].ToString(),

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

        public void GetWrongCarten(out List<Default2> DataList, string Refno, string SalesId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select SalesReturnId as Value, RefNo As Name from dbo.ItemScanChild where RefNo = '" + Refno + "' and SalesId = '" + SalesId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        #region Production
        public void GetProcessTagKg(out List<Default2> DataList, string ProcessId, string EntityId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select ProcessId As Name,IsParameterBased As value from hkp.EntityProcessTag where ProcessId = '" + ProcessId + "' and EntityId = '" + EntityId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetProductionParameterId(out List<Default2> DataList, string ProcessId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT ProcessId As Name, Id As value FROM dbo.ProductionBookingProcessParameter  where ProcessId = '" + ProcessId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetProductionOrderDetail(out List<ProductionEntryDetail> DataList, string ProcessId, string entityId, string productionDate, string shiftId, string Workcenter)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<ProductionEntryDetail>();

            System.Data.DataSet dsRef;
            try
            {
                #region SQl
                strSQL = @"SELECT distinct wc.Id as WorkCenterMasterId,wc.ProcessId,CAST (CASE WHEN pw.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,Cast(0 as bit) ClickRow,pw.PPQFlag,pw.Id,wc.UserName as WorkCenter,
                        isnull(pw.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + "' and WorkCenterMasterId=wc.Id order by AddedDate desc)) as ProductionOrderId,PO.IsPreDefineLotApplicable,(Case when IsPreDefineLotApplicable=1 then isnull((select top 1 CEILING(ProcessPlanQty) from ProductionOrderLotControl where UserLotNo=pw.LotNumber),(select top 1 CEILING(ProcessPlanQty) from ProductionOrderLotControl where UserLotNo=(select top 1 LotNumber from TRN.ProductionSummary where ProcessId = '" + ProcessId + "' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))) else 0 end) as LotProcessPlanQty,isnull(pw.LotNumber,(select top 1 LotNumber from TRN.ProductionSummary where ProcessId = '" + ProcessId + "' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc)) as LotNumber,M.EmployeeName as Mentor,
                        PI.EmployeeName as ProductionInCharge,PI.SystemId as ProductionInChargeId,
                        isnull(R.EmployeeName, (select EmployeeName from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterID = WC.Id order by AddedDate desc))) as ResponsiblePerson,
                        isnull(R.SystemId, (select SystemId from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterID = WC.Id order by AddedDate desc))) as ResponsiblePersonId,
                        isnull(I.EmployeeName, (select EmployeeName from EmployeeInformation where SystemId = (select top 1 InChargeId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterID = WC.Id order by AddedDate desc))) as InCharge,
                        isnull(I.SystemId, (select SystemId from EmployeeInformation where SystemId = (select top 1 InChargeId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterID = WC.Id order by AddedDate desc))) as InChargeId,
                        isnull(C.EmployeeName, (select EmployeeName from EmployeeInformation where SystemId = (select top 1 CheckedBy from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterId = wc.Id order by AddedDate desc))) as CheckedByName,pw.Quantity,isnull(pw.ProductionGrade, 'A') as ProductionGrade,pw.Remarks,isnull(SM.SumMinute, 0) as SumMin,
                        --ISNULL((CASE WHEN ISNULL(PPS.Qty, 0) = 0 THEN ISNULL(PQ.Qty, PO.PlannedQty) ELSE PO.PlannedQty * PPS.Qty / 100 END) - ISNULL(CEILING(PRS.TotalProductionQty), 0),0) RemainingQty, 
                        Case when  isnull(PPS.ProductionBookingLevel, (select ProductionBookingLevel from hkp.EntityProcessTag where EntityId = '" + entityId + "' and ProcessId = '" + ProcessId + @"')) = 'ProductionOrder' then isnull(PQ.Qty, POQ.POQty)/ POQ.POQty * SOP.OrderQty * PPS.Qty / 100 - ISNULL(CEILING(PRS.TotalProductionQty), 0) else 0 end RemainingQty,
                                      Case when isnull(PPS.ProductionBookingLevel, (select ProductionBookingLevel from hkp.EntityProcessTag where EntityId= '" + entityId + "' and ProcessId = '" + ProcessId + @"')) = 'ProductionOrder' then SOP.OrderQty else 0 end OrderQty,
                        --OrderQty = ISNULL(CASE WHEN ISNULL(PPS.Qty, 0) = 0 THEN ISNULL(CEILING(PQ.Qty), PO.PlannedQty) ELSE CEILING(PO.PlannedQty * PPS.Qty / 100) END, 0),
						--ISNULL(CEILING(PRS.TotalProductionQty), 0) as BookedQty,
                        Case when  isnull(PPS.ProductionBookingLevel, (select ProductionBookingLevel from hkp.EntityProcessTag where EntityId = '" + entityId + "' and ProcessId = '" + ProcessId + @"')) = 'ProductionOrder' then ISNULL(CEILING(PRS.TotalProductionQty), 0)  else 0 end BookedQty,
                            Case when isnull(PPS.ProductionBookingLevel, (select ProductionBookingLevel from hkp.EntityProcessTag where EntityId= '" + entityId + "' and ProcessId = '" + ProcessId + @"')) = 'ProductionOrder' then POQ.POQty else 0 end POQty,
                              Case when isnull(PPS.ProductionBookingLevel, (select ProductionBookingLevel from hkp.EntityProcessTag where EntityId= '" + entityId + "' and ProcessId = '" + ProcessId + @"')) = 'ProductionOrder' then isnull(PQ.Qty,POQ.POQty)/POQ.POQty*SOP.OrderQty*PPS.Qty/100 else 0 end ProcessPlanQty,
isnull(PQ.Qty, POQ.POQty)/ POQ.POQty * SOP.OrderQty * PPS.Qty / 100 - ISNULL(CEILING(PRS.TotalProductionQty), 0) as CurPOBalProd,isnull(PPP.TotalProductionQty,0)  as POPreviousProdQty,
        isnull(PQ.Qty, POQ.POQty) as ActualPlannedQty,PPS.Qty ProcessPlanPercentage, RM.TargetProductionFP,isnull(PPS.ProductionBookingLevel, (select ProductionBookingLevel from hkp.EntityProcessTag where EntityId = '" + entityId + "' and ProcessId = '" + ProcessId + @"')) as BookingLevel,pw.SalesOrderId,pw.MasterOrderItemId,'' as ReasonId,'' as ReasonName,PPS.Sequence POProcessSequence,PPS.IsProductionVerification ProductionVerification,isnull(FPP.FirstProductionQty,0) POFirstProcessProductionQty,
(select MA.StandardName from trn.salesorder SO
left outer join trn.MasterOrderItem MOI ON MOI.Id = SO.MasterOrderItemId
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
where SO.Id = pw.SalesOrderId) as SOArticle,pw.MasterOrderItemId,(select MA.StandardName from trn.MasterOrderItem MOI
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
where MOI.Id = pw.MasterOrderItemId) as MOIArticle,(select MA.StandardName from trn.MasterOrderItem MOI
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
where MOI.Id = pw.MasterOrderItemId) as ProductCodeArticle,
                                       Article = STUFF((select distinct ',' + MA.StandardName from trn.ProductionOrderDetail Pod
   
                                                               left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId = so.Id
                                                            left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
                                                            left outer join[MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
                                                            where Pod.ProductionOrderId = isnull(pw.ProductionOrderId, (select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterId = wc.Id order by AddedDate desc))    for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
						               SONo = STUFF((select distinct ',' + sox.Id from trn.MasterOrderItem XMOI
    
                                                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id

                                                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id

                                                            where podx.ProductionOrderId = isnull(pw.ProductionOrderId, (select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterId = wc.Id order by AddedDate desc))   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                       Customer = STUFF((select distinct ',' + XP.UserName from
                                                                trn.SalesOrder XSO
    
                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                                            left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId

                                                            left outer join[HKP].[Party] Xp on XP.Id = XMO.PartyId

                                                            where Xpod.ProductionOrderId = isnull(pw.ProductionOrderId, (select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterId = wc.Id order by AddedDate desc))   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                      ProductCode = STUFF((select distinct ',' + PM.Code from trn.ProductionOrderDetail Pod
    
                                                                left outer JOIN trn.SalesOrder SO ON pod.SalesOrderId = so.Id
                                                            left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
                                                            left outer join mst.MaterialMaster mm on mm.id = MOI.MaterialMasterId
                                                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId = mm.Id
                                                            left outer join[MST].[ProductMaster] PM on pm.id = pd.ProductMasterId
                                                            where Pod.ProductionOrderId = isnull(pw.ProductionOrderId, (select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterId = wc.Id order by AddedDate desc))    for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
						             ProductDetails = STUFF((select distinct ',' + PM.UserName from trn.ProductionOrderDetail Pod
    
                                                                left outer JOIN trn.SalesOrder SO ON pod.SalesOrderId = so.Id
                                                            left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
                                                            left outer join mst.MaterialMaster mm on mm.id = MOI.MaterialMasterId
                                                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId = mm.Id
                                                            left outer join[MST].[ProductMaster] PM on pm.id = pd.ProductMasterId
                                                            where Pod.ProductionOrderId = isnull(pw.ProductionOrderId, (select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterId = wc.Id order by AddedDate desc))    for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
						             CustomerRefNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from trn.MasterOrder XMOI
   
                                                               INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId = XMOI.Id

                                                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = moi.Id

                                                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id

                                                            where podx.ProductionOrderId = isnull(pw.ProductionOrderId, (select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterId = wc.Id order by AddedDate desc))   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') 
                        FROM SCS.WorkCenterMaster wc
                        LEFT JOIN TRN.ProductionSummary pw ON pw.WorkCenterMasterId = wc.Id AND pw.ProcessId = '" + ProcessId + @"'
                        AND pw.EntityId = '" + entityId + "' AND PW.ProductionDate = '" + productionDate + "' AND PW.ProductionShiftId = '" + shiftId + @"'
                        LEFT JOIN trn.ProductionOrder AS PO ON PO.ID = isnull(pw.ProductionOrderId, (select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + "'  and EntityId = '" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterId = wc.Id order by AddedDate desc))
						LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id AND PPS.ProcessId = '" + ProcessId + @"'
                        LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID = PO.Id
LEFT JOIN (select SUM(PP.Quantity)TotalProductionQty, PP.ProductionOrderId from [TRN].[ProductionSummary] PP where PP.ProcessId = 
(select ProcessId from TRN.ProductionOrderProcessSet B where B.ProductionOrderId=PP.ProductionOrderId  and B.Sequence =
(select top 1 Sequence=Sequence - 1  from TRN.ProductionOrderProcessSet A where A.ProductionOrderId=PP.ProductionOrderId
and A.ProcessId='" + ProcessId + @"')) GROUP BY PP.ProductionOrderId
 ) AS PPP ON PPP.ProductionOrderId = PO.Id
LEFT JOIN (select Sum(FP.Quantity) as FirstProductionQty, FP.ProductionOrderId from [TRN].[ProductionSummary] FP where FP.ProcessId = 
(select ProcessId from TRN.ProductionOrderProcessSet B where B.ProductionOrderId=FP.ProductionOrderId  and B.Sequence = 
(select top 1 Sequence from TRN.ProductionOrderProcessSet A where A.ProductionOrderId=FP.ProductionOrderId and A.IsProductionVerification=1)) GROUP BY FP.ProductionOrderId
 ) AS FPP ON FPP.ProductionOrderId = PO.Id
                          LEFT JOIN
                            (SELECT SUM(SO.Qty) OrderQty, PD.ProductionOrderId
                            FROM TRN.SalesOrder SO

                            left join TRN.ProductionOrderDetail PD ON PD.SalesOrderId= SO.Id 

                            where SO.OrderStatusId<>'Cancelled' GROUP BY PD.ProductionOrderId
                            ) AS SOP ON SOP.ProductionOrderId = PO.Id
                            LEFT JOIN
                            (SELECT SUM(SO.Qty) POQty, PD.ProductionOrderId
                            FROM TRN.SalesOrder SO

                            left join TRN.ProductionOrderDetail PD ON PD.SalesOrderId= SO.Id

                            where SO.OrderStatusId<>'Cancelled' GROUP BY PD.ProductionOrderId
                            ) AS POQ ON POQ.ProductionOrderId = PO.Id

                         LEFT JOIN
                            (SELECT SUM(PS.Quantity) TotalProductionQty, PS.ProductionOrderId
                            FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + ProcessId + @"'   GROUP BY PS.ProductionOrderId
                            ) AS PRS ON PRS.ProductionOrderId = PO.Id
                        LEFT JOIN EmployeeInformation R ON PW.ResponsiblePersonId = R.SystemId
                        LEFT JOIN EmployeeInformation M ON PW.MentorId = M.SystemId
                        LEFT JOIN EmployeeInformation C ON PW.CheckedBy = C.SystemId
                        LEFT JOIN EmployeeInformation I ON PW.InChargeId = I.SystemId
                        LEFT JOIN EmployeeInformation PI ON PW.ProductionInChargeId = PI.SystemId
                        LEFT JOIN TRN.RunningMachineSetUpTarget RM ON RM.EntityId = '" + entityId + "' and RM.ProcessId = '" + ProcessId + "'  and RM.TargetDate = '" + productionDate + "' and RM.ProductionShiftId = '" + shiftId + @"' and RM.WorkCenterMasterId = wc.Id and RM.ProductionOrderId = pw.ProductionOrderId

                        LEFT JOIN(select ISNULL(sum(Minute),0) as SumMinute,WorkCenterId, ProductionSummaryId from MachineMasterTransaction MT where MT.ProcessId = '" + ProcessId + "'  and MT.EntityId = '" + entityId + "' AND MT.Date = '" + productionDate + "' AND MT.ShiftId = '" + shiftId + @"'
                        group by WorkCenterId,ProductionSummaryId) SM ON SM.WorkCenterId = wc.Id and SM.ProductionSummaryId = pw.Id
                        where wc.Active = 1 and wc.ProcessId = '" + ProcessId + "'  and wc.EntityId = '" + entityId + "' and wc.UserName = '" + Workcenter + "' order by wc.UserName";
                #endregion SQl
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new ProductionEntryDetail
                    {
                        WorkCenterMasterId = dsRef.Tables[0].Rows[i]["WorkCenterMasterId"].ToString(),
                        ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                        Flag = dsRef.Tables[0].Rows[i]["Flag"].ToString(),
                        ClickRow = dsRef.Tables[0].Rows[i]["ClickRow"].ToString(),
                        PPQFlag = dsRef.Tables[0].Rows[i]["PPQFlag"].ToString(),
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        WorkCenter = dsRef.Tables[0].Rows[i]["WorkCenter"].ToString(),
                        ProductionOrderId = dsRef.Tables[0].Rows[i]["ProductionOrderId"].ToString(),
                        IsPreDefineLotApplicable = dsRef.Tables[0].Rows[i]["IsPreDefineLotApplicable"].ToString(),
                        LotProcessPlanQty = dsRef.Tables[0].Rows[i]["LotProcessPlanQty"].ToString(),
                        LotNumber = dsRef.Tables[0].Rows[i]["LotNumber"].ToString(),
                        Mentor = dsRef.Tables[0].Rows[i]["Mentor"].ToString(),
                        ProductionInCharge = dsRef.Tables[0].Rows[i]["ProductionInCharge"].ToString(),
                        ProductionInChargeId = dsRef.Tables[0].Rows[i]["ProductionInChargeId"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        InCharge = dsRef.Tables[0].Rows[i]["InCharge"].ToString(),
                        InChargeId = dsRef.Tables[0].Rows[i]["InChargeId"].ToString(),
                        CheckedByName = dsRef.Tables[0].Rows[i]["CheckedByName"].ToString(),
                        Quantity = dsRef.Tables[0].Rows[i]["Quantity"].ToString(),
                        ProductionGrade = dsRef.Tables[0].Rows[i]["ProductionGrade"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        SumMin = dsRef.Tables[0].Rows[i]["SumMin"].ToString(),
                        RemainingQty = dsRef.Tables[0].Rows[i]["RemainingQty"].ToString(),
                        OrderQty = dsRef.Tables[0].Rows[i]["OrderQty"].ToString(),
                        BookedQty = dsRef.Tables[0].Rows[i]["BookedQty"].ToString(),
                        POQty = dsRef.Tables[0].Rows[i]["POQty"].ToString(),
                        ProcessPlanQty = dsRef.Tables[0].Rows[i]["ProcessPlanQty"].ToString(),
                        CurPOBalProd = dsRef.Tables[0].Rows[i]["CurPOBalProd"].ToString(),
                        POPreviousProdQty = dsRef.Tables[0].Rows[i]["POPreviousProdQty"].ToString(),
                        ActualPlannedQty = dsRef.Tables[0].Rows[i]["ActualPlannedQty"].ToString(),
                        ProcessPlanPercentage = dsRef.Tables[0].Rows[i]["ProcessPlanPercentage"].ToString(),
                        TargetProductionFP = dsRef.Tables[0].Rows[i]["TargetProductionFP"].ToString(),
                        BookingLevel = dsRef.Tables[0].Rows[i]["BookingLevel"].ToString(),
                        SalesOrderId = dsRef.Tables[0].Rows[i]["SalesOrderId"].ToString(),
                        MasterOrderItemId = dsRef.Tables[0].Rows[i]["MasterOrderItemId"].ToString(),
                        ReasonId = dsRef.Tables[0].Rows[i]["ReasonId"].ToString(),
                        ReasonName = dsRef.Tables[0].Rows[i]["ReasonName"].ToString(),
                        POProcessSequence = dsRef.Tables[0].Rows[i]["POProcessSequence"].ToString(),
                        ProductionVerification = dsRef.Tables[0].Rows[i]["ProductionVerification"].ToString(),
                        POFirstProcessProductionQty = dsRef.Tables[0].Rows[i]["POFirstProcessProductionQty"].ToString(),
                        SOArticle = dsRef.Tables[0].Rows[i]["SOArticle"].ToString(),
                        MOIArticle = dsRef.Tables[0].Rows[i]["MOIArticle"].ToString(),
                        ProductCodeArticle = dsRef.Tables[0].Rows[i]["ProductCodeArticle"].ToString(),
                        Article = dsRef.Tables[0].Rows[i]["Article"].ToString(),
                        SONo = dsRef.Tables[0].Rows[i]["SONo"].ToString(),
                        Customer = dsRef.Tables[0].Rows[i]["Customer"].ToString(),
                        ProductCode = dsRef.Tables[0].Rows[i]["ProductCode"].ToString(),
                        ProductDetails = dsRef.Tables[0].Rows[i]["ProductDetails"].ToString(),
                        CustomerRefNo = dsRef.Tables[0].Rows[i]["CustomerRefNo"].ToString(),


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

        public void GetProductionParameter(out List<Default2> DataList, string ParameterId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select SandardName As Name , Id As Value from dbo.ProductionBookingParameter
where ProductionBookingProcessParameterId='" + ParameterId + "' and EntryState = 'Entry' and Active = 1";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetProductionCalculate(out List<Default2> DataList, string ParameterId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select FormulaId Value , Formula Name from dbo.ProductionBookingParameter
where ProductionBookingProcessParameterId='" + ParameterId + "' and EntryState = 'Calculate' and Active = 1";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetProductionCalculateValue(out List<Default2> DataList, string Formula)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select " + Formula + " Value , 'Name' Name";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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



        #endregion Production
        public void GetPoWisereport(out List<POWiseReport> DataList, string POId, string POStatusId, string CustomerId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            string strSQL1 = "";
            DataList = new List<POWiseReport>();

            System.Data.DataSet dsRef;
            try
            {
                #region Sql
                strSQL1 = @"DECLARE @POCreationDate varchar(100)=DATEADD(day,-180,GETDate())
                            SELECT x.ProcessIndex,X.EntityId,X.Entity,X.CustomerId ,X.Customer,X.Article,X.SONo,X.POId PONo,X.POStatusId,X.POStatus,X.AddedBy,X.AddedDate,X.UpdatedBy,X.UpdatedDate,X.SOQty,X.BaseProcPlanPercentage,X.ActualPlanScheduleQty,X.ShouldBeBaseProcessPlannedQty
                            ,X.BaseProcessProduceQty,X.BaseProcessRemainingQty,X.Sequence,X.ProcessId,X.Process,X.PercentQty,X.ProcessPlannedQty,X.ProcProdQty,X.PreProcProdQty,X.WIP,X.ProcBalanceToProduce,X.RelayProcess,X.IsBaseProcess
                            ,X.ProcessLegDays,X.POFirstDelivery,X.POLastDelivery,X.BaseProcProdStartDate,X.BaseProcLatestProdDate,X.BaseProcPlanStartDate,X.BaseProcPlanCompletionDate
                            ,X.POStartDate,X.POCompletionDate,X.FirstProcessActualBookDate,X.POFirstProdBookDate,X.POLatestProdBookDate,X.ShouldBeProcessStartDate,X.ShouldBeProcessEndDate
                            ,ISNULL(X.ProcessFirstBookDate,'-')ProcessFirstBookDate,ISNULL(X.ProcessLatestBookDate,'-')ProcessLatestBookDate,X.ProcessStartDays,X.ProcessEndDays,X.ProcessPlanPercent,X.ProcessStatus,X.FirstProcessWC,X.ProcLossPercent,X.ProcLossQty,X.BaseProcProdPerenct
                            ,ROUND(X.ProcProdPercent*100,0)ProcProdPercent,X.EntryCheck,ROUND(X.ProceessProdQtyVsSOQty*100,0)ProceessProdQtyVsSOQty,ISNULL(X.Remarks,'-') ProcessStatusRemark--,X.ProcessProdBookDate
                            ,POReviewStatus=CASE WHEN CONVERT(datetime,X.ProcessLatestBookDate)< (GETDATE()-2) THEN 'To Review' ELSE X.POStatus END
                            
                            ,LotNoQty=ISNULL(STUFF((select distinct ', '+xp.LotNumber+'-'+CONVERT(varchar(100),X.ProcProdQty) from
                            TRN.ProductionSummary AS xp
                            where X.POId=xp.ProductionOrderId for xml path('') ), 1, 1, ''),'-')
                            ,ISNULL(X.InputRecoveryPercentage,0)InputRecoveryPercentage,ActualInputPlanPercentage=ROUND((X.FirstProcessProQty/NULLIF(X.BaseProcessProduceQty,0))*100,0)
,LatestProcessProdBookDays=CASE WHEN DATEDIFF(day,X.ProcessLatestBookDate,GETDATE()) IS NULL THEN 'Entry Missing' ELSE CONVERT(Varchar(100),DATEDIFF(day,X.ProcessLatestBookDate,GETDATE())) END
,ProcessReviewStatus=CASE WHEN DATEDIFF(day,X.ProcessLatestBookDate,GETDATE())>2 THEN 'To Review' ELSE  'NA' END,ProcessBalanceProd=X.ProcessPlannedQty-X.ProcProdQty
                            FROM(
                            SELECT 
                            T1.*,ISNULL(T2.ProcProdQty,0) PreProcProdQty,WIP=case when T1.Sequence=1 then 0 else ISNULL(ISNULL(T2.ProcProdQty,0)-ISNULL(T1.ProcProdQty,0),0) end, ProcLossPercent=ISNULL(t2.PercentQty-t1.PercentQty,0)
                            ,ProcLossQty=ISNULL(T2.ProcessPlannedQty-T1.ProcessPlannedQty,0),BaseProcProdPerenct=ISNULL(t2.BaseProcessProduceQty/t2.BaseProcessPlannedQty,0)
                            ,ProcProdPercent=ISNULL(T1.ProcProdQty/t1.ProcessPlannedQty,0)
                            ,EntryCheck=CASE WHEN T2.ProcProdQty-T1.ProcProdQty<0 THEN 'ToCheck' ELSE '' END
                            ,ProceessProdQtyVsSOQty=COALESCE(T1.ProcProdQty / NULLIF(T1.SOQty ,0), 0)
                            FROM
                            (Select ROW_NUMBER() OVER(partition by A.POId ORDER BY A.Sequence) ProcessIndex,A.*
                            from (select E.Id EntityId,E.UserName Entity,P.Id POId,PRS.Id POStatusId,PRS.UserName POStatus,P.AddedBy,Format(P.AddedDate,'dd-MMM-yyyy')AddedDate,P.UpdatedBy,Format(P.UpdatedDate,'dd-MMM-yyyy')UpdatedDate
                            --,SOQty=P.Qty*PSQ.Qty/100
                            ,SOQty=(select SUM(xp.Qty) from trn.SalesOrder AS xp
                                INNER JOIN TRN.ProductionOrderDetail PD ON pd.SalesOrderId=xp.id
                                where P.Id=PD.ProductionOrderId)
                           
                            ,BaseProcPlanPercentage=(Select Qty from TRN.ProductionOrderProcessSet Where IsBaseProcess=1 AND ProductionOrderId=P.id)
                            ,ActualPlanScheduleQty=PQ.Qty
                            ,(PQ.Qty*(Select Qty from TRN.ProductionOrderProcessSet Where IsBaseProcess=1 AND ProductionOrderId=P.id)/100) ShouldBeBaseProcessPlannedQty
                            ,ISNULL(PS.Quantity,0) BaseProcessProduceQty
                            ,ISNULL(FPSQ.Quantity,0) FirstProcessProQty
                            ,PQ.Qty-ISNULL(PS.Quantity,0) BaseProcessRemainingQty
                            ,PSQ.Sequence,PRO.Id ProcessId,PRO.UserName Process
                            ,PSQ.Qty PercentQty
                            ,ProcessPlannedQty=(CASE WHEN PSQ.IsBaseProcess=1 THEN PQ.Qty ELSE PQ.Qty*PSQ.Qty/100 END)
                            ,ISNULL(PBQ.ProcProdQty,0) ProcProdQty
                            ,ProcBalanceToProduce=ISNULL((CASE WHEN PSQ.IsBaseProcess=1 THEN PQ.Qty ELSE PQ.Qty*PSQ.Qty/100 END)-PBQ.ProcProdQty,0)
                            ,RelayProcess=CASE WHEN PSQ.IsCompleted=1 THEN 'Yes' ELSE 'No' End
                            ,PSQ.IsBaseProcess,PSQ.Remarks
                            ,ProcessLegDays= CASE WHEN PSQ.Symbol='+' THEN CONVERT(varchar(100),PSQ.Days) ELSE ISNULL((PSQ.Symbol+''+CONVERT(varchar(100),PSQ.Days)),0) END
                            ,FORMAT(POD.POFirstDelivery,'dd-MMM-yyyy')POFirstDelivery,FORMAT(POD.POLastDelivery,'dd-MMM-yyyy')POLastDelivery
                            ,FORMAT(BASEP.BaseProcProdStartDate,'dd-MMM-yyyy')BaseProcProdStartDate,FORMAT(BASEP.BaseProcLatestProdDate,'dd-MMM-yyyy')BaseProcLatestProdDate,ISNULL(FORMAT(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy'),'') BaseProcPlanStartDate,ISNULL(FORMAT(Type1.BaseProcPlanCompletionDate,'dd-MMM-yyyy'),'')BaseProcPlanCompletionDate

                            ,POStartDate=FORMAT(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  < Type1.BaseProcPlanStartDate  then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end,'dd-MMM-yyyy')

                            ,POCompletionDate=FORMAT((case when Type1.BaseProcPlanCompletionDate is null or BASEP.BaseProcLatestProdDate  > Type1.BaseProcPlanCompletionDate  then BASEP.BaseProcLatestProdDate else Type1.BaseProcPlanCompletionDate end),'dd-MMM-yyyy')

                            ,FirstProcessActualBookDate=FORMAT(BASEP.BaseProcProdStartDate,'dd-MMM-yyyy')

                            ,FORMAT(FBPPD.POFirstProdBookDate,'dd-MMM-yyyy')POFirstProdBookDate
                            ,FORMAT(FBPPD.POLatestProdBookDate,'dd-MMM-yyyy')POLatestProdBookDate

                            ,ShouldBeProcessStartDate=FORMAT(DATEADD(DAY,PSQ.Days
                            ,(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  < Type1.BaseProcPlanStartDate  then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end)),'dd-MMM-yyyy')

                            ,ShouldBeProcessEndDate=FORMAT(DATEADD(DAY,PSQ.Days
                            ,(case when Type1.BaseProcPlanCompletionDate is null or BASEP.BaseProcLatestProdDate  > Type1.BaseProcPlanCompletionDate  then BASEP.BaseProcLatestProdDate else Type1.BaseProcPlanCompletionDate end)),'dd-MMM-yyyy')

                            ,FORMAT(PBQ.ProcessFirstBookDate,'dd-MMM-yyyy')ProcessFirstBookDate,FORMAT(PBQ.ProcessLatestBookDate,'dd-MMM-yyyy')ProcessLatestBookDate
                            ,ProcessStartDays=DateDiff(Day,PBQ.ProcessFirstBookDate,GETDate())
                            ,ProcessEndDays=DateDiff(Day,PBQ.ProcessLatestBookDate,GETDate())

                            ,ProcessPlanPercent=PSQ.Qty

                            ,ProcessStatus= CASE WHEN PBQ.ProcProdQty>=ISNULL(CASE WHEN ISNULL(PSQ.Qty,0)=0 THEN ISNULL(PQ.Qty,P.PlannedQty) ELSE P.PlannedQty*PSQ.Qty/100 END,0) THEN 'Complete'
					                            WHEN PBQ.ProcProdQty=0 THEN 'To Start' WHEN PBQ.ProcProdQty>0 THEN 'Running' ELSE 'To Check' END
                            ,FirstProcessWC=ISNULL(STUFF((select distinct ','+xw.UserName from
                            ProductionOrderFirstProcessWorkCenter AS xp
                            INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
                            where P.Id=xp.ProductionOrderId for xml path('') ), 1, 1, ''),'')

                            ,InputRecoveryPercentage=STUFF((select distinct ', '+CONVERT(Varchar(100),xp.PlanPercentage) from
                            dbo.MaterialIssueControlMaster AS xp
                            where P.Id=xp.POId for xml path('') ), 1, 1, '')

                            ,Customer=STUFF((select distinct ','+XP.UserName from 
		                                      trn.SalesOrder XSO 
		                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                      left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                      left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
		                                      where P.Id=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
											   ,CustomerId=STUFF((select distinct ','+XP.Id from 
		                                      trn.SalesOrder XSO 
		                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                      left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                      left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
		                                      where P.Id=Xpod.ProductionOrderId	 for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,Article=STUFF((select distinct ','+XMO.StandardName from 
		                                      trn.SalesOrder XSO 
		                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                      left outer join MST.MaterialMasterArticle XMO on Xmo.Id=Xmoi.ArticleId
		                                      where P.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,SONo=STUFF((select distinct ','+XSO.Id from 
		                                      trn.SalesOrder XSO 
		                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id		                                      
		                                      where P.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                            from TRN.ProductionOrder P
                            Left JOIN ORG.Entity E ON E.Id=P.EntityId
                            LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id
                            LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID = P.Id
                            LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
                            LEFT JOIN HKP.Process PRO ON PRO.Id=PSQ.ProcessId

                            LEFT JOIN (Select MIN(SO.DeliveryDate) POFirstDelivery,MAX(SO.DeliveryDate) POLastDelivery,PD.ProductionOrderId FROM TRN.SalesOrder SO
                            LEFT JOIN TRN.ProductionOrderDetail PD ON PD.SalesOrderId=SO.Id GROUP BY PD.ProductionOrderId)POD ON POD.ProductionOrderId=P.Id
                            LEFT JOIN(Select MIN(ProductionDate)BaseProcProdStartDate,MAX(ProductionDate)BaseProcLatestProdDate,A.ProductionOrderId From TRN.ProductionSummary A
                            LEFT JOIN HKP.Process B ON B.Id=A.ProcessId
                            Group By A.ProductionOrderId) BASEP ON BASEP.ProductionOrderId=P.Id
                            LEFT JOIN(Select SUM(Quantity)ProQty,MIN(ProductionDate)POFirstProdBookDate,MAX(ProductionDate)POLatestProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=P.Id
                            LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanCompletionDate,ProductionOrderId From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=P.Id

                            LEFT JOIN(Select B.ProductionOrderId,SUM(Quantity)Quantity from TRN.ProductionSummary B
                            left join TRN.ProductionOrderProcessSet A ON A.ProductionOrderId=B.ProductionOrderId AND B.ProcessId=A.ProcessId Where A.IsBaseProcess=1 Group BY B.ProductionOrderId) PS ON P.Id=PS.ProductionOrderId
                            LEFT JOIN (Select SUM(Quantity)ProcProdQty, MIN(ProductionDate)ProcessFirstBookDate,MAX(ProductionDate)ProcessLatestBookDate ,ProductionOrderId,ProcessId from TRN.ProductionSummary Group BY ProductionOrderId,ProcessId) PBQ ON P.Id=PBQ.ProductionOrderId AND PBQ.ProcessId=PRO.Id
                            LEFT JOIN(Select B.ProductionOrderId,SUM(Quantity)Quantity from TRN.ProductionSummary B
                            left join TRN.ProductionOrderProcessSet A ON A.ProductionOrderId=B.ProductionOrderId AND B.ProcessId=A.ProcessId Where A.Sequence=1 Group BY B.ProductionOrderId) FPSQ ON P.Id=FPSQ.ProductionOrderId
                            Where P.AddedDate>= @POCreationDate 
                            GROUP BY E.Id,E.UserName,P.Id,PSQ.Sequence,PRO.Id,PRO.UserName,P.PlannedQty,P.Qty,PSQ.Qty,PRS.Id,PRS.UserName,P.AddedBy,P.AddedDate,P.UpdatedBy,P.UpdatedDate,PQ.Qty,PBQ.ProcProdQty,PSQ.IsCompleted,PSQ.IsBaseProcess
                            ,PSQ.Days,PSQ.Symbol,POD.POFirstDelivery,POD.POLastDelivery,BASEP.BaseProcProdStartDate,BASEP.BaseProcLatestProdDate,BASEP.BaseProcLatestProdDate,Type1.BaseProcPlanStartDate
                            ,Type1.BaseProcPlanCompletionDate,Type1.BaseProcPlanStartDate,FBPPD.POFirstProdBookDate,FBPPD.POLatestProdBookDate,PBQ.ProcessFirstBookDate,PBQ.ProcessLatestBookDate,PS.Quantity,PSQ.Remarks,FPSQ.Quantity--,BPP.ProcessPlannedQty 
                            ) A )T1
                            LEFT JOIN (Select ROW_NUMBER() OVER(partition by A.POId ORDER BY A.Sequence)+1 ProcessIndex,A.*
                            from (select 

                            (PQ.Qty*(Select Qty from TRN.ProductionOrderProcessSet Where IsBaseProcess=1 AND ProductionOrderId=P.id)/100) BaseProcessPlannedQty
                            ,PS.Quantity BaseProcessProduceQty

                            ,PSQ.Qty PercentQty
                            ,ProcessPlannedQty=(CASE WHEN PSQ.IsBaseProcess=1 THEN PQ.Qty ELSE PQ.Qty*PSQ.Qty/100 END)
                            ,P.Id POId,PSQ.Sequence,PBQ.ProcProdQty

                            from TRN.ProductionOrder P
                            Left JOIN ORG.Entity E ON E.Id=P.EntityId
                            LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id
                            LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID = P.Id
                            LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
                            LEFT JOIN HKP.Process PRO ON PRO.Id=PSQ.ProcessId
                            LEFT JOIN(Select B.ProductionOrderId,SUM(Quantity)Quantity from TRN.ProductionSummary B
                            left join TRN.ProductionOrderProcessSet A ON A.ProductionOrderId=B.ProductionOrderId  AND B.ProcessId=A.ProcessId Where A.IsBaseProcess=1 Group BY B.ProductionOrderId ) PS ON P.Id=PS.ProductionOrderId
                            LEFT JOIN (Select SUM(Quantity)ProcProdQty,ProductionOrderId,ProcessId from TRN.ProductionSummary Group BY ProductionOrderId,ProcessId) PBQ ON P.Id=PBQ.ProductionOrderId AND PBQ.ProcessId=PRO.Id
                            Where P.AddedDate>= @POCreationDate 
                            GROUP BY P.Id,PSQ.Sequence,P.Qty,PSQ.Qty,PSQ.IsBaseProcess,PQ.Qty,PBQ.ProcProdQty,PS.Quantity 
                            ) A )T2 ON T1.ProcessIndex=T2.ProcessIndex AND  T1.POId=T2.POId

                            )X";
                if (CustomerId != null && POId == null && POStatusId == null)
                {
                    strSQL = strSQL1 + " Where x.CustomerId = '" + CustomerId + "' Order By X.POId,X.ProcessIndex,X.Sequence,X.Process";
                }
                if (CustomerId == null && POId != null && POStatusId == null)
                {
                    strSQL = strSQL1 + " Where x.POId = '" + POId + "' Order By X.POId,X.ProcessIndex,X.Sequence,X.Process";
                }
                if (CustomerId == null && POId == null && POStatusId != null)
                {
                    strSQL = strSQL1 + " Where x.POStatusId = '" + POStatusId + "' Order By X.POId,X.ProcessIndex,X.Sequence,X.Process";
                }
                if (CustomerId != null && POId != null && POStatusId == null)
                {
                    strSQL = strSQL1 + " Where x.CustomerId = '" + CustomerId + "' and x.POId = '" + POId + "' Order By X.POId,X.ProcessIndex,X.Sequence,X.Process";
                }
                if (CustomerId != null && POId == null && POStatusId != null)
                {
                    strSQL = strSQL1 + " Where x.CustomerId = '" + CustomerId + "' and x.POStatusId = '" + POStatusId + "' Order By X.POId,X.ProcessIndex,X.Sequence,X.Process";
                }
                if (CustomerId == null && POId != null && POStatusId != null)
                {
                    strSQL = strSQL1 + " Where x.POId = '" + POId + "' and x.POStatusId = '" + POStatusId + "' Order By X.POId,X.ProcessIndex,X.Sequence,X.Process";
                }
                if (CustomerId != null && POId != null && POStatusId != null)
                {
                    strSQL = strSQL1 + " Where x.POId = '" + POId + "' and x.POStatusId = '" + POStatusId + "' and x.CustomerId = '" + CustomerId + "'  Order By X.POId,X.ProcessIndex,X.Sequence,X.Process";
                }

                #endregion Sql
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new POWiseReport
                    {
                        ProcessIndex = dsRef.Tables[0].Rows[i]["ProcessIndex"].ToString(),
                        EntityId = dsRef.Tables[0].Rows[i]["EntityId"].ToString(),
                        Entity = dsRef.Tables[0].Rows[i]["Entity"].ToString(),
                        CustomerId = dsRef.Tables[0].Rows[i]["CustomerId"].ToString(),
                        Customer = dsRef.Tables[0].Rows[i]["Customer"].ToString(),
                        Article = dsRef.Tables[0].Rows[i]["Article"].ToString(),
                        SONo = dsRef.Tables[0].Rows[i]["SONo"].ToString(),
                        PONo = dsRef.Tables[0].Rows[i]["PONo"].ToString(),
                        POStatusId = dsRef.Tables[0].Rows[i]["POStatusId"].ToString(),
                        POStatus = dsRef.Tables[0].Rows[i]["POStatus"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        UpdatedBy = dsRef.Tables[0].Rows[i]["UpdatedBy"].ToString(),
                        UpdatedDate = dsRef.Tables[0].Rows[i]["UpdatedDate"].ToString(),
                        SOQty = dsRef.Tables[0].Rows[i]["SOQty"].ToString(),
                        BaseProcPlanPercentage = dsRef.Tables[0].Rows[i]["BaseProcPlanPercentage"].ToString(),
                        ActualPlanScheduleQty = dsRef.Tables[0].Rows[i]["ActualPlanScheduleQty"].ToString(),
                        ShouldBeBaseProcessPlannedQty = dsRef.Tables[0].Rows[i]["ShouldBeBaseProcessPlannedQty"].ToString(),
                        BaseProcessProduceQty = dsRef.Tables[0].Rows[i]["BaseProcessProduceQty"].ToString(),
                        BaseProcessRemainingQty = dsRef.Tables[0].Rows[i]["BaseProcessRemainingQty"].ToString(),
                        Sequence = dsRef.Tables[0].Rows[i]["Sequence"].ToString(),
                        ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                        Process = dsRef.Tables[0].Rows[i]["Process"].ToString(),
                        PercentQty = dsRef.Tables[0].Rows[i]["PercentQty"].ToString(),
                        ProcessPlannedQty = dsRef.Tables[0].Rows[i]["ProcessPlannedQty"].ToString(),
                        ProcProdQty = dsRef.Tables[0].Rows[i]["ProcProdQty"].ToString(),
                        PreProcProdQty = dsRef.Tables[0].Rows[i]["PreProcProdQty"].ToString(),
                        WIP = dsRef.Tables[0].Rows[i]["WIP"].ToString(),
                        ProcBalanceToProduce = dsRef.Tables[0].Rows[i]["ProcBalanceToProduce"].ToString(),
                        RelayProcess = dsRef.Tables[0].Rows[i]["RelayProcess"].ToString(),
                        IsBaseProcess = dsRef.Tables[0].Rows[i]["IsBaseProcess"].ToString(),
                        ProcessLegDays = dsRef.Tables[0].Rows[i]["ProcessLegDays"].ToString(),
                        POFirstDelivery = dsRef.Tables[0].Rows[i]["POFirstDelivery"].ToString(),
                        POLastDelivery = dsRef.Tables[0].Rows[i]["POLastDelivery"].ToString(),
                        BaseProcProdStartDate = dsRef.Tables[0].Rows[i]["BaseProcProdStartDate"].ToString(),
                        BaseProcLatestProdDate = dsRef.Tables[0].Rows[i]["BaseProcLatestProdDate"].ToString(),
                        BaseProcPlanStartDate = dsRef.Tables[0].Rows[i]["BaseProcPlanStartDate"].ToString(),
                        BaseProcPlanCompletionDate = dsRef.Tables[0].Rows[i]["BaseProcPlanCompletionDate"].ToString(),
                        POStartDate = dsRef.Tables[0].Rows[i]["POStartDate"].ToString(),
                        POCompletionDate = dsRef.Tables[0].Rows[i]["POCompletionDate"].ToString(),
                        FirstProcessActualBookDate = dsRef.Tables[0].Rows[i]["FirstProcessActualBookDate"].ToString(),
                        POFirstProdBookDate = dsRef.Tables[0].Rows[i]["POFirstProdBookDate"].ToString(),
                        POLatestProdBookDate = dsRef.Tables[0].Rows[i]["POLatestProdBookDate"].ToString(),
                        ShouldBeProcessStartDate = dsRef.Tables[0].Rows[i]["ShouldBeProcessStartDate"].ToString(),
                        ShouldBeProcessEndDate = dsRef.Tables[0].Rows[i]["ShouldBeProcessEndDate"].ToString(),
                        ProcessFirstBookDate = dsRef.Tables[0].Rows[i]["ProcessFirstBookDate"].ToString(),
                        ProcessLatestBookDate = dsRef.Tables[0].Rows[i]["ProcessLatestBookDate"].ToString(),
                        ProcessStartDays = dsRef.Tables[0].Rows[i]["ProcessStartDays"].ToString(),
                        ProcessEndDays = dsRef.Tables[0].Rows[i]["ProcessEndDays"].ToString(),
                        ProcessPlanPercent = dsRef.Tables[0].Rows[i]["ProcessPlanPercent"].ToString(),
                        ProcessStatus = dsRef.Tables[0].Rows[i]["ProcessStatus"].ToString(),
                        FirstProcessWC = dsRef.Tables[0].Rows[i]["FirstProcessWC"].ToString(),
                        ProcLossPercent = dsRef.Tables[0].Rows[i]["ProcLossPercent"].ToString(),
                        ProcLossQty = dsRef.Tables[0].Rows[i]["ProcLossQty"].ToString(),
                        BaseProcProdPerenct = dsRef.Tables[0].Rows[i]["BaseProcProdPerenct"].ToString(),
                        ProcProdPercent = dsRef.Tables[0].Rows[i]["ProcProdPercent"].ToString(),
                        EntryCheck = dsRef.Tables[0].Rows[i]["EntryCheck"].ToString(),
                        ProceessProdQtyVsSOQty = dsRef.Tables[0].Rows[i]["ProceessProdQtyVsSOQty"].ToString(),
                        ProcessStatusRemark = dsRef.Tables[0].Rows[i]["ProcessStatusRemark"].ToString(),
                        POReviewStatus = dsRef.Tables[0].Rows[i]["POReviewStatus"].ToString(),
                        LotNoQty = dsRef.Tables[0].Rows[i]["LotNoQty"].ToString(),
                        InputRecoveryPercentage = dsRef.Tables[0].Rows[i]["InputRecoveryPercentage"].ToString(),
                        ActualInputPlanPercentage = dsRef.Tables[0].Rows[i]["ActualInputPlanPercentage"].ToString(),
                        LatestProcessProdBookDays = dsRef.Tables[0].Rows[i]["LatestProcessProdBookDays"].ToString(),
                        ProcessReviewStatus = dsRef.Tables[0].Rows[i]["ProcessReviewStatus"].ToString(),
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
        #endregion Written By Aman

        #region Test For production service
        public string PostProductionServiceTest(IEnumerable<ProcessServiceTest> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.ProductionService";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<ProcessServiceTest> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from TRN.ProductionService where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (ProcessServiceTest item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);




                        dr["Id"] = "PS" + _Id;
                        dr["ProductionDate"] = item.ProductionDate;
                        dr["EntityId"] = item.EntityID;
                        dr["ProcessId"] = item.ProcessID;
                        dr["ShiftId"] = item.ShiftID;
                        dr["ResponsiblePerson"] = item.ResponsiblePerson;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["EntityId"] = item.EntityID;
                        dr["ProductionDate"] = item.ProductionDate;
                        dr["ProcessId"] = item.ProcessID;
                        dr["ShiftId"] = item.ShiftID;
                        dr["ResponsiblePerson"] = item.ResponsiblePerson;


                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;


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

        #region Attendance
        public void GetUserGroup(out List<Default2> DataList, string EmpsysId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct RGM.id as Value, RGM.UserGroup as Name from TRN.HrreportmasterResponsiblePerson RP
left join HKP.HRReportMaster HRM on HRM.Id = RP.HRReportMasterId
left join TRN.HRReportMasterChild  HRC on  HRC.HRReportMasterId = HRM.Id
left join TRN.HRReportMasterBudgetUserGroup BG on BG.HRReportMasterChildId = HRC.Id
left join HKP.HRReportGroupMaster RGM on RGM.Id = BG.UserGroupId where RP.EmpSystemId = '" + EmpsysId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetSevenDaysAttendance(out List<SevenDaysAttdn> DataList, string Empcode)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<SevenDaysAttdn>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct format(WorkDate, 'dd-MMM-yyy') as Date,
case when DayStatus is  null then InStatus
else DayStatus end as DayStatus , format(InTime,'dd-MMM-yyyy hh:mm tt') InTime ,format(OutTime,'dd-MMM-yyyy hh:mm tt') OutTime
from AttdnProcessData
where WorkDate between DATEADD(day, -7, CAST(GETDATE() AS date)) and GETDATE() and EmpSystemID = '" + Empcode + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new SevenDaysAttdn
                    {
                        Date = dsRef.Tables[0].Rows[i]["Date"].ToString(),
                        DayStatus = dsRef.Tables[0].Rows[i]["DayStatus"].ToString(),
                        InTime = dsRef.Tables[0].Rows[i]["InTime"].ToString(),
                        OutTime = dsRef.Tables[0].Rows[i]["OutTime"].ToString(),

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



        public void GetSevenDaysAttendanceDefault(out List<SevenDaysAttdn> DataList, string Empcode)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<SevenDaysAttdn>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct format(WorkDate, 'dd-MMM-yyy') as Date,
case when DayStatus is  null then InStatus
else DayStatus end as DayStatus , format(InTime,'dd-MMM-yyyy hh:mm tt') InTime ,format(OutTime,'dd-MMM-yyyy hh:mm tt') OutTime
from AttdnProcessData
where WorkDate between DATEADD(day, -7, CAST(GETDATE() AS date)) and GETDATE() and EmpSystemID = '" + Empcode + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new SevenDaysAttdn
                    {
                        Date = dsRef.Tables[0].Rows[i]["Date"].ToString(),
                        DayStatus = dsRef.Tables[0].Rows[i]["DayStatus"].ToString(),
                        InTime = dsRef.Tables[0].Rows[i]["InTime"].ToString(),
                        OutTime = dsRef.Tables[0].Rows[i]["OutTime"].ToString(),

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

        public void GetEmpInformation(out List<EmpInformation> DataList, string Empcode)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<EmpInformation>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct EMP.SystemID,EMP.EmployeeCode EMPCode, EMP.EmployeeName EmployeeName, SC.StandardName Section,SBC.StandardName SubSection, 
DSG.StandardName Designation ,MBGT.Code BudgetCode, sd.ShiftDefinationName Shift , format(Emp.DOJ , 'yyyy-MMM-dd') DOJ, EC.UserName as EmpType

From EmployeeInformation EMP 
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Section SC on SC.Id = EMP.SectionId
left join ORG.SubSection SBC on SBC.Id = EMP.SubSectionId
LEFT JOIN hkp.Designation DSG on DSG.id = Emp.DesignationSystemID
left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
left join ORG.Position POS on POS.Id = MBGT.PositionId
left join mst.DesignationMaster DM on DM.DesignationId = POS.DesignationId
left join HKP.EmployeeCategory EC on EC.Id = Dm.EmployeeCategoryId
where Emp.EmployeeCode = '" + Empcode + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new EmpInformation
                    {
                        SystemID = dsRef.Tables[0].Rows[i]["SystemID"].ToString(),
                        EMPCode = dsRef.Tables[0].Rows[i]["EMPCode"].ToString(),
                        EmployeeName = dsRef.Tables[0].Rows[i]["EmployeeName"].ToString(),
                        Section = dsRef.Tables[0].Rows[i]["Section"].ToString(),
                        SubSection = dsRef.Tables[0].Rows[i]["SubSection"].ToString(),
                        Designation = dsRef.Tables[0].Rows[i]["Designation"].ToString(),
                        BudgetCode = dsRef.Tables[0].Rows[i]["BudgetCode"].ToString(),
                        Shift = dsRef.Tables[0].Rows[i]["Shift"].ToString(),
                        DOJ = dsRef.Tables[0].Rows[i]["DOJ"].ToString(),
                        EmpType = dsRef.Tables[0].Rows[i]["EmpType"].ToString(),

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


        public void GetLocation(out List<Locations> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Locations>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct Location from dbo.ResidenceMaster";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Locations
                    {
                        Location = dsRef.Tables[0].Rows[i]["Location"].ToString(),

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

        public void GetAttdnreport(out List<AttendanceReport> DataList, string date, string shiftid, string groupid, string inmis, string locations, string entityid, string tbs, string longabsent)
        {
            if (inmis == "IN" || inmis == "IM" || inmis == "W")
            {

                clsConnectionManager objCon = null;
                string strSQL = "";
                string strSQL1 = "";
                DataList = new List<AttendanceReport>();

                System.Data.DataSet dsRef;
                try
                {
                    #region Sql
                    strSQL1 = @"select * from (  select ROW_NUMBER() OVER(ORDER BY (ISNULL(A.ToDayIN,0)-MBGT.Deployment)) AS SrNo,  LTY.Code LeaveCode,EMP.SystemID,EMP.EmployeeCode EMPCode, EMP.EmployeeName EmployeeName, SC.StandardName Section,SBC.StandardName SubSection, 
DSG.StandardName Designation,x.StandardName Category, POS.Activity,apd.InStatus, UN.Id EntityId,UN.UserName EntityName,
case when apd.WeeklyStatus = 'W' then 'W'
when (select top 1 rw.PTime from AttdnRawData rw
where rw.LogDownLoadNum = apd.EmpSystemID and rw.PDate = apd.WorkDate AND rw.PType <> 'OUT'
order by rw.PTime asc) is null then 'IM'
else 'IN' end as RawDayStatus ,
(select top 1 rw.PTime from AttdnRawData rw
where rw.LogDownLoadNum = apd.EmpSystemID and rw.PDate = apd.WorkDate AND rw.PType <> 'OUT'
order by rw.PTime asc) InTime,pv.InTime as InVerificationTime, MBGT.Code BudgetCode, sd.ShiftDefinationName Shift, sd.SystemID as ShiftId, emp.CellPhnNo MobileNo,apd.WeeklyStatus, RG.StandardName Residence, TG.StandardName Transport,
Hrg.ManpowerBudgetId, Hg.UserGroup , Hg.Id as GroupId , RM.Location as Location , Emp.EmployeeCurrentStatus as CurrentStatus ,MBGT.Deployment,ISNULL(A.ToDayIN , 0) as ToDayIN, Diffenence= ISNULL(A.ToDayIN,0)-MBGT.Deployment ,
case when (ISNULL(A.ToDayIN,0)-MBGT.Deployment) > 0 then 'Excess' 
when (ISNULL(A.ToDayIN,0)-MBGT.Deployment) < 0 then 'Short' 
else 'Ok' end DifferenceColor
from AttdnProcessData apd 
left Join EmployeeInformation EMP on EMP.SystemId = apd.EmpSystemID
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN hkp.Designation DSG on DSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
LEFT JOIN MST.DesignationMasterLegalDesignation DMLD on DMLD.LegalDesignationId = GDSG.Id
left join mst.DesignationMaster dm on dm.Id = DMLD.DesignationMasterId
left join scs.designationmasterconfiguration dmc on dmc.designationmasterid = dm.id
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
left join mst.LegalSalaryGradeDesignation GRD on GRD.legaldesignationid = gdsg.id
left join scs.legalsalarygrade lsg on lsg.id = grd.legalsalarygradeid
left join scs.legalsalarygradehead lsh on lsh.legalsalarygradeid = lsg.id
left join mst.LegalSalaryStructure lss on lss.legalsalarygradeid = lsh.legalsalarygradeid
left join mst.LegalSalaryStructureValue lsv on lsv.legalsalarystructureid = lss.id
left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
left join SalaryRuleMaster SRM on srm.systemid = dmc.salaryrulemasterid
left join EmployeeBankInfo BNK on BNK.EmpSystemID = emp.SystemId
left join ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId
left join TransportGroup TG on TG.Id = EMP.TransportGroupId
left join employeecodetype ect on ect.id = emp.employeecodetypeid
left join hkp.Process PR on PR.Id = POS.ProcessId
left join scs.District DT on DT.Id = emp.ParmDistrictID
left join scs.[State] ST on ST.Id = EMP.ParmStateId
left join dbo.PhysicalVerification pv on pv.EmpSystemID = apd.EmpSystemID and pv.WorkDate = apd.WorkDate
left join TRN.HRReportMasterChild Hrg on Hrg.ManpowerBudgetId = Emp.BudgetCode
left join TRN.HRReportMasterBudgetUserGroup HBG on HBG.HRReportMasterChildId = Hrg.Id
left join HKP.HRReportGroupMaster Hg on Hg.Id = HBG.UserGroupId
left join LeaveTransaction LT on LT.EmpSystemID = apd.EmpSystemID and (LT.FromDate <= apd.WorkDate and LT.ToDate >= apd.WorkDate)
left join LeaveType LTY on LTY.Id = LT.LTSystemID
left join ResidenceAllocatedEmployees RA on RA.EmployeeSystemId = apd.EmpSystemID
left join ResidenceMaster RM on RM.Id = RA.ResidenceId
LEFT JOIN (Select ISNULL(COUNT(EmpSystemID), 0) ToDayIN,BudgetId from dbo.AttdnProcessData Where WorkDate= '" + date + @"' AND ISNULL(InTime,'')<>'' Group BY BudgetId) A ON A.BudgetId=MBGT.Id 
where emp.employeecode is not null and emp.employeestatus = 'Active' and MBGT.code is not null and MBGT.Active = 1
and emp.employeecode NOT IN (2222229, 2222230)  and apd.WorkDate = '" + date + "'  and Hg.Id = '" + groupid + "'";



                    if (inmis == "IN" || inmis == "IM" || inmis == "W")
                    {
                        strSQL = strSQL1 + "and (case when apd.WeeklyStatus = 'W' then 'W' when(select top 1 rw.PTime from AttdnRawData rw where rw.LogDownLoadNum = apd.EmpSystemID and rw.PDate = apd.WorkDate order by rw.PTime asc) is null then 'IM' else 'IN' end) = '" + inmis + "'";
                    }

                    if (entityid != null && shiftid == null && locations == null)
                    {
                        strSQL = strSQL + "  and MBGT.EntityId =  '" + entityid + "'";
                    }
                    if (entityid == null && shiftid != null && locations == null)
                    {
                        strSQL = strSQL + "  and sd.SystemID =  '" + shiftid + "'";
                    }
                    if (entityid == null && shiftid == null && locations != null)
                    {
                        strSQL = strSQL + " and  RM.Location = '" + locations + "'";
                    }
                    if (entityid != null && shiftid != null && locations == null)
                    {
                        strSQL = strSQL + "  and MBGT.EntityId =  '" + entityid + "'" + "  and sd.SystemID =  '" + shiftid + "'";
                    }
                    if (entityid == null && shiftid != null && locations != null)
                    {
                        strSQL = strSQL + "  and sd.SystemID =  '" + shiftid + "'" + " and  RM.Location = '" + locations + "'";
                    }
                    if (entityid != null && shiftid == null && locations != null)
                    {
                        strSQL = strSQL + " and  RM.Location = '" + locations + "'" + "  and MBGT.EntityId =  '" + entityid + "'";
                    }
                    if (entityid != null && shiftid != null && locations != null)
                    {
                        strSQL = strSQL + " and  RM.Location = '" + locations + "'" + "  and MBGT.EntityId =  '" + entityid + "'" + "  and sd.SystemID =  '" + shiftid + "'";
                    }

                    if (tbs != null && longabsent == null)
                    {
                        strSQL = strSQL + " and Emp.EmployeeCurrentStatus = 'TBS'  ) a order by a.SrNo";
                    }

                    if (tbs == null && longabsent != null)
                    {
                        strSQL = strSQL + " and Emp.EmployeeCurrentStatus = 'LONG ABSENTEEISM'  ) a order by a.SrNo";
                    }
                    if (tbs == null && longabsent == null)
                    {
                        strSQL = strSQL + " and Emp.EmployeeCurrentStatus is null  ) a order by a.SrNo";
                    }




                    #endregion Sql
                    objCon = new clsConnectionManager();
                    objCon.BeginTransaction();
                    objCon.getDataSet(strSQL, out dsRef);
                    objCon.CommitTransaction();
                    for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                    {
                        DataList.Add(new AttendanceReport
                        {
                            SrNo = dsRef.Tables[0].Rows[i]["SrNo"].ToString(),
                            LeaveCode = dsRef.Tables[0].Rows[i]["LeaveCode"].ToString(),
                            SystemID = dsRef.Tables[0].Rows[i]["SystemID"].ToString(),
                            EMPCode = dsRef.Tables[0].Rows[i]["EMPCode"].ToString(),
                            EmployeeName = dsRef.Tables[0].Rows[i]["EmployeeName"].ToString(),
                            Section = dsRef.Tables[0].Rows[i]["Section"].ToString(),
                            SubSection = dsRef.Tables[0].Rows[i]["SubSection"].ToString(),
                            Designation = dsRef.Tables[0].Rows[i]["Designation"].ToString(),
                            Category = dsRef.Tables[0].Rows[i]["Category"].ToString(),
                            Activity = dsRef.Tables[0].Rows[i]["Activity"].ToString(),
                            InStatus = dsRef.Tables[0].Rows[i]["InStatus"].ToString(),
                            InTime = dsRef.Tables[0].Rows[i]["InTime"].ToString(),
                            InVerificationTime = dsRef.Tables[0].Rows[i]["InVerificationTime"].ToString(),
                            BudgetCode = dsRef.Tables[0].Rows[i]["BudgetCode"].ToString(),
                            Shift = dsRef.Tables[0].Rows[i]["Shift"].ToString(),
                            ShiftId = dsRef.Tables[0].Rows[i]["ShiftId"].ToString(),
                            MobileNo = dsRef.Tables[0].Rows[i]["MobileNo"].ToString(),
                            WeeklyStatus = dsRef.Tables[0].Rows[i]["WeeklyStatus"].ToString(),
                            Residence = dsRef.Tables[0].Rows[i]["Residence"].ToString(),
                            Transport = dsRef.Tables[0].Rows[i]["Transport"].ToString(),
                            ManpowerBudgetId = dsRef.Tables[0].Rows[i]["ManpowerBudgetId"].ToString(),
                            UserGroup = dsRef.Tables[0].Rows[i]["UserGroup"].ToString(),
                            GroupId = dsRef.Tables[0].Rows[i]["GroupId"].ToString(),
                            Location = dsRef.Tables[0].Rows[i]["Location"].ToString(),
                            CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                            Deployment = dsRef.Tables[0].Rows[i]["Deployment"].ToString(),
                            ToDayIN = dsRef.Tables[0].Rows[i]["ToDayIN"].ToString(),
                            Diffenence = dsRef.Tables[0].Rows[i]["Diffenence"].ToString(),
                            RawDayStatus = dsRef.Tables[0].Rows[i]["RawDayStatus"].ToString(),
                            EntityId = dsRef.Tables[0].Rows[i]["EntityId"].ToString(),
                            EntityName = dsRef.Tables[0].Rows[i]["EntityName"].ToString(),
                            DifferenceColor = dsRef.Tables[0].Rows[i]["DifferenceColor"].ToString(),

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
            else if (inmis == "LateIn")
            {
                clsConnectionManager objCon = null;
                string strSQL = "";
                string strSQL1 = "";
                DataList = new List<AttendanceReport>();

                System.Data.DataSet dsRef;
                try
                {
                    #region Sql
                    strSQL1 = @"select * from (  select ROW_NUMBER() OVER(ORDER BY (ISNULL(A.ToDayIN,0)-MBGT.Deployment)) AS SrNo,  LTY.Code LeaveCode,EMP.SystemID,EMP.EmployeeCode EMPCode, EMP.EmployeeName EmployeeName, SC.StandardName Section,SBC.StandardName SubSection, 
DSG.StandardName Designation,x.StandardName Category, POS.Activity,apd.InStatus, UN.Id EntityId,UN.UserName EntityName,
case when apd.WeeklyStatus = 'W' then 'W'
when (select top 1 rw.PTime from AttdnRawData rw
where rw.LogDownLoadNum = apd.EmpSystemID and rw.PDate = apd.WorkDate AND rw.PType <> 'OUT'
order by rw.PTime asc) is null then 'IM'
else 'IN' end as RawDayStatus ,
(select top 1 rw.PTime from AttdnRawData rw
where rw.LogDownLoadNum = apd.EmpSystemID and rw.PDate = apd.WorkDate AND rw.PType <> 'OUT'
order by rw.PTime asc) InTime,pv.InTime as InVerificationTime, MBGT.Code BudgetCode, sd.ShiftDefinationName Shift, sd.SystemID as ShiftId, emp.CellPhnNo MobileNo,apd.WeeklyStatus, RG.StandardName Residence, TG.StandardName Transport,
Hrg.ManpowerBudgetId, Hg.UserGroup , Hg.Id as GroupId , RM.Location as Location , Emp.EmployeeCurrentStatus as CurrentStatus ,MBGT.Deployment,ISNULL(A.ToDayIN , 0) as ToDayIN, Diffenence= ISNULL(A.ToDayIN,0)-MBGT.Deployment ,
case when (ISNULL(A.ToDayIN,0)-MBGT.Deployment) > 0 then 'Access' 
when (ISNULL(A.ToDayIN,0)-MBGT.Deployment) < 0 then 'Short' 
else 'Ok' end DifferenceColor,
case when apd.InStatus = 'EI' then cast((apd.ShiftInTime - apd.PunchInTime) as time(0)) 
when apd.PunchInTime > apd.ShiftInTime then cast((apd.PunchInTime - apd.ShiftInTime) as time(0))
else '00:00:00' end LateInTime,
case when apd.PunchInTime < apd.ShiftInTime then 'EI'
else 'LI' end LateInStatus
from AttdnProcessData apd 
left Join EmployeeInformation EMP on EMP.SystemId = apd.EmpSystemID
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN hkp.Designation DSG on DSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
LEFT JOIN MST.DesignationMasterLegalDesignation DMLD on DMLD.LegalDesignationId = GDSG.Id
left join mst.DesignationMaster dm on dm.Id = DMLD.DesignationMasterId
left join scs.designationmasterconfiguration dmc on dmc.designationmasterid = dm.id
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
left join mst.LegalSalaryGradeDesignation GRD on GRD.legaldesignationid = gdsg.id
left join scs.legalsalarygrade lsg on lsg.id = grd.legalsalarygradeid
left join scs.legalsalarygradehead lsh on lsh.legalsalarygradeid = lsg.id
left join mst.LegalSalaryStructure lss on lss.legalsalarygradeid = lsh.legalsalarygradeid
left join mst.LegalSalaryStructureValue lsv on lsv.legalsalarystructureid = lss.id
left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
left join SalaryRuleMaster SRM on srm.systemid = dmc.salaryrulemasterid
left join EmployeeBankInfo BNK on BNK.EmpSystemID = emp.SystemId
left join ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId
left join TransportGroup TG on TG.Id = EMP.TransportGroupId
left join employeecodetype ect on ect.id = emp.employeecodetypeid
left join hkp.Process PR on PR.Id = POS.ProcessId
left join scs.District DT on DT.Id = emp.ParmDistrictID
left join scs.[State] ST on ST.Id = EMP.ParmStateId
left join dbo.PhysicalVerification pv on pv.EmpSystemID = apd.EmpSystemID and pv.WorkDate = apd.WorkDate
left join TRN.HRReportMasterChild Hrg on Hrg.ManpowerBudgetId = Emp.BudgetCode
left join TRN.HRReportMasterBudgetUserGroup HBG on HBG.HRReportMasterChildId = Hrg.Id
left join HKP.HRReportGroupMaster Hg on Hg.Id = HBG.UserGroupId
left join LeaveTransaction LT on LT.EmpSystemID = apd.EmpSystemID and (LT.FromDate <= apd.WorkDate and LT.ToDate >= apd.WorkDate)
left join LeaveType LTY on LTY.Id = LT.LTSystemID
left join ResidenceAllocatedEmployees RA on RA.EmployeeSystemId = apd.EmpSystemID
left join ResidenceMaster RM on RM.Id = RA.ResidenceId
LEFT JOIN (Select ISNULL(COUNT(EmpSystemID), 0) ToDayIN,BudgetId from dbo.AttdnProcessData Where WorkDate= '" + date + @"' AND ISNULL(InTime,'')<>'' Group BY BudgetId) A ON A.BudgetId=MBGT.Id 
where emp.employeecode is not null and emp.employeestatus = 'Active' and MBGT.code is not null and MBGT.Active = 1
and emp.employeecode NOT IN (2222229, 2222230) and (case when apd.InStatus = 'EI' then cast((apd.ShiftInTime - apd.PunchInTime) as time(0)) 
when apd.PunchInTime > apd.ShiftInTime then cast((apd.PunchInTime - apd.ShiftInTime) as time(0))
else '00:00:00' end) <> '00:00:00'   and apd.WorkDate = '" + date + "'  and Hg.Id = '" + groupid + "'";

                    if (inmis == "LateIn")
                    {
                        strSQL = strSQL1;
                    }

                    if (entityid != null && shiftid == null && locations == null)
                    {
                        strSQL = strSQL + "  and MBGT.EntityId =  '" + entityid + "'";
                    }
                    if (entityid == null && shiftid != null && locations == null)
                    {
                        strSQL = strSQL + "  and sd.SystemID =  '" + shiftid + "'";
                    }
                    if (entityid == null && shiftid == null && locations != null)
                    {
                        strSQL = strSQL + " and  RM.Location = '" + locations + "'";
                    }
                    if (entityid != null && shiftid != null && locations == null)
                    {
                        strSQL = strSQL + "  and MBGT.EntityId =  '" + entityid + "'" + "  and sd.SystemID =  '" + shiftid + "'";
                    }
                    if (entityid == null && shiftid != null && locations != null)
                    {
                        strSQL = strSQL + "  and sd.SystemID =  '" + shiftid + "'" + " and  RM.Location = '" + locations + "'";
                    }
                    if (entityid != null && shiftid == null && locations != null)
                    {
                        strSQL = strSQL + " and  RM.Location = '" + locations + "'" + "  and MBGT.EntityId =  '" + entityid + "'";
                    }
                    if (entityid != null && shiftid != null && locations != null)
                    {
                        strSQL = strSQL + " and  RM.Location = '" + locations + "'" + "  and MBGT.EntityId =  '" + entityid + "'" + "  and sd.SystemID =  '" + shiftid + "'";
                    }

                    if (tbs != null && longabsent == null)
                    {
                        strSQL = strSQL + " and Emp.EmployeeCurrentStatus = 'TBS' ) a order by a.SrNo";
                    }

                    if (tbs == null && longabsent != null)
                    {
                        strSQL = strSQL + " and Emp.EmployeeCurrentStatus = 'LONG ABSENTEEISM' ) a order by a.SrNo";
                    }
                    if (tbs == null && longabsent == null)
                    {
                        strSQL = strSQL + " and Emp.EmployeeCurrentStatus is null ) a order by a.SrNo";
                    }

                    #endregion Sql
                    objCon = new clsConnectionManager();
                    objCon.BeginTransaction();
                    objCon.getDataSet(strSQL, out dsRef);
                    objCon.CommitTransaction();
                    for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                    {
                        DataList.Add(new AttendanceReport
                        {
                            SrNo = dsRef.Tables[0].Rows[i]["SrNo"].ToString(),
                            LeaveCode = dsRef.Tables[0].Rows[i]["LeaveCode"].ToString(),
                            SystemID = dsRef.Tables[0].Rows[i]["SystemID"].ToString(),
                            EMPCode = dsRef.Tables[0].Rows[i]["EMPCode"].ToString(),
                            EmployeeName = dsRef.Tables[0].Rows[i]["EmployeeName"].ToString(),
                            Section = dsRef.Tables[0].Rows[i]["Section"].ToString(),
                            SubSection = dsRef.Tables[0].Rows[i]["SubSection"].ToString(),
                            Designation = dsRef.Tables[0].Rows[i]["Designation"].ToString(),
                            Category = dsRef.Tables[0].Rows[i]["Category"].ToString(),
                            Activity = dsRef.Tables[0].Rows[i]["Activity"].ToString(),
                            InStatus = dsRef.Tables[0].Rows[i]["InStatus"].ToString(),
                            InTime = dsRef.Tables[0].Rows[i]["InTime"].ToString(),
                            InVerificationTime = dsRef.Tables[0].Rows[i]["InVerificationTime"].ToString(),
                            BudgetCode = dsRef.Tables[0].Rows[i]["BudgetCode"].ToString(),
                            Shift = dsRef.Tables[0].Rows[i]["Shift"].ToString(),
                            ShiftId = dsRef.Tables[0].Rows[i]["ShiftId"].ToString(),
                            MobileNo = dsRef.Tables[0].Rows[i]["MobileNo"].ToString(),
                            WeeklyStatus = dsRef.Tables[0].Rows[i]["WeeklyStatus"].ToString(),
                            Residence = dsRef.Tables[0].Rows[i]["Residence"].ToString(),
                            Transport = dsRef.Tables[0].Rows[i]["Transport"].ToString(),
                            ManpowerBudgetId = dsRef.Tables[0].Rows[i]["ManpowerBudgetId"].ToString(),
                            UserGroup = dsRef.Tables[0].Rows[i]["UserGroup"].ToString(),
                            GroupId = dsRef.Tables[0].Rows[i]["GroupId"].ToString(),
                            Location = dsRef.Tables[0].Rows[i]["Location"].ToString(),
                            CurrentStatus = dsRef.Tables[0].Rows[i]["CurrentStatus"].ToString(),
                            Deployment = dsRef.Tables[0].Rows[i]["Deployment"].ToString(),
                            ToDayIN = dsRef.Tables[0].Rows[i]["ToDayIN"].ToString(),
                            Diffenence = dsRef.Tables[0].Rows[i]["Diffenence"].ToString(),
                            RawDayStatus = dsRef.Tables[0].Rows[i]["RawDayStatus"].ToString(),
                            EntityId = dsRef.Tables[0].Rows[i]["EntityId"].ToString(),
                            EntityName = dsRef.Tables[0].Rows[i]["EntityName"].ToString(),
                            DifferenceColor = dsRef.Tables[0].Rows[i]["DifferenceColor"].ToString(),
                            LateInTime = dsRef.Tables[0].Rows[i]["LateInTime"].ToString(),
                            LateInStatus = dsRef.Tables[0].Rows[i]["LateInStatus"].ToString(),

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
            else
            {
                clsConnectionManager objCon = null;
                string strSQL = "";
                string strSQL1 = "";
                DataList = new List<AttendanceReport>();

                System.Data.DataSet dsRef;
                try
                {
                    #region Sql
                    strSQL1 = @"select * from (  select ROW_NUMBER() OVER(ORDER BY (ISNULL(A.ToDayIN,0)-MBGT.Deployment)) AS SrNo,   SC.StandardName Section,SBC.StandardName SubSection, 
DSG.StandardName Designation, POS.Activity, 
 MBGT.Code BudgetCode, ISNULL(A.ToDayIN , 0) as ToDayIN, 
Hrg.ManpowerBudgetId, Hg.UserGroup , Hg.Id as GroupId  ,MBGT.Deployment, Diffenence= ISNULL(A.ToDayIN,0)-MBGT.Deployment ,
case when (ISNULL(A.ToDayIN,0)-MBGT.Deployment) > 0 then 'Access' 
when (ISNULL(A.ToDayIN,0)-MBGT.Deployment) < 0 then 'Short' 
else 'Ok' end DifferenceColor

from MST.ManpowerBudget MBGT
left Join EmployeeInformation EMP on MBGT.Id = EMP.BudgetCode
--LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN hkp.Designation DSG on DSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
LEFT JOIN MST.DesignationMasterLegalDesignation DMLD on DMLD.LegalDesignationId = GDSG.Id
left join mst.DesignationMaster dm on dm.Id = DMLD.DesignationMasterId
left join scs.designationmasterconfiguration dmc on dmc.designationmasterid = dm.id
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
left join mst.LegalSalaryGradeDesignation GRD on GRD.legaldesignationid = gdsg.id
left join scs.legalsalarygrade lsg on lsg.id = grd.legalsalarygradeid
left join scs.legalsalarygradehead lsh on lsh.legalsalarygradeid = lsg.id
left join mst.LegalSalaryStructure lss on lss.legalsalarygradeid = lsh.legalsalarygradeid
left join mst.LegalSalaryStructureValue lsv on lsv.legalsalarystructureid = lss.id
left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
left join SalaryRuleMaster SRM on srm.systemid = dmc.salaryrulemasterid
left join EmployeeBankInfo BNK on BNK.EmpSystemID = emp.SystemId
left join ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId
left join TransportGroup TG on TG.Id = EMP.TransportGroupId
left join employeecodetype ect on ect.id = emp.employeecodetypeid
left join hkp.Process PR on PR.Id = POS.ProcessId
left join scs.District DT on DT.Id = emp.ParmDistrictID
left join scs.[State] ST on ST.Id = EMP.ParmStateId
left join TRN.HRReportMasterChild Hrg on Hrg.ManpowerBudgetId = Emp.BudgetCode
left join TRN.HRReportMasterBudgetUserGroup HBG on HBG.HRReportMasterChildId = Hrg.Id
left join HKP.HRReportGroupMaster Hg on Hg.Id = HBG.UserGroupId
left join ResidenceAllocatedEmployees RA on RA.EmployeeSystemId = EMP.SystemId
left join ResidenceMaster RM on RM.Id = RA.ResidenceId
LEFT JOIN (select APD.BudgetId  ,Count(isnull(PT.LogDownLoadNum,0)) ToDayIN
From AttdnProcessData APD 
left join 
	(select row_number()over (partition by LogDownLoadNum order by PTime asc) Empsys,PDate ,PTime, LogDownLoadNum
	from AttdnRawData
	where PType = 'IN' and PTime is not null and PDate = '" + date + @"') PT on PT.LogDownLoadNum = APD.EmpSystemID and PT.Empsys = 1
where APD.WorkDate = '" + date + @"'  
Group by APD.BudgetId ) A ON A.BudgetId=MBGT.Id 
where emp.employeecode is not null and emp.employeestatus = 'Active' and MBGT.code is not null and MBGT.Active = 1
and emp.employeecode NOT IN (2222229, 2222230)   and MBGT.Active = 1  and Hg.Id = '" + groupid + "'";

                    if (inmis == "Bugcode")
                    {
                        strSQL = strSQL1;
                    }

                    if (entityid != null && shiftid == null && locations == null)
                    {
                        strSQL = strSQL + "  and MBGT.EntityId =  '" + entityid + "'";
                    }
                    if (entityid == null && shiftid != null && locations == null)
                    {
                        strSQL = strSQL + "  and sd.SystemID =  '" + shiftid + "'";
                    }
                    if (entityid == null && shiftid == null && locations != null)
                    {
                        strSQL = strSQL + " and  RM.Location = '" + locations + "'";
                    }
                    if (entityid != null && shiftid != null && locations == null)
                    {
                        strSQL = strSQL + "  and MBGT.EntityId =  '" + entityid + "'" + "  and sd.SystemID =  '" + shiftid + "'";
                    }
                    if (entityid == null && shiftid != null && locations != null)
                    {
                        strSQL = strSQL + "  and sd.SystemID =  '" + shiftid + "'" + " and  RM.Location = '" + locations + "'";
                    }
                    if (entityid != null && shiftid == null && locations != null)
                    {
                        strSQL = strSQL + " and  RM.Location = '" + locations + "'" + "  and MBGT.EntityId =  '" + entityid + "'";
                    }
                    if (entityid != null && shiftid != null && locations != null)
                    {
                        strSQL = strSQL + " and  RM.Location = '" + locations + "'" + "  and MBGT.EntityId =  '" + entityid + "'" + "  and sd.SystemID =  '" + shiftid + "'";
                    }

                    if (tbs != null && longabsent == null)
                    {
                        strSQL = strSQL + " and Emp.EmployeeCurrentStatus = 'TBS' ) a order by a.SrNo";
                    }

                    if (tbs == null && longabsent != null)
                    {
                        strSQL = strSQL + " and Emp.EmployeeCurrentStatus = 'LONG ABSENTEEISM' ) a order by a.SrNo";
                    }
                    if (tbs == null && longabsent == null)
                    {
                        strSQL = strSQL + " and Emp.EmployeeCurrentStatus is null ) a order by a.SrNo";
                    }

                    #endregion Sql
                    objCon = new clsConnectionManager();
                    objCon.BeginTransaction();
                    objCon.getDataSet(strSQL, out dsRef);
                    objCon.CommitTransaction();
                    for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                    {
                        DataList.Add(new AttendanceReport
                        {

                            Section = dsRef.Tables[0].Rows[i]["Section"].ToString(),
                            SubSection = dsRef.Tables[0].Rows[i]["SubSection"].ToString(),
                            Designation = dsRef.Tables[0].Rows[i]["Designation"].ToString(),
                            Activity = dsRef.Tables[0].Rows[i]["Activity"].ToString(),
                            BudgetCode = dsRef.Tables[0].Rows[i]["BudgetCode"].ToString(),
                            ManpowerBudgetId = dsRef.Tables[0].Rows[i]["ManpowerBudgetId"].ToString(),
                            UserGroup = dsRef.Tables[0].Rows[i]["UserGroup"].ToString(),
                            GroupId = dsRef.Tables[0].Rows[i]["GroupId"].ToString(),
                            Deployment = dsRef.Tables[0].Rows[i]["Deployment"].ToString(),
                            ToDayIN = dsRef.Tables[0].Rows[i]["ToDayIN"].ToString(),
                            Diffenence = dsRef.Tables[0].Rows[i]["Diffenence"].ToString(),
                            DifferenceColor = dsRef.Tables[0].Rows[i]["DifferenceColor"].ToString(),

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
        }
        #endregion Attendance



        public class ProcessServiceTest
        {
            public string Id { get; set; }
            public string ProductionDate { get; set; }
            public string EntityID { get; set; }
            public string ProcessID { get; set; }
            public string ShiftID { get; set; }
            public string ResponsiblePerson { get; set; }
            public string AddedBy { get; set; }
            public DateTime AddedDate { get; set; }
            public string AddedFromIP { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }
        }
        #endregion Test For production service

        #region Sales Return
        public void GetSalesNumber(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id as Value, InvoiceNo as Name from TRN.Sales where SourceType = 'Packing'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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


        #endregion Sales Return


        #region seven days attendance
        public string PostPlantinoutcontrl(IEnumerable<Plantcontrol> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.PlantInOutControl";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<Plantcontrol> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.PlantInOutControl where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (Plantcontrol item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);




                        dr["Id"] = _Id;
                        dr["Date"] = item.Date;
                        dr["Time"] = item.Time;
                        dr["EmployeeCode"] = item.EmployeeCode;
                        dr["InandOut"] = item.InandOut;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();

                        dr["UpdatedBy"] = DBNull.Value;
                        dr["UpdatedDate"] = DBNull.Value;
                        dr["UpdatedFromIP"] = DBNull.Value;


                        dsMaster.Tables[0].Rows.Add(dr);

                    }
                    /* else
                     {
                         DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                         dr.BeginEdit();

                         dr["Date"] = item.Date;
                         dr["Time"] = item.Time;
                         dr["EmployeeCode"] = item.EmployeeCode;
                         dr["InandOut"] = item.InandOut;


                         dr["UpdatedBy"] = item.UpdatedBy;
                         dr["UpdatedDate"] = item.UpdatedDate;
                         dr["UpdatedFromIP"] = item.UpdatedFromIP;


                         dr.EndEdit();
                     }*/

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


        public void GetLastOut(out List<Plantcontrol> DataList, string EmpSysId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Plantcontrol>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Top 1 Id , EmployeeCode , Date, Time , InandOut, AddedBy   from PlantInOutControl where AddedDate > DATEADD(HOUR , -12 , GETDATE()) 
and EmployeeCode = '" + EmpSysId + "' order by AddedDate Desc ";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Plantcontrol
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        EmployeeCode = dsRef.Tables[0].Rows[i]["EmployeeCode"].ToString(),
                        Date = dsRef.Tables[0].Rows[i]["Date"].ToString(),
                        Time = dsRef.Tables[0].Rows[i]["Time"].ToString(),
                        InandOut = dsRef.Tables[0].Rows[i]["InandOut"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
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



        #endregion seven days attendance


        #region Budget Code Change

        public string PostChangeBudgetCode(IEnumerable<TempBudgetCode> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.TempBudgetCodeChange";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<TempBudgetCode> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.TempBudgetCodeChange where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (TempBudgetCode item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);



                        dr["Id"] = _Id;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["ExistingBudgetId"] = item.ExistingBudgetId;
                        dr["NewBudgetId"] = item.NewBudgetId;
                        dr["WorkDate"] = item.WorkDate;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

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


        public void GetNewBudgetCode(out List<TempBudgetCode> DataList, string EmpsysId, string WorkDate)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<TempBudgetCode>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select * from dbo.TempBudgetCodeChange where EmpSystemId='" + EmpsysId + "' and WorkDate = '" + WorkDate + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new TempBudgetCode
                    {
                        EmpSystemId = dsRef.Tables[0].Rows[i]["EmpSystemId"].ToString(),
                        ExistingBudgetId = dsRef.Tables[0].Rows[i]["ExistingBudgetId"].ToString(),
                        NewBudgetId = dsRef.Tables[0].Rows[i]["NewBudgetId"].ToString(),
                        WorkDate = dsRef.Tables[0].Rows[i]["WorkDate"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),

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





        public string PostUpdateBudgetCodeChange(IEnumerable<TempBudgetCode> DataToSave, string EmpsysId, string WorkDate)
        {
            try
            {
                DataSet dsMaster;
                //  string TableName = "dbo.AttdnProcessData";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<TempBudgetCode> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.TempBudgetCodeChange where EmpSystemId='" + EmpsysId + "' and WorkDate = '" + WorkDate + "'", out dsMaster, false, "1");

                foreach (TempBudgetCode item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["ExistingBudgetId"] = item.ExistingBudgetId;
                        dr["NewBudgetId"] = item.NewBudgetId;
                        dr["WorkDate"] = item.WorkDate;
                        dr["Remarks"] = item.Remarks;


                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();


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
                throw (ex);
            }
        }
        #endregion Budget Code Change
        // location
        public void GetCartoonLocation(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct mm.ToLocation as Name,mm.ToStorageLocId as Value
from mst.MaterialMovementMaster mm
left join hkp.MaterialMovementPurpose MP ON MP.Id = mm.PurposeId
left join ORG.Entity ent on ent.Id = mm.EntityId
where isnull(MP.IsInventoryOut,0) = 0 and mm.ToStorageLocId is not null";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        // For Barcode Data 
        public string PostBarcodeScanData(IEnumerable<BarcodeScan> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.BarcodeScanData";
                string PackedBy = "''";
                string RefNo = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<BarcodeScan> items = DataToSave.ToList();

                foreach (BarcodeScan item in DataToSave)
                {
                    PackedBy += ",'" + item.PackedBy + "'";
                    RefNo += ",'" + item.RefNo + "'";
                }

                con.OpenDataSetThroughAdapter("select * from dbo.BarcodeScanData where Id='" + items[0].Id + "'", out dsMaster, false, "1");


                foreach (BarcodeScan item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"RefNo='" + item.RefNo + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);

                        dr["Id"] = _Id;
                        dr["LocMasterId"] = item.LocMasterId;
                        dr["SubLocation"] = item.SubLocation;
                        dr["ProductCode"] = item.ProductCode;
                        dr["POId"] = item.POId;
                        dr["LotNo"] = item.LotNo;
                        dr["RefNo"] = item.RefNo;
                        dr["Cones"] = item.Cones;
                        dr["NetWeight"] = item.NetWeight;
                        dr["GWeight"] = item.GWeight;
                        dr["PackedBy"] = item.PackedBy;
                        dr["Shade"] = item.Shade;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

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

        #region Vehicle Requisition
        public string PostVehicleRequisition(IEnumerable<Vehicle> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.VehicleMovementRequisition";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<Vehicle> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from TRN.VehicleMovementRequisition where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (Vehicle item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);



                        dr["Id"] = "22" + _Id;
                        dr["FromDate"] = item.FromDate;
                        dr["ToDate"] = item.ToDate;
                        dr["FromTime"] = item.FromTime;
                        dr["ToTime"] = item.ToTime;
                        dr["PersonalOfficial"] = item.PersonalOfficial;
                        dr["PurposeId"] = item.PurposeId;
                        dr["Name"] = item.Name;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["NumberOfPassengers"] = item.NumberOfPassengers;
                        dr["VehiclePurposeResponsiblePersonId"] = item.VehiclePurposeResponsiblePersonId;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

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

        public string PostVehicleRequisitionChild(IEnumerable<VehicleChild> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.VehicleMovementRequisitionChild";
                string Id = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<VehicleChild> items = DataToSave.ToList();

                foreach (VehicleChild item in DataToSave)
                {
                    Id += ",'" + item.Id + "'";
                }

                con.OpenDataSetThroughAdapter("select * from TRN.VehicleMovementRequisitionChild where Id='" + items[0].Id + "'", out dsMaster, false, "1");


                foreach (VehicleChild item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.Id + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);

                        dr["Id"] = "22" + _Id;
                        dr["VehicleMovementRequisitionId"] = item.VehicleMovementRequisitionId;
                        dr["FromLocationId"] = item.FromLocationId;
                        dr["ToLocationId"] = item.ToLocationId;
                        dr["WithoutPassenger"] = item.WithoutPassenger;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();



                        dsMaster.Tables[0].Rows.Add(dr);

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


        public string PostUpdateVehicleRequisition(IEnumerable<Vehicle> DataToSave, string VehicleId)
        {
            try
            {
                DataSet dsMaster;
                //  string TableName = "dbo.AttdnProcessData";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<Vehicle> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from TRN.VehicleMovementRequisition where Id='" + VehicleId + "'", out dsMaster, false, "1");

                foreach (Vehicle item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["FromDate"] = item.FromDate;
                        dr["ToDate"] = item.ToDate;
                        dr["FromTime"] = item.FromTime;
                        dr["ToTime"] = item.ToTime;
                        dr["PersonalOfficial"] = item.PersonalOfficial;
                        dr["PurposeId"] = item.PurposeId;
                        dr["Name"] = item.Name;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["NumberOfPassengers"] = item.NumberOfPassengers;
                        dr["VehiclePurposeResponsiblePersonId"] = item.VehiclePurposeResponsiblePersonId;
                        dr["Remarks"] = item.Remarks;


                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();


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
                throw (ex);
            }
        }

        public string PostCancelVehicleRequisition(IEnumerable<Vehicle> DataToSave, string VehicleId)
        {
            try
            {
                DataSet dsMaster;
                //  string TableName = "dbo.AttdnProcessData";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<Vehicle> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from TRN.VehicleMovementRequisition where Id='" + VehicleId + "'", out dsMaster, false, "1");

                foreach (Vehicle item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["FromDate"] = item.FromDate;
                        dr["ToDate"] = item.ToDate;
                        dr["FromTime"] = item.FromTime;
                        dr["ToTime"] = item.ToTime;
                        dr["PersonalOfficial"] = item.PersonalOfficial;
                        dr["PurposeId"] = item.PurposeId;
                        dr["Name"] = item.Name;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["NumberOfPassengers"] = item.NumberOfPassengers;
                        dr["Remarks"] = item.Remarks;
                        dr["isCancel"] = true;


                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();


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
                throw (ex);
            }
        }

        #region commit
        /* public string PostVehicleRequisitionChild(IEnumerable<VehicleChild> DataToSave , string MasterId)
         {
             try
             {
                 DataSet dsMaster;
                 string TableName = "TRN.VehicleMovementRequisitionChild";
                 ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                 if (DataToSave.Count() == 0)
                     return "";
                 List<VehicleChild> items = DataToSave.ToList();

                 con.OpenDataSetThroughAdapter("select * from TRN.VehicleMovementRequisitionChild where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                 int ccount = 0;

                 foreach (VehicleChild item in DataToSave)
                 {
                     DataView dv = new DataView(dsMaster.Tables[0]);
                     dv.RowFilter = "Id='" + item.Id + "'";

                     if(dv.Count == 0)
                     {
                         ccount++;
                         item.Id = MakePK(MasterId, ccount , 2);
                         AddNewRowD(dsMaster.Tables[0], item);
                     }



                    *//* dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.Id + "' ";
                     if (dsMaster.Tables[0].Rows.Count == 0)
                     {
                         DataRow dr = dsMaster.Tables[0].NewRow();

                         bplib.clsGenID genid = new bplib.clsGenID();
                         genid.GenID(TableName, out string _Id);



                         dr["Id"] = _Id;
                         dr["VehicleMovementRequisitionId"] = item.VehicleMovementRequisitionId;
                         dr["FromLocationId"] = item.FromLocationId;
                         dr["ToLocationId"] = item.ToLocationId;
                         dr["WithoutPassenger"] = item.WithoutPassenger;
                         dr["Remarks"] = item.Remarks;

                         dr["AddedBy"] = item.AddedBy;
                         dr["AddedFromIP"] = item.AddedFromIP;
                         dr["AddedDate"] = System.DateTime.Now.ToString();


                         dsMaster.Tables[0].Rows.Add(dr);
                         AddNewRowD(dsMaster.Tables[0] , dr);


                     }*//*


                 }
                 clsStaticInfo _info = new clsStaticInfo();
                 _info.SaveDataSets(dsMaster);
                 string MasterIds = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                 return MasterIds;

             }
             catch (Exception ex)
             {
                 return ex.ToString();
             }

         }*/

        /* private void AddNewRowD(DataTable dt, Dictionary<string, object> sourceData)
         {
             var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
             DataRow dr = dt.NewRow(); foreach (var item in sourceData.Keys)
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
             dr["UpdatedFromIP"] = identity.IPAddress; dt.Rows.Add(dr);
         }

        private string MakePK(string masterId, int currentId, int padLeft)
         {
             return masterId + currentId.ToString().PadLeft(padLeft, '0');
         }*/


        #endregion commit
        public void GetVehicleLocation(out List<Default2> DataList, string ID)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id as Value , StandardName as Name from HKP.LocationMaster where Id <> '" + ID + "' order by StandardName";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetPurpose(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id as Value, StandardName as Name from HKP.PurposeMaster where Active = 1";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetPurposeResponsible(out List<Default3> DataList, string PurposeId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default3>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select PR.Id Value ,PR.ResponsiblePersonId SystemId,  EI.EmployeeName Name from TRN.VehiclePurposeResponsiblePerson PR
left join EmployeeInformation EI on EI.SystemId = PR.ResponsiblePersonId where VehiclePurposeId = '" + PurposeId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default3
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),
                        SystemId = dsRef.Tables[0].Rows[i]["SystemId"].ToString(),

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


        public void GetVehicleCreations(out List<VehicleCreation> DataList, string EmpsysId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<VehicleCreation>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"Select VMR.Id,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial
                     ,VMR.Name, VMR.PurposeId,PM.UserName Purpose, VMR.Remarks,EI.EmployeeName, EI.SystemId ResponsiblePersonCode, VMR.NumberOfPassengers , EIM.EmployeeName SelectedApprovePerson, VMR.VehiclePurposeResponsiblePersonId PurposeResponsibleId
                    from[TRN].[VehicleMovementRequisition] VMR
                    left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                    left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
					left join EmployeeInformation EIM on EIM.SystemId = VMR.VehiclePurposeResponsiblePersonId
					where VMR.AppliedId is null  and VMR.IsReject is null and VMR.isCancel is null and VMR.IsApprove is null and VMR.AddedBy = '" + EmpsysId + "' order by VMR.FromDate asc";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new VehicleCreation
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        FromDate = dsRef.Tables[0].Rows[i]["FromDate"].ToString(),
                        ToDate = dsRef.Tables[0].Rows[i]["ToDate"].ToString(),
                        FromTime = dsRef.Tables[0].Rows[i]["FromTime"].ToString(),
                        ToTime = dsRef.Tables[0].Rows[i]["ToTime"].ToString(),
                        PersonalOfficial = dsRef.Tables[0].Rows[i]["PersonalOfficial"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),
                        PurposeId = dsRef.Tables[0].Rows[i]["PurposeId"].ToString(),
                        Purpose = dsRef.Tables[0].Rows[i]["Purpose"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        EmployeeName = dsRef.Tables[0].Rows[i]["EmployeeName"].ToString(),
                        ResponsiblePersonCode = dsRef.Tables[0].Rows[i]["ResponsiblePersonCode"].ToString(),
                        NumberOfPassengers = dsRef.Tables[0].Rows[i]["NumberOfPassengers"].ToString(),
                        SelectedApprovePerson = dsRef.Tables[0].Rows[i]["SelectedApprovePerson"].ToString(),
                        PurposeResponsibleId = dsRef.Tables[0].Rows[i]["PurposeResponsibleId"].ToString(),

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



        public void GetVehiclestatus(out List<VehicleStatus> DataList, string EmpsysId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<VehicleStatus>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"Select VMR.Id,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial
                     ,VMR.Name, VMR.PurposeId,PM.UserName Purpose, VMR.Remarks,EI.EmployeeName, EI.EmployeeCode ResponsiblePersonCode, VMR.NumberOfPassengers
                    ,RequisitionStatus = case when VMR.IsApprove = 1 then 'Approved' 
                    when VMR.IsReject = 1 then 'Reject'
                    end, ApprovedBy = case when VMR.IsApprove = 1 then EIM.EmployeeName end
                    , RejectBy = case when VMR.IsReject = 1 then EIM.EmployeeName end
                    from[TRN].[VehicleMovementRequisition] VMR
                    left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                    left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
                    left join TRN.VehicleTrip VT on VT.Id = VMR.AppliedId 
					left join EmployeeInformation EIM on EIM.SystemId = VMR.VehiclePurposeResponsiblePersonId
					where (VMR.IsApprove = 1  or VMR.IsReject = 1 ) and VMR.isCancel is null and VMR.AddedBy = '" + EmpsysId + "'  order by FromDate Desc";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new VehicleStatus
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        FromDate = dsRef.Tables[0].Rows[i]["FromDate"].ToString(),
                        ToDate = dsRef.Tables[0].Rows[i]["ToDate"].ToString(),
                        FromTime = dsRef.Tables[0].Rows[i]["FromTime"].ToString(),
                        ToTime = dsRef.Tables[0].Rows[i]["ToTime"].ToString(),
                        PersonalOfficial = dsRef.Tables[0].Rows[i]["PersonalOfficial"].ToString(),
                        PurposeId = dsRef.Tables[0].Rows[i]["PurposeId"].ToString(),
                        Purpose = dsRef.Tables[0].Rows[i]["Purpose"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        EmployeeName = dsRef.Tables[0].Rows[i]["EmployeeName"].ToString(),
                        ResponsiblePersonCode = dsRef.Tables[0].Rows[i]["ResponsiblePersonCode"].ToString(),
                        NumberOfPassengers = dsRef.Tables[0].Rows[i]["NumberOfPassengers"].ToString(),
                        RequisitionStatus = dsRef.Tables[0].Rows[i]["RequisitionStatus"].ToString(),
                        ApprovedBy = dsRef.Tables[0].Rows[i]["ApprovedBy"].ToString(),
                        RejectBy = dsRef.Tables[0].Rows[i]["RejectBy"].ToString(),

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



        public void GetVehicleOutlist(out List<VehicleOutin> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<VehicleOutin>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select  Distinct VMR.Id as MasterId, FromLocation = stuff((select ',  ' + LM.UserName from TRN.VehicleMovementRequisitionChild VMC                            
left join HKP.LocationMaster LM on LM.Id = VMC.FromLocationId
where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,'') ,ToLocation =  stuff((select ',  ' + TM.UserName 
from TRN.VehicleMovementRequisitionChild VMC
left join HKP.LocationMaster TM on TM.Id = VMC.ToLocationId
where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,''), EM.EmployeeName as RequisitionBy ,PM.StandardName as Purpose , DP.UserName as Department, VT.Id, VT.Id TripNumber ,VA.TripId ,VT.Id AppliedId ,FORMAT(VT.FromDate, 'dd-MMM-yyyy')FromDate, FORMAT(VT.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(VT.FromTime, 'hh:mm tt')FromTime
, FORMAT(VT.ToTime, 'hh:mm tt')ToTime, VA.DriverMasterId ,EI.EmployeeName DriverName, VA.VehicleMasterId, VM.VehicleNumber , VIO.Id as VIOId ,  VA.Id VehicleAllocationId
from TRN.VehicleTrip VT
left join TRN.VehicleAllocation VA on VA.TripId = VT.Id
left join HKP.VehicleMaster VM on VM.Id = VA.VehicleMasterId
left join HKP.DriverMaster DM on DM.Id = VA.DriverMasterId
left join EmployeeInformation EI on EI.SystemId = DM.DriverId
left join TRN.VehicleMovementInOut VIO on VIO.VehicleAllocationId = VA.Id
left join TRN.VehicleMovementRequisition VMR on VMR.AppliedId = VT.Id
left join TRN.VehicleMovementRequisitionChild VRC on VRC.VehicleMovementRequisitionId = VMR.Id
left join HKP.LocationMaster LM on LM.Id = vrc.FromLocationId
left join HKP.LocationMaster LMN on LMN.Id = vrc.ToLocationId
left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId 
left join EmployeeInformation Em on EM.SystemId = VMR.AddedBy
left join ORG.Department DP on DP.Id = Em.DepartmentId 
where VIO.OutReading is null and VA.Id is not null and VIO.Id is null and VT.FromDate >= Cast(GETDATE() as date) order by FromDate Asc";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new VehicleOutin
                    {
                        MasterId = dsRef.Tables[0].Rows[i]["MasterId"].ToString(),
                        FromLocation = dsRef.Tables[0].Rows[i]["FromLocation"].ToString(),
                        ToLocation = dsRef.Tables[0].Rows[i]["ToLocation"].ToString(),
                        RequisitionBy = dsRef.Tables[0].Rows[i]["RequisitionBy"].ToString(),
                        Purpose = dsRef.Tables[0].Rows[i]["Purpose"].ToString(),
                        Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        FromDate = dsRef.Tables[0].Rows[i]["FromDate"].ToString(),
                        ToDate = dsRef.Tables[0].Rows[i]["ToDate"].ToString(),
                        FromTime = dsRef.Tables[0].Rows[i]["FromTime"].ToString(),
                        ToTime = dsRef.Tables[0].Rows[i]["ToTime"].ToString(),
                        TripNumber = dsRef.Tables[0].Rows[i]["TripNumber"].ToString(),
                        TripId = dsRef.Tables[0].Rows[i]["TripId"].ToString(),
                        AppliedId = dsRef.Tables[0].Rows[i]["AppliedId"].ToString(),
                        DriverMasterId = dsRef.Tables[0].Rows[i]["DriverMasterId"].ToString(),
                        DriverName = dsRef.Tables[0].Rows[i]["DriverName"].ToString(),
                        VehicleMasterId = dsRef.Tables[0].Rows[i]["VehicleMasterId"].ToString(),
                        VehicleNumber = dsRef.Tables[0].Rows[i]["VehicleNumber"].ToString(),
                        VIOId = dsRef.Tables[0].Rows[i]["VIOId"].ToString(),
                        VehicleAllocationId = dsRef.Tables[0].Rows[i]["VehicleAllocationId"].ToString(),

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


        public void GetVehiclInlist(out List<VehicleOutin> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<VehicleOutin>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Distinct VMR.Id as MasterId, FromLocation = stuff((select ',  ' + LM.UserName from TRN.VehicleMovementRequisitionChild VMC                            
left join HKP.LocationMaster LM on LM.Id = VMC.FromLocationId
where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,'') ,ToLocation =  stuff((select ',  ' + TM.UserName 
from TRN.VehicleMovementRequisitionChild VMC
left join HKP.LocationMaster TM on TM.Id = VMC.ToLocationId
where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,''), EM.EmployeeName as RequisitionBy ,PM.StandardName as Purpose , DP.UserName as Department, VT.Id, VT.Id TripNumber ,VA.TripId ,VT.Id AppliedId ,FORMAT(VT.FromDate, 'dd-MMM-yyyy')FromDate, FORMAT(VT.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(VT.FromTime, 'hh:mm tt')FromTime
, FORMAT(VT.ToTime, 'hh:mm tt')ToTime, VA.DriverMasterId ,EI.EmployeeName DriverName, VA.VehicleMasterId, VM.VehicleNumber , VIO.Id as VIOId ,  VA.Id VehicleAllocationId
from TRN.VehicleTrip VT
left join TRN.VehicleAllocation VA on VA.TripId = VT.Id
left join HKP.VehicleMaster VM on VM.Id = VA.VehicleMasterId
left join HKP.DriverMaster DM on DM.Id = VA.DriverMasterId
left join EmployeeInformation EI on EI.SystemId = DM.DriverId
left join TRN.VehicleMovementInOut VIO on VIO.VehicleAllocationId = VA.Id
left join TRN.VehicleMovementRequisition VMR on VMR.AppliedId = VT.Id
left join TRN.VehicleMovementRequisitionChild VRC on VRC.VehicleMovementRequisitionId = VMR.Id
left join HKP.LocationMaster LM on LM.Id = vrc.FromLocationId
left join HKP.LocationMaster LMN on LMN.Id = vrc.ToLocationId
left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId 
left join EmployeeInformation Em on EM.SystemId = VMR.AddedBy
left join ORG.Department DP on DP.Id = Em.DepartmentId 
where VIO.OutReading is not null and VIO.InReading is null and VA.Id is not null and VIO.Id is not null order by FromDate Desc";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new VehicleOutin
                    {
                        MasterId = dsRef.Tables[0].Rows[i]["MasterId"].ToString(),
                        RequisitionBy = dsRef.Tables[0].Rows[i]["RequisitionBy"].ToString(),
                        Purpose = dsRef.Tables[0].Rows[i]["Purpose"].ToString(),
                        Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        FromDate = dsRef.Tables[0].Rows[i]["FromDate"].ToString(),
                        ToDate = dsRef.Tables[0].Rows[i]["ToDate"].ToString(),
                        FromTime = dsRef.Tables[0].Rows[i]["FromTime"].ToString(),
                        ToTime = dsRef.Tables[0].Rows[i]["ToTime"].ToString(),
                        TripNumber = dsRef.Tables[0].Rows[i]["TripNumber"].ToString(),
                        TripId = dsRef.Tables[0].Rows[i]["TripId"].ToString(),
                        AppliedId = dsRef.Tables[0].Rows[i]["AppliedId"].ToString(),
                        DriverMasterId = dsRef.Tables[0].Rows[i]["DriverMasterId"].ToString(),
                        DriverName = dsRef.Tables[0].Rows[i]["DriverName"].ToString(),
                        VehicleMasterId = dsRef.Tables[0].Rows[i]["VehicleMasterId"].ToString(),
                        VehicleNumber = dsRef.Tables[0].Rows[i]["VehicleNumber"].ToString(),
                        VIOId = dsRef.Tables[0].Rows[i]["VIOId"].ToString(),
                        VehicleAllocationId = dsRef.Tables[0].Rows[i]["VehicleAllocationId"].ToString(),

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


        public void GetVehicleCreationDetail(out List<Vehiclecreationdetails> DataList, string MasterId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Vehiclecreationdetails>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select LM.StandardName as FromLocation , LMN.StandardName as ToLocation, EM.EmployeeName as RequisitionBy ,PM.StandardName as Purpose , DP.UserName as Department  from TRN.VehicleMovementRequisitionChild vrc
left join TRN.VehicleMovementRequisition vr on vr.Id = vrc.VehicleMovementRequisitionId
left join HKP.LocationMaster LM on LM.Id = vrc.FromLocationId
left join HKP.LocationMaster LMN on LMN.Id = vrc.ToLocationId
left join HKP.PurposeMaster PM on PM.Id = vr.PurposeId 
left join EmployeeInformation Em on EM.SystemId = vr.AddedBy
left join ORG.Department DP on DP.Id = Em.DepartmentId  
where  vr.AppliedId = '" + MasterId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Vehiclecreationdetails
                    {
                        FromLocation = dsRef.Tables[0].Rows[i]["FromLocation"].ToString(),
                        ToLocation = dsRef.Tables[0].Rows[i]["ToLocation"].ToString(),
                        RequisitionBy = dsRef.Tables[0].Rows[i]["RequisitionBy"].ToString(),
                        Purpose = dsRef.Tables[0].Rows[i]["Purpose"].ToString(),
                        Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),

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

        public string PostVehicleInOutEntry(IEnumerable<VehicleInout> DataToSave, string VInOutId)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.VehicleMovementInOut";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<VehicleInout> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from TRN.VehicleMovementInOut where Id='" + VInOutId + "'", out dsMaster, false, "1");

                foreach (VehicleInout item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);



                        dr["Id"] = "22" + _Id;
                        dr["VehicleAllocationId"] = item.VehicleAllocationId;
                        dr["InDate"] = DBNull.Value;
                        dr["OutDate"] = item.OutDate;
                        dr["InTime"] = DBNull.Value;
                        dr["OutTime"] = item.OutTime;
                        dr["InReading"] = DBNull.Value;
                        dr["OutReading"] = item.OutReading;
                        dr["InRemarks"] = DBNull.Value;
                        dr["OutRemarks"] = item.OutRemarks;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = "::1";
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

                    }

                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["VehicleAllocationId"] = item.VehicleAllocationId;
                        dr["InDate"] = item.InDate;
                        dr["InTime"] = item.InTime;
                        dr["InReading"] = item.InReading;
                        dr["InRemarks"] = item.InRemarks;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedFromIP"] = "::1";
                        dr["UpdatedDate"] = DateTime.Now.ToString();

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

        public void GetVehicleApprove(out List<VehicleOutin> DataList, string EmpSystemId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<VehicleOutin>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Distinct VMR.Id as MasterId, FromLocation = stuff((select ',  ' + LM.UserName from TRN.VehicleMovementRequisitionChild VMC                            
left join HKP.LocationMaster LM on LM.Id = VMC.FromLocationId
where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,'') ,ToLocation =  stuff((select ',  ' + TM.UserName 
from TRN.VehicleMovementRequisitionChild VMC
left join HKP.LocationMaster TM on TM.Id = VMC.ToLocationId
where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,'') ,Concat(FORMAT(VMR.FromDate, 'dd-MMM-yyyy'),' ' ,FORMAT(VMR.FromTime, 'hh:mm tt')) as FromDate ,concat(FORMAT(VMR.ToDate, 'dd-MMM-yyyy'),' ',
FORMAT(VMR.ToTime, 'hh:mm tt'))ToDate ,VMR.FromTime
,VMR.ToTime , EM.EmployeeName as RequisitionBy ,PM.StandardName as Purpose , DP.UserName as Department
from TRN.VehicleMovementRequisition VMR
left join TRN.VehicleMovementRequisitionChild VRC on VRC.VehicleMovementRequisitionId = VMR.Id
left join EmployeeInformation Em on EM.SystemId = VMR.AddedBy
left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId 
left join ORG.Department DP on DP.Id = Em.DepartmentId 
where VMR.AppliedId is null and VMR.IsReject is null and VMR.isCancel is null and VRC.VehicleMovementRequisitionId = VMR.Id and VMR.IsApprove is null
and VMR.VehiclePurposeResponsiblePersonId = '" + EmpSystemId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new VehicleOutin
                    {
                        MasterId = dsRef.Tables[0].Rows[i]["MasterId"].ToString(),
                        RequisitionBy = dsRef.Tables[0].Rows[i]["RequisitionBy"].ToString(),
                        FromLocation = dsRef.Tables[0].Rows[i]["FromLocation"].ToString(),
                        ToLocation = dsRef.Tables[0].Rows[i]["ToLocation"].ToString(),
                        Purpose = dsRef.Tables[0].Rows[i]["Purpose"].ToString(),
                        Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),
                        FromDate = dsRef.Tables[0].Rows[i]["FromDate"].ToString(),
                        ToDate = dsRef.Tables[0].Rows[i]["ToDate"].ToString(),
                        FromTime = dsRef.Tables[0].Rows[i]["FromTime"].ToString(),
                        ToTime = dsRef.Tables[0].Rows[i]["ToTime"].ToString(),

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


        public string PostVehicleTrip(IEnumerable<VehicleApproveList> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.VehicleTrip";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<VehicleApproveList> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from TRN.VehicleTrip where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (VehicleApproveList item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);



                        dr["Id"] = "22" + _Id;
                        dr["FromDate"] = item.FromDate;
                        dr["ToDate"] = item.ToDate;
                        dr["FromTime"] = item.FromTime;
                        dr["ToTime"] = item.ToTime;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

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

        public string PostUpdateVehicleApprove(IEnumerable<Vehicle> DataToSave, string VehicleId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<Vehicle> items = DataToSave.ToList();


                con.OpenDataSetThroughAdapter("select * from TRN.VehicleMovementRequisition where Id='" + VehicleId + "'", out dsMaster, false, "1");

                foreach (Vehicle item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        // DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["isApprove"] = 1;

                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();


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
                throw (ex);
            }
        }

        public string PostUpdateVehicleReject(IEnumerable<Vehicle> DataToSave, string VehicleId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<Vehicle> items = DataToSave.ToList();


                con.OpenDataSetThroughAdapter("select * from TRN.VehicleMovementRequisition where Id='" + VehicleId + "'", out dsMaster, false, "1");

                foreach (Vehicle item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        // DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["IsReject"] = 1;

                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();


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
                throw (ex);
            }
        }

        public string PostCombineVehicleApprove(IEnumerable<Vehicle> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string ErrorList = "";

                if (DataToSave.Count() == 0)
                {
                    return "No Data Found";
                }

                string IDS = "''";
                foreach (Vehicle item in DataToSave)
                {
                    IDS += ",'" + item.Id + "'";
                }

                var items = DataToSave.ToList();

                var sqlx = @"select * from TRN.VehicleMovementRequisition where Id(" + IDS + @")";
                con.OpenDataSetThroughAdapter(sqlx, out dsMaster, false, "1");

                foreach (Vehicle item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.Id + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                    {

                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["AppliedId"] = item.AppliedId;

                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr.EndEdit();

                    }
                    else
                    {
                        ErrorList += item.Id + "...";
                    }
                }

                return "true";

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion Vehicle Requisition

        #region Incident

        public void GetIncidentCategory(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id as Value , StandardName as Name from HKP.IncedentCategory";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetIncedentCategoryDetail(out List<Default2> DataList, string Id)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select  EI.SystemId as Value,EI.EmployeeName as Name from [MST].[ManpowerBudget] MB
                        left join dbo.EmployeeInformation EI On EI.BudgetCode=MB.Id
                        left join [HKP].[IncedentCategory] IC ON IC.InchargeNameBgtCodeId=MB.Id
                        where IC.Id='" + Id + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetEmployeeBudget(out List<ROCode> DataList, string Id)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<ROCode>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select  MB.Id as Value , MB.Code as Name , MB.ROBudgetCode as ROCodes from MST.ManpowerBudget MB
left join EmployeeInformation EI on EI.BudgetCode = MB.Id
where EI.SystemId = '" + Id + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new ROCode
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),
                        ROCodes = dsRef.Tables[0].Rows[i]["ROCodes"].ToString(),

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


        public void GetRoName(out List<Default2> DataList, string Id)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Top 1 EI.SystemId as Value  , EI.EmployeeName as Name from MST.ManpowerBudget MB 
left join EmployeeInformation EI on EI.BudgetCode = MB.ROBudgetCode
where EI.EmployeeStatus = 'Active' and MB.ROBudgetCode = '" + Id + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetIncidentTitle(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id as Value , UserName as Name from HKP.IncedentTitle";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public string PostIncedentCreation(IEnumerable<Incedent> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.IncedentCategoryUpdate";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<Incedent> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from TRN.IncedentCategoryUpdate where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (Incedent item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);



                        dr["Id"] = "22" + _Id;
                        dr["Date"] = item.Date;
                        dr["Time"] = item.Time;
                        dr["EmployeeId"] = item.EmployeeId;
                        dr["BudgetCode"] = item.BudgetCode;
                        dr["RONameId"] = item.RONameId;
                        dr["IncedentCategoryId"] = item.IncedentCategoryId;
                        dr["IncedentItemTitle"] = item.IncedentItemTitle;
                        dr["IncedentDetail"] = item.IncedentDetail;
                        dr["IncedentType"] = item.IncedentType;
                        dr["CriticalityLevel"] = item.CriticalityLevel;
                        dr["ActionTaken"] = item.ActionTaken;
                        dr["StoryPoints"] = item.StoryPoints;
                        dr["FollowUpApplicable"] = item.FollowUpApplicable;
                        dr["FollowUpDays"] = item.FollowUpDays;
                        dr["FollowUpById"] = item.FollowUpById;
                        dr["IssueInchargeId"] = item.IssueInchargeId;
                        dr["FinalStatus"] = item.FinalStatus;
                        dr["Remarks"] = item.Remarks;
                        dr["FileName"] = item.FileName;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

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
        #endregion Incident

        #region Attdn Lock
        public void GetCheckAttdnLock(out List<Default2> DataList, string Date)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select LockedDate Value,IsActive Name from PlantWiseAttendanceLock where LockedDate = '" + Date + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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
        #endregion Attdn Lock

        #region Quality Control
        public void GetQualityGeneraWiseIssue(out List<QualityGenaralIssue> DataList, string ResposibleId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<QualityGenaralIssue>();
            string ResponsiblePerson = string.Empty;

            if (ResposibleId != "null" && ResposibleId != "undefined")
            {
                ResponsiblePerson = " where QGIEmployeeId = '" + ResposibleId + "'";
            }
            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select  GI.* from (select  QC.Id,QID.IssueNameId,(select top 1 RepeatEntry from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc) as RepeatEntry,
case when (select top 1 RepeatEntry from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc)='Repeat' then format((select top 1 AddedDate from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc),'dd-MMM-yyyy') else
format(DATEADD(hour, QID.CheckingInterval,(select top 1 AddedDate from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc)),'dd-MMM-yyyy') end as QualityIssueDate,
case when (select top 1 RepeatEntry from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc)='Repeat' then format((select top 1 AddedDate from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc),'hh:mm tt') else
format(DATEADD(hour, QID.CheckingInterval, CAST((select top 1 AddedDate from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc) AS DATETIME)),'hh:mm tt') end as QualityIssueTime,
E.Id  EntityId,E.UserName Entity,P.Id ProcessId,P.UserName Process,QID.IssueNameId IssueId,QID.Id DefineIssueId,
QMM.UserName QGIssue,
reverse(stuff(reverse((select EI.EmployeeName + ',' from EmployeeInformation EI where EmployeeStatus='Active' and PositionID in (select PositionCodeId from MST.QualityManagementPositionCode where QMID=QID.IssueNameId) for xml path(''))),1,1,'')) as PositionEmployee,
isnull(QC.QGIEmployeeId,QID.ResponsiblePersonId) as QGIEmployeeId,isnull((select EmployeeName from EmployeeInformation where SystemId=QC.QGIEmployeeId),(select EmployeeName from EmployeeInformation where SystemId=QID.ResponsiblePersonId)) as QGIEmployee
from MST.QualityIssueDetails  QID
left join TRN.QualityIssueControl as QC on QC.DefineIssueId=QID.Id and QC.Id = (select top 1 Id from TRN.QualityIssueControl where DefineIssueId=QID.Id order by AddedDate desc) 
left join MST.QualityManagementMaster QMM on QMM.Id=QID.IssueNameId
left join org.Entity E on E.Id=QID.EntityId
left join hkp.Process P on P.Id=QID.ProcessId) GI
" + ResponsiblePerson + @" order by Convert(Date,GI.QualityIssueDate)
";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new QualityGenaralIssue
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        IssueNameId = dsRef.Tables[0].Rows[i]["IssueNameId"].ToString(),
                        RepeatEntry = dsRef.Tables[0].Rows[i]["RepeatEntry"].ToString(),
                        QualityIssueDate = dsRef.Tables[0].Rows[i]["QualityIssueDate"].ToString(),
                        QualityIssueTime = dsRef.Tables[0].Rows[i]["QualityIssueTime"].ToString(),
                        EntityId = dsRef.Tables[0].Rows[i]["EntityId"].ToString(),
                        Entity = dsRef.Tables[0].Rows[i]["Entity"].ToString(),
                        ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                        Process = dsRef.Tables[0].Rows[i]["Process"].ToString(),
                        IssueId = dsRef.Tables[0].Rows[i]["IssueId"].ToString(),
                        DefineIssueId = dsRef.Tables[0].Rows[i]["DefineIssueId"].ToString(),
                        QGIssue = dsRef.Tables[0].Rows[i]["QGIssue"].ToString(),
                        PositionEmployee = dsRef.Tables[0].Rows[i]["PositionEmployee"].ToString(),
                        QGIEmployeeId = dsRef.Tables[0].Rows[i]["QGIEmployeeId"].ToString(),
                        QGIEmployee = dsRef.Tables[0].Rows[i]["QGIEmployee"].ToString(),

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

        public void GetQualityPOWiseIssue(out List<QualityPOIssue> DataList, string POIssueDate, string ResponsibleId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<QualityPOIssue>();
            string ResponsiblePerson = string.Empty;
            if (ResponsibleId != "null" && ResponsibleId != "undefined")
            {
                ResponsiblePerson = " and QPEmployeeId = '" + ResponsibleId + "'";
            }
            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"Select Format(PO1.Date,'dd-MMM-yyyy') PODate,Format(PO1.QualityPlanDate,'dd-MMM-yyyy') QPDate,PO1.* from (Select distinct QPC.Id,PD.Id QPId,PO.Id POId,PO.EntryLevel,PO.LotNumber,PD.IssueId,QMM.UserName QPIssue,PO.ProcessId,P.UserName Process,PD.Legdays,
PD.DependentDate DependentOn,E.UserName Entity,PO.EntityId,
(select top 1 RepeatEntry from TRN.QualityControl where IssueId=QMM.Id and QualityPlanId=QPC.Id and PlanType='POIssue' and RepeatEntry is not null order by AddedDate desc) as RepeatEntry,
PD.Remarks,PO.POStatus,PO.Customer,
convert(Date,case 
when PD.DependentDate='ItemDate' then format(MOI.AddedDate,'dd-MMM-yyyy')
when PD.DependentDate='ExFactoryDate' then format((select top 1 PlanExFactoryDate from TRN.SalesOrder where Id=SO.Id order by PlanExFactoryDate desc),'dd-MMM-yyyy')
when PD.DependentDate='PODate' then PO.POCreationDate
when PD.DependentDate='POStartDate' then PO.POStartDate
when PD.DependentDate='POEndDate' then PO.POEndDate
end)Date, 
convert(Date,case 
when PD.DependentDate='ItemDate' then format(DATEADD(Day, PD.Legdays, MOI.AddedDate),'dd-MMM-yyyy')
when PD.DependentDate='ExFactoryDate' then format(DATEADD(Day, PD.Legdays, (select top 1 PlanExFactoryDate from TRN.SalesOrder where Id=SO.Id order by PlanExFactoryDate desc)),'dd-MMM-yyyy')
when PD.DependentDate='PODate' then format(DATEADD(Day, PD.Legdays, PO.POCreationDate),'dd-MMM-yyyy')
when PD.DependentDate='POStartDate' then format(DATEADD(Day, PD.Legdays, PO.POStartDate),'dd-MMM-yyyy')
when PD.DependentDate='POEndDate' then format(DATEADD(Day, PD.Legdays,PO.POEndDate),'dd-MMM-yyyy')
end) QualityPlanDate,
PO.POStartDate,PO.POEndDate,PO.POCreationDate,
isnull(QPC.QPEmployeeId,PD.ResponsiblePersonId) as QPEmployeeId,
isnull((select EmployeeName from EmployeeInformation where SystemId=QPC.QPEmployeeId),(select EmployeeName from EmployeeInformation where SystemId=PD.ResponsiblePersonId)) as QPEmployee
from (select distinct PO.Id,PS.UserName POStatus, 'PO' EntryLevel,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
reverse(stuff(reverse((select distinct LotNumber + ',' from TRN.ProductionSummary where ProductionOrderId=PO.Id and ProcessId=Prod.ProcessId for xml path(''))),1,1,'')) as LotNumber,
PO.EntityId,Prod.ProcessId,POFirstProdBookDate,POProcessFirstProdBookDate,POLatestProdBookDate,BaseProcPlanStartDate,BaseProcPlanCompletionDate, 
isnull(format(FBPPD.POFirstProdBookDate,'dd-MMM-yyyy'),format(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')) POStartDate,
isnull(format(Type1.BaseProcPlanCompletionDate,'dd-MMM-yyyy'),format(LBPPD.POLatestProdBookDate,'dd-MMM-yyyy')) POEndDate,
format(PO.AddedDate,'dd-MMM-yyyy') POCreationDate
from TRN.ProductionOrder PO
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
left join TRN.ProductionSummary Prod on Prod.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POFirstProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POProcessFirstProdBookDate,ProductionOrderId,ProcessId From TRN.ProductionSummary Group By ProductionOrderId,ProcessId) POPFBD ON POPFBD.ProductionOrderId=PO.Id and Prod.ProcessId=POPFBD.ProcessId
LEFT JOIN (Select SUM(Quantity)ProQty,MAX(ProductionDate)POLatestProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) LBPPD ON LBPPD.ProductionOrderId=PO.Id
LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanCompletionDate,ProductionOrderId From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=PO.Id
where PS.UserName in ('Running','To Close') 
union
select distinct PO.Id,PS.UserName POStatus, 'LOT' EntryLevel,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),Prod.LotNumber,PO.EntityId,
Prod.ProcessId,POFirstProdBookDate,POProcessFirstProdBookDate,POLatestProdBookDate,BaseProcPlanStartDate,BaseProcPlanCompletionDate, 
isnull(format(FBPPD.POFirstProdBookDate,'dd-MMM-yyyy'),format(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')) POStartDate,
isnull(format(Type1.BaseProcPlanCompletionDate,'dd-MMM-yyyy'),format(LBPPD.POLatestProdBookDate,'dd-MMM-yyyy')) POEndDate,
format(PO.AddedDate,'dd-MMM-yyyy') POCreationDate
from TRN.ProductionOrder PO
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
left join TRN.ProductionSummary Prod on Prod.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POFirstProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POProcessFirstProdBookDate,ProductionOrderId,ProcessId From TRN.ProductionSummary Group By ProductionOrderId,ProcessId) POPFBD ON POPFBD.ProductionOrderId=PO.Id and Prod.ProcessId=POPFBD.ProcessId
LEFT JOIN (Select SUM(Quantity)ProQty,MAX(ProductionDate)POLatestProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) LBPPD ON LBPPD.ProductionOrderId=PO.Id
LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanCompletionDate,ProductionOrderId From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=PO.Id
where PS.UserName in ('Running','To Close')) PO
left join MST.POQualityPlanDetails PD on PD.EntryLevel=PO.EntryLevel and PO.ProcessId=PD.ProcessId
left Join MST.QualityManagementMaster QMM on QMM.Id=PD.IssueId
left join [TRN].[QualityPlanControl] QPC on QPC.QPId=PD.Id and QPC.POId=PO.Id and QPC.LotNumber=PO.LotNumber and QPC.EntryLevel=PO.EntryLevel
left join TRN.ProductionOrderDetail POD on POD.ProductionOrderId=PO.Id
left join TRN.SalesOrder SO on SO.Id=POD.SalesOrderId 
left join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
left join hkp.Process P on P.Id=PO.ProcessId
left join ORG.Entity E on E.Id=PO.EntityId
where PO.ProcessId is not null and E.Id in (select EntityId from MST.QualityManagementEntity where QMID=QMM.Id) 
and QPC.QCID is null or (select top 1 RepeatEntry from TRN.QualityControl where IssueId=QMM.Id and QualityPlanId=QPC.Id and PlanType='POIssue' order by AddedDate desc) is not null
union
Select distinct QPC.Id,PD.Id QPId,PO.Id POId,PO.EntryLevel,PO.LotNumber,PD.IssueId,QMM.UserName QPIssue,PO.ProcessId,P.UserName Process,PD.Legdays,
PD.DependentDate DependentOn,E.UserName Entity,PO.EntityId,
(select top 1 RepeatEntry from TRN.QualityControl where IssueId=QMM.Id and QualityPlanId=QPC.Id and PlanType='POIssue' and RepeatEntry is not null order by AddedDate desc) as RepeatEntry,
PD.Remarks,PO.POStatus,PO.Customer,
convert(Date,case 
when PD.DependentDate='ItemDate' then format(MOI.AddedDate,'dd-MMM-yyyy')
when PD.DependentDate='ExFactoryDate' then format((select top 1 PlanExFactoryDate from TRN.SalesOrder where Id=SO.Id order by PlanExFactoryDate desc),'dd-MMM-yyyy')
when PD.DependentDate='PODate' then PO.POCreationDate
when PD.DependentDate='POStartDate' then PO.POStartDate
when PD.DependentDate='POEndDate' then PO.POEndDate
end)Date, 
convert(Date,case 
when PD.DependentDate='ItemDate' then format(DATEADD(Day, PD.Legdays, MOI.AddedDate),'dd-MMM-yyyy')
when PD.DependentDate='ExFactoryDate' then format(DATEADD(Day, PD.Legdays, (select top 1 PlanExFactoryDate from TRN.SalesOrder where Id=SO.Id order by PlanExFactoryDate desc)),'dd-MMM-yyyy')
when PD.DependentDate='PODate' then format(DATEADD(Day, PD.Legdays, PO.POCreationDate),'dd-MMM-yyyy')
when PD.DependentDate='POStartDate' then format(DATEADD(Day, PD.Legdays, PO.POStartDate),'dd-MMM-yyyy')
when PD.DependentDate='POEndDate' then format(DATEADD(Day, PD.Legdays,PO.POEndDate),'dd-MMM-yyyy')
end) QualityPlanDate,
PO.POStartDate,PO.POEndDate,PO.POCreationDate,
isnull(QPC.QPEmployeeId,PD.ResponsiblePersonId) as QPEmployeeId,
isnull((select EmployeeName from EmployeeInformation where SystemId=QPC.QPEmployeeId),(select EmployeeName from EmployeeInformation where SystemId=PD.ResponsiblePersonId)) as QPEmployee
from (select distinct PO.Id,PS.UserName POStatus, 'PO' EntryLevel,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
reverse(stuff(reverse((select distinct LotNumber + ',' from TRN.ProductionSummary where ProductionOrderId=PO.Id and ProcessId=Prod.ProcessId for xml path(''))),1,1,'')) as LotNumber,
PO.EntityId,Prod.ProcessId,POFirstProdBookDate,POProcessFirstProdBookDate,POLatestProdBookDate,BaseProcPlanStartDate,BaseProcPlanCompletionDate, 
isnull(format(FBPPD.POFirstProdBookDate,'dd-MMM-yyyy'),format(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')) POStartDate,
isnull(format(Type1.BaseProcPlanCompletionDate,'dd-MMM-yyyy'),format(LBPPD.POLatestProdBookDate,'dd-MMM-yyyy')) POEndDate,
format(PO.AddedDate,'dd-MMM-yyyy') POCreationDate
from TRN.ProductionOrder PO
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
left join TRN.ProductionSummary Prod on Prod.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POFirstProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POProcessFirstProdBookDate,ProductionOrderId,ProcessId From TRN.ProductionSummary Group By ProductionOrderId,ProcessId) POPFBD ON POPFBD.ProductionOrderId=PO.Id and Prod.ProcessId=POPFBD.ProcessId
LEFT JOIN (Select SUM(Quantity)ProQty,MAX(ProductionDate)POLatestProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) LBPPD ON LBPPD.ProductionOrderId=PO.Id
LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanCompletionDate,ProductionOrderId From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=PO.Id
where PS.UserName in ('Running','To Close') 
union
select distinct PO.Id,PS.UserName POStatus, 'LOT' EntryLevel,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),Prod.LotNumber,PO.EntityId,
Prod.ProcessId,POFirstProdBookDate,POProcessFirstProdBookDate,POLatestProdBookDate,BaseProcPlanStartDate,BaseProcPlanCompletionDate, 
isnull(format(FBPPD.POFirstProdBookDate,'dd-MMM-yyyy'),format(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')) POStartDate,
isnull(format(Type1.BaseProcPlanCompletionDate,'dd-MMM-yyyy'),format(LBPPD.POLatestProdBookDate,'dd-MMM-yyyy')) POEndDate,
format(PO.AddedDate,'dd-MMM-yyyy') POCreationDate
from TRN.ProductionOrder PO
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
left join TRN.ProductionSummary Prod on Prod.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POFirstProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POProcessFirstProdBookDate,ProductionOrderId,ProcessId From TRN.ProductionSummary Group By ProductionOrderId,ProcessId) POPFBD ON POPFBD.ProductionOrderId=PO.Id and Prod.ProcessId=POPFBD.ProcessId
LEFT JOIN (Select SUM(Quantity)ProQty,MAX(ProductionDate)POLatestProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) LBPPD ON LBPPD.ProductionOrderId=PO.Id
LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanCompletionDate,ProductionOrderId From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=PO.Id
where PS.UserName in ('Running','To Close')) PO
left join MST.POQualityPlanDetails PD on PD.EntryLevel=PO.EntryLevel
left Join MST.QualityManagementMaster QMM on QMM.Id=PD.IssueId
left join [TRN].[QualityPlanControl] QPC on QPC.QPId=PD.Id and QPC.POId=PO.Id and QPC.LotNumber=PO.LotNumber and QPC.EntryLevel=PO.EntryLevel
left join TRN.ProductionOrderDetail POD on POD.ProductionOrderId=PO.Id
left join TRN.SalesOrder SO on SO.Id=POD.SalesOrderId 
left join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
left join hkp.Process P on P.Id=PO.ProcessId
left join ORG.Entity E on E.Id=PO.EntityId
where PO.ProcessId is null and E.Id in (select EntityId from MST.QualityManagementEntity where QMID=QMM.Id) 
and QPC.QCID is null or (select top 1 RepeatEntry from TRN.QualityControl where IssueId=QMM.Id and QualityPlanId=QPC.Id and PlanType='POIssue' order by AddedDate desc) is not null
) PO1
where PO1.QualityPlanDate < = '" + POIssueDate + "'" + ResponsiblePerson + @" or PO1.QualityPlanDate is null order by PO1.QualityPlanDate";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new QualityPOIssue
                    {
                        PODate = dsRef.Tables[0].Rows[i]["PODate"].ToString(),
                        QPDate = dsRef.Tables[0].Rows[i]["QPDate"].ToString(),
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        QPId = dsRef.Tables[0].Rows[i]["QPId"].ToString(),
                        RepeatEntry = dsRef.Tables[0].Rows[i]["RepeatEntry"].ToString(),
                        POId = dsRef.Tables[0].Rows[i]["POId"].ToString(),
                        EntityId = dsRef.Tables[0].Rows[i]["EntityId"].ToString(),
                        Entity = dsRef.Tables[0].Rows[i]["Entity"].ToString(),
                        ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                        Process = dsRef.Tables[0].Rows[i]["Process"].ToString(),
                        IssueId = dsRef.Tables[0].Rows[i]["IssueId"].ToString(),
                        QPIssue = dsRef.Tables[0].Rows[i]["QPIssue"].ToString(),
                        DependentOn = dsRef.Tables[0].Rows[i]["DependentOn"].ToString(),
                        Legdays = dsRef.Tables[0].Rows[i]["Legdays"].ToString(),
                        Date = dsRef.Tables[0].Rows[i]["Date"].ToString(),
                        QualityPlanDate = dsRef.Tables[0].Rows[i]["QualityPlanDate"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        LotNumber = dsRef.Tables[0].Rows[i]["LotNumber"].ToString(),
                        EntryLevel = dsRef.Tables[0].Rows[i]["EntryLevel"].ToString(),
                        Customer = dsRef.Tables[0].Rows[i]["Customer"].ToString(),
                        POStatus = dsRef.Tables[0].Rows[i]["POStatus"].ToString(),
                        QPEmployeeId = dsRef.Tables[0].Rows[i]["QPEmployeeId"].ToString(),
                        QPEmployee = dsRef.Tables[0].Rows[i]["QPEmployee"].ToString(),

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

        public void GetQualityShift(out List<Default> DataList, string processId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT distinct sd.SystemID [Value],sd.UserName [Text] FROM [dbo].[WorkCenterWiseShift] WCS
                                        LEFT JOIN dbo.ShiftDefination AS sd ON sd.SystemID = WCS.ShiftDefinationID
                                        WHERE WorkCenterMasterId IN(SELECT Id FROM SCS.WorkCenterMaster AS wcm  WHERE wcm.ProcessId='" + processId + "')";
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

        public void GetQualityPO(out List<Default> DataList, string EntityId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT distinct PO.Id [Value],PO.Id [Text]

								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   WHERE PS.UserName in ('Running','To Close') and PO.EntityId='" + EntityId + "'";
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

        public void GetQualityPeriod(out List<Default> DataList, string IssueId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select  Top 1 WTD.Id [Value],WTD.PeriodName +' (' + format(WTD.FromTime,'hh:mm tt')+' - ' + format(WTD.ToTime,'hh:mm tt')+')' as  [Text] from [MST].[QualityTimeDetails] WTD
where WTD.IssueId='" + IssueId + "'";
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

        public string PostQualityHeader(IEnumerable<QualityHeader> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.QualityControl";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<QualityHeader> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from TRN.QualityControl where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (QualityHeader item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);



                        dr["Id"] = "22" + _Id;
                        dr["PlantId"] = item.PlantId;
                        dr["EntityId"] = item.EntityId;
                        dr["ProcessId"] = item.ProcessId;
                        dr["ProductionDate"] = item.ProductionDate;
                        dr["ProductionShiftId"] = item.ProductionShiftId;
                        dr["ProductionOrderId"] = item.ProductionOrderId;
                        dr["IssueId"] = item.IssueId;
                        dr["PeriodId"] = item.PeriodId;
                        dr["ProductionInchargeId"] = item.ProductionInchargeId;
                        dr["LotNumber"] = item.LotNumber;
                        dr["Remarks"] = item.Remarks;
                        dr["MasterOrderItemId"] = item.MasterOrderItemId;
                        dr["SalesOrderId"] = item.SalesOrderId;
                        dr["QualityPlanId"] = item.QualityPlanId;
                        dr["PlanType"] = item.PlanType;
                        dr["WorkCenterId"] = item.WorkCenterId;
                        dr["RepeatEntry"] = item.RepeatEntry;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

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

        public string PostQualityHeaderChild(IEnumerable<QualityHeaderChild> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.QualityControlDetails";
                string Id = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<QualityHeaderChild> items = DataToSave.ToList();
                foreach (QualityHeaderChild item in DataToSave)
                {
                    Id += ",'" + item.Id + "'";
                }
                con.OpenDataSetThroughAdapter("select * from TRN.QualityControlDetails where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (QualityHeaderChild item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.Id + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);



                        dr["Id"] = "23" + _Id;
                        dr["QCId"] = item.QCId;
                        dr["ItemId"] = item.ItemId;
                        dr["Value"] = item.Value;
                        dr["GradeId"] = item.GradeId;
                        dr["ActionToBeTaken"] = item.ActionToBeTaken;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["Remarks"] = item.Remarks;
                        dr["Repeat"] = false;
                        dr["RepeatEntry"] = item.RepeatEntry;
                        dr["WorkCenterId"] = item.WorkCenterId;
                        dr["Status"] = item.Status;
                        dr["ConfirmBy"] = item.ConfirmBy;
                        dr["ConfirmationRemarks"] = item.ConfirmationRemarks;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

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

        public void GetQualityGrade(out List<Default> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select QGD.Id as Value,QGD.GradeName as Text from [MST].[QualityGradeDetails] QGD";
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

        public void GetQualityActionToBeTaken(out List<Default> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id as Value,ActionToBeTakenName as Text from [MST].[QualityActionToBeTakenDetails]";
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

        public void GetQualityWorkCenter(out List<Default> DataList, string IssueId, string EntityId, string ProcessId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select QMW.WorkCenterMasterId as Value, WCM.UserName as Text from MST.QualityManagementWorkCenter QMW
left join scs.WorkCenterMaster WCM on WCM.Id=QMW.WorkCenterMasterId
where QMW.QMID ='" + IssueId + "' and WCM.EntityId='" + EntityId + "' and WCM.ProcessId='" + ProcessId + "'";
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

        public void GetQualityChildList(out List<QualityChild> DataList, string IssueId, string PId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<QualityChild>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct QIC.Id,QII.Id ItemId,QII.SNO,PM.UserName ItemName,QII.UOMId,U.UserName as UOM,QIC.Value,QGD.Id as GradeId,QII.Max as MaxValue,QII.Min as MinValue,
QIC.Remarks,QIC.ActionToBeTaken,isnull(QIC.WorkCenterId,(select WorkCenterId from TRN.QualityControl where Id='" + PId + @"')) as WorkCenterId,
isnull(R.EmployeeName,(select EmployeeName from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.[QualityControlDetails] where ItemId=QII.Id order by AddedDate desc))) as ResponsiblePerson,
isnull(QIC.ResponsiblePersonId,(select top 1 ResponsiblePersonId from TRN.[QualityControlDetails] where ItemId=QII.Id order by AddedDate desc)) as ResponsiblePersonId,
reverse(stuff(reverse((select CheckPoints +',' from QualityManagementParameterCheckPoints where ParameterId=QII.Id for xml path(''))),1,1,'')) as Checkpoints,
QIC.QCId,QIC.Repeat,(select IsWorkCenter from MST.QualityManagementParameterItem where Id=QII.Id) as IsWorkCenter
from MST.QualityManagementParameterItem QII
LEFT JOIN TRN.QualityControl QC ON QC.IssueId=QII.QMID
LEFT JOIN TRN.[QualityControlDetails] QIC ON QIC.QCId='" + PId + @"' and QIC.ItemId=QII.Id
LEFT JOIN SCS.UnitOfMeasurement U ON U.Id = QII.UOMId
left Join MST.QualityGradeDetails QGD ON QGD.Id=QIC.GradeId
LEFT JOIN EmployeeInformation R ON  R.SystemId = QIC.ResponsiblePersonId
left join hkp.ParameterMaster PM on PM.Id=QII.ParameterId
where QII.QMID='" + IssueId + "' and QII.IsActive = 1  order by QII.SNO";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new QualityChild
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        ItemId = dsRef.Tables[0].Rows[i]["ItemId"].ToString(),
                        SNO = dsRef.Tables[0].Rows[i]["SNO"].ToString(),
                        ItemName = dsRef.Tables[0].Rows[i]["ItemName"].ToString(),
                        UOMId = dsRef.Tables[0].Rows[i]["UOMId"].ToString(),
                        UOM = dsRef.Tables[0].Rows[i]["UOM"].ToString(),
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        GradeId = dsRef.Tables[0].Rows[i]["GradeId"].ToString(),
                        MaxValue = dsRef.Tables[0].Rows[i]["MaxValue"].ToString(),
                        MinValue = dsRef.Tables[0].Rows[i]["MinValue"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        ActionToBeTaken = dsRef.Tables[0].Rows[i]["ActionToBeTaken"].ToString(),
                        WorkCenterId = dsRef.Tables[0].Rows[i]["WorkCenterId"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        Checkpoints = dsRef.Tables[0].Rows[i]["Checkpoints"].ToString(),
                        QCId = dsRef.Tables[0].Rows[i]["QCId"].ToString(),
                        Repeat = dsRef.Tables[0].Rows[i]["Repeat"].ToString(),
                        IsWorkCenter = dsRef.Tables[0].Rows[i]["IsWorkCenter"].ToString(),

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
        public void GetGIEmployee(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct EI.SystemId Value,EI.EmployeeName Name from 
TRN.QualityIssueControl QIC
left join dbo.EmployeeInformation EI on EI.SystemId=QIC.QGIEmployeeId
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
where EI.EmployeeStatus='Active' and EI.EmployeeCode is not null and QIC.QGIEmployeeId is not null 
and QIC.QCId is null";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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
        public void GetPIEmployee(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct EI.SystemId Value,EI.EmployeeName Name  from 
TRN.QualityPlanControl QPC
left join dbo.EmployeeInformation EI on EI.SystemId=QPC.QPEmployeeId
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
where EI.EmployeeStatus='Active' and EI.EmployeeCode is not null and QPC.QPEmployeeId is not null 
and QPC.QCId is null";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetResponsibleEmployee(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct EI.SystemId Value,EI.EmployeeName Name from [MST].[QualityActionResponsiblePerson] QAP
left join dbo.EmployeeInformation EI on QAP.QualityActionResponsiblePersonId = EI.SystemId";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetProductionBookingLevel(out List<Default2> DataList, string ProcessId, string EntityId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id Value , ProductionBookingLevel Name from hkp.EntityProcessTag where ProcessId = '" + ProcessId + "' and EntityId = '" + EntityId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetArticleItems(out List<ArticleItem> DataList, string ProductionOrderId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<ArticleItem>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT DISTINCT so.MasterOrderItemId
	                                ,ISNULL(so.Id,'') SOId
                                    
	                                ,ISNULL(mma.StandardName, '') Article
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS  GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id 
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE  PS.UserName in ('Running','To Close')  AND PO.Id='" + ProductionOrderId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new ArticleItem
                    {
                        MasterOrderItemId = dsRef.Tables[0].Rows[i]["MasterOrderItemId"].ToString(),
                        SOId = dsRef.Tables[0].Rows[i]["SOId"].ToString(),
                        Article = dsRef.Tables[0].Rows[i]["Article"].ToString(),

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

        public string PostQualityProcess(IEnumerable<QualityHeader> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.QualityControl";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<QualityHeader> items = DataToSave.ToList();

                con.executeQuery("Delete From TRN.QualityPlanControl where QCId is null");
                con.OpenDataSetThroughAdapter("select * from TRN.QualityControl where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (QualityHeader item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);



                        dr["Id"] = "22" + _Id;
                        dr["PlantId"] = item.PlantId;
                        dr["EntityId"] = item.EntityId;
                        dr["ProcessId"] = item.ProcessId;
                        dr["ProductionDate"] = item.ProductionDate;
                        dr["ProductionShiftId"] = item.ProductionShiftId;
                        dr["ProductionOrderId"] = item.ProductionOrderId;
                        dr["IssueId"] = item.IssueId;
                        dr["PeriodId"] = item.PeriodId;
                        dr["ProductionInchargeId"] = item.ProductionInchargeId;
                        dr["LotNumber"] = item.LotNumber;
                        dr["Remarks"] = item.Remarks;
                        dr["MasterOrderItemId"] = item.MasterOrderItemId;
                        dr["SalesOrderId"] = item.SalesOrderId;
                        dr["QualityPlanId"] = item.QualityPlanId;
                        dr["PlanType"] = item.PlanType;
                        dr["WorkCenterId"] = item.WorkCenterId;
                        dr["RepeatEntry"] = item.RepeatEntry;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

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
        public string PostQualityProcess(IEnumerable<QualityPlanProcess> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.QualityPlanControl";
                string Id = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<QualityPlanProcess> items = DataToSave.ToList();
                foreach (QualityPlanProcess item in DataToSave)
                {
                    Id += ",'" + item.Id + "'";
                }

                con.executeQuery("Delete From TRN.QualityPlanControl where QCId is null");
                con.OpenDataSetThroughAdapter("select * from TRN.QualityPlanControl where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (QualityPlanProcess item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.Id + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);



                        dr["Id"] = "23" + _Id;
                        dr["QPId"] = item.QPId;
                        dr["POId"] = item.POId;
                        dr["IssueId"] = item.IssueId;
                        dr["DependentOn"] = item.DependentOn;
                        dr["Date"] = item.Date;
                        dr["QualityPlanDate"] = item.QualityPlanDate;
                        dr["QCId"] = item.QCId;
                        dr["QPEmployeeId"] = item.QPEmployeeId;
                        dr["RepeatEntry"] = item.RepeatEntry;
                        dr["LotNumber"] = item.LotNumber;
                        dr["EntryLevel"] = item.EntryLevel;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);

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
        #endregion Quality Control

        #region Leave
        public DataSet GetCalYearInfo(string CalYearId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select * from YearlyCalendar WHERE ID='" + CalYearId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public void GetLeaveBalanceType(out List<Leavesystem> DataList, string EmployeeSystemId, string calYearId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Leavesystem>();

            System.Data.DataSet dsRef;
            try
            {
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                // var esic = GetESICEligibleEmployee(EmpSystemID);
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {

                    DataTable dtCalendar = _sqlRepository.GetDataTable("select * from YearlyCalendar where YearNo=" + DateTime.Now.Year.ToString() + @" AND PlantId=''");
                    if (dtCalendar.Rows.Count > 0)
                    {
                        _FromDate = dtCalendar.Rows[0]["FromDate"].ToString();
                        _ToDate = dtCalendar.Rows[0]["ToDate"].ToString();
                    }
                }

                string _sql = @"Select * from (SELECT ei.SystemId,lt.Id LeaveTypeId,ei.EmployeeCode,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy') AS DOJ,p.UserName AS PlantName,D.UserName AS Designation,
DEPT.UserName AS Department,ct.UserName AS EmployeeCategory, lt.UserName LeaveName
										 ,	DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end
 ,CurrentYearAllocation=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end,
                                        CurrentAllocation=ISNULL(CASE WHEN LT.LeaveType='Earn' THEN ALD.Opening ELSE ISNULL(els.CurrentYearAllocation, 0) END,0)
										,0 YearEndEncash,''AppliedLeave
										,CarryForwardOpeningBalance=CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                                         BroughtForward=CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END
										  ,LeaveDays=(CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)+(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end)
										   ,ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) AppliedDays
										   ,ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) AvailedDays
                                            ,ISNULL(tav.av, 0)AllFutureAppliedLeave
										 ,ClosingBalance=((CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)+(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end))-(ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0))

----------------------------------------------------------------------------------------------------------------------
                                          FROM (    select S.* from trn.EmployeeLeaveSummary S
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
														where CalanderYearId IN (Select Id from YearlyCalendar  where '" + _FromDate + @"' BETWEEN FromDate AND ToDate)
														AND S.EmployeeId ='" + EmployeeSystemId + @"' AND lp.EncashmentBasis='CalanderYear'

                                                        UNION

                                                        select S.* from trn.EmployeeLeaveSummary S
                                                        JOIN  trn.EmployeeLeaveSummary SS ON S.Id=ss.Id
                                                        AND S.Id=(SELECT TOP 1 SX.Id FROM trn.EmployeeLeaveSummary SX WHERE ss.EmployeeId=SX.EmployeeId AND ss.LeaveTypeId=SX.LeaveTypeId ORDER BY sx.ToDate DESC)
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
														Join YearlyCalendar AS C ON C.PlantId=EI.PlantId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                                                        where S.EmployeeId ='" + EmployeeSystemId + @"' AND lp.EncashmentBasis<>'CalanderYear') els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
LEFT JOIN
											(
										   select A.Opening,A.EmployeeId,A.LeaveTypeId,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate from dbo.AnnualLeaveDataCurrent A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
											)ALD ON ALD.EmployeeId=els.EmployeeId AND lt.Id=ALD.LeaveTypeId

 LEFT JOIN
											(
										  select PBroughtForward=CASE WHEN A.Opening=0 THEN A.Adjustment ELSE A.Opening END,A.EmployeeId,A.LeaveTypeId from dbo.AnnualLeaveDataPast A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
											)ALP ON ALP.EmployeeId=els.EmployeeId AND lt.Id=ALP.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
                            where  (FromDate between '" + _FromDate + @"' and '" + _ToDate + @"') and (ToDate between '" + _FromDate + @"' and '" + _ToDate + @"')
                                                    group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID,m.EmpSystemID from  dbo.LeaveTransaction m
LEFT JOIN dbo.LeaveTransactionDetails d ON d.LvTrnsSystemID=m.SystemId
where d.IsAvailed = 1 and d.WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
group by d.LvTrnsSystemID,m.EmpSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																				LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId --where dc.plantid=''
 ) dm where dm.DesignationId =(select givendesignationId  from dbo.EmployeeInformation  where SystemId ='" + EmployeeSystemId + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
LEFT JOIN EmployeeInformation AS ei ON ei.SystemId  = els.EmployeeId
LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
LEFT JOIN hkp.EmployeeCategory CT ON ct.Id=dm.EmployeeCategoryId
LEFT JOIN org.Plant AS p ON p.Id=ei.PlantId                              
LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
 WHERE els.EmployeeID ='" + EmployeeSystemId + @"'   AND LT.UserName NOT LIKE '%Maternity%')A";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(_sql, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Leavesystem
                    {
                        SystemId = dsRef.Tables[0].Rows[i]["SystemId"].ToString(),
                        LeaveTypeId = dsRef.Tables[0].Rows[i]["LeaveTypeId"].ToString(),
                        EmployeeCode = dsRef.Tables[0].Rows[i]["EmployeeCode"].ToString(),
                        EmployeeName = dsRef.Tables[0].Rows[i]["EmployeeName"].ToString(),
                        DOJ = dsRef.Tables[0].Rows[i]["DOJ"].ToString(),
                        PlantName = dsRef.Tables[0].Rows[i]["PlantName"].ToString(),
                        Designation = dsRef.Tables[0].Rows[i]["Designation"].ToString(),
                        Department = dsRef.Tables[0].Rows[i]["Department"].ToString(),
                        EmployeeCategory = dsRef.Tables[0].Rows[i]["EmployeeCategory"].ToString(),
                        LeaveName = dsRef.Tables[0].Rows[i]["LeaveName"].ToString(),
                        DaysCanBeSanctioned = dsRef.Tables[0].Rows[i]["DaysCanBeSanctioned"].ToString(),
                        CurrentYearAllocation = dsRef.Tables[0].Rows[i]["CurrentYearAllocation"].ToString(),
                        CurrentAllocation = dsRef.Tables[0].Rows[i]["CurrentAllocation"].ToString(),
                        YearEndEncash = dsRef.Tables[0].Rows[i]["YearEndEncash"].ToString(),
                        AppliedLeave = dsRef.Tables[0].Rows[i]["AppliedLeave"].ToString(),
                        CarryForwardOpeningBalance = dsRef.Tables[0].Rows[i]["CarryForwardOpeningBalance"].ToString(),
                        BroughtForward = dsRef.Tables[0].Rows[i]["BroughtForward"].ToString(),
                        LeaveDays = dsRef.Tables[0].Rows[i]["LeaveDays"].ToString(),
                        AppliedDays = dsRef.Tables[0].Rows[i]["AppliedDays"].ToString(),
                        AvailedDays = dsRef.Tables[0].Rows[i]["AvailedDays"].ToString(),
                        AllFutureAppliedLeave = dsRef.Tables[0].Rows[i]["AllFutureAppliedLeave"].ToString(),
                        ClosingBalance = dsRef.Tables[0].Rows[i]["ClosingBalance"].ToString(),

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
        #endregion Leave
        #region EmployeeUsrId
        public void GetEmployeeUserId(out List<Default2> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select EI.SystemId Value, EI.EmployeeCode Name from sec.[User] US
left join EmployeeInformation EI on EI.SystemId  = US.EmployeeId where US.UserId = '" + UserId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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
        #endregion EmployeeUsrId

        #region Daily Account
        public void GetBankCategory(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id Value , UserName Name from  HKP.BankCategory";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetBankSubCategory(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id Value , UserName Name from  HKP.BankSubCategory";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetBankName(out List<Default2> DataList, string categoryId, string subcategoryId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Distinct BK.UserName Name , BK.Id Value from mst.BankMaster BM 
left join HKP.Bank BK on BK.Id = BM.BankId 
where BM.BankCategoryId = '" + categoryId + "' and BM.BankSubCategoryId = '" + subcategoryId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetBankAccount(out List<Default2> DataList, string bankId, string categoryId, string subcategoryId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Distinct BM.AccountNumber Name , BM.Id Value from mst.BankMaster BM 
left join HKP.Bank BK on BK.Id = BM.BankId 
where BM.BankCategoryId = '" + categoryId + "' and BM.BankSubCategoryId = '" + subcategoryId + "' and BM.BankId = '" + bankId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public string PostDailyAccountClosing(IEnumerable<AccountBalence> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.DailyAccountBalence";
                string Id = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<AccountBalence> items = DataToSave.ToList();

                foreach (AccountBalence item in DataToSave)
                {
                    Id += ",'" + item.Id + "'";
                }

                con.OpenDataSetThroughAdapter("select * from TRN.DailyAccountBalence where Id='" + items[0].Id + "'", out dsMaster, false, "1");


                foreach (AccountBalence item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.Id + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);

                        dr["Id"] = _Id;
                        dr["BankMasterId"] = item.BankMasterId;
                        dr["ClosingDate"] = item.ClosingDate;
                        dr["ClosingBalence"] = item.ClosingBalence;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();



                        dsMaster.Tables[0].Rows.Add(dr);

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
        #endregion Daily Account 

        #region Gate pass
        public void GetGatepasschecking(out List<GatePassCheckApprove> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<GatePassCheckApprove>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"Select SC.*,WE.EmployeeName ByWhom,SE.EmployeeName SecurityInCharge,RE.EmployeeName ResponsiblePerson,CE.EmployeeName CheckBy,AE.EmployeeName ApproveBy
								from [dbo].[SalesChalan] SC
								LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=SC.ByWhomId
								LEFT JOIN dbo.EmployeeInformation SE ON SE.SystemId=SC.SecurityInChargeId
								LEFT JOIN dbo.EmployeeInformation RE ON RE.SystemId=SC.ResponsiblePersonId 
								LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=SC.CheckById
								LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=SC.ApproveById 
								where SC.CheckedStatus='To Be Check' AND SC.CheckById='" + UserId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new GatePassCheckApprove
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        VechileNo = dsRef.Tables[0].Rows[i]["VechileNo"].ToString(),
                        ByWhomId = dsRef.Tables[0].Rows[i]["ByWhomId"].ToString(),
                        MobileNo = dsRef.Tables[0].Rows[i]["MobileNo"].ToString(),
                        SecurityInChargeId = dsRef.Tables[0].Rows[i]["SecurityInChargeId"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        CheckById = dsRef.Tables[0].Rows[i]["CheckById"].ToString(),
                        ApproveById = dsRef.Tables[0].Rows[i]["ApproveById"].ToString(),
                        UserRef = dsRef.Tables[0].Rows[i]["UserRef"].ToString(),
                        FromDate = dsRef.Tables[0].Rows[i]["FromDate"].ToString(),
                        Remark = dsRef.Tables[0].Rows[i]["Remark"].ToString(),
                        ToDate = dsRef.Tables[0].Rows[i]["ToDate"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        AddedFromIP = dsRef.Tables[0].Rows[i]["AddedFromIP"].ToString(),
                        UpdatedBy = dsRef.Tables[0].Rows[i]["UpdatedBy"].ToString(),
                        UpdatedDate = dsRef.Tables[0].Rows[i]["UpdatedDate"].ToString(),
                        UpdatedFromIP = dsRef.Tables[0].Rows[i]["UpdatedFromIP"].ToString(),
                        ApprovedReason = dsRef.Tables[0].Rows[i]["ApprovedReason"].ToString(),
                        ApprovedStatus = dsRef.Tables[0].Rows[i]["ApprovedStatus"].ToString(),
                        CheckedStatus = dsRef.Tables[0].Rows[i]["CheckedStatus"].ToString(),
                        CheckedReason = dsRef.Tables[0].Rows[i]["CheckedReason"].ToString(),
                        IsDispatchConfirmation = dsRef.Tables[0].Rows[i]["IsDispatchConfirmation"].ToString(),
                        DispatchConfirmationBy = dsRef.Tables[0].Rows[i]["DispatchConfirmationBy"].ToString(),
                        DispatchConfirmationDate = dsRef.Tables[0].Rows[i]["DispatchConfirmationDate"].ToString(),
                        ByWhom = dsRef.Tables[0].Rows[i]["ByWhom"].ToString(),
                        SecurityInCharge = dsRef.Tables[0].Rows[i]["SecurityInCharge"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        CheckBy = dsRef.Tables[0].Rows[i]["CheckBy"].ToString(),
                        ApproveBy = dsRef.Tables[0].Rows[i]["ApproveBy"].ToString(),

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

        public void GetGatepassapproving(out List<GatePassCheckApprove> DataList, string UserId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<GatePassCheckApprove>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"Select SC.*,WE.EmployeeName ByWhom,SE.EmployeeName SecurityInCharge,RE.EmployeeName ResponsiblePerson,CE.EmployeeName CheckBy,AE.EmployeeName ApproveBy
								from [dbo].[SalesChalan] SC
								LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=SC.ByWhomId
								LEFT JOIN dbo.EmployeeInformation SE ON SE.SystemId=SC.SecurityInChargeId
								LEFT JOIN dbo.EmployeeInformation RE ON RE.SystemId=SC.ResponsiblePersonId
								LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=SC.CheckById
								LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=SC.ApproveById 
								where SC.CheckedStatus='Checked' AND SC.ApprovedStatus='To Be Approve' AND SC.ApproveById='" + UserId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new GatePassCheckApprove
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        VechileNo = dsRef.Tables[0].Rows[i]["VechileNo"].ToString(),
                        ByWhomId = dsRef.Tables[0].Rows[i]["ByWhomId"].ToString(),
                        MobileNo = dsRef.Tables[0].Rows[i]["MobileNo"].ToString(),
                        SecurityInChargeId = dsRef.Tables[0].Rows[i]["SecurityInChargeId"].ToString(),
                        ResponsiblePersonId = dsRef.Tables[0].Rows[i]["ResponsiblePersonId"].ToString(),
                        CheckById = dsRef.Tables[0].Rows[i]["CheckById"].ToString(),
                        ApproveById = dsRef.Tables[0].Rows[i]["ApproveById"].ToString(),
                        UserRef = dsRef.Tables[0].Rows[i]["UserRef"].ToString(),
                        FromDate = dsRef.Tables[0].Rows[i]["FromDate"].ToString(),
                        Remark = dsRef.Tables[0].Rows[i]["Remark"].ToString(),
                        ToDate = dsRef.Tables[0].Rows[i]["ToDate"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        AddedFromIP = dsRef.Tables[0].Rows[i]["AddedFromIP"].ToString(),
                        UpdatedBy = dsRef.Tables[0].Rows[i]["UpdatedBy"].ToString(),
                        UpdatedDate = dsRef.Tables[0].Rows[i]["UpdatedDate"].ToString(),
                        UpdatedFromIP = dsRef.Tables[0].Rows[i]["UpdatedFromIP"].ToString(),
                        ApprovedReason = dsRef.Tables[0].Rows[i]["ApprovedReason"].ToString(),
                        ApprovedStatus = dsRef.Tables[0].Rows[i]["ApprovedStatus"].ToString(),
                        CheckedStatus = dsRef.Tables[0].Rows[i]["CheckedStatus"].ToString(),
                        CheckedReason = dsRef.Tables[0].Rows[i]["CheckedReason"].ToString(),
                        IsDispatchConfirmation = dsRef.Tables[0].Rows[i]["IsDispatchConfirmation"].ToString(),
                        DispatchConfirmationBy = dsRef.Tables[0].Rows[i]["DispatchConfirmationBy"].ToString(),
                        DispatchConfirmationDate = dsRef.Tables[0].Rows[i]["DispatchConfirmationDate"].ToString(),
                        ByWhom = dsRef.Tables[0].Rows[i]["ByWhom"].ToString(),
                        SecurityInCharge = dsRef.Tables[0].Rows[i]["SecurityInCharge"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        CheckBy = dsRef.Tables[0].Rows[i]["CheckBy"].ToString(),
                        ApproveBy = dsRef.Tables[0].Rows[i]["ApproveBy"].ToString(),

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

        public void GetGatepassAprovelperson(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select EI.EmployeeName Name , EI.SystemId Value from AuthorizationConfig ATC
left join EmployeeInformation EI on EI.systemid = ATC.Employeeid
where ActionStatus = 'GatePassApproveBy'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public string PostGatePassChecking(IEnumerable<GatePassCheckApprove> DataToSave, string GatePassId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<GatePassCheckApprove> items = DataToSave.ToList();


                con.OpenDataSetThroughAdapter("select * from dbo.SalesChalan where Id='" + GatePassId + "'", out dsMaster, false, "1");

                foreach (GatePassCheckApprove item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        // DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["CheckById"] = item.CheckById;
                        dr["ApproveById"] = item.ApproveById;
                        dr["CheckedStatus"] = item.CheckedStatus;
                        dr["ApprovedStatus"] = item.ApprovedStatus;

                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = "163.47.212.50";


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
                throw (ex);
            }
        }

        public string PostGatePassApprove(IEnumerable<GatePassCheckApprove> DataToSave, string GatePassId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<GatePassCheckApprove> items = DataToSave.ToList();


                con.OpenDataSetThroughAdapter("select * from dbo.SalesChalan where Id='" + GatePassId + "'", out dsMaster, false, "1");

                foreach (GatePassCheckApprove item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        // DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["ApproveById"] = item.ApproveById;
                        dr["ApprovedStatus"] = item.ApprovedStatus;

                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = "163.47.212.50";


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
                throw (ex);
            }
        }
        #endregion Gate pass

        public void GetEmployeeInColumnWithoutAssociate(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select CONCAT(EMP.EmployeeCode, '       ' , EMP.EmployeeName) as Name  , EMp.SystemId as Value from EmployeeInformation EMP
LEFT JOIN MST.DesignationMasterLegalDesignation DMLD ON DMLD.LegalDesignationId = EMP.LegalDesignationId
left join mst.DesignationMaster dm on dm.Id = DMLD.DesignationMasterId
left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
Where EMP.EmployeeStatus = 'Active' and x.UserName <> 'Associate'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetLeaveApprovestatus(out List<Default2> DataList, string Fmdate, string Todate)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select LockedDate Name , IsActive Value from PlantWiseAttendanceLock where PlantId='202034'
                and LockedDate between '" + Fmdate + "' and '" + Todate + "' and IsActive='1'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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
        public void GetInvoiceResponsibleperson(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct EI.SystemId Value, EI.EmployeeName Name from trn.Sales IR
left join hkp.Party Pt on Pt.Id = PartyId
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
left join EmployeeInformation EI on EI.SystemId = MO.ResponsiblePersonId
where Invoicestatus <> 'Closed' and EI.SystemId is not null";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetInvoiceCustomen(out List<Default2> DataList, string Respr, string Type)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();
            var CusAll = "";
            if (Type != null)
            {
                if (Type == "Export")
                {
                    Type = "Customer Export";
                }
                if (Type == "Local")
                {
                    Type = "Customer Local";
                }
                if (Type == "Both")
                {
                    Type = null;
                }
            }
            if (Respr != null && Type == null)
            {
                CusAll = " and EI.SystemId = '" + Respr + "'";
            }
            if (Respr == null && Type != null)
            {
                CusAll = " and PAG.StandardName = '" + Type + "'";
            }
            if (Respr != null && Type != null)
            {
                CusAll = " and EI.SystemId = '" + Respr + "' and PAG.StandardName = '" + Type + "'";
            }


            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct pt.Id Value , PT.UserName Name from trn.Sales IR
left join hkp.Party Pt on Pt.Id = PartyId
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
left join HKP.CompanyParty CP on CP.PartyId = Pt.Id and CP.PartyType = 'Customer'
left join HKP.PartyAccountGroup PAG on PAG.Id = CP.PartyAccountGroupId 
left join EmployeeInformation EI on EI.SystemId = MO.ResponsiblePersonId
where Invoicestatus <> 'Closed' and EI.SystemId is not null" + CusAll;
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public void GetInvoiceNumber(out List<Default2> DataList, string Respr, string Type, string Customer)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();
            var CusAll = "";
            if (Type != null)
            {
                if (Type == "Export")
                {
                    Type = "Customer Export";
                }
                if (Type == "Local")
                {
                    Type = "Customer Local";
                }
                if (Type == "Both")
                {
                    Type = null;
                }
            }
            if (Respr != null && Type == null && Customer == null)
            {
                CusAll = " and EI.SystemId = '" + Respr + "'";
            }
            if (Respr != null && Type != null && Customer == null)
            {
                CusAll = " and EI.SystemId = '" + Respr + "' and PAG.StandardName = '" + Type + "'";
            }
            if (Respr != null && Type != null && Customer != null)
            {
                CusAll = " and EI.SystemId = '" + Respr + "' and PAG.StandardName = '" + Type + "' and PT.Id = '" + Customer + "'";
            }

            if (Respr == null && Type != null && Customer == null)
            {
                CusAll = " and PAG.StandardName = '" + Type + "'";
            }
            if (Respr == null && Type == null && Customer != null)
            {
                CusAll = " and PT.Id = '" + Customer + "'";
            }
            if (Respr == null && Type != null && Customer != null)
            {
                CusAll = " and PAG.StandardName = '" + Type + "' and PT.Id = '" + Customer + "'";
            }
            if (Respr != null && Type == null && Customer != null)
            {
                CusAll = " and EI.SystemId = '" + Respr + "' and PT.Id = '" + Customer + "'";
            }
            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct IR.InvoiceNo Name, IR.Id Value
from trn.Sales IR
left join hkp.Party Pt on Pt.Id = PartyId
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
left join HKP.CompanyParty CP on CP.PartyId = Pt.Id and CP.PartyType = 'Customer'
left join HKP.PartyAccountGroup PAG on PAG.Id = CP.PartyAccountGroupId 
left join EmployeeInformation EI on EI.SystemId = MO.ResponsiblePersonId
left join PostSalesInvoice psi on psi.SalesId = ir.Id
where Invoicestatus <> 'Closed' and EI.SystemId is not null" + CusAll;
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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
        public void GetInvoiceData(out List<InvoiceDataGetset> DataList, string ResPer, string Type, string Customer, string InvoiceNo)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<InvoiceDataGetset>();
            var CusAll = "";
            if (Type != null)
            {
                if (Type == "Export")
                {
                    Type = "Customer Export";
                }
                if (Type == "Local")
                {
                    Type = "Customer Local";
                }
                if (Type == "Both")
                {
                    Type = null;
                }
            }
            if (ResPer != null && Type == null && Customer == null && InvoiceNo == null)
            {
                CusAll = " and EI.SystemId = '" + ResPer + "'";
            }
            if (ResPer != null && Type != null && Customer == null && InvoiceNo == null)
            {
                CusAll = " and EI.SystemId = '" + ResPer + "' and PAG.StandardName = '" + Type + "'";
            }
            if (ResPer != null && Type != null && Customer != null && InvoiceNo == null)
            {
                CusAll = " and EI.SystemId = '" + ResPer + "' and PAG.StandardName = '" + Type + "' and PT.Id = '" + Customer + "'";
            }
            if (ResPer == null && Type == null && Customer == null && InvoiceNo != null)
            {
                CusAll = " and IR.InvoiceNo = '" + InvoiceNo + "'";
            }
            if (ResPer == null && Type != null && Customer == null && InvoiceNo == null)
            {
                CusAll = " and PAG.StandardName = '" + Type + "'";
            }
            if (ResPer == null && Type == null && Customer != null && InvoiceNo == null)
            {
                CusAll = " and PT.Id = '" + Customer + "'";
            }
            if (ResPer != null && Type != null && Customer != null && InvoiceNo != null)
            {
                CusAll = " and EI.SystemId = '" + ResPer + "' and PAG.StandardName = '" + Type + "' and PT.Id = '" + Customer + "' and IR.InvoiceNo = '" + InvoiceNo + "'";
            }
            if (ResPer != null && Type == null && Customer != null && InvoiceNo == null)
            {
                CusAll = " and EI.SystemId = '" + ResPer + "' and PT.Id = '" + Customer + "'";
            }
            if (ResPer != null && Type == null && Customer == null && InvoiceNo != null)
            {
                CusAll = " and EI.SystemId = '" + ResPer + "' and IR.InvoiceNo = '" + InvoiceNo + "'";
            }
            if (ResPer == null && Type != null && Customer != null && InvoiceNo == null)
            {
                CusAll = " and PAG.StandardName = '" + Type + "' and PT.Id = '" + Customer + "'";
            }

            if (ResPer == null && Type == null && Customer != null && InvoiceNo != null)
            {
                CusAll = " and PT.Id = = '" + Customer + "' and IR.InvoiceNo = '" + InvoiceNo + "'";
            }
            if (ResPer == null && Type != null && Customer == null && InvoiceNo != null)
            {
                CusAll = " and PAG.StandardName = '" + Type + "' and IR.InvoiceNo = '" + InvoiceNo + "'";
            }
            if (ResPer != null && Type == null && Customer != null && InvoiceNo != null)
            {
                CusAll = " and EI.SystemId = '" + ResPer + "' and PT.Id = '" + Customer + "' and IR.InvoiceNo = '" + InvoiceNo + "'";
            }
            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct IR.InvoiceNo , PT.UserName Customer, EI.EmployeeName ResponsiblePerson ,PAG.StandardName CustomerType
,format(psi.InvoiceDate , 'dd-MM-yyyy') InvoiceDate , format(psi.ShipmentDate , 'dd-MM-yyyy') ShipmentDate , format(psi.DocumentReceiveDate , 'dd-MM-yyyy') DocReceivedate
,format(psi.DocumentSubmissionDate , 'dd-MM-yyyy') DocSubDate , format(psi.DocAcceptanceDate , 'dd-MM-yyyy') DocAccpDate
,psi.PaymentAdviseNo PayAdbisNo , format(psi.PaymentReceivedDate , 'dd-MM-yyyy') PayResDate 
, InvoiceAmount = (select  convert(decimal(30,2) ,Sum(NetAmount)) InvoiceAmount from trn.SalesMaterial where SalesId  = IR.Id)
from trn.Sales IR
left join hkp.Party Pt on Pt.Id = PartyId
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
left join HKP.CompanyParty CP on CP.PartyId = Pt.Id and CP.PartyType = 'Customer'
left join HKP.PartyAccountGroup PAG on PAG.Id = CP.PartyAccountGroupId 
left join EmployeeInformation EI on EI.SystemId = MO.ResponsiblePersonId
left join PostSalesInvoice psi on psi.SalesId = ir.Id
where Invoicestatus <> 'Closed' and EI.SystemId is not null" + CusAll;
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new InvoiceDataGetset
                    {
                        InvoiceNo = dsRef.Tables[0].Rows[i]["InvoiceNo"].ToString(),
                        Customer = dsRef.Tables[0].Rows[i]["Customer"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        CustomerType = dsRef.Tables[0].Rows[i]["CustomerType"].ToString(),
                        InvoiceDate = dsRef.Tables[0].Rows[i]["InvoiceDate"].ToString(),
                        ShipmentDate = dsRef.Tables[0].Rows[i]["ShipmentDate"].ToString(),
                        DocReceivedate = dsRef.Tables[0].Rows[i]["DocReceivedate"].ToString(),
                        DocSubDate = dsRef.Tables[0].Rows[i]["DocSubDate"].ToString(),
                        DocAccpDate = dsRef.Tables[0].Rows[i]["DocAccpDate"].ToString(),
                        PayAdbisNo = dsRef.Tables[0].Rows[i]["PayAdbisNo"].ToString(),
                        PayResDate = dsRef.Tables[0].Rows[i]["PayResDate"].ToString(),
                        InvoiceAmount = dsRef.Tables[0].Rows[i]["InvoiceAmount"].ToString(),
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

        public void GetInvoiceRemarksData(out List<InvoiceDataEntry> DataList, string ActionById)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<InvoiceDataEntry>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select * from trn.InvoiceRemarks where ActionToBeTakenId = '" + ActionById + "' and CloseStatus <> 1  order by AddedDate desc";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new InvoiceDataEntry
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        SalesId = dsRef.Tables[0].Rows[i]["SalesId"].ToString(),
                        Status = dsRef.Tables[0].Rows[i]["Status"].ToString(),
                        ActionToBeTakenId = dsRef.Tables[0].Rows[i]["ActionToBeTakenId"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        CloseStatus = dsRef.Tables[0].Rows[i]["CloseStatus"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        UpdatedBy = dsRef.Tables[0].Rows[i]["UpdatedBy"].ToString(),
                        UpdatedDate = dsRef.Tables[0].Rows[i]["UpdatedDate"].ToString(),


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

        public void GetInvoiceRemarksDataInvoice(out List<InvoiceRemarksDataInvoice> DataList, string InvoiceNo)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<InvoiceRemarksDataInvoice>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select IRS.Id InvoiceRemarksId, IRS.Status , EIS.EmployeeCode ActionToBeTakenById,  EIS.EmployeeName ActionToBeTakenByName
,case when IRS.CloseStatus = 1 then 'Closed' else 'Active' end CloseStatus  , IRS.AddedBy , IRS.Remarks , IRS.CloseRemarks , format(IRS.AddedDate , 'dd-MM-yyyy') InvoiceRemarksADDDT
,IRS.UpdatedBy , format(IRS.UpdatedDate , 'dd-MM-yyyy') InvoiceRemarksUPPDT
,IR.InvoiceNo , PT.UserName Customer, EI.EmployeeName ResponsiblePerson ,PAG.StandardName CustomerType
,format(psi.InvoiceDate , 'dd-MM-yyyy') InvoiceDate , format(psi.ShipmentDate , 'dd-MM-yyyy') ShipmentDate , format(psi.DocumentReceiveDate , 'dd-MM-yyyy') DocReceivedate
,format(psi.DocumentSubmissionDate , 'dd-MM-yyyy') DocSubDate , format(psi.DocAcceptanceDate , 'dd-MM-yyyy') DocAccpDate
,psi.PaymentAdviseNo PayAdbisNo , format(psi.PaymentReceivedDate , 'dd-MM-yyyy') PayResDate 
, InvoiceAmount = (select  convert(decimal(30,2) ,Sum(NetAmount)) InvoiceAmount from trn.SalesMaterial where SalesId  = IR.Id)
from trn.InvoiceRemarks IRS
left join trn.Sales IR on IR.Id = IRS.SalesId
left join hkp.Party Pt on Pt.Id = PartyId
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
left join HKP.CompanyParty CP on CP.PartyId = Pt.Id and CP.PartyType = 'Customer'
left join HKP.PartyAccountGroup PAG on PAG.Id = CP.PartyAccountGroupId 
left join EmployeeInformation EI on EI.SystemId = MO.ResponsiblePersonId
left join EmployeeInformation EIS on EIS.SystemId = IRS.ActionToBeTakenId
left join PostSalesInvoice psi on psi.SalesId = ir.Id
where Invoicestatus <> 'Closed' and EI.SystemId is not null and IRS.SalesId = '" + InvoiceNo + "' order by IRS.AddedDate Desc ";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new InvoiceRemarksDataInvoice
                    {
                        InvoiceRemarksId = dsRef.Tables[0].Rows[i]["InvoiceRemarksId"].ToString(),
                        Status = dsRef.Tables[0].Rows[i]["Status"].ToString(),
                        ActionToBeTakenId = dsRef.Tables[0].Rows[i]["ActionToBeTakenById"].ToString(),
                        ActionToBeTakenByName = dsRef.Tables[0].Rows[i]["ActionToBeTakenByName"].ToString(),
                        CloseStatus = dsRef.Tables[0].Rows[i]["CloseStatus"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        CloseRemarks = dsRef.Tables[0].Rows[i]["CloseRemarks"].ToString(),
                        InvoiceRemarksADDDT = dsRef.Tables[0].Rows[i]["InvoiceRemarksADDDT"].ToString(),
                        UpdatedBy = dsRef.Tables[0].Rows[i]["UpdatedBy"].ToString(),
                        InvoiceRemarksUPPDT = dsRef.Tables[0].Rows[i]["InvoiceRemarksUPPDT"].ToString(),
                        InvoiceNo = dsRef.Tables[0].Rows[i]["InvoiceNo"].ToString(),
                        Customer = dsRef.Tables[0].Rows[i]["Customer"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        CustomerType = dsRef.Tables[0].Rows[i]["CustomerType"].ToString(),
                        InvoiceDate = dsRef.Tables[0].Rows[i]["InvoiceDate"].ToString(),
                        ShipmentDate = dsRef.Tables[0].Rows[i]["ShipmentDate"].ToString(),
                        DocReceivedate = dsRef.Tables[0].Rows[i]["DocReceivedate"].ToString(),
                        DocSubDate = dsRef.Tables[0].Rows[i]["DocSubDate"].ToString(),
                        DocAccpDate = dsRef.Tables[0].Rows[i]["DocAccpDate"].ToString(),
                        PayAdbisNo = dsRef.Tables[0].Rows[i]["PayAdbisNo"].ToString(),
                        PayResDate = dsRef.Tables[0].Rows[i]["PayResDate"].ToString(),
                        InvoiceAmount = dsRef.Tables[0].Rows[i]["InvoiceAmount"].ToString(),

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

        public string PostInvoiceRemarks(IEnumerable<InvoiceDataEntry> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.InvoiceRemarks";
                string Id = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<InvoiceDataEntry> items = DataToSave.ToList();

                foreach (InvoiceDataEntry item in DataToSave)
                {
                    Id += ",'" + item.Id + "'";
                }

                con.OpenDataSetThroughAdapter("select * from TRN.InvoiceRemarks where Id='" + items[0].Id + "'", out dsMaster, false, "1");


                foreach (InvoiceDataEntry item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.Id + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);
                        string year = DateTime.Now.ToString("yyyy");
                        dr["Id"] = year + "-" + _Id;
                        dr["SalesId"] = item.SalesId;
                        dr["Status"] = item.Status;
                        dr["ActionToBeTakenId"] = item.ActionToBeTakenId;
                        dr["Remarks"] = item.Remarks;
                        dr["CloseStatus"] = item.CloseStatus;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();



                        dsMaster.Tables[0].Rows.Add(dr);

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
        public string PostInvoiceRemarksClos(IEnumerable<InvoiceDataEntry> DataToSave, string IRId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<InvoiceDataEntry> items = DataToSave.ToList();


                con.OpenDataSetThroughAdapter("select * from TRN.InvoiceRemarks where Id='" + IRId + "'", out dsMaster, false, "1");

                foreach (InvoiceDataEntry item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        // DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["CloseRemarks"] = item.CloseRemarks;
                        dr["CloseStatus"] = item.CloseStatus;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();


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
                throw (ex);
            }
        }
        #region payment Receive
        public void GetPaymentStatus(out List<PaymentStatus> DataList, string PartyId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<PaymentStatus>();
            var CusAll = "";
            if (PartyId != null)
            {
                CusAll = "where X.PartyId = '" + PartyId + "'";
            }


            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"SELECT ISNULL( count(X.NoOfInvoice),0 )NoOfInvoice, convert(bit,0) AS isSelected
                    ,x.PartyNature,x.PartyGroup,x.PartyCategory,x.PartySubCategory,x.ResponsiblePerson,ISNULL( X.PartyId,'')PartyId,ISNULL( X.PartyCode,'')PartyCode
                    ,ISNULL( X.PartyName,'')PartyName,ISNULL( x.CurrencyCode,'')CurrencyCode

				 ,convert(decimal(30,3) ,ISNULL(SUM(X.GrossSales),0 ))GrossSales 
				,convert(decimal(30,3) ,ISNULL( SUM(X.Receipts),0 ))Receipts
                , ISNULL((SELECT convert(decimal(30,3) ,sum(VDCA.CrAmount)) -convert(decimal(30,3) ,sum(ISNULL(AW.AdvanceWriteOffBooksAmount,0))) FROM TRN.Advance A
					INNER JOIN  [TRN].[AdvanceDetail] AD ON AD.AdvanceId=A.Id
					INNER JOIN  [TRN].[VoucherDetail] VDA ON VDA.AdvanceDetailId=AD.Id
				    INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCA ON VDCA.VoucherDetailId=VDA.Id
                    LEFT JOIN (select SUM(VDCW.DrAmount)AdvanceWriteOffBooksAmount,AdvanceId from [TRN].[AdvanceWriteOffDetail] AWD
					INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.AdvanceWriteOffDetailId=AWD.Id
					INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
                    LEFT JOIN [TRN].[AdvanceWriteOff] AW ON AW.Id=AWD.AdvanceWriteOffId WHERE AW.IsPark=0 AND AW.Archive=0 GROUP BY AdvanceId)AW ON AW.AdvanceId=A.Id
					where A.PlantId='202034' and A.PartyId=X.PartyId AND A.IsPark=0 and A.SourceType='CustomerAdvance'  group by A.PartyId ),0) BooksAdvance
                , ISNULL((SELECT convert(decimal(30,3),SUM(VDC.DrAmount)) - convert(decimal(30,3),SUM(ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)))  FROM [TRN].[AdjustmentNote] A
					 INNER JOIN  [TRN].[AdjustmentNoteDetail] AD ON AD.AdjustmentNoteId=A.Id
                     INNER JOIN  [TRN].[VoucherDetail] VDA ON VDA.AdjustmentNoteDetailId=AD.Id
                     INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VDA.Id
					 LEFT JOIN (select SUM(ISNULL(VDCW.CrAmount,0))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
										INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
										INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
										 INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
										where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
										GROUP BY  IWD.AdjustmentNoteId)W ON W.AdjustmentNoteId=AD.AdjustmentNoteId
					where A.PlantId='202034' and A.PartyId=X.PartyId and VDA.PartyType='Customer' and A.SourceType='DebitNote'  AND A.IsPark=0  group by A.PartyId ),0) DebitNote 
                , ISNULL((SELECT convert(decimal(30,3),SUM(VDC.CrAmount)) - convert(decimal(30,3),SUM(ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)))  FROM [TRN].[AdjustmentNote] A
					 INNER JOIN  [TRN].[AdjustmentNoteDetail] AD ON AD.AdjustmentNoteId=A.Id
                     INNER JOIN  [TRN].[VoucherDetail] VDA ON VDA.AdjustmentNoteDetailId=AD.Id
                     INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VDA.Id
					 LEFT JOIN (select SUM(ISNULL(VDCW.DrAmount,0))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
										INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
										INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
										 INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
										where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
										GROUP BY  IWD.AdjustmentNoteId)W ON W.AdjustmentNoteId=AD.AdjustmentNoteId
					where A.PlantId='202034' and A.PartyId=X.PartyId and VDA.PartyType='Customer' and A.SourceType='CreditNote'  AND A.IsPark=0  group by A.PartyId ),0) CreditNote 
                ,convert(decimal(30,3),ISNULL( SUM(X.Balance),0)) Balance
                ,convert(decimal(30,3),ISNULL( SUM(X.Balance),0)) 
                -ISNULL((SELECT convert(decimal(30,3),sum(VDCA.CrAmount)) -convert(decimal(30,3),sum(ISNULL(AW.AdvanceWriteOffBooksAmount,0))) FROM TRN.Advance A
					INNER JOIN  [TRN].[AdvanceDetail] AD ON AD.AdvanceId=A.Id
					INNER JOIN  [TRN].[VoucherDetail] VDA ON VDA.AdvanceDetailId=AD.Id
				    INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCA ON VDCA.VoucherDetailId=VDA.Id
                    LEFT JOIN (select SUM(VDCW.DrAmount)AdvanceWriteOffBooksAmount,AdvanceId from [TRN].[AdvanceWriteOffDetail] AWD
					INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.AdvanceWriteOffDetailId=AWD.Id
					INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
                    LEFT JOIN [TRN].[AdvanceWriteOff] AW ON AW.Id=AWD.AdvanceWriteOffId WHERE AW.IsPark=0 AND AW.Archive=0 GROUP BY AdvanceId)AW ON AW.AdvanceId=A.Id
					where A.PlantId='202034' and A.PartyId=X.PartyId AND A.IsPark=0 and A.SourceType='CustomerAdvance'  group by A.PartyId ),0) 
                -ISNULL((SELECT convert(decimal(30,3),SUM(VDC.CrAmount)) - convert(decimal(30,3),SUM(ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)))  FROM [TRN].[AdjustmentNote] A
					 INNER JOIN  [TRN].[AdjustmentNoteDetail] AD ON AD.AdjustmentNoteId=A.Id
                     INNER JOIN  [TRN].[VoucherDetail] VDA ON VDA.AdjustmentNoteDetailId=AD.Id
                     INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VDA.Id
					 LEFT JOIN (select convert(decimal(30,3),SUM(ISNULL(VDCW.DrAmount,0)))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
										INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
										INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
										 INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
										where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
										GROUP BY  IWD.AdjustmentNoteId)W ON W.AdjustmentNoteId=AD.AdjustmentNoteId
					where A.PlantId='202034' and A.PartyId=X.PartyId and VDA.PartyType='Customer' and A.SourceType='CreditNote'  AND A.IsPark=0  group by A.PartyId ),0) NetBalance
				 ,convert(decimal(30,3),ISNULL(SUM(X.ActualBalance),0)) -ISNULL((SELECT convert(decimal(30,3),sum(VDCA.CrAmount)) -convert(decimal(30,3),sum(ISNULL(AW.AdvanceWriteOffBooksAmount,0))) FROM TRN.Advance A
					INNER JOIN  [TRN].[AdvanceDetail] AD ON AD.AdvanceId=A.Id
					INNER JOIN  [TRN].[VoucherDetail] VDA ON VDA.AdvanceDetailId=AD.Id
				    INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCA ON VDCA.VoucherDetailId=VDA.Id
                    LEFT JOIN (select convert(decimal(30,3),SUM(VDCW.DrAmount))AdvanceWriteOffBooksAmount,AdvanceId from [TRN].[AdvanceWriteOffDetail] AWD
					INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.AdvanceWriteOffDetailId=AWD.Id
					INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
                    LEFT JOIN [TRN].[AdvanceWriteOff] AW ON AW.Id=AWD.AdvanceWriteOffId WHERE AW.IsPark=0 AND AW.Archive=0 GROUP BY AdvanceId)AW ON AW.AdvanceId=A.Id
					where A.PlantId='202034' and A.PartyId=X.PartyId AND A.IsPark=0 and A.SourceType='CustomerAdvance'  group by A.PartyId ),0) 
                -ISNULL((SELECT convert(decimal(30,3),SUM(VDC.CrAmount)) - convert(decimal(30,3),SUM(ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)))  FROM [TRN].[AdjustmentNote] A
					 INNER JOIN  [TRN].[AdjustmentNoteDetail] AD ON AD.AdjustmentNoteId=A.Id
                     INNER JOIN  [TRN].[VoucherDetail] VDA ON VDA.AdjustmentNoteDetailId=AD.Id
                     INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VDA.Id
					 LEFT JOIN (select convert(decimal(30,3),SUM(ISNULL(VDCW.DrAmount,0)))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
										INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
										INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
										 INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
										where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
										GROUP BY  IWD.AdjustmentNoteId)W ON W.AdjustmentNoteId=AD.AdjustmentNoteId
					where A.PlantId='202034' and A.PartyId=X.PartyId and VDA.PartyType='Customer' and A.SourceType='CreditNote'  AND A.IsPark=0  group by A.PartyId ),0) ActualBalance
                 ,ISNULL((SELECT  round(convert(decimal(30,3),SUM(ISNULL(CC.CompanyCurrencyDrAmount, 0))) - convert(decimal(30,3),SUM(ISNULL(CC.CompanyCurrencyCrAmount, 0))),2) AS LedgerBalanceAmount
                    FROM [TRN].[VoucherDetail] AS VD
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                    FROM [TRN].[VoucherDetailCurrency] AS VDC
	                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' 
                    ) AS CC ON CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.PlantId='202034' AND convert(Date,V.PostingDate) <= convert(date, getdate()) AND VD.PartyId=X.PartyId AND VD.PartyType IN ('Customer') 
					GROUP BY VD.PartyId),0) LedgerBalanceAmount
                ,CASE WHEN (SELECT COUNT(V.Id)NoOfPendingPostWriteOff
                    FROM [TRN].[VoucherDetail] AS VD
		            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
		            WHERE  V.IsPark=1  AND VD.PartyType='Customer'  AND V.PlantId='202034' AND VD.PartyId=X.PartyId
		            AND V.SourceType in ('CreditNoteSetOff','CustomerAdvanceWriteOff','CustomerBanksReceipt','CustomerReceipt','DebitNoteSetOff','ReceiptByBank','VendorAdvanceWriteOff','VendorPayment')
		            AND  convert(Date,V.PostingDate) <= convert(date, getdate()) 
		            GROUP BY VD.PartyId)>0 THEN 'Yes' ELSE '' END WriteOffPendingPost
                ,convert(decimal(30,3),ISNULL( SUM(X.BooksGrossSales) ,0))BooksGrossSales
				,convert(decimal(30,3),ISNULL( SUM(X.BooksReceipts) ,0))BooksReceipts
				,convert(decimal(30,3),SUM(X.BooksBalance)) BooksBalance

				,convert(decimal(30,3),ISNULL( sum(X.ODueMoreThan30) ,0)) OverDueMoreThan30
				,convert(decimal(30,3),ISNULL( sum(X.ODueMoreThan15) ,0)) OverDueMoreThan15
				,convert(decimal(30,3),ISNULL( sum(X.ODueLessThan15) ,0)) OverDueLessThan15
				,convert(decimal(30,3),ISNULL( sum(X.TodayBalance),0)) TodayBalance
				,convert(decimal(30,3),ISNULL( sum(X.OneToSevenBalance) ,0)) OneToSevenBalance
				,convert(decimal(30,3),ISNULL( sum(X.EightToThirtyBalance),0)) EightToThirtyBalance
				,convert(decimal(30,3),ISNULL( sum(X.ThirtyToSixtyBalance) ,0)) ThirtyToSixtyBalance
				,convert(decimal(30,3),ISNULL( sum(X.Onword60),0)) Onword60
				,CASE WHEN (SELECT COUNT(Id) FROM HKP.CompanyParty WHERE PartyId=X.PartyId AND  PartyType='Vendor')>0 THEN 'Yes' ELSE 'No' END IsVendor
				
				
                FROM (
                SELECT ISNULL( IV.PartyId,'') NoOfInvoice,P.PartyNature,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,E.EmployeeName ResponsiblePerson,ISNULL( IV.PartyId,'')PartyId
				,ISNULL( P.Code,'') PartyCode,ISNULL( P.UserName,'') PartyName ,ISNULL( c.Code,'') CurrencyCode
                , ISNULL(IVD.InvoiceBooksAmount,0) AS GrossSales
				, ISNULL(IVD.SetOffBooksAmount,0) AS Receipts
				, ISNULL(IVD.InvoiceBooksAmount,0)-ISNULL(IVD.SetOffBooksAmount,0) AS Balance
				, ISNULL(IVD.InvoiceBooksAmount,0)-ISNULL(IV.WrittenOffAmount*IV.CompanyCurrencyRate,0) AS ActualBalance
                , ISNULL(IVD.InvoiceBooksAmount,0) AS BooksGrossSales
				, ISNULL(IVD.SetOffBooksAmount,0) AS BooksReceipts
				, ISNULL(IVD.InvoiceBooksAmount,0)-ISNULL(IVD.SetOffBooksAmount,0) AS BooksBalance
                , ISNULL(OM30.ODueMoreThan30 *IV.CompanyCurrencyRate,0) ODueMoreThan30
                , ISNULL(OM15.ODueMoreThan15*IV.CompanyCurrencyRate ,0) ODueMoreThan15
                , ISNULL(OV.OverDdueBalance*IV.CompanyCurrencyRate ,0) ODueLessThan15
				, ISNULL(TB.TodayBalance *IV.CompanyCurrencyRate ,0) TodayBalance
				, ISNULL(OTS.OneToSevenBalance*IV.CompanyCurrencyRate,0) OneToSevenBalance
				, ISNULL(ETT.EightToThirtyBalance*IV.CompanyCurrencyRate,0) EightToThirtyBalance
				, ISNULL(TTS.ThirtyToSixtyBalance*IV.CompanyCurrencyRate,0) ThirtyToSixtyBalance
				, ISNULL(O60.Onword60*IV.CompanyCurrencyRate,0) Onword60
                , ISNULL(IVD.InvoiceBooksAmount,0) AS GrossTranAmount
				, ISNULL(IVD.InvoiceBooksAmount*IV.CompanyCurrencyRate,0) AS GrossAmount
                FROM [TRN].[Invoice] AS IV 
                 JOIN (select IDE.InvoiceId,VD.PartyId,SUM(VDC.DrAmount) InvoiceBooksAmount ,SUM(IwV.SetOffBooksAmount) SetOffBooksAmount
						FROM  [TRN].[InvoiceDetail] IDE
						LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IDE.Id
						LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
						LEFT JOIN [TRN].[Voucher] AS VI ON VI.Id=VD.VoucherId
						LEFT JOIN (SELECT iwd.InvoiceDetailId,iw.PartyId
                            ,SUM(VDC.CrAmount) SetOffBooksAmount
							FROM  [TRN].[InvoiceWriteOffDetail] iwd 
							JOIN TRN.InvoiceWriteOff iw on iw.Id=iwd.InvoiceWriteOffId 
							LEFT JOIN TRN.VoucherDetail VD ON VD.InvoiceWriteOffDetailId=iwd.Id
							LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
							 JOIN TRN.Voucher WV ON WV.Id=VD.VoucherId
							WHERE WV.IsPark=0 AND ( convert(Date,WV.PostingDate) <= convert(date, getdate()) )
							GROUP BY iwd.InvoiceDetailId,iw.PartyId
							)AS IwV ON IwV.InvoiceDetailId=IDE.Id AND VD.PartyId=IwV.PartyId
						WHERE VI.IsPark=0 and VD.PartyType='Customer' --AND VD.PartyId='202017395'
						GROUP BY IDE.InvoiceId,VD.PartyId
				 ) AS IVD ON IVD.InvoiceId=IV.Id AND IVD.PartyId=IV.PartyId
                 
                LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                LEFT JOIN dbo.EmployeeInformation AS E ON E.SystemID=P.ResponsiblePersonId
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=IV.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS ODueMoreThan30 FROM TRN.Invoice I 
							WHERE DATEDIFF(DAY, GETDATE(),I.ActualDueDate)<-30 
							and I.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales') 
                            and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0 AND I.IsWrittenOff=0 AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) OM30 ON OM30.Id=IV.Id
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS ODueMoreThan15 FROM TRN.Invoice I 
							WHERE DATEDIFF(DAY, GETDATE(),I.ActualDueDate)<-15 and DATEDIFF(DAY, GETDATE(),I.ActualDueDate)>=-30
							and I.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales') 
                             and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0 AND I.IsWrittenOff=0 AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) OM15 ON OM15.Id=IV.Id
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS OverDdueBalance FROM TRN.Invoice I 
							WHERE DATEDIFF(DAY, GETDATE(),I.ActualDueDate)<0 and DATEDIFF(DAY, GETDATE(),I.ActualDueDate)>=-15
							and I.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales') 
                            and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0 AND I.IsWrittenOff=0 AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) OV ON OV.Id=IV.Id
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS TodayBalance FROM TRN.Invoice I 
							WHERE DATEDIFF(DAY, GETDATE(),I.ActualDueDate)=0 and I.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales') 
                            and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0 AND I.IsWrittenOff=0 AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) TB ON TB.Id=IV.Id
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS OneToSevenBalance FROM TRN.Invoice I 
							WHERE DATEDIFF(DAY, GETDATE(),I.ActualDueDate)>0 and DATEDIFF(DAY, GETDATE(),I.ActualDueDate)<=7 
							and I.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales') 
                             and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0 AND I.IsWrittenOff=0 AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) OTS ON OTS.Id=IV.Id
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS EightToThirtyBalance FROM TRN.Invoice I 
							WHERE DATEDIFF(DAY, GETDATE(),I.ActualDueDate)>7 and DATEDIFF(DAY, GETDATE(),I.ActualDueDate)<=30 
							and I.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales') 
                             and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0 AND I.IsWrittenOff=0 AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) ETT ON ETT.Id=IV.Id

								LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS ThirtyToSixtyBalance FROM TRN.Invoice I 
							WHERE DATEDIFF(DAY, GETDATE(),I.ActualDueDate)>30 and DATEDIFF(DAY, GETDATE(),I.ActualDueDate)<=60
							and I.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales') 
                            and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0 AND I.IsWrittenOff=0 AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) TTS ON TTS.Id=IV.Id



				 LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS Onword60 FROM TRN.Invoice I 
							WHERE DATEDIFF(DAY, GETDATE(),I.ActualDueDate)>60 and 
							I.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales') 
                             and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0  AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) O60 ON O60.Id=IV.Id
                
                WHERE  V.IsPark=0  AND IV.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales')
                and  IV.CompanyGroupId='CG20181'   AND IV.CompanyId='C20201' AND IV.PlantId='202034' AND ( convert(Date,IV.PostingDate) <= convert(date, getdate()) )
                and ISNULL(IVD.InvoiceBooksAmount,0)-ISNULL(IVD.SetOffBooksAmount,0)>0
                
                union all
				 SELECT ISNULL( IV.PartyId,'') NoOfInvoice,P.PartyNature,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,E.EmployeeName ResponsiblePerson,ISNULL( IV.PartyId,'')PartyId
				,ISNULL( P.Code,'') PartyCode,ISNULL( P.UserName,'') PartyName,ISNULL( c.Code,'') CurrencyCode
                ,ISNULL(IVD.Amount,0) AS GrossSales
				,ISNULL(IVD.WrittenOffAmount ,0) AS Receipts
				, ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0)-ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0) AS Balance
                , ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0)-ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0) AS ActualBalance
                 ,ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS BooksGrossSales
				,ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0) AS BooksReceipts
				, ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0)-ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0) AS BooksBalance
                , ISNULL(OM30.ODueMoreThan30 *CC.CompanyCurrencyRate,0) ODueMoreThan30
                , ISNULL(OM15.ODueMoreThan15*CC.CompanyCurrencyRate ,0) ODueMoreThan15
                , ISNULL(OV.OverDdueBalance*CC.CompanyCurrencyRate ,0) ODueLessThan15
				, ISNULL(TB.TodayBalance *cc.CompanyCurrencyRate ,0) TodayBalance
				, ISNULL(OTS.OneToSevenBalance*cc.CompanyCurrencyRate,0) OneToSevenBalance
				, ISNULL(ETT.EightToThirtyBalance*cc.CompanyCurrencyRate,0) EightToThirtyBalance
				, ISNULL(TTS.ThirtyToSixtyBalance*cc.CompanyCurrencyRate,0) ThirtyToSixtyBalance
				, ISNULL(O60.Onword60*cc.CompanyCurrencyRate,0) Onword60
				, ISNULL(IVD.Amount,0) AS GrossTranAmount
				, ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS GrossAmount
                FROM [TRN].[AdjustmentNoteDetail] AS IVD
                LEFT JOIN [TRN].[AdjustmentNote] AS IV ON IVD.AdjustmentNoteId=IV.Id
                LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                LEFT JOIN dbo.EmployeeInformation AS E ON E.SystemID=P.ResponsiblePersonId
                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                LEFT JOIN (select SUM(ISNULL(VDCW.CrAmount,0))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
										INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
										INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
										 INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
										where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
										GROUP BY  IWD.AdjustmentNoteId)W ON W.AdjustmentNoteId=IVD.AdjustmentNoteId
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS ODueMoreThan30 FROM TRN.AdjustmentNote I 
							WHERE DATEDIFF(DAY, GETDATE(),I.PostingDate)<-30 
							and I.SourceType in ('DebitNote','CustomerReceipt') AND I.PartyType='Customer'
                            and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0 AND I.IsWrittenOff=0 AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) OM30 ON OM30.Id=IV.Id
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS ODueMoreThan15 FROM TRN.AdjustmentNote I 
							WHERE DATEDIFF(DAY, GETDATE(),I.PostingDate)<-15 and DATEDIFF(DAY, GETDATE(),I.PostingDate)>=-30
							and I.SourceType in ('DebitNote','CustomerReceipt') AND I.PartyType='Customer'
                             and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0 AND I.IsWrittenOff=0 AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) OM15 ON OM15.Id=IV.Id
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS OverDdueBalance FROM TRN.AdjustmentNote I 
							WHERE DATEDIFF(DAY, GETDATE(),I.PostingDate)<0 and DATEDIFF(DAY, GETDATE(),I.PostingDate)>=-15
							and I.SourceType in ('DebitNote','CustomerReceipt') AND I.PartyType='Customer'
                             and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0  AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) OV ON OV.Id=IV.Id

				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS TodayBalance FROM TRN.AdjustmentNote I 
							WHERE DATEDIFF(DAY, GETDATE(),I.PostingDate)=0 and I.SourceType in ('DebitNote','CustomerReceipt') AND I.PartyType='Customer'
                             and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0  AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) TB ON TB.Id=IV.Id
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS OneToSevenBalance FROM TRN.AdjustmentNote I 
							WHERE DATEDIFF(DAY, GETDATE(),I.PostingDate)>0 and DATEDIFF(DAY, GETDATE(),I.PostingDate)<=7 
							and I.SourceType in ('DebitNote','CustomerReceipt') AND I.PartyType='Customer'
                            and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0  AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) OTS ON OTS.Id=IV.Id
				LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS EightToThirtyBalance FROM TRN.AdjustmentNote I 
							WHERE DATEDIFF(DAY, GETDATE(),I.PostingDate)>7 and DATEDIFF(DAY, GETDATE(),I.PostingDate)<=30 
							and I.SourceType in ('DebitNote','CustomerReceipt') AND I.PartyType='Customer'
                            and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0  AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) ETT ON ETT.Id=IV.Id

								LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS ThirtyToSixtyBalance FROM TRN.AdjustmentNote I 
							WHERE DATEDIFF(DAY, GETDATE(),I.PostingDate)>30 and DATEDIFF(DAY, GETDATE(),I.PostingDate)<=60
							and I.SourceType in ('DebitNote','CustomerReceipt') AND I.PartyType='Customer'
                             and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034'  AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0  AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) TTS ON TTS.Id=IV.Id

				 LEFT JOIN (SELECT Id,SUM(ISNULL(I.Amount - I.WrittenOffAmount,0)) AS Onword60 FROM TRN.AdjustmentNote I 
							WHERE DATEDIFF(DAY, GETDATE(),I.PostingDate)>60 and 
							I.SourceType in ('DebitNote','CustomerReceipt') AND I.PartyType='Customer'
                            and  I.CompanyGroupId='CG20181'   AND I.CompanyId='C20201' AND I.PlantId='202034' AND ( convert(Date,I.PostingDate) <= convert(date, getdate()) ) and I.Archive=0  AND I.IsWrittenOff=0 AND i.IsPark=0
                            group by Id) O60 ON O60.Id=IV.Id
                LEFT JOIN (
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='C20201'
                ) AS CC ON CC.VoucherDetailId=VD.Id
                
                WHERE IV.Archive=0 AND V.IsPark=0  AND IV.PartyType='Customer' AND IV.SourceType in ('DebitNote','CustomerReceipt')
                    AND ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0)-ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)>0
                 and  IV.CompanyGroupId='CG20181'   AND IV.CompanyId='C20201' AND IV.PlantId='202034' AND ( convert(Date,IV.PostingDate) <= convert(date, getdate()) )
				)
				X " + CusAll + "" +
                @"GROUP BY x.PartyNature,x.PartyGroup,x.PartyCategory,x.PartySubCategory,x.ResponsiblePerson,PartyId,PartyName,PartyCode,CurrencyCode
                order by x.PartyNature,x.PartyGroup,x.PartyName ";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new PaymentStatus
                    {
                        NoOfInvoice = dsRef.Tables[0].Rows[i]["NoOfInvoice"].ToString(),
                        isSelected = dsRef.Tables[0].Rows[i]["isSelected"].ToString(),
                        PartyNature = dsRef.Tables[0].Rows[i]["PartyNature"].ToString(),
                        PartyGroup = dsRef.Tables[0].Rows[i]["PartyGroup"].ToString(),
                        PartyCategory = dsRef.Tables[0].Rows[i]["PartyCategory"].ToString(),
                        PartySubCategory = dsRef.Tables[0].Rows[i]["PartySubCategory"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        PartyId = dsRef.Tables[0].Rows[i]["PartyId"].ToString(),
                        PartyCode = dsRef.Tables[0].Rows[i]["PartyCode"].ToString(),
                        PartyName = dsRef.Tables[0].Rows[i]["PartyName"].ToString(),
                        CurrencyCode = dsRef.Tables[0].Rows[i]["CurrencyCode"].ToString(),
                        GrossSales = dsRef.Tables[0].Rows[i]["GrossSales"].ToString(),
                        Receipts = dsRef.Tables[0].Rows[i]["Receipts"].ToString(),
                        BooksAdvance = dsRef.Tables[0].Rows[i]["BooksAdvance"].ToString(),
                        DebitNote = dsRef.Tables[0].Rows[i]["DebitNote"].ToString(),
                        CreditNote = dsRef.Tables[0].Rows[i]["CreditNote"].ToString(),
                        Balance = dsRef.Tables[0].Rows[i]["Balance"].ToString(),
                        NetBalance = dsRef.Tables[0].Rows[i]["NetBalance"].ToString(),
                        ActualBalance = dsRef.Tables[0].Rows[i]["ActualBalance"].ToString(),
                        LedgerBalanceAmount = dsRef.Tables[0].Rows[i]["LedgerBalanceAmount"].ToString(),
                        WriteOffPendingPost = dsRef.Tables[0].Rows[i]["WriteOffPendingPost"].ToString(),
                        BooksGrossSales = dsRef.Tables[0].Rows[i]["BooksGrossSales"].ToString(),
                        BooksReceipts = dsRef.Tables[0].Rows[i]["BooksReceipts"].ToString(),
                        BooksBalance = dsRef.Tables[0].Rows[i]["BooksBalance"].ToString(),
                        OverDueMoreThan30 = dsRef.Tables[0].Rows[i]["OverDueMoreThan30"].ToString(),
                        OverDueMoreThan15 = dsRef.Tables[0].Rows[i]["OverDueMoreThan15"].ToString(),
                        OverDueLessThan15 = dsRef.Tables[0].Rows[i]["OverDueLessThan15"].ToString(),
                        TodayBalance = dsRef.Tables[0].Rows[i]["TodayBalance"].ToString(),
                        OneToSevenBalance = dsRef.Tables[0].Rows[i]["OneToSevenBalance"].ToString(),
                        EightToThirtyBalance = dsRef.Tables[0].Rows[i]["EightToThirtyBalance"].ToString(),
                        ThirtyToSixtyBalance = dsRef.Tables[0].Rows[i]["ThirtyToSixtyBalance"].ToString(),
                        Onword60 = dsRef.Tables[0].Rows[i]["Onword60"].ToString(),
                        IsVendor = dsRef.Tables[0].Rows[i]["IsVendor"].ToString(),
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
        public void GetPaymentstatusInvoiceWise(out List<INvoiceWiseAccount> DataList, string PartyId, string RespId, string CustomerType)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<INvoiceWiseAccount>();
            var CusAll = "";
            if (PartyId != null)
            {
                CusAll = "AND IV.PartyId in(" + PartyId + @")";
            }
            if (PartyId == null)
            {
                if (RespId != null && CustomerType == null)
                {
                    CusAll = "and EI.SystemId = '" + RespId + @"'";
                }
                if (RespId == null && CustomerType != null)
                {
                    if (CustomerType != null)
                    {
                        if (CustomerType == "Export")
                        {
                            CusAll = " and PAG.StandardName = 'Customer Export'";
                        }
                        if (CustomerType == "Local")
                        {
                            CusAll = " and PAG.StandardName = 'Customer Local'";
                        }
                        if (CustomerType == "Both")
                        {
                            CusAll = null;
                        }
                    }
                }
                if (RespId != null && CustomerType != null)
                {
                    if (CustomerType == "Export")
                    {
                        CusAll = " and PAG.StandardName = 'Customer Export' and EI.SystemId = '" + RespId + @"'";
                    }
                    if (CustomerType == "Local")
                    {
                        CusAll = " and PAG.StandardName = 'Customer Local' and EI.SystemId = '" + RespId + @"'";
                    }
                    if (CustomerType == "Both")
                    {
                        CusAll = "and EI.SystemId = '" + RespId + @"'";
                    }
                }

            }
            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select x.* from (

                    SELECT   P.PartyNature,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,E.EmployeeName ResponsiblePerson,EN.UserName Entity,IV.PartyType,IV.PartyId, IV.PartyPlantId,p.code PartyCode, P.UserName PartyName, PP.UserName AS PartyPlantName
										,V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate,V.DocRefNo InvoiceNo
										, replace (convert(varchar(11),iv.DocDate, 106),'', '-')as DocDate,iv.DocDate  SortDocDate, C.Code CurrencyCode,IV.BaseNoOfDays
										, REPLACE(CONVERT(VARCHAR(11), IV.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
										, REPLACE(CONVERT(VARCHAR(11), IV.ActualDueDate, 106), ' ', '-') AS ActualDueDate
										,Days=DATEDIFF(DAY,IV.DocDate, GETDATE())
										,AgingInvoice= case 
														when DATEDIFF(DAY, GETDATE(),Iv.ActualDueDate)<-30  OR IV.ActualDueDate IS NULL then 'OverDueMoreThan30'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<-15 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-30  OR IV.ActualDueDate IS NULL then 'OverDueMoreThan15'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-15  OR IV.ActualDueDate IS NULL then 'OverDueLessThan15'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then 'Today'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=7 then '1-7'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>7 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=30 then '8-30'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>30 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=60 then '31-60'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>60 then '60 Onword'
														end
										,AgingSorting= case 
														when DATEDIFF(DAY, GETDATE(),Iv.ActualDueDate)<-30  OR IV.ActualDueDate IS NULL then '1.OverDueMoreThan30'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<-15 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-30  OR IV.ActualDueDate IS NULL then '2.OverDueMoreThan15'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-15  OR IV.ActualDueDate IS NULL then '3.OverDueLessThan15'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then '4.Today'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=7 then '5.1-7'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>7 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=30 then '6.8-30'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>30 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=60 then '7.31-60'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>60 then '8.60 Onward'
														end
										,Convert(decimal(30,3),ISNULL(IVD.InvoiceBooksAmount,0)) AS GrossSales,0 DebitNoteAmount,0 TaxAmount,
                                         TrnReceipt=Convert(decimal(30,3),ISNULL(IVD.SetOffBooksAmount, 0)) 
										 ,Convert(decimal(30,3),ISNULL(IVD.InvoiceBooksAmount,0))-Convert(decimal(30,3),ISNULL(IVD.SetOffBooksAmount,0)) AS TrnBalance
										 ,Convert(decimal(30,3),ISNULL(IVD.InvoiceBooksAmount ,0)) AS BooksGrossSales
										,0  BooksDebitNoteAmount,0  BooksTaxAmount,
                                         BooksReceipt=Convert(decimal(30,3),ISNULL(IVD.SetOffBooksAmount, 0) )
										 ,Convert(decimal(30,3),ISNULL(IVD.InvoiceBooksAmount,0))-Convert(decimal(30,3),ISNULL(IVD.SetOffBooksAmount,0)) AS BooksBalance
										 ,case when ss.Id is null then V.DocRefNo else ss.Id end INVS, AddRemarks = 'AddRemarks'
										 ,EI.EmployeeName Resp
										 ,PAG.StandardName CustomerType
                                        FROM  [TRN].[Invoice] AS IV 
										 JOIN (select IDE.InvoiceId,VD.PartyId,SUM(VDC.DrAmount) InvoiceBooksAmount ,SUM(IwV.SetOffBooksAmount) SetOffBooksAmount
											FROM  [TRN].[InvoiceDetail] IDE
											LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IDE.Id
											LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
											LEFT JOIN [TRN].[Voucher] AS VI ON VI.Id=VD.VoucherId
											LEFT JOIN (SELECT iwd.InvoiceDetailId,iw.PartyId
												,SUM(VDC.CrAmount) SetOffBooksAmount
												FROM  [TRN].[InvoiceWriteOffDetail] iwd 
												JOIN TRN.InvoiceWriteOff iw on iw.Id=iwd.InvoiceWriteOffId 
												LEFT JOIN TRN.VoucherDetail VD ON VD.InvoiceWriteOffDetailId=iwd.Id
												LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
													JOIN TRN.Voucher WV ON WV.Id=VD.VoucherId
												WHERE WV.IsPark=0 AND ( convert(Date,WV.PostingDate) <= convert(date, getdate()) )
												GROUP BY iwd.InvoiceDetailId,iw.PartyId
												)AS IwV ON IwV.InvoiceDetailId=IDE.Id AND VD.PartyId=IwV.PartyId
											WHERE VI.IsPark=0 and VD.PartyType='Customer'
											GROUP BY IDE.InvoiceId,VD.PartyId
										) AS IVD ON IVD.InvoiceId=IV.Id AND IVD.PartyId=IV.PartyId
									    LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                                        LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                                        LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                                        LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                                        LEFT JOIN dbo.EmployeeInformation AS E ON E.SystemID=P.ResponsiblePersonId
									    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=IV.VoucherId
										left join trn.Sales ss on ss.VoucherId  = V.Id
										LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = ss.Id
										LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
										LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
										left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
										left join EmployeeInformation EI on EI.SystemId = MO.ResponsiblePersonId
										
										left join HKP.CompanyParty CP on CP.PartyId = P.Id and CP.PartyType = 'Customer'
										left join HKP.PartyAccountGroup PAG on PAG.Id = CP.PartyAccountGroupId 
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                                        WHERE IV.Archive=0 AND V.IsPark=0  AND IV.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales')
										AND ISNULL(IVD.InvoiceBooksAmount,0)-ISNULL(IVD.SetOffBooksAmount,0)>0
                                        AND IV.CompanyGroupId='CG20181' AND IV.CompanyId='C20201' AND IV.PlantId='202034' AND ( convert(Date,IV.PostingDate) <= convert(date, getdate()) )
                                      " + CusAll + @"

								    UNION ALL
                                    SELECT   P.PartyNature,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,E.EmployeeName ResponsiblePerson,EN.UserName Entity,IV.PartyType,IV.PartyId, IV.PartyPlantId,p.code PartyCode, P.UserName PartyName, PP.UserName AS PartyPlantName
										,V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate,V.DocRefNo InvoiceNo
										,replace (convert(varchar(11),iv.DocDate, 106),'', '-')as DocDate ,iv.DocDate  SortDocDate,C.Code CurrencyCode
										,'' BaseNoOfDays, '' BaseOnDueDate, REPLACE(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') AS ActualDueDate
										,Days=DATEDIFF(DAY,IV.DocDate, GETDATE())
										
												   	,AgingInvoice= case 
														when DATEDIFF(DAY, GETDATE(),Iv.PostingDate)<-30  OR IV.PostingDate IS NULL then 'OverDueMoreThan30'
														when DATEDIFF(DAY, GETDATE(),IV.PostingDate)<-15 and DATEDIFF(DAY, GETDATE(),IV.PostingDate)>=-30  OR IV.PostingDate IS NULL then 'OverDueMoreThan15'
														when DATEDIFF(DAY, GETDATE(),IV.PostingDate)<0 and DATEDIFF(DAY, GETDATE(),IV.PostingDate)>=-15  OR IV.PostingDate IS NULL then 'OverDueLessThan15'
															when DATEDIFF(DAY, GETDATE(),IV.PostingDate)=0 then 'Today'
															when DATEDIFF(DAY, GETDATE(),IV.PostingDate)>0 and DATEDIFF(DAY, GETDATE(),IV.PostingDate)<=7 then '1-7'
															when DATEDIFF(DAY, GETDATE(),IV.PostingDate)>7 and DATEDIFF(DAY, GETDATE(),IV.PostingDate)<=30 then '8-30'
															when DATEDIFF(DAY, GETDATE(),IV.PostingDate)>30 and DATEDIFF(DAY, GETDATE(),IV.PostingDate)<=60 then '31-60'
															when DATEDIFF(DAY, GETDATE(),IV.PostingDate)>60 then '60 Onword'
															end
										,AgingSorting = case 
														when DATEDIFF(DAY, GETDATE(),Iv.PostingDate)<-30  OR IV.PostingDate IS NULL then '1.OverDueMoreThan30'
														when DATEDIFF(DAY, GETDATE(),IV.PostingDate)<-15 and DATEDIFF(DAY, GETDATE(),IV.PostingDate)>=-30  OR IV.PostingDate IS NULL then '2.OverDueMoreThan15'
														when DATEDIFF(DAY, GETDATE(),IV.PostingDate)<0 and DATEDIFF(DAY, GETDATE(),IV.PostingDate)>=-15  OR IV.PostingDate IS NULL then '3.OverDueLessThan15'

															when DATEDIFF(DAY, GETDATE(),IV.PostingDate)=0 then '4.Today'
															when DATEDIFF(DAY, GETDATE(),IV.PostingDate)>0 and DATEDIFF(DAY, GETDATE(),IV.PostingDate)<=7 then '5.1-7'
															when DATEDIFF(DAY, GETDATE(),IV.PostingDate)>7 and DATEDIFF(DAY, GETDATE(),IV.PostingDate)<=30 then '6.8-30'
															when DATEDIFF(DAY, GETDATE(),IV.PostingDate)>30 and DATEDIFF(DAY, GETDATE(),IV.PostingDate)<=60 then '7.31-60'
															when DATEDIFF(DAY, GETDATE(),IV.PostingDate)>60 then '8.60 Onword'
															end
										 ,Convert(decimal(30,3),ISNULL(IVD.Amount,0)) AS GrossSales,0 DebitNoteAmount ,0 TaxAmount
                                         ,TrnReceipt=Convert(decimal(30,3),ISNULL(IVD.WrittenOffAmount, 0) )
										 ,Convert(decimal(30,3),ISNULL(IVD.Amount,0))-Convert(decimal(30,3),ISNULL(IVD.WrittenOffAmount,0)) AS TrnBalance
										 ,Convert(decimal(30,3),ISNULL(IVD.Amount *CC.CompanyCurrencyRate,0)) AS BooksGrossSales,0  BooksDebitNoteAmount,0  BooksTaxAmount,
                                         BooksReceipt=Convert(decimal(30,3),ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)) 
										 ,Convert(decimal(30,3),ISNULL(IVD.Amount * CC.CompanyCurrencyRate,0)) - Convert(decimal(30,3),ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)) AS BooksBalance
										 ,case when ss.Id is null then V.DocRefNo else ss.Id end INVS, AddRemarks = 'AddRemarks'
										 ,EI.EmployeeName Resp
										 ,PAG.StandardName CustomerType
                                        FROM [TRN].[AdjustmentNoteDetail] AS IVD
										LEFT JOIN [TRN].[AdjustmentNote] AS IV ON IVD.AdjustmentNoteId=IV.Id
										LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                                        LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                                        LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                                        LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                                        LEFT JOIN dbo.EmployeeInformation AS E ON E.SystemID=P.ResponsiblePersonId
										LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
										LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=IVD.Id
										LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
										left join trn.Sales ss on ss.VoucherId  = V.Id
										LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = ss.Id
										LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
										LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
										left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
										left join EmployeeInformation EI on EI.SystemId = MO.ResponsiblePersonId
										
										left join HKP.CompanyParty CP on CP.PartyId = P.Id and CP.PartyType = 'Customer'
										left join HKP.PartyAccountGroup PAG on PAG.Id = CP.PartyAccountGroupId 
										LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
										LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
										LEFT JOIN (select SUM(ISNULL(VDCW.CrAmount,0))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
												INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
												INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
												INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
												where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
												GROUP BY  IWD.AdjustmentNoteId)W ON W.AdjustmentNoteId=IVD.AdjustmentNoteId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='C20201'
									) AS CC ON CC.VoucherDetailId=VD.Id
									
                                        WHERE IV.Archive=0 AND V.IsPark=0  AND IV.PartyType='Customer' AND IV.SourceType in ('DebitNote','CustomerReceipt')
										AND ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0)-ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)>0
                                        AND IV.CompanyGroupId='CG20181' AND IV.CompanyId='C20201' AND IV.PlantId='202034' AND ( convert(Date,IV.PostingDate) <= convert(date, getdate()) )
                                       " + CusAll + @"
									

										) x
										order by x.PartyNature,x.PartyGroup,x.PartyName asc";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new INvoiceWiseAccount
                    {
                        PartyNature = dsRef.Tables[0].Rows[i]["PartyNature"].ToString(),
                        PartyGroup = dsRef.Tables[0].Rows[i]["PartyGroup"].ToString(),
                        PartyCategory = dsRef.Tables[0].Rows[i]["PartyCategory"].ToString(),
                        PartySubCategory = dsRef.Tables[0].Rows[i]["PartySubCategory"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        Entity = dsRef.Tables[0].Rows[i]["Entity"].ToString(),
                        PartyType = dsRef.Tables[0].Rows[i]["PartyType"].ToString(),
                        PartyId = dsRef.Tables[0].Rows[i]["PartyId"].ToString(),
                        PartyPlantId = dsRef.Tables[0].Rows[i]["PartyPlantId"].ToString(),
                        PartyCode = dsRef.Tables[0].Rows[i]["PartyCode"].ToString(),
                        PartyName = dsRef.Tables[0].Rows[i]["PartyName"].ToString(),
                        PartyPlantName = dsRef.Tables[0].Rows[i]["PartyPlantName"].ToString(),
                        VoucherNo = dsRef.Tables[0].Rows[i]["VoucherNo"].ToString(),
                        PostingDate = dsRef.Tables[0].Rows[i]["PostingDate"].ToString(),
                        InvoiceNo = dsRef.Tables[0].Rows[i]["InvoiceNo"].ToString(),
                        DocDate = dsRef.Tables[0].Rows[i]["DocDate"].ToString(),
                        SortDocDate = dsRef.Tables[0].Rows[i]["SortDocDate"].ToString(),
                        CurrencyCode = dsRef.Tables[0].Rows[i]["CurrencyCode"].ToString(),
                        BaseNoOfDays = dsRef.Tables[0].Rows[i]["BaseNoOfDays"].ToString(),
                        BaseOnDueDate = dsRef.Tables[0].Rows[i]["BaseOnDueDate"].ToString(),
                        ActualDueDate = dsRef.Tables[0].Rows[i]["ActualDueDate"].ToString(),
                        Days = dsRef.Tables[0].Rows[i]["Days"].ToString(),
                        AgingInvoice = dsRef.Tables[0].Rows[i]["AgingInvoice"].ToString(),
                        AgingSorting = dsRef.Tables[0].Rows[i]["AgingSorting"].ToString(),
                        GrossSales = dsRef.Tables[0].Rows[i]["GrossSales"].ToString(),
                        DebitNoteAmount = dsRef.Tables[0].Rows[i]["DebitNoteAmount"].ToString(),
                        TaxAmount = dsRef.Tables[0].Rows[i]["TaxAmount"].ToString(),
                        TrnReceipt = dsRef.Tables[0].Rows[i]["TrnReceipt"].ToString(),
                        TrnBalance = dsRef.Tables[0].Rows[i]["TrnBalance"].ToString(),
                        BooksGrossSales = dsRef.Tables[0].Rows[i]["BooksGrossSales"].ToString(),
                        BooksDebitNoteAmount = dsRef.Tables[0].Rows[i]["BooksDebitNoteAmount"].ToString(),
                        BooksTaxAmount = dsRef.Tables[0].Rows[i]["BooksTaxAmount"].ToString(),
                        BooksReceipt = dsRef.Tables[0].Rows[i]["BooksReceipt"].ToString(),
                        BooksBalance = dsRef.Tables[0].Rows[i]["BooksBalance"].ToString(),
                        INVS = dsRef.Tables[0].Rows[i]["INVS"].ToString(),
                        AddRemarks = dsRef.Tables[0].Rows[i]["AddRemarks"].ToString(),
                        Resp = dsRef.Tables[0].Rows[i]["Resp"].ToString(),
                        CustomerType = dsRef.Tables[0].Rows[i]["CustomerType"].ToString(),

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
        #endregion payment Receive
        #region Quality Action 
        public void GetQualityControll(out List<QualityControll> DataList, string FromDate, string ToDate, string ResponsiblePersonId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<QualityControll>();
            string FilterDate = string.Empty;
            string ResponsiblePerson = string.Empty;
            System.Data.DataSet dsRef;
            if (FromDate != null && ToDate != null && FromDate != "undefined" && ToDate != "undefined")
            {
                FilterDate = " and convert(Date,QCD.AddedDate) between '" + FromDate + "' and '" + ToDate + "'";
            }

            if (ResponsiblePersonId != null && ResponsiblePersonId != "undefined")
            {
                ResponsiblePerson = " and ResponsiblePersonId = '" + ResponsiblePersonId + "'";
            }
            try
            {
                strSQL = @"select distinct QC.Id as HeaderId,format(QC.AddedDate,'dd-MMM-yyyy') as Date,DATEDIFF(Hour,QC.AddedDate,GETDATE()) PendingTime,E.Id EntityId,E.UserName Entity,P.Id ProcessId,P.UserName Process,
QC.IssueId,QMM.UserName Issue,EI.SystemId CheckedById,EI.EmployeeName CheckedBy,QC.ProductionOrderId PONo,QC.LotNumber,
Article=STUFF((select distinct ','+MA.StandardName from trn.ProductionOrderDetail Pod 
left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId=so.Id
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
where Pod.ProductionOrderId=QC.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PS.UserName POStatus from TRN.QualityControlDetails QCD
left join TRN.QualityControl QC on QC.Id=QCD.QCId
left join ORG.Entity E on E.Id=QC.EntityId
left join hkp.Process P on P.Id=QC.ProcessId
left join MST.QualityManagementMaster QMM on QMM.Id=QC.IssueId
left join EmployeeInformation EI on EI.SystemId=QC.ProductionInchargeId
left join TRN.ProductionOrder PO on PO.Id=QC.ProductionOrderId
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
where QCD.Status not in ('Close','Complete') and PS.UserName in ('Running','To Close') and QCD.GradeId in (select Id from MST.QualityGradeDetails where ActionApplicable=1) " + FilterDate + @" " + ResponsiblePerson + @" order by DATEDIFF(Hour,QC.AddedDate,GETDATE()) desc";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new QualityControll
                    {
                        HeaderId = dsRef.Tables[0].Rows[i]["HeaderId"].ToString(),
                        Date = dsRef.Tables[0].Rows[i]["Date"].ToString(),
                        PendingTime = dsRef.Tables[0].Rows[i]["PendingTime"].ToString(),
                        EntityId = dsRef.Tables[0].Rows[i]["EntityId"].ToString(),
                        Entity = dsRef.Tables[0].Rows[i]["Entity"].ToString(),
                        ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                        Process = dsRef.Tables[0].Rows[i]["Process"].ToString(),
                        IssueId = dsRef.Tables[0].Rows[i]["IssueId"].ToString(),
                        Issue = dsRef.Tables[0].Rows[i]["Issue"].ToString(),
                        CheckedById = dsRef.Tables[0].Rows[i]["CheckedById"].ToString(),
                        CheckedBy = dsRef.Tables[0].Rows[i]["CheckedBy"].ToString(),
                        PONo = dsRef.Tables[0].Rows[i]["PONo"].ToString(),
                        LotNumber = dsRef.Tables[0].Rows[i]["LotNumber"].ToString(),
                        Article = dsRef.Tables[0].Rows[i]["Article"].ToString(),
                        POStatus = dsRef.Tables[0].Rows[i]["POStatus"].ToString(),


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

        public void GetQualityActionUpdateParameter(out List<QualityControllUpdate> DataList, string HeaderId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<QualityControllUpdate>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select QCD.Id ParameterId,PM.UserName Parameter,QCD.Status,UOM.UserName UOM,QCD.Value,QMP.Max,QMP.Min,WC.UserName WorkCenter,QGD.GradeName,
QAD.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,QCD.Remarks,QCD.ItemId,format(QCD.AddedDate,'dd-MMM-yyyy') as AddedDate,format(QCD.AddedDate,'hh:mm tt') as AddedTime  from TRN.QualityControlDetails QCD
left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
left join hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId
left join SCS.WorkCenterMaster WC on WC.Id=QCD.WorkCenterId
left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
left join MST.QualityActionToBeTakenDetails QAD on QAD.Id=QCD.ActionToBeTaken
left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
where QCD.Status not in ('Close','Complete') and QCD.GradeId in (select Id from MST.QualityGradeDetails where ActionApplicable=1)
and QCD.QCId='" + HeaderId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new QualityControllUpdate
                    {
                        ParameterId = dsRef.Tables[0].Rows[i]["ParameterId"].ToString(),
                        Parameter = dsRef.Tables[0].Rows[i]["Parameter"].ToString(),
                        Status = dsRef.Tables[0].Rows[i]["Status"].ToString(),
                        UOM = dsRef.Tables[0].Rows[i]["UOM"].ToString(),
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Max = dsRef.Tables[0].Rows[i]["Max"].ToString(),
                        Min = dsRef.Tables[0].Rows[i]["Min"].ToString(),
                        WorkCenter = dsRef.Tables[0].Rows[i]["WorkCenter"].ToString(),
                        GradeName = dsRef.Tables[0].Rows[i]["GradeName"].ToString(),
                        ActionToBeTakenName = dsRef.Tables[0].Rows[i]["ActionToBeTakenName"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        ItemId = dsRef.Tables[0].Rows[i]["ItemId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        AddedTime = dsRef.Tables[0].Rows[i]["AddedTime"].ToString(),


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

        public void GetEmployeeQualityUpdate(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select distinct EI.SystemId Value,CONCAT( EI.EmployeeCode, '    ' ,EI.EmployeeName) Name
from 
TRN.QualityControlDetails QCD
left join dbo.EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
where EI.EmployeeStatus='Active' and EI.EmployeeCode is not null and QCD.Status='Inprogress' and QCD.ResponsiblePersonId is not null";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        public string PostQualityActionUpdate(IEnumerable<QualityActionUpdate> DataToSave, string PId, string Status)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "[TRN].[QualityActionTakenUpdate]";
                string Id = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<QualityActionUpdate> items = DataToSave.ToList();

                foreach (QualityActionUpdate item in DataToSave)
                {
                    Id += ",'" + item.Id + "'";
                }

                con.OpenDataSetThroughAdapter("select * from [TRN].[QualityActionTakenUpdate] where Id='" + items[0].Id + "' and ParameterId='" + PId + "'", out dsMaster, false, "1");


                foreach (QualityActionUpdate item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.Id + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);

                        dr["Id"] = "QATM" + _Id;
                        dr["SNO"] = item.SNO;
                        dr["ActionTaken"] = item.ActionTaken;
                        dr["ActionById"] = item.ActionById;
                        dr["Remarks"] = item.Remarks;
                        dr["ParameterId"] = item.ParameterId;
                        dr["ReasonName"] = item.ReasonName;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();



                        dsMaster.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["ActionTaken"] = item.ActionTaken;
                        dr["ActionById"] = item.ActionById;
                        dr["Remarks"] = item.Remarks;
                        dr["ParameterId"] = item.ParameterId;
                        dr["ReasonName"] = item.ReasonName;


                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedFromIP"] = "192.168.137.44";
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    }

                }
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("Update TRN.QualityControlDetails set Status='" + Status + "' where Id='" + PId + @"'");
                conC.CommitTransaction();

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = "QATM";

                return MasterId;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public void GetQualityActionUpdate(out List<QualityActionUpdate> DataList, string ParameterId, string SNO)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<QualityActionUpdate>();

            System.Data.DataSet dsRef;
            try
            {
                if (SNO != null)
                {
                    strSQL = @"select * from [TRN].[QualityActionTakenUpdate] where ParameterId = '" + ParameterId + "' and SNO = '" + SNO + "'";
                }
                else
                {
                    strSQL = @"select * from [TRN].[QualityActionTakenUpdate] where ParameterId = '" + ParameterId + "'";
                }
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new QualityActionUpdate
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        SNO = dsRef.Tables[0].Rows[i]["SNO"].ToString(),
                        ReasonId = dsRef.Tables[0].Rows[i]["ReasonId"].ToString(),
                        ActionTaken = dsRef.Tables[0].Rows[i]["ActionTaken"].ToString(),
                        ActionById = dsRef.Tables[0].Rows[i]["ActionById"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        ParameterId = dsRef.Tables[0].Rows[i]["ParameterId"].ToString(),
                        ReasonName = dsRef.Tables[0].Rows[i]["ReasonName"].ToString(),
                        ConfirmRemarks = dsRef.Tables[0].Rows[i]["ConfirmRemarks"].ToString(),
                        AddedBy = dsRef.Tables[0].Rows[i]["AddedBy"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),


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

        public void GetQualityConfirmControll(out List<QualityControll> DataList, string ResponsiblePersonId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<QualityControll>();
            string FilterDate = string.Empty;
            string ResponsiblePerson = string.Empty;
            System.Data.DataSet dsRef;

            if (ResponsiblePersonId != null && ResponsiblePersonId != "undefined")
            {
                ResponsiblePerson = " and ResponsiblePersonId = '" + ResponsiblePersonId + "'";
            }
            try
            {
                strSQL = @"select distinct QC.Id as HeaderId,format(QC.AddedDate,'dd-MMM-yyyy') as Date,DATEDIFF(Hour,QC.AddedDate,GETDATE()) PendingTime,E.Id EntityId,E.UserName Entity,P.Id ProcessId,P.UserName Process,
QC.IssueId,QMM.UserName Issue,EI.SystemId CheckedById,EI.EmployeeName CheckedBy,QC.ProductionOrderId PONo,QC.LotNumber,
Article=STUFF((select distinct ','+MA.StandardName from trn.ProductionOrderDetail Pod 
left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId=so.Id
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
where Pod.ProductionOrderId=QC.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PS.UserName POStatus from TRN.QualityControlDetails QCD
left join TRN.QualityControl QC on QC.Id=QCD.QCId
left join ORG.Entity E on E.Id=QC.EntityId
left join hkp.Process P on P.Id=QC.ProcessId
left join MST.QualityManagementMaster QMM on QMM.Id=QC.IssueId
left join EmployeeInformation EI on EI.SystemId=QC.ProductionInchargeId
left join TRN.ProductionOrder PO on PO.Id=QC.ProductionOrderId
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
where QCD.Status in ('Close') and PS.UserName in ('Running','To Close') and QCD.GradeId in (select Id from MST.QualityGradeDetails where ActionApplicable=1) " + ResponsiblePerson + @" order by DATEDIFF(Hour,QC.AddedDate,GETDATE()) desc";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new QualityControll
                    {
                        HeaderId = dsRef.Tables[0].Rows[i]["HeaderId"].ToString(),
                        Date = dsRef.Tables[0].Rows[i]["Date"].ToString(),
                        PendingTime = dsRef.Tables[0].Rows[i]["PendingTime"].ToString(),
                        EntityId = dsRef.Tables[0].Rows[i]["EntityId"].ToString(),
                        Entity = dsRef.Tables[0].Rows[i]["Entity"].ToString(),
                        ProcessId = dsRef.Tables[0].Rows[i]["ProcessId"].ToString(),
                        Process = dsRef.Tables[0].Rows[i]["Process"].ToString(),
                        IssueId = dsRef.Tables[0].Rows[i]["IssueId"].ToString(),
                        Issue = dsRef.Tables[0].Rows[i]["Issue"].ToString(),
                        CheckedById = dsRef.Tables[0].Rows[i]["CheckedById"].ToString(),
                        CheckedBy = dsRef.Tables[0].Rows[i]["CheckedBy"].ToString(),
                        PONo = dsRef.Tables[0].Rows[i]["PONo"].ToString(),
                        LotNumber = dsRef.Tables[0].Rows[i]["LotNumber"].ToString(),
                        Article = dsRef.Tables[0].Rows[i]["Article"].ToString(),
                        POStatus = dsRef.Tables[0].Rows[i]["POStatus"].ToString(),


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

        public void GetQualityConfirmActionUpdateParameter(out List<QualityControllUpdate> DataList, string HeaderId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<QualityControllUpdate>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select QCD.Id ParameterId,PM.UserName Parameter,QCD.Status,UOM.UserName UOM,QCD.Value,QMP.Max,QMP.Min,WC.UserName WorkCenter,QGD.GradeName,
QAD.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,QCD.Remarks,QCD.ItemId,format(QCD.AddedDate,'dd-MMM-yyyy') as AddedDate,format(QCD.AddedDate,'hh:mm tt') as AddedTime  from TRN.QualityControlDetails QCD
left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
left join hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId
left join SCS.WorkCenterMaster WC on WC.Id=QCD.WorkCenterId
left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
left join MST.QualityActionToBeTakenDetails QAD on QAD.Id=QCD.ActionToBeTaken
left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
where QCD.Status in ('Close') and QCD.GradeId in (select Id from MST.QualityGradeDetails where ActionApplicable=1)
and QCD.QCId='" + HeaderId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new QualityControllUpdate
                    {
                        ParameterId = dsRef.Tables[0].Rows[i]["ParameterId"].ToString(),
                        Parameter = dsRef.Tables[0].Rows[i]["Parameter"].ToString(),
                        Status = dsRef.Tables[0].Rows[i]["Status"].ToString(),
                        UOM = dsRef.Tables[0].Rows[i]["UOM"].ToString(),
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Max = dsRef.Tables[0].Rows[i]["Max"].ToString(),
                        Min = dsRef.Tables[0].Rows[i]["Min"].ToString(),
                        WorkCenter = dsRef.Tables[0].Rows[i]["WorkCenter"].ToString(),
                        GradeName = dsRef.Tables[0].Rows[i]["GradeName"].ToString(),
                        ActionToBeTakenName = dsRef.Tables[0].Rows[i]["ActionToBeTakenName"].ToString(),
                        ResponsiblePerson = dsRef.Tables[0].Rows[i]["ResponsiblePerson"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        ItemId = dsRef.Tables[0].Rows[i]["ItemId"].ToString(),
                        AddedDate = dsRef.Tables[0].Rows[i]["AddedDate"].ToString(),
                        AddedTime = dsRef.Tables[0].Rows[i]["AddedTime"].ToString(),


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
        public void GetQualityConfirmActionUpdate(out List<QualityActionUpdate> DataList, string ParameterId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<QualityActionUpdate>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"select QAT.Id,isnull(QAT.SNO,QPR.SNO) SNO,QPR.Id ReasonId,isnull(QRM.UserName,QAT.ReasonName) ReasonName,QAT.ActionTaken,QAT.ActionById,EI.EmployeeName ActionBy,QAT.Remarks, 'OK' Saved 
,  QAT.ParameterId , null ConfirmRemarks 
from [TRN].[QualityActionTakenUpdate]  QAT
left join [MST].[QualityManagementParameterReason] QPR on QPR.Id=QAT.ReasonId and QPR.IsActive=1
left join [HKP].[QualityManagementReasonMaster] QRM on QRM.Id=QPR.ReasonId
left join EmployeeInformation EI on EI.SystemId=QAT.ActionById
where QAT.ParameterId='" + ParameterId + "'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new QualityActionUpdate
                    {
                        Id = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        SNO = dsRef.Tables[0].Rows[i]["SNO"].ToString(),
                        ReasonId = dsRef.Tables[0].Rows[i]["ReasonId"].ToString(),
                        ActionTaken = dsRef.Tables[0].Rows[i]["ActionTaken"].ToString(),
                        ActionById = dsRef.Tables[0].Rows[i]["ActionById"].ToString(),
                        ActionBy = dsRef.Tables[0].Rows[i]["ActionBy"].ToString(),
                        Remarks = dsRef.Tables[0].Rows[i]["Remarks"].ToString(),
                        ParameterId = dsRef.Tables[0].Rows[i]["ParameterId"].ToString(),
                        ReasonName = dsRef.Tables[0].Rows[i]["ReasonName"].ToString(),
                        ConfirmRemarks = dsRef.Tables[0].Rows[i]["ConfirmRemarks"].ToString(),
                        Saved = dsRef.Tables[0].Rows[i]["Saved"].ToString(),


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
        public string PostQualityConfirmationUpdate(IEnumerable<QualityActionUpdate> DataToSave, string PId, string Status, string ConfirmationRemarks, string ConfirmBy)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "[TRN].[QualityActionTakenUpdate]";
                string Id = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<QualityActionUpdate> items = DataToSave.ToList();

                foreach (QualityActionUpdate item in DataToSave)
                {
                    Id += ",'" + item.Id + "'";
                }

                con.OpenDataSetThroughAdapter("select * from [TRN].[QualityActionTakenUpdate] where Id='" + items[0].Id + "' and ParameterId='" + PId + "'", out dsMaster, false, "1");


                foreach (QualityActionUpdate item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.Id + "' ";
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        // DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["ConfirmRemarks"] = item.ConfirmRemarks;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedFromIP"] = "192.168.137.44";
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();


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
        public string PostQualityConfirmationMasterUpdate(IEnumerable<QualityConfirmssControllMaster> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string Id = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<QualityConfirmssControllMaster> items = DataToSave.ToList();

                foreach (QualityConfirmssControllMaster item in DataToSave)
                {
                    Id += ",'" + item.PID + "'";
                }

                con.OpenDataSetThroughAdapter("select * from [TRN].[QualityControlDetails] where Id='" + items[0].PID + "'", out dsMaster, false, "1");


                foreach (QualityConfirmssControllMaster item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.PID + "' ";
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        // DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["Status"] = item.Status;
                        dr["ConfirmBy"] = item.ConfirmBy;
                        dr["ConfirmationRemarks"] = item.ConfirmationRemarks;


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

        #endregion Quality Action 
        #region UtilityMaster
        public void GetUtilityTransectionDetail(out List<UtilityMasterGet> DataList, string UtilityMasterId)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<UtilityMasterGet>();

            System.Data.DataSet dsRef;
            try
            {

                strSQL = @"Select TOP(1)* from (select LastReading=(select LastReading=(select top(1) Reading from UtilityTransaction Where UtilityMasterId ='" + UtilityMasterId + @"' order by Date desc))
									, LastReadingDate=(select top(1) FORMAT([Date],'dd-MMM-yyyy') from UtilityTransaction Where UtilityMasterId='" + UtilityMasterId + @"' order by Date desc) + ' ' +
                                     (select top(1) CONVERT(varchar(5),[AddedDate],108) from UtilityTransaction Where UtilityMasterId = '" + UtilityMasterId + @"' order by Date desc)
                                    , MultiplyingFactor = (select top(1)  MultiplyingFactor from UtilityMaster where Id = '" + UtilityMasterId + @"' order by Date desc)
									,UtilityMasterId = (Select distinct Id from UtilityMaster where Id = '" + UtilityMasterId + @"' )
									,UtilityMaster = (Select distinct UserName from UtilityMaster where Id = '" + UtilityMasterId + @"' )
                                    ,UoMId = (Select distinct UoMId from UtilityMaster where Id = '" + UtilityMasterId + @"' )
                                    from UtilityTransaction
                                    Where UtilityMasterId='" + UtilityMasterId + @"')A";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new UtilityMasterGet
                    {
                        LastReading = dsRef.Tables[0].Rows[i]["LastReading"].ToString(),
                        LastReadingDate = dsRef.Tables[0].Rows[i]["LastReadingDate"].ToString(),
                        MultiplyingFactor = dsRef.Tables[0].Rows[i]["MultiplyingFactor"].ToString(),
                        UtilityMaster = dsRef.Tables[0].Rows[i]["UtilityMaster"].ToString(),
                        UtilityMasterId = dsRef.Tables[0].Rows[i]["UtilityMasterId"].ToString(),
                        UoMId = dsRef.Tables[0].Rows[i]["UoMId"].ToString(),

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

        public string PostUtilityTransection(IEnumerable<UtilityMasterGet> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "UtilityTransaction";
                string Id = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<UtilityMasterGet> items = DataToSave.ToList();

                foreach (UtilityMasterGet item in DataToSave)
                {
                    Id += ",'" + item.Id + "'";
                }

                con.OpenDataSetThroughAdapter("select * from UtilityTransaction where Id='" + items[0].Id + "'", out dsMaster, false, "1");


                foreach (UtilityMasterGet item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id='" + item.Id + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);

                        dr["Id"] = "20" + _Id;
                        dr["Date"] = item.Date;
                        dr["UtilityMasterId"] = item.UtilityMasterId;
                        dr["Reading"] = item.Reading;
                        dr["Remarks"] = item.Remarks;
                        dr["Quantity"] = item.Quantity;
                        dr["LastReading"] = item.LastReading;
                        dr["LastReadingDate"] = item.LastReadingDate;
                        dr["LastReadingTime"] = item.LastReadingTime;
                        dr["MultiplyingFactor"] = item.MultiplyingFactor;
                        dr["UoMId"] = item.UoMId;
                        dr["IsAppEntry"] = true;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedDate"] = System.DateTime.Now.ToString();



                        dsMaster.Tables[0].Rows.Add(dr);

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

        public void GetUtilityMasterList(out List<Default2> DataList)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select Id Value , UserName Name from UtilityMaster  where Active = 1  order by UserName";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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
        #endregion UtilityMaster

        #region Production Entry
        public void GetDateFilter(out List<Default2> DataList , string Time)
        {
            clsConnectionManager objCon = null;
            string strSQL = "";
            DataList = new List<Default2>();

            System.Data.DataSet dsRef;
            try
            {
                strSQL = @"select left(Time,5) Name , Date Value from trn.datefilter  where Time like '" + Time + "%'";
                objCon = new clsConnectionManager();
                objCon.BeginTransaction();
                objCon.getDataSet(strSQL, out dsRef);
                objCon.CommitTransaction();
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new Default2
                    {
                        Value = dsRef.Tables[0].Rows[i]["Value"].ToString(),
                        Name = dsRef.Tables[0].Rows[i]["Name"].ToString(),

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

        /*public List<OpenHeadModelNew> Calculate(IEnumerable<OpenHeadModelNew> OpenHeadNew)
        {
            DataTable dtValue = new DataTable();
            dtValue.TableName = "TempTable";
            dtValue.Columns.Add("ProductionBookingParameterId");
            dtValue.Columns.Add("Amount");
            string sFormulaResult = null;

            DataSet dsOpenHead = Library.Service.Helpers.DataTableExtensions.ToDataSet<OpenHeadModelNew>(OpenHeadNew);
            for (int i = 0; i < dsOpenHead.Tables[0].Rows.Count; i++)
            {
                if (i == 0)
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["ProductionBookingParameterId"] = dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }
                else if (i > 0 && string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["ProductionBookingParameterId"] = dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }

                if (!string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    ReLoadFormulaWithValue(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString(), ref dtValue, out string _formulaValue);
                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("#####");

                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["ProductionBookingParameterId"] = dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString().Trim();

                    if (sFormulaResult == "" || sFormulaResult == "∞")
                    {
                        dtValueRow["Amount"] = 0;
                    }
                    else
                    {
                        dtValueRow["Amount"] = sFormulaResult;
                    }

                    dtValue.Rows.Add(dtValueRow);

                    DataView dv = new DataView(dsOpenHead.Tables[0]);
                    dv.RowFilter = "ProductionBookingParameterId='" + dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString() + "'";
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();
                        if (sFormulaResult == "" || sFormulaResult == "∞" || sFormulaResult == "NaN")
                        {
                            drmo["Value"] = 0;
                        }
                        else
                        {
                            drmo["Value"] = sFormulaResult;
                        }
                        drmo.EndEdit();

                    }


                }
           

            }


            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dsOpenHead.Tables[0]);
           // return Json(new { NewData, Message = AplosMessage.Success });
        }*/

        public void ReLoadFormulaWithValue(string strFormulaID, ref DataTable dtValue, out string lblFormulaValue)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;

            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = strFormulaID.Trim();

                lblFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        dvLocal = new DataView();
                        dvLocal.Table = dtValue;

                        dvLocal.RowFilter = "ProductionBookingParameterId = '" + strTemp.Trim() + "'";
                        if (dvLocal.Count > 0)
                        {
                            if (dvLocal[0]["Amount"].ToString().Trim() == "")
                            {
                                strTemp = "0";
                            }
                            else
                            {
                                strTemp = dvLocal[0]["Amount"].ToString().Trim();
                            }
                        }
                    }

                    lblFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }
        #endregion Production Entry
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

    public class PODetail
    {
        public string POId { get; set; }
        public string StandardName { get; set; }
        public string BookingLevel { get; set; }
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

    public class ProcessService
    {
        public string Id { get; set; }
        public string ProductionDate { get; set; }
        public string Entity { get; set; }
        public string EntityID { get; set; }
        public string Process { get; set; }
        public string ProcessID { get; set; }
        public string shift { get; set; }
        public string ShiftID { get; set; }
        public string ResponsiblePerson { get; set; }
        public string ResponsiblePersonID { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class ProcessServiceChild
    {
        public string Id { get; set; }
        public string ProductionServiceId { get; set; }
        public string WorkcenterMasterId { get; set; }
        public string PO { get; set; }
        public string Value { get; set; }
        public string Remarks { get; set; }
        public string Detention1 { get; set; }
        public string Detention1Time { get; set; }
        public string Detention2 { get; set; }
        public string Detention2Time { get; set; }
        public string Detention3 { get; set; }
        public string Detention3Time { get; set; }
        public string Detention4 { get; set; }
        public string Detention4Time { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }


    public class ProcessServiceParameter
    {
        public string Id { get; set; }
        public string ProductionServiceId { get; set; }
        public string StandardName { get; set; }
        public string Production100 { get; set; }
        public string Efficiency { get; set; }
        public string Speed { get; set; }
        public string ProductionShouldBe { get; set; }
        public string TPI { get; set; }
        public string NoOfSpindle { get; set; }
        public string MachineHank { get; set; }
        public string Wrapping { get; set; }
        public string ProductionActual { get; set; }
        public string DetentionInMin { get; set; }
        public string ActualEfficiency { get; set; }
        public string Utilization { get; set; }
        public string AllottedManpower { get; set; }
        public string ProdCapacityPerManpower { get; set; }
        public string WorkingHours { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
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

    public class POWiseReport
    {
        public string ProcessIndex { get; set; }
        public string EntityId { get; set; }
        public string Entity { get; set; }
        public string CustomerId { get; set; }
        public string Customer { get; set; }
        public string Article { get; set; }
        public string SONo { get; set; }
        public string PONo { get; set; }
        public string POStatusId { get; set; }
        public string POStatus { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string SOQty { get; set; }
        public string BaseProcPlanPercentage { get; set; }
        public string ActualPlanScheduleQty { get; set; }
        public string ShouldBeBaseProcessPlannedQty { get; set; }
        public string BaseProcessProduceQty { get; set; }
        public string BaseProcessRemainingQty { get; set; }
        public string Sequence { get; set; }
        public string ProcessId { get; set; }
        public string Process { get; set; }
        public string PercentQty { get; set; }
        public string ProcessPlannedQty { get; set; }
        public string ProcProdQty { get; set; }
        public string PreProcProdQty { get; set; }
        public string WIP { get; set; }
        public string ProcBalanceToProduce { get; set; }
        public string RelayProcess { get; set; }
        public string IsBaseProcess { get; set; }
        public string ProcessLegDays { get; set; }
        public string POFirstDelivery { get; set; }
        public string POLastDelivery { get; set; }
        public string BaseProcProdStartDate { get; set; }
        public string BaseProcLatestProdDate { get; set; }
        public string BaseProcPlanStartDate { get; set; }
        public string BaseProcPlanCompletionDate { get; set; }
        public string POStartDate { get; set; }
        public string POCompletionDate { get; set; }
        public string FirstProcessActualBookDate { get; set; }
        public string POFirstProdBookDate { get; set; }
        public string POLatestProdBookDate { get; set; }
        public string ShouldBeProcessStartDate { get; set; }
        public string ShouldBeProcessEndDate { get; set; }
        public string ProcessFirstBookDate { get; set; }
        public string ProcessLatestBookDate { get; set; }
        public string ProcessStartDays { get; set; }
        public string ProcessEndDays { get; set; }
        public string ProcessPlanPercent { get; set; }
        public string ProcessStatus { get; set; }
        public string FirstProcessWC { get; set; }
        public string ProcLossPercent { get; set; }
        public string ProcLossQty { get; set; }
        public string BaseProcProdPerenct { get; set; }
        public string ProcProdPercent { get; set; }
        public string EntryCheck { get; set; }
        public string ProceessProdQtyVsSOQty { get; set; }
        public string ProcessStatusRemark { get; set; }
        public string POReviewStatus { get; set; }
        public string LotNoQty { get; set; }
        public string InputRecoveryPercentage { get; set; }
        public string ActualInputPlanPercentage { get; set; }
        public string LatestProcessProdBookDays { get; set; }
        public string ProcessReviewStatus { get; set; }
        public string ProcessBalanceProd { get; set; }
    }


    public class Plantcontrol
    {
        public string Id { get; set; }
        public string EmployeeCode { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string InandOut { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
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

    public class Default2
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class Default3
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
        public string SystemId { get; set; } = "";
    }

    public class Weight
    {
        public string CartonQty { get; set; } = "";
        public string BookedQty { get; set; } = "";
    }

    #region Attendance
    public class AttendanceReport
    {
        public string SrNo { get; set; }
        public string LeaveCode { get; set; }
        public string SystemID { get; set; }
        public string EMPCode { get; set; }
        public string EmployeeName { get; set; }
        public string Section { get; set; }
        public string SubSection { get; set; }
        public string Designation { get; set; }
        public string Category { get; set; }
        public string Activity { get; set; }
        public string InStatus { get; set; }
        public string InTime { get; set; }
        public string InVerificationTime { get; set; }
        public string BudgetCode { get; set; }
        public string Shift { get; set; }
        public string ShiftId { get; set; }
        public string MobileNo { get; set; }
        public string WeeklyStatus { get; set; }
        public string Residence { get; set; }
        public string Transport { get; set; }
        public string ManpowerBudgetId { get; set; }
        public string UserGroup { get; set; }
        public string GroupId { get; set; }
        public string Location { get; set; }
        public string CurrentStatus { get; set; }
        public string Deployment { get; set; }
        public string ToDayIN { get; set; }
        public string Diffenence { get; set; }
        public string RawDayStatus { get; set; }
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string DifferenceColor { get; set; }
        public string LateInTime { get; set; }
        public string LateInStatus { get; set; }

    }

    public class Locations
    {
        public string Location { get; set; } = "";
    }

    public class EmpInformation
    {
        public string SystemID { get; set; } = "";
        public string EMPCode { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Section { get; set; } = "";
        public string SubSection { get; set; } = "";
        public string Designation { get; set; } = "";
        public string BudgetCode { get; set; } = "";
        public string Shift { get; set; } = "";
        public string DOJ { get; set; } = "";
        public string EmpType { get; set; } = "";
    }
    #endregion Attendance

    #region Aman c
    public class Userinfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string BloodGroup { get; set; }
        public string Status { get; set; }
    }

    public class Receiver
    {
        public string Id { get; set; }
        public string DonorName { get; set; }
        public string PhoneNumber { get; set; }
        public string BloodGroup { get; set; }
        public string Address { get; set; }
    }


    public class Ambulance
    {
        public string Id { get; set; }
        public string CompanyName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Price { get; set; }
    }

    #endregion Aman c

    #region TempBudgetCode
    public class TempBudgetCode
    {
        public string Id { get; set; }
        public string EmpSystemId { get; set; }
        public string ExistingBudgetId { get; set; }
        public string NewBudgetId { get; set; }
        public string WorkDate { get; set; }
        public string Remarks { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
    }
    #endregion TempBudgetCode

    // Barcode scan data 
    public class BarcodeScan
    {
        public string Id { get; set; }
        public string LocMasterId { get; set; }
        public string SubLocation { get; set; }
        public string ProductCode { get; set; }
        public string POId { get; set; }
        public string LotNo { get; set; }
        public string RefNo { get; set; }
        public string Cones { get; set; }
        public string NetWeight { get; set; }
        public string GWeight { get; set; }
        public string PackedBy { get; set; }
        public string Shade { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
    }


    #region vehicle
    public class Vehicle
    {
        public string Id { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public string PersonalOfficial { get; set; }
        public string PurposeId { get; set; }
        public string Name { get; set; }
        public string EmpSystemId { get; set; }
        public string NumberOfPassengers { get; set; }
        public string Remarks { get; set; }
        public string AppliedId { get; set; }
        public string IsReject { get; set; }
        public string isCancel { get; set; }
        public string IsApprove { get; set; }
        public string VehiclePurposeResponsiblePersonId { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class VehicleChild
    {
        public string Id { get; set; }
        public string VehicleMovementRequisitionId { get; set; }
        public string FromLocationId { get; set; }
        public string ToLocationId { get; set; }
        public string WithoutPassenger { get; set; }
        public string Remarks { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class VehicleCreation
    {
        public string Id { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public string PersonalOfficial { get; set; }
        public string Name { get; set; }
        public string PurposeId { get; set; }
        public string Purpose { get; set; }
        public string Remarks { get; set; }
        public string EmployeeName { get; set; }
        public string ResponsiblePersonCode { get; set; }
        public string NumberOfPassengers { get; set; }
        public string SelectedApprovePerson { get; set; }
        public string PurposeResponsibleId { get; set; }
    }

    public class VehicleStatus
    {
        public string Id { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public string PersonalOfficial { get; set; }
        public string PurposeId { get; set; }
        public string Purpose { get; set; }
        public string Remarks { get; set; }
        public string EmployeeName { get; set; }
        public string ResponsiblePersonCode { get; set; }
        public string NumberOfPassengers { get; set; }
        public string RequisitionStatus { get; set; }
        public string ApprovedBy { get; set; }
        public string RejectBy { get; set; }
    }

    public class VehicleOutin
    {
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public string RequisitionBy { get; set; }
        public string Purpose { get; set; }
        public string Department { get; set; }
        public string Id { get; set; }
        public string MasterId { get; set; }
        public string TripNumber { get; set; }
        public string TripId { get; set; }
        public string AppliedId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public string DriverMasterId { get; set; }
        public string DriverName { get; set; }
        public string VehicleMasterId { get; set; }
        public string VehicleNumber { get; set; }
        public string VIOId { get; set; }
        public string VehicleAllocationId { get; set; }
    }


    public class VehicleInout
    {
        public string Id { get; set; }
        public string VehicleAllocationId { get; set; }
        public string InDate { get; set; }
        public string OutDate { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string InReading { get; set; }
        public string OutReading { get; set; }
        public string InRemarks { get; set; }
        public string OutRemarks { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }


    }


    public class Vehiclecreationdetails
    {
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public string RequisitionBy { get; set; }
        public string Purpose { get; set; }
        public string Department { get; set; }

    }

    public class VehicleApproveList
    {
        public string Id { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class IncedentCategory
    {
        public string Id { get; set; }
        public string EmployeeName { get; set; }
        public string StandardName { get; set; }

    }


    public class ROCode
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
        public string ROCodes { get; set; } = "";
    }

    public class Incedent
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string EmployeeId { get; set; }
        public string BudgetCode { get; set; }
        public string RONameId { get; set; }
        public string IncedentCategoryId { get; set; }
        public string IncedentItemTitle { get; set; }
        public string IncedentDetail { get; set; }
        public string IncedentType { get; set; }
        public string CriticalityLevel { get; set; }
        public string ActionTaken { get; set; }
        public string StoryPoints { get; set; }
        public string FollowUpApplicable { get; set; }
        public string FollowUpDays { get; set; }
        public string FollowUpById { get; set; }
        public string IssueInchargeId { get; set; }
        public string FinalStatus { get; set; }
        public string Remarks { get; set; }
        public string FileName { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    #endregion vehicle

    public class SevenDaysAttdn
    {
        public string Date { get; set; }
        public string DayStatus { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }

    }

    public class QualityGenaralIssue
    {

        public string Id { get; set; }
        public string IssueNameId { get; set; }
        public string RepeatEntry { get; set; }
        public string QualityIssueDate { get; set; }
        public string QualityIssueTime { get; set; }
        public string EntityId { get; set; }
        public string Entity { get; set; }
        public string ProcessId { get; set; }
        public string Process { get; set; }
        public string IssueId { get; set; }
        public string DefineIssueId { get; set; }
        public string QGIssue { get; set; }
        public string PositionEmployee { get; set; }
        public string QGIEmployeeId { get; set; }
        public string QGIEmployee { get; set; }

    }

    public class QualityPOIssue
    {

        public string PODate { get; set; }
        public string QPDate { get; set; }
        public string Id { get; set; }
        public string QPId { get; set; }
        public string POId { get; set; }
        public string IssueId { get; set; }
        public string QPIssue { get; set; }
        public string ProcessId { get; set; }
        public string Process { get; set; }
        public string EntityId { get; set; }
        public string Entity { get; set; }
        public string DependentOn { get; set; }
        public string Legdays { get; set; }
        public string RepeatEntry { get; set; }
        public string Date { get; set; }
        public string QualityPlanDate { get; set; }
        public string Remarks { get; set; }
        public string LotNumber { get; set; }
        public string EntryLevel { get; set; }
        public string Customer { get; set; }
        public string POStatus { get; set; }
        public string QPEmployeeId { get; set; }
        public string QPEmployee { get; set; }

    }

    public class QualityHeader
    {
        public string Id { get; set; }
        public string PlantId { get; set; }
        public string EntityId { get; set; }
        public string ProcessId { get; set; }
        public string ProductionDate { get; set; }
        public string ProductionShiftId { get; set; }
        public string ProductionOrderId { get; set; }
        public string IssueId { get; set; }
        public string PeriodId { get; set; }
        public string ProductionInchargeId { get; set; }
        public string LotNumber { get; set; }
        public string Remarks { get; set; }
        public string MasterOrderItemId { get; set; }
        public string SalesOrderId { get; set; }
        public string QualityPlanId { get; set; }
        public string PlanType { get; set; }
        public string WorkCenterId { get; set; }
        public string RepeatEntry { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class QualityChild
    {
        public string Id { get; set; }
        public string ItemId { get; set; }
        public string SNO { get; set; }
        public string ItemName { get; set; }
        public string UOMId { get; set; }
        public string UOM { get; set; }
        public string Value { get; set; }
        public string GradeId { get; set; }
        public string MaxValue { get; set; }
        public string MinValue { get; set; }
        public string Remarks { get; set; }
        public string ActionToBeTaken { get; set; }
        public string WorkCenterId { get; set; }
        public string ResponsiblePerson { get; set; }
        public string Checkpoints { get; set; }
        public string QCId { get; set; }
        public string Repeat { get; set; }
        public string IsWorkCenter { get; set; }

    }

    public class Leavesystem
    {
        public string SystemId { get; set; }
        public string LeaveTypeId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DOJ { get; set; }
        public string PlantName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string EmployeeCategory { get; set; }
        public string LeaveName { get; set; }
        public string DaysCanBeSanctioned { get; set; }
        public string CurrentYearAllocation { get; set; }
        public string CurrentAllocation { get; set; }
        public string YearEndEncash { get; set; }
        public string AppliedLeave { get; set; }
        public string CarryForwardOpeningBalance { get; set; }
        public string BroughtForward { get; set; }
        public string LeaveDays { get; set; }
        public string AppliedDays { get; set; }
        public string AvailedDays { get; set; }
        public string AllFutureAppliedLeave { get; set; }
        public string ClosingBalance { get; set; }

    }

    public class QualityHeaderChild
    {
        public string Id { get; set; }
        public string QCId { get; set; }
        public string ItemId { get; set; }
        public string Value { get; set; }
        public string GradeId { get; set; }
        public string ActionToBeTaken { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string Remarks { get; set; }
        public string Repeat { get; set; }
        public string RepeatEntry { get; set; }
        public string WorkCenterId { get; set; }
        public string Status { get; set; }
        public string ConfirmBy { get; set; }
        public string ConfirmationRemarks { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class ArticleItem
    {
        public string MasterOrderItemId { get; set; }
        public string SOId { get; set; }
        public string Article { get; set; }

    }
    public class QualityPlanProcess
    {
        public string Id { get; set; }
        public string QPId { get; set; }
        public string POId { get; set; }
        public string IssueId { get; set; }
        public string DependentOn { get; set; }
        public string Legdays { get; set; }
        public string Date { get; set; }
        public string QualityPlanDate { get; set; }
        public string QCId { get; set; }
        public string QPEmployeeId { get; set; }
        public string RepeatEntry { get; set; }
        public string LotNumber { get; set; }
        public string EntryLevel { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class AccountBalence
    {
        public string Id { get; set; }
        public string BankMasterId { get; set; }
        public string ClosingDate { get; set; }
        public string ClosingBalence { get; set; }
        public string Remarks { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class GatePassCheckApprove
    {
        public string Id { get; set; }
        public string VechileNo { get; set; }
        public string ByWhomId { get; set; }
        public string MobileNo { get; set; }
        public string SecurityInChargeId { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string CheckById { get; set; }
        public string ApproveById { get; set; }
        public string UserRef { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Remark { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string ApprovedReason { get; set; }
        public string ApprovedStatus { get; set; }
        public string CheckedStatus { get; set; }
        public string CheckedReason { get; set; }
        public string IsDispatchConfirmation { get; set; }
        public string DispatchConfirmationBy { get; set; }
        public string DispatchConfirmationDate { get; set; }
        public string ByWhom { get; set; }
        public string SecurityInCharge { get; set; }
        public string ResponsiblePerson { get; set; }
        public string CheckBy { get; set; }
        public string ApproveBy { get; set; }
    }

    public class InvoiceDataGetset
    {
        public string InvoiceNo { get; set; }
        public string Customer { get; set; }
        public string ResponsiblePerson { get; set; }
        public string CustomerType { get; set; }
        public string InvoiceDate { get; set; }
        public string ShipmentDate { get; set; }
        public string DocReceivedate { get; set; }
        public string DocSubDate { get; set; }
        public string DocAccpDate { get; set; }
        public string PayAdbisNo { get; set; }
        public string PayResDate { get; set; }
        public string InvoiceAmount { get; set; }

    }

    public class InvoiceDataEntry
    {
        public string Id { get; set; }
        public string SalesId { get; set; }
        public string Status { get; set; }
        public string ActionToBeTakenId { get; set; }
        public string Remarks { get; set; }
        public string CloseStatus { get; set; }
        public string CloseRemarks { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }

    }

    public class InvoiceRemarksDataInvoice
    {

        public string InvoiceRemarksId { get; set; }
        public string Status { get; set; }
        public string ActionToBeTakenId { get; set; }
        public string ActionToBeTakenByName { get; set; }
        public string CloseStatus { get; set; }
        public string AddedBy { get; set; }
        public string CloseRemarks { get; set; }
        public string Remarks { get; set; }
        public string InvoiceRemarksADDDT { get; set; }
        public string UpdatedBy { get; set; }
        public string InvoiceRemarksUPPDT { get; set; }
        public string InvoiceNo { get; set; }
        public string Customer { get; set; }
        public string ResponsiblePerson { get; set; }
        public string CustomerType { get; set; }
        public string InvoiceDate { get; set; }
        public string ShipmentDate { get; set; }
        public string DocReceivedate { get; set; }
        public string DocSubDate { get; set; }
        public string DocAccpDate { get; set; }
        public string PayAdbisNo { get; set; }
        public string PayResDate { get; set; }
        public string InvoiceAmount { get; set; }

    }

    public class PaymentStatus
    {
        public string NoOfInvoice { get; set; }
        public string isSelected { get; set; }
        public string PartyNature { get; set; }
        public string PartyGroup { get; set; }
        public string PartyCategory { get; set; }
        public string PartySubCategory { get; set; }
        public string ResponsiblePerson { get; set; }
        public string PartyId { get; set; }
        public string PartyCode { get; set; }
        public string PartyName { get; set; }
        public string CurrencyCode { get; set; }
        public string GrossSales { get; set; }
        public string Receipts { get; set; }
        public string BooksAdvance { get; set; }
        public string DebitNote { get; set; }
        public string CreditNote { get; set; }
        public string Balance { get; set; }
        public string NetBalance { get; set; }
        public string ActualBalance { get; set; }
        public string LedgerBalanceAmount { get; set; }
        public string WriteOffPendingPost { get; set; }
        public string BooksGrossSales { get; set; }
        public string BooksReceipts { get; set; }
        public string BooksBalance { get; set; }
        public string OverDueMoreThan30 { get; set; }
        public string OverDueMoreThan15 { get; set; }
        public string OverDueLessThan15 { get; set; }
        public string TodayBalance { get; set; }
        public string OneToSevenBalance { get; set; }
        public string EightToThirtyBalance { get; set; }
        public string ThirtyToSixtyBalance { get; set; }
        public string Onword60 { get; set; }
        public string IsVendor { get; set; }

    }

    public class INvoiceWiseAccount
    {
        public string PartyNature { get; set; }
        public string PartyGroup { get; set; }
        public string PartyCategory { get; set; }
        public string PartySubCategory { get; set; }
        public string ResponsiblePerson { get; set; }
        public string Entity { get; set; }
        public string PartyType { get; set; }
        public string PartyId { get; set; }
        public string PartyPlantId { get; set; }
        public string PartyCode { get; set; }
        public string PartyName { get; set; }
        public string PartyPlantName { get; set; }
        public string VoucherNo { get; set; }
        public string PostingDate { get; set; }
        public string InvoiceNo { get; set; }
        public string DocDate { get; set; }
        public string SortDocDate { get; set; }
        public string CurrencyCode { get; set; }
        public string BaseNoOfDays { get; set; }
        public string BaseOnDueDate { get; set; }
        public string ActualDueDate { get; set; }
        public string Days { get; set; }
        public string AgingInvoice { get; set; }
        public string AgingSorting { get; set; }
        public string GrossSales { get; set; }
        public string DebitNoteAmount { get; set; }
        public string TaxAmount { get; set; }
        public string TrnReceipt { get; set; }
        public string TrnBalance { get; set; }
        public string BooksGrossSales { get; set; }
        public string BooksDebitNoteAmount { get; set; }
        public string BooksTaxAmount { get; set; }
        public string BooksReceipt { get; set; }
        public string BooksBalance { get; set; }
        public string INVS { get; set; }
        public string AddRemarks { get; set; }
        public string Resp { get; set; }
        public string CustomerType { get; set; }

    }

    public class QualityControll
    {
        public string HeaderId { get; set; }
        public string Date { get; set; }
        public string PendingTime { get; set; }
        public string EntityId { get; set; }
        public string Entity { get; set; }
        public string ProcessId { get; set; }
        public string Process { get; set; }
        public string IssueId { get; set; }
        public string Issue { get; set; }
        public string CheckedById { get; set; }
        public string CheckedBy { get; set; }
        public string PONo { get; set; }
        public string LotNumber { get; set; }
        public string Article { get; set; }
        public string POStatus { get; set; }

    }

    public class QualityControllUpdate
    {
        public string ParameterId { get; set; }
        public string Parameter { get; set; }
        public string Status { get; set; }
        public string UOM { get; set; }
        public string Value { get; set; }
        public string Max { get; set; }
        public string Min { get; set; }
        public string WorkCenter { get; set; }
        public string GradeName { get; set; }
        public string ActionToBeTakenName { get; set; }
        public string ResponsiblePerson { get; set; }
        public string Remarks { get; set; }
        public string ItemId { get; set; }
        public string AddedDate { get; set; }
        public string AddedTime { get; set; }
    }
    public class QualityActionUpdate
    {
        public string Id { get; set; }
        public string SNO { get; set; }
        public string ReasonId { get; set; }
        public string ActionTaken { get; set; }
        public string ActionById { get; set; }
        public string Remarks { get; set; }
        public string ParameterId { get; set; }
        public string ReasonName { get; set; }
        public string ConfirmRemarks { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string ActionBy { get; set; }
        public string Saved { get; set; }
    }
    public class QualityConfirmssControll
    {
        public string HeaderId { get; set; }
        public string Date { get; set; }
        public string PendingTime { get; set; }
        public string EntityId { get; set; }
        public string Entity { get; set; }
        public string ProcessId { get; set; }
        public string Process { get; set; }
        public string IssueId { get; set; }
        public string Issue { get; set; }
        public string CheckedById { get; set; }
        public string CheckedBy { get; set; }
        public string PONo { get; set; }
        public string LotNumber { get; set; }
        public string Article { get; set; }
        public string POStatus { get; set; }


    }

    public class QualityConfirmssControllMaster
    {
        public string PID { get; set; }
        public string Status { get; set; }
        public string ConfirmBy { get; set; }
        public string ConfirmationRemarks { get; set; }
       
    }

    public class UtilityMasterGet
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string UtilityMasterId { get; set; }
        public string Reading { get; set; }
        public string Quantity { get; set; }
        public string LastReading { get; set; }
        public string LastReadingDate { get; set; }
        public string LastReadingTime { get; set; }
        public string Remarks { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string MultiplyingFactor { get; set; }
        public string UtilityMaster { get; set; }
        public string UoMId { get; set; }
        public string IsMobileEntry { get; set; }

    }

    public class OpenHeadModelNew
    {
        public string Id { get; set; }
        public string ProductionSummaryId { get; set; }
        public string ProductionBookingParameterId { get; set; }
        public string DetentionMasterMachineParameterId { get; set; }
        public string UserName { get; set; }
        public string Formula { get; set; }
        public string FormulaId { get; set; }
        public decimal Value { get; set; }
        public string EntryState { get; set; }
        public string ValueIN { get; set; }
        public bool IsProduction { get; set; }
    }

    public class ProductionEntryDetail
    {
        public string WorkCenterMasterId { get; set; }
        public string ProcessId { get; set; }
        public string Flag { get; set; }
        public string ClickRow { get; set; }
        public string PPQFlag { get; set; }
        public string Id { get; set; }
        public string WorkCenter { get; set; }
        public string ProductionOrderId { get; set; }
        public string IsPreDefineLotApplicable { get; set; }
        public string LotProcessPlanQty { get; set; }
        public string LotNumber { get; set; }
        public string Mentor { get; set; }
        public string ProductionInCharge { get; set; }
        public string ProductionInChargeId { get; set; }
        public string ResponsiblePerson { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string InCharge { get; set; }
        public string InChargeId { get; set; }
        public string CheckedByName { get; set; }
        public string Quantity { get; set; }
        public string ProductionGrade { get; set; }
        public string Remarks { get; set; }
        public string SumMin { get; set; }
        public string RemainingQty { get; set; }
        public string OrderQty { get; set; }
        public string BookedQty { get; set; }
        public string POQty { get; set; }
        public string ProcessPlanQty { get; set; }
        public string CurPOBalProd { get; set; }
        public string POPreviousProdQty { get; set; }
        public string ActualPlannedQty { get; set; }
        public string ProcessPlanPercentage { get; set; }
        public string TargetProductionFP { get; set; }
        public string BookingLevel { get; set; }
        public string SalesOrderId { get; set; }
        public string MasterOrderItemId { get; set; }
        public string ReasonId { get; set; }
        public string ReasonName { get; set; }
        public string POProcessSequence { get; set; }
        public string ProductionVerification { get; set; }
        public string POFirstProcessProductionQty { get; set; }
        public string SOArticle { get; set; }
        public string MOIArticle { get; set; }
        public string ProductCodeArticle { get; set; }
        public string Article { get; set; }
        public string SONo { get; set; }
        public string Customer { get; set; }
        public string ProductCode { get; set; }
        public string ProductDetails { get; set; }
        public string CustomerRefNo { get; set; }

    }
}
