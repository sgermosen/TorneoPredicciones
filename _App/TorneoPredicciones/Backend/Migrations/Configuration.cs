namespace Backend.Migrations
{
    using Domain;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<Models.DataContextLocal>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(Models.DataContextLocal context)
        {
            context.Status.AddOrUpdate(
               p => p.Name,
                new Status() { Name = "Pending" },
                new Status() { Name = "Started" },
                new Status() { Name = "Closed" },
                new Status() { Name = "Suspended" },
                new Status() { Name = "Posposed" }
           );
            context.UserTypes.AddOrUpdate(
               p => p.Name,
                new UserType() { Name = "Local" },
                new UserType() { Name = "Facebook" },
                new UserType() { Name = "Twitter" },
                new UserType() { Name = "Instagram" },
                new UserType() { Name = "Microsoft" },
                new UserType() { Name = "Google" }
           );
            context.Leagues.AddOrUpdate(
               p => p.Name,
                     new League() { Name = "Xamarin League", Logo = "~/Content/Logos/x.png" },
                     new League() { Name = "MLB", Logo = "~/Content/Logos/mlb.png" },
                     new League() { Name = "LIDOM", Logo = "~/Content/Logos/lidom.png" },
                     new League() { Name = "NBA", Logo = "~/Content/Logos/nba.png" },
                     new League() { Name = "LNB", Logo = "~/Content/Logos/lnb.png" },
                     new League() { Name = "FIFA", Logo = "~/Content/Logos/fifa.png" }
               );
            context.SaveChanges();

            context.Teams.AddOrUpdate(
            p => p.Name,
             new Team() { Name = "Team XamarinDO", LeagueId = 1, Initials = "XDO", Logo = "~/Content/Logos/xdo.png" },
             new Team() { Name = "Team XamarinRD", LeagueId = 1, Initials = "XRD", Logo = "~/Content/Logos/xrd.png" }
        );

            context.Tournaments.AddOrUpdate(
            p => p.Name,
             new Tournament() { Name = "Xamarin Tournament 2018", IsActive = true, Order = 1, Logo = "~/Content/Logos/x.png" }   );

            context.SaveChanges();

            context.TournamentGroups.AddOrUpdate(
          p => p.Name,
           new TournamentGroup() { Name = "First Round Group",  TournamentId=1 });
            context.SaveChanges();

        //    context.TournamentGroups.AddOrUpdate(
        //p => p.Name,
        // new TournamentGroup() { Name = "First Round Group", TournamentId = 1 });


        //    context.SaveChanges();
        }
    }
}
