using Library.Core;
using System;

namespace Library.Model.Payrolls
{
    public class XLUploadMaster : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string PlantId { get; set; }
        public DateTime? PDate { get; set; }
        public bool ProcessStatus { get; set; }
        public string ProcessMessage { get; set; }
        public string ProcessName { get; set; }
        public string UploadingFlag { get; set; }

        #endregion Scalar Properties

        #region Audit Properties


        #endregion Audit Properties
    }
}