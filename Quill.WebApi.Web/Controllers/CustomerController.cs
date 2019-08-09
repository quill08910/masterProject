using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Quill.WebApi.Service;
using Quill.WebApi.Service.Interfaces;

namespace Quill.WebApi.Web.Controllers
{
    public class CustomerController : Controller
    {
        private ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            this._customerService = customerService;
        }

        public string Index(string phone)
        {
            var customer = this._customerService.Query(phone);
            if (customer != null)
                return JsonConvert.SerializeObject(customer);

            return "data not found";
        }

        public ActionResult Register(string phone)
        {
            var result = this._customerService.Register(phone);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Login(string phone)
        {
            var result = this._customerService.Login(phone);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}