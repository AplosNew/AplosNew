#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class SalaryHeadService : Service<SalaryHead>, ISalaryHeadService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public SalaryHeadService(
            IRepositoryAsync<SalaryHead> SalaryHeadRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(SalaryHeadRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(SalaryHead), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(SalaryHead entity)
        {
            try
            {
                entity.SalaryHeadID = "SH-" + GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(SalaryHead entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT SH.SalaryHeadID SalaryHeadId,SH.SalaryHead,SH.HeadCategory,SH.HeadType FROM SalaryHead SH";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> GetSalaryHeadQuery()
        {
            try
            {
                var sql = @"SELECT SH.SalaryHeadID SalaryHeadId,SH.SalaryHead,SH.HeadCategory,SH.HeadType FROM SalaryHead SH";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetSalaryHeadQueryWithLocalLanguage(string LanguageId, string flag)
        {
            try
            {
                var sql = "";
                if (flag.ToUpper() =="SALARYHEAD")
                {
                    sql = @"SELECT SH.SalaryHeadID Sid,(SH.SalaryHead +'('+SH.HeadType+')') UserName,BSH.*  FROM SalaryHead SH 
                             LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID
                             ORDER BY SH.HeadType desc";
                }
                if (flag.ToUpper() == "LEAVE")
                {
                    sql = @"SELECT L.Id Sid,L.UserName, BSH.* FROM dbo.LeaveType L
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE LeaveTypeId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.LeaveTypeId = L.Id
                            ORDER BY UserName";

                }
                if (flag.ToUpper() == "EMPGRADE")
                {
                    sql = @"SELECT SG.Id Sid,SG.UserName, BSH.* FROM SCS.LegalSalaryGrade SG
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE LegalSalaryGradeId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.LegalSalaryGradeId = SG.Id
                            ORDER BY UserName";

                }
                if (flag.ToUpper() == "LABEL")
                {
                    sql = @" SELECT LL.ID Sid,LL.ID UserName, BSH.* FROM LabelList LL
                             LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE LabelName IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.LabelName = LL.ID
                             ORDER BY UserName";
                }
                if (flag.ToUpper() == "DESIGNATION")
                {
                    sql = @"SELECT D.Id Sid,D.UserName, BSH.* FROM HKP.Designation D
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE DesignationId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.DesignationId = D.Id
                            ORDER BY UserName";
                }
                if (flag.ToUpper() == "LINE")
                {
                    sql = @"SELECT L.Id Sid,L.UserName, BSH.* FROM ORG.Line L
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE LineId IS NOT NULL AND LanguageId = '"+ LanguageId + @"') AS BSH ON BSH.LineId = L.Id
                            ORDER BY UserName";
                }
                if (flag.ToUpper() == "SECTION")
                {
                    sql = @"SELECT S.Id Sid,S.UserName, BSH.* FROM ORG.Section S
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SectionId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.SectionId = S.Id
                            ORDER BY UserName";
                }

                if (flag.ToUpper() == "SUBSECTION")
                {
                    sql = @"SELECT SS.Id Sid,SS.UserName, BSH.* FROM ORG.SubSection SS
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SubSectionId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.SubSectionId = SS.Id
                            ORDER BY UserName";
                }
                if (flag.ToUpper() == "DEPARTMENT")
                {
                    sql = @"SELECT D.Id Sid, D.UserName, BSH.* FROM ORG.Department D
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE DepartmentId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.DepartmentId = D.Id
                            ORDER BY UserName	";
                }
                if (flag.ToUpper() == "PLANT")
                {
                    sql = @"SELECT P.Id Sid, P.UserName, BSH.* FROM ORG.Plant P
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE PlantId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.PlantId = P.Id
                            ORDER BY UserName";
                }
                if (flag.ToUpper() == "UNIT")
                {
                    sql = @"SELECT U.Id Sid, U.UserName, BSH.* FROM ORG.Unit U
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE UnitId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.UnitId = U.Id
                            ORDER BY UserName";
                }
                if (flag.ToUpper() == "DIVISION")
                {
                    sql = @"SELECT DV.Id Sid, DV.UserName, BSH.* FROM ORG.Division DV
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE DivisionId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.DivisionId = DV.Id
                            ORDER BY UserName";
                }
                if (flag.ToUpper() == "SUBDIVISION")
                {
                    sql = @"SELECT SDV.Id Sid, SDV.UserName, BSH.* FROM ORG.SubDivision SDV
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SubDivisionId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.DivisionId = SDV.Id
                            ORDER BY UserName";
                }
                if (flag.ToUpper() == "COMPANYGROUP")
                {
                    sql = @"SELECT CG.Id Sid, CG.UserName, BSH.* FROM ORG.CompanyGroup CG
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE CompanyGroupId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.CompanyGroupId = CG.Id
                            ORDER BY UserName";
                }
                if (flag.ToUpper() == "COMPANY")
                {
                    sql = @"SELECT C.Id Sid, C.UserName, BSH.* FROM ORG.Company C
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE CompanyId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.CompanyId = C.Id
                            ORDER BY UserName";
                }
                if (flag.ToUpper() == "LEGALDESIGNATION")
                {
                    sql = @"SELECT LGD.Id Sid, LGD.UserName, BSH.* FROM HKP.LegalDesignation LGD
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE LegalDesignationId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.LegalDesignationId = LGD.Id
                            ORDER BY UserName";
                }
                if (flag.ToUpper() == "EMPLOYEEWORKTYPE")
                {
                    sql = @"SELECT D.Id Sid,D.UserName, BSH.* FROM [dbo].[EmployeeWorkType] D
                          LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE EmployeeWorkTypeId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.EmployeeWorkTypeId = D.Id ORDER BY UserName";
                }
                if (flag.ToUpper() == "FINALSETTLEMENTHEAD")
                {
                    sql = @"SELECT SH.ID Sid,SH.UserName,BSH.*  FROM [dbo].[FinalSettlementDeductionHead] SH 
                         LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE FinalSettlementHeadId IS NOT NULL AND LanguageId = '" + LanguageId + @"') AS BSH ON BSH.FinalSettlementHeadId = sh.Id
                         ORDER BY SH.Category desc";
                }

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetLeaveTypeQuery()
        {
            try
            {
                var sql = @"select Id,CompanyGroupId,LeaveType,Code,UserName,Description from dbo.LeaveType ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
        public IEnumerable<object> QueryLocalLanguage()
        {
            try
            {
                var sql = @"SELECT Id, LanguageId, Name Language FROM HKP.LocalLanguage ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        

    }
}