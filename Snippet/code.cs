DataTable dt2 = DataUtil.ConvertToDataTable(dt.Rows.Cast<DataRow>().Take(1).ToList());
//Property(t => t.CM).HasPrecision(18, 4);
var data = viewModel.GroupBy(t => new { t.ProductionDate, t.Line }).Select(x => x.First());
int count = _legalSalaryGradeHeadRepository.CreateChildPk(t => t.LegalSalaryGradeId == pk, x => x.Id, pk).ToInt();
//(CheckProcessIdUseInOperationMachineType(new[] { item.ProcessId }, machineTypeId))
private void IsCheckNumberExist(string bankMasterId, int fromNo, int toNo)
{
    try
    {
        var sql = @"SELECT MIN(CD.CheckNumber) AS MinNo,  MAX(CD.CheckNumber) AS MaxNo
                            FROM TRn.CheckLotDetail AS CD
                            INNER JOIN TRN.CheckLot AS C ON CD.CheckLotId=C.Id
                            WHERE C.BankMasterId='" + bankMasterId + "' AND CD.CheckNumber>='" + fromNo + "' AND CD.CheckNumber<='" + toNo + "'";
        var data = _sqlDbContext.ModelData(sql, null);
        if (!string.IsNullOrEmpty(data["MinNo"].ToString()))
        {
            var minNo = data["MinNo"].ToString();
            var maxNo = data["MaxNo"].ToString() == "" ? "" : data["MaxNo"].ToString();

            throw new CustomException("This bank master contain check number [" + minNo + " to " + maxNo + "]");
        }
    }
    catch (Exception ex)
    {
        throw new CustomException(ex.Message, ex,
            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
    }
}
var data = base.Query(t => t.MaterialMasterId == materialMasterId).Include(t => t.MaterialAttributeValues.Select(a => a.MaterialAttribute)).Select();
foreach (var item in data)
{
    foreach (var item2 in item.MaterialAttributeValues)
    {
        if (!string.IsNullOrEmpty(item2.MaterialAttributeId))
        {
            // statments............
            item2.MaterialAttributeValueFreeText = _materialAttributeValueService.Query(t => t.MaterialAttributeId == item2.MaterialAttributeId
                        && t.Id == item2.MaterialAttributeValueId)
                .Select(t => t.Description).FirstOrDefault();
        }

    }
}

public IEnumerable<object> GetMaterialTypeNatureListCbo()
{
    return Enum.GetValues(typeof(EnumMaterialTypeNatureList)).Cast<EnumMaterialTypeNatureList>().Select(v => new
    {
        Text = v.ToString(),
        Value = v.ToString()
    });
}

private bool CheckIdUse(string companyId, string[] processIds)
{
    try
    {
        var process = "";
        if (processIds != null && processIds.Length > 0)
            process = string.Join(",", processIds.Select(item => "'" + item + "'"));
        else
            process = "' '";
        string sql = @"IF EXISTS(SELECT 1 FROM(
                                SELECT A.CheckingColumn1,B.CheckingColumn2 FROM
                                (SELECT Id,CompanyId AS CheckingColumn1 FROM HKP.ProcessSet) AS A LEFT OUTER JOIN
                                (SELECT ProcessSetId,ProcessId AS CheckingColumn2 FROM HKP.ProcessSetDetail ) AS B ON A.Id=B.ProcessSetId
                               ) AA WHERE CheckingColumn1 ='" + companyId + "' AND CheckingColumn2 IN (" + process + ")) SELECT 1 ELSE SELECT 0 RETURN";
        return Convert.ToBoolean(_sqlDbContext.ExecuteSqlCommand(sql));
    }
    catch
    {
        throw;
    }
}
//public IEnumerable<object> GetCbo(string companyId)
//{
//    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//    string _sql = "SELECT DMC.Id AS Value, " +
//                           "DM.UserName AS Text  " +
//                    "FROM MST.CompanyDesignation AS DMC  " +
//                    "LEFT OUTER JOIN MST.DesignationMaster AS DM ON DMC.DesignationMasterId=DM.Id  " +
//                    $"WHERE DMC.CompanyGroupId='{identity.CompanyGroupId}' AND DMC.CompanyId='{companyId}' AND DMC.Archive=0 AND DM.Active= 1 " +
//                    "ORDER BY DM.UserName ";
//    return _sqlDbContext.ModelDataCollection(_sql, null);
//}

