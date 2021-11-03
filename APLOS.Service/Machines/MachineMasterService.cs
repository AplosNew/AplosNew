#region

using Library.Data;
using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Machines;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace Library.Service.Machines
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   A machine master service. </summary>
    /// <summary>   Author:Belayet Hossain, Date:06-Feb-2016. </summary>
    ///-------------------------------------------------------------------------------------------------
    public class MachineMasterService : Service<MachineMaster>, IMachineMasterService
    {
        #region Constructor

        /// <summary>   The unit of work. </summary>
        private readonly IUnitOfWork _unitOfWork;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        /// <param name="machineMasterRepository">  The machine master repository. </param>
        /// <param name="unitOfWork">               The unit of work. </param>
        ///-------------------------------------------------------------------------------------------------
        public MachineMasterService(
            IRepositoryAsync<MachineMaster> machineMasterRepository,
            IUnitOfWork unitOfWork) :
            base(machineMasterRepository, unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #endregion

        #region GetAutoSequence

        /// <summary>
        /// Query auto sequence number.
        /// </summary>
        /// <returns>decimal</returns>
        public decimal GetAutoSequence()
        {
            try
            {
                return Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        #endregion

        //#region Query
        //public override IEnumerable<MachineMaster> Query()
        //{
        //    try
        //    {
        //        return _machineMasterRepository.Query(r => !r.IsArchive).Select().OrderBy(r=> r.Sequence);
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}
        //#endregion

        #region GetMachineMasterList

        public IEnumerable<object> GetMachineMasterList()
        {
            try
            {
                return from m in Query(r => r.Active)
                       select new { Text = m.Id, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Menu.ToString()));
            }
        }

        #endregion

        #region GetAllById

        public IEnumerable<MachineMaster> GetAllById(string Id)
        {
            try
            {
                return Query(r => r.Id == Id).Select();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Menu.ToString()));
            }
        }

        #endregion
    }
}