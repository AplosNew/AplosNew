using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Core;

namespace Library.Model.EmployeeServices
{
    public class ServiceTypeAndCategory : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string Service { get; set; }
        public string Form { get; set; }
        public string UOMId { get; set; }
        public string UOM { get; set; }
        public string Category { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

    }

    public class ServiceCategory : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string ServiceMasterId { get; set; }
        public string ServiceId { get; set; }
        public string Service { get; set; }
        public string Category { get; set; }


        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

    }

}
