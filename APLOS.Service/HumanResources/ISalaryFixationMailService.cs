#region Using

using Library.Model.HumanResources;
using Library.Service.Core;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface ISalaryFixationMailService : IService<SalaryFixationMail>
    {
        void InsertOrUpdateSFMail(string PreReceuitmentEmployeeId, string PlantId);

        void SaveSFM(string PreReceuitmentEmployeeId, string PlantId);
    }
}