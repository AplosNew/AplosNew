using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class ProjectPlanningPORequisitionMaterialMasterArticleConfiguration : EntityTypeConfiguration<ProjectPlanningPORequisitionMaterialMasterArticle>
    {
        public ProjectPlanningPORequisitionMaterialMasterArticleConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(ProjectPlanningPORequisitionMaterialMasterArticle), DbSchema.Masters);
        }
    }
}