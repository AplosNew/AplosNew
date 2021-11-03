using Library.Core;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.Banks
{
   
        public interface ICheckLotNewService : IService<CheckLot>
        {
        void UpdateCheckLot(CheckLot checkLot);
        IEnumerable<ComboModel> GetCbo(string checkLotId, bool isNonSequential);
        IEnumerable<ComboModel> GetExistingCbo(string checkLotId, bool isNonSequential);
        IEnumerable<object> GetCbo(string bankMasterId);

        IEnumerable<ComboModel> GetCbo1(string checkLotId, bool isNonSequential);
       // IEnumerable<object> GetCbo1(string bankMasterId);

        //IEnumerable<object> GetCbo1();

        void UpdateGraphAndPrint(string voucherDetailId, int checkLotDetailId, decimal amount, string checkDate, string printBy,string party,string partyBankId,string partyAccount);
        void UpdateGraphAndRePrint(string voucherDetailId, int checkLotDetailId, decimal amount, string checkDate, string printBy, string party, string partyBankId, string partyAccount);

        void UpdateGraphAndPrintCashCheck(string voucherDetailId, int checkLotDetailId, decimal amount, string checkDate, string printBy);

        void UpdateGraphAndCashChequeRePrint(string voucherDetailId, int checkLotDetailId, decimal amount, string checkDate, string printBy);

        void UpdateGraphAndCheckVoidPrint(string voucherDetailId, int checkLotDetailId, decimal amount, string checkDate, string printBy);
        
    }

}