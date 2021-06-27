using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Currencies;
using Library.Model.Enums;
using Library.Model.Vouchers;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Properties;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class AccountsMaterialGroupGlService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        CustomIdentity identity;


        public AccountsMaterialGroupGlService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
            try
            {
                identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            }
            catch (Exception ex)
            {

            }
        }


        private string MaterialGroupGlSql()
        {


            return @" DECLARE @sql_ nvarchar(max)

                                    select MaterialGroupMasterId,
									MaterialGroupMasterName,MaterialGroup1Name,
									MaterialGroup2Name,MaterialGroup3Name,MaterialGroup4Name,
									MaterialTypeName,MaterialGroup1Id,COAName
									,DownPaymentGLInfo									
									,ClearingAccountGLInfo									
									,InventoryGLInfo							
									,ExpenseGLInfo									
									,DebitNoteGLInfo									
									,CreditNoteGLInfo								
									,ShortageGLInfo
									,RejectionGLInfo								
									,GL,
							
									PartyAccountGroup +'GL' PartyAccountGroup
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (
									SELECT  
									MGM.Id MaterialGroupMasterId,
									 MGM.UserName AS MaterialGroupMasterName
									, MG1.UserName AS MaterialGroup1Name
					, MG2.UserName AS MaterialGroup2Name
					 , MG3.UserName AS MaterialGroup3Name, MG4.UserName AS MaterialGroup4Name
					, MT.[Description] AS MaterialTypeName, MGM.MaterialGroup1Id
					, F.UserName 'COAName', F.DownPaymentGLInfo
					,F.ClearingAccountGLInfo
					, F.InventoryGLInfo
					, F.ExpenseGLInfo
					, F.DebitNoteGLInfo
					, F.CreditNoteGLInfo
					, F.ShortageGLInfo
					, F.RejectionGLInfo
					,MGGL.PartyAccountGroup--
					,MGGL.GL
					FROM MST.MaterialGroupMaster As MGM
					LEFT JOIN HKP.MaterialGroup1 As MG1 ON MG1.Id = MGM.MaterialGroup1Id
					LEFT JOIN HKP.MaterialGroup2 As MG2 ON MG2.Id = MGM.MaterialGroup2Id
					LEFT JOIN HKP.MaterialGroup3 As MG3 ON MG3.Id = MGM.MaterialGroup3Id
					LEFT JOIN HKP.MaterialGroup4 As MG4 ON MG4.Id = MGM.MaterialGroup4Id
					LEFT JOIN HKP.MaterialType As MT ON MT.Id = MGM.MaterialTypeId
					LEFT JOIN (SELECT MAD.Id, MAD.MaterialGroupMasterId, c.Id AS COAId, GLGI1.AccountCode
					, C.UserName, GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo, GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					, GLGI3.AccountCode + ' - ' + GLGI3.UserName AS InventoryGLInfo, GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo

					, GLGIDN.AccountCode + ' - ' + GLGIDN.UserName AS DebitNoteGLInfo 
					, GLGICN.AccountCode + ' - ' + GLGICN.UserName AS CreditNoteGLInfo 
					, GLGIST.AccountCode + ' - ' + GLGIST.UserName AS ShortageGLInfo 
					, GLGIRJ.AccountCode + ' - ' + GLGIRJ.UserName AS RejectionGLInfo 

					FROM HKP.COA AS C
					LEFT JOIN HKP.MaterialGroupGL AS MAD ON MAD.COAId=c.Id
					LEFT JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
					LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
					LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.InventoryGLId
					LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
					LEFT JOIN HKP.GLGeneralInfo AS GLGIDN ON GLGIDN.Id=MAD.DebitNoteGLId
					LEFT JOIN HKP.GLGeneralInfo AS GLGICN ON GLGICN.Id=MAD.CreditNoteGLId
					LEFT JOIN HKP.GLGeneralInfo AS GLGIST ON GLGIST.Id=MAD.ShortageGLId
					LEFT JOIN HKP.GLGeneralInfo AS GLGIRJ ON GLGIRJ.Id=MAD.RejectionGLId

					where ISNULL(c.Id,'') ='1'
					)AS F ON F.MaterialGroupMasterId = MGM.Id

					LEFT JOIN ( SELECT MGL.Id,MGL.MaterialGroupMasterId,GL.UserName GL
					,MGL.GLType,MGL.PartyAccountGroupId,PAG.UserName PartyAccountGroup
					FROM HKP.MaterialGroupPartyAccountGroupGL MGL
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=MGL.PartyAccountGroupId
					LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=MGL.GLGeneralInfoId
					LEFT JOIN MST.BudgetMaster AS BM ON MGL.BudgetMasterId = BM.Id
					) MGGL ON MGGL.MaterialGroupMasterId=MGM.Id
										) A
										group by  a.MaterialGroupMasterId,a.MaterialGroupMasterName,a.MaterialGroup1Name,a.MaterialGroup2Name
										,a.MaterialGroup2Name,a.MaterialGroup3Name,a.MaterialGroup4Name,a.MaterialTypeName,a.MaterialGroup1Id,a.COAName,a.DownPaymentGLInfo
										
										,a.ClearingAccountGLInfo
										,a.InventoryGLInfo
										,a.ExpenseGLInfo
										,a.DebitNoteGLInfo
										,a.CreditNoteGLInfo
										,a.ShortageGLInfo
										,a.RejectionGLInfo
										,a.PartyAccountGroup
										,a.GL
										,a.ClearingAccountGLInfo
										,a.MaterialGroup1Name

                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max),
                                    @col1 nvarchar(max)

                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(PartyAccountGroup)	
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )
							

                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([GL]) FOR [PartyAccountGroup] IN ('+STUFF(@col,1,1,'')+')
								 
                            ) as pvt)'
				
                            EXEC sp_executesql @sql
                            drop table #tempOT

                    ";

        }
        private string MaterialGroupBudgetSql()
        {

            return @" DECLARE @sql_ nvarchar(max)
                                    select MaterialGroupMasterId																	
									,DownPaymentBudgetName
									,ClearingAccountBudgetName
									
									,InventoryBudgetName
									,ExpenseBudgetName
									,DebitNoteBudgetName
									,CreditNoteBudgetName
									,ShortageBudgetName
									,RejectionBudgetName														
									,Budget
									,PartyAccountGroup +'Budget' PartyAccountGroup
                                    INTO #tempOTT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (
									SELECT  
									MGM.Id MaterialGroupMasterId							
							, F.DownPaymentBudgetName
							, F.ClearingAccountBudgetName
							, F.InventoryBudgetName
							, F.ExpenseBudgetName
							, F.DebitNoteBudgetName
							, F.CreditNoteBudgetName
							, F.ShortageBudgetName
							, F.RejectionBudgetName
							,MGGL.PartyAccountGroup


							,MGGL.Budget
							FROM MST.MaterialGroupMaster As MGM
							
							LEFT JOIN (SELECT MAD.Id, MAD.MaterialGroupMasterId, 
							 C.UserName

							, DPB.UserName AS DownPaymentBudgetName 
							, CAB.UserName AS ClearingAccountBudgetName
							, MAD.ExpenseBudgetMasterId, MAD.ExpenseActivityId, IB.UserName AS InventoryBudgetName
							, EB.UserName AS ExpenseBudgetName
							, BDN.UserName AS DebitNoteBudgetName 
							, BCN.UserName AS CreditNoteBudgetName 
							,BST.UserName AS ShortageBudgetName 
							,BRJ.UserName AS RejectionBudgetName 
							FROM HKP.COA AS C
							LEFT JOIN HKP.MaterialGroupGL AS MAD ON MAD.COAId=c.Id

							LEFT JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
							LEFT JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id

							LEFT JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
							LEFT JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id

							LEFT JOIN MST.BudgetMaster AS IBM ON MAD.InventoryBudgetMasterId = IBM.Id
							LEFT JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
							LEFT JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
							LEFT JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
							LEFT JOIN MST.BudgetMaster AS BMDN ON MAD.DebitNoteBudgetMasterId = BMDN.Id
							LEFT JOIN HKP.Budget AS BDN ON BMDN.BudgetId = BDN.Id
							LEFT JOIN MST.BudgetMaster AS BMCN ON MAD.CreditNoteBudgetMasterId = BMDN.Id
							LEFT JOIN HKP.Budget AS BCN ON BMCN.BudgetId = BCN.Id
							LEFT JOIN MST.BudgetMaster AS BMST ON MAD.ShortageBudgetMasterId = BMDN.Id
							LEFT JOIN HKP.Budget AS BST ON BMCN.BudgetId = BST.Id
							LEFT JOIN MST.BudgetMaster AS BMRJ ON MAD.RejectionBudgetMasterId = BMDN.Id
							LEFT JOIN HKP.Budget AS BRJ ON BMCN.BudgetId = BRJ.Id
							where ISNULL(c.Id,'') ='1'
							)AS F ON F.MaterialGroupMasterId = MGM.Id

							LEFT JOIN ( SELECT MGL.Id,MGL.MaterialGroupMasterId,GL.UserName GL
							,B.UserName Budget,A.UserName Activity
							,MGL.GLType,MGL.PartyAccountGroupId,PAG.UserName PartyAccountGroup
							FROM HKP.MaterialGroupPartyAccountGroupGL MGL
							LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=MGL.PartyAccountGroupId
							LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=MGL.GLGeneralInfoId
							LEFT JOIN MST.BudgetMaster AS BM ON MGL.BudgetMasterId = BM.Id
							LEFT JOIN HKP.Budget AS B ON BM.BudgetId = B.Id
							LEFT JOIN HKP.Activity AS A ON MGL.ActivityId = A.Id
							) MGGL ON MGGL.MaterialGroupMasterId=MGM.Id
										) A
										group by  a.MaterialGroupMasterId
										,a.DownPaymentBudgetName
										,a.ClearingAccountBudgetName
										,a.InventoryBudgetName
										,a.ExpenseBudgetName
										,a.DebitNoteBudgetName
										,a.CreditNoteBudgetName
										,a.ShortageBudgetName
										,a.RejectionBudgetName
										,a.PartyAccountGroup					
										,a.Budget	
										, a.ClearingAccountBudgetName
														
                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max),
                                    @col1 nvarchar(max)

                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(PartyAccountGroup)	
                                FROM #tempOTT 
                                FOR XML PATH ('')
                            )
							

                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOTT
                            PIVOT (
                                MAX([Budget]) FOR [PartyAccountGroup] IN ('+STUFF(@col,1,1,'')+')
								 
                            ) as pvt)'
				
                            EXEC sp_executesql @sql
                            drop table #tempOTT
                            


                    ";

        }
        private string MaterialGroupActivitySql()
        {

            return @" 
                      
                        DECLARE @sql_ nvarchar(max)

                                    select MaterialGroupMasterId
									
									,DownPaymentActivityName
									,ClearingAccountActivityName
								,InventoryActivityName
							,ExpenseActivityName
									,DebitNoteActivityName
									
									
									,CreditNoteActivityName
									,ShortageActivityName
									,RejectionActivityName,
									Activity,


									PartyAccountGroup +'Activity' PartyAccountGroup
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (
									SELECT  
									MGM.Id MaterialGroupMasterId
									
									

										, F.DownPaymentActivityName
										, F.ClearingAccountActivityName
										, F.InventoryActivityName
										, F.ExpenseActivityName
										, F.DebitNoteActivityName
										, F.CreditNoteActivityName
										, F.ShortageActivityName
										, F.RejectionActivityName
										,MGGL.PartyAccountGroup
										,MGGL.Activity
										FROM MST.MaterialGroupMaster As MGM

										LEFT JOIN (SELECT MAD.Id, MAD.MaterialGroupMasterId


										 ,DPA.UserName AS DownPaymentActivityName
										, CAA.UserName AS ClearingAccountActivityName
										, MAD.ExpenseActivityId
										,IA.UserName AS InventoryActivityName
										, EA.UserName AS ExpenseActivityName

										, ADN.UserName AS DebitNoteActivityName
										, ACN.UserName AS CreditNoteActivityName
										 , AST.UserName AS ShortageActivityName
										 , ARJ.UserName AS RejectionActivityName

										FROM HKP.COA AS C
										LEFT JOIN HKP.MaterialGroupGL AS MAD ON MAD.COAId=c.Id

										LEFT JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id
										LEFT JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id
										LEFT JOIN HKP.Activity AS IA ON MAD.InventoryActivityId = IA.Id
										LEFT JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = EA.Id
										LEFT JOIN HKP.Activity AS ADN ON MAD.DebitNoteActivityId = ADN.Id
										LEFT JOIN HKP.Activity AS ACN ON MAD.CreditNoteActivityId = ACN.Id
										LEFT JOIN HKP.Activity AS AST ON MAD.ShortageActivityId = AST.Id
										LEFT JOIN HKP.Activity AS ARJ ON MAD.RejectionActivityId = ARJ.Id
																					where ISNULL(c.Id,'') ='1'
											)AS F ON F.MaterialGroupMasterId = MGM.Id

											LEFT JOIN ( SELECT MGL.Id,MGL.MaterialGroupMasterId
											,A.UserName Activity,MGL.PartyAccountGroupId,PAG.UserName PartyAccountGroup
											FROM HKP.MaterialGroupPartyAccountGroupGL MGL
											LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=MGL.PartyAccountGroupId
											LEFT JOIN HKP.Activity AS A ON MGL.ActivityId = A.Id
											) MGGL ON MGGL.MaterialGroupMasterId=MGM.Id
										) A
										group by  a.MaterialGroupMasterId
										,a.DownPaymentActivityName
										,a.ClearingAccountActivityName
									    ,a.InventoryActivityName
										,a.ExpenseActivityName
										,a.DebitNoteActivityName
										,a.CreditNoteActivityName
										,a.ShortageActivityName
							     		,a.RejectionActivityName
										,a.PartyAccountGroup
										,a.Activity								
										,a.DownPaymentActivityName
									, a.ClearingAccountActivityName  
                              ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max),
                                    @col1 nvarchar(max)

                                SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(PartyAccountGroup)	
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )
						
                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([Activity]) FOR [PartyAccountGroup] IN ('+STUFF(@col,1,1,'')+')
								 
                            ) as pvt)'
				
                            EXEC sp_executesql @sql
                            drop table #tempOT
    

                    ";

        }

        public void MaterialGrouprRport()
        {
            try
            {
               
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Material Group Report";

                DataTable dtMaterialGroupWithGL = _sqlRepository.GetDataTable(MaterialGroupGlSql());
                DataTable dtMaterialGroupWithBudget = _sqlRepository.GetDataTable(MaterialGroupBudgetSql());
                DataTable dtMaterialGroupWithActivity = _sqlRepository.GetDataTable(MaterialGroupActivitySql());

                int ROW = 6;
                int COL = 1;

                sheet[ROW, COL].Text = "Sl No.";
                sheet[ROW, COL].ColumnWidth = 3;
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Material Group Master Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colMaterialGroupMasterName = COL;
                COL++;
                sheet[ROW, COL].Text = "Material Group1 Name";

                sheet[ROW, COL].ColumnWidth = 15;
                int colMaterialGroup1Name = COL;
                COL++;
                sheet[ROW, COL].Text = "Material Group2 Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colMaterialGroup2Name = COL;
                COL++;
                sheet[ROW, COL].Text = "Material Group3 Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colMaterialGroup3Name = COL;
                COL++;
                sheet[ROW, COL].Text = "Material Group4 Name";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colMaterialGroup4Name = COL;
                COL++;
                sheet[ROW, COL].Text = "Material Type Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colMaterialTypeName = COL;
                COL++;
                sheet[ROW, COL].Text = "Material Group1 Id";
                sheet[ROW, COL].ColumnWidth = 15;
                int colMaterialGroup1Id = COL;
                COL++;
                sheet[ROW, COL].Text = "COA Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCOAName = COL;
                COL++;
                sheet[ROW, COL].Text = "Down Payment GL Info";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDownPaymentGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Down Payment Budget Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDownPaymentBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Down Payment Activity Name	";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDownPaymentActivityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Clearing Account GL Info";
                sheet[ROW, COL].ColumnWidth = 15;
                int colClearingAccountGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Clearing Account Budget Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colClearingAccountBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Clearing Account Activity Name	";
                sheet[ROW, COL].ColumnWidth = 15;
                int colClearingAccountActivityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Inventory GL Info";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colInventoryGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Inventory Budget Name";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colInventoryBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Inventory Activity Name";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colInventoryActivityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Expense GL Info";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colExpenseGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Expense Budget Name";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colExpenseBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Expense Activity Name";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colExpenseActivityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Debit Note GL Info";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colDebitNoteGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Debit Note Budget Name";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colDebitNoteBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Debit Note Activity Name";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colDebitNoteActivityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Credit Note GL Info";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colCreditNoteGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Credit Note Budget Name";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colCreditNoteBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Credit Note Activity Name";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colCreditNoteActivityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Shortage GL Info";
                sheet[ROW, COL].ColumnWidth = 15;
                int colShortageGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Shortage Budget Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colShortageBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Shortage Activity Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colshortageActivityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Rejection GL Info";
                sheet[ROW, COL].ColumnWidth = 15;
                int colRejectionGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Rejection Budget Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colRejectionBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Rejection Activity Name";
                sheet[ROW, COL].ColumnWidth = 15;
                int colRejectionActivityName = COL;
            

                Dictionary<string, int> DicColIndex = new Dictionary<string, int>();
                bool GLColumnFound = false;
                for (int i = 0; i < dtMaterialGroupWithGL.Columns.Count - 1; i++)
                {
                    
                   if (dtMaterialGroupWithGL.Columns[i].ColumnName.ToString().ToUpper() == "REJECTIONGLINFO")
                        GLColumnFound = true;

                    if (GLColumnFound == false)
                        continue;

                    COL++;
                    sheet[ROW, COL].Text = dtMaterialGroupWithGL.Columns[i + 1].ColumnName;
                    sheet[ROW, COL].ColumnWidth = 15;
                    DicColIndex.Add(dtMaterialGroupWithGL.Columns[i + 1].ColumnName, COL);

                    COL++;
                    sheet[ROW, COL].Text = dtMaterialGroupWithGL.Columns[i + 1].ColumnName.Replace("GL", "Budget");
                    sheet[ROW, COL].ColumnWidth = 15;
                    DicColIndex.Add(dtMaterialGroupWithGL.Columns[i + 1].ColumnName.Replace("GL", "Budget"), COL);

                    COL++;
                    sheet[ROW, COL].Text = dtMaterialGroupWithGL.Columns[i + 1].ColumnName.Replace("GL", "Activity");
                    sheet[ROW, COL].ColumnWidth = 15;
                    DicColIndex.Add(dtMaterialGroupWithGL.Columns[i + 1].ColumnName.Replace("GL", "Activity"), COL);
                }

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtMaterialGroupWithGL.Rows.Count; i++)
                {
                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colMaterialGroupMasterName].Text = dtMaterialGroupWithGL.Rows[i]["MaterialGroupMasterName"].ToString();
                    sheet[ROW, colMaterialGroup1Name].Text = dtMaterialGroupWithGL.Rows[i]["MaterialGroup1Name"].ToString();
                    sheet[ROW, colMaterialGroup2Name].Text = dtMaterialGroupWithGL.Rows[i]["MaterialGroup2Name"].ToString();
                    sheet[ROW, colMaterialGroup3Name].Text = dtMaterialGroupWithGL.Rows[i]["MaterialGroup3Name"].ToString();
                    sheet[ROW, colMaterialGroup4Name].Text = dtMaterialGroupWithGL.Rows[i]["MaterialGroup4Name"].ToString();
                    sheet[ROW, colMaterialTypeName].Text = dtMaterialGroupWithGL.Rows[i]["MaterialTypeName"].ToString();
                    sheet[ROW, colMaterialGroup1Id].Text = dtMaterialGroupWithGL.Rows[i]["MaterialGroup1Id"].ToString();
                    sheet[ROW, colCOAName].Text = dtMaterialGroupWithGL.Rows[i]["COAName"].ToString();
                    sheet[ROW, colDownPaymentGLInfo].Text = dtMaterialGroupWithGL.Rows[i]["DownPaymentGLInfo"].ToString();
                    sheet[ROW, colClearingAccountGLInfo].Text = dtMaterialGroupWithGL.Rows[i]["ClearingAccountGLInfo"].ToString();
                    sheet[ROW, colInventoryGLInfo].Text = dtMaterialGroupWithGL.Rows[i]["InventoryGLInfo"].ToString();
                    sheet[ROW, colExpenseGLInfo].Text = dtMaterialGroupWithGL.Rows[i]["ExpenseGLInfo"].ToString();
                    sheet[ROW, colDebitNoteGLInfo].Text = dtMaterialGroupWithGL.Rows[i]["DebitNoteGLInfo"].ToString();
                    sheet[ROW, colCreditNoteGLInfo].Text = dtMaterialGroupWithGL.Rows[i]["CreditNoteGLInfo"].ToString();
                    sheet[ROW, colShortageGLInfo].Text = dtMaterialGroupWithGL.Rows[i]["ShortageGLInfo"].ToString();
                    sheet[ROW, colRejectionGLInfo].Text = dtMaterialGroupWithGL.Rows[i]["RejectionGLInfo"].ToString();
            
                    foreach (var item in DicColIndex)
                    {
                        if (dtMaterialGroupWithGL.Columns.Contains(item.Key))
                            sheet[ROW, item.Value].Text = dtMaterialGroupWithGL.Rows[i][item.Key].ToString();
                    }

                    dtMaterialGroupWithBudget.DefaultView.RowFilter = "MaterialGroupMasterId='" + dtMaterialGroupWithGL.Rows[i]["MaterialGroupMasterId"].ToString() + @"'";
                    if (dtMaterialGroupWithBudget.DefaultView.Count > 0)
                    {

                        sheet[ROW, colDownPaymentBudgetName].Text = dtMaterialGroupWithBudget.DefaultView[0]["DownPaymentBudgetName"].ToString();
                        sheet[ROW, colClearingAccountBudgetName].Text = dtMaterialGroupWithBudget.DefaultView[0]["ClearingAccountBudgetName"].ToString();
                        sheet[ROW, colInventoryBudgetName].Text = dtMaterialGroupWithBudget.DefaultView[0]["InventoryBudgetName"].ToString();
                        sheet[ROW, colExpenseBudgetName].Text = dtMaterialGroupWithBudget.DefaultView[0]["ExpenseBudgetName"].ToString();
                        sheet[ROW, colDebitNoteBudgetName].Text = dtMaterialGroupWithBudget.DefaultView[0]["DebitNoteBudgetName"].ToString();
                        sheet[ROW, colCreditNoteBudgetName].Text = dtMaterialGroupWithBudget.DefaultView[0]["CreditNoteBudgetName"].ToString();
                        sheet[ROW, colShortageBudgetName].Text = dtMaterialGroupWithBudget.DefaultView[0]["ShortageBudgetName"].ToString();
                        sheet[ROW, colRejectionBudgetName].Text = dtMaterialGroupWithBudget.DefaultView[0]["RejectionBudgetName"].ToString();
                  

                        foreach (var item in DicColIndex)
                        {
                            if (dtMaterialGroupWithBudget.Columns.Contains(item.Key))
                                sheet[ROW, item.Value].Text = dtMaterialGroupWithBudget.DefaultView[0][item.Key].ToString();
                        }

                    }
                    dtMaterialGroupWithActivity.DefaultView.RowFilter = "MaterialGroupMasterId='" + dtMaterialGroupWithGL.Rows[i]["MaterialGroupMasterId"].ToString() + @"'";

                    if (dtMaterialGroupWithActivity.DefaultView.Count > 0)
                    {
                        sheet[ROW, colDownPaymentActivityName].Text = dtMaterialGroupWithActivity.DefaultView[0]["DownPaymentActivityName"].ToString();
                        sheet[ROW, colClearingAccountActivityName].Text = dtMaterialGroupWithActivity.DefaultView[0]["ClearingAccountActivityName"].ToString();
                        sheet[ROW, colInventoryActivityName].Text = dtMaterialGroupWithActivity.DefaultView[0]["InventoryActivityName"].ToString();
                        sheet[ROW, colExpenseActivityName].Text = dtMaterialGroupWithActivity.DefaultView[0]["ExpenseActivityName"].ToString();
                        sheet[ROW, colDebitNoteActivityName].Text = dtMaterialGroupWithActivity.DefaultView[0]["DebitNoteActivityName"].ToString();
                        sheet[ROW, colCreditNoteActivityName].Text = dtMaterialGroupWithActivity.DefaultView[0]["CreditNoteActivityName"].ToString();
                        sheet[ROW, colshortageActivityName].Text = dtMaterialGroupWithActivity.DefaultView[0]["ShortageActivityName"].ToString();
                        sheet[ROW, colRejectionActivityName].Text = dtMaterialGroupWithActivity.DefaultView[0]["RejectionActivityName"].ToString();
             
                        foreach (var item in DicColIndex)
                        {
                            if (dtMaterialGroupWithActivity.Columns.Contains(item.Key))
                                sheet[ROW, item.Value].Text = dtMaterialGroupWithActivity.DefaultView[0][item.Key].ToString();
                        }
                    }

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyGroupHeader(ref sheet, endCol, "Material Group Report", identity.CompanyGroupId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "Material Group Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
