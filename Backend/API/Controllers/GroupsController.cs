namespace API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Models;
    using Domain;

    [Authorize]
    public class GroupsController : ApiController
    {
        private readonly DataContext _db = new DataContext();

        // GET: api/Groups
        public IQueryable<Group> GetGroups()
        {
            return _db.Groups;
        }

        // GET: api/Groups/5
        [ResponseType(typeof(Group))]
        public async Task<IHttpActionResult> GetGroup(int id)
        {
            try
            {
                var groups = await _db.Groups.ToListAsync();
                var list = (from @group in groups
                    let userGroup = @group.GroupUsers.FirstOrDefault(gu => gu.UserId == id)
                    where userGroup != null
                    select new GroupResponse
                    {
                        GroupId = @group.GroupId,
                        GroupUsers = ToGroupUserResponse(@group.GroupUsers),
                        Logo = @group.Logo,
                        Name = @group.Name,
                        TournamentId = @group.TournamentId,
                        Owner = @group.Owner,
                        OwnerId = @group.OwnerId,
                    }).ToList();
                return Ok(list);
            }
            catch (Exception e)
            {
                return Ok();
            }
            
        }

        private List<GroupUserResponse> ToGroupUserResponse(ICollection<GroupUser> groupUsers)
        {
            var list = new List<GroupUserResponse>();
            foreach (var groupUser in groupUsers.OrderByDescending(gu=>gu.Points))
            {
                list.Add(new GroupUserResponse
                {
                    GroupId = groupUser.GroupId,
                    GroupUserId = groupUser.GroupUserId,
                    IsAccepted = groupUser.IsAccepted,
                    IsBlocked = groupUser.IsBlocked,
                    Points = groupUser.Points,
                    User = ToUserResponse(groupUser.User),
                    UserId = groupUser.UserId,
                });
            }
            return list;
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

        // PUT: api/Groups/5
        //[ResponseType(typeof(void))]
        //public async Task<IHttpActionResult> PutGroup(int id, Group group)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != group.GroupId)
        //    {
        //        return BadRequest();
        //    }

        //    _db.Entry(group).State = EntityState.Modified;

        //    try
        //    {
        //        await _db.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!GroupExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        //[ResponseType(typeof(Group))]
        //public async Task<IHttpActionResult> PostGroup(GroupRequest request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (request.ImageArray != null && request.ImageArray.Length > 0)
        //    {
        //        var stream = new MemoryStream(request.ImageArray);
        //        var guid = Guid.NewGuid().ToString();
        //        var file = string.Format("{0}.jpg", guid);
        //        var folder = "~/Content/Logos";
        //        var fullPath = string.Format("{0}/{1}", folder, file);
        //        var response = FilesHelper.UploadPhoto(stream, folder, file);

        //        if (response)
        //        {
        //            request.Logo = fullPath;
        //        }
        //    }

        //    var group = ToGroup(request);
        //    _db.Groups.Add(group);
        //    await _db.SaveChangesAsync();
        //    // CreateUserAsp(request.Email, "User", request.Password);
        //    return CreatedAtRoute("DefaultApi", new { id = group.GroupId }, group);
        //   // return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        //}

        private Group ToGroup(GroupRequest request)
        {
            return new Group
            {
                Name = request.Name,
                Requirements = request.Requirements,
                OwnerId = request.OwnerId,
                TournamentId = request.TournamentId,
                Logo = request.Logo
            };
        }
       
        //[ResponseType(typeof(Group))]
        //public async Task<IHttpActionResult> PostGroup(Group group)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    _db.Groups.Add(group);
        //    await _db.SaveChangesAsync();

        //    return CreatedAtRoute("DefaultApi", new { id = group.GroupId }, group);
        //}

        // DELETE: api/Groups/5
        //[ResponseType(typeof(Group))]
        //public async Task<IHttpActionResult> DeleteGroup(int id)
        //{
        //    var group = await _db.Groups.FindAsync(id);
        //    if (group == null)
        //    {
        //        return NotFound();
        //    }

        //    _db.Groups.Remove(group);
        //    await _db.SaveChangesAsync();

        //    return Ok(group);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GroupExists(int id)
        {
            return _db.Groups.Count(e => e.GroupId == id) > 0;
        }
    }
}