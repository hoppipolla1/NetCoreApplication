using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NetCoreApp_Models;
using NetCoreApp_Models.ViewModels;
using NetCoreApplication_Utility;
using Rocky_DataAccess.Repository.IRepository;

namespace Rocky.Controllers
{
    //[Authorize(Roles=WebConstants.AdminRole)]
    public class DogCharacteristicsController : Controller
    {
        private readonly IDogCharacteristicsRepository _dogCharacteristicsRepo;

        public DogCharacteristicsController(IDogCharacteristicsRepository dogCharacteristicsRepo)
        {
            _dogCharacteristicsRepo = dogCharacteristicsRepo;
        }

        public IActionResult Index()
        {
            IEnumerable<DogCharacteristics> objList = _dogCharacteristicsRepo.GetAll(includeProperties: "Dog,Characteristics");

            return View(objList);
        }
        
        //GET - Upsert
        public IActionResult Upsert(int? id)
        {

            DogCharacteristicsVM dogCharacteristicsVM = new DogCharacteristicsVM()
            {
                DogCharacteristics = new DogCharacteristics(),
                DogSelectList = _dogCharacteristicsRepo.GetAllDropdownList(WebConstants.DogName),
                CharacteristicsSelectList = _dogCharacteristicsRepo.GetAllDropdownList(WebConstants.CharacteristicsName),
            };

            if (id == null)
                return View(dogCharacteristicsVM);
            else
            {
                dogCharacteristicsVM.DogCharacteristics = _dogCharacteristicsRepo.Find(id.GetValueOrDefault());
                if (dogCharacteristicsVM.DogCharacteristics == null)
                    return NotFound();
                return View(dogCharacteristicsVM);
            }
        }

        //POST - Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(DogCharacteristicsVM dogCharacteristicsVM)
        {
            if (ModelState.IsValid)
            {
                if (dogCharacteristicsVM.DogCharacteristics.Id == 0)
                {
                    _dogCharacteristicsRepo.Add(dogCharacteristicsVM.DogCharacteristics);
                }
                else
                {
                    _dogCharacteristicsRepo.Update(dogCharacteristicsVM.DogCharacteristics);
                }
                _dogCharacteristicsRepo.Save();
                return RedirectToAction("Index");
            }

            dogCharacteristicsVM.DogSelectList = _dogCharacteristicsRepo.GetAllDropdownList(WebConstants.DogName);
            dogCharacteristicsVM.CharacteristicsSelectList = _dogCharacteristicsRepo.GetAllDropdownList(WebConstants.CharacteristicsName);

            return View(dogCharacteristicsVM);
        }

        //GET - DELETE
        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();

            DogCharacteristics dogCharacteristics = _dogCharacteristicsRepo.FirstOrDefault(x => x.Id == Id, includeProperties: "Dog, Characteristics");

            if (dogCharacteristics == null)
                return NotFound();

            return View(dogCharacteristics);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? Id)
        {
            var dogCharacteristics = _dogCharacteristicsRepo.Find(Id.GetValueOrDefault());
            if (dogCharacteristics == null)
                return NotFound();

            _dogCharacteristicsRepo.Remove(dogCharacteristics);
            _dogCharacteristicsRepo.Save();
            return RedirectToAction("Index");
        }

    }
}