public IEnumerable<ComboModel> GetCbo(string companyId)
{
    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    string _sql = "SELECT D.Id, " +
                         "D.UserName " +
                    "FROM MST.CompanyDesignation AS DMC  " +
                    "LEFT OUTER JOIN MST.DesignationMaster AS DM ON DMC.DesignationMasterId=DM.Id  " +
                    "LEFT OUTER JOIN HKP.Designation AS D ON DM.DesignationId=D.Id " +
                    $"WHERE DMC.CompanyGroupId='{identity.CompanyGroupId}' AND DMC.CompanyId='{companyId}' AND DMC.Archive=0 AND DM.Active= 1 " +
                    "ORDER BY D.UserName ";
    return _sqlRepository.GetCombo(_sql, "Id", "UserName");
}
public IEnumerable<object> GetCboList(string companyGroupId)
{
    try
    {
        return (from m in base.Query(t => t.CompanyGroupId == companyGroupId && t.Active).Select().OrderBy(t => t.UserName)
                select new { Text = m.UserName, Value = m.Id }).Distinct();
    }
    catch (Exception ex)
    {
        throw new CustomException(ex.Message, ex,
            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Menu.ToString()));
    }
}

[Authorize]
public JsonResult GetMenuItemGroupList()
{
    return Json(new SelectList(_menuItemService.GetMenuItemGroupList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
}


private void IsUseInMaster(string id)
{
    try
    {
        string sql = @"IF EXISTS(SELECT 1 FROM(
                                SELECT MaterialGroupMasterId AS CheckingColumn FROM MST.MaterialAttributeMaster WHERE Archive=0
                                ) A WHERE CheckingColumn = '" + id + "') SELECT 1 ELSE SELECT 0 RETURN ";
        var data = Convert.ToBoolean(_subMaterialRepository.SqlQuery<int>(sql).Single());
        if (data)
            throw new CustomException("Already designation master exist, you can't delete....!");
    }
    catch (CustomException cx)
    {
        throw cx;
    }
    catch (Exception ex)
    {
        throw new CustomException(ex.Message, ex,
            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Designation.ToString()));
    }
}
public IEnumerable<OperationTimeCaptureDetail> GetOperationTimeCaptureDetailList_tested(string MasterId)
{
    try
    {
        string _sql = "select * from trn.OperationTimeCaptureDetail";
        //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
        return Mapper.LoadModelCollection<OperationTimeCaptureDetail>(_sql, null);
    }
    catch (Exception ex)
    {
        throw (ex);
    }
}
public OperationTimeCaptureDetail GetOperationTimeCaptureDetail(string MasterId)//TBT
{
    try
    {
        string _sql = "select * from trn.OperationTimeCaptureDetail";
        return _operationtimecapturedetailservice.SelectQuery(_sql, null);
    }
    catch (Exception ex)
    {
        throw (ex);
    }
}
public IEnumerable<object> GetOperationTimeCaptureDetailList(string MasterId)
{
    try
    {
        string _sql = "select * from trn.OperationTimeCaptureDetail";
        //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
        return _sqlDbContext.ModelDataCollection(_sql, null);
    }
    catch (Exception ex)
    {
        throw (ex);
    }
}
string stringPrefix = "";
string branchIdWithPad = branchId.PadLeft(2, '0');
stringPrefix = "" + companyId + "" + branchIdWithPad + "";
        int id = 0;
var idList = this._itemStockRepository.GetAll().Select(x => x.Id).ToList();
        if (idList.Count() != 0)
        {
            id = idList.Max(x => Convert.ToInt32(x.Substring(stringPrefix.Length)) + 1);
        }
        else
        {
            id = 1;
        }


		 string id = CreatePk(skillId);
int count = id.ToInt();
item.Id = skillId + "-" + count;

