namespace Domain
{
    using System.Data.Entity.ModelConfiguration;

    internal class GroupsMap : EntityTypeConfiguration<Group>
    {
        public GroupsMap()
        {
            HasRequired(o => o.Owner)
                .WithMany(m => m.UserGroups)
                .HasForeignKey(m => m.OwnerId);
        }

    }
}