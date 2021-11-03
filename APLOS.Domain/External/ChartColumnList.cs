using Library.Core;

namespace Library.Model.External
{
    public class ChartColumnList : BaseModel
    {
        public string Id { get; set; }
        public string ColumnName { get; set; }
        public string AplosColumnName { get; set; }
        public int Sequence { get; set; }
        public string Text { get; set; }
    }
}