using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VMS.Library;
using VMS.Web.Models;
using System.Runtime;

namespace VMS.Web.Controllers
{
    public class HomeController : Controller
    {
        UserAction userAct = new UserAction();
        // GET: Home
        public ActionResult Index()
        {
            if (Sessions.GetUseID() != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginViewModel model)
        {
            var result = userAct.GetDetailUser(model);
            if (result.Success)
            {
                var _data = result.data;
                TempData["SUCCESSLOGIN"] = "Success Access!- Login - " + DateTime.Now.ToString() + "";
                Session["UseID"] = _data.UseID;
                Session["UseNam"] = _data.UseNam;
                Session["UseDep"] = _data.UseDep;
                Session["BusFunc"] = _data.BusFunc;
                Session["UseLev"] = _data.UseLev;
                Session["UsePlant"] = _data.UsePlant;
                Session["ApprovalGroup"] = _data.ApprovalGroup;
                Session["isAuthoriseSeeNumber"] = _data.isAuthoriseSeeNumber;
                Session["MenuList"] = userAct.GetMenuList(_data.BusFunc);

                //Cookies.PostCookies("UseID", _data.UseID);
                //Cookies.PostCookies("UseNam", _data.UseNam);
                //Cookies.PostCookies("UseDep", _data.UseDep);
                //Cookies.PostCookies("BusFunc", _data.BusFunc);
                //Cookies.PostCookies("UseLev", _data.UseLev);
                //Cookies.PostCookies("UsePlant", _data.UsePlant);

                var _page = _data.StartPage.Split('/');
                return RedirectToAction(_page[1], _page[0]);
            }
            else
            {
                TempData["ERROR"] = "Unauthorized Access!- Login - " + DateTime.Now.ToString() + "";
                return View();
            }
            //if (model.UseID == null)
            //{
            //    TempData["ERROR"] = "Please Fill the Username!";
            //    return View();
            //}
            //else if (model.UsePass == null)
            //{
            //    TempData["ERROR"] = "Please Fill the Password!";
            //    return View();
            //}
            //if (model.isWinAuth == true)
            //{
            //    var _data = userAct.GetLoginWinAuth(model.UseID, model.UsePass);
            //    if (_data != null)
            //    {
            //        TempData["SUCCESSLOGIN"] = "Success Access!- Login - " + DateTime.Now.ToString() + "";
            //        Session["UseID"] = _data.UseID;
            //        Session["UseNam"] = _data.UseNam;
            //        Session["UseDep"] = _data.UseDep;
            //        Session["BusFunc"] = _data.BusFunc;
            //        Session["UseLev"] = _data.UseLev;
            //        Session["UsePlant"] = _data.UsePlant;
            //        Session["MenuList"] = userAct.GetMenuList(_data.BusFunc);
            //        var _page = _data.StartPage.Split('/');
            //        return RedirectToAction(_page[1], _page[0]);
            //    }
            //    else
            //    {
            //        TempData["ERROR"] = "Unauthorized Access!- Login - " + DateTime.Now.ToString() + "";
            //        return View();
            //    }

            //}
            //else
            //{

            //    var _data = userAct.GetLoginCommon(model.UseID, model.UsePass);
            //    if (_data != null)
            //    {
            //        TempData["SUCCESSLOGIN"] = "Success Access!- Login - " + DateTime.Now.ToString() + "";
            //        Session["UseID"] = _data.UseID;
            //        Session["UseNam"] = _data.UseNam;
            //        Session["UseDep"] = _data.UseDep;
            //        Session["BusFunc"] = _data.BusFunc;
            //        Session["UseLev"] = _data.UseLev;
            //        Session["UsePlant"] = _data.UsePlant;
            //        Session["isDelegate"] = _data.isDelegate;
            //        Session["MenuList"] = userAct.GetMenuList(_data.BusFunc);
            //        return RedirectToAction("Index", "Dashboard");
            //    }
            //    else
            //    {
            //        TempData["ERROR"] = "Unauthorized Access!- Login - " + DateTime.Now.ToString() + "";
            //        return View();
            //    }
            //}

        }
        public PartialViewResult GetMenuForUser()
        {
            var model = Session["MenuList"] as List<MenulistDto>;

            return PartialView("_Sidebar", model);
        }
        public JsonResult RemoveCookies(string Name)
        {
            try
            {
                Cookies.DeleteCookies(Name);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Logout()
        {
            Session.RemoveAll();
            return RedirectToAction("Index", "Home");
        }
    }

}