using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Library.MaterialManagement.Material
{
    public class clsMaterial
    {
        ISqlRepository _sqlRepository;
        public clsMaterial()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetCharacteristicsValueCboByCharacteristicsIdAfterSave(string materialMasterId, string characteristicsId, string valueAssignmentLevel,string MarkerMasterId)
        {
            try
            {
                var _sql = string.Empty;
                if (valueAssignmentLevel == ValueAssignmentEnum.Specific.ToString())

                    _sql = @"SELECT IsSelect = case when M.Id is null then Convert(bit, 'False')ELSE Convert(bit, 'True') END  ,CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] 
                                ,Ratio = case when M.Id is null then null else M.Ratio end,M.Id,M.MarkerMasterId
                                FROM [HKP].[Characteristics] C
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
							left join MarkerDetails M on M.CharacteristicsValueId=CV.Id and M.MarkerMasterId='" + MarkerMasterId + @"'
                            Where CV.MaterialMasterId='" + materialMasterId + "' AND CV.CharacteristicsId='" + characteristicsId + "' AND  C.ValueAssignmentLevel='" + valueAssignmentLevel + "'  Order by CV.Sequence";

                else

                    _sql = @"SELECT IsSelect = case when M.Id is null then Convert(bit, 'False')ELSE Convert(bit, 'True') END  ,CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] 
                                ,Ratio = case when M.Id is null then null else M.Ratio end,M.Id,M.MarkerMasterId
                                FROM [HKP].[Characteristics] C
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
							left join MarkerDetails M on M.CharacteristicsValueId=CV.Id and M.MarkerMasterId='" + MarkerMasterId + @"'
                            Where CV.CharacteristicsId='" + characteristicsId + "' AND  C.ValueAssignmentLevel='" + valueAssignmentLevel + "' AND  CV.SourceType='" + valueAssignmentLevel + "'  Order by CV.Sequence";
                return _sqlRepository.GetDataCollection(_sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetCharacteristicsValueCboByCharacteristicsId(string materialMasterId, string characteristicsId, string valueAssignmentLevel)
        {
            try
            {
                var _sql = string.Empty;
                if (valueAssignmentLevel == ValueAssignmentEnum.Specific.ToString())

                    _sql = @"SELECT IsSelect =Convert(bit, 'False') ,CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] ,'' Ratio,null Id FROM [HKP].[Characteristics] C
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
                            Where CV.MaterialMasterId='" + materialMasterId + "' AND CV.CharacteristicsId='" + characteristicsId + "' AND  C.ValueAssignmentLevel='" + valueAssignmentLevel + "'  Order by CV.Sequence";

                else

                    _sql = @"SELECT IsSelect =Convert(bit, 'False'),CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] ,'' Ratio,null Id  FROM [HKP].[Characteristics] C
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
                            Where CV.CharacteristicsId='" + characteristicsId + "' AND  C.ValueAssignmentLevel='" + valueAssignmentLevel + "' AND  CV.SourceType='" + valueAssignmentLevel + "' Order by CV.Sequence";
                return _sqlRepository.GetDataCollection(_sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    
}
