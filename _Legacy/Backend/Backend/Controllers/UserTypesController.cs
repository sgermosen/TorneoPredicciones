namespace Backend.Controllers
{
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Net;
    using System.Web.Mvc;
    using Classes;
    using Models;
    using Domain;

    [Authorize(Roles = "Admin")]
    public class UserTypesController : Controller
    {
        private readonly DataContextLocal _db = new DataContextLocal();

        public async Task<ActionResult> Index()
        {
            return View(await _db.UserTypes.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userType = await _db.UserTypes.FindAsync(id);

            if (userType == null)
            {
                return HttpNotFound();
            }

            return View(userType);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserType userType)
        {
            if (ModelState.IsValid)
            {
                _db.UserTypes.Add(userType);
                var response = await DBHelper.SaveChanges(_db);
                if (response.IsSuccess)
                {
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, response.Message);
            }

            return View(userType);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userType = await _db.UserTypes.FindAsync(id);

            if (userType == null)
            {
                return HttpNotFound();
            }

            return View(userType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserType userType)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(userType).State = EntityState.Modified;
                var response = await DBHelper.SaveChanges(_db);
                if (response.IsSuccess)
                {
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, response.Message);
            }

            return View(userType);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userType = await _db.UserTypes.FindAsync(id);

            if (userType == null)
            {
                return HttpNotFound();
            }

            return View(userType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var userType = await _db.UserTypes.FindAsync(id);
            _db.UserTypes.Remove(userType);
            var response = await DBHelper.SaveChanges(_db);
            if (response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Message);
            }

            return RedirectToAction("Index");
        }
        //// GET: UserTypes
        //public async Task<ActionResult> Index()
        //{
        //    return View(await _db.UserTypes.ToListAsync());
        //}

        //// GET: UserTypes/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var userType = await _db.UserTypes.FindAsync(id);
        //    if (userType == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(userType);
        //}

        //// GET: UserTypes/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: UserTypes/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "UserTypeId,Name")] UserType userType)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.UserTypes.Add(userType);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(userType);
        //}

        //// GET: UserTypes/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var userType = await _db.UserTypes.FindAsync(id);
        //    if (userType == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(userType);
        //}

        //// POST: UserTypes/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "UserTypeId,Name")] UserType userType)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Entry(userType).State = EntityState.Modified;
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(userType);
        //}

        //// GET: UserTypes/Delete/5
        //public async Task<ActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var userType = await _db.UserTypes.FindAsync(id);
        //    if (userType == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(userType);
        //}

        //// POST: UserTypes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    var userType = await _db.UserTypes.FindAsync(id);
        //    if (userType != null) _db.UserTypes.Remove(userType);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
