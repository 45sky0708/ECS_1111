using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECS.Models;
using PagedList;

namespace ECS.Controllers
{
    public class EmpController : Controller
    {
        ECSEntities db = new ECSEntities();
        /// <summary>
        /// 員工管理畫面，顯示所有管理者
        /// </summary>
        public ActionResult EmpManage(int page = 1, string empid = "",string empsn ="", string empdept = "")
        {
            List<ECSViewModel> EmpList = new List<ECSViewModel>();
            string role = Session["SessionRole"].ToString();
            if (role == "Manage")
            {
                empdept = Session["SessionEmpDept"].ToString();
            }

            //分頁
            int pageSize = 10;
            int currentPage = page < 1 ? 1 : page;

            //搜尋欄位存值
            Session["Searchempsn"] = empsn;
            Session["Searchempdept"] = empdept;

            //if (!string.IsNullOrWhiteSpace(empid) || !string.IsNullOrWhiteSpace(empsn) || !string.IsNullOrWhiteSpace(empdept))
            //{
                EmpList = (from E in db.EmpData
                                   where E.Disable == "N" && (!string.IsNullOrEmpty(empid) ? E.EmpID == empid : true)
                                                          && (!string.IsNullOrEmpty(empsn) ? E.EmpSN == empsn : true)
                                                          && (!string.IsNullOrEmpty(empdept) ? E.EmpDept == empdept : true)
                                                          
                                   select new ECSViewModel
                                   {
                                       EmpID = E.EmpID,
                                       EmpName = E.EmpName,
                                       EmpDept = E.EmpDept,
                                       Email = E.Email,
                                       EmpState = E.EmpState,
                                       EmpSN = E.EmpSN,
                                       Role = E.Role
                                   }).ToList();
                ViewBag.Message = true;
                if (EmpList.Count == 0)
                {
                    ViewBag.Message = false;
                }
            //}
            //else
            //{
            //    EmpList = (from E in db.EmpData
            //                       where E.Disable == "N" && (E.Role == "Admin" || E.Role == "Manage")
            //                       select new ECSViewModel
            //                       {
            //                           EmpID = E.EmpID,
            //                           EmpName = E.EmpName,
            //                           EmpDept = E.EmpDept,
            //                           Email = E.Email,
            //                           EmpState = E.EmpState,
            //                           EmpSN = E.EmpSN,
            //                           Role = E.Role
            //                       }).ToList();
            //    ViewBag.Message = true;
            //}
            return View(EmpList.ToPagedList(currentPage, pageSize)); 
        }

        /// <summary>
        /// 員工新增
        /// </summary>
        public ActionResult EmpCreat()
        {
            return PartialView(db);
        }

        [HttpPost]
        public JsonResult EmpCreat(EmpData DataCreat)
        {

            db.EmpData.Add(DataCreat);
            DataCreat.Disable = "N";
            DataCreat.EmpState = "未借用";
            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 員工修改
        /// </summary>
        public ActionResult EmpEdit(string empid)
        {
            var EmpList = db.EmpData.Where(x => x.EmpID == empid).ToList().FirstOrDefault();
            return PartialView(EmpList);
        }

        [HttpPost]
        public JsonResult EmpEdit(EmpData DataEdit)
        {
            EmpData E = db.EmpData.Where(x => x.EmpID == DataEdit.EmpID).FirstOrDefault();
            E.EmpID = DataEdit.EmpID;
            E.EmpName = DataEdit.EmpName;
            E.EmpDept = DataEdit.EmpDept;
            E.Email = DataEdit.Email;
            E.EmpState = DataEdit.EmpState;
            E.EmpSN = DataEdit.EmpSN;
            E.Role = DataEdit.Role;
            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 員工刪除
        /// </summary>
        public ActionResult EmpDelete(string empid)
        {
            var EmpList = db.EmpData.Where(x => x.EmpID == empid).ToList().FirstOrDefault();
            EmpList.Disable = "Y";
            db.SaveChanges();
            return RedirectToAction("EmpManage");
        }
    }
}
