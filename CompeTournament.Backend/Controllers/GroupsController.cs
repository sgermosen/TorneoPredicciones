using CompeTournament.Backend.Data;
using CompeTournament.Backend.Data.Entities;
using CompeTournament.Backend.Persistence.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Azure.NotificationHubs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Controllers
{
    public class GroupsController : Controller
    {
        private readonly NotificationHubClient _hub;

        private readonly IGroupRepository _groupRepo;
        private readonly ITournamentTypeRepository _typeRepository;
        private readonly ILeagueRepository _leagueRepo;
        private readonly ITeamRepository _teamRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly ApplicationDbContext _context;

        public GroupsController(IGroupRepository groupRepo, ITournamentTypeRepository typeRepository
            , ILeagueRepository leagueRepo, ITeamRepository teamRepository, IMatchRepository matchRepository,
            ApplicationDbContext context)
        {
            _groupRepo = groupRepo;
            _typeRepository = typeRepository;
            _leagueRepo = leagueRepo;
            _teamRepository = teamRepository;
            _matchRepository = matchRepository;
            _context = context;

            _hub = NotificationHubClient
                .CreateClientFromConnectionString("Endpoint=sb://psmhub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=VRZxkd15WUJsWXWmkt0UySRsCg6K/9ZSytBIdsV7Grs=", "MainNotification");

        }

        #region Prediction
        public async Task<IActionResult> MakePrediction(int id)
        {
            var prediction = new Prediction
            {
                MatchId = id
            };

            return View(prediction);
        }

        [HttpPost]
        public async Task<IActionResult> MakePrediction(Prediction model)
        {

            if (this.ModelState.IsValid)
            {
                model.AdquiredPoints = 0;
                model.Id = 0;
                await _matchRepository.AddPrediction(model);
                return this.RedirectToAction(nameof(Index)); //new { id = gMember.GroupId }
            }

            return this.View(model);
        }
        #endregion

        #region Group
        public async Task<IActionResult> Index()
        {
            var groups = _groupRepo.GetWithType();
            return View(groups);
        }

        public async Task<IActionResult> JoinRequest(int id) //groupId
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uGroup = new GroupUser
            {
                GroupId = id,
                ApplicationUserId = userId,
                IsAccepted = false,
                IsBlocked = false,
                Points = 0
            };
            try
            {
                await _groupRepo.JoinRequest(uGroup);
            }
            catch (System.Exception)
            {
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AcceptRequestJoin(int id) //groupuserId
        {

            var gMember = await _groupRepo.GroupMember(id);
            gMember.IsAccepted = true;
            try
            {
                await _groupRepo.AcceptRequestJoin(gMember);
            }
            catch (System.Exception)
            {
                throw;
            }

            return RedirectToAction(nameof(Details), new { id = gMember.GroupId });
        }

        public async Task<IActionResult> FireFromGroup(int id) //groupuserId
        {

            var gMember = await _groupRepo.GroupMember(id);
            gMember.IsBlocked = true;
            try
            {
                await _groupRepo.AcceptRequestJoin(gMember);
            }
            catch (System.Exception)
            {
                throw;
            }

            return RedirectToAction(nameof(Details), new { id = gMember.GroupId });
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _groupRepo.GetByIdWithChildrens(id);

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var tt = _typeRepository.GetAll().ToList();
            // ViewBag.TournamentTypes = tt.ToList();
            ViewData["TournamentTypeId"] = new SelectList(tt, "Id", "Name");
            //var model = new GroupView
            //{
            //    TournamentTypes = tt.ToList()
            //};
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Group model)
        {

            if (this.ModelState.IsValid)
            {
                await _groupRepo.CreateAsync(model);
                return this.RedirectToAction(nameof(Index));
            }

            return this.View(model);
        }
        #endregion

        #region League
        public async Task<IActionResult> DetailsLeague(int id)
        {
            var model = await _leagueRepo.GetByIdWithChildrens(id);

            return View(model);
        }

        public async Task<IActionResult> CreateLeague(int id)//this id is from the group
        {
            var model = new League
            {
                GroupId = id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLeague(League model)
        {//Todo: you need to revalidate than the user than is creating be owner

            if (this.ModelState.IsValid)
            {
                model.Id = 0;
                await _leagueRepo.CreateAsync(model);
                return this.RedirectToAction(nameof(Details), new { id = model.GroupId });
            }

            return this.View(model);
        }
        #endregion

        #region Team
        public async Task<IActionResult> CreateTeam(int id)//this id is from the group
        {
            var model = new Team
            {
                LeagueId = id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam(Team model)
        {//Todo: you need to revalidate than the user than is creating be owner

            if (this.ModelState.IsValid)
            {
                model.Id = 0;
                await _teamRepository.CreateAsync(model);
                return this.RedirectToAction(nameof(DetailsLeague), new { id = model.LeagueId });
            }

            return this.View(model);
        }
        #endregion

        #region Match

        private int GetStatus(int LocalPoints, int VisitorPoints)
        {
            if (LocalPoints > VisitorPoints)
            {
                return 1; // Local win
            }

            if (VisitorPoints > LocalPoints)
            {
                return 2; // Visitor win
            }

            return 3; // Draw
        }

        private async Task SendNotificationThreePoints(List<string> tags, Match match)
        {
            var message = string.Format("{0} {1} Vs. {2} {3}, Has finished and you have gotten 3 points, congratulations!.",
                match.Local.Initials, match.LocalPoints, match.VisitorPoints, match.Visitor.Initials);
            await SendNotification(tags, message);
        }

        private async Task SendNotificationOnePoint(List<string> tags, Match match)
        {
            var message = string.Format("{0} {1} Vs. {2} {3}, Has finished and you have gotten 1 point, congratulations!.",
                match.Local.Initials, match.LocalPoints, match.VisitorPoints, match.Visitor.Initials);
            await SendNotification(tags, message);
        }

        private async Task SendNotificationNoPoints(List<string> tags, Match match)
        {
            var message = string.Format("{0} {1} Vs. {2} {3}, Has finished... sorry you don't gain any point.",
                match.Local.Initials, match.LocalPoints, match.VisitorPoints, match.Visitor.Initials);
            await SendNotification(tags, message);
        }

        private async Task SendNotification(List<string> tags, string message)
        {
            try
            {
                do
                {
                    if (tags.Count <= 20)
                    {
                        //Todo: search the equivalent of gcm, mean while comment it
                      //  await _hub.SendGcmNativeNotificationAsync("{ \"data\" : {\"Message\":\"" + message + "\"}}", tags);
                        tags.Clear();
                    }
                    else
                    {
                        var tags20 = new List<string>();
                        for (int i = 0; i < 20; i++)
                        {
                            tags20.Add(tags[i]);
                        }

                        tags.RemoveRange(0, 20);
                        //Todo: search the equivalent of gcm, mean while comment it
                       // await _hub.SendGcmNativeNotificationAsync("{ \"data\" : {\"Message\":\"" + message + "\"}}", tags20);
                    }
                } while (tags.Count > 0);

            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        public async Task<IActionResult> CloseMatch(int id)
        {
            var match = _matchRepository.GetByIdWithChildrens(id);
            return View(match);
        }

        [HttpPost]
        public async Task<IActionResult> CloseMatch(Match match)
        {
            using (var transacction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Update match
                    var oldMatch = await _context.Matches.FindAsync(match.Id);
                    oldMatch.LocalPoints = match.LocalPoints;
                    oldMatch.VisitorPoints = match.VisitorPoints;
                    oldMatch.StatusId = 3; // Closed
                    _context.Entry(oldMatch).State = EntityState.Modified;

                    var statusMatch = GetStatus(match.LocalPoints.Value, match.VisitorPoints.Value);

                    // Update tournaments statistics
                    var local = await _context.Teams
                        .Where(tt => tt.League.GroupId == oldMatch.GroupId &&
                                     tt.Id == oldMatch.LocalId)
                        .FirstOrDefaultAsync();

                    var visitor = await _context.Teams
                        .Where(tt => tt.League.GroupId == oldMatch.GroupId &&
                                     tt.Id == oldMatch.VisitorId)
                        .FirstOrDefaultAsync();

                    local.MatchesPlayed++;
                    local.FavorPoints += oldMatch.LocalPoints.Value;
                    local.AgainstPoints += oldMatch.VisitorPoints.Value;

                    visitor.MatchesPlayed++;
                    visitor.FavorPoints += oldMatch.VisitorPoints.Value;
                    visitor.AgainstPoints += oldMatch.LocalPoints.Value;

                    if (statusMatch == 1) // Local won
                    {
                        local.MatchesWon++;
                        local.CumulativePoints += 3;
                        visitor.MatchesLost++;
                    }
                    else if (statusMatch == 2) // Visitor won
                    {
                        visitor.MatchesWon++;
                        visitor.CumulativePoints += 3;
                        local.MatchesLost++;
                    }
                    else // Draw
                    {
                        local.MatchesTied++;
                        visitor.MatchesTied++;
                        local.CumulativePoints++;
                        visitor.CumulativePoints++;
                    }

                    _context.Entry(local).State = EntityState.Modified;
                    _context.Entry(visitor).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    // Update positions
                    var teams = await _context.Teams
                        .Where(tt => tt.League.GroupId == oldMatch.GroupId)
                        .ToListAsync();
                    var i = 1;
                    foreach (var team in teams.OrderByDescending(t => t.CumulativePoints)
                        .ThenByDescending(t => t.FavorPoints - t.AgainstPoints)
                        .ThenByDescending(t => t.FavorPoints))
                    {
                        team.Position = i;
                        _context.Entry(team).State = EntityState.Modified;
                        i++;
                    }

                    var noPoints = new List<string>();
                    var onePoint = new List<string>();
                    var threePoints = new List<string>();

                    // Update predictions
                    var predictions = await _context.Predictions
                        .Where(p => p.MatchId == oldMatch.Id)
                        .ToListAsync();
                    foreach (var prediction in predictions)
                    {
                        var points = 0;
                        if (prediction.LocalPoints == oldMatch.LocalPoints &&
                            prediction.VisitorPoints == oldMatch.VisitorPoints)
                        {
                            points = 3;
                            threePoints.Add(string.Format("userId:{0}", prediction.CreatedBy));
                        }
                        else
                        {
                            var statusPrediction = GetStatus(prediction.LocalPoints.Value, prediction.VisitorPoints.Value);
                            if (statusMatch == statusPrediction)
                            {
                                points = 1;
                                onePoint.Add(string.Format("userId:{0}", prediction.CreatedBy));
                            }
                            else
                            {
                                noPoints.Add(string.Format("userId:{0}", prediction.CreatedBy));
                            }
                        }

                        if (points != 0)
                        {
                            prediction.AdquiredPoints = points;
                            _context.Entry(prediction).State = EntityState.Modified;

                            //added because Need to update the user only on the group

                            var userGroup = await _context.GroupUsers
                                .Where(p => p.CreatedBy == prediction.CreatedBy && p.IsAccepted && !p.IsBlocked)
                                .FirstOrDefaultAsync();
                            userGroup.Points += points;
                            _context.Entry(userGroup).State = EntityState.Modified;
                            await _context.SaveChangesAsync();

                            //commented because points are individuals for each group due logical changes of perspective
                            // Update user
                            // var user = await _context.Users.FindAsync(prediction.CreatedBy);
                            //user.Points += points;
                            //_context.Entry(user).State = EntityState.Modified;
                            //await _context.SaveChangesAsync();

                            // Update points in groups
                            //commented because due this new perspective each prediction is exclusive of one group
                            //try
                            //{
                            //    //var groupUsers = await _context.GroupUsers.Where(gu => gu.UserId == user.UserId &&
                            //    //                                                  gu.IsAccepted && gu.Group.TournamentId == oldMatch.TournamentGroup.TournamentId &&
                            //    //                                                  !gu.IsBlocked)
                            //    //    .ToListAsync();
                            //    var groupUsers = await _context.GroupUsers.Where(gu => gu.CreatedBy == user.Id &&
                            //                                                                            gu.IsAccepted &&
                            //                                                                            !gu.IsBlocked)
                            //                               .ToListAsync();
                            //    foreach (var groupUser in groupUsers)
                            //    {
                            //        groupUser.Points += points;
                            //        _context.Entry(groupUser).State = EntityState.Modified;
                            //    }
                            //}
                            //catch (Exception e)
                            //{
                            //    Console.WriteLine(e);
                            //    throw;
                            //}

                        }
                    }

                    await _context.SaveChangesAsync();
                    transacction.Commit();
                    try
                    {
                        if (noPoints.Count > 0)
                        {
                            await SendNotificationNoPoints(noPoints, oldMatch);
                        }

                        if (onePoint.Count > 0)
                        {
                            await SendNotificationOnePoint(onePoint, oldMatch);
                        }

                        if (threePoints.Count > 0)
                        {
                            await SendNotificationThreePoints(threePoints, oldMatch);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    return RedirectToAction(string.Format("Details/{0}", oldMatch.GroupId));
                }
                catch (Exception ex)
                {
                    transacction.Rollback();
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(match);
                }
            }

            //if (this.ModelState.IsValid)
            //{
            //   // model.AdquiredPoints = 0;
            //   // model.Id = 0;
            //    await _matchRepository.UpdateAsync(model);
            //    return this.RedirectToAction(nameof(Index)); //new { id = gMember.GroupId }
            //}

           // return this.View(match);
        }

        public async Task<IActionResult> DetailsMatch(int id)
        {
            var model = await _matchRepository.GetByIdWithChildrens(id);

            return View(model);
        }

        public async Task<IActionResult> CreateMatch(int id)//this id is from the group
        {
            var tt = _teamRepository.GetAll().ToList();//TODO: need to be filtered
            // ViewBag.TournamentTypes = tt.ToList();
            ViewData["LocalId"] = new SelectList(tt, "Id", "Name");
            ViewData["VisitorId"] = new SelectList(tt, "Id", "Name");

            var model = new Match
            {
                GroupId = id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMatch(Match model)
        {//Todo: you need to revalidate than the user than is creating be owner

            if (this.ModelState.IsValid)
            {
                model.Id = 0;
                model.StatusId = 1;
                await _matchRepository.CreateAsync(model);
                return this.RedirectToAction(nameof(Details), new { id = model.GroupId });
            }

            return this.View(model);
        }

        #endregion

    }
}