namespace Backend.Controllers
{
    using System.Linq;
    using System.Web.Mvc;
    using Models;

    [Authorize(Roles = "Admin")]
    public class GenericController : Controller
    {
        private readonly DataContextLocal _db = new DataContextLocal();

        public JsonResult GetTeams(int leagueId)
        {
            _db.Configuration.ProxyCreationEnabled = false;
            var teams = _db.Teams.Where(m => m.LeagueId == leagueId);
            return Json(teams);
        }
        //public JsonResult GetCities(int departmentId)
        //{
        //    db.Configuration.ProxyCreationEnabled = false;
        //    var cities = db.Cities.Where(c => c.DepartmentId == departmentId);
        //    return Json(cities);
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