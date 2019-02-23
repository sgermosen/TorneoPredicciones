namespace API.Controllers
{
    using System;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Models;
    using Domain;
    using Newtonsoft.Json.Linq;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using PsTools;
    using FilesHelper = API.Classes.FilesHelper;

    [RoutePrefix("api/Users")]
   // [Authorize(Roles = "User")]

    public class UsersController : ApiController
    {
        private readonly DataContext _db = new DataContext();

        [HttpPost]
        [Route("PasswordRecovery")]
        public async Task<IHttpActionResult> PasswordRecovery(JObject form)
        {
            try
            {
                string email;
                dynamic jsonObject = form;

                try
                {
                    email = jsonObject.Email.Value;
                }
                catch
                {
                    return BadRequest("Incorrect call");
                }

                var user = await _db.Users
                    .Where(u => u.Email.ToLower() == email.ToLower())
                    .FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound();
                }

                var userContext = new ApplicationDbContext();
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
                var userAsp = userManager.FindByEmail(email);
                if (userAsp == null)
                {
                    return NotFound();
                }

                var random = new Random();
                var newPassword = string.Format("{0}", random.Next(100000, 999999));
                var response1 = userManager.RemovePassword(userAsp.Id);
                var response2 = await userManager.AddPasswordAsync(userAsp.Id, newPassword);
                if (response2.Succeeded)
                {
                    var subject = "Torneo Predicciones - Password Recovery";
                    var body = string.Format(@"
            <h1>Torneo Predicciones - Password Recovery</h1>
            <p>Your new password is: <strong>{0}</strong></p>
            <p>Please, don't forget change it for one easy remember for you.",
                        newPassword);

                    await Emails.SendMail(email, subject, body);
                    return Ok(true);
                }

                return BadRequest("The password can't be changed.");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("LoginFacebook")]
        public async Task<IHttpActionResult> LoginFacebook(FacebookResponse profile)
        {
            try
            {
                var user = await _db.Users.Where(u => u.Email == profile.Id).FirstOrDefaultAsync();
                if (user == null)
                {
                    user = new User
                    {
                        Email = profile.Id,
                        FirstName = profile.FirstName,
                        LastName = profile.LastName,
                        FavoriteTeamId=1,
                        NickName = profile.Name.Length >20 ? profile.Name.Substring(0,20).Trim(): profile.Name,
                        Picture = profile.Picture.Data.Url,
                        Points = 0,
                        UserTypeId = 2 //facebook
                    };

                    _db.Users.Add(user);
                    CreateUserAsp(profile.Id, "User", profile.Id);
                }
                else
                {
                    user.FirstName = profile.FirstName;
                    user.LastName = profile.LastName;
                    user.Picture = profile.Picture.Data.Url;

                    _db.Entry(user).State = EntityState.Modified;

                }
                await _db.SaveChangesAsync();
               //

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
               
            }

        }

        [HttpGet]
        [Route("GetPoints/{id}")]
        public async Task<IHttpActionResult> GetPoints(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            return Ok(new PointsResponse { Points = user.Points });
        }

        [HttpGet]
        [Route("GetGroupPoints/{id},{gId}")]
        public async Task<IHttpActionResult> GetGroupPoints(int id,int gId)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var group = await _db.Groups.FindAsync(gId);
            if (group == null)
            {
                return BadRequest("Group not found");
            }

            var groupUser = await _db.GroupUsers.Where(p => p.GroupId == gId && p.UserId == id).FirstOrDefaultAsync();

            return Ok(new PointsResponse { Points = groupUser.Points });
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(JObject form)
        {
            string email;
            string currentPassword;
            string newPassword;

            dynamic jsonObject = form;

            try
            {
                email = jsonObject.Email.Value;
                currentPassword = jsonObject.CurrentPassword.Value;
                newPassword = jsonObject.NewPassword.Value;
            }
            catch
            {
                return BadRequest("Incorrect call");
            }
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));

            var userAsp = userManager.FindByEmail(email);
            if (userAsp == null)
            {
                return BadRequest("Incorrect call");
            }
            var response = await userManager.ChangePasswordAsync(userAsp.Id, currentPassword, newPassword);

            if (response.Succeeded)
            {
                return BadRequest(response.Errors.FirstOrDefault());
            }
            return Ok("ok");

        }

        [HttpPost]
        [Route("GetUserByEmail")]
        public async Task<IHttpActionResult> GetUserByEmail(JObject form)
        {
            //string email;
            //dynamic jsonObject = form;

            //try
            //{
            //    email = jsonObject.Email.Value;
            //}
            //catch
            //{
            //    return BadRequest("Incorrect call");
            //}

            //var user = await _db.Users.Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            //if (user == null)
            //{
            //    return NotFound();
            //}

            //var userResponse = ToUserResponse(user);
            //return Ok(userResponse);
            string email;
            dynamic jsonObject = form;

            try
            {
                email = jsonObject.Email.Value;
            }
            catch
            {
                return BadRequest("Incorrect call");
            }

            var user = await _db.Users
                .Where(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }

            if (user.GroupUsers.Count == 0)
            {
                await AddUserToGroup(user.UserId);
            }

            var userResponse = ToUserResponse(user);
            return Ok(userResponse);
        }

        private async Task AddUserToGroup(int userId)
        {
            var groups = await _db.Groups.ToListAsync();
            var radom = new Random();
            //var groupUser = new GroupUser
            //{
            //    GroupId = groups[radom.Next(groups.Count)].GroupId,
            //    IsAccepted = true,
            //    IsBlocked = false,
            //    Points = 0,
            //    UserId = userId,
            //};
            var groupUser = new GroupUser
            {
                GroupId = 1,
                IsAccepted = true,
                IsBlocked = false,
                Points = 0,
                UserId = userId,
            };

            _db.GroupUsers.Add(groupUser);
            await _db.SaveChangesAsync();
        }


        //[HttpPost]
        //[Route("GetUserByEmail")]
        //public async Task<IHttpActionResult> GetUserByEmail(JObject form)
        //{
        //    string email;
        //    dynamic jsonObject = form;

        //    try
        //    {
        //        email = jsonObject.Email.Value;
        //    }
        //    catch
        //    {
        //        return BadRequest("Incorrect call");
        //    }

        //    // var user = await db.Users.Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
        //    var user = await db.Users
        //        .Where(u => u.Email.ToLower() == email.ToLower())
        //        .FirstOrDefaultAsync();

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    var userResponse = ToUserResponse(user);
        //    return Ok(userResponse);
        //}

        public IQueryable<User> GetUsers()
        {
            //si estoy trabajando con una sola tabla donde esta todo, puedo solucionarlo asi
            //sin embargo no la necesito si pongo el jsonignore en cada virtual
            _db.Configuration.ProxyCreationEnabled = false;
            return _db.Users;
        }


        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> GetUser(int id)
        {
            //si estoy trabajando con una sola tabla donde esta todo, puedo solucionarlo asi
            // db.Configuration.ProxyCreationEnabled = false;
            User user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var userResponse = ToUserResponse(user);
            return Ok(userResponse);
        }

        private UserResponse ToUserResponse(User user)
        {
            return new UserResponse
            {
                Email = user.Email,
                FavoriteTeam = user.FavoriteTeam,
                FavoriteTeamId = user.FavoriteTeamId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                NickName = user.NickName,
                Picture = user.Picture,
                Points = user.Points,
                UserId = user.UserId,
                UserType = user.UserType,
                UserTypeId = user.UserTypeId,
            };
        }
        //private UserResponse ToUserResponse(User user)
        //{
        //    return new UserResponse
        //    {
        //        Email = user.Email,
        //        FavoriteTeam = user.FavoriteTeam,
        //        FavoriteTeamId = user.FavoriteTeamId,
        //        FirstName = user.FirstName,
        //        // GroupUsers = user.GroupUsers.ToList(),
        //        LastName = user.LastName,
        //        NickName = user.NickName,
        //        Picture = user.Picture,
        //        Points = user.Points,
        //        // Predictions = user.Predictions.ToList(),
        //        // UserGroups = user.UserGroups.ToList(),
        //        UserId = user.UserId,
        //        //   UserType = user.UserType,
        //        UserTypeId = user.UserTypeId
        //    };
        //}

        // PUT: api/Users/5
        // PUT: api/Users/5

        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutUser(int id, UserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != request.UserId)
            {
                return BadRequest();
            }

            var oldUser = await _db.Users.FindAsync(id);
            if (oldUser == null)
            {
                return NotFound();
            }

            var oldEmail = oldUser.Email;
            var isEmailChanged = oldUser.Email.ToLower() != request.Email.ToLower();

            if (request.ImageArray != null && request.ImageArray.Length > 0)
            {
                var stream = new MemoryStream(request.ImageArray);
                var guid = Guid.NewGuid().ToString();
                var file = string.Format("{0}.jpg", guid);
                var folder = "~/Content/Users";
                var fullPath = string.Format("{0}/{1}", folder, file);
                var response = FilesHelper.UploadPhoto(stream, folder, file);

                if (response)
                {
                    request.Picture = fullPath;
                }
            }
            else
            {
                request.Picture = oldUser.Picture;
            }

            oldUser.Email = request.Email;
            oldUser.FavoriteTeamId = request.FavoriteTeamId;
            oldUser.FirstName = request.FirstName;
            oldUser.LastName = request.LastName;
            oldUser.NickName = request.NickName;
            oldUser.Picture = request.Picture;
            _db.Entry(oldUser).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
                if (isEmailChanged)
                {
                    UpdateUserName(oldEmail, request.Email);
                }

                return Ok(oldUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
 
        private bool UpdateUserName(string currentUserName, string newUserName)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userAsp = userManager.FindByEmail(currentUserName);
            if (userAsp == null)
            {
                return false;
            }

            userAsp.UserName = newUserName;
            userAsp.Email = newUserName;
            var response = userManager.Update(userAsp);
            return response.Succeeded;
        }

        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> PostUser(UserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.ImageArray != null && request.ImageArray.Length > 0)
            {
                var stream = new MemoryStream(request.ImageArray);
                var guid = Guid.NewGuid().ToString();
                var file = string.Format("{0}.jpg", guid);
                var folder = "~/Content/Users";
                var fullPath = string.Format("{0}/{1}", folder, file);
                var response = FilesHelper.UploadPhoto(stream, folder, file);

                if (response)
                {
                    request.Picture = fullPath;
                }
            }

            var user = ToUser(request);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            CreateUserAsp(request.Email, "User", request.Password);

            return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        }

        private void CreateUserAsp(string email, string roleName, string password)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));

            var userAsp = new ApplicationUser
            {
                Email = email,
                UserName = email,
            };

            var result = userManager.Create(userAsp, password);
            if (result.Succeeded)
            {
                userManager.AddToRole(userAsp.Id, roleName);
            }
        }

        private User ToUser(UserRequest request)
        {
            return new User
            {
                Email = request.Email,
                FavoriteTeamId = request.FavoriteTeamId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                NickName = request.NickName,
                Picture = request.Picture,
                Points = 0,
                UserTypeId = request.UserTypeId,
            };
        }

        // POST: api/Users
        //[ResponseType(typeof(User))]
        //public async Task<IHttpActionResult> PostUser(User user)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Users.Add(user);
        //    await db.SaveChangesAsync();

        //    return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        //}

        // DELETE: api/Users/5

        //[ResponseType(typeof(User))]
        //public async Task<IHttpActionResult> DeleteUser(int id)
        //{
        //    User user = await db.Users.FindAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Users.Remove(user);
        //    await db.SaveChangesAsync();

        //    return Ok(user);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return _db.Users.Count(e => e.UserId == id) > 0;
        }

    }
}