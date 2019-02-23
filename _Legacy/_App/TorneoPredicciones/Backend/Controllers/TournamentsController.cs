namespace Backend.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Web.Mvc;
    using Models;
    using Domain;
    using Microsoft.Azure.NotificationHubs;
    using PsTools;

    [Authorize(Roles = "Admin")]
    public class TournamentsController : Controller
    {
        private readonly DataContextLocal _db = new DataContextLocal();
        private readonly NotificationHubClient _hub;

        public TournamentsController()
        {
            _hub = NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb://psmhub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=VRZxkd15WUJsWXWmkt0UySRsCg6K/9ZSytBIdsV7Grs=", "MainNotification");
                       }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CloseMatch(Match match)
        {
            using (var transacction = _db.Database.BeginTransaction())
            {
                try
                {
                    // Update match
                    var oldMatch = await _db.Matches.FindAsync(match.MatchId);
                    oldMatch.LocalGoals = match.LocalGoals;
                    oldMatch.VisitorGoals = match.VisitorGoals;
                    oldMatch.StatusId = 3; // Closed
                    _db.Entry(oldMatch).State = EntityState.Modified;

                    var statusMatch = GetStatus(match.LocalGoals.Value, match.VisitorGoals.Value);

                    // Update tournaments statistics
                    var local = await _db.TournamentTeams
                        .Where(tt => tt.TournamentGroupId == oldMatch.TournamentGroupId &&
                                     tt.TeamId == oldMatch.LocalId)
                        .FirstOrDefaultAsync();

                    var visitor = await _db.TournamentTeams
                        .Where(tt => tt.TournamentGroupId == oldMatch.TournamentGroupId &&
                                     tt.TeamId == oldMatch.VisitorId)
                        .FirstOrDefaultAsync();

                    local.MatchesPlayed++;
                    local.FavorGoals += oldMatch.LocalGoals.Value;
                    local.AgainstGoals += oldMatch.VisitorGoals.Value;

                    visitor.MatchesPlayed++;
                    visitor.FavorGoals += oldMatch.VisitorGoals.Value;
                    visitor.AgainstGoals += oldMatch.LocalGoals.Value;

                    if (statusMatch == 1) // Local won
                    {
                        local.MatchesWon++;
                        local.Points += 3;
                        visitor.MatchesLost++;
                    }
                    else if (statusMatch == 2) // Visitor won
                    {
                        visitor.MatchesWon++;
                        visitor.Points += 3;
                        local.MatchesLost++;
                    }
                    else // Draw
                    {
                        local.MatchesTied++;
                        visitor.MatchesTied++;
                        local.Points++;
                        visitor.Points++;
                    }

                    _db.Entry(local).State = EntityState.Modified;
                    _db.Entry(visitor).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    // Update positions
                    var teams = await _db.TournamentTeams
                        .Where(tt => tt.TournamentGroupId == oldMatch.TournamentGroupId)
                        .ToListAsync();
                    var i = 1;
                    foreach (var team in teams.OrderByDescending(t => t.Points)
                        .ThenByDescending(t => t.FavorGoals - t.AgainstGoals)
                        .ThenByDescending(t => t.FavorGoals))
                    {
                        team.Position = i;
                        _db.Entry(team).State = EntityState.Modified;
                        i++;
                    }

                    var noPoints = new List<string>();
                    var onePoint = new List<string>();
                    var threePoints = new List<string>();

                    // Update predictions
                    var predictions = await _db.Predictions
                        .Where(p => p.MatchId == oldMatch.MatchId)
                        .ToListAsync();
                    foreach (var prediction in predictions)
                    {
                        var points = 0;
                        if (prediction.LocalGoals == oldMatch.LocalGoals &&
                            prediction.VisitorGoals == oldMatch.VisitorGoals)
                        {
                            points = 3;
                            threePoints.Add(string.Format("userId:{0}", prediction.UserId));
                        }
                        else
                        {
                            var statusPrediction = GetStatus(prediction.LocalGoals, prediction.VisitorGoals);
                            if (statusMatch == statusPrediction)
                            {
                                points = 1;
                                onePoint.Add(string.Format("userId:{0}", prediction.UserId));
                            }
                            else
                            {
                                noPoints.Add(string.Format("userId:{0}", prediction.UserId));
                            }
                        }

                        if (points != 0)
                        {
                            prediction.Points = points;
                            _db.Entry(prediction).State = EntityState.Modified;

                            // Update user
                            var user = await _db.Users.FindAsync(prediction.UserId);
                            user.Points += points;
                            _db.Entry(user).State = EntityState.Modified;

                            // Update points in groups
                            var groupUsers = await _db.GroupUsers.Where(gu => gu.UserId == user.UserId &&
                                                                             gu.IsAccepted && gu.Group.TournamentId == oldMatch.TournamentGroup.TournamentId &&
                                                                             !gu.IsBlocked)
                                .ToListAsync();
                            foreach (var groupUser in groupUsers)
                            {
                                groupUser.Points += points;
                                _db.Entry(groupUser).State = EntityState.Modified;
                            }
                        }
                    }

                    await _db.SaveChangesAsync();
                    transacction.Commit();

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

                    return RedirectToAction(string.Format("DetailsDate/{0}", oldMatch.DateId));
                }
                catch (Exception ex)
                {
                    transacction.Rollback();
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(match);
                }
            }
        }

        private async Task SendNotificationThreePoints(List<string> tags, Match match)
        {
            var message = string.Format("{0} {1} Vs. {2} {3}, Has finished and you have gotten 3 points, congratulations!.",
                match.Local.Initials, match.LocalGoals, match.VisitorGoals, match.Visitor.Initials);
            await SendNotification(tags, message);
        }

        private async Task SendNotificationOnePoint(List<string> tags, Match match)
        {
            var message = string.Format("{0} {1} Vs. {2} {3}, Has finished and you have gotten 1 point, congratulations!.",
                match.Local.Initials, match.LocalGoals, match.VisitorGoals, match.Visitor.Initials);
            await SendNotification(tags, message);
        }

        private async Task SendNotificationNoPoints(List<string> tags, Match match)
        {
            var message = string.Format("{0} {1} Vs. {2} {3}, Has finished... sorry you don't gain any point.",
                match.Local.Initials, match.LocalGoals, match.VisitorGoals, match.Visitor.Initials);
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
                        await _hub.SendGcmNativeNotificationAsync("{ \"data\" : {\"Message\":\"" + message + "\"}}", tags);
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
                        await _hub.SendGcmNativeNotificationAsync("{ \"data\" : {\"Message\":\"" + message + "\"}}", tags20);
                    }
                } while (tags.Count > 0);

            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        private int GetStatus(int localGoals, int visitorGoals)
        {
            if (localGoals > visitorGoals)
            {
                return 1; // Local win
            }

            if (visitorGoals > localGoals)
            {
                return 2; // Visitor win
            }

            return 3; // Draw
        }

        public async Task<ActionResult> CloseMatch(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var match = await _db.Matches.FindAsync(id);

            if (match == null)
            {
                return HttpNotFound();
            }

            if (match.StatusId == 3)
            {
                return RedirectToAction(string.Format("DetailsDate/{0}", match.DateId));
            }

            return View(match);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeletePrediction(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var prediction = await _db.Predictions.FindAsync(id);

            if (prediction == null)
            {
                return HttpNotFound();
            }

            _db.Predictions.Remove(prediction);
            await _db.SaveChangesAsync();
            return RedirectToAction(string.Format("Predictions/{0}", prediction.MatchId));
        }

        public async Task<ActionResult> EditPrediction(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var prediction = await _db.Predictions.FindAsync(id);

            if (prediction == null)
            {
                return HttpNotFound();
            }

            ViewBag.UserId = new SelectList(_db.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName), "UserId", "FullName", prediction.UserId);
            return View(prediction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditPrediction(Prediction prediction)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(prediction).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("Predictions/{0}", prediction.MatchId));
            }

            ViewBag.UserId = new SelectList(_db.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName), "UserId", "FullName", prediction.UserId);
            return View(prediction);
        }

        public async Task<ActionResult> CreatePrediction(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var match = await _db.Matches.FindAsync(id);

            if (match == null)
            {
                return HttpNotFound();
            }

            ViewBag.UserId = new SelectList(_db.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName), "UserId", "FullName");
            var view = new Prediction { MatchId = match.MatchId, Points = 0, Match = match, };
            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreatePrediction(Prediction prediction)
        {
            if (ModelState.IsValid)
            {
                _db.Predictions.Add(prediction);
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("Predictions/{0}", prediction.MatchId));
            }

            var match = await _db.Matches.FindAsync(prediction.MatchId);
            prediction.Match = match;
            ViewBag.UserId = new SelectList(_db.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName), "UserId", "FullName", prediction.UserId);
            return View(prediction);
        }

        public async Task<ActionResult> Predictions(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var match = await _db.Matches.FindAsync(id);

            if (match == null)
            {
                return HttpNotFound();
            }

            return View(match);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteMatch(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var match = await _db.Matches.FindAsync(id);

            if (match == null)
            {
                return HttpNotFound();
            }

            _db.Matches.Remove(match);
            await _db.SaveChangesAsync();
            return RedirectToAction(string.Format("DetailsDate/{0}", match.DateId));
        }

        public async Task<ActionResult> EditMatch(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var match = await _db.Matches.FindAsync(id);

            if (match == null)
            {
                return HttpNotFound();
            }

            ViewBag.LocalLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", match.Local.LeagueId);
            ViewBag.LocalId = new SelectList(_db.Teams.Where(t => t.LeagueId == match.Local.LeagueId).OrderBy(t => t.Name), "TeamId", "Name", match.LocalId);
            ViewBag.VisitorLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", match.Visitor.LeagueId);
            ViewBag.VisitorId = new SelectList(_db.Teams.Where(t => t.LeagueId == match.Visitor.LeagueId).OrderBy(t => t.Name), "TeamId", "Name", match.VisitorId);
            ViewBag.TournamentGroupId = new SelectList(_db.TournamentGroups.Where(tg => tg.TournamentId == match.Date.TournamentId).OrderBy(tg => tg.Name), "TournamentGroupId", "Name", match.TournamentGroupId);
            ViewBag.StatusId = new SelectList(_db.Status.OrderBy(s => s.Name), "StatusId", "Name", match.StatusId);
            var view = ToView(match);
            return View(view);
        }

        private MatchView ToView(Match match)
        {
            // ***
            return new MatchView
            {
                Date = match.Date,
                DateId = match.DateId,
                DateString = string.Format("{0:dd/MM/yyyy}", match.DateTime.ToLocalTime()),
                DateTime = match.DateTime.ToLocalTime(),
                Local = match.Local,
                LocalGoals = match.LocalGoals,
                LocalId = match.LocalId,
                LocalLeagueId = match.Local.LeagueId,
                MatchId = match.MatchId,
                Status = match.Status,
                StatusId = match.StatusId,
                TimeString = string.Format("{0:hh:mm tt}", match.DateTime.ToLocalTime()),
                TournamentGroup = match.TournamentGroup,
                TournamentGroupId = match.TournamentGroupId,
                Visitor = match.Visitor,
                VisitorGoals = match.VisitorGoals,
                VisitorId = match.VisitorId,
                VisitorLeagueId = match.Visitor.LeagueId,
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditMatch(MatchView view)
        {
            if (ModelState.IsValid)
            {
                // ***
                var localDateTime = Convert.ToDateTime(string.Format("{0} {1}", view.DateString, view.TimeString));
                view.DateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime);
                var match = ToMatch(view);
                _db.Entry(match).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("DetailsDate/{0}", view.DateId));
            }

            var date = await _db.Dates.FindAsync(view.DateId);
            ViewBag.LocalLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", view.LocalLeagueId);
            ViewBag.LocalId = new SelectList(_db.Teams.Where(t => t.LeagueId == view.LocalLeagueId).OrderBy(t => t.Name), "TeamId", "Name", view.LocalId);
            ViewBag.VisitorLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", view.VisitorLeagueId);
            ViewBag.VisitorId = new SelectList(_db.Teams.Where(t => t.LeagueId == view.VisitorLeagueId).OrderBy(t => t.Name), "TeamId", "Name", view.VisitorId);
            ViewBag.TournamentGroupId = new SelectList(_db.TournamentGroups.Where(tg => tg.TournamentId == date.TournamentId).OrderBy(tg => tg.Name), "TournamentGroupId", "Name", view.TournamentGroupId);
            ViewBag.StatusId = new SelectList(_db.Status.OrderBy(s => s.Name), "StatusId", "Name", view.StatusId);
            return View(view);
        }

        public async Task<ActionResult> CreateMatch(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var date = await _db.Dates.FindAsync(id);

            if (date == null)
            {
                return HttpNotFound();
            }

            ViewBag.LocalLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name");
            ViewBag.LocalId = new SelectList(_db.Teams.Where(t => t.LeagueId == _db.Leagues.FirstOrDefault().LeagueId).OrderBy(t => t.Name), "TeamId", "Name");
            ViewBag.VisitorLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name");
            ViewBag.VisitorId = new SelectList(_db.Teams.Where(t => t.LeagueId == _db.Leagues.FirstOrDefault().LeagueId).OrderBy(t => t.Name), "TeamId", "Name");
            ViewBag.TournamentGroupId = new SelectList(_db.TournamentGroups.Where(tg => tg.TournamentId == date.TournamentId).OrderBy(tg => tg.Name), "TournamentGroupId", "Name");
            var view = new MatchView { DateId = date.DateId, };
            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateMatch(MatchView view)
        {
            if (ModelState.IsValid)
            {
                view.StatusId = 1;
                // ***
                var localDateTime = Convert.ToDateTime(string.Format("{0} {1}", view.DateString, view.TimeString));
                view.DateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime);
                var match = ToMatch(view);
                _db.Matches.Add(match);
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("DetailsDate/{0}", view.DateId));
            }

            var date = await _db.Dates.FindAsync(view.DateId);
            ViewBag.LocalLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", view.LocalLeagueId);
            ViewBag.LocalId = new SelectList(_db.Teams.Where(t => t.LeagueId == view.LocalLeagueId).OrderBy(t => t.Name), "TeamId", "Name", view.LocalId);
            ViewBag.VisitorLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", view.VisitorLeagueId);
            ViewBag.VisitorId = new SelectList(_db.Teams.Where(t => t.LeagueId == view.VisitorLeagueId).OrderBy(t => t.Name), "TeamId", "Name", view.VisitorId);
            ViewBag.TournamentGroupId = new SelectList(_db.TournamentGroups.Where(tg => tg.TournamentId == date.TournamentId).OrderBy(tg => tg.Name), "TournamentGroupId", "Name", view.TournamentGroupId);
            return View(view);
        }

        private Match ToMatch(MatchView view)
        {
            return new Match
            {
                DateId = view.DateId,
                DateTime = view.DateTime,
                LocalId = view.LocalId,
                MatchId = view.MatchId,
                StatusId = view.StatusId,
                TournamentGroupId = view.TournamentGroupId,
                VisitorId = view.VisitorId,
            };
        }

        public async Task<ActionResult> DetailsDate(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var date = await _db.Dates.FindAsync(id);

            if (date == null)
            {
                return HttpNotFound();
            }

            // *** 
            foreach (var match in date.Matches)
            {
                match.DateTime = match.DateTime.ToLocalTime();
            }

            return View(date);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteTeam(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tournamentTeam = await _db.TournamentTeams.FindAsync(id);

            if (tournamentTeam == null)
            {
                return HttpNotFound();
            }

            _db.TournamentTeams.Remove(tournamentTeam);
            await _db.SaveChangesAsync();
            return RedirectToAction(string.Format("DetailsGroup/{0}", tournamentTeam.TournamentGroupId));
        }

        public async Task<ActionResult> EditTeam(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tournamentTeam = await _db.TournamentTeams.FindAsync(id);

            if (tournamentTeam == null)
            {
                return HttpNotFound();
            }

            ViewBag.LeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", tournamentTeam.Team.LeagueId);
            ViewBag.TeamId = new SelectList(_db.Teams.Where(t => t.LeagueId == tournamentTeam.Team.LeagueId).OrderBy(t => t.Name), "TeamId", "Name", tournamentTeam.Team.TeamId);
            var view = ToView(tournamentTeam);
            return View(view);
        }

        private TournamentTeamView ToView(TournamentTeam tournamentTeam)
        {
            return new TournamentTeamView
            {
                AgainstGoals = tournamentTeam.AgainstGoals,
                FavorGoals = tournamentTeam.FavorGoals,
                LeagueId = tournamentTeam.Team.LeagueId,
                MatchesLost = tournamentTeam.MatchesLost,
                MatchesPlayed = tournamentTeam.MatchesPlayed,
                MatchesTied = tournamentTeam.MatchesTied,
                MatchesWon = tournamentTeam.MatchesWon,
                Points = tournamentTeam.Points,
                Position = tournamentTeam.Position,
                Team = tournamentTeam.Team,
                TeamId = tournamentTeam.TeamId,
                TournamentGroup = tournamentTeam.TournamentGroup,
                TournamentGroupId = tournamentTeam.TournamentGroupId,
                TournamentTeamId = tournamentTeam.TournamentTeamId,
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditTeam(TournamentTeamView view)
        {
            if (ModelState.IsValid)
            {
                var tournamentTeam = ToTournamentTeam(view);
                _db.Entry(tournamentTeam).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("DetailsGroup/{0}", tournamentTeam.TournamentGroupId));
            }

            ViewBag.LeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", view.LeagueId);
            ViewBag.TeamId = new SelectList(_db.Teams.Where(t => t.LeagueId == view.LeagueId).OrderBy(t => t.Name), "TeamId", "Name", view.TeamId);
            return View(view);
        }

        private TournamentTeam ToTournamentTeam(TournamentTeamView view)
        {
            return new TournamentTeam
            {
                AgainstGoals = view.AgainstGoals,
                FavorGoals = view.FavorGoals,
                MatchesLost = view.MatchesLost,
                MatchesPlayed = view.MatchesPlayed,
                MatchesTied = view.MatchesTied,
                MatchesWon = view.MatchesWon,
                Points = view.Points,
                Position = view.Position,
                Team = view.Team,
                TeamId = view.TeamId,
                TournamentGroup = view.TournamentGroup,
                TournamentGroupId = view.TournamentGroupId,
                TournamentTeamId = view.TournamentTeamId,
            };
        }

        public async Task<ActionResult> CreateTeam(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tournamentGroup = await _db.TournamentGroups.FindAsync(id);

            if (tournamentGroup == null)
            {
                return HttpNotFound();
            }

            ViewBag.LeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name");
            ViewBag.TeamId = new SelectList(_db.Teams.Where(t => t.LeagueId == _db.Leagues.FirstOrDefault().LeagueId).OrderBy(t => t.Name), "TeamId", "Name");
            var view = new TournamentTeamView { TournamentGroupId = tournamentGroup.TournamentGroupId, };
            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateTeam(TournamentTeamView view)
        {
            if (ModelState.IsValid)
            {
                var tournamentTeam = ToTournamentTeam(view);
                _db.TournamentTeams.Add(tournamentTeam);
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("DetailsGroup/{0}", view.TournamentGroupId));
            }

            ViewBag.LeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", view.LeagueId);
            ViewBag.TeamId = new SelectList(_db.Teams.Where(t => t.LeagueId == view.LeagueId).OrderBy(t => t.Name), "TeamId", "Name", view.TeamId);
            return View(view);
        }

        public async Task<ActionResult> DetailsGroup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tournamentGroup = await _db.TournamentGroups.FindAsync(id);

            if (tournamentGroup == null)
            {
                return HttpNotFound();
            }

            return View(tournamentGroup);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDate(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var date = await _db.Dates.FindAsync(id);

            if (date == null)
            {
                return HttpNotFound();
            }

            _db.Dates.Remove(date);
            await _db.SaveChangesAsync();
            return RedirectToAction(string.Format("Details/{0}", date.TournamentId));
        }

        public async Task<ActionResult> EditDate(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var date = await _db.Dates.FindAsync(id);

            if (date == null)
            {
                return HttpNotFound();
            }

            return View(date);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditDate(Date date)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(date).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("Details/{0}", date.TournamentId));
            }

            return View(date);
        }

        public async Task<ActionResult> CreateDate(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tournament = await _db.Tournaments.FindAsync(id);

            if (tournament == null)
            {
                return HttpNotFound();
            }

            var view = new Date { TournamentId = tournament.TournamentId, };
            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateDate(Date date)
        {
            if (ModelState.IsValid)
            {
                _db.Dates.Add(date);
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("Details/{0}", date.TournamentId));
            }

            return View(date);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteGroup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tournamentGroup = await _db.TournamentGroups.FindAsync(id);

            if (tournamentGroup == null)
            {
                return HttpNotFound();
            }

            _db.TournamentGroups.Remove(tournamentGroup);
            await _db.SaveChangesAsync();
            return RedirectToAction(string.Format("Details/{0}", tournamentGroup.TournamentId));
        }

        public async Task<ActionResult> EditGroup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tournamentGroup = await _db.TournamentGroups.FindAsync(id);

            if (tournamentGroup == null)
            {
                return HttpNotFound();
            }

            return View(tournamentGroup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditGroup(TournamentGroup tournamentGroup)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(tournamentGroup).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("Details/{0}", tournamentGroup.TournamentId));
            }

            return View(tournamentGroup);
        }

        public async Task<ActionResult> CreateGroup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tournament = await _db.Tournaments.FindAsync(id);

            if (tournament == null)
            {
                return HttpNotFound();
            }

            var view = new TournamentGroup { TournamentId = tournament.TournamentId, };
            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateGroup(TournamentGroup tournamentGroup)
        {
            if (ModelState.IsValid)
            {
                _db.TournamentGroups.Add(tournamentGroup);
                await _db.SaveChangesAsync();
                return RedirectToAction(string.Format("Details/{0}", tournamentGroup.TournamentId));
            }

            return View(tournamentGroup);
        }

        public async Task<ActionResult> Index()
        {
            return View(await _db.Tournaments.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tournament = await _db.Tournaments.FindAsync(id);

            if (tournament == null)
            {
                return HttpNotFound();
            }

            return View(tournament);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(TournamentView view)
        {
            if (ModelState.IsValid)
            {
                var pic = string.Empty;
                var folder = "~/Content/Tournaments";

                if (view.LogoToFile != null)
                {
                    pic = Files.UploadPhoto(view.LogoToFile, folder,"");
                    pic = string.Format("{0}/{1}", folder, pic);
                }

                var tournament = ToTournament(view);
                tournament.Logo = pic;
                _db.Tournaments.Add(tournament);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(view);
        }

        private Tournament ToTournament(TournamentView view)
        {
            return new Tournament
            {
                Groups = view.Groups,
                Logo = view.Logo,
                Name = view.Name,
                TournamentId = view.TournamentId,
                Order = view.Order,
                IsActive = view.IsActive,
            };
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tournament = await _db.Tournaments.FindAsync(id);

            if (tournament == null)
            {
                return HttpNotFound();
            }

            var view = ToView(tournament);
            return View(view);
        }

        private TournamentView ToView(Tournament tournament)
        {
            return new TournamentView
            {
                Groups = tournament.Groups,
                Logo = tournament.Logo,
                Name = tournament.Name,
                TournamentId = tournament.TournamentId,
                Order = tournament.Order,
                IsActive = tournament.IsActive,
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(TournamentView view)
        {
            if (ModelState.IsValid)
            {
                var pic = view.Logo;
                var folder = "~/Content/Tournaments";

                if (view.LogoToFile != null)
                {
                    pic = Files.UploadPhoto(view.LogoToFile, folder,"");
                    pic = string.Format("{0}/{1}", folder, pic);
                }

                var tournament = ToTournament(view);
                tournament.Logo = pic;
                _db.Entry(tournament).State = EntityState.Modified;
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

            var tournament = await _db.Tournaments.FindAsync(id);

            if (tournament == null)
            {
                return HttpNotFound();
            }

            return View(tournament);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var tournament = await _db.Tournaments.FindAsync(id);
            if (tournament == null) throw new ArgumentNullException(nameof(tournament));
            _db.Tournaments.Remove(tournament);
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
    }
    //    public class TournamentsController : Controller
    //    {
    //        private readonly DataContextLocal _db = new DataContextLocal();

    //        private readonly NotificationHubClient _hub;

    //        public TournamentsController()
    //        {
    //            _hub = NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb://psmhub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=VRZxkd15WUJsWXWmkt0UySRsCg6K/9ZSytBIdsV7Grs=", "MainNotification");
    //        }


    //        #region MatchesController

    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        [Authorize(Roles = "Admin")]
    //        public async Task<ActionResult> CloseMatch(Match match)
    //        {
    //            using (var transacction = _db.Database.BeginTransaction())
    //            {
    //                try
    //                {
    //                    // Update match
    //                    var oldMatch = await _db.Matches.FindAsync(match.MatchId);
    //                    oldMatch.LocalGoals = match.LocalGoals;
    //                    oldMatch.VisitorGoals = match.VisitorGoals;
    //                    oldMatch.StatusId = 3;
    //                    _db.Entry(oldMatch).State = EntityState.Modified;

    //                    var statusMatch = GetStatus(match.LocalGoals.Value, match.VisitorGoals.Value);

    //                    // Update tournaments statics
    //                    var local = await _db.TournamentTeams
    //                        .Where(tt => tt.TournamentGroupId == oldMatch.TournamentGroupId &&
    //                                     tt.TeamId == oldMatch.LocalId)
    //                        .FirstOrDefaultAsync();

    //                    var visitor = await _db.TournamentTeams
    //                        .Where(tt => tt.TournamentGroupId == oldMatch.TournamentGroupId &&
    //                                     tt.TeamId == oldMatch.VisitorId)
    //                        .FirstOrDefaultAsync();

    //                    local.MatchesPlayed++;
    //                    local.FavorGoals += oldMatch.LocalGoals.Value;
    //                    local.AgainstGoals += oldMatch.VisitorGoals.Value;

    //                    visitor.MatchesPlayed++;
    //                    visitor.FavorGoals += oldMatch.VisitorGoals.Value;
    //                    visitor.AgainstGoals += oldMatch.LocalGoals.Value;

    //                    if (statusMatch == 1)//Local won
    //                    {
    //                        local.MatchesWon++;
    //                        local.Points += 3;
    //                        visitor.MatchesLost++;
    //                    }
    //                    else if (statusMatch == 2) //visitor won
    //                    {
    //                        visitor.MatchesWon++;
    //                        visitor.Points += 3;
    //                        local.MatchesLost++;
    //                    }
    //                    else //draw
    //                    {
    //                        local.MatchesTied++;
    //                        visitor.MatchesTied++;
    //                        local.Points++;
    //                        visitor.Points++;
    //                    }

    //                    _db.Entry(local).State = EntityState.Modified;
    //                    _db.Entry(visitor).State = EntityState.Modified;
    //                    await _db.SaveChangesAsync();

    //                    // Update positions
    //                    var teams = await _db.TournamentTeams
    //                        .Where(tt => tt.TournamentGroupId == oldMatch.TournamentGroupId)
    //                        .ToListAsync();
    //                    var i = 1;

    //                    foreach (var team in teams.OrderByDescending(t => t.Points)
    //                        .ThenByDescending(t => t.FavorGoals - t.AgainstGoals)
    //                        .ThenByDescending(t => t.FavorGoals))
    //                    {
    //                        team.Position = i;
    //                        _db.Entry(team).State = EntityState.Modified;
    //                        i++;
    //                    }

    //                    //crear la lista de los usuarios
    //                    var noPoints = new List<string>();
    //                    var onePoints = new List<string>();
    //                    var threePoints = new List<string>();

    //                    // Update predictions
    //                    var predictions = await _db.Predictions.Where(p => p.MatchId == oldMatch.MatchId).ToListAsync();
    //                    foreach (var prediction in predictions)
    //                    {
    //                        var points = 0;
    //                        if (prediction.LocalGoals == oldMatch.LocalGoals &&
    //                            prediction.VisitorGoals == oldMatch.VisitorGoals) //adivino el estatus exacto
    //                        {
    //                            points = 3;
    //                            threePoints.Add(string.Format("userId:{0}",prediction.UserId)); //userId en este caso es nuestro tag referencial
    //                        }
    //                        else
    //                        {
    //                            var statusPrediction = GetStatus(prediction.LocalGoals, prediction.VisitorGoals);
    //                            if (statusMatch == statusPrediction) //no adivino el resultado exacto pero gano quien el indico
    //                            {
    //                                points = 1;
    //                                onePoints.Add(string.Format("userId:{0}", prediction.UserId)); //userId en este caso es nuestro tag referencial
    //                            }
    //                            else
    //                            {
    //                                noPoints.Add(string.Format("userId:{0}", prediction.UserId)); //userId en este caso es nuestro tag referencial
    //                            }
    //                        }

    //                        if (points != 0)
    //                        {
    //                            prediction.Points = points;
    //                            _db.Entry(prediction).State = EntityState.Modified;
    //                        }

    //                        // Update user
    //                        var user = await _db.Users.FindAsync(prediction.UserId);
    //                        user.Points += points;
    //                        _db.Entry(user).State = EntityState.Modified;

    //                        // Update points in groups
    //                        var groupUsers = await _db.GroupUsers.Where(gu => gu.UserId == user.UserId &&
    //                                                                         gu.IsAccepted &&
    //                                                                         !gu.IsBlocked)
    //                            .ToListAsync();
    //                        foreach (var groupUser in groupUsers)
    //                        {
    //                            groupUser.Points += points;
    //                            _db.Entry(groupUser).State = EntityState.Modified;
    //                        }
    //                    }

    //                    await _db.SaveChangesAsync();
    //                    transacction.Commit();

    //                    await SendNotificationNoPoints(noPoints, oldMatch);
    //                    await SendNotificationOnePoints(onePoints, oldMatch);
    //                    await SendNotificationThreePoints(threePoints, oldMatch);


    //                    return RedirectToAction(string.Format("DetailsDate/{0}", oldMatch.DateId));
    //                }
    //                catch (Exception ex)
    //                {
    //                    transacction.Rollback();
    //                    ModelState.AddModelError(string.Empty, ex.Message);
    //                    return View(match);
    //                }
    //            }
    //        }

    //        private async Task SendNotificationThreePoints(List<string> tags, Match match)
    //        {
    //            var message = string.Format("{0} {1} Vs. {2} {3}, Ha finalizado y has obtenido 3 puntos, Felicidades!",match.Local.Initials, 
    //                match.LocalGoals,match.VisitorGoals,match.Visitor.Initials);
    //            await _hub.SendGcmNativeNotificationAsync("{ \"data\" : {\"Message\":\"" + message + "\"}}", tags);
    //        }

    //        private async Task SendNotificationOnePoints(List<string> tags, Match match)
    //        {
    //            var message = string.Format("{0} {1} Vs. {2} {3}, Ha finalizado y has obtenido 1 punto, Felicidades!", match.Local.Initials,
    //                match.LocalGoals, match.VisitorGoals, match.Visitor.Initials);
    //            await _hub.SendGcmNativeNotificationAsync("{ \"data\" : {\"Message\":\"" + message + "\"}}", tags);
    //        }

    //        private async Task SendNotificationNoPoints(List<string> tags, Match match)
    //        {
    //            var message = string.Format("{0} {1} Vs. {2} {3}, Ha finalizado y no has obtenido puntos, Lo sentimos!", match.Local.Initials,
    //                match.LocalGoals, match.VisitorGoals, match.Visitor.Initials);
    //            await _hub.SendGcmNativeNotificationAsync("{ \"data\" : {\"Message\":\"" + message + "\"}}", tags);
    //        }

    //        //14.1.2	GetStatus
    //        private int GetStatus(int localGoals, int visitorGoals)
    //        {
    //            if (localGoals > visitorGoals)
    //            {
    //                return 1; // Local win
    //            }

    //            if (visitorGoals > localGoals)
    //            {
    //                return 2; // Visitor win
    //            }

    //            return 3; // Draw
    //        }

    //       // 14.1.3	CloseMatch Get
    //        public async Task<ActionResult> CloseMatch(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var match = await _db.Matches.FindAsync(id);

    //            if (match == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            if (match.StatusId == 3)
    //            {
    //                return RedirectToAction(string.Format("DetailsDate/{0}", match.DateId));
    //            }

    //            return View(match);
    //        }

    //        //public async Task<ActionResult> CloseMatch(int? id)
    //        //{

    //        //    if (id == null)
    //        //    {
    //        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //        //    }

    //        //    var match = await db.Matches.FindAsync(id);

    //        //    if (match == null)
    //        //    {
    //        //        return HttpNotFound();
    //        //    }

    //        //    return View(match);
    //        //}

    //        public async Task<ActionResult> EditMatch(int? id)
    //        {

    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var match = await _db.Matches.FindAsync(id);

    //            if (match == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            ViewBag.LocalLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", match.Local.LeagueId);
    //            ViewBag.LocalId = new SelectList(_db.Teams.Where(t => t.LeagueId == match.Local.LeagueId).OrderBy(t => t.Name), "TeamId", "Name", match.Local.TeamId);

    //            ViewBag.VisitorLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", match.Visitor.LeagueId);
    //            ViewBag.VisitorId = new SelectList(_db.Teams.Where(t => t.LeagueId == match.Visitor.LeagueId).OrderBy(t => t.Name), "TeamId", "Name", match.Visitor.TeamId);

    //            ViewBag.TournamentGroupId = new SelectList(_db.TournamentGroups.Where(tg => tg.TournamentId == match.Date.TournamentId).OrderBy(tg => tg.Name), "TournamentGroupId", "Name",match.TournamentGroupId);

    //            var view = ToView(match);
    //            return View(view);
    //        }

    //        private MatchView ToView(Match match)
    //        {
    //            return new MatchView
    //            {
    //                // Date = view.Date,
    //                DateId = match.DateId,
    //                DateTime = match.DateTime,
    //                // Local = view.Local,
    //                //  LocalGoals = view.LocalGoals,
    //                LocalId = match.LocalId,
    //                //  MatchId = view.LocalId,
    //                //  Status = view.Status,
    //                StatusId = match.StatusId,
    //                TournamentGroupId = match.TournamentGroupId,
    //                VisitorId = match.VisitorId

    //            };
    //        }

    //        // POST: Matches/Edit/5
    //        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    //        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> EditMatch(Match match)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                _db.Entry(match).State = EntityState.Modified;
    //                await _db.SaveChangesAsync();
    //                return RedirectToAction(String.Format("DetailsGroup/{0}", match.TournamentGroupId));
    //            }
    //            ViewBag.LocalLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name");
    //            ViewBag.LocalId = new SelectList(_db.Teams.Where(t => t.LeagueId == _db.Leagues.FirstOrDefault().LeagueId).OrderBy(t => t.Name), "TeamId", "Name");

    //            ViewBag.VisitorLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name");
    //            ViewBag.VisitorId = new SelectList(_db.Teams.Where(t => t.LeagueId == _db.Leagues.FirstOrDefault().LeagueId).OrderBy(t => t.Name), "TeamId", "Name");

    //            ViewBag.TournamentGroupId = new SelectList(_db.TournamentGroups.Where(tg => tg.TournamentId == match.Date.TournamentId).OrderBy(tg => tg.Name), "TournamentGroupId", "Name");

    //            return View(match);
    //        }

    //        public async Task<ActionResult> DetailsDate(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var date = await _db.Dates.FindAsync(id);

    //            if (date == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            return View(date);
    //        }

    //        public async Task<ActionResult> CreateMatch(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var date = await _db.Dates.FindAsync(id);

    //            if (date == null)
    //            {
    //                return HttpNotFound();
    //            }



    //            ViewBag.LocalLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name");
    //            ViewBag.LocalId = new SelectList(_db.Teams.Where(t => t.LeagueId == _db.Leagues.FirstOrDefault().LeagueId).OrderBy(t => t.Name), "TeamId", "Name");

    //            ViewBag.VisitorLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name");
    //            ViewBag.VisitorId = new SelectList(_db.Teams.Where(t => t.LeagueId == _db.Leagues.FirstOrDefault().LeagueId).OrderBy(t => t.Name), "TeamId", "Name");

    //            ViewBag.TournamentGroupId = new SelectList(_db.TournamentGroups.Where(tg => tg.TournamentId == date.TournamentId).OrderBy(tg => tg.Name), "TournamentGroupId", "Name");


    //            var view = new MatchView { DateId = date.DateId, };
    //            return View(view);
    //        }


    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> CreateMatch(MatchView view)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                view.StatusId = 1;
    //                // view.DateTime = Convert.ToDateTime(string.Format("{0} {1}", view.DateString, view.TimeString));
    //                view.DateTime = Convert.ToDateTime($"{view.DateString} {view.TimeString}");
    //                var match = ToMatch(view);
    //                _db.Matches.Add(match);
    //                await _db.SaveChangesAsync();
    //                return RedirectToAction($"DetailsDate/{view.DateId}");
    //                //  return RedirectToAction(string.Format("DetailsDate/{0}", view.DateId));

    //            }
    //            var date = await _db.Dates.FindAsync(view.DateId);

    //            ViewBag.LocalLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", view.LocalLeagueId);
    //            ViewBag.LocalId = new SelectList(_db.Teams.Where(t => t.LeagueId == view.LocalLeagueId).OrderBy(t => t.Name), "TeamId", "Name", view.LocalId);
    //            ViewBag.VisitorLeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", view.VisitorLeagueId);
    //            ViewBag.VisitorId = new SelectList(_db.Teams.Where(t => t.LeagueId == view.VisitorLeagueId).OrderBy(t => t.Name), "TeamId", "Name", view.VisitorLeagueId);

    //            ViewBag.TournamentGroupId = new SelectList(_db.TournamentGroups.Where(tg => tg.TournamentId == date.TournamentId).OrderBy(tg => tg.Name), "TournamentGroupId", "Name");


    //            return View(view);
    //        }

    //        private Match ToMatch(MatchView view)
    //        {
    //            return new Match
    //            {
    //                // Date = view.Date,
    //                DateId = view.DateId,
    //                DateTime = view.DateTime,
    //                // Local = view.Local,
    //                //  LocalGoals = view.LocalGoals,
    //                LocalId = view.LocalId,
    //                //  MatchId = view.LocalId,
    //                //  Status = view.Status,
    //                StatusId = view.StatusId,
    //                TournamentGroupId = view.TournamentGroupId,
    //                VisitorId = view.VisitorId
    //            };
    //        }

    //        #endregion

    //        #region TeamController

    //        public async Task<ActionResult> DeleteTeam(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var tournamentTeam = await _db.TournamentTeams.FindAsync(id);

    //            if (tournamentTeam == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            _db.TournamentTeams.Remove(tournamentTeam);
    //            await _db.SaveChangesAsync();
    //            return RedirectToAction(String.Format("DetailsGroup/{0}", tournamentTeam.TournamentGroupId));


    //        }
    //        public async Task<ActionResult> EditTeam(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var tournamentTeam = await _db.TournamentTeams.FindAsync(id);

    //            if (tournamentTeam == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            ViewBag.LeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", tournamentTeam.Team.LeagueId);
    //            ViewBag.TeamId = new SelectList(_db.Teams.Where(t => t.LeagueId == tournamentTeam.Team.LeagueId).OrderBy(t => t.Name), "TeamId", "Name", tournamentTeam.Team.TeamId);
    //            var view = ToView(tournamentTeam);
    //            return View(view);
    //        }

    //        private TournamentTeamView ToView(TournamentTeam tournamentTeam)
    //        {
    //            return new TournamentTeamView
    //            {
    //                AgainstGoals = tournamentTeam.AgainstGoals,
    //                FavorGoals = tournamentTeam.FavorGoals,
    //                LeagueId = tournamentTeam.Team.LeagueId,
    //                MatchesLost = tournamentTeam.MatchesLost,
    //                MatchesPlayed = tournamentTeam.MatchesPlayed,
    //                MatchesTied = tournamentTeam.MatchesTied,
    //                MatchesWon = tournamentTeam.MatchesWon,
    //                Points = tournamentTeam.Points,
    //                Position = tournamentTeam.Position,
    //                Team = tournamentTeam.Team,
    //                TeamId = tournamentTeam.TeamId,
    //                TournamentGroup = tournamentTeam.TournamentGroup,
    //                TournamentGroupId = tournamentTeam.TournamentGroupId,
    //                TournamentTeamId = tournamentTeam.TournamentTeamId,
    //            };
    //        }

    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> EditTeam(TournamentTeamView view)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                var tournamentTeam = ToTournamentTeam(view);
    //                _db.Entry(tournamentTeam).State = EntityState.Modified;
    //                await _db.SaveChangesAsync();
    //                return RedirectToAction(String.Format("DetailsGroup/{0}", tournamentTeam.TournamentGroupId));
    //            }
    //            ViewBag.LeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", view.LeagueId);
    //            ViewBag.TeamId = new SelectList(_db.Teams.Where(t => t.LeagueId == view.LeagueId).OrderBy(t => t.Name), "TeamId", "Name", view.TeamId);
    //            return View(view);
    //        }

    //        private TournamentTeam ToTournamentTeam(TournamentTeamView view)
    //        {
    //            return new TournamentTeam
    //            {
    //                AgainstGoals = view.AgainstGoals,
    //                FavorGoals = view.FavorGoals,
    //                MatchesLost = view.MatchesLost,
    //                MatchesPlayed = view.MatchesPlayed,
    //                MatchesTied = view.MatchesTied,
    //                MatchesWon = view.MatchesWon,
    //                Points = view.Points,
    //                Position = view.Position,
    //                Team = view.Team,
    //                TeamId = view.TeamId,
    //                TournamentGroup = view.TournamentGroup,
    //                TournamentGroupId = view.TournamentGroupId,
    //                TournamentTeamId = view.TournamentTeamId,
    //            };
    //        }


    //        public async Task<ActionResult> CreateTeam(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var tournamentGroups = await _db.TournamentGroups.FindAsync(id);

    //            if (tournamentGroups == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            ViewBag.LeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name");
    //            ViewBag.TeamId = new SelectList(_db.Teams.Where(t => t.LeagueId == _db.Leagues.FirstOrDefault().LeagueId).OrderBy(t => t.Name), "TeamId", "Name");
    //            var view = new TournamentTeamView { TournamentGroupId = tournamentGroups.TournamentGroupId, };
    //            return View(view);
    //        }

    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> CreateTeam(TournamentTeamView view)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                var tournamentTeam = ToTournamentTeam(view);
    //                _db.TournamentTeams.Add(tournamentTeam);
    //                await _db.SaveChangesAsync();
    //                return RedirectToAction(String.Format("DetailsGroup/{0}", tournamentTeam.TournamentGroupId));
    //            }

    //            //ViewBag.TeamId = new SelectList(db.Teams.OrderBy(t=>t.Name), "TeamId", "Name", tournamentTeam.TeamId);
    //            ViewBag.LeagueId = new SelectList(_db.Leagues.OrderBy(t => t.Name), "LeagueId", "Name", view.LeagueId);
    //            ViewBag.TeamId = new SelectList(_db.Teams.Where(t => t.LeagueId == view.LeagueId).OrderBy(t => t.Name), "TeamId", "Name", view.TeamId);

    //            return View(view);
    //        }


    //        public async Task<ActionResult> DetailsGroup(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var tournamentGroups = await _db.TournamentGroups.FindAsync(id);

    //            if (tournamentGroups == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            return View(tournamentGroups);
    //        }


    //        #endregion

    //        #region DatesController

    //        public async Task<ActionResult> CreateDate(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var tournament = await _db.Tournaments.FindAsync(id);

    //            if (tournament == null)
    //            {
    //                return HttpNotFound();
    //            }
    //            var view = new Date { TournamentId = tournament.TournamentId };
    //            return View(view);
    //        }

    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> CreateDate(Date date)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                _db.Dates.Add(date);
    //                await _db.SaveChangesAsync();
    //                return RedirectToAction(string.Format("Details/{0}", date.TournamentId));
    //            }

    //            return View(date);
    //        }

    //        public async Task<ActionResult> EditDate(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }
    //            var date = await _db.Dates.FindAsync(id);
    //            if (date == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            return View(date);
    //        }

    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> EditDate(Date date)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                _db.Entry(date).State = EntityState.Modified;
    //                await _db.SaveChangesAsync();
    //                return RedirectToAction(string.Format("Details/{0}", date.TournamentId));
    //            }

    //            return View(date);
    //        }

    //        public async Task<ActionResult> DeleteDate(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }
    //            var date = await _db.Dates.FindAsync(id);
    //            if (date == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            _db.Dates.Remove(date);
    //            await _db.SaveChangesAsync();
    //            return RedirectToAction(string.Format("Details/{0}", date.TournamentId));
    //        }

    //        #endregion

    //        #region GroupsController
    //        public async Task<ActionResult> CreateGroup(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var tournament = await _db.Tournaments.FindAsync(id);

    //            if (tournament == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            //    ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "Name");
    //            var view = new TournamentGroup { TournamentId = tournament.TournamentId, };
    //            return View(view);
    //        }

    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> CreateGroup(TournamentGroup tournamentGroup)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                _db.TournamentGroups.Add(tournamentGroup);
    //                await _db.SaveChangesAsync();
    //                return RedirectToAction(string.Format("Details/{0}", tournamentGroup.TournamentId));
    //            }

    //            //   ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "Name", tournamentGroup.TournamentId);
    //            return View(tournamentGroup);
    //        }

    //        public async Task<ActionResult> EditGroup(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var tournamentGroup = await _db.TournamentGroups.FindAsync(id);

    //            if (tournamentGroup == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            // ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "Name", tournamentGroup.TournamentId);
    //            return View(tournamentGroup);
    //        }

    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> EditGroup(TournamentGroup tournamentGroup)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                _db.Entry(tournamentGroup).State = EntityState.Modified;
    //                await _db.SaveChangesAsync();
    //                return RedirectToAction(string.Format("Details/{0}", tournamentGroup.TournamentId));
    //            }
    //            //  ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "Name", tournamentGroup.TournamentId);
    //            return View(tournamentGroup);
    //        }


    //        public async Task<ActionResult> DeleteGroup(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var tournamentGroup = await _db.TournamentGroups.FindAsync(id);

    //            if (tournamentGroup == null)
    //            {
    //                return HttpNotFound();
    //            }

    //            _db.TournamentGroups.Remove(tournamentGroup);
    //            await _db.SaveChangesAsync();
    //            return RedirectToAction(string.Format("Details/{0}", tournamentGroup.TournamentId));
    //        }

    //        #endregion

    //        #region TournamentController

    //        public async Task<ActionResult> Index()
    //        {
    //            return View(await _db.Tournaments.ToListAsync());
    //        }

    //        public async Task<ActionResult> Details(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }
    //            var tournament = await _db.Tournaments.FindAsync(id);
    //            if (tournament == null)
    //            {
    //                return HttpNotFound();
    //            }
    //            return View(tournament);
    //        }

    //        public ActionResult Create()
    //        {
    //            return View();
    //        }

    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> Create(TournamentView view)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                var pic = string.Empty;
    //                var folder = "~/Content/Logos";

    //                if (view.LogoFile != null)
    //                {
    //                    pic = FilesHelper.UploadPhoto(view.LogoFile, folder,"");
    //                    pic = string.Format("{0}/{1}", folder, pic);
    //                }

    //                var tournament = ToTournament(view);
    //                tournament.Logoxx = pic;

    //                _db.Tournaments.Add(tournament);
    //                await _db.SaveChangesAsync();
    //                return RedirectToAction("Index");
    //            }

    //            return View(view);
    //        }

    //        private Tournament ToTournament(TournamentView view)
    //        {
    //            return new Tournament
    //            {
    //                TournamentId = view.TournamentId,
    //                Logoxx = view.Logoxx,
    //                Name = view.Name,
    //                IsActive = view.IsActive,
    //                Order = view.Order
    //            };
    //        }

    //        public async Task<ActionResult> Edit(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }

    //            var tournament = await _db.Tournaments.FindAsync(id);

    //            if (tournament == null)
    //            {
    //                return HttpNotFound();
    //            }
    //            var view = ToView(tournament);
    //            return View(view);
    //        }

    //        private TournamentView ToView(Tournament tournament)
    //        {
    //            return new TournamentView
    //            {
    //                TournamentId = tournament.TournamentId,
    //                Logoxx = tournament.Logoxx,
    //                Name = tournament.Name,
    //                IsActive = tournament.IsActive,
    //                Order = tournament.Order
    //            };
    //        }

    //        [HttpPost]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> Edit(TournamentView view)
    //        {
    //            if (ModelState.IsValid)
    //            {

    //                var pic = view.Logoxx;
    //                var folder = "~/Content/Logos";

    //                if (view.LogoFile != null)
    //                {
    //                    pic = FilesHelper.UploadPhoto(view.LogoFile, folder,"");
    //                    pic = string.Format("{0}/{1}", folder, pic);
    //                }

    //                var tournament = ToTournament(view);
    //                tournament.Logoxx = pic;

    //                _db.Entry(tournament).State = EntityState.Modified;
    //                await _db.SaveChangesAsync();
    //                return RedirectToAction("Index");
    //            }
    //            return View(view);
    //        }

    //        public async Task<ActionResult> Delete(int? id)
    //        {
    //            if (id == null)
    //            {
    //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //            }
    //            var tournament = await _db.Tournaments.FindAsync(id);
    //            if (tournament == null)
    //            {
    //                return HttpNotFound();
    //            }
    //            return View(tournament);
    //        }

    //        [HttpPost, ActionName("Delete")]
    //        [ValidateAntiForgeryToken]
    //        public async Task<ActionResult> DeleteConfirmed(int id)
    //        {
    //            var tournament = await _db.Tournaments.FindAsync(id);
    //            if (tournament != null) _db.Tournaments.Remove(tournament);
    //            await _db.SaveChangesAsync();
    //            return RedirectToAction("Index");
    //        }

    //        protected override void Dispose(bool disposing)
    //        {
    //            if (disposing)
    //            {
    //                _db.Dispose();
    //            }
    //            base.Dispose(disposing);
    //        }

    //        #endregion

    //    }
    //}

}