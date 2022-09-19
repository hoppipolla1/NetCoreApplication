using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using NetCore_DataAccess.Repository.IRepository;
using NetCoreApp_DataAccess;
using NetCoreApp_DataAccess.Repository;
using NetCoreApp_DataAccess.Repository.IRepository;
using NetCoreApp_Models;
using NetCoreApp_Models.ViewModels;
using NetCoreApplication_Utility;
using Rocky_DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreApplication.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _prodRepo;
        private readonly IApplicationUserRepository _userRepo;
        private readonly IOrderHeaderRepository _orderHRepo;
        private readonly IOrderDetailRepository _orderDRepo;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(IProductRepository prodRepo, IApplicationUserRepository userRepo,
            IOrderHeaderRepository orderHRepo, IOrderDetailRepository orderDRepo)
        {
            _prodRepo = prodRepo;
            _userRepo = userRepo;
            _orderHRepo = orderHRepo;
            _orderDRepo = orderDRepo;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> prodList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in prodList)
            {
                shoppingCartList.Add(new ShoppingCart() { ProductId = prod.Id, Qt = prod.TempQt });
            }

            HttpContext.Session.Set(WebConstants.sessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<ShoppingCart> shoppingCartList = new();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.sessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.sessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.sessionCart);
            }

            List<int> ProdInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            IList<Product> prodList = _prodRepo.GetAll(x => ProdInCart.Contains(x.Id)).ToList();

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = _userRepo.FirstOrDefault(x => x.Id == claim.Value),
            };

            foreach(var cartItem in shoppingCartList)
            {
                Product prodTemp = _prodRepo.FirstOrDefault(x => x.Id == cartItem.ProductId);
                prodTemp.TempQt = cartItem.Qt;
                ProductUserVM.ProductList.Add(prodTemp);
            }

            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(ProductUserVM ProductUserVM)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderHeader orderHeader = new OrderHeader()
            {
                CreatedByUserId = claim.Value,
                FinalOrderTotal = ProductUserVM.ProductList.Sum(x => x.TempQt * x.Price),
                Address = ProductUserVM.ApplicationUser.Address,
                FullName = ProductUserVM.ApplicationUser.FullName,
                Email = ProductUserVM.ApplicationUser.Email,
                PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                OrderDate = DateTime.Now,
                OrderStatus = WebConstants.OrderStatusInProcess
            };
            _orderHRepo.Add(orderHeader);
            _orderHRepo.Save();

            foreach (var prod in ProductUserVM.ProductList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderHeaderId = orderHeader.Id,
                    Price = prod.Price,
                    Qt = prod.TempQt,
                    ProductId = prod.Id
                };
                _orderDRepo.Add(orderDetail);

            }
            _orderDRepo.Save();

            TempData[WebConstants.Success] = "Order submitted successfully";

            return RedirectToAction(nameof(InquiryConfirmation), new {id=orderHeader.Id});
        }

        public IActionResult InquiryConfirmation(int id = 0)
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);
        }

        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.sessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.sessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.sessionCart);
            }

            List<int> ProdInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            IEnumerable<Product> prodListTemp = _prodRepo.GetAll(x => ProdInCart.Contains(x.Id));
            IList<Product> prodList = new List<Product>();

            foreach (var shopItem in shoppingCartList)
            {
                Product tempProduct = prodListTemp.FirstOrDefault(x => x.Id == shopItem.ProductId);
                tempProduct.TempQt = shopItem.Qt ;
                prodList.Add(tempProduct);
            }

            return View(prodList);
        }

        public IActionResult Remove(int? id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.sessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstants.sessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.sessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(x => x.ProductId == id));

            HttpContext.Session.Set(WebConstants.sessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> prodList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach(Product prod in prodList)
            {
                shoppingCartList.Add( new ShoppingCart() { ProductId = prod.Id, Qt = prod.TempQt });
            }

            HttpContext.Session.Set(WebConstants.sessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}
