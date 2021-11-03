using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI.WebControls;

namespace OTSBD
{
    public class xxclsSalaryProc4
    {
        public xxclsSalaryProc4()
        {
            // TODO: Add constructor logic here
        }

        public void GetEmployeeWisePFValueAfterCal(string sEmpInfo, string sDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT FC.EmpSystemID, E.PlantId, FC.SalaryRuleMasterSystemID, FC.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, FC.SlrCate, FC.PFMntEmpWiseCalID, 
									FC.PFEligibleEmpID, FC.IsDistribution, FC.ContributionAmount, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
						    FROM
						    (
							    SELECT * FROM
								    (
									    SELECT PFE.EmpSystemID, PFSlrHd.SalaryHeadID, SEFD.SalaryRuleMasterSystemID, PMC.PFMntEmpWiseCalID, PMC.PFEligibleEmpID, PMC.IsDistribution, PMC.ContributionAmount, PMC.SlrCate
									    FROM PFEligibleEmployee PFE
										    INNER JOIN (
													    SELECT SLM.* FROM 
															    (
																    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																	    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																    FROM SalaryInfoDefineMaster
																    UNION 
																	    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																			    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																	    )
													    ) SLM 
														    INNER JOIN
																    (
																	    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
																	    FROM 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoDefineMaster
																			    UNION 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																		    )
																		    ) A
																	    WHERE IsApproved = 1 AND EffectiveDate <= '" + sDate + @"'
																	    GROUP BY EmpInfoSystemID
																    ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
													    ) SEFD ON PFE.EmpSystemID = SEFD.EmpInfoSystemID
										    INNER JOIN (
													    SELECT *, 'PF Employee Contribution' SlrCate FROM SalaryRulePF WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'PF Employee Contribution')
													    UNION
													    (
													    SELECT *, 'PF Employer Contribution' SlrCate FROM SalaryRulePF WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'PF Employer Contribution')
													    ) 
													    ) PFSlrHd ON SEFD.SalaryRuleMasterSystemID = PFSlrHd.SalaryRuleMasterSystemID 
										    INNER JOIN (
													    SELECT ID PFMntEmpWiseCalID, PFEligibleEmpID, IsDistributionEmp IsDistribution, EmployeeContributionAmount ContributionAmount, 'PF Employee Contribution' SlrCate 
														    FROM [dbo].[PFMonthlyEmpWiseCalculation]
													    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
													    UNION
														    (
														    SELECT ID PFMntEmpWiseCalID, PFEligibleEmpID, IsDistributionEmpr IsDistribution, EmployerContributionAmount ContributionAmount, 'PF Employer Contribution' SlrCate 
															    FROM [dbo].[PFMonthlyEmpWiseCalculation]
															    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
														    )
													    ) PMC  ON PMC.PFEligibleEmpID = PFE.ID AND PFSlrHd.SlrCate = PMC.SlrCate
								    ) AB
								    UNION
								    (
								    SELECT PFE.EmpSystemID, D.SalaryHeadID, SEFD.SalaryRuleMasterSystemID, C.ID PFMntEmpWiseCalID, C.PFEligibleEmpID, CONVERT(BIT, 'False') IsDistribution, D.Amount ContributionAmount, '' SlrCate 
									    FROM [dbo].[PFMonthlyEmpWiseCalculation] C
											    INNER JOIN PFEligibleEmployee PFE ON C.PFEligibleEmpID = PFE.ID
											    INNER JOIN (
														    SELECT SLM.* FROM 
																    (
																	    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																		    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																	    FROM SalaryInfoDefineMaster
																	    UNION 
																		    (
																			    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																			    FROM SalaryInfoBackMaster
																		    )
																    ) SLM 
																	    INNER JOIN
																			    (
																				    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
																				    FROM 
																					    (
																					    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																							    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																					    FROM SalaryInfoDefineMaster
																						    UNION 
																					    (
																					    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																							    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																					    FROM SalaryInfoBackMaster
																					    )
																					    ) A
																				    WHERE IsApproved = 1 AND EffectiveDate <= '" + sDate + @"'
																				    GROUP BY EmpInfoSystemID
																			    ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
													    ) SEFD ON PFE.EmpSystemID = SEFD.EmpInfoSystemID
											    INNER JOIN
													    (
														    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM(Amount) Amount FROM
																					    (
																						    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM([Value]) Amount FROM [dbo].[PFMonthlyDistributionEmployee]
																						    WHERE ISNULL(SalaryHeadID, '') != '' AND [Value] > 0
																						    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
																						    UNION
																						    (SELECT PFMntEmpWiseCalID, ResidualValueSlrHdID SalaryHeadID, SUM([UpperLimit]) Amount FROM [dbo].[PFMonthlyDistributionEmployee]
																						    WHERE ISNULL(ResidualValueSlrHdID, '') != '' AND [UpperLimit] > 0
																						    GROUP BY PFMntEmpWiseCalID, ResidualValueSlrHdID)
																					    ) A 
																					    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
														    UNION
														    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM(Amount) Amount FROM
																					    (
																						    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM([Value]) Amount FROM [dbo].[PFMonthlyDistributionEmployer]
																						    WHERE ISNULL(SalaryHeadID, '') != '' AND [Value] > 0
																						    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
																						    UNION
																						    (SELECT PFMntEmpWiseCalID, ResidualValueSlrHdID SalaryHeadID, SUM([UpperLimit]) Amount FROM [dbo].[PFMonthlyDistributionEmployer]
																						    WHERE ISNULL(ResidualValueSlrHdID, '') != '' AND [UpperLimit] > 0
																						    GROUP BY PFMntEmpWiseCalID, ResidualValueSlrHdID)
																					    ) A 
																					    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
													    ) D ON D.PFMntEmpWiseCalID = C.ID 
														    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
								    )
						    ) FC 
								INNER JOIN EmployeeInformation E ON FC.EmpSystemID = E.SystemId
							    INNER JOIN SalaryHead SH ON FC.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' --AND ISNULL(SH.HeadCategory, '') != 'PF Voluntary'
							    INNER JOIN SalaryRuleMaster SRM ON FC.SalaryRuleMasterSystemID = SRM.SystemID
							    LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND FC.SalaryHeadID = CRC.SalaryHeadID
							    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
							    LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
							    LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id	
						    WHERE FC.IsDistribution = 0
								  ------AND FC.SalaryHeadID NOT IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'PF Voluntary')
								  AND 
                                  FC.EmpSystemID IN (" + sEmpInfo + @") 
						    ORDER BY FC.EmpSystemID";

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
        public void GetEmployeeWiseESICFValueAfterCal(string sEmpInfo, string sDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT FC.*, E.PlantId, SH.SalaryHead, SH.HeadType, SH.HeadCategory, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
						    FROM
						    (
							    SELECT * FROM
								    (
									    SELECT ESICE.EmpSystemID, ESICSlrHd.SalaryHeadID, SEFD.SalaryRuleMasterSystemID, PMC.ESICMntEmpWiseCalID, PMC.ESICEligibleEmpID, PMC.ContributionAmount, PMC.SlrCate
									    FROM ESICEligibleEmployee ESICE
										    INNER JOIN (
													    SELECT SLM.* FROM 
															    (
																    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																	    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																    FROM SalaryInfoDefineMaster
																    UNION 
																	    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																			    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																	    )
													    ) SLM 
														    INNER JOIN
																    (
																	    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
																	    FROM 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoDefineMaster
																			    UNION 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																		    )
																		    ) A
																	    WHERE IsApproved = 1 AND EffectiveDate <= '" + sDate + @"'
																	    GROUP BY EmpInfoSystemID
																    ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
													    ) SEFD ON ESICE.EmpSystemID = SEFD.EmpInfoSystemID
										    INNER JOIN (
													    SELECT *, 'ESIC Employee Contribution' SlrCate FROM SalaryRuleESIC WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'ESIC Employee Contribution')
													    UNION
													    (
													    SELECT *, 'ESIC Employer Contribution' SlrCate FROM SalaryRuleESIC WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'ESIC Employer Contribution')
													    ) 
													    ) ESICSlrHd ON SEFD.SalaryRuleMasterSystemID = ESICSlrHd.SalaryRuleMasterSystemID 
										    INNER JOIN (
													    SELECT ID ESICMntEmpWiseCalID, ESICEligibleEmpID, EmployeeContributionAmount ContributionAmount, 'ESIC Employee Contribution' SlrCate 
														    FROM [dbo].[ESICMonthlyEmpWiseCalculation]
													    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
													    UNION
														    (
														    SELECT ID ESICMntEmpWiseCalID, ESICEligibleEmpID, EmployerContributionAmount ContributionAmount, 'ESIC Employer Contribution' SlrCate 
															    FROM [dbo].[ESICMonthlyEmpWiseCalculation]
															    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
														    )
													    ) PMC  ON PMC.ESICEligibleEmpID = ESICE.ID AND ESICSlrHd.SlrCate = PMC.SlrCate
								    ) AB
						    ) FC 
								INNER JOIN EmployeeInformation E ON FC.EmpSystemID = E.SystemId
							    INNER JOIN SalaryHead SH ON FC.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') != 'ESIC Voluntary'
							    INNER JOIN SalaryRuleMaster SRM ON FC.SalaryRuleMasterSystemID = SRM.SystemID
							    LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND FC.SalaryHeadID = CRC.SalaryHeadID
							    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
							    LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
							    LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id	
						    WHERE FC.EmpSystemID IN (" + sEmpInfo + @") 
						    ORDER BY FC.EmpSystemID";

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
        public void GetESICStructureData(string sEmpInfo, string sDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT FC.*, SH.SalaryHead, SH.HeadType, SH.HeadCategory, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,0.0 ContributionAmount 
						    FROM
						    (
							    SELECT * FROM
								    (
									    SELECT E.SystemID EmpSystemID, ESICSlrHd.SalaryHeadID, SEFD.SalaryRuleMasterSystemID--, PMC.ESICMntEmpWiseCalID, PMC.ESICEligibleEmpID, PMC.ContributionAmount
										, ESICSlrHd.SlrCate,E.PlantId
									    FROM EmployeeInformation E
										    INNER JOIN (
													    SELECT SLM.* FROM 
															    (
																    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																	    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																    FROM SalaryInfoDefineMaster
																    UNION 
																	    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																			    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																	    )
													    ) SLM 
														    INNER JOIN
																    (
																	    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
																	    FROM 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoDefineMaster
																			    UNION 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																		    )
																		    ) A
																	    WHERE IsApproved = 1 AND EffectiveDate <= '" + sDate + @"'
																	    GROUP BY EmpInfoSystemID
																    ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
													    ) SEFD ON E.SystemID = SEFD.EmpInfoSystemID
										    INNER JOIN (
													    SELECT *, 'ESIC Employee Contribution' SlrCate FROM SalaryRuleESIC WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'ESIC Employee Contribution')
													    UNION
													    (
													    SELECT *, 'ESIC Employer Contribution' SlrCate FROM SalaryRuleESIC WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'ESIC Employer Contribution')
													    ) 
													    ) ESICSlrHd ON SEFD.SalaryRuleMasterSystemID = ESICSlrHd.SalaryRuleMasterSystemID 
										   
								    ) AB
						    ) FC 
							    INNER JOIN SalaryHead SH ON FC.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') != 'ESIC Voluntary'
							    INNER JOIN SalaryRuleMaster SRM ON FC.SalaryRuleMasterSystemID = SRM.SystemID
							    LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND FC.SalaryHeadID = CRC.SalaryHeadID
							    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
							    LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
							    LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id	
						    WHERE FC.EmpSystemID IN (" + sEmpInfo + @") 
						    ORDER BY FC.EmpSystemID";

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
        public void LoadEmpSlrDefForSlrProcessList(string sPlantID, string sEmpInfo, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM 
		                            (
                                     SELECT SD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                                            SD.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate,	
                                            SD.EntryCurrencyID, ECR.Name AS EntryCurrency, SD.EntryAmount, SD.DefineCurrencyID, SD.SalaryID, SRM.CurrencyRuleSystemID,
                                            DECR.Name AS DefinitionCurrency, ISNULL(SD.DefineAmount,0) DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                              ELSE SD.SalaryHeadID END,
			                                CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
			                                SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay

                                            , ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
											IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
																  ELSE SlrDis.IsNetPayEffect END, 
											SlrDis.FormulaDesID, ISNULL(SlrDis.BaseOnNetPay, Convert(bit, 'FALSE')) BaseOnNetPay, ISNULL(SlrDis.RefAbsentism, Convert(bit, 'FALSE')) RefAbsentism, 
											ISNULL(SlrDis.IsGNRBaseOthSlrHD, Convert(bit, 'FALSE')) IsGNRBaseOthSlrHD, SlrDis.GNRBaseOthSlrHDFormula, SlrDis.GNRApplicableMonthNo,
											SlrDis.IsRetain, SlrDis.IsMinWages, SD.SequenceNo, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                            ISNULL(SlrDis.IsWorkDaysInAMonthIncHold, 0) IsWorkDaysInAMonthIncHold, SD.SalaryCategory 

                                            --==================
                                            ,   ISNULL(SlrDis.HasMaxLimit, Convert(bit, 'FALSE')) HasMaxLimit
                                            ,	ISNULL(SlrDis.FixedMaxLimit, Convert(bit, 'FALSE')) FixedMaxLimit
                                            ,	ISNULL(SlrDis.PercentageMaxLimit, Convert(bit, 'FALSE')) PercentageMaxLimit
                                            ,	isnull(SlrDis.MaxLimitValue,0) MaxLimitValue,SlrDis.PercentageMaxLimitSalaryHeadId	

                                            ,   ISNULL(SlrDis.HasMinLimit, Convert(bit, 'FALSE')) HasMinLimit
                                            ,	ISNULL(SlrDis.FixedMinLimit, Convert(bit, 'FALSE')) FixedMinLimit
                                            ,	ISNULL(SlrDis.PercentageMinLimit, Convert(bit, 'FALSE')) PercentageMinLimit
                                            ,	isnull(SlrDis.MinLimitValue,0) MinLimitValue,SlrDis.PercentageMinLimitSalaryHeadId	

											--,SlrDis.HasMinLimit,	SlrDis.FixedMinLimit,  SlrDis.PercentageMinLimit,	SlrDis.MinLimitValue,SlrDis.PercentageMinLimitSalaryHeadId
                                            --==================================
		                            FROM (
                                          SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
	                                             AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated, SequenceNo, SalaryCategory
                                          FROM SalaryInfoDefine
                                            UNION
                                          (
                                           SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
		                                          AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated, SequenceNo, SalaryCategory                   
                                           FROM SalaryInfoBack
                                          )
                                         ) SD
										INNER JOIN 
												(
												 SELECT SLM.* FROM 
                                                            (
                                                             SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
		                                                            IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                                                             FROM SalaryInfoDefineMaster
                                                             UNION 
                                                            (
                                                             SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
		                                                            IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                                                             FROM SalaryInfoBackMaster
                                                            )
                                                            ) SLM 
	                                                            INNER JOIN
			                                                            (
			                                                             SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
			                                                             FROM 
				                                                             (
				                                                               SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
						                                                              IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
				                                                               FROM SalaryInfoDefineMaster
						                                                           UNION 
				                                                              (
					                                                            SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
							                                                           IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
					                                                            FROM SalaryInfoBackMaster
				                                                              )
				                                                             ) A
				                                                            WHERE IsApproved = 1 AND EffectiveDate <= '" + sToDate + @"'
				                                                            GROUP BY EmpInfoSystemID
			                                                            ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
												) SEFD ON SD.SalaryID = SEFD.SystemID
			                            INNER JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
			                            INNER JOIN SalaryHead SH ON SD.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') != 'PF Voluntary'
			                            INNER JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
			                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SD.SalaryHeadID = CRC.SalaryHeadID
			                            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
			                            LEFT JOIN 
					                            (
                                                 SELECT SalaryRuleMasterSystemID, g.SalaryHeadID, 'Gen' RuleType, h.PartOfNetPay IsNetPayEffect, FixedMonthDayValue, IsMonthDay,  
						                                IsMonthWorkDay, IsFixedDisbus, BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, GNRApplicableMonthNo,                                                        
                                                        FormulaDesID, IsRetain, IsMinWages ,IsWorkDaysInAMonthIncHold

                                                    ,HasMaxLimit,	FixedMaxLimit,	PercentageMaxLimit,	MaxLimitValue,
												   PercentageMaxLimitSalaryHeadId,	
												   HasMinLimit,	FixedMinLimit,  PercentageMinLimit,	MinLimitValue,	
												   PercentageMinLimitSalaryHeadId

												   FROM SalaryRuleGeneral  g
												   left join SalaryHead h on h.SalaryHeadID=g.SalaryHeadID
						                            UNION
					                             (
                                                  SELECT SalaryRuleMasterSystemID, g.SalaryHeadID, 'Abs' RuleType, h.PartOfNetPay IsNetPayEffect, FixedMonthDayValue, IsMonthDay, 
						                                 IsMonthWorkDay, IsFixedDisbus, Convert(bit, isnull(BaseOnNetPay,0)) BaseOnNetPay, Convert(bit, 'FALSE') RefAbsentism, Convert(bit, 'FALSE') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, '' GNRApplicableMonthNo,
                                                         FormulaDesID, Convert(bit, 'FALSE') IsRetain, Convert(bit, 'FALSE') IsMinWages  ,Convert(bit, 'FALSE') IsWorkDaysInAMonthIncHold

                                                    ,Convert(bit, 'FALSE') HasMaxLimit,	Convert(bit, 'FALSE') FixedMaxLimit,	Convert(bit, 'FALSE') PercentageMaxLimit, 0	MaxLimitValue,
												   '' PercentageMaxLimitSalaryHeadId,	
												   Convert(bit, 'FALSE') HasMinLimit,	Convert(bit, 'FALSE') FixedMinLimit,  Convert(bit, 'FALSE') PercentageMinLimit,	0 MinLimitValue,	
												   '' PercentageMinLimitSalaryHeadId

												   FROM SalaryRuleAbsenteeism  g
												   left join SalaryHead h on h.SalaryHeadID=g.SalaryHeadID
                                                 )
                                                ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SD.SalaryHeadID = SlrDis.SalaryHeadID
			                            LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
											                            AND SD.SalaryHeadID = SRDSM.SalaryHeadID
                                        WHERE E.DOJ <= '" + sToDate + @"' AND (E.DOS >= '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
                                                                               OR E.DOS = '' OR E.DOS = '01/01/1901')
                                              AND SEFD.IsApproved = 1 AND SEFD.EffectiveDate <= '" + sToDate + @"'
                                    ) A 
                                  WHERE (" + sEmpInfo + @") ";
                if (sPlantID != "ALL" & sPlantID != "")
                {
                    strSql += @"
                                AND PlantID = '" + sPlantID + @"' ";
                }

                strSql += @"
                            ORDER BY EmpInfoSystemID, SequenceNo, HeadType DESC";

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
        public void LoadEmpSlrDefForSlrProcess(string sPlantID, string sEmpInfo, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM 
		                            (
                                     SELECT SD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                                            SD.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate,	
                                            SD.EntryCurrencyID, ECR.Name AS EntryCurrency, SD.EntryAmount, SD.DefineCurrencyID, SD.SalaryID, SRM.CurrencyRuleSystemID,
                                            DECR.Name AS DefinitionCurrency, ISNULL(SD.DefineAmount,0) DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                              ELSE SD.SalaryHeadID END,
			                                CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
			                                SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay, ISNULL(SlrDis.IsBankPayment, 'TRUE') IsBankPayment, 
                                            ISNULL(SlrDis.IsCashPayment, 'TRUE') IsCashPayment, ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
											IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
																  ELSE SlrDis.IsNetPayEffect END, 
											SlrDis.FormulaDesID, ISNULL(SlrDis.BaseOnNetPay, Convert(bit, 'FALSE')) BaseOnNetPay, ISNULL(SlrDis.RefAbsentism, Convert(bit, 'FALSE')) RefAbsentism, 
											ISNULL(SlrDis.IsGNRBaseOthSlrHD, Convert(bit, 'FALSE')) IsGNRBaseOthSlrHD, SlrDis.GNRBaseOthSlrHDFormula, SlrDis.GNRApplicableMonthNo,
											SlrDis.IsRetain, SlrDis.IsMinWages, SD.SequenceNo, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                            ISNULL(SlrDis.IsWorkDaysInAMonthIncHold, 0) IsWorkDaysInAMonthIncHold, SD.SalaryCategory 
		                            FROM (
                                          SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
	                                             AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated, SequenceNo, SalaryCategory
                                          FROM SalaryInfoDefine
                                            UNION
                                          (
                                           SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
		                                          AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated, SequenceNo, SalaryCategory                   
                                           FROM SalaryInfoBack
                                          )
                                         ) SD
										INNER JOIN 
												(
												 SELECT SLM.* FROM 
                                                            (
                                                             SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
		                                                            IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                                                             FROM SalaryInfoDefineMaster
                                                             UNION 
                                                            (
                                                             SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
		                                                            IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                                                             FROM SalaryInfoBackMaster
                                                            )
                                                            ) SLM 
	                                                            INNER JOIN
			                                                            (
			                                                             SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
			                                                             FROM 
				                                                             (
				                                                               SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
						                                                              IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
				                                                               FROM SalaryInfoDefineMaster
						                                                           UNION 
				                                                              (
					                                                            SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
							                                                           IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
					                                                            FROM SalaryInfoBackMaster
				                                                              )
				                                                             ) A
				                                                            WHERE IsApproved = 1 AND EffectiveDate <= '" + sToDate + @"'
				                                                            GROUP BY EmpInfoSystemID
			                                                            ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
												) SEFD ON SD.SalaryID = SEFD.SystemID
			                            INNER JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
			                            INNER JOIN SalaryHead SH ON SD.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') != 'PF Voluntary'
			                            INNER JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
			                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SD.SalaryHeadID = CRC.SalaryHeadID
			                            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
			                            LEFT JOIN 
					                            (
                                                 SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Gen' RuleType, IsGNRNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, ISNULL(IsBankPayment, Convert(bit, 'True')) IsBankPayment, 
						                                ISNULL(IsCashPayment, Convert(bit, 'True')) IsCashPayment, IsMonthWorkDay, IsFixedDisbus, BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, GNRApplicableMonthNo,
                                                        
                                                        FormulaDesID, IsRetain, IsMinWages ,IsWorkDaysInAMonthIncHold
												   FROM SalaryRuleGeneral
						                            UNION
					                             (
                                                  SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Abs' RuleType, IsAbsNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, Convert(bit, 'True') IsBankPayment, Convert(bit, 'True') IsCashPayment, 
						                                 IsMonthWorkDay, IsFixedDisbus, Convert(bit, 'FALSE') BaseOnNetPay, Convert(bit, 'FALSE') RefAbsentism, Convert(bit, 'FALSE') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, '' GNRApplicableMonthNo,
                                                         FormulaDesID, Convert(bit, 'FALSE') IsRetain, Convert(bit, 'FALSE') IsMinWages  ,Convert(bit, 'FALSE') IsWorkDaysInAMonthIncHold
												   FROM SalaryRuleAbsenteeism
                                                 )
                                                ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SD.SalaryHeadID = SlrDis.SalaryHeadID
			                            LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
											                            AND SD.SalaryHeadID = SRDSM.SalaryHeadID
                                        WHERE E.DOJ <= '" + sToDate + @"' AND (E.DOS >= '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
                                                                               OR E.DOS = '' OR E.DOS = '01/01/1901')
                                              AND SEFD.IsApproved = 1 AND SEFD.EffectiveDate <= '" + sToDate + @"'
                                    ) A 
                                  WHERE (" + sEmpInfo + @") ";
                if (sPlantID != "ALL" & sPlantID != "")
                {
                    strSql += @"
                                AND PlantID = '" + sPlantID + @"' ";
                }

                strSql += @"
                            ORDER BY EmpInfoSystemID, SequenceNo, HeadType DESC";

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

        public void GetEmployeeWisePFValueAfterCalx(string sEmpInfo, string sDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"
SELECT FC.EmpSystemID, E.PlantId, FC.SalaryRuleMasterSystemID, FC.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, FC.SlrCate, FC.PFMntEmpWiseCalID, 
									FC.PFEligibleEmpID, FC.IsDistribution, FC.ContributionAmount, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo 

SELECT FC.EmpSystemID, E.PlantId, FC.SalaryRuleMasterSystemID, FC.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, FC.SlrCate, FC.PFMntEmpWiseCalID, 
									FC.PFEligibleEmpID, FC.IsDistribution, FC.ContributionAmount, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    ISNULL(CRC.IsDexxx
                                   
						           

SELECT FC.EmpSystemID, E.PlantId, FC.SalaryRuleMasterSystemID, FC.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, FC.SlrCate, FC.PFMntEmpWiseCalID, 
									FC.PFEligibleEmpID, FC.IsDistribution, FC.ContributionAmount, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
						    FROM
						    (
							    SELECT * FROM
								    (
									    SELECT PFE.EmpSystemID, PFSlrHd.SalaryHeadID, SEFD.SalaryRuleMasterSystemID, PMC.PFMntEmpWiseCalID, PMC.PFEligibleEmpID, PMC.IsDistribution, PMC.ContributionAmount, PMC.SlrCate
									    FROM PFEligibleEmployee PFE
										    INNER JOIN (
													    SELECT SLM.* FROM 
															    (
																    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																	    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																    FROM SalaryInfoDefineMaster
																    UNION 
																	    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																			    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																	    )
													    ) SLM 
														    INNER JOIN
																    (
																	    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
																	    FROM 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoDefineMaster
																			    UNION 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																		    )
																		    ) A
SELECT FC.EmpSystemID, E.PlantId, FC.SalaryRuleMasterSystemID, FC.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, FC.SlrCate, FC.PFMntEmpWiseCalID, 
									FC.PFEligibleEmpID, FC.IsDistribution, FC.ContributionAmount, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
						    FROM
						    (
							    SELECT * FROM
								    (
									    SELECT PFE.EmpSystemID, PFSlrHd.SalaryHeadID, SEFD.SalaryRuleMasterSystemID, PMC.PFMntEmpWiseCalID, PMC.PFEligibleEmpID, PMC.IsDistribution, PMC.ContributionAmount, PMC.SlrCate
									    FROM PFEligibleEmployee PFE
										    INNER JOIN (
													    SELECT SLM.* FROM 
															    (
																    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																	    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																    FROM SalaryInfoDefineMaster
																    UNION 
																	    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																			    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																	    )
													    ) SLM 
														    INNER JOIN
																    (
																	    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
																	    FROM 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoDefineMaster
																			    UNION 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																		    )
																		    ) A
SELECT FC.EmpSystemID, E.PlantId, FC.SalaryRuleMasterSystemID, FC.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, FC.SlrCate, FC.PFMntEmpWiseCalID, 
									FC.PFEligibleEmpID, FC.IsDistribution, FC.ContributionAmount, ECR.Id AS EntryCurrencyID, ECR.Name AS EntryCurrency, SRM.CurrencyRuleSystemID,
                                    DECR.Id AS DefinitionCurrencyID, DECR.Name AS DefinitionCurrency, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                    AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                        ELSE SH.SalaryHeadID END,
			                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                    ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
						    FROM
						    (
							    SELECT * FROM
								    (
									    SELECT PFE.EmpSystemID, PFSlrHd.SalaryHeadID, SEFD.SalaryRuleMasterSystemID, PMC.PFMntEmpWiseCalID, PMC.PFEligibleEmpID, PMC.IsDistribution, PMC.ContributionAmount, PMC.SlrCate
									    FROM PFEligibleEmployee PFE
										    INNER JOIN (
													    SELECT SLM.* FROM 
															    (
																    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																	    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																    FROM SalaryInfoDefineMaster
																    UNION 
																	    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																			    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																	    )
													    ) SLM 
														    INNER JOIN
																    (
																	    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
																	    FROM 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoDefineMaster
																			    UNION 
																		    (
																		    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																		    FROM SalaryInfoBackMaster
																		    )
																		    ) A
																	    WHERE IsApproved = 1 AND EffectiveDate <= '" + sDate + @"'
																	    GROUP BY EmpInfoSystemID
																    ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
													    ) SEFD ON PFE.EmpSystemID = SEFD.EmpInfoSystemID
										    INNER JOIN (
													    SELECT *, 'PF Employee Contribution' SlrCate FROM SalaryRulePF WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'PF Employee Contribution')
													    UNION
													    (
													    SELECT *, 'PF Employer Contribution' SlrCate FROM SalaryRulePF WHERE SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'PF Employer Contribution')
													    ) 
													    ) PFSlrHd ON SEFD.SalaryRuleMasterSystemID = PFSlrHd.SalaryRuleMasterSystemID 
										    INNER JOIN (
													    SELECT ID PFMntEmpWiseCalID, PFEligibleEmpID, IsDistributionEmp IsDistribution, EmployeeContributionAmount ContributionAmount, 'PF Employee Contribution' SlrCate 
														    FROM [dbo].[PFMonthlyEmpWiseCalculation]
													    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
													    UNION
														    (
														    SELECT ID PFMntEmpWiseCalID, PFEligibleEmpID, IsDistributionEmpr IsDistribution, EmployerContributionAmount ContributionAmount, 'PF Employer Contribution' SlrCate 
															    FROM [dbo].[PFMonthlyEmpWiseCalculation]
															    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
														    )
													    ) PMC  ON PMC.PFEligibleEmpID = PFE.ID AND PFSlrHd.SlrCate = PMC.SlrCate
								    ) AB
								    UNION
								    (
								    SELECT PFE.EmpSystemID, D.SalaryHeadID, SEFD.SalaryRuleMasterSystemID, C.ID PFMntEmpWiseCalID, C.PFEligibleEmpID, CONVERT(BIT, 'False') IsDistribution, D.Amount ContributionAmount, '' SlrCate 
									    FROM [dbo].[PFMonthlyEmpWiseCalculation] C
											    INNER JOIN PFEligibleEmployee PFE ON C.PFEligibleEmpID = PFE.ID
											    INNER JOIN (
														    SELECT SLM.* FROM 
																    (
																	    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																		    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																	    FROM SalaryInfoDefineMaster
																	    UNION 
																		    (
																			    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																				    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																			    FROM SalaryInfoBackMaster
																		    )
																    ) SLM 
																	    INNER JOIN
																			    (
																				    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
																				    FROM 
																					    (
																					    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																							    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																					    FROM SalaryInfoDefineMaster
																						    UNION 
																					    (
																					    SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
																							    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
																					    FROM SalaryInfoBackMaster
																					    )
																					    ) A
																				    WHERE IsApproved = 1 AND EffectiveDate <= '" + sDate + @"'
																				    GROUP BY EmpInfoSystemID
																			    ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
													    ) SEFD ON PFE.EmpSystemID = SEFD.EmpInfoSystemID
											    INNER JOIN
													    (
														    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM(Amount) Amount FROM
																					    (
																						    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM([Value]) Amount FROM [dbo].[PFMonthlyDistributionEmployee]
																						    WHERE ISNULL(SalaryHeadID, '') != '' AND [Value] > 0
																						    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
																						    UNION
																						    (SELECT PFMntEmpWiseCalID, ResidualValueSlrHdID SalaryHeadID, SUM([UpperLimit]) Amount FROM [dbo].[PFMonthlyDistributionEmployee]
																						    WHERE ISNULL(ResidualValueSlrHdID, '') != '' AND [UpperLimit] > 0
																						    GROUP BY PFMntEmpWiseCalID, ResidualValueSlrHdID)
																					    ) A 
																					    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
														    UNION
														    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM(Amount) Amount FROM
																					    (
																						    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM([Value]) Amount FROM [dbo].[PFMonthlyDistributionEmployer]
																						    WHERE ISNULL(SalaryHeadID, '') != '' AND [Value] > 0
																						    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
																						    UNION
																						    (SELECT PFMntEmpWiseCalID, ResidualValueSlrHdID SalaryHeadID, SUM([UpperLimit]) Amount FROM [dbo].[PFMonthlyDistributionEmployer]
																						    WHERE ISNULL(ResidualValueSlrHdID, '') != '' AND [UpperLimit] > 0
																						    GROUP BY PFMntEmpWiseCalID, ResidualValueSlrHdID)
																					    ) A 
																					    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
													    ) D ON D.PFMntEmpWiseCalID = C.ID 
														    WHERE MonthNo = MONTH(CONVERT(DATE, '" + sDate + @"')) AND YearNo = YEAR(CONVERT(DATE, '" + sDate + @"'))
								    )
						    ) FC 
								INNER JOIN EmployeeInformation E ON FC.EmpSystemID = E.SystemId
							    INNER JOIN SalaryHead SH ON FC.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' --AND ISNULL(SH.HeadCategory, '') != 'PF Voluntary'
							    INNER JOIN SalaryRuleMaster SRM ON FC.SalaryRuleMasterSystemID = SRM.SystemID
							    LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND FC.SalaryHeadID = CRC.SalaryHeadID
							    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
							    LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
							    LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id	
						    WHERE FC.IsDistribution = 0
								  ------AND FC.SalaryHeadID NOT IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'PF Voluntary')
								  AND 
                                  FC.EmpSystemID IN (" + sEmpInfo + @") 
						    ORDER BY FC.EmpSystemID";

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
    }
}
