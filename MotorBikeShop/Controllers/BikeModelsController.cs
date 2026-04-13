using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Data;
using MotorBikeShop.Models;
using MotorBikeShop.Services;

public class BikeModelsController : Controller
{
    private readonly ShopService _shopService;

    public BikeModelsController(ShopService shopService)
    {
        _shopService = shopService;
    }

    // GET: BikeModels
    public async Task<IActionResult> Index()
    {
        var bikes = await _shopService.GetBikeModelIndex();
        return View(bikes);
    }

    // GET: BikeModels/Details/5
    public async Task<IActionResult> Details(int id)
    {
        if (id == null) return NotFound();

        var bike = await _shopService.GetBikeModelDetails(id);

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
    public async Task<IActionResult> Create(BikeViewModel bikeModel)
    {
        if (ModelState.IsValid)
        {
            await _shopService.UpdateBike(bikeModel);
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction("SomeOtherAction", "SomeController");
    }

    // GET: BikeModels/Edit/5
    public async Task<IActionResult> Edit(int id)
    {

        var bike = await _shopService.GetBike(id);
        if (bike == null) return NotFound();

        return View(bike);
    }

    // POST: BikeModels/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BikeViewModel bikeModel)
    {
        if (id != bikeModel.Id) return NotFound();

        if (ModelState.IsValid)
        {
            await _shopService.UpdateBike(bikeModel);
            return RedirectToAction(nameof(Index));
        }
        return View(bikeModel);
    }

    // GET: BikeModels/Delete/5
    public async Task<IActionResult> Delete(int id)
    {

        var bike = await _shopService.GetBike(id);
        if (bike == null) return NotFound();

        return View(bike);
    }

    // POST: BikeModels/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bike = await _shopService.DeleteBike(id);

        return RedirectToAction(nameof(Index));
    }
}