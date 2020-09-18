namespace CompeTournament.Backend.Data
{
    using CompeTournament.Backend.Data.Configurations;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Backend.Extensions;
    using CompeTournament.Backend.Helpers;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ICurrentUserFactory _currentUser;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserFactory currentUser = null) : base(options)
        {
            _currentUser = currentUser;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            base.OnModelCreating(modelBuilder);
            AddMyFilters(ref modelBuilder);
            // modelBuilder.ApplyAllConfigurations();
            modelBuilder.ApplyConfiguration(new ApplicationUserConfig());
            //registering the configurations for all the classes
            //  new ApplicationUserConfig(modelBuilder.Entity<ApplicationUser>());
            // new OwnerConfig(modelBuilder.Entity<Owner>());
            //new ShopConfig(modelBuilder.Entity<Shop>());

            //if I want to remove the AspNet prefix from the identity tables
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var table = entityType.Relational().TableName;
                if (table.StartsWith("AspNet"))
                {
                    entityType.Relational().TableName = table.Substring(6);
                }
            };


        }

        #region Tables
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<TournamentType> TournamentTypes { get; set; }
       // public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<GroupUser> GroupUsers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Prediction> Predictions { get; set; }
        #endregion

        public override int SaveChanges()
        {
            MakeAudit();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            MakeAudit();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            MakeAudit();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void MakeAudit()
        {
            var modifiedEntries = ChangeTracker.Entries().Where(
                x => x.Entity is AuditEntity
                    && (
                    x.State == EntityState.Added
                    || x.State == EntityState.Modified
                )
            );

            var user = new CurrentUser();

            if (_currentUser != null)
            {
                user = _currentUser.Get;
            }

            foreach (var entry in modifiedEntries)
            {
                var entity = entry.Entity as AuditEntity;

                if (entity != null)
                {
                    var date = DateTime.Now;
                    string userId = user.UserId;

                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedAt = date;
                        entity.CreatedBy = userId;
                    }
                    else if (entity is ISoftDeleted && ((ISoftDeleted)entity).Deleted)
                    {
                        entity.DeletedAt = date;
                        //  entity.DeletedBy = userId;
                    }

                    Entry(entity).Property(x => x.CreatedAt).IsModified = false;
                    Entry(entity).Property(x => x.CreatedBy).IsModified = false;

                    entity.UpdatedAt = date;
                    // entity.UpdatedBy = userId;
                }
            }
        }

        private void AddMyFilters(ref ModelBuilder modelBuilder)
        {
            var user = new CurrentUser();

            if (_currentUser != null)
            {
                user = _currentUser.Get;
            }

            #region SoftDeleted
            //modelBuilder.Entity<ApplicationUser>().HasQueryFilter(x => x.Owner.Id == user.OwnerId && !x.Deleted);
            //modelBuilder.Entity<Wallet>().HasQueryFilter(x => x.CreatedUser.Owner.Id == user.OwnerId && !x.State);
            // modelBuilder.Entity<ApplicationUser>().HasQueryFilter(x => !x.Deleted);
            //modelBuilder.Entity<Course>().HasQueryFilter(x => !x.Deleted);
            //modelBuilder.Entity<Doctor>().HasQueryFilter(x => !x.Deleted);
            //modelBuilder.Entity<Patient>().HasQueryFilter(x => !x.Deleted);
            //modelBuilder.Entity<Person>().HasQueryFilter(x => !x.Deleted);
            //modelBuilder.Entity<Owner>().HasQueryFilter(x => !x.Deleted);
            //modelBuilder.Entity<Shop>().HasQueryFilter(x => !x.Deleted);

            // modelBuilder.Entity<Wallet>().HasQueryFilter(x => !x.State);
            //modelBuilder.Entity<LikesPerPhoto>().HasQueryFilter(x => !x.Deleted);
            //modelBuilder.Entity<Photo>().HasQueryFilter(x => !x.Deleted);
            #endregion
        }
    }
}
