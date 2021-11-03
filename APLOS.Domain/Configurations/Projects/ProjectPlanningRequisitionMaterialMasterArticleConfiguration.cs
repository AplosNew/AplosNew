using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningRequisitionMaterialMasterArticleConfiguration : EntityTypeConfiguration<ProjectPlanningRequisitionMaterialMasterArticle>
    {
        public ProjectPlanningRequisitionMaterialMasterArticleConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(ProjectPlanningRequisitionMaterialMasterArticle), DbSchema.Masters);
        }
    }
}