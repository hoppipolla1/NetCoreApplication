using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCoreApp_DataAccess;
using NetCoreApp_DataAccess.Repository.IRepository;
using NetCoreApp_Models;
using NetCoreApplication_Utility;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace NetCoreApplication.Controllers
{
    [Authorize(Roles = WebConstants.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _catRepo;

        public CategoryController(ICategoryRepository catRepo)
        {
            _catRepo = catRepo;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> objList = _catRepo.GetAll();
            return View(objList);
        }
        
        //GET - Greate
        public IActionResult Create()
        {
            return View();
        }

        //POST - Greate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Add(category);
                _catRepo.Save();
                TempData[WebConstants.Success] = "Category created Successfully";
                return RedirectToAction("Index");
            }
            TempData[WebConstants.Error] = "Error while creating category";
            return View(category);
        }

        //GET - EDIT
        public IActionResult Edit(int? Id)
        {
            if(Id == null || Id == 0)
                return NotFound();

            var category = _catRepo.Find(Id.GetValueOrDefault());
            if (category == null)
                return NotFound();
            
            return View(category);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Update(category);
                _catRepo.Save();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        //GET - DELETE
        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();

            var category = _catRepo.Find(Id.GetValueOrDefault());
            if (category == null)
                return NotFound();

            return View(category);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? Id)
        {
            var category = _catRepo.Find(Id.GetValueOrDefault());
            if (category == null)
                return NotFound();

            _catRepo.Remove(category);
            _catRepo.Save();
            return RedirectToAction("Index");
        }
    }
}
