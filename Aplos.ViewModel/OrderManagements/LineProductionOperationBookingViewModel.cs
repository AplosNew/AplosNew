using Library.Core;
using System;

namespace Library.ViewModel.OrderManagements
{
    public class LineProductionOperationBookingViewModel : BaseModel
    {
        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }
        public string PlantName { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime OperationDate { get; set; }
        public string Line { get; set; }
        public string ProductionShift { get; set; }
        public string SalesOrder { get; set; }
        public string Fabrication { get; set; }
        public string Style { get; set; }
        public decimal ProductionQty { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalManPower { get; set; }
        public decimal PlanRunMC { get; set; }
        public decimal ActualRunMC { get; set; }
        public decimal ExtraMC { get; set; }
        public decimal TrimCheckPress { get; set; }
        public decimal SewingSMV { get; set; }
        public decimal TotalSMV { get; set; }
        public decimal MCMINAvailable { get; set; }
        public decimal NonMCMINAvailable { get; set; }
        public decimal TotalMINAvailable { get; set; }
        public decimal ActualMINWorked { get; set; }
        public decimal MCSAMProd { get; set; }
        public decimal TotalSAMProd { get; set; }
        public decimal MCEfficiency { get; set; }
        public decimal OrderQty { get; set; }
        public decimal TargetQuantity { get; set; }
        public int OperatorQty { get; set; }
        public decimal Amount { get; set; }
        public string DefaultWeekOff { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }

        public string MachineType { get; set; }
        public string OperationType { get; set; }
        public string OperationName { get; set; }
        public decimal Target { get; set; }
        public decimal Rate { get; set; }
        public decimal BasicSalary { get; set; }
        public string LineOperationBookingId { get; set; }
        public string LineProductionBookingId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string TempEmployeeId { get; set; }
        public bool NoApplicablePcsRate { get; set; }

	}
}

