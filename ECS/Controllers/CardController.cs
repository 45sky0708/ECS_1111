using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECS.Models;
using PagedList;

namespace ECS.Controllers
{
    public class CardController : Controller
    {
        ECSEntities db = new ECSEntities();

        /// <summary>
        /// 卡片管理畫面，顯示所有卡片最新一筆資料
        /// </summary>
        public ActionResult CardManage(string cardid, int page = 1, string empdept = "")
        {
            //FormViewModel EmpList = new FormViewModel();
            string role = Session["SessionRole"].ToString();
            if (role == "Manage")
            {
                empdept = Session["SessionEmpDept"].ToString();
            }

            //分頁
            int pageSize = 10;
            int currentPage = page < 1 ? 1 : page;

            //搜尋欄位存值
            Session["Searchcardid"] = cardid;
            Session["Searchempdept"] = empdept;

            var CardList = (from C in db.CardData
                            where C.Disable == "N" && (!string.IsNullOrEmpty(cardid) ? C.CardID == cardid : true)
                                                   && (!string.IsNullOrEmpty(empdept) ? C.CardDept == empdept : true)                            
                            orderby C.CardID descending
                            orderby C.CardState == "可使用" descending
                            select new ECSViewModel
                            {
                                CardID = C.CardID,
                                CardDept = C.CardDept,
                                CardName = C.CardName,
                                CardState = C.CardState,
                                CardAmount = C.CardAmount
                            }).ToList();
            ViewBag.Message = true;
            if (CardList.Count == 0)
            {
                ViewBag.Message = false;
            }

            return View(CardList.ToPagedList(currentPage, pageSize));
        }

        /// <summary>
        /// 卡片新增
        /// </summary>
        public ActionResult CardCreat()
        {
            return PartialView(db);
        }

        [HttpPost]
        public JsonResult CardCreat(CardData DataCreat)
        {

            db.CardData.Add(DataCreat);
            DataCreat.Disable = "N";
            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 卡片修改
        /// </summary>
        public ActionResult CardEdit(string cardid)
        {
            //DropDownList(HtmlHelper, String, IEnumerable<SelectListItem>)
            //使用 Model
            var CardList = db.CardData.Where(x => x.CardID == cardid).ToList().FirstOrDefault();

            var categories = (from C in db.CardData
                              where C.CardDept != "未選取"
                              select C.CardDept).Distinct().ToList();
            var categories2 = (from C in db.CardData
                               where C.CardState != "已預約"
                               select C.CardState).Distinct().ToList();

            List<SelectListItem> countryList = new List<SelectListItem>();
            List<SelectListItem> countryList2 = new List<SelectListItem>();
            //卡片部門下拉選單
            foreach (var item in categories)
            {
                countryList.Add(new SelectListItem()
                {
                    Text = item,
                    Value = item,
                    Selected = item.Equals(CardList.CardDept.ToString())
                });
            }
            ViewBag.CardDept = countryList;
            foreach (var item in categories2)
            {
                countryList2.Add(new SelectListItem()
                {
                    Text = item,
                    Value = item,
                    Selected = item.Equals(CardList.CardState.ToString())
                });
            }
            ViewBag.CardState = countryList2;


            return PartialView(CardList);
        }


        [HttpPost]
        public JsonResult CardEdit(CardData DataEdit)
        {
            CardData C = db.CardData.Where(x => x.CardID == DataEdit.CardID).FirstOrDefault();
            C.CardID = DataEdit.CardID;
            C.CardDept = DataEdit.CardDept;
            C.CardName = DataEdit.CardName;
            C.CardState = DataEdit.CardState;
            C.CardAmount = DataEdit.CardAmount;
            db.SaveChanges();

            return Json(JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 卡片刪除
        /// </summary>
        public ActionResult CardDelete(string cardid)
        {
            var CardList = db.CardData.Where(x => x.CardID == cardid).ToList().FirstOrDefault();
            CardList.Disable = "Y";
            db.SaveChanges();
            return RedirectToAction("CardManage");
        }

        /// <summary>
        /// 卡片儲值
        /// </summary>
        public ActionResult CardGift()
        {
            return PartialView(db);
        }
        public ActionResult CardIDsearch(ECSViewModel ECS, string CardID = "")
        {
            CardData C = db.CardData.Where(x => x.CardID == CardID).FirstOrDefault();
            if (C == null)
            {
                ECS.CardName = "不存在";
                ECS.CardDept = "不存在";
                ECS.CardAmount = 0;
            }
            else
            {
                ECS.CardName = C.CardName;
                ECS.CardDept = C.CardDept;
                ECS.CardAmount = C.CardAmount;
            }
            return Json(ECS, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CardGift(CardData CData, RecordData RData, string CardID = "")
        {
            string Empid = Session["SessionEmpID"].ToString();

            CardData C = db.CardData.Where(x => x.CardID == CardID).FirstOrDefault();
            C.CardAmount = C.CardAmount + RData.TotalSpent;
            db.RecordData.Add(RData);
            RData.RecordNumber = db.RecordData.Select(x => x.RecordNumber).Max(); //找出編號最大，並加1
            RData.RecordNumber++;
            RData.EmpID = Empid;
            RData.TimeLend = DateTime.Now;
            RData.CardID = C.CardID;
            RData.Disable = "N";
            RData.RecordState = "儲值";
            RData.UseDay = "0";
            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }
    }
}
