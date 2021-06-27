using Library.Model.Enums;
using Library.Model.Products;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class ItemMasterConfiguration : EntityTypeConfiguration<ItemMaster>
    {
        public ItemMasterConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);

            // Properties
            Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(t => t.StandardName)
                .HasMaxLength(50);

            Property(t => t.Description)
                .HasMaxLength(50);

            Property(t => t.ShortName)
                .HasMaxLength(50);

            Property(t => t.UserName)
                .HasMaxLength(50);

            Property(t => t.Code)
                .HasMaxLength(50);

            Property(t => t.Sequence)
                .HasMaxLength(50);

            Property(t => t.Remarks)
                .HasMaxLength(50);

            Property(t => t.ItemTypeId)
                ;

            Property(t => t.ProcurementCategoryId)
                ;

            Property(t => t.ProcurementBaseId)
                ;

            Property(t => t.ProcurementFrequencyId)
                ;

            Property(t => t.PaymentPolicyId)
                ;

            Property(t => t.PaymentTermId)
                ;

            Property(t => t.DependentDateId)
                ;

            Property(t => t.AT73C18)
                .HasMaxLength(1);

            Property(t => t.AT73C19)
                .HasMaxLength(1);

            // Table & Column Configuration
            ToTable(nameof(ItemMaster), DbSchema.Masters);
            Property(t => t.Id).HasColumnName("ID");
            Property(t => t.StandardName).HasColumnName("StandardName");
            Property(t => t.Description).HasColumnName("Description");
            Property(t => t.ShortName).HasColumnName("ShortName");
            Property(t => t.UserName).HasColumnName("UserName");
            Property(t => t.Code).HasColumnName("Code");
            Property(t => t.Sequence).HasColumnName("Sequence");
            Property(t => t.Remarks).HasColumnName("Remarks");
            Property(t => t.IsActive).HasColumnName("IsActive");
            Property(t => t.ItemTypeId).HasColumnName("ItemTypeID");
            Property(t => t.ProcurementCategoryId).HasColumnName("ProcurementCatID");
            Property(t => t.ProcurementBaseId).HasColumnName("ProcurementBasID");
            Property(t => t.ProcurementFrequencyId).HasColumnName("ProcurementFreqID");
            Property(t => t.PaymentPolicyId).HasColumnName("PaymentPolicyID");
            Property(t => t.PaymentTermId).HasColumnName("PaymentTermID");
            Property(t => t.DependentDateId).HasColumnName("DependentDateID");
            Property(t => t.ItemCategoryId).HasColumnName("ItemCategoryID");
            Property(t => t.ItemSubCategoryId).HasColumnName("ItemSubCategoryID");
            Property(t => t.ItemId).HasColumnName("ItemID");
            Property(t => t.AT73C18).HasColumnName("AT73C18");
            Property(t => t.AT73C19).HasColumnName("AT73C19");
            Property(t => t.IsArchive).HasColumnName("IsArchive");
            //Relationship
            //this.HasOptional(c => c.DependentDate).WithMany(c => c.ItemMasters).HasForeignKey(c => c.DependentDateId);
            //this.HasOptional(c => c.ItemType).WithMany(c => c.ItemMasters).HasForeignKey(c => c.ItemTypeId);
            //this.HasOptional(c => c.ProcurementCategory).WithMany(c => c.ItemMasters).HasForeignKey(c => c.ProcurementCategoryId);
            //this.HasOptional(c => c.ProcurementBase).WithMany(c => c.ItemMasters).HasForeignKey(c => c.ProcurementBaseId);
            //this.HasOptional(c => c.ProcurementFrequency).WithMany(c => c.ItemMasters).HasForeignKey(c => c.ProcurementFrequencyId);
            //this.HasOptional(c => c.PaymentPolicy).WithMany(c => c.ItemMasters).HasForeignKey(c => c.PaymentPolicyId);
            //this.HasOptional(c => c.PaymentTerm).WithMany(c => c.ItemMasters).HasForeignKey(c => c.PaymentTermId);
            //this.HasOptional(c => c.ItemCategory).WithMany(c => c.ItemMasters).HasForeignKey(c => c.ItemCategoryId);
            //this.HasOptional(c => c.ItemSubCategory).WithMany(c => c.ItemMasters).HasForeignKey(c => c.ItemSubCategoryId);
            //this.HasOptional(c => c.Item).WithMany(c => c.ItemMasters).HasForeignKey(c => c.ItemId);
        }
    }
}