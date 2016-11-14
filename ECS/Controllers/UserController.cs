using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECS.Models;

namespace ECS.Controllers
{

    public class UserController : Controller
    {
        ECSEntities db = new ECSEntities();


        /// <summary>
        /// 使用者預約借用
        /// </summary>
        public ActionResult UserReserve()
        {
            EmpData ED = new EmpData();

            if (Session["SessionEmpID"] == null || Session["SessionRole"] == null)    //當沒有登入，沒SessionEmpID，無權限前往此頁面，return回登入畫面
            {
                ViewBag.Message = false;
                return View("SpaceView");
            }
            if (Session["SessionRole"] != null)
            {
                string role = Session["SessionRole"].ToString();
                if (role != "User")
                {
                    ViewBag.Message = false;
                    return View("SpaceView");
                }
            }
            if (Session["SessionEmpID"] != null)
            {
                string Empid = Session["SessionEmpID"].ToString();
                ED = db.EmpData.Where(x => x.EmpID == Empid).FirstOrDefault();
            }
            return View(ED);
        }

        [HttpPost]
        public JsonResult UserReserve(EmpData E, RecordData R)
        {

            EmpData ED = db.EmpData.Where(x => x.EmpID == E.EmpID).FirstOrDefault();
            ED.EmpState = "已預約";

            db.RecordData.Add(R);
            R.RecordNumber = db.RecordData.Select(x => x.RecordNumber).Max(); //找出編號最大，並加1
            R.RecordNumber++;
            R.Disable = "N";
            R.CardID = "未選取";
            R.RecordState = "已預約";

            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 使用者借用紀錄
        /// </summary>
        public ActionResult UserRecord(String datetimesd, String datetimeed)
        {
            if (Session["SessionEmpID"] == null)    //當沒有登入，沒SessionEmpID，無權限前往此頁面，return回登入畫面
            {
                ViewBag.Message = false;
                return View("SpaceView");
            }
            FormViewModel VMList = new FormViewModel();
            if (Session["SessionEmpID"] != null)
            {
                string Empid = Session["SessionEmpID"].ToString();
                string Empstate = Session["SessionEmpState"].ToString();

                DateTime Timesd = Convert.ToDateTime("0001-01-01");
                DateTime Timeed = Convert.ToDateTime("9999-01-01");
                if (!string.IsNullOrWhiteSpace(datetimesd))
                {
                    Timesd = Convert.ToDateTime(datetimesd);
                }
                if (!string.IsNullOrWhiteSpace(datetimeed))
                {
                    Timeed = Convert.ToDateTime(datetimeed);
                }
                VMList.ECSdata = (from R in db.RecordData
                                  join C in db.CardData on R.CardID equals C.CardID
                                  where R.Disable == "N" && R.EmpID == Empid
                                                         && (!string.IsNullOrEmpty(datetimesd) ? R.TimeLend >= Timesd : true)
                                                         && (!string.IsNullOrEmpty(datetimeed) ? R.TimeLend <= Timeed : true)
                                  orderby R.RecordNumber descending /*降冪排序*/
                                  select new ECSViewModel
                                  {
                                      RecordNumber = R.RecordNumber,
                                      CardID = C.CardID,
                                      CardName = C.CardName,
                                      TimeLend = R.TimeLend,
                                      TimeReturn = R.TimeReturn,
                                      RecordState = R.RecordState
                                  }).ToList();
                VMList.IsSuccess = true;
                if (VMList.ECSdata.Count == 0)
                {
                    VMList.IsSuccess = false;
                    VMList.Msg = "No Data.";
                }
            }
            return View(VMList);
        }

        /// <summary>
        /// 使用者預約修改
        /// </summary>
        public ActionResult UserEdit(int RecordNumber)
        {
            if (Session["SessionEmpID"] == null)    //當沒有登入，沒SessionEmpID，無權限前往此頁面，return回登入畫面
            {
                ViewBag.Message = false;
                return View("SpaceView");
            }
            var VMList = (from R in db.RecordData
                          join E in db.EmpData on R.EmpID equals E.EmpID
                          where R.RecordNumber == RecordNumber
                          select new ECSViewModel
                          {
                              RecordNumber = R.RecordNumber,
                              EmpID = E.EmpID,
                              EmpName = E.EmpName,
                              TimeLend = R.TimeLend,
                              UseDay = R.UseDay
                          }).ToList();
            return PartialView(VMList);
        }

        [HttpPost]
        public JsonResult UserEdit(RecordData DataEdit)
        {
            RecordData R = db.RecordData.Where(x => x.RecordNumber == DataEdit.RecordNumber).FirstOrDefault();
            R.TimeLend = DataEdit.TimeLend;
            R.UseDay = DataEdit.UseDay;
            db.SaveChanges();

            return Json(JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 使用者預約取消
        /// </summary>
        public ActionResult UserCancel(int recordnumber, string empid = "")
        {
            //將預約那筆單號，Disable狀態改為Y，不顯示資料
            var RecordList = db.RecordData.Where(x => x.RecordNumber == recordnumber).ToList().FirstOrDefault();
            RecordList.Disable = "Y";
            RecordList.RecordState = "預約取消";
            empid = RecordList.EmpID;
            //將預約員工狀態，從已預約修改為可借用
            var EmpList = db.EmpData.Where(x => x.EmpID == empid).ToList().FirstOrDefault();
            EmpList.EmpState = "可借用";
            db.SaveChanges();
            return RedirectToAction("UserRecord");
        }

        /// <summary>
        /// 使用者續借
        /// </summary>
        public ActionResult UserRenew(int recordnumber)
        {
            //將預約那筆單號，Disable狀態改為Y，不顯示資料
            var RecordList = db.RecordData.Where(x => x.RecordNumber == recordnumber).ToList().FirstOrDefault();
            RecordList.UseDay = "5";
            db.SaveChanges();
            return RedirectToAction("UserRecord");
        }
    }
}
