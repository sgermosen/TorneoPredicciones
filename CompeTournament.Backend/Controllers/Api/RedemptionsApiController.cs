namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Shared.Tournaments;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/groups/{groupId:int}")]
    public class RedemptionsApiController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RedemptionsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("redemption-items")]
        public async Task<ActionResult<List<RedemptionItemDto>>> Items(int groupId)
        {
            var userId = CurrentUserId;

            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }

            var canManage = CanManageOwnedBy(group.CreatedBy);
            var isMember = await _context.GroupUsers
                .AnyAsync(gu => gu.GroupId == groupId && gu.ApplicationUserId == userId && gu.IsAccepted && !gu.IsBlocked);
            if (!isMember && !canManage)
            {
                return StatusCode(403, new { message = "No perteneces a este torneo." });
            }

            var query = _context.RedemptionItems.Where(ri => ri.GroupId == groupId);
            if (!canManage)
            {
                query = query.Where(ri => ri.Active);
            }

            var items = await query
                .OrderBy(ri => ri.Name)
                .AsNoTracking()
                .ToListAsync();

            return Ok(items.Select(MapItem).ToList());
        }

        [HttpPost("redemption-items")]
        public async Task<ActionResult<RedemptionItemDto>> CreateItem(int groupId, [FromBody] CreateRedemptionItemRequest request)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            if (!group.CreditsEnabled)
            {
                return BadRequest(new { message = "Este torneo no tiene fichas activadas." });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Escribe el nombre del premio." });
            }

            if (request.CostCredits <= 0)
            {
                return BadRequest(new { message = "El costo en fichas debe ser mayor que cero." });
            }

            if (request.Stock.HasValue && request.Stock.Value < 0)
            {
                return BadRequest(new { message = "El inventario no puede ser negativo." });
            }

            var item = new RedemptionItem
            {
                GroupId = groupId,
                Name = request.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                CostCredits = request.CostCredits,
                Stock = request.Stock,
                IsDigital = request.IsDigital,
                Active = true
            };
            _context.RedemptionItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(MapItem(item));
        }

        [HttpPost("redemption-items/{itemId:int}/active")]
        public async Task<IActionResult> SetItemActive(int groupId, int itemId, [FromBody] SetItemActiveRequest request)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            var item = await _context.RedemptionItems.FirstOrDefaultAsync(ri => ri.Id == itemId && ri.GroupId == groupId);
            if (item == null)
            {
                return NotFound();
            }

            item.Active = request.Active;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("redeem")]
        public async Task<ActionResult<RedemptionDto>> Redeem(int groupId, [FromBody] RedeemRequest request)
        {
            var userId = CurrentUserId;

            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (!group.CreditsEnabled)
            {
                return BadRequest(new { message = "Este torneo no tiene fichas activadas." });
            }

            var member = await _context.GroupUsers
                .Include(gu => gu.ApplicationUser)
                .FirstOrDefaultAsync(gu => gu.GroupId == groupId && gu.ApplicationUserId == userId && gu.IsAccepted && !gu.IsBlocked);
            if (member == null)
            {
                return Forbid();
            }

            var item = await _context.RedemptionItems
                .FirstOrDefaultAsync(ri => ri.Id == request.RedemptionItemId && ri.GroupId == groupId);
            if (item == null)
            {
                return NotFound(new { message = "El premio no existe en este torneo." });
            }

            if (!item.Active)
            {
                return BadRequest(new { message = "Ese premio no está disponible." });
            }

            if (item.Stock.HasValue && item.Stock.Value <= 0)
            {
                return BadRequest(new { message = "Ese premio está agotado." });
            }

            if (member.Credits < item.CostCredits)
            {
                return BadRequest(new { message = "No tienes suficientes fichas para canjear este premio." });
            }

            member.Credits -= item.CostCredits;
            _context.CreditTransactions.Add(new CreditTransaction
            {
                GroupUserId = member.Id,
                Amount = -item.CostCredits,
                BalanceAfter = member.Credits,
                Reason = "Redeemed"
            });

            if (item.Stock.HasValue)
            {
                item.Stock = item.Stock.Value - 1;
            }

            var redemption = new Redemption
            {
                GroupUserId = member.Id,
                RedemptionItemId = item.Id,
                CostCredits = item.CostCredits,
                Status = "Requested",
                DeliveryInfo = string.IsNullOrWhiteSpace(request.DeliveryInfo) ? null : request.DeliveryInfo.Trim()
            };
            _context.Redemptions.Add(redemption);
            await _context.SaveChangesAsync();

            return Ok(MapRedemption(redemption, item, member.ApplicationUserId, member.ApplicationUser != null ? member.ApplicationUser.FullName : null));
        }

        [HttpGet("redemptions/mine")]
        public async Task<ActionResult<List<RedemptionDto>>> Mine(int groupId)
        {
            var userId = CurrentUserId;

            var redemptions = await _context.Redemptions
                .Include(r => r.RedemptionItem)
                .Where(r => r.CreatedBy == userId && r.RedemptionItem.GroupId == groupId)
                .OrderByDescending(r => r.Id)
                .AsNoTracking()
                .ToListAsync();

            return Ok(redemptions.Select(r => MapRedemption(r, r.RedemptionItem, userId, null)).ToList());
        }

        [HttpGet("redemptions")]
        public async Task<ActionResult<List<RedemptionDto>>> All(int groupId)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            var redemptions = await _context.Redemptions
                .Include(r => r.RedemptionItem)
                .Include(r => r.GroupUser)
                    .ThenInclude(gu => gu.ApplicationUser)
                .Where(r => r.RedemptionItem.GroupId == groupId)
                .OrderByDescending(r => r.Id)
                .AsNoTracking()
                .ToListAsync();

            return Ok(redemptions.Select(r => MapRedemption(
                r,
                r.RedemptionItem,
                r.GroupUser != null ? r.GroupUser.ApplicationUserId : null,
                r.GroupUser != null && r.GroupUser.ApplicationUser != null ? r.GroupUser.ApplicationUser.FullName : null)).ToList());
        }

        [HttpPost("redemptions/{redemptionId:int}/deliver")]
        public async Task<IActionResult> Deliver(int groupId, int redemptionId, [FromBody] DeliverRedemptionRequest request)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            var redemption = await _context.Redemptions
                .Include(r => r.RedemptionItem)
                .FirstOrDefaultAsync(r => r.Id == redemptionId && r.RedemptionItem.GroupId == groupId);
            if (redemption == null)
            {
                return NotFound();
            }

            if (redemption.Status != "Requested")
            {
                return BadRequest(new { message = "Ese canje ya fue procesado." });
            }

            redemption.Status = "Delivered";
            redemption.DeliveryReference = string.IsNullOrWhiteSpace(request?.DeliveryReference) ? null : request.DeliveryReference.Trim();
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("redemptions/{redemptionId:int}/reject")]
        public async Task<IActionResult> Reject(int groupId, int redemptionId)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            var redemption = await _context.Redemptions
                .Include(r => r.RedemptionItem)
                .Include(r => r.GroupUser)
                .FirstOrDefaultAsync(r => r.Id == redemptionId && r.RedemptionItem.GroupId == groupId);
            if (redemption == null)
            {
                return NotFound();
            }

            if (redemption.Status != "Requested")
            {
                return BadRequest(new { message = "Ese canje ya fue procesado." });
            }

            Refund(redemption);
            redemption.Status = "Rejected";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("redemptions/{redemptionId:int}/cancel")]
        public async Task<IActionResult> Cancel(int groupId, int redemptionId)
        {
            var userId = CurrentUserId;

            var redemption = await _context.Redemptions
                .Include(r => r.RedemptionItem)
                .Include(r => r.GroupUser)
                .FirstOrDefaultAsync(r => r.Id == redemptionId && r.RedemptionItem.GroupId == groupId);
            if (redemption == null)
            {
                return NotFound();
            }

            if (redemption.CreatedBy != userId)
            {
                return Forbid();
            }

            if (redemption.Status != "Requested")
            {
                return BadRequest(new { message = "Ese canje ya fue procesado." });
            }

            Refund(redemption);
            redemption.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private void Refund(Redemption redemption)
        {
            var member = redemption.GroupUser;
            if (member != null)
            {
                member.Credits += redemption.CostCredits;
                _context.CreditTransactions.Add(new CreditTransaction
                {
                    GroupUserId = member.Id,
                    Amount = redemption.CostCredits,
                    BalanceAfter = member.Credits,
                    Reason = "Refund"
                });
            }

            var item = redemption.RedemptionItem;
            if (item != null && item.Stock.HasValue)
            {
                item.Stock = item.Stock.Value + 1;
            }
        }

        private static RedemptionItemDto MapItem(RedemptionItem item)
        {
            return new RedemptionItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                CostCredits = item.CostCredits,
                Stock = item.Stock,
                IsDigital = item.IsDigital,
                Active = item.Active
            };
        }

        private static RedemptionDto MapRedemption(Redemption redemption, RedemptionItem item, string userId, string userFullName)
        {
            return new RedemptionDto
            {
                Id = redemption.Id,
                RedemptionItemId = redemption.RedemptionItemId,
                ItemName = item?.Name,
                IsDigital = item != null && item.IsDigital,
                CostCredits = redemption.CostCredits,
                Status = redemption.Status,
                DeliveryInfo = redemption.DeliveryInfo,
                DeliveryReference = redemption.DeliveryReference,
                UserId = userId,
                UserFullName = userFullName,
                CreatedAt = redemption.CreatedAt
            };
        }
    }
}
