using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECS.Models;

namespace ECS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Login(string EmpID = "", string EmpPwd = "")
        {
            ECSEntities db = new ECSEntities();
            if (!string.IsNullOrWhiteSpace(EmpID))
            {
                
                var ED = db.EmpData.Where(x => x.EmpID == EmpID).FirstOrDefault();
                if (ED != null)
                {
                    if (EmpID == ED.EmpID && EmpPwd == ED.EmpPwd)
                    {
                        Session["SessionEmpID"] = ED.EmpID;  
                        Session["SessionEmpPwd"] = ED.EmpPwd;
                        Session["SessionEmpName"] = ED.EmpName;
                        Session["SessionEmpDept"] = ED.EmpDept;
                        Session["SessionEmpState"] = ED.EmpState;
                        Session["SessionRole"] = ED.Role;
                        if (ED.Role == "Admin" || ED.Role == "Manage")
                        {
                            return RedirectToAction("Reserve", "Borrow");
                        }
                        else if (ED.Role == "User")
                        {
                            return RedirectToAction("UserReserve", "User");
                        }
                    }
                    else
                    {
                        ViewBag.Message = false;
                    }
                }
                else
                {
                    ViewBag.Message = false;
                }
            }
            return View();

        }
        public ActionResult Logout()
        {
            Session.RemoveAll();
            return RedirectToAction("Login");
        }
    }
}