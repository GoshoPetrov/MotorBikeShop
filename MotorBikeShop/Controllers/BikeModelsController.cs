using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Data;

public class BikeModelsController : Controller
{
    private readonly MotorBikeShopContext _context;

    public BikeModelsController(MotorBikeShopContext context)
    {
        _context = context;
    }

    // GET: BikeModels
    public async Task<IActionResult> Index()
    {
        var bikes = await _context.BikeModels.ToListAsync();
        return View(bikes ?? new List<BikeModel>());
    }

    // GET: BikeModels/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var bike = await _context.BikeModels.FirstOrDefaultAsync(m => m.Id == id);

        if (bike == null) return NotFound();

        return View(bike);
    }

    // GET: BikeModels/Create
    public IActionResult Create()
    {
        return View("~/Views/BikeModels/Create.cshtml");
    }

    // POST: BikeModels/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BikeModel bikeModel)
    {
        if (ModelState.IsValid)
        {
            _context.Add(bikeModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction("SomeOtherAction", "SomeController");
    }

    // GET: BikeModels/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var bike = await _context.BikeModels.FindAsync(id);
        if (bike == null) return NotFound();

        return View(bike);
    }

    // POST: BikeModels/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BikeModel bikeModel)
    {
        if (id != bikeModel.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(bikeModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(bikeModel);
    }

    // GET: BikeModels/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var bike = await _context.BikeModels.FirstOrDefaultAsync(m => m.Id == id);
        if (bike == null) return NotFound();

        return View(bike);
    }

    // POST: BikeModels/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bike = await _context.BikeModels.FindAsync(id);
        if (bike != null)
        {
            _context.BikeModels.Remove(bike);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}