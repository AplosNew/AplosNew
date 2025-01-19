using Aplos.Controllers;
using Aplos.Properties;
//using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.HumanResources;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class BulkIncrementController : BaseController
    {
        #region Constructor

       
        private readonly ISqlRepository _sqlRepository;
      
        public BulkIncrementController( ISqlRepository sqlRepository )
        {            
            _sqlRepository = sqlRepository;          
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetEmployeeListWithSalaryInfo(string MonthNo, string LoadEffectiveDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * FROM (SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             ei.SystemId 
                            ,ei.EmployeeCode                          
                            ,ei.EmployeeName
                           
                            --,sidm.SystemID
                            , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,S.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,srm.SalaryRuleName
                            ,sid1.EntryAmount Basic
                            ,sid1.EntryAmount BasicOld
                            ,sidGross.EntryAmount Gross
                            ,sidGross.EntryAmount GrossOld
                            ,sidm.SalaryRuleMasterSystemID  
                            ,sidm.SystemID SalaryId
                            ,FORMAT(ei.DOJ,'dd-MMM-yyyy') DOJ
							,FORMAT(sidm.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
							,FORMAT(ND.NextDueDate,'dd-MMM-yyyy') NextDueDate
							,DENSE_RANK() OVER (PARTITION BY sidm.SystemID ORDER BY NextDueDate DESC) AS LastDueFlag
							
							,DateDiff(MONTH, sidm.EffectiveDate,'" + LoadEffectiveDate + @"') diff
							,sidG.SalaryHeadID SalaryHeadID
							,sidG.Sequence SalaryHdSequence							
							,sidG.SalaryHead 
							,sidG.HeadCategory
							,sid1.EntryCurrencyID EntryCurrency



                            FROM SalaryInfoDefineMaster sidm
                            LEFT JOIN EmployeeInformation ei ON sidm.EmpInfoSystemID = ei.SystemId
                            LEFT JOIN SalaryRuleMaster srm ON srm.SystemID=sidm.SalaryRuleMasterSystemID
                            --LEFT JOIN SalaryInfoDefineMaster AS sidm2 ON sidm2.EmpInfoSystemID = ei.SystemId 
																		--AND sidm2.SalaryRuleMasterSystemID = srm.SystemID
							INNER JOIN SalaryInfoDefine AS sid1 ON sid1.SalaryID = sidm.SystemID 
							INNER JOIN SalaryHead sidH ON sidh.SalaryHeadID=sid1.SalaryHeadID AND  sidH.HeadCategory='Basic'
											
                            INNER JOIN SalaryInfoDefine AS sidGross ON sidGross.SalaryID = sidm.SystemID 
                            INNER JOIN SalaryHead sidG ON sidG.SalaryHeadID=sidGross.SalaryHeadID AND  sidG.HeadCategory='Gross'
							
		
							LEFT OUTER JOIN SalaryIncrementNextDueDate ND ON nd.EmpSystemId=sidm.EmpInfoSystemID	AND ND.EffectiveDate = sidm.EffectiveDate	
                            LEFT JOIN HKP.EmployeeCategory AS EC ON EI.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON Ei.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON Ei.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON Ei.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section S ON Ei.SectionID = S.Id
                            LEFT JOIN ORG.SubSection SB ON Ei.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON Ei.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON Ei.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON Ei.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON Ei.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=ei.BudgetCode
							WHERE  ei.EmployeeStatus='Active' AND sidm.IsApproved=1  AND ei.PlantId='" + identity.PlantId + @"' 
							AND  ei.SystemId NOT IN (SELECT EmpSystemId FROM ExceptionEmployee)
							AND  ei.SystemId NOT IN (SELECT EmpInfoSystemID  FROM SalaryInfoDefineMaster WHERE IsApproved=0)
							---AND DateDiff(MONTH, sidm.EffectiveDate,'"+ LoadEffectiveDate + @"') >=" + MonthNo + @"
                            AND DateAdd(MONTH," + MonthNo + @",  sidm.EffectiveDate) <= '" + LoadEffectiveDate + @"'
							) AS K WHERE isnull(K.LastDueFlag,0)=1 ";

            var data = _sqlRepository.GetDataCollection(sql);
            //return Json(data, JsonRequestBehavior.AllowGet);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpGet, Authorize]
        public ActionResult GetEmployeeListWithSalaryInfoByJoinDate(string MonthNo, string LoadEffectiveDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * FROM (SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             ei.SystemId 
                            ,ei.EmployeeCode                          
                            ,ei.EmployeeName
                           
                            --,sidm.SystemID
                            , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,S.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,srm.SalaryRuleName
                            ,sid1.EntryAmount Basic
                            ,sid1.EntryAmount BasicOld
                            ,sidGross.EntryAmount Gross
                            ,sidGross.EntryAmount GrossOld
                            ,sidm.SalaryRuleMasterSystemID  
                            ,sidm.SystemID SalaryId
                            ,FORMAT(ei.DOJ,'dd-MMM-yyyy') DOJ
							,FORMAT(sidm.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
							,FORMAT(ND.NextDueDate,'dd-MMM-yyyy') NextDueDate
							,DENSE_RANK() OVER (PARTITION BY sidm.SystemID ORDER BY NextDueDate DESC) AS LastDueFlag
							
							,DateDiff(MONTH, sidm.EffectiveDate,'" + LoadEffectiveDate + @"') diff
							,sidG.SalaryHeadID SalaryHeadID
							,sidG.Sequence SalaryHdSequence							
							,sidG.SalaryHead 
							,sidG.HeadCategory
							,sid1.EntryCurrencyID EntryCurrency



                            FROM SalaryInfoDefineMaster sidm
                            LEFT JOIN EmployeeInformation ei ON sidm.EmpInfoSystemID = ei.SystemId
                            LEFT JOIN SalaryRuleMaster srm ON srm.SystemID=sidm.SalaryRuleMasterSystemID
                            --LEFT JOIN SalaryInfoDefineMaster AS sidm2 ON sidm2.EmpInfoSystemID = ei.SystemId 
																		--AND sidm2.SalaryRuleMasterSystemID = srm.SystemID
							INNER JOIN SalaryInfoDefine AS sid1 ON sid1.SalaryID = sidm.SystemID 
							INNER JOIN SalaryHead sidH ON sidh.SalaryHeadID=sid1.SalaryHeadID AND  sidH.HeadCategory='Basic'
											
                            INNER JOIN SalaryInfoDefine AS sidGross ON sidGross.SalaryID = sidm.SystemID 
                            INNER JOIN SalaryHead sidG ON sidG.SalaryHeadID=sidGross.SalaryHeadID AND  sidG.HeadCategory='Gross'
							
		
							LEFT OUTER JOIN SalaryIncrementNextDueDate ND ON nd.EmpSystemId=sidm.EmpInfoSystemID	AND ND.EffectiveDate = sidm.EffectiveDate	
                            LEFT JOIN HKP.EmployeeCategory AS EC ON EI.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON Ei.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON Ei.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON Ei.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section S ON Ei.SectionID = S.Id
                            LEFT JOIN ORG.SubSection SB ON Ei.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON Ei.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON Ei.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON Ei.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON Ei.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=ei.BudgetCode
							WHERE  ei.EmployeeStatus='Active' AND sidm.IsApproved=1  AND ei.PlantId='" + identity.PlantId + @"' 
							AND  ei.SystemId NOT IN (SELECT EmpSystemId FROM ExceptionEmployee)
							AND  ei.SystemId NOT IN (SELECT EmpInfoSystemID  FROM SalaryInfoDefineMaster WHERE IsApproved=0)
							---AND DateDiff(MONTH, sidm.EffectiveDate,'" + LoadEffectiveDate + @"') >=" + MonthNo + @"
                            AND DateAdd(MONTH," + MonthNo + @",  ei.DOJ) <= '" + LoadEffectiveDate + @"'
							) AS K WHERE isnull(K.LastDueFlag,0)=1 ";

            var data = _sqlRepository.GetDataCollection(sql);
            //return Json(data, JsonRequestBehavior.AllowGet);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }



        [HttpGet, Authorize]
        public ActionResult GetAllEmployeeListWithSalaryInfo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * FROM (SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             ei.SystemId 
                            ,ei.EmployeeCode                          
                            ,ei.EmployeeName
                           
                            --,sidm.SystemID
                            , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,S.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,srm.SalaryRuleName
                            ,sid1.EntryAmount Basic
                            ,sid1.EntryAmount BasicOld
                            ,sidGross.EntryAmount Gross
                            ,sidGross.EntryAmount GrossOld
                            ,sidm.SalaryRuleMasterSystemID  
                            ,sidm.SystemID SalaryId
                            ,FORMAT(ei.DOJ,'dd-MMM-yyyy') DOJ
							,FORMAT(sidm.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
							,FORMAT(ND.NextDueDate,'dd-MMM-yyyy') NextDueDate
							,DENSE_RANK() OVER (PARTITION BY sidm.SystemID ORDER BY NextDueDate DESC) AS LastDueFlag						
							,DateDiff(MONTH, sidm.EffectiveDate,'" + DateTime.Now + @"') diff
							
		                    ,sidG.SalaryHeadID SalaryHeadID
							,sidG.Sequence SalaryHdSequence							
							,sidG.SalaryHead 
							,sidG.HeadCategory
							,sid1.EntryCurrencyID EntryCurrency

                            FROM SalaryInfoDefineMaster sidm
                            LEFT JOIN EmployeeInformation ei ON sidm.EmpInfoSystemID = ei.SystemId
                            LEFT JOIN SalaryRuleMaster srm ON srm.SystemID=sidm.SalaryRuleMasterSystemID
                            --LEFT JOIN SalaryInfoDefineMaster AS sidm2 ON sidm2.EmpInfoSystemID = ei.SystemId 
																		--AND sidm2.SalaryRuleMasterSystemID = srm.SystemID
							INNER JOIN SalaryInfoDefine AS sid1 ON sid1.SalaryID = sidm.SystemID 
							INNER JOIN SalaryHead sidH ON sidh.SalaryHeadID=sid1.SalaryHeadID AND  sidH.HeadCategory='Basic'
											
                            INNER JOIN SalaryInfoDefine AS sidGross ON sidGross.SalaryID = sidm.SystemID 
                            INNER JOIN SalaryHead sidG ON sidG.SalaryHeadID=sidGross.SalaryHeadID AND  sidG.HeadCategory='Gross'
							
		
							LEFT OUTER JOIN SalaryIncrementNextDueDate ND ON nd.EmpSystemId=sidm.EmpInfoSystemID	AND ND.EffectiveDate = sidm.EffectiveDate	
                            LEFT JOIN MST.DesignationMaster dm ON Ei.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=ei.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                            LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=ei.LegalDesignationId
							LEFT JOIN mst.DesignationMaster AS dmmm ON dmmm.Id=dmld.DesignationMasterId
                            LEFT JOIN HKP.EmployeeCategory AS EC ON dmmm.EmployeeCategoryId = EC.Id
                            LEFT JOIN ORG.Unit U ON EN.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON PR.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON PR.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section S ON PR.SectionID = S.Id
                            LEFT JOIN ORG.SubSection SB ON PR.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON MB.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON Ei.GivenDesignationId = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON Ei.LegalDesignationId = ld.Id
							
							WHERE  ei.EmployeeStatus='Active' AND sidm.IsApproved=1  AND ei.PlantId='" + identity.PlantId + @"' 
							AND  ei.SystemId NOT IN (SELECT EmpSystemId FROM ExceptionEmployee)
							AND  ei.SystemId NOT IN (SELECT EmpInfoSystemID  FROM SalaryInfoDefineMaster WHERE IsApproved=0)
							
							) AS K WHERE isnull(K.LastDueFlag,0)=1 ";

            var data = _sqlRepository.GetDataCollection(sql);
            //return Json(data, JsonRequestBehavior.AllowGet);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


       // [HttpGet, Authorize]
       // public ActionResult xGetAllIncrementedEmployeeListWithSalaryInfo()
       // {
       //     var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
       //     string sql = @" SELECT * FROM (SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
       //                      ei.SystemId 
       //                     ,ei.EmployeeCode                          
       //                     ,ei.EmployeeName
                           
       //                     --,sidm.SystemID
       //                     , EC.UserName EmpCategoryName  
       //                     ,ld.UserName Designation
       //                     ,U.UserName Unit 
       //                     ,Dv.UserName Division
       //                     ,Dp.UserName Department
       //                     ,S.UserName Section 
       //                     ,SB.UserName SubSection 
       //                     ,L.UserName Line
       //                     ,srm.SalaryRuleName
       //                     ,sid1.EntryAmount Basic
       //                     ,sidBasicA.EntryAmount BasicOld
       //                     ,sidGross.EntryAmount Gross
       //                     ,sidGrossA.EntryAmount GrossOld
       //                     ,sidm.SalaryRuleMasterSystemID  
       //                     ,sidm.SystemID SalaryId
       //                     ,FORMAT(ei.DOJ,'dd-MMM-yyyy') DOJ
							//,FORMAT(sidmA.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
	      //                  ,FORMAT(sidm.EffectiveDate,'dd-MMM-yyyy') EffectiveDateNew
							//,FORMAT(ND.NextDueDate,'dd-MMM-yyyy') NextDueDate
							//,DENSE_RANK() OVER (PARTITION BY sidm.SystemID ORDER BY NextDueDate DESC) AS LastDueFlag							
							//,DateDiff(MONTH, sidm.EffectiveDate,'" + DateTime.Now + @"') diff

							//,sidG.SalaryHeadID SalaryHeadID
							//,sidG.Sequence SalaryHdSequence							
							//,sidG.SalaryHead 
							//,sidG.HeadCategory
							//,sid1.EntryCurrencyID EntryCurrency

       //                     FROM SalaryInfoDefineMaster sidm
       //                     LEFT JOIN EmployeeInformation ei ON sidm.EmpInfoSystemID = ei.SystemId
       //                     LEFT JOIN SalaryRuleMaster srm ON srm.SystemID=sidm.SalaryRuleMasterSystemID
             
							//INNER JOIN SalaryInfoDefine AS sid1 ON sid1.SalaryID = sidm.SystemID 
							//INNER JOIN SalaryHead sidH ON sidh.SalaryHeadID=sid1.SalaryHeadID AND  sidH.HeadCategory='Basic'
											
       //                     INNER JOIN SalaryInfoDefine AS sidGross ON sidGross.SalaryID = sidm.SystemID 
       //                     INNER JOIN SalaryHead sidG ON sidG.SalaryHeadID=sidGross.SalaryHeadID AND  sidG.HeadCategory='Gross'
                            
       //                     LEFT JOIN  SalaryInfoDefineMaster sidmA ON sidmA.EmpInfoSystemID = ei.SystemId AND sidmA.IsApproved=1
							//INNER JOIN SalaryInfoDefine AS sidBasicA ON sidBasicA.SalaryID = sidmA.SystemID 
							//INNER JOIN SalaryHead sidHA ON sidHA.SalaryHeadID=sidBasicA.SalaryHeadID AND  sidHA.HeadCategory='Basic'
											
       //                     INNER JOIN SalaryInfoDefine AS sidGrossA ON sidGrossA.SalaryID = sidmA.SystemID 
       //                     INNER JOIN SalaryHead sidGA ON sidGA.SalaryHeadID=sidGrossA.SalaryHeadID AND  sidGA.HeadCategory='Gross'
		
							//LEFT OUTER JOIN SalaryIncrementNextDueDate ND ON nd.EmpSystemId=sidm.EmpInfoSystemID	AND ND.EffectiveDate = sidm.EffectiveDate	
       //                     LEFT JOIN HKP.EmployeeCategory AS EC ON EI.EmployeeCategorySystemID = EC.Id
       //                     LEFT JOIN ORG.Unit U ON Ei.UnitID = U.Id
       //                     LEFT JOIN ORG.Division Dv ON Ei.DivisionID = Dv.Id
       //                     LEFT JOIN ORG.Department Dp ON Ei.DepartmentID = Dp.Id
       //                     LEFT JOIN ORG.Section S ON Ei.SectionID = S.Id
       //                     LEFT JOIN ORG.SubSection SB ON Ei.SubSectionID = SB.Id
       //                     LEFT JOIN ORG.Line L ON Ei.LineID = L.Id
       //                     LEFT JOIN HKP.Designation D ON Ei.DesignationSystemID = D.Id
       //                     LEFT JOIN HKP.LegalDesignation AS ld  ON Ei.LegalDesignationId = ld.Id
							//LEFT JOIN MST.DesignationMaster dm ON Ei.GivenDesignationId = dm.DesignationId
							//LEFT JOIN MST.ManpowerBudget mb ON mb.Id=ei.BudgetCode
							//WHERE  ei.EmployeeStatus='Active' AND sidm.IsApproved=0  AND ei.PlantId='" + identity.PlantId + @"' 
							//AND  ei.SystemId NOT IN (SELECT EmpSystemId FROM ExceptionEmployee)
							//AND  ei.SystemId  IN (SELECT EmpInfoSystemID  FROM SalaryInfoDefineMaster WHERE IsApproved=1)
							
							//) AS K WHERE isnull(K.LastDueFlag,0)=1 ";

       //     var data = _sqlRepository.GetDataCollection(sql);
       //     return Json(data, JsonRequestBehavior.AllowGet);
       // }

        //with back
        [HttpGet]
        public ActionResult GetAllIncrementedEmployeeListWithSalaryInfo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * FROM (SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             ei.SystemId 
                            ,ei.EmployeeCode                          
                            ,ei.EmployeeName
                           
                            --,sidm.SystemID
                            , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,S.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,srm.SalaryRuleName
                            ,sid1.EntryAmount Basic
                            ,sidBasicA.EntryAmount BasicOld
                            ,sidGross.EntryAmount Gross
                            ,sidGrossA.EntryAmount GrossOld
                            ,sidm.SalaryRuleMasterSystemID  
                            ,sidm.SystemID SalaryId
                            ,FORMAT(ei.DOJ,'dd-MMM-yyyy') DOJ
							,FORMAT(sidmA.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
	                        ,FORMAT(sidm.EffectiveDate,'dd-MMM-yyyy') EffectiveDateNew
							,FORMAT(ND.NextDueDate,'dd-MMM-yyyy') NextDueDate
							,DENSE_RANK() OVER (PARTITION BY sidm.SystemID ORDER BY NextDueDate DESC) AS LastDueFlag							
							,DateDiff(MONTH, sidm.EffectiveDate,'" + DateTime.Now + @"') diff

							,sidG.SalaryHeadID SalaryHeadID
							,sidG.Sequence SalaryHdSequence							
							,sidG.SalaryHead 
							,sidG.HeadCategory
							,sid1.EntryCurrencyID EntryCurrency

                            FROM SalaryInfoDefineMaster sidm
                            LEFT JOIN EmployeeInformation ei ON sidm.EmpInfoSystemID = ei.SystemId
                            LEFT JOIN SalaryRuleMaster srm ON srm.SystemID=sidm.SalaryRuleMasterSystemID
             
							INNER JOIN SalaryInfoDefine AS sid1 ON sid1.SalaryID = sidm.SystemID 
							INNER JOIN SalaryHead sidH ON sidh.SalaryHeadID=sid1.SalaryHeadID AND  sidH.HeadCategory='Basic'
											
                            INNER JOIN SalaryInfoDefine AS sidGross ON sidGross.SalaryID = sidm.SystemID 
                            INNER JOIN SalaryHead sidG ON sidG.SalaryHeadID=sidGross.SalaryHeadID AND  sidG.HeadCategory='Gross'
                            
                                                  --------------------------------------
                            LEFT JOIN  (
                            -----------new	---
                            SELECT  m.EffectiveDate,	m.EmpInfoSystemID	,	m.systemid FROM 
								(SELECT  EffectiveDate,EmpInfoSystemID,systemid from SalaryInfoDefineMaster
								union
								SELECT   EffectiveDate,EmpInfoSystemID,systemid from SalaryInfobackMaster
								)
								 m 
								INNER JOIN (

								SELECT MAX( EffectiveDate) EffectiveDate,EmpInfoSystemID FROM (
								SELECT  EffectiveDate,EmpInfoSystemID
								  FROM SalaryInfoDefineMaster WHERE IsApproved=1 --AND EmpInfoSystemID=1800009
								UNION 
								SELECT MAX( EffectiveDate) EffectiveDate,EmpInfoSystemID FROM SalaryInfobackMaster WHERE IsApproved=1 ---AND EmpInfoSystemID=1800009 
								GROUP BY EmpInfoSystemID
								) d GROUP BY d.EmpInfoSystemID) dd
								ON m.EffectiveDate=dd.EffectiveDate AND m.EmpInfoSystemID=dd.EmpInfoSystemID
                            
                            -----------new	---
                            ) sidmA ON sidmA.EmpInfoSystemID = ei.SystemId --AND sidmA.IsApproved=1
							INNER JOIN (
								SELECT SystemID,	SalaryID,	SalaryHeadID,	EntryCurrencyID,	EntryAmount,	DefineCurrencyID,	DefineAmount,	AmtDefinitionCurrencyID,	AmtDefinitionRate,	AddedBy,	DateAdded,	UpdatedBy,	DateUpdated,	SequenceNo,	SalaryCategory FROM SalaryInfoDefine 
								UNION
							    SELECT SystemID,	SalaryID,	SalaryHeadID,	EntryCurrencyID,	EntryAmount,	DefineCurrencyID,	DefineAmount,	AmtDefinitionCurrencyID,	AmtDefinitionRate,	AddedBy,	DateAdded,	UpdatedBy,	DateUpdated,	SequenceNo,	SalaryCategory FROM SalaryInfoBack 
							) AS sidBasicA ON sidBasicA.SalaryID = sidmA.SystemID 
							INNER JOIN SalaryHead sidHA ON sidHA.SalaryHeadID=sidBasicA.SalaryHeadID AND  sidHA.HeadCategory='Basic'
											
                            INNER JOIN (
                            	SELECT SystemID,	SalaryID,	SalaryHeadID,	EntryCurrencyID,	EntryAmount,	DefineCurrencyID,	DefineAmount,	AmtDefinitionCurrencyID,	AmtDefinitionRate,	AddedBy,	DateAdded,	UpdatedBy,	DateUpdated,	SequenceNo,	SalaryCategory FROM SalaryInfoDefine 
								UNION
							    SELECT SystemID,	SalaryID,	SalaryHeadID,	EntryCurrencyID,	EntryAmount,	DefineCurrencyID,	DefineAmount,	AmtDefinitionCurrencyID,	AmtDefinitionRate,	AddedBy,	DateAdded,	UpdatedBy,	DateUpdated,	SequenceNo,	SalaryCategory FROM SalaryInfoBack 
                            ) AS sidGrossA ON sidGrossA.SalaryID = sidmA.SystemID 
                            INNER JOIN SalaryHead sidGA ON sidGA.SalaryHeadID=sidGrossA.SalaryHeadID AND  sidGA.HeadCategory='Gross'
		                    --------------------------------------------
		
							LEFT OUTER JOIN SalaryIncrementNextDueDate ND ON nd.EmpSystemId=sidm.EmpInfoSystemID	AND ND.EffectiveDate = sidm.EffectiveDate
                            LEFT JOIN MST.ManpowerBudget mb ON mb.Id = EI.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                            LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationID
                            LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                            LEFT JOIN ORG.Unit U ON EN.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON PR.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON PR.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section S ON PR.SectionID = S.Id
                            LEFT JOIN ORG.SubSection SB ON PR.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON MB.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON Ei.GivenDesignationID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON Ei.LegalDesignationId = ld.Id
							WHERE  ei.EmployeeStatus='Active' AND sidm.IsApproved=0  AND ei.PlantId='" + identity.PlantId + @"' 
							AND  ei.SystemId NOT IN (SELECT EmpSystemId FROM ExceptionEmployee)
							--AND  ei.SystemId  IN (SELECT EmpInfoSystemID  FROM SalaryInfoDefineMaster WHERE IsApproved=1)
							
							) AS K WHERE isnull(K.LastDueFlag,0)=1 ";

            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }




       // [HttpGet, Authorize]
       // public ActionResult xGetEmployeeListWithSalaryInfo()
       // {
       //     var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
       //     string sql = @"  SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
       //                      ei.SystemId 
       //                     ,ei.EmployeeCode                          
       //                     ,ei.EmployeeName
       //                     --,sidm.SystemID
       //                     , EC.UserName EmpCategoryName  
       //                     ,ld.UserName Designation
       //                     ,U.UserName Unit 
       //                     ,Dv.UserName Division
       //                     ,Dp.UserName Department
       //                     ,S.UserName Section 
       //                     ,SB.UserName SubSection 
       //                     ,L.UserName Line
       //                     ,srm.SalaryRuleName
       //                     ,sid1.EntryAmount Basic
       //                     ,sid1.EntryAmount BasicOld
       //                     ,sidGross.EntryAmount Gross
       //                     ,sidGross.EntryAmount GrossOld
       //                     ,sidm2.SalaryRuleMasterSystemID  
       //                     ,sidm.SystemID SalaryId
       //                     FROM SalaryInfoDefineMaster sidm
       //                     LEFT JOIN EmployeeInformation ei ON sidm.EmpInfoSystemID = ei.SystemId
       //                     LEFT JOIN SalaryRuleMaster srm ON srm.SystemID=sidm.SalaryRuleMasterSystemID
       //                     LEFT JOIN SalaryInfoDefineMaster AS sidm2 ON sidm2.EmpInfoSystemID = ei.SystemId 
							//											AND sidm2.SalaryRuleMasterSystemID = srm.SystemID
							//INNER JOIN SalaryInfoDefine AS sid1 ON sid1.SalaryID = sidm2.SystemID 
							//					AND sid1.SalaryHeadID =( SELECT SalaryHeadID  FROM SalaryHead WHERE HeadCategory='Basic'	)	
       //                     LEFT JOIN SalaryInfoDefine AS sidGross ON sidGross.SalaryID = sidm2.SystemID 
							//					AND sidGross.SalaryHeadID =( SELECT SalaryHeadID  FROM SalaryHead WHERE HeadCategory='Gross'	)
       //                     LEFT JOIN HKP.EmployeeCategory AS EC ON EI.EmployeeCategorySystemID = EC.Id
       //                     LEFT JOIN ORG.Unit U ON Ei.UnitID = U.Id
       //                     LEFT JOIN ORG.Division Dv ON Ei.DivisionID = Dv.Id
       //                     LEFT JOIN ORG.Department Dp ON Ei.DepartmentID = Dp.Id
       //                     LEFT JOIN ORG.Section S ON Ei.SectionID = S.Id
       //                     LEFT JOIN ORG.SubSection SB ON Ei.SubSectionID = SB.Id
       //                     LEFT JOIN ORG.Line L ON Ei.LineID = L.Id
       //                     LEFT JOIN HKP.Designation D ON Ei.DesignationSystemID = D.Id
       //                     LEFT JOIN HKP.LegalDesignation AS ld  ON Ei.LegalDesignationId = ld.Id
							//LEFT JOIN MST.DesignationMaster dm ON Ei.GivenDesignationId = dm.DesignationId
							//LEFT JOIN MST.ManpowerBudget mb ON mb.Id=ei.BudgetCode
							//WHERE  ei.EmployeeStatus='Active' AND sidm.IsApproved=1  AND ei.PlantId='" + identity.PlantId + @"' AND  ei.SystemId NOT IN (SELECT EmpSystemId FROM ExceptionEmployee)";

       //     var data = _sqlRepository.GetDataCollection(sql);
       //     return Json(data, JsonRequestBehavior.AllowGet);
       // }


        [HttpPost, Authorize]
        public JsonResult Calculate( List<CustomParaBulkIncrement> BulkIncrement)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            clsBulkIncrement ob = new clsBulkIncrement();
            List<CustomParaBulkIncrement> data=   ob.CalculateclsBulkIncrementValue(BulkIncrement, identity.PlantId);
            return Json(new { data, Message = AplosMessage.Success });
           

        }


        [HttpPost]
        public JsonResult Save(List<CustomParaBulkIncrement> BulkIncrement, ParaDateBulkIncrement custompara)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            clsBulkIncrement ob = new clsBulkIncrement();
            ob.SaveBulkIncrement(BulkIncrement, custompara, identity);
            return Json(new { Message = AplosMessage.Success });

        }


















       

      

        #endregion -- Operations

    }

   
}