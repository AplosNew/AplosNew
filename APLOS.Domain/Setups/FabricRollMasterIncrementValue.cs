using Library.Core;
using Library.Model.Organizations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Model.Setups
{
    public class FabricRollMasterIncrementValue : BaseModel
    {
        #region Scalar Properties
        public int Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string PlantId { get; set; } 
        public int IncrementValue { get; set; }
        #endregion Scalar Properties
    }
}