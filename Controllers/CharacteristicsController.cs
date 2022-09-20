using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NetCoreApp_Models;
using NetCoreApp_Models.ViewModels;
using NetCoreApplication_Utility;
using Rocky_DataAccess.Repository.IRepository;
using Characteristics = NetCoreApp_Models.Characteristics;

namespace Rocky.Controllers
{
    [Authorize(Roles=WebConstants.AdminRole)]
    public class CharacteristicsController : Controller
    {
        private readonly ICharacteristicsRepository _characteristicsRepo;

        public CharacteristicsController(ICharacteristicsRepository characteristicsRepo)
        {
            _characteristicsRepo = characteristicsRepo;
        }
        public IActionResult Index()
        {
            IEnumerable<Characteristics> objList = _characteristicsRepo.GetAll();

            return View(objList);
        }

        //GET - Upsert
        public IActionResult Upsert(int? id)
        {

            CharacteristicsVM Characteristics = new CharacteristicsVM()
            {
                Characteristics = new Characteristics()
            };

            if (id == null)
                return View(Characteristics);
            else
            {
                Characteristics.Characteristics = _characteristicsRepo.Find(id.GetValueOrDefault());
                if (Characteristics.Characteristics == null)
                    return NotFound();
                return View(Characteristics);
            }
        }

        //POST - Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CharacteristicsVM CharacteristicsVM)
        {
            if (ModelState.IsValid)
            {
                if (CharacteristicsVM.Characteristics.Id == 0)
                {
                    _characteristicsRepo.Add(CharacteristicsVM.Characteristics);
                }
                else
                {
                    _characteristicsRepo.Update(CharacteristicsVM.Characteristics);
                }
                _characteristicsRepo.Save();
                return RedirectToAction("Index");
            }

            return View(CharacteristicsVM);
        }

        //GET - DELETE
        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();

            Characteristics characteristic = _characteristicsRepo.FirstOrDefault(x => x.Id == Id);

            if (characteristic == null)
                return NotFound();

            return View(characteristic);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? Id)
        {
            var characteristic = _characteristicsRepo.Find(Id.GetValueOrDefault());
            if (characteristic == null)
                return NotFound();

            _characteristicsRepo.Remove(characteristic);
            _characteristicsRepo.Save();
            return RedirectToAction("Index");
        }

    }
}