private string CreatePk(string skillId)
{
    try
    {
        string id = string.Empty;
        var Db_Pk = base.Query(t => t.SkillId == skillId).Select(t => t.Id).AsEnumerable();
        if (Db_Pk.Count() != 0)
        {
            id = Db_Pk.Max(x => Convert.ToInt32(x.Substring(skillId.Length + 1)) + 1).ToString();
        }
        else
        {
            id = "1";
        }
        return id;
    }
    catch (Exception ex)
    {
        throw new CustomException(ex.Message, ex,
        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
    }
}

public ActionResult GetMachineTypeByProcessIdWithoutMTId(GridParameter parameters, string processId, string machineTypeIds)
{
    return Json(_machineTypeService.GetMachineType(parameters, processId, new JavaScriptSerializer().Deserialize<string[]>(machineTypeIds)), JsonRequestBehavior.AllowGet);
}

public GridModel ProcessSearch(GridParameter parameters, string[] processIds)
{
    try
    {
        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        var process = "";
        if (processIds.Length > 0)
            process = string.Join(",", processIds.Select(item => "'" + item + "'"));
        else
            process = "' '";
        parameters.cmdText = @"SELECT P.Id
                                        ,P.[Sequence]
                                        ,P.Code
                                        ,P.UserName
                                        ,P.LocalName
                                        ,P.Alias
                                        ,MT.[Description] AS MaterialType
                                        ,P.Active
                                        ,'' AS Flag
                                FROM HKP.Process AS P
                                LEFT OUTER JOIN HKP.MaterialType AS MT ON P.MaterialTypeId=MT.Id
                                WHERE P.CompanyGroupId='" + identity.CompanyGroupId + "' AND P.Id NOT IN(" + process + ") AND P.Archive=0";
        return base.Query(parameters);
    }
    catch (Exception ex)
    {
        throw (ex);
    }
}

//***************************file save to folder(out of project scope)*********************************//
 if (file != null)
            {
                var directory = new AppSettingsReader().GetValue("USERPIC", typeof(string)).ToString(); //get pic url from web config
                if (!Directory.Exists(directory)) //CreateDirectory
                    Directory.CreateDirectory(directory);
                string path = Path.Combine(directory, preRecruitmentEmployees.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else 
                    file.SaveAs(path);
            }
//************************************************************//



//******************** db list del********************************//

		if (dbList.IsNotNull()&& dbList.Count>0)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                base.DeleteGraph(item);
                            }
                        }
                    }
                }



//_sqlDbContext.FKDependency("[ORG].[Position]", id)
private void UseChecking(string id)
{
    if (_sqlDbContext.FKDependency("[ORG].[Position]", id))
        throw new CustomException("Update or Delete is not allowed after transaction.");
}


private void IsCheckNumberExist(string bankMasterId, int fromNo, int toNo)
{
    try
    {
        var sql = @"SELECT MIN(CD.CheckNumber) AS MinNo,  MAX(CD.CheckNumber) AS MaxNo
                            FROM TRn.CheckLotDetail AS CD
                            INNER JOIN TRN.CheckLot AS C ON CD.CheckLotId=C.Id
                            WHERE C.BankMasterId='" + bankMasterId + "' AND CD.CheckNumber>='" + fromNo + "' AND CD.CheckNumber<='" + toNo + "'";
        var data = _sqlDbContext.ModelData(sql, null);
        if (!string.IsNullOrEmpty(data["MinNo"].ToString()))
        {
            var minNo = data["MinNo"].ToString();
            var maxNo = data["MaxNo"].ToString() == "" ? "" : data["MaxNo"].ToString();

            throw new CustomException("This bank master contain check number [" + minNo + " to " + maxNo + "]");
        }
    }
    catch (Exception ex)
    {
        throw new CustomException(ex.Message, ex,
            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
    }
}
var count = _productDefinitionRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ProductDefinitionEfficency] WHERE ProductDefinitionId='{entity.Id}'").First();
foreach (var item in efficencyList)
{
    count++;
    item.Id = MakePK(entity.Id, count, 2);
item.ProductDefinitionId = entity.Id;
    AuditService.AddedLog(item);
    _efficencyRepository.Insert(item);
}
