using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace api.fefarm.mx.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index(int error = 0)
        {
            ViewBag.error = error;

            switch (error)
            {
                case 400:
                    ViewBag.Title = "Bad Request";
                    ViewBag.Description = "The server cannot or will not process the request due to an apparent client error";
                    break;
                case 403:
                    ViewBag.Title = "Forbidden";
                    ViewBag.Description = "The request was valid, but the server is refusing action";
                    break;
                case 404:
                default:
                    ViewBag.Title = "Not Found";
                    ViewBag.Description = "The requested resource could not be found but may be available in the future";
                    break;
                case 500:
                    ViewBag.Title = "Internal Server Error";
                    ViewBag.Description = "A 500 error occurs when something blows up with our code";
                    break;
            }

            return View();
        }
    }
}