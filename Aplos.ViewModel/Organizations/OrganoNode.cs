using System.Collections.Generic;

namespace Library.ViewModel.Organizations
{
    public class OrganoNode
    {
        public string name { get; set; }
        public int size { get; set; }
        public List<OrganoNode> children { get; set; }

        public OrganoNode()
        {
        }

        public OrganoNode(string Name, List<OrganoNode> Children)
        {
            this.name = Name;
            this.children = Children;
        }
    }
}