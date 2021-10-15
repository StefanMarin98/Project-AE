using AE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AE.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(Product product)
        {
            var db = new Database1Entities();
            HttpPostedFileBase photo = Request.Files["ImageData"];

            if (photo != null)
            {
                product.Photo = new byte[photo.ContentLength];
                photo.InputStream.Read(product.Photo, 0, photo.ContentLength);
            }
            db.Products.Add(product);
            db.SaveChanges();
            return RedirectToAction("Add","Product");
        }
    }
}