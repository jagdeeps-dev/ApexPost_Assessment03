using ApexPost.Web.Middleware;
using ApexPost.Application;
using ApexPost.Domain;
using ApexPost.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexPost.Web.Controllers;

[Authorize(Roles = "Administrator"), RestrictedEndpoint]
public sealed class BranchesController(IBranchRepository branches, IUnitOfWork unitOfWork, IPlatformQueryService queries) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await queries.GetBranchesAsync(cancellationToken));
    [HttpGet] public IActionResult Create() => View("Edit", new BranchEditViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BranchEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("Edit", model);
        var all = await branches.GetAllAsync(cancellationToken);
        if (all.Any(x => x.Code.Equals(model.Code, StringComparison.OrdinalIgnoreCase))) { ModelState.AddModelError(nameof(model.Code), "Branch code already exists."); return View("Edit", model); }
        await branches.AddAsync(new Branch { Code = model.Code.Trim().ToUpperInvariant(), Name = model.Name.Trim(), Region = model.Region.Trim(), OpeningHours = model.OpeningHours.Trim(), Status = BranchStatus.Active, Counters = [new Counter(Guid.NewGuid().ToString("N"), model.CounterName.Trim())] }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
    [HttpGet] public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var item = await branches.GetByIdAsync(id, cancellationToken); if (item is null) return NotFound();
        return View(new BranchEditViewModel { Id = item.Id, Code = item.Code, Name = item.Name, Region = item.Region, OpeningHours = item.OpeningHours, CounterName = item.Counters.FirstOrDefault()?.Name ?? "Counter 1" });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, BranchEditViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        if (!ModelState.IsValid) return View(model);
        var item = await branches.GetByIdAsync(id, cancellationToken); if (item is null) return NotFound();
        item.Name = model.Name.Trim(); item.Region = model.Region.Trim(); item.OpeningHours = model.OpeningHours.Trim();
        if (item.Counters.Count == 0) item.Counters.Add(new Counter(Guid.NewGuid().ToString("N"), model.CounterName.Trim()));
        else item.Counters[0] = item.Counters[0] with { Name = model.CounterName.Trim() };
        await branches.UpdateAsync(item, cancellationToken); await unitOfWork.SaveChangesAsync(cancellationToken); return RedirectToAction(nameof(Index));
    }
}
