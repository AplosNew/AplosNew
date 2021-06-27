using Library.Core;

namespace Library.ViewModel.Organizations
{
    public class OrgStructureListViewModel : BaseModel
    {
        public string Id { get; set; }
        public string StandardName { get; set; }
        public string ColumnName { get; set; }
        public string RType { get; set; }
        public int? Sequence { get; set; }
        public string Text { get; set; }
    }

}