using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompeTournament.Backend.Data.Configurations
{
    public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
    {

        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(50);
            builder.Property(x => x.Lastname).HasMaxLength(50);
        }

        //public ApplicationUserConfig(EntityTypeBuilder<ApplicationUser> entityBuilder)
        //{
        //    entityBuilder.HasKey(x => x.Id);

        //    entityBuilder.Property(x => x.Name).HasMaxLength(50);
        //    entityBuilder.Property(x => x.Lastname).HasMaxLength(50);

        //    //entityBuilder.HasOne(e => e.Shop)
        //    //    .WithMany(s => s.ApplicationUsers)
        //    //    .HasForeignKey(s => s.ShopId)
        //    //    .IsRequired();

        //    //entityBuilder.HasOne(e => e.UserType)
        //    //    .WithMany(s => s.ApplicationUsers)
        //    //    .HasForeignKey(s => s.UserTypeId)
        //    //    .IsRequired();


        //}


    }
}
