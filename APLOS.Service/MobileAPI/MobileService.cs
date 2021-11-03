using Library.Core;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Materials;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.MobileAPI
{
    public class MobileService : Service<OperationMasterData>, IMobileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ICompanyGroupCharacteristicsService _comgroupcharService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<OperationMasterData> _charaterRepository;
        public MobileService(
            IRepositoryAsync<OperationMasterData> charaterRepository,
            IPKGeneratorService pkGeneratorService,
            ICompanyGroupCharacteristicsService comgroupcharService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(charaterRepository, unitOfWork)
        {
            _charaterRepository = charaterRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _comgroupcharService = comgroupcharService;
            _sqlRepository = sqlRepository;
        }

        #region data Access Queries


        #endregion data access Queries


        #region functions for Operation-Employee Tag

        public List<OperationMasterData> SearchOperationMasterData(string strKey, string CompanyGroupId)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            List<OperationMasterData> DataList = new List<OperationMasterData>();
            System.Data.DataSet dsRef;
            try
            {
                if (string.IsNullOrEmpty(strKey))
                    strKey = "1=1";
                else
                    strKey = "isnull(code,'')+isnull(username,'') like '%" + strKey + "%'";


                strSql = @"select * from (select * from mst.OperationMaster where CompanyGroupId='" + CompanyGroupId + "') AS K where " + strKey;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new OperationMasterData
                    {
                        ID = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        Code = dsRef.Tables[0].Rows[i]["Code"].ToString(),
                        UserName = dsRef.Tables[0].Rows[i]["UserName"].ToString(),

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
            return DataList;
        }
        public List<OperationMasterData> GetOperationMasterData(string id)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            List<OperationMasterData> DataList = new List<OperationMasterData>();
            System.Data.DataSet dsRef;
            try
            {


                strSql = @"select * from mst.OperationMaster where id='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new OperationMasterData
                    {
                        ID = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        Code = dsRef.Tables[0].Rows[i]["Code"].ToString(),
                        UserName = dsRef.Tables[0].Rows[i]["UserName"].ToString(),

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
            return DataList;
        }



        #endregion functions for Operation-Employee Tag


    }


    public class OperationMasterData : BaseModel
    {
        public string ID { get; set; } = "";
        public string Code { get; set; } = "";
        public string UserName { get; set; } = "";
    }
}
