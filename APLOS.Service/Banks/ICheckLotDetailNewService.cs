using Library.Core;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.Banks
{
   
        public interface ICheckLotDetailNewService : IService<CheckLotDetail>
        {
        void DetailUpdate(CheckLot checkLot);

        }

}