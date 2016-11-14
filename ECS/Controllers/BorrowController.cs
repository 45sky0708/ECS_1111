using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECS.Models;

namespace ECS.Controllers
{
    public class BorrowController : Controller
    {
        ECSEntities db = new ECSEntities();

        /// <summary>
        /// 借還管理畫面，顯示所有預約資料
        /// </summary>
        public ActionResult Reserve()
        {
            return View();
        }

        public ActionResult ReserveShowdata(string empsn = "", string empdept = "")
        {
            string role = Session["SessionRole"].ToString();
            if (role == "Manage")
            {
                empdept = Session["SessionEmpDept"].ToString();
            }
            FormViewModel VMList = new FormViewModel();

            VMList.ECSdata = (from R in db.RecordData
                              join E in db.EmpData on R.EmpID equals E.EmpID
                              where R.RecordState == "已預約" && R.Disable == "N"
                                                              && (!string.IsNullOrEmpty(empdept) ? E.EmpDept == empdept : true)
                                                              && (!string.IsNullOrEmpty(empsn) ? E.EmpSN == empsn : true)
                              orderby R.RecordNumber descending /*降冪排序*/
                              select new ECSViewModel
                              {
                                  RecordNumber = R.RecordNumber,
                                  EmpID = E.EmpID,
                                  EmpSN = E.EmpSN,
                                  EmpDept = E.EmpDept,
                                  EmpName = E.EmpName,
                                  TimeLend = R.TimeLend,
                                  UseDay = R.UseDay
                              }).ToList();
            VMList.IsSuccess = true;
            if (VMList.ECSdata.Count == 0)
            {
                VMList.IsSuccess = false;
                VMList.Msg = "No Data.";
            }
            return PartialView(VMList);
        }

        /// <summary>
        /// 預約確認/修改
        /// </summary>
        public ActionResult ReserveConfirm(int recordnumber)
        {
            var ConfirmList = (from R in db.RecordData
                               join E in db.EmpData on R.EmpID equals E.EmpID
                               where R.RecordNumber == recordnumber && R.RecordState == "已預約"
                               select new ECSViewModel
                               {
                                   EmpID = E.EmpID,
                                   EmpDept = E.EmpDept,
                                   EmpName = E.EmpName,
                                   TimeLend = R.TimeLend,
                                   UseDay = R.UseDay,
                                   RecordNumber = R.RecordNumber
                               }).ToList();
            return PartialView(ConfirmList);
        }

        [HttpPost]
        public JsonResult ReserveConfirm(EmpData EData, CardData CData, RecordData RData, string mistake = "")
        {
            CardData C = db.CardData.Where(x => x.CardID == CData.CardID).FirstOrDefault();
            EmpData E = db.EmpData.Where(x => x.EmpID == EData.EmpID).FirstOrDefault();
            //判斷卡片是否跟借用員工同部門
            if (E.EmpDept != C.CardDept)
            {
                mistake = "Case01";
                return Json(mistake);
            }
            //判斷卡片狀態
            if (C.CardState == "已借用")
            {
                mistake = "Case02";
                return Json(mistake);
            }
            E.EmpState = "已借用";
            C.CardState = "已借用";
            RecordData R = db.RecordData.Where(x => x.RecordNumber == RData.RecordNumber).FirstOrDefault();
            R.CardID = C.CardID;
            R.TimeLend = RData.TimeLend;
            R.UseDay = RData.UseDay;
            R.RecordState = "已借用";

            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 預約取消
        /// </summary>
        public ActionResult BorrowCancel(int recordnumber, string empid = "")
        {
            //將預約那筆單號，Disable狀態改為Y，不顯示資料
            var RecordList = db.RecordData.Where(x => x.RecordNumber == recordnumber).ToList().FirstOrDefault();
            RecordList.Disable = "Y";
            empid = RecordList.EmpID;
            //將預約員工狀態，從已預約修改為可借用
            var EmpList = db.EmpData.Where(x => x.EmpID == empid).ToList().FirstOrDefault();
            EmpList.EmpState = "可使用";
            db.SaveChanges();
            return RedirectToAction("Reserve");
        }

        /// <summary>
        /// 現場借用
        /// </summary>
        public ActionResult BorrowCreat(ECSViewModel ECS)
        {
            ECS.TimeLend = DateTime.Now;
            return View(ECS);
        }

        public ActionResult EmpSNsearch(ECSViewModel ECS, string empsn = "")
        {
            EmpData ED = db.EmpData.Where(x => x.EmpSN == empsn).FirstOrDefault();
            ECS.EmpID = ED.EmpID;
            ECS.EmpName = ED.EmpName;
            return Json(ECS, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult BorrowCreat(ECSViewModel ECS, EmpData EData, CardData CData, RecordData RData, string mistake ="", string empsn = "")
        {
            //將輸入empsn的員工資料，搜尋出來
            if (!string.IsNullOrWhiteSpace(empsn))
            {
                EmpData ED = db.EmpData.Where(x => x.EmpSN == empsn).FirstOrDefault();
                ECS.EmpID = ED.EmpID;
                ECS.EmpName = ED.EmpName;
                return Json(ECS, JsonRequestBehavior.AllowGet);
            }
            EmpData E = db.EmpData.Where(x => x.EmpID == EData.EmpID).FirstOrDefault();
            CardData C = db.CardData.Where(x => x.CardID == CData.CardID).FirstOrDefault();
            //判斷此員工是否為預約與借用狀態
            if (E.EmpState == "已預約" || E.EmpState == "已借用")
            {
                mistake = "Case01";
                return Json(mistake);
            }
            //判斷卡片是否跟借用員工同部門
            if (E.EmpDept != C.CardDept)
            {
                mistake = "Case02";
                return Json(mistake);
            }
            //判斷卡片狀態
            if (C.CardState == "已借用")
            {
                mistake = "Case03";
                return Json(mistake);
            }
            
            E.EmpState = "已借用";            
            C.CardState = "已借用";

            db.RecordData.Add(RData);
            RData.RecordNumber = db.RecordData.Select(x => x.RecordNumber).Max(); //找出編號最大，並加1
            RData.RecordNumber++;
            RData.CardID = C.CardID;
            RData.Disable = "N";
            RData.RecordState = "已借用";

            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 卡片歸還畫面，顯示所有借用資料 
        /// </summary>
        public ActionResult Return()
        {
            return View();
        }

        public ActionResult ReturnShowdata(string cardid = "", string empdept = "")
        {
            FormViewModel ReturnList = new FormViewModel();
            string role = Session["SessionRole"].ToString();
            if (role == "Manage")
            {
                empdept = Session["SessionEmpDept"].ToString();
            }            
            ReturnList.ECSdata = (from R in db.RecordData
                                  join E in db.EmpData on R.EmpID equals E.EmpID
                                  join C in db.CardData on R.CardID equals C.CardID
                                  where R.RecordState == "已借用" && (!string.IsNullOrEmpty(cardid) ? R.CardID == cardid : true)
                                                                  && (!string.IsNullOrEmpty(empdept) ? E.EmpDept == empdept : true)
                                  orderby R.RecordNumber descending /*降冪排序*/
                                  select new ECSViewModel
                                  {
                                      CardName = C.CardName,
                                      CardID = C.CardID,
                                      EmpID = E.EmpID,
                                      EmpDept = E.EmpDept,
                                      EmpName = E.EmpName,
                                      TimeLend = R.TimeLend,
                                      UseDay = R.UseDay,
                                      RecordNumber = R.RecordNumber
                                  }).ToList();
            ReturnList.IsSuccess = true;
            if (ReturnList.ECSdata.Count == 0)
            {
                ReturnList.IsSuccess = false;
                ReturnList.Msg = "No Data.";
            }
            return PartialView(ReturnList);
        }

        /// <summary>
        /// 卡片歸還 
        /// </summary>
        public ActionResult ReturnConfirm(int recordnumber)
        {
            var ConfirmList = (from R in db.RecordData
                               join E in db.EmpData on R.EmpID equals E.EmpID
                               join C in db.CardData on R.CardID equals C.CardID
                               where R.RecordNumber == recordnumber
                               select new ECSViewModel
                               {
                                   CardName = C.CardName,
                                   EmpID = E.EmpID,
                                   EmpDept = E.EmpDept,
                                   EmpName = E.EmpName,
                                   TimeLend = R.TimeLend,
                                   RecordNumber = R.RecordNumber
                               }).ToList();
            return PartialView(ConfirmList);
        }

        [HttpPost]
        public JsonResult ReturnConfirm(EmpData EData, CardData CData, RecordData RData)
        {
            EmpData E = db.EmpData.Where(x => x.EmpID == EData.EmpID).FirstOrDefault();
            E.EmpState = "未借用";
            RecordData R = db.RecordData.Where(x => x.RecordNumber == RData.RecordNumber).FirstOrDefault();
            R.TimeReturn = DateTime.Now; //////取得按鈕後時間
            R.RecordState = "已歸還";
            CardData C = db.CardData.Where(x => x.CardName == CData.CardName).FirstOrDefault();
            C.CardState = "可使用";
            R.TotalSpent = CData.CardAmount - C.CardAmount;
            if (CData.CardAmount != null)
            {
                C.CardAmount = C.CardAmount - CData.CardAmount;
            }            
            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }
    }
}
