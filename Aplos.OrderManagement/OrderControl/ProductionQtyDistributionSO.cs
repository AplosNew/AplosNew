namespace Library.OrderManagement.OrderControl
{
    internal class ProductionQtyDistributionSO
    {
        public double DistributedQty { get; internal set; }
        public double CumulativeQty { get; internal set; }
        public double ProducedQtyToday { get; internal set; }
        
    }
}