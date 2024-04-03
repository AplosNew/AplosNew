#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Organizations;
using Library.Model.Payrolls;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Setups
{
    public class DesignationMasterConfigurationService : Service<DesignationMasterConfiguration>, IDesignationMasterConfigurationService
    {
        private IEnumerable<object> SalaryRuleMstHeadList = null;
        private string[] AttdnBonusPmtPolicyHeadList = null;
        private string[] ESICPolicyHeadList = new string[] { };
        private string[] PFPolicyHeadList = new string[] { };

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<DesignationMaster> _designationMasterRepository;
        private readonly IRepositoryAsync<SalaryFixation> _salaryFixationRepository;
        private readonly IRepositoryAsync<BonusPolicyMaster> _bonusPolicyMasterRepository;
        private readonly IRepositoryAsync<AttdnBonusPmtPolicyMaster> _attdnBonusPmtPolicyMasterRepository;

        public DesignationMasterConfigurationService(
            IRepositoryAsync<DesignationMasterConfiguration> DesignationMasterConfigurationRepository,
            IPKGeneratorService pkGeneratorService,
            IRepositoryAsync<DesignationMaster> designationMasterRepository,
            IRepositoryAsync<SalaryFixation> salaryFixationRepository,
            IRepositoryAsync<BonusPolicyMaster> bonusPolicyMasterRepository,
             IRepositoryAsync<AttdnBonusPmtPolicyMaster> attdnBonusPmtPolicyMasterRepository,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(DesignationMasterConfigurationRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _designationMasterRepository = designationMasterRepository;
            _salaryFixationRepository = salaryFixationRepository;
            _bonusPolicyMasterRepository = bonusPolicyMasterRepository;
            _attdnBonusPmtPolicyMasterRepository = attdnBonusPmtPolicyMasterRepository;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertORUpdate(IEnumerable<DesignationMasterConfiguration> entities)
        {
            var flag = false;
            string operators = "+-*/%()[]{}@#$^=";
            try
            {
                if (entities != null)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    SalaryRuleMstHeadList = GetSalaryRuleMstHead();
                    ////var attdnBonus = GetAttdnBonusPmtPolicyHead().FirstOrDefault();//
                    ////if (attdnBonus != null)
                    ////{
                    ////    var dic = (Dictionary<string, object>)attdnBonus;
                    ////    char[] splitchar = { ',' };
                    ////    AttdnBonusPmtPolicyHeadList = dic["SalaryHeadID"].ToString().Split(splitchar);
                    ////}
                    var pk = GetMaxNumber(nameof(DesignationMasterConfiguration), PKGeneratorEnum.Yearly, null, DateTime.Now);
                    foreach (var item in entities)
                    {
                        if (!string.IsNullOrEmpty(item.Id))
                        {
                            if (item.SalaryRuleMasterId != null && item.AttdnBonusPmtPolicyMasterId != null)
                                //if (AttdnBonusPmtPolicyHeadList == null)
                                //{
                                //    throw new CustomException(GetSalaryHeadName(item.SalaryRuleMasterId) + " this rule master is not in bonus policy master");
                                //}
                                if (AttdnBonusPmtPolicyHeadList != null)
                                {
                                    if (!Validation(item.SalaryRuleMasterId))
                                    {
                                        throw new CustomException(GetSalaryHeadName(item.SalaryRuleMasterId) + " this rule master is not in bonus policy master");
                                    }
                                }

                            var eSICPolicy = GetESICPolicyHead(item.ESICPolicyMasterID);
                            if (eSICPolicy != null)
                            {
                                var myList = new List<string>();
                                foreach (var ob in eSICPolicy)
                                {
                                    var dic = (Dictionary<string, object>)ob;
                                    string[] split = dic["SalaryHeadID"].ToString().Split(' ');
                                    //foreach (var sp in split)
                                    //{
                                    //    myList.Add(sp);
                                    //}
                                    myList = GetListofHeadIds(operators, split);
                                }
                                ESICPolicyHeadList = myList.ToArray();
                            }
                            if (item.ESICPolicyMasterID != null)
                                //if (ESICPolicyHeadList == null)
                                //{
                                //    throw new CustomException(GetEsicHeadName(item.ESICPolicyMasterID) + " this rule master is not in Salary rule master");
                                //}
                                //else
                                if (!ValidationForHead(ESICPolicyHeadList))
                                {
                                    throw new CustomException(GetEsicHeadName(item.SalaryRuleMasterId) + " this rule master is not in bonus policy master");
                                }
                            var pFPolicy = GetPFPolicyHead(item.PFPolicyMasterID);
                            if (pFPolicy != null)
                            {
                                var mypFPolicyList = new List<string>();
                                foreach (var y in pFPolicy)
                                {
                                    var dic = (Dictionary<string, object>)y;
                                    string[] split = dic["SalaryHeadID"].ToString().Split(' ');

                                    mypFPolicyList= GetListofHeadIds(operators,split);
                                    //foreach (var spt in split)
                                    //{
                                    //    mypFPolicyList.Add(spt);
                                    //}
                                }
                                PFPolicyHeadList = mypFPolicyList.ToArray();
                            }
                            if (item.PFPolicyMasterID != null)
                                //if (PFPolicyHeadList == null)
                                //{
                                //    throw new CustomException(GetPFPolicyHeadName(item.PFPolicyMasterID) + " this rule master is not in Salary rule master");
                                //}
                                //else
                                if (!ValidationForHead(PFPolicyHeadList))
                                {
                                    throw new CustomException(GetSalaryHeadName(item.PFPolicyMasterID) + " this rule master is not in bonus policy master");
                                }
                            //var dbOb = _designationMasterRepository.Find(item.Id); for accident
                            //item.EmployeeCategoryId = dbOb.EmployeeCategoryId; for accident
                            UpdateGraph(item);
                        }
                        else
                        {
                            pk.MaxNumber++;
                            item.Id = DateTime.Now.ToString("yy") + "-" + pk.MaxNumber.ToString();
                            if (item.SalaryRuleMasterId != null && item.AttdnBonusPmtPolicyMasterId != null)
                            {
                                //if (AttdnBonusPmtPolicyHeadList == null)
                                //{
                                //    throw new CustomException(GetSalaryHeadName(item.SalaryRuleMasterId) + " this rule master is not in bonus policy master");
                                //}
                                if (AttdnBonusPmtPolicyHeadList != null)
                                {
                                    if (!Validation(item.SalaryRuleMasterId))
                                    {
                                        throw new CustomException(GetSalaryHeadName(item.SalaryRuleMasterId) + " this rule master is not in bonus policy master");
                                    }
                                }
                            }
                            var eSICPolicy = GetESICPolicyHead(item.ESICPolicyMasterID);
                            if (eSICPolicy != null)
                            {
                                var myList = new List<string>();
                                foreach (var ob in eSICPolicy)
                                {
                                    var dic = (Dictionary<string, object>)ob;
                                    string[] split = dic["SalaryHeadID"].ToString().Split(' ');
                                    //foreach (var sp in split)
                                    //{
                                    //    myList.Add(sp);
                                    //}
                                    myList = GetListofHeadIds(operators, split);
                                }
                                ESICPolicyHeadList = myList.ToArray();
                            }
                            if (item.ESICPolicyMasterID != null)
                            {
                                //if (ESICPolicyHeadList == null)
                                //{
                                //    throw new CustomException(GetEsicHeadName(item.ESICPolicyMasterID) + " this rule master is not in Salary rule master");
                                //}
                                //else
                                if (!ValidationForHead(ESICPolicyHeadList))
                                {
                                    throw new CustomException(GetEsicHeadName(item.SalaryRuleMasterId) + " this rule master is not in bonus policy master");
                                }
                            }
                            var pFPolicy = GetPFPolicyHead(item.PFPolicyMasterID);
                            if (pFPolicy != null)
                            {
                                var mypFPolicyList = new List<string>();
                                foreach (var y in pFPolicy)
                                {
                                    var dic = (Dictionary<string, object>)y;
                                    string[] split = dic["SalaryHeadID"].ToString().Split(' ');
                                    mypFPolicyList = GetListofHeadIds(operators, split);
                                }
                                PFPolicyHeadList = mypFPolicyList.ToArray();
                            }
                            if (item.PFPolicyMasterID != null) 
                            {
                                //if (PFPolicyHeadList == null)
                                //{
                                //    throw new CustomException(GetPFPolicyHeadName(item.PFPolicyMasterID) + " this rule master is not in Salary rule master");
                                //}
                                //else
                                if (!ValidationForHead(PFPolicyHeadList))
                                {
                                    throw new CustomException(GetSalaryHeadName(item.PFPolicyMasterID) + " this rule master is not in bonus policy master");
                                }
                            }
                            //var dbOb = _designationMasterRepository.Find(item.Id); for accident
                            //item.EmployeeCategoryId = dbOb.EmployeeCategoryId; for accident
                            InsertGraph(item);
                        }
                    }
                }
                else
                {
                    throw new CustomException("No data found to save");
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public List<string> GetListofHeadIds(string operators,string[]heads)
        {
            List<string> strHid = new List<string>();
            foreach (var item in heads)
            {
                strHid.Add(item);
            }

            for (int i = 0; i < strHid.Count; i++)
            {
                strHid[i] = strHid[i].Trim();
            }
            foreach (char item in operators)
            {
               var hid= strHid.Where(x => x.Trim() == item.ToString()).ToList();

                foreach (var h in hid)
                {
                    strHid.Remove(h);
                }
            }       

            return strHid;
        }

        private bool Validation(string salaryRuleMasterId)
        {
            foreach (var item in SalaryRuleMstHeadList)
            {
                var ob = (Dictionary<string, object>)item;
                if (ob["SystemID"].ToString() == salaryRuleMasterId)
                {
                    for (int i = 0; i < AttdnBonusPmtPolicyHeadList.Length; i++)
                    {
                        var ob2 = AttdnBonusPmtPolicyHeadList[i];
                        if (ob["SalaryHeadID"].ToString() == ob2)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool ValidationForHead(string[] list)
        {
            foreach (var item in SalaryRuleMstHeadList)
            {
                var ob = (Dictionary<string, object>)item;
                for (int i = 0; i < list.Length; i++)
                {
                    var ob2 = list[i];
                    if (ob["SalaryHeadID"].ToString() == ob2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(DesignationMasterConfiguration), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(DesignationMasterConfiguration entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void Delete(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                base.Delete(Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM SCS.DesignationMasterConfiguration ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetAttdnBonusPmtPolicyHead()
        {
            var sql = @"SELECT SalaryHeadID FROM [dbo].[AttdnBonusPmtPolicyDetails] WHERE SalaryHeadID != null";
            var svalu = _sqlRepository.GetDataCollection(sql);

            return svalu;
        }

        public IEnumerable<object> GetESICPolicyHead(string esicPolicyMstId)
        {
            var sql = @"SELECT ED.SalaryHeadIDEarning SalaryHeadID FROM [dbo].[ESICPolicyDetails] ED WHERE ED.ESICPolicyMasterID='" + esicPolicyMstId + @"'
                        union
                        SELECT ED.SalaryHeadIDEmp SalaryHeadID FROM [dbo].[ESICPolicyDetails] ED  WHERE ED.ESICPolicyMasterID='" + esicPolicyMstId + @"'
                        union
                        SELECT ED.SalaryHeadIDEmployer SalaryHeadID FROM [dbo].[ESICPolicyDetails] ED WHERE ED.ESICPolicyMasterID='" + esicPolicyMstId + @"'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPFPolicyHead(string pFPolicyMstId)
        {
            var sql = @"SELECT ED.SalaryHeadIDEarning SalaryHeadID FROM [dbo].[PFPolicyDetails] ED WHERE ED.PFPolicyMasterID='" + pFPolicyMstId + @"'
                        union
                        SELECT ED.SalaryHeadIDEmp SalaryHeadID FROM [dbo].[PFPolicyDetails] ED  WHERE ED.PFPolicyMasterID= '" + pFPolicyMstId + @"'
                        union
                        SELECT ED.SalaryHeadIDEmployer SalaryHeadID FROM [dbo].[PFPolicyDetails] ED WHERE ED.PFPolicyMasterID= '" + pFPolicyMstId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public string GetSalaryHeadName(string salaryRuleMstId)
        {
            var sql = @"SeLECT SalaryRuleName FROM SalaryRuleMaster where SystemID='" + salaryRuleMstId + "'";
            return _salaryFixationRepository.SqlQuery<string>(sql).FirstOrDefault();
        }

        public string GetEsicHeadName(string eSICPolicyMasterId)
        {
            var sql = @"SELECT ESICPolicyName from ESICPolicyMaster where ID='" + eSICPolicyMasterId + "'";
            return _salaryFixationRepository.SqlQuery<string>(sql).FirstOrDefault();
        }

        public string GetPFPolicyHeadName(string pFPolicyMasterId)
        {
            var sql = @"SELECT PFPolicyName from PFPolicyMaster where ID='" + pFPolicyMasterId + "'";
            return _salaryFixationRepository.SqlQuery<string>(sql).FirstOrDefault();
        }

        public IEnumerable<object> GetSalaryRuleMstHead()
        {
            var sql = @"SELECT  SR.SystemID,CRC.SalaryHeadID, SH.SalaryHead FROM CurrencyRuleChild CRC
                        INNER JOIN SalaryRuleMaster SR ON SR.CurrencyRuleSystemID = CRC.MstSystemID
                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> QueryGraph(string plantId, string companyGroupId)
        {
            var sql = @"SELECT B.SalaryHead,B.Description,B.HeadCategory,A.* FROM [MST].[DesignationMasterConfiguration] AS A
                                LEFT OUTER JOIN [dbo].[SalaryHead] AS B ON A.SalaryHeadId=B.SalaryHeadID
                                WHERE A.PlantId='" + plantId + "' AND A.CompanyGroupId='" + companyGroupId + "' ORDER BY B.SalaryHead";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> QueryDesignation(string designationGroupId, string plantId, string companyGroupId)
        {
            var _sql = @"SELECT DC.Id,DM.Id DesignationMasterId,DC.RecruitmentProcessSetId,DC.AccountsGroupId,DC.SalaryRuleMasterId,DC.PlantId,DC.LeavePolicyMasterId,DC.SalaryFixationSettingId,DC.AttdnBonusPmtPolicyMasterId,
                         DC.BonusPolicyMasterId,DC.PFPolicyMasterID,DC.ESICPolicyMasterID,DC.IsOTEntitled,DC.BnsPlcMthRetainID,DC.OverTimePmtPolicyMasterID ,D.Id DesignationId,
                         D.UserName,D.Code,C.UserName EmployeeCategory,DC.AddedBy,DC.AddedDate,DC.AddedFromIP,DC.UpdatedBy,DC.UpdatedDate,DC.UpdatedFromIP,DC.HolidayPayDayMasterId,DC.NoticePeriod
                         ,LegalDesignation=STUFF((SELECT DISTINCT ','+LD.UserName FROM 
							[MST].[DesignationMasterLegalDesignation] DMLD
							LEFT JOIN HKP.LegalDesignation LD ON DMLD.LegalDesignationId=LD.Id
							WHERE DMLD.DesignationMasterId=DM.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                         FROM MST.DesignationMaster DM
						 LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId AND DC.PlantId = '" + plantId + @"'
                         LEFT JOIN HKP.Designation D ON DM.DesignationId=D.Id
                         LEFT JOIN HKP.EmployeeCategory C ON DM.EmployeeCategoryId=C.Id
                         WHERE DM.DesignationGroupId='" + designationGroupId + "' AND DM.CompanyGroupId='" + companyGroupId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public IEnumerable<object> GetLeavePolicyCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var _sql = @"SELECT SystemId [Value], PolicyName [Text] FROM [dbo].[LeavePolicyMaster] WHERE GroupID='" + identity.CompanyGroupId + "' and PlantID='" + plantId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public IEnumerable<object> GetAttdnBonusHeaderData(string plantId)
        {
            var _sql = @"SELECT ah.Id as Value,ah.UserName as Text FROM  AttdnBonusPlantChild ac
            left join AttdnBonusHeader ah on ac.HeaderId=ah.id
            where ac.PlantId='" + plantId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public IEnumerable<object> GetBonusPolicyMasterCbo(string plantId, string companyGroupId)
        {

            var _sql = @"SELECT SystemId [Value], PolicyName [Text] FROM [dbo].[BonusPolicyMaster]
                        WHERE GroupID='" + companyGroupId + @"' and SystemID IN (SELECT BonusPolicyID FROM [dbo].[BonusPolicyPlantWise] WHERE PlantId='" + plantId + "')";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public IEnumerable<object> GetAttdnBonusPmtPolicyMasterCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var _sql = @"SELECT ID [Value], AttenBnsPolicyName [Text] FROM [dbo].[AttdnBonusPmtPolicyMaster] WHERE GroupID='" + identity.CompanyGroupId + "' and PlantID='" + plantId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }
        public IEnumerable<object> GetBonusPolicyMonthlyRetainMasterCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var _sql = @"SELECT ID [Value], BnsPlcMthRetainName [Text] FROM [dbo].[BonusPolicyMonthlyRetainMaster] WHERE GroupID='" + identity.CompanyGroupId + "' and PlantID='" + plantId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }
        public IEnumerable<object> GetPFPolicyMasterCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var _sql = @"SELECT ID [Value], PFPolicyName [Text] FROM [dbo].[PFPolicyMaster] WHERE GroupID='" + identity.CompanyGroupId + "' and PlantID='" + plantId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public IEnumerable<object> GetESICPolicyMasterCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var _sql = @"SELECT ID [Value], ESICPolicyName [Text] FROM [dbo].[ESICPolicyMaster] WHERE GroupID='" + identity.CompanyGroupId + "' and PlantID='" + plantId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }
        public IEnumerable<object> OverTimePmtPolicyMasterCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var _sql = @"SELECT ID [Value], OverTimePolicyName [Text] FROM [dbo].[OverTimePmtPolicyMaster] WHERE GroupID='" + identity.CompanyGroupId + "' and PlantID='" + plantId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public IEnumerable<object> GetLegalDesignationbyDesignationMaster(string designationMasterId)
        {
            var _sql = @"SELECT LD.* FROM [MST].[DesignationMasterLegalDesignation] DMLD 
                        LEFT JOIN HKP.LegalDesignation LD ON DMLD.LegalDesignationId=LD.Id
                        WHERE DesignationMasterId='" + designationMasterId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }
    }
}