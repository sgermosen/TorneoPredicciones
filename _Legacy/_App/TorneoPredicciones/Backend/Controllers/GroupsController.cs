namespace Backend.Controllers
{
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Net;
    using System.Web.Mvc;
    using Models;
    using Domain;
    using Helpers;

    [Authorize(Roles = "Admin")]
    public class GroupsController : Controller
    {
        private DataContextLocal db = new DataContextLocal();

        // GET: Groups
        public async Task<ActionResult> Index()
        {
            var groups = db.Groups.Include(g => g.Owner);
            return View(await groups.ToListAsync());
        }
        // GET: Groups/Create
        public ActionResult Create()
        {
            ViewBag.OwnerId = new SelectList(db.Users, "UserId", "FirstName");
            ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "Name");

            return View();
        }
      //   private readonly ApiService _apiService;
        // POST: Groups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create( GroupView view)
        {
            if (ModelState.IsValid)
            {
                ////var pic = string.Empty;
                ////var folder = "~/Content/Users";

                ////if (view.PictureFile != null)
                ////{
                ////    pic = Files.UploadPhoto(view.PictureFile, folder, "");
                ////    pic = string.Format("{0}/{1}", folder, pic);
                ////}

                ////var user = ToUser(view);
                ////user.Picture = pic;

                var imageArray = FilesHelper.ReadFully(view.LogoGFile.InputStream);// _file.GetStream());
                                                                                   // _file.Dispose();
                ApiService _apiService = new ApiService();

                var group = new Group
                {
                    Logo = view.Logo,
                    Name = view.Name,
                    OwnerId = view.OwnerId,
                    Requirements = view.Requirements,
                    TournamentId = view.TournamentId,
                    ImageArray = imageArray,                    
                };

                var response = await _apiService.Post("https://torneoprediccionesapi.azurewebsites.net", "/api", "/Groups", group);
              
                if (!response.IsSuccess)
                {
                    return RedirectToAction("Index");
                }
               // db.Groups.Add(group);
               // await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.OwnerId = new SelectList(db.Users, "UserId", "FirstName", view.OwnerId);
            ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "Name", view.TournamentId);

            return View(view);
        }

        // GET: Groups/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            ViewBag.OwnerId = new SelectList(db.Users, "UserId", "FirstName", group.OwnerId);
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "GroupId,Name,OwnerId,Logo")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Entry(group).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.OwnerId = new SelectList(db.Users, "UserId", "FirstName", group.OwnerId);
            return View(group);
        }

        // GET: Groups/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Group group = await db.Groups.FindAsync(id);
            db.Groups.Remove(group);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
