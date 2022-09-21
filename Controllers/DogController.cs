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
    public class DogController : Controller
    {
        private readonly IDogRepository _dogRepo;
        private readonly ICharacteristicsRepository _characteristicsRepo;
        private readonly IDogCharacteristicsRepository _dogCharacteristicsRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DogController(IDogRepository dogRepo, 
            ICharacteristicsRepository characteristicsRepo, 
            IWebHostEnvironment webHostEnvironment, 
            IDogCharacteristicsRepository dogCharacteristicsRepo)
        {
            _dogRepo = dogRepo;
            _characteristicsRepo = characteristicsRepo;
            _webHostEnvironment = webHostEnvironment;
            _dogCharacteristicsRepo = dogCharacteristicsRepo;
        }

        public IActionResult Index()
        {
            IEnumerable<Dog> objList = _dogRepo.GetAll();

            return View(objList);
        }
        
        //GET - Upsert
        public IActionResult Upsert(int? id)
        {

            DogVM dogVM = new DogVM()
            {
                Dog = new Dog()
            };

            if (id == null)
                return View(dogVM);
            else
            {
                dogVM.Dog = _dogRepo.Find(id.GetValueOrDefault());
                if (dogVM.Dog == null)
                    return NotFound();
                return View(dogVM);
            }
        }

        //POST - Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(DogVM dogVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if (dogVM.Dog.Id == 0)
                {
                    //create
                    string upload = webRootPath + WebConstants.imagePath;
                    string filename = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    dogVM.Dog.Image = filename + extension;
                    _dogRepo.Add(dogVM.Dog);
                }
                else
                {
                    var objFromDb = _dogRepo.FirstOrDefault(x => x.Id == dogVM.Dog.Id, isTracking: false);

                    if (files.Count > 0)
                    {
                        string upload = webRootPath + WebConstants.imagePath;
                        string filename = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var oldFile = Path.Combine(upload, objFromDb.Image);

                        if (System.IO.File.Exists(oldFile))
                            System.IO.File.Delete(oldFile);

                        using (var fileStream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        dogVM.Dog.Image = filename + extension;
                    }
                    else
                    {
                        dogVM.Dog.Image = objFromDb.Image;
                    }
                    _dogRepo.Update(dogVM.Dog);
                }
                _dogRepo.Save();
                return RedirectToAction("Index");
            }

            //dogVM.CategorySelectList = _prodRepo.GetAllDropdownList(WebConstants.CategoryName);
            //dogVM.ApplicationTypeSelectList = _prodRepo.GetAllDropdownList(WebConstants.ApplicationTypeName);

            return View(dogVM);
        }

        //GET - DELETE
        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
                return NotFound();

            Dog dog = _dogRepo.FirstOrDefault(x => x.Id == Id);

            if (dog == null)
                return NotFound();

            return View(dog);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? Id)
        {
            var dog = _dogRepo.Find(Id.GetValueOrDefault());
            if (dog == null)
                return NotFound();

            string upload = _webHostEnvironment.WebRootPath + WebConstants.imagePath;

            var oldFile = Path.Combine(upload, dog.Image);

            if (System.IO.File.Exists(oldFile))
                System.IO.File.Delete(oldFile);


            _dogRepo.Remove(dog);
            _dogRepo.Save();
            return RedirectToAction("Index");
        }

    }
}
