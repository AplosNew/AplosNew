using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using System.Web;
using System.IO;
using Library.Service.Helpers;
using OTSBD;
using System.Linq;
using Library.Service.Enums;
using Library.Model.Enums;

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class IssueStatusReportsController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;


        public IssueStatusReportsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
     
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion


        private string GetSql(Dictionary<string, string> filterString)
        {
            string FilterString = "";

            string FromDate = filterString["FromDate"];
            string ToDate = filterString["ToDate"];
            string ActiveStatus = filterString["ActiveStatus"];
            string status = filterString["Status"];
            string Today = DateTime.Now.ToString("dd-MMM-yyyy");
            string FirstDayOfEndNextWeek = DateTime.Now.AddDays(8).ToString();

            FilterString = "WHERE 1=1 ";
            if (ActiveStatus == "Active")
            {
                FilterString += " AND isnull(CurrentStatus,'')<>'Closed'";
                FilterString += " AND Convert(date,DueDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "')";

            }
            else if (ActiveStatus == "Closed")
            {
                FilterString += " AND isnull(CurrentStatus,'')='Closed'";
                FilterString += " AND Convert(date,ClosingDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "')";

            }
            else
            {
                FilterString += " AND ( (Convert(date,DueDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "') AND isnull(CurrentStatus,'')<>'Closed')";
                FilterString += " OR (Convert(date,ClosingDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "') AND isnull(CurrentStatus,'')='Closed'))";

            }



            return @"SELECT * FROM (SELECT TMM.Id AS TaskMasterId,it.Id AS IssueId,
                     FORMAT(it.IssueDate,'dd-MMM-yyyy') AS IssueCreationDate, FORMAT(it.RequiredDate,'dd-MMM-yyyy') AS IssueRequiredDate,
                      FORMAT(it.CloseDate,'dd-MMM-yyyy') AS IssueCloseDate,
                     FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus,TSC.UserName as SubCategory,TC.UserName as Category,TMM.TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  NULL MasterOrderNo,
                           
                        Buyer=STUFF((select distinct ', ' + XB.UserName from IssueTransaction AS XIT 
                        INNER JOIN IssueBuyer AS XIB ON XIB.IssueTransactionId = XIT.Id
                        LEFT OUTER JOIN [HKP].[Buyer] AS XB ON XB.Id = XIB.BuyerId
                        where XIT.Id=IT.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                        isnull(ii.UserName,'') AS IssueImportance,isnull(eim.EmployeeName,'') AS Mentor,isnull(ig.Name,'') AS IssueGroupName,
                        datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                        ,Department=NULL,Division=NULL
                        FROM TaskManagerMaster AS TMM
                            
                        LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
                        LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                        INNER JOIN IssueTransaction AS IT ON IT.Id = TMM.IssueTransactionId
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)
                               
                        LEFT OUTER JOIN IssueImportance AS ii ON ii.Id=it.IssueImportanceId
                        LEFT OUTER JOIN EmployeeInformation AS eiM ON eim.SystemID=it.MentorId
                        LEFT OUTER JOIN IssueGroup AS ig ON ig.Id=it.IssueGroupId

                        LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                                                    LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId) AS K " + FilterString + " order by IssueGroupName,DueDate";


        }

        private void GetTNAStatusReportsData(out DataTable dtTna, Dictionary<string, string> filterString)
        {
            string sql = GetSql(filterString);
            dtTna = _sqlRepository.GetDataTable(sql);
            dtTna.Columns.Add("EarlyBy", typeof(int));
            dtTna.Columns.Add("LateBy", typeof(int));
            for (int i = 0; i < dtTna.Rows.Count; i++)
            {
                if (dtTna.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED" && dtTna.Rows[i]["ClosingDate"].ToString() != "")
                {

                    DateTime dtDueDate = Convert.ToDateTime(dtTna.Rows[i]["DueDate"].ToString());
                    DateTime dtClosingDate = Convert.ToDateTime(dtTna.Rows[i]["ClosingDate"].ToString());
                    if (dtClosingDate < dtDueDate)
                        dtTna.Rows[i]["EarlyBy"] = Math.Abs(clsStaticInfo.dateDiff(dtClosingDate.ToString("dd-MMM-yyyy"), dtDueDate.ToString("dd-MMM-yyyy")));
                    if (dtClosingDate > dtDueDate)
                        dtTna.Rows[i]["LateBy"] = Math.Abs(clsStaticInfo.dateDiff(dtDueDate.ToString("dd-MMM-yyyy"), dtClosingDate.ToString("dd-MMM-yyyy")));
                }
            }
        }

        private Dictionary<string, List<DataRow>> GetSqlTaskComments(Dictionary<string, string> filterString)
        {
            string FilterString = "";

            string FromDate = filterString["FromDate"];
            string ToDate = filterString["ToDate"];
            string ActiveStatus = filterString["ActiveStatus"];
            string status = filterString["Status"];
            string Today = DateTime.Now.ToString("dd-MMM-yyyy");
            string FirstDayOfEndNextWeek = DateTime.Now.AddDays(8).ToString();

            FilterString = "WHERE 1=1 ";
            if (ActiveStatus == "Active")
            {
                FilterString += " AND isnull(CurrentStatus,'')<>'Closed'";
                FilterString += " AND Convert(date,DueDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "')";

            }
            else if (ActiveStatus == "Closed")
            {
                FilterString += " AND isnull(CurrentStatus,'')='Closed'";
                FilterString += " AND Convert(date,ClosingDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "')";

            }
            else
            {
                FilterString += " AND ( (Convert(date,DueDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "') AND isnull(CurrentStatus,'')<>'Closed')";
                FilterString += " OR (Convert(date,ClosingDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "') AND isnull(CurrentStatus,'')='Closed'))";

            }




            string sql = @"select * from (SELECT tcom.TaskManagerMasterId, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, TMM.TaskType,
                                TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo,
                                format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),
                                ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate,  NULL MasterOrderNo,
                            FORMAT(tcom.CreatedTime,'dd-MMM-yyyy HH:mm:ss tt') AS CreatedTime,ei.EmployeeName AS CommentedBy,
                                    tcom.CommentText,
                            Buyer=null
                            ,StyleNo= NULL
                            ,SONo=NULL
                            ,PRNo=NULL
                            ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=NULL,Division=NULL
                            FROM TaskManagerMaster AS TMM
                             INNER JOIN TaskComments AS tcom ON tcom.TaskManagerMasterId=tmm.Id
                            INNER JOIN EmployeeInformation AS ei ON ei.SystemId=tcom.CreatedById

                            LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo'
                            LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                            LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                            LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
                            where  TMM.TaskTypeGroup = 'Issue') AS K " + FilterString + " order by TaskManagerMasterId,convert(datetime,CreatedTime)";
            DataTable dt = _sqlRepository.GetDataTable(sql);

            Dictionary<string, List<DataRow>> dicComments = new Dictionary<string, List<DataRow>>();
            List<DataRow> Data = new List<DataRow>();
            string id = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (id != dt.Rows[i]["TaskManagerMasterId"].ToString())
                {
                    Data = new List<DataRow>();
                    dicComments.Add(dt.Rows[i]["TaskManagerMasterId"].ToString(), Data);
                }
                Data.Add(dt.Rows[i]);

                id = dt.Rows[i]["TaskManagerMasterId"].ToString();
            }

            return dicComments;
        }

        public IWorkbook GetTNAStatusReport(string CompanyGroupId, string CompanyId, string PlantId, string PlantName, string EmployeeId, string UserName, Dictionary<string, string> filterString)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();

            DataTable dtTNA = null;

            DataSet dsCmp = null;

            DataSet dsFactory = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string FactoryAddress = string.Empty;
            string OTConsiderOn = string.Empty;
            #endregion

            try
            {
                objRpt = new clsReport();


                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);

                #region Get Data Query
                GetTNAStatusReportsData(out dtTNA, filterString);
                if (dtTNA.Rows.Count == 0)
                    throw new Exception("No data found");

                #endregion

                Dictionary<string, List<DataRow>> dicComments = GetSqlTaskComments(filterString);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";

                var isl = 0;
                var SLNo = 1;

                int colTaskType = 0;
                int colTask = 0;
                int colAssignBy = 0;
                int colAssignTo = 0;
                int colDueDate = 0;
                int colCommitmentDate = 0;
                int colMasterOrderNo = 0;
                int colStyleNo = 0;
                int colSONo = 0;
                int colPRNo = 0;
                int colSubCategory = 0;
                int colCategory = 0;
                int colEarlyBy = 0;
                int colLateBy = 0;
                int colClosingDate = 0;

                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);

                objRpt.SelectedPlant(PlantId, out dsFactory);

                workbook = application.Workbooks.Create(1);

                #region Task List

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "SL";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                xlsCol += 1;
                int colIssueId = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Issue Id";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                xlsCol += 1;
                colDueDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Due Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                int colBuyer = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                int colIssueGroupName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Issue Group";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;

                xlsCol += 1;
                int colIssueImportance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Importance";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                int colCurrentStatus = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Current Status";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;

                xlsCol += 1;
                colTaskType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;

                xlsCol += 1;
                colTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 70;

                xlsCol += 1;
                colAssignTo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Assigned To";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol += 1;
                int colLastChat = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Last Activity";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                colCategory = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Category";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                colSubCategory = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Sub Category";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;



                xlsCol += 1;
                colAssignBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Assigned By";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol += 1;
                int colIssueCreationdate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Issue Creation date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                int colrequireddate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Issue Required Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                int colIssueCloseDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Issue Closing Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                colCommitmentDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Commitment Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                colClosingDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Closing Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                colEarlyBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Early By";
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 9;
                xlsCol += 1;
                colLateBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Late By";
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 9;






                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------
                int StartRow = xlsRow;

                //Add rich-text Excel comment
                IFont fontCaption = workbook.CreateFont();
                fontCaption.Size = 8f;
                IFont fontRegular = workbook.CreateFont();
                fontRegular.Italic = true;
                fontRegular.Size = 6f;
                for (int i = 0; i < dtTNA.Rows.Count; i++)
                {

                    #region ----------------------Data-----------------------
                    sheet1.Range[xlsRow, colIssueId].Text = dtTNA.Rows[i]["IssueId"].ToString();

                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, colTaskType].Text = dtTNA.Rows[i]["TaskType"].ToString();
                    sheet1.Range[xlsRow, colTask].Text = dtTNA.Rows[i]["Task"].ToString();
                    sheet1.Range[xlsRow, colAssignBy].Text = dtTNA.Rows[i]["AssignBy"].ToString();
                    sheet1.Range[xlsRow, colAssignTo].Text = dtTNA.Rows[i]["AssignTo"].ToString();
                    clsStaticInfo.SetDate(sheet1[xlsRow, colDueDate], dtTNA.Rows[i]["DueDate"].ToString());
                    clsStaticInfo.SetDate(sheet1[xlsRow, colCommitmentDate], dtTNA.Rows[i]["CommitmentDate"].ToString());

                    sheet1.Range[xlsRow, colLastChat].Text = dtTNA.Rows[i]["LastChat"].ToString();


                    clsStaticInfo.SetDate(sheet1[xlsRow, colIssueCreationdate], dtTNA.Rows[i]["IssueCreationDate"].ToString());
                    clsStaticInfo.SetDate(sheet1[xlsRow, colrequireddate], dtTNA.Rows[i]["IssueRequiredDate"].ToString());
                    clsStaticInfo.SetDate(sheet1[xlsRow, colIssueCloseDate], dtTNA.Rows[i]["IssueCloseDate"].ToString());

                    sheet1.Range[xlsRow, colClosingDate].Text = dtTNA.Rows[i]["ClosingDate"].ToString();

                    sheet1.Range[xlsRow, colBuyer].Text = dtTNA.Rows[i]["Buyer"].ToString();
                    sheet1.Range[xlsRow, colCurrentStatus].Text = dtTNA.Rows[i]["CurrentStatus"].ToString();

                    sheet1.Range[xlsRow, colIssueGroupName].Text = dtTNA.Rows[i]["IssueGroupName"].ToString();
                    sheet1.Range[xlsRow, colIssueImportance].Text = dtTNA.Rows[i]["IssueImportance"].ToString();

                    //sheet1.Range[xlsRow, colDepartment].Text = dtTNA.Rows[i]["Department"].ToString();
                    //sheet1.Range[xlsRow, colDivision].Text = dtTNA.Rows[i]["Division"].ToString();
                    //sheet1.Range[xlsRow, colMasterOrderNo].Text = dtTNA.Rows[i]["MasterOrderNo"].ToString();
                    //sheet1.Range[xlsRow, colStyleNo].Text = dtTNA.Rows[i]["StyleNo"].ToString();
                    //sheet1.Range[xlsRow, colSONo].Text = dtTNA.Rows[i]["SONo"].ToString();
                    //sheet1.Range[xlsRow, colPRNo].Text = dtTNA.Rows[i]["PRNo"].ToString();

                    sheet1.Range[xlsRow, colSubCategory].Text = dtTNA.Rows[i]["SubCategory"].ToString();

                    sheet1.Range[xlsRow, colCategory].Text = dtTNA.Rows[i]["Category"].ToString();

                    double earlyOrLate = clsStaticInfo.dbl(dtTNA.Rows[i]["EarlyOrLateBy"].ToString());

                    double earlyBy = 0;
                    double lateBy = 0;
                    if (earlyOrLate < 0)
                    {
                        earlyBy = Math.Abs(earlyOrLate);
                    }
                    else if (earlyOrLate > 0)
                    {
                        lateBy = Math.Abs(earlyOrLate);
                    }



                    //today's task
                    DateTime DueDate = Convert.ToDateTime(dtTNA.Rows[i]["DueDate"].ToString());
                    DateTime CurrentDate = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy"));
                    if (DueDate == CurrentDate)
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#E6F0FF");


                    //overdue
                    if (DueDate < CurrentDate)
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#FFF4E6");

                    //overdue
                    if (DueDate > CurrentDate)
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#F5FFE6");




                    if (dtTNA.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED")
                    {
                        DateTime ClosingDate = Convert.ToDateTime(dtTNA.Rows[i]["ClosingDate"].ToString());
                        //late closed
                        if (DueDate < ClosingDate)
                            sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#52b3d9");



                        //early closed
                        if (DueDate >= ClosingDate)
                            sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#2ecc71");

                    }

                    #region Comments

                    if (dicComments.ContainsKey(dtTNA.Rows[i]["TaskMasterId"].ToString()))
                    {
                        IRange range = sheet1[xlsRow, colTask];
                        ICommentShape shape = range.AddComment();

                        for (int COMM = 0; COMM < dicComments[dtTNA.Rows[i]["TaskMasterId"].ToString()].Count; COMM++)
                        {
                            DataRow drTempComment = dicComments[dtTNA.Rows[i]["TaskMasterId"].ToString()][COMM];
                            shape.RichText.Append(drTempComment["CommentedBy"].ToString() + " says :" + drTempComment["CommentText"].ToString(), fontCaption);
                            shape.RichText.Append(" " + drTempComment["CreatedTime"].ToString() + Environment.NewLine + Environment.NewLine, fontRegular);
                            shape.IsTextLocked = false;
                            shape.AutoSize = false;

                            shape.Height += 30;
                            shape.Width = 300;
                        }

                    }

                    #endregion Comments

                    sheet1.Range[xlsRow, colEarlyBy].Number = earlyBy;
                    sheet1.Range[xlsRow, colLateBy].Number = lateBy;

                    xlsRow++;
                    SLNo++;
                    #endregion ----------------------Data-----------------------

                }
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                sheet1.Range[StartRow, 1, xlsRow - 1, endXlsCol].CellStyle.Font.Size = 8f;
                sheet1.AutoFilters.FilterRange = sheet1.Range[StartRow - 1, 1, xlsRow, endXlsCol];


                sheet1.Range[StartRow, colDueDate, xlsRow, colDueDate].NumberFormat = "dd-MMM-yyyy";
                sheet1.Range[StartRow, colCommitmentDate, xlsRow, colCommitmentDate].NumberFormat = "dd-MMM-yyyy";
                sheet1.Range[StartRow, colIssueCreationdate, xlsRow, colIssueCreationdate].NumberFormat = "dd-MMM-yyyy";
                sheet1.Range[StartRow, colrequireddate, xlsRow, colrequireddate].NumberFormat = "dd-MMM-yyyy";
                sheet1.Range[StartRow, colIssueCloseDate, xlsRow, colIssueCloseDate].NumberFormat = "dd-MMM-yyyy";

                #region ******************Report Header******************
                xlsRow = 1;
                FactoryName = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Issue List ";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment
                sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange.WrapText = true;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + UserName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Task List";
                #endregion Page Setup

                #endregion  ManualOutTime



                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Worker Late Status
        [HttpPost, Authorize]
        public ActionResult GetTNAStatusReports(ReportFormat reportFormat, Dictionary<string, string> filterString)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = GetTNAStatusReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, identity.EmployeeId, identity.Name, filterString);

                workbook.Version = ExcelVersion.Excel2013;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "Issue Status Reports.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        # endregion WORKER Late Status

        [HttpPost, Authorize]
        public ActionResult GetTaskList(Dictionary<string, string> filterString)
        {

            string sql = GetSql(filterString);
            DataTable dtTna = _sqlRepository.GetDataTable(sql);
            dtTna.Columns.Add("EarlyBy", typeof(int));
            dtTna.Columns.Add("LateBy", typeof(int));
            for (int i = 0; i < dtTna.Rows.Count; i++)
            {
                if (dtTna.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED" && dtTna.Rows[i]["ClosingDate"].ToString() != "")
                {

                    DateTime dtDueDate = Convert.ToDateTime(dtTna.Rows[i]["DueDate"].ToString());
                    DateTime dtClosingDate = Convert.ToDateTime(dtTna.Rows[i]["ClosingDate"].ToString());
                    if (dtClosingDate < dtDueDate)
                        dtTna.Rows[i]["EarlyBy"] = Math.Abs(clsStaticInfo.dateDiff(dtClosingDate.ToString("dd-MMM-yyyy"), dtDueDate.ToString("dd-MMM-yyyy")));
                    if (dtClosingDate > dtDueDate)
                        dtTna.Rows[i]["LateBy"] = Math.Abs(clsStaticInfo.dateDiff(dtDueDate.ToString("dd-MMM-yyyy"), dtClosingDate.ToString("dd-MMM-yyyy")));
                }
            }
            var jsondata = Json(CustomJsonResultService.DataTableToJson(dtTna), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

    }
}