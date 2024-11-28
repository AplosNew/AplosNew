using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.Materials
{
    public class MaterialMasterArticleService : Service<MaterialMasterArticle>, IMaterialMasterArticleService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<MaterialMasterArticle> _articleRepository;
        private readonly IRepositoryAsync<MaterialMasterArticleValue> _valueRepository;
        private readonly IRepositoryAsync<MaterialMasterArticleProcess> _processRepository;

        public MaterialMasterArticleService(
              IRepositoryAsync<MaterialMasterArticle> articleRepository
            , IRepositoryAsync<MaterialMasterArticleValue> valueRepository
            , IRepositoryAsync<MaterialMasterArticleProcess> processRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork)
            : base(articleRepository, unitOfWork, pkGeneratorService)
        {
            _articleRepository = articleRepository;
            _valueRepository = valueRepository;
            _processRepository = processRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Query

        public IEnumerable<object> Query(string materialMasterId)
        {
            try
            {
                var _sql = @"SELECT MMA.Id, MMA.MaterialMasterId, MMA.Code, MMA.ShortName, MMA.StandardName, MMA.UserName,HC.Code as HSNCode,MMA.HSNCodeId,MMA.RPM,           MMA.MachineAllowance,MMA.StitchCodeId,MMA.MachineMasterId,MM.UserName MachineMaster,MMA.OrderLevel
                            ,MMA.IsMachineApplicable
							,MMA.IsWorkCenterApplicable,MMA.Active,MMA.ProductionGroupingId,MMA.ProcessSetId, PS.[Description] ProcessSet

		                    FROM MST.MaterialMasterArticle MMA
                           LEFT JOIN [MST].[MachineMaster] MM ON MM.Id=MMA.MachineMasterId
						   LEFT JOIN [HKP].[HSNCode] HC ON HC.id=MMA.HSNCodeId
						   LEFT JOIN [HKP].ProductionGrouping PG ON PG.id=MMA.ProductionGroupingId
						   LEFT JOIN [HKP].ProcessSet PS ON PS.id=MMA.ProcessSetId
                            WHERE MaterialMasterId='" + materialMasterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel GetMaterialArticle(GridParameter parameters, string materialMasterId)
        {
            try
            {
                var sql = "";
                if (!string.IsNullOrEmpty(materialMasterId))
                    sql = " AND MaterialMasterId='" + materialMasterId + "'";

                parameters.CmdText = @"SELECT DISTINCT  ART.Id, ART.MaterialMasterId, MM.UserName AS MaterialMasterName
                                    , MM.Code AS MaterialCode, MG.UserName AS MaterialGroup
                                    , ART.Code, ART.ShortName, ART.StandardName
									,HSNCodeId=CASE WHEN ART.HSNCodeId IS NULL THEN MM.HSNCodeId ELSE ART.HSNCodeId END
									,HSNCode=CASE WHEN ART.HSNCodeId IS NULL THEN MHSN.Code ELSE HSN.Code END,ART.MinimumValue,ART.MaximumValue
                        FROM MST.MaterialMasterArticle AS ART
                        LEFT JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id 
                        LEFT JOIN MST.MaterialGroupMaster AS MG ON MM.MaterialGroupMasterId=MG.Id
                        LEFT JOIN HKP.MaterialType AS MT ON MG.MaterialTypeId=MT.Id
						LEFT JOIN HKP.HSNCode MHSN ON MHSN.Id=MM.HSNCodeId
						LEFT JOIN HKP.HSNCode HSN ON HSN.Id=ART.HSNCodeId
                        WHERE  MaterialMasterId='" + materialMasterId + "' AND ART.Active=1";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetMaterialArticle(string materialMasterId, string[] materialType)
        {
            try
            {
                var sql = "";
                if (!string.IsNullOrEmpty(materialMasterId))
                    sql = " MaterialMasterId='" + materialMasterId + "'";

                var sql1 = @"SELECT DISTINCT  ART.Id, ART.MaterialMasterId, MM.UserName AS MaterialMasterName
                                    , MM.Code AS MaterialCode, MG.UserName AS MaterialGroup
                                    , ART.Code, ART.ShortName, ART.StandardName
									,HSNCodeId=CASE WHEN ART.HSNCodeId IS NULL THEN MM.HSNCodeId ELSE ART.HSNCodeId END
									,HSNCode=CASE WHEN ART.HSNCodeId IS NULL THEN MHSN.Code ELSE HSN.Code END,ART.MinimumValue,ART.MaximumValue
                        FROM MST.MaterialMasterArticle AS ART
                        LEFT JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id 
                        LEFT JOIN MST.MaterialGroupMaster AS MG ON MM.MaterialGroupMasterId=MG.Id
                        LEFT JOIN HKP.MaterialType AS MT ON MG.MaterialTypeId=MT.Id
						LEFT JOIN HKP.HSNCode MHSN ON MHSN.Id=MM.HSNCodeId
						LEFT JOIN HKP.HSNCode HSN ON HSN.Id=ART.HSNCodeId
                        WHERE " + sql;
                return _sqlRepository.GetDataCollection(sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetMaterialArticleValue(string articleId)
        {
            try
            {
                var sql = @"SELECT NULL AS Id
                                        , MMA.MaterialMasterId
                                        , MMA.MaterialAttributeId AS MaterialAttributeId
                                        , MA.UserName AS MaterialAttributeName
                                        , MMA.IsFreeField
                                        , MMA.IsPreDefinedField
                                        , MMA.IsMandatory
                                        , MA.ValueAssignmentLevel
                                        , MMAV.MaterialMasterArticleId
		                                , MMAV.MaterialAttributeValueId
		                                , MaterialAttributeValueFreeText = MAV.UserName
                                FROM MST.MaterialMasterAttribute AS MMA
                                JOIN HKP.MaterialAttribute AS MA ON MMA.MaterialAttributeId = MA.Id
                                LEFT JOIN MST.MaterialMasterArticleValue AS MMAV ON MMAV.MaterialAttributeId = MA.Id AND MMAV.MaterialMasterId = MMA.MaterialMasterId
                                LEFT JOIN HKP.MaterialAttributeValue AS MAV ON MAV.MaterialAttributeId = MMA.MaterialAttributeId AND MMAV.MaterialAttributeValueId = MAV.Id
                                WHERE MMAV.MaterialMasterArticleId = '" + articleId + "' ORDER BY MMA.[Sequence]";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        /// <summary>
        /// use : Product definition
        /// </summary>
        /// <param name="materialMasterId"></param>
        /// <returns></returns>
        public IEnumerable<object> GetArticlListByMaterialMaster(string materialMasterId)
        {
            try
            {
                var dataList = base.Query(t => t.MaterialMasterId == materialMasterId).Include(t => t.MaterialMasterArticleProcess.Select(a => a.Process)).Select();
                var listData = new List<object>();
                foreach (var item in dataList)
                {
                    foreach (var child in item.MaterialMasterArticleProcess)
                    {
                        child.Code = child.Process.Code;
                        child.ShortName = child.Process.ShortName;
                        child.StandardName = child.Process.StandardName;
                        child.UserName = child.Process.UserName;
                    }
                    var row = new
                    {
                        item.Id,
                        item.Code,
                        item.ShortName,
                        item.StandardName,
                        item.MaterialMasterId,
                        item.MaterialMasterArticleProcess,
                    };
                    listData.Add(row);
                }
                return listData;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetArticlValueHead(string materialMasterId)
        {
            var _sql = @"SELECT MMA.MaterialAttributeId,MA.UserName AS MaterialAttributeName, MMA.[Sequence] FROM MST.MaterialMasterAttribute AS MMA
                            INNER JOIN HKP.MaterialAttribute AS MA ON MMA.MaterialAttributeId=MA.Id WHERE MMA.MaterialMasterId='" + materialMasterId + "'";
            return _sqlRepository.GetDataCollection(_sql, null);
        }

        #endregion Query

        private string GetPK()
        {
            return GetAutoNumber(nameof(MaterialMasterArticle), PKGeneratorEnum.Auto, null, DateTime.Now);
        }


        public void Comapare(List<MaterialMasterArticleNew> allArticles, List<MaterialMasterArticleValue> currentArticles)
        {
            try
            {
                if (allArticles == null || allArticles.Count == 0)
                    return;
                
                for (int i = 0; i < currentArticles.Count; i++)
                {
                    if (string.IsNullOrEmpty(currentArticles[i].MaterialAttributeValueFreeText))
                        currentArticles[i].MaterialAttributeValueFreeText = "";

                    if (string.IsNullOrEmpty(currentArticles[i].MaterialAttributeId))
                        currentArticles[i].MaterialAttributeId = "";
                }
                for (int i = 0; i < allArticles.Count; i++)
                {
                    for (int j = 0; j < allArticles[i].MaterialMasterArticleValues.Count; j++)
                    {
                        if (string.IsNullOrEmpty(allArticles[i].MaterialMasterArticleValues[j].MaterialAttributeValueFreeText))
                            allArticles[i].MaterialMasterArticleValues[j].MaterialAttributeValueFreeText = "";

                        if (string.IsNullOrEmpty(allArticles[i].MaterialMasterArticleValues[j].MaterialAttributeId))
                            allArticles[i].MaterialMasterArticleValues[j].MaterialAttributeId = "";
                    }
                }

                List<MaterialMasterArticleNew> TempArticles = new List<MaterialMasterArticleNew>();
                for (int i = 0; i < currentArticles.Count; i++)
                {
                    allArticles = compareX(allArticles, currentArticles[i]);
                }

                if (allArticles != null && allArticles.Count > 0)
                {
                    throw new Exception("Same combination already exists!!!");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private List<MaterialMasterArticleNew> compareX(List<MaterialMasterArticleNew> allArticles, MaterialMasterArticleValue currentArticlesValue)
        {
            List<MaterialMasterArticleNew> TempArticles = new List<MaterialMasterArticleNew>();
            for (int i = 0; i < allArticles.Count; i++)
            {
                for (int j = 0; j < allArticles[i].MaterialMasterArticleValues.Count; j++)
                {
                    if (allArticles[i].MaterialMasterArticleValues[j].MaterialAttributeId == currentArticlesValue.MaterialAttributeId
                        && allArticles[i].MaterialMasterArticleValues[j].MaterialAttributeValueFreeText == currentArticlesValue.MaterialAttributeValueFreeText)
                    {
                        
                        TempArticles.Add(allArticles[i]);
                    }
                }
            }
            return TempArticles;
        }



        public void InsertOrUpdateGraph(IEnumerable<MaterialMasterArticle> articles, string materialCode)
        {
            var flag = false;
            try
            {
                if (articles == null)
                    throw new CustomException("Can't insert without article.");
                _unitOfWork.BeginTransaction();
                flag = true;
               
                var materialMasterId = articles.First().MaterialMasterId;
                var dbList = base.Query(t => t.MaterialMasterId == materialMasterId).Include(t => t.MaterialMasterArticleValues).Select().AsEnumerable();

                var masterId = articles.FirstOrDefault().MaterialMasterId;
                var count = _articleRepository.SqlQuery<int>("SELECT ISNULL(MAX(CAST(SUBSTRING(Code, LEN('" + materialCode + "') + 1, (LEN(Code)-LEN('" + materialCode + "'))) AS INT)), 0) AS Code FROM [MST].[MaterialMasterArticle] WHERE MaterialMasterId = '" + masterId + "'").First();

                foreach (var item in articles)
                {
                    var localList = item.MaterialMasterArticleValues.ToList();
                    foreach (var del in localList)
                    {
                        if (del.Id == 0 && string.IsNullOrEmpty(del.MaterialAttributeValueId) && string.IsNullOrEmpty(del.MaterialAttributeValueFreeText))
                            item.MaterialMasterArticleValues.Remove(del);
                    }
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        var pk = GetMaxNumber(nameof(MaterialMasterArticle), PKGeneratorEnum.Auto, null, DateTime.Now);
                        count++;
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.Code = MakePK(materialCode, count, 3);
                        InsertUpdateOrDeleteArticleValue(item.MaterialMasterArticleValues, item.Id);
                        base.InsertGraph(item);
                    }
                    else
                    {
                        InsertUpdateOrDeleteArticleValue(item.MaterialMasterArticleValues, item.Id);

                        var art = new MaterialMasterArticle();
                        art.Id = item.Id;
                        art.MaterialMasterId = item.MaterialMasterId;
                        art.Code = item.Code;
                        art.ShortName = item.ShortName;
                        art.StandardName = item.StandardName;
                        art.UserName = item.UserName;
                        art.Active = item.Active;
                        art.HSNCodeId = item.HSNCodeId;
                        art.ProductionGroupingId = item.ProductionGroupingId;
                        art.ProcessSetId = item.ProcessSetId;
                        art.UpdatedBy = item.UpdatedBy;
                        art.UpdatedDate = item.UpdatedDate;
                        art.UpdatedFromIP = item.UpdatedFromIP;

                        base.UpdateGraph(art);
                    }
                }

                if (dbList.Count() > 0)
                {
                    foreach (var item in dbList)
                    {
                        if (!articles.Any(t => t.Id == item.Id))
                        {
                            DeleteMaterialMasterArticleValue(item);
                            DeleteArticleProcessGraphByArticle(item.Id);
                            base.DeleteGraph(item);
                        }
                    }
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string materialMasterId)
        {
            var dbList = base.Query(t => t.MaterialMasterId == materialMasterId).Include(t => t.MaterialMasterArticleValues).Select().ToList();
            if (dbList != null)
            {
                foreach (var item in dbList)
                {
                    DeleteMaterialMasterArticleValue(item);
                    DeleteArticleProcessGraphByArticle(item.Id);
                    base.DeleteGraph(item);
                }
            }
        }

        public void Delete(string articleId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = base.Query(t => t.Id == articleId).Include(t => t.MaterialMasterArticleValues).Select().FirstOrDefault();
                if (data != null)
                {
                    DeleteMaterialMasterArticleValue(data);
                    DeleteArticleProcessGraphByArticle(data.Id);
                    base.DeleteGraph(data);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }


       
        public IEnumerable<object> getArticleAliaslist(string articleId, string masterOrderItemId)
        {
            string sql = @"select AA.*,P.UserName PartyName 
                            from [dbo].[ArticleAlias] AA
                            left join [HKP].[Party] P on P.Id=AA.PartyId
                            where ArticleId ='" + articleId + @"' AND MasterOrderItemId= '"+ masterOrderItemId + "'";

            return _sqlRepository.GetDataCollection(sql, null);
        }

        public void deleteArticleAliasData(string Id)
        {
            var flag = false;
            try
            {
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery("delete from [dbo].[ArticleAlias] where Id='" + Id + "'");
                connection.CommitTransaction();

            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }


        #region Validation

        private void IfMaterialAttributeExist(string id)
        {
            var sql = @"IF EXISTS(SELECT 1 FROM(
                                SELECT MaterialGroupMasterId AS CheckingColumn FROM MST.MaterialAttributeMaster WHERE Archive=0
                                ) A WHERE CheckingColumn = '" + id + "') SELECT 1 ELSE SELECT 0 RETURN ";
            var data = Convert.ToBoolean(_articleRepository.SqlQuery<int>(sql).Single());
            if (data)
                throw new CustomException("Please insert at least one sub-material....!");
        }

        private void IfMaterialAttributeValueExist(MaterialMasterArticle subMaterial, IEnumerable<MaterialMasterAttributeValue> dbMValue)
        {
            try
            {
                //var attrList = subMaterial.MaterialAttributeValues.Select(t => t.MaterialAttributeId).ToList();
                //var subMaterialList = dbMValue.Where(t => t.MaterialMasterAttributeId != subMaterial.Id).Select(t => t.MaterialMasterAttributeId).ToList().Distinct();
                ////var uiMVList = SubMaterial.MaterialAttributeValues.Select(t => t.Id).ToList();
                //foreach (var item in subMaterialList)//SubMaterial List
                //{
                //    var mvUI = subMaterial.MaterialAttributeValues.ToList();
                //    var mvDB = dbMValue.Where(t => t.MaterialMasterAttributeId == item).AsEnumerable();
                //    int count = 0;
                //    for (int i = 0; i < attrList.Count; i++)//Attribute List
                //    {
                //        count += MaterialValueValidation(mvUI, mvDB, attrList[i]);
                //    }//Attribute List
                //    if (attrList.Count == count)
                //    {
                //        var materialMaster = (Dictionary<string, object>)GetMaterialMaster(item).FirstOrDefault();
                //        throw new CustomException("Sub-Material [" + subMaterial.Code + "] exist in material master [" + materialMaster["Code"] + "]");
                //    }
                //}//SubMaterial List
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private int MaterialValueValidation(IEnumerable<MaterialMasterAttributeValue> mvUI, IEnumerable<MaterialMasterAttributeValue> mvDB, string attributeId)
        {
            try
            {
                //var attrValueUi = mvUI.Where(t => t.MaterialAttributeId == attributeId).Select(t => t.MaterialAttributeValueFreeText).FirstOrDefault();
                //var attrValueDb = mvDB.Where(t => t.MaterialAttributeId == attributeId).Select(t => t.MaterialAttributeValueFreeText).FirstOrDefault();
                //if (attrValueUi != null && attrValueDb != null && attrValueUi.ToUpper() == attrValueDb.ToUpper())
                //    return 1;
                //else
                return 0;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public bool GetAttributeValueValidation1Row(string materialMasterArticleId, string materialAttributeValueId, string materialAttributeId)
        {
            try
            {

                var _sql = @"select b.UserName,a.* from MSt.MaterialMasterArticleValue a
                        left join hkp.MaterialAttributeValue b on a.MaterialAttributeValueId=b.Id
                        where a.MaterialMasterArticleId='" + materialMasterArticleId + @"' and a.MaterialAttributeValueId='" + materialAttributeValueId + @"' AND a.MaterialAttributeId='" + materialAttributeId + "'";
                return Convert.ToBoolean(_valueRepository.SqlQuery<int>(_sql).Single());

                //if (list.Count > 0)
                //{
                //    return false;
                //}
                //else
                //{
                //    return true;
                //}
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool GetAttributeValueValidation2Row(string materialMasterArticleId, string materialAttributeValueId, string materialAttributeId)
        {
            try
            {

                var _sql = @"select b.UserName,a.* from MSt.MaterialMasterArticleValue a
                        left join hkp.MaterialAttributeValue b on a.MaterialAttributeValueId=b.Id
                        where a.MaterialMasterArticleId='" + materialMasterArticleId + @"' and a.MaterialAttributeValueId='" + materialAttributeValueId + @"' AND a.MaterialAttributeId='" + materialAttributeId + "'";
                return Convert.ToBoolean(_valueRepository.SqlQuery<int>(_sql).Single());
                //if (list.Count > 0)
                //{
                //    return false;
                //}
                //else
                //{
                //    return true;
                //}
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Validation

        #region Article Value



        private void InsertUpdateOrDeleteArticleValue(IEnumerable<MaterialMasterArticleValue> entity, string articleId)
        {
            if (entity != null)
            {
                foreach (var item in entity)
                {
                    if (item.Id == 0)//Insert
                    {
                        if (string.IsNullOrEmpty(item.MaterialAttributeValueId) && string.IsNullOrEmpty(item.MaterialAttributeValueFreeText))
                        {
                            //Do Nothing.
                        }
                        else
                        {
                            SetMaterialAttributeValueId(item);
                            item.MaterialMasterArticleId = articleId;
                            AuditService.AddedLog(item);
                            _valueRepository.Insert(item);
                        }
                    }
                    else
                    {
                        //Edit
                        if (string.IsNullOrEmpty(item.MaterialAttributeValueId) && string.IsNullOrEmpty(item.MaterialAttributeValueFreeText))
                        {
                            _valueRepository.Delete(item);
                        }
                        else
                        {
                            SetMaterialAttributeValueId(item);
                            AuditService.UpdatedLog(item);
                            _valueRepository.Update(item);
                        }
                    }
                }
            }
        }

        private void DeleteMaterialMasterArticleValue(MaterialMasterArticle article)
        {
            if (article.MaterialMasterArticleValues != null)
            {
                foreach (var item in article.MaterialMasterArticleValues.ToList())
                {
                    item.ModelState = ModelState.Deleted;
                    _valueRepository.Delete(item);
                }
            }
        }

        private static void SetMaterialAttributeValueId(MaterialMasterArticleValue item)
        {
            if (item.MaterialAttributeValueId != null)//
                item.MaterialAttributeValueFreeText = null;
            else
            {
                if (item.MaterialAttributeValueFreeText == null)
                    throw new CustomException("Free Text can not be null");
            }
        }

        public IEnumerable<object> GetAttributeValueList(string materialMasterId)
        {
            var _sql = @"SELECT A.Id
	                            , A.MaterialAttributeId
	                            , A.MaterialMasterId
	                            , A.MaterialMasterArticleId
	                            , A.MaterialAttributeValueId
	                            , MaterialAttributeValueFreeText=CASE WHEN A.MaterialAttributeValueId<>'' THEN B.UserName
										                            ELSE A.MaterialAttributeValueFreeText END
                                ,MA.UserName  MaterialAttributeName
                        FROM MST.MaterialMasterArticleValue AS A
                        LEFT JOIN HKP.MaterialAttributeValue AS B ON B.Id=A.MaterialAttributeValueId
                        LEFT JOIN HKP.MaterialAttribute AS MA ON B.MaterialAttributeId = MA.Id
                        WHERE A.MaterialMasterArticleId IN (SELECT Id FROM MST.MaterialMasterArticle WHERE MaterialMasterId='" + materialMasterId + "')";
            return _sqlRepository.GetDataCollection(_sql);
        }

        #endregion Article Value

        #region Article Process

        public void ProcessInsertGraph(string productDefinitionId, IEnumerable<MaterialMasterArticle> articleList)
        {
            try
            {
                foreach (var item in articleList)
                {
                    if (item.MaterialMasterArticleProcess != null)
                    {
                        var count = _processRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [MST].[MaterialMasterArticleProcess] WHERE MaterialMasterArticleId='{item.Id}'").First();
                        foreach (var process in item.MaterialMasterArticleProcess)
                        {
                            if (string.IsNullOrEmpty(process.Id))
                            {
                                count++;
                                process.Id = MakePK(item.Id, count, 2);
                                process.ProductDefinitionId = productDefinitionId;
                                AuditService.AddedLog(process);
                                _processRepository.Insert(process);
                            }
                        }
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteArticleProcess(string id)
        {
            try
            {
                var data = _processRepository.Find(id);
                if (data != null)
                {
                    _processRepository.Delete(data);
                    _unitOfWork.SaveChanges();
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteArticleProcessGraphByProductDefinition(string id)
        {
            try
            {
                var data = _processRepository.SqlQuery<MaterialMasterArticleProcess>(@"SELECT * FROM MST.MaterialMasterArticleProcess WHERE ProductDefinitionId='" + id + "'").ToList();
                if (data != null)
                    _processRepository.Delete(data);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private void DeleteArticleProcessGraphByArticle(string articleId)
        {
            try
            {
                var data = _processRepository.SqlQuery<MaterialMasterArticleProcess>(@"SELECT * FROM MST.MaterialMasterArticleProcess WHERE MaterialMasterArticleId='" + articleId + "'").ToList();
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        item.ModelState = ModelState.Deleted;
                        _processRepository.Delete(item);
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        #endregion Article Process

        #region Machine Attribute

        public void UpdateGraph(IEnumerable<MaterialMasterArticle> entities)
        {
            try
            {
                foreach (var item in entities)
                {
                    base.UpdateGraph(item);
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        #endregion Machine Attribute
    }
    public class MaterialMasterArticleNew : BaseModel
    {

        public List<MaterialMasterArticleValue> MaterialMasterArticleValues { get; set; }
        public string MaterialMasterId { get; set; }
        public virtual MaterialMaster MaterialMaster { get; set; }
        public string UpdatedFromIP { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedBy { get; set; }
        public decimal MachineAllowance { get; set; }
        public int RPM { get; set; }
        public string StandardName { get; set; }
        public string ShortName { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; }
        public string HSNCodeId { get; set; }
        public string Id { get; set; }
        public string StitchCodeId { get; set; }
        public bool IsWorkCenterApplicable { get; set; }
        public bool IsMachineApplicable { get; set; }
        public string OrderLevel { get; set; }
    }
}