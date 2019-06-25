namespace Backend.Controllers
{
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Net;
    using System.Web.Mvc;
    using Models;
    using Domain;
    using PsTools;

    [Authorize(Roles = "Admin")]
    public class LeaguesController : Controller
    {
        private readonly DataContextLocal _db = new DataContextLocal();


        #region TeamsController
        public async Task<ActionResult> DeleteTeam(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var team = await _db.Teams.FindAsync(id);

            if (team == null)
            {
                return HttpNotFound();
            }

            _db.Teams.Remove(team);
            await _db.SaveChangesAsync();
            return RedirectToAction(string.Format("Details/{0}", team.LeagueId));
        }

        public async Task<ActionResult> EditTeam(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var team = await _db.Teams.FindAsync(id);

            if (team == null)
            {
                return HttpNotFound();
            }

            var view = ToView(team);
            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditTeam(TeamView view)
        {
            if (ModelState.IsValid)
            {
                var pic = view.Logo;
                var folder = "~/Content/Teams";

                if (view.LogoTFile != null)
                {
                    pic = Files.UploadPhoto(view.LogoTFile, folder,"");
                    pic = string.Format("{0}/{1}", folder, pic);
                }

                var team = ToTeam(view);
                team.Logo = pic;
                _db.Entry(team).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("Details/{0}", view.LeagueId));
            }

            ViewBag.LeagueId = new SelectList(_db.Leagues, "LeagueId", "Name", view.LeagueId);
            return View(view);
        }

        public async Task<ActionResult> CreateTeam(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var league = await _db.Leagues.FindAsync(id);

            if (league == null)
            {
                return HttpNotFound();
            }

            var view = new TeamView { LeagueId = league.LeagueId, };
            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateTeam(TeamView view)
        {
            if (ModelState.IsValid)
            {
                var pic = string.Empty;
                var folder = "~/Content/Teams";

                if (view.LogoTFile != null)
                {
                    pic = Files.UploadPhoto(view.LogoTFile, folder,"");
                    pic = string.Format("{0}/{1}", folder, pic);
                }

                var team = ToTeam(view);
                team.Logo = pic;
                _db.Teams.Add(team);
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("Details/{0}", view.LeagueId));
            }

            return View(view);
        }

        private Team ToTeam(TeamView view)
        {
            return new Team
            {
                Initials = view.Initials,
                League = view.League,
                LeagueId = view.LeagueId,
                Logo = view.Logo,
                Name = view.Name,
                TeamId = view.TeamId,
            };
        }

        private TeamView ToView(Team team)
        {
            return new TeamView
            {
                Initials = team.Initials,
                League = team.League,
                LeagueId = team.LeagueId,
                Logo = team.Logo,
                Name = team.Name,
                TeamId = team.TeamId,
            };
        }

        //public async Task<ActionResult> DetailsTeam(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var team = await db.Teams.FindAsync(id);

        //    if (team == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(team);
        //}

        //public async Task<ActionResult> DetailsTeam(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var team = await db.Teams.FindAsync(id);

        //    if (team == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var view = ToViewTeam(team);
        //    return View(view);
        //}

        //public async Task<ActionResult> DetailsTeam(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var team = await db.Teams.FindAsync(id);

        //    if (team == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    var view = ToViewTeam(team);
        //    return View(view);
        //}

        //private Team ToTeam(TeamView view)
        //{
        //    return new Team
        //    {
        //        LeagueId = view.LeagueId,
        //        Logoxx = view.Logoxx,
        //        Name = view.Name,
        //        Initials = view.Initials,
        //        League = view.League,
        //        TeamId = view.TeamId
        //    };
        //}
        //private TeamView ToViewTeam(Team team)
        //{
        //    return new TeamView
        //    {
        //        LeagueId = team.LeagueId,
        //        Logoxx = team.Logoxx,
        //        Name = team.Name,
        //        Initials = team.Initials,
        //        League = team.League,
        //        TeamId = team.TeamId
        //    };
        //}
        //public async Task<ActionResult> CreateTeam(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var league = await _db.Leagues.FindAsync(id);

        //    if (league == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var view = new TeamView { LeagueId = league.LeagueId, };
        //    return View(view);
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> CreateTeam(TeamView view)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var table = _db.Teams
        //            .Where(u => u.Name.ToLower() == view.Name.ToLower() && u.LeagueId == view.LeagueId)
        //            .FirstOrDefaultAsync();

        //        if (table.Result != null)
        //        {
        //            ModelState.AddModelError(string.Empty,
        //                "Este nombre ya esta en uso en esta liga, escoja uno diferente");

        //        }
        //        else
        //        {
        //            var table2 = _db.Teams
        //                .Where(u => u.Initials.ToLower() == view.Initials.ToLower() && u.LeagueId == view.LeagueId)
        //                .FirstOrDefaultAsync();

        //            if (table2.Result != null)
        //            {
        //                ModelState.AddModelError(string.Empty,
        //                    "Estas iniciales ya estan en uso en esta liga, escoja uno diferente");
        //            }
        //            else
        //            {

        //                var pic = string.Empty;
        //                var folder = "~/Content/Logos";

        //                if (view.LogoFile != null)
        //                {
        //                    pic = FilesHelper.UploadPhoto(view.LogoFile, folder,"");
        //                    pic = string.Format("{0}/{1}", folder, pic);
        //                }

        //                var team = ToTeam(view);
        //                team.Logoxx = pic;

        //                _db.Teams.Add(team);
        //                await _db.SaveChangesAsync();

        //                return RedirectToAction(string.Format("Details/{0}", team.LeagueId));
        //            }
        //        }
        //    }

        //    return View(view);
        //}

        //public async Task<ActionResult> EditTeam(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var team = await _db.Teams.FindAsync(id);

        //    if (team == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var view = ToViewTeam(team);
        //    return View(view);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditTeam(TeamView view)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var table = _db.Teams
        //            .Where(u => u.Name.ToLower() == view.Name.ToLower() && u.LeagueId == view.LeagueId && u.TeamId != view.TeamId)
        //            .FirstOrDefaultAsync();

        //        if (table.Result != null)
        //        {
        //            ModelState.AddModelError(string.Empty,
        //                "Este nombre ya esta en uso en esta liga, escoja uno diferente");
        //        }
        //        else
        //        {
        //            var table2 = _db.Teams
        //                .Where(u => u.Initials.ToLower() == view.Initials.ToLower() && u.LeagueId == view.LeagueId && u.TeamId != view.TeamId)
        //                .FirstOrDefaultAsync();

        //            if (table2.Result != null)
        //            {
        //                ModelState.AddModelError(string.Empty,
        //                    "Estas iniciales ya estan en uso en esta liga, escoja uno diferente");
        //            }
        //            else
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    var pic = view.Logoxx;
        //                    var folder = "~/Content/Logos";

        //                    if (view.LogoFile != null)
        //                    {
        //                        pic = FilesHelper.UploadPhoto(view.LogoFile, folder,"");
        //                        pic = string.Format("{0}/{1}", folder, pic);
        //                    }

        //                    var team = ToTeam(view);
        //                    team.Logoxx = pic;

        //                    _db.Entry(team).State = EntityState.Modified;
        //                    await _db.SaveChangesAsync();
        //                    return RedirectToAction(string.Format("Details/{0}", team.LeagueId));
        //                }
        //            }
        //        }
        //    }

        //    return View(view);
        //}

        //public async Task<ActionResult> DeleteTeam(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var team = await _db.Teams.FindAsync(id);

        //    if (team == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    _db.Teams.Remove(team);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(string.Format("Details/{0}", team.LeagueId));
        //}


        #endregion

        #region LeagueController


        public async Task<ActionResult> Index()
        {
            return View(await _db.Leagues.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var league = await _db.Leagues.FindAsync(id);

            if (league == null)
            {
                return HttpNotFound();
            }
            return View(league);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(LeagueView view)
        {
            if (ModelState.IsValid)
            {
                var pic = string.Empty;
                var folder = "~/Content/Leagues";

                if (view.LogoLFile != null)
                {
                    pic = Files.UploadPhoto(view.LogoLFile, folder,"");
                    pic = string.Format("{0}/{1}", folder, pic);
                }

                var league = ToLeague(view);
                league.Logo = pic;
                _db.Leagues.Add(league);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(view);
        }

        private League ToLeague(LeagueView view)
        {
            return new League
            {
                LeagueId = view.LeagueId,
                Logo = view.Logo,
                Name = view.Name,
                Teams = view.Teams,
            };
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var league = await _db.Leagues.FindAsync(id);

            if (league == null)
            {
                return HttpNotFound();
            }

            var view = ToView(league);
            return View(view);
        }

        private LeagueView ToView(League league)
        {
            return new LeagueView
            {
                LeagueId = league.LeagueId,
                Logo = league.Logo,
                Name = league.Name,
                Teams = league.Teams,
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(LeagueView view)
        {
            if (ModelState.IsValid)
            {
                var pic = view.Logo;
                var folder = "~/Content/Leagues";

                if (view.LogoLFile != null)
                {
                    pic = Files.UploadPhoto(view.LogoLFile, folder,"");
                    pic = string.Format("{0}/{1}", folder, pic);
                }

                var league = ToLeague(view);
                league.Logo = pic;
                _db.Entry(league).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(view);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var league = await _db.Leagues.FindAsync(id);

            if (league == null)
            {
                return HttpNotFound();
            }
            return View(league);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var league = await _db.Leagues.FindAsync(id);
            if (league != null) _db.Leagues.Remove(league);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
