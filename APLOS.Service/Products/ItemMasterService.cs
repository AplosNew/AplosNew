#region

using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Products;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion

namespace Library.Service.Products
{
    public class ItemMasterService : Service<ItemMaster>, IItemMasterService
    {
        #region Constructor
        private readonly IRepositoryAsync<ItemMaster> _itemMasterRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ItemMasterService(
            IRepositoryAsync<ItemMaster> itemMasterRepository,
            IUnitOfWork unitOfWork) :
            base(itemMasterRepository, unitOfWork)
        {
            _itemMasterRepository = itemMasterRepository;
            _unitOfWork = unitOfWork;
        }

        #endregion

        //#region Query
        //public override IEnumerable<ItemMaster> Query()
        //{
        //    try
        //    {
        //        return _itemMasterRepository.Query(r => !r.IsArchive).Select().OrderBy(r=>r.Sequence);
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}
        //#endregion

        #region GetItemMasterList

        public IEnumerable<object> GetItemMasterList()
        {
            try
            {
                return from m in _itemMasterRepository.Query(r => r.IsActive && !r.IsArchive)
                       select new { Text = m.StandardName, Value = m.Id };
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region ItemMaster

        public IEnumerable<ItemMaster> GetAllById(string Id)
        {
            try
            {
                return _itemMasterRepository.Query(r => r.Id == Id).Select();
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}