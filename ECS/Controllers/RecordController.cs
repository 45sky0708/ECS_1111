using System;
using System.Linq;
using System.Web.Mvc;
using ECS.Models;
using System.IO;
using System.Web.UI.WebControls;
using PagedList;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ECS.Infrastructure.CustomResults;

using System.Web;
using System.Web.Configuration;

namespace ECS.Controllers
{
    public class RecordController : Controller
    {
        ECSEntities db = new ECSEntities();
        public ActionResult Record(int page = 1, String datetimesd = "", String datetimeed = "", string empid = "", string empsn = "", string empdept = "", string cardid = "")
        {
            ECSEntities db = new ECSEntities();
            //分頁
            int pageSize = 10;
            int currentPage = page < 1 ? 1 : page;

            //搜尋欄位存值
            Session["Searchdatetimesd"] = datetimesd;
            Session["Searchdatetimeed"] = datetimeed;
            Session["Searchempid"] = empid;
            Session["Searchempsn"] = empsn;
            Session["Searchempdept"] = empdept;
            Session["Searchcardid"] = cardid;

            FormViewModel RecordList = new FormViewModel();
            //Timesd & Timeed 初始值
            DateTime Timesd = Convert.ToDateTime("0001-01-01");
            DateTime Timeed = Convert.ToDateTime("9999-01-01");
            //當月時間
            string First = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).ToShortDateString();
            string Last = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).ToShortDateString();
            DateTime FirstDay = Convert.ToDateTime(First);
            DateTime LastDay = Convert.ToDateTime(Last);
            //Timesd & Timeed 使用者輸入值
            if (!string.IsNullOrWhiteSpace(datetimesd))
            {
                Timesd = Convert.ToDateTime(datetimesd);
            }
            if (!string.IsNullOrWhiteSpace(datetimeed))
            {
                Timeed = Convert.ToDateTime(datetimeed);
            }
            //判斷管理者部門
            string role = Session["SessionRole"].ToString();
            if (role == "Manage")
            {
                empdept = Session["SessionEmpDept"].ToString();
            }
            //Search當月的資料
            if (string.IsNullOrWhiteSpace(empid) && string.IsNullOrWhiteSpace(empsn) && string.IsNullOrWhiteSpace(empdept) && string.IsNullOrWhiteSpace(cardid) && string.IsNullOrWhiteSpace(datetimesd) && string.IsNullOrWhiteSpace(datetimeed))
            {
                RecordList.ExportData = (from R in db.RecordData                            
                                      join E in db.EmpData on R.EmpID equals E.EmpID
                                      join C in db.CardData on R.CardID equals C.CardID
                                      where R.Disable == "N" /*&& R.TimeLend >= FirstDay && R.TimeLend < LastDay*/
                                                             && (!string.IsNullOrEmpty(empdept) ? E.EmpDept == empdept : true)
                                      orderby R.RecordNumber descending /*降冪排序*/
                                      select new ExportDataViewModel
                                      {
                                          RecordNumber = R.RecordNumber,
                                          EmpID = E.EmpID,
                                          EmpName = E.EmpName,
                                          EmpDept = E.EmpDept,
                                          CardID = C.CardID,
                                          CardName = C.CardName,
                                          CardDept = C.CardDept,
                                          TimeLend = R.TimeLend,
                                          TimeReturn = R.TimeReturn,
                                          RecordState = R.RecordState,
                                          UseDay = R.UseDay,
                                          CardAmount = C.CardAmount,
                                          TotalSpent = R.TotalSpent
                                      }).ToList();
                ViewBag.Message = true;
                Session["SessionECSdata"] = RecordList.ExportData;
            }
            //Search輸入條件的所有資料
            else
            {
                RecordList.ExportData = (from R in db.RecordData                            
                                      join E in db.EmpData on R.EmpID equals E.EmpID
                                      join C in db.CardData on R.CardID equals C.CardID
                                      where R.Disable == "N" && (!string.IsNullOrEmpty(empid) ? E.EmpID == empid : true)
                                                             && (!string.IsNullOrEmpty(empsn) ? E.EmpSN == empsn : true)                                      
                                                             && (!string.IsNullOrEmpty(empdept) ? E.EmpDept == empdept : true)
                                                             && (!string.IsNullOrEmpty(cardid) ? C.CardID == cardid : true)
                                                             && (!string.IsNullOrEmpty(datetimesd) ? R.TimeLend >= Timesd : true)
                                                             && (!string.IsNullOrEmpty(datetimeed) ? R.TimeLend <= Timeed : true)
                                      orderby R.RecordNumber descending /*降冪排序*/
                                      select new ExportDataViewModel
                                      {
                                          RecordNumber = R.RecordNumber,
                                          EmpID = E.EmpID,
                                          EmpName = E.EmpName,
                                          EmpDept = E.EmpDept,
                                          CardID = C.CardID,
                                          CardName = C.CardName,
                                          CardDept = C.CardDept,
                                          TimeLend = R.TimeLend,
                                          TimeReturn = R.TimeReturn,
                                          RecordState = R.RecordState,
                                          UseDay = R.UseDay,
                                          CardAmount = C.CardAmount,
                                          TotalSpent = R.TotalSpent
                                      }).ToList();
                ViewBag.Message = true;
                Session["SessionECSdata"] = RecordList.ExportData;
                //Search不到資料
                if (RecordList.ExportData.Count == 0)
                {
                    ViewBag.Message = false;
                }
                
            }
            //return View(RecordList);
            return View(RecordList.ExportData.ToPagedList(currentPage, pageSize));  //分頁
        }

        /// <summary>
        /// 使用 ClosedXML - 匯出 Excel
        /// </summary>
        public ActionResult ExportData()
        {
            
            var con = (List<ExportDataViewModel>)Session["SessionECSdata"] ;
            DataTable dataTable = LINQToDataTable<ExportDataViewModel>(con);

            # region 將DaTaTable 資料 存成文字檔格式
            StringBuilder builder = new StringBuilder();
            List<string> columnNames = new List<string>();
            List<string> rows = new List<string>();
            
            foreach (DataColumn column in dataTable.Columns)
            {
                columnNames.Add(column.ColumnName);
            }

            builder.Append(string.Join(",", columnNames.ToArray())).Append("\n");
            foreach (DataRow row in dataTable.Rows)
            {
                List<string> currentRow = new List<string>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    object item = row[column];

                    currentRow.Add(item.ToString());
                }

                rows.Add(string.Join(",", currentRow.ToArray()));
            }
            builder.Append(string.Join("\n", rows.ToArray()));
            #endregion

            # region 下載 csv 檔案
            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=myfilename.csv");
            Response.ContentEncoding = Encoding.UTF8;
            StreamWriter sw = new StreamWriter(Response.OutputStream,Encoding.UTF8);
            //Response.Write(builder.ToString()); 寫文字檔使用Excel 開啟會產生亂碼
            sw.Write(builder.ToString());
            sw.Close();
            Response.End();
            #endregion

            //Response.AddHeader("content-disposition",
            //"attachment;filename=UnicodeChar.csv");
            //Response.ContentType = "application/octet-stream";
            //Response.ContentEncoding = Encoding.UTF8;
            //System.IO.StreamWriter sw =
            //    new System.IO.StreamWriter(
            //    Response.OutputStream,
            //    Encoding.UTF8);
            //sw.Write("犇,很多牛");
            //sw.Close();
            //Response.End();

            return RedirectToAction("Record");

        }

        # region List<T> 物件 轉成 DataTable
        public DataTable LINQToDataTable<T>(IEnumerable<T> varlist)
        {
            DataTable dtReturn = new DataTable();

            // column names 
            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow
                if (oProps == null)
                {
                    oProps = ((Type)rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                        == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue
                    (rec, null);
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        #endregion


        [HttpPost]
        public ActionResult HasData()
        {
            JObject jo = new JObject();
            bool result = !Session["SessionECSdata"].Equals(0);
            jo.Add("Msg", result.ToString());
            return Content(JsonConvert.SerializeObject(jo), "application/json");
        }

        public ActionResult Export()
        {
            var exportSpource = this.GetExportData();
            var dt = JsonConvert.DeserializeObject<DataTable>(exportSpource.ToString());

            var exportFileName = string.Concat(
                "RecordData_",
                DateTime.Now.ToString("yyyyMMddHHmm"),
                ".xlsx");

            return new ExportExcelResult
            {
                SheetName = "紀錄資料",
                FileName = exportFileName,
                ExportData = dt
            };
        }

        public JArray GetExportData()
        {
            List<ExportDataViewModel> query = Session["SessionECSdata"] as List<ExportDataViewModel>;

            JArray jObjects = new JArray();

            foreach (var item in query)
            {
                var jo = new JObject();
                jo.Add("紀錄編號	", item.RecordNumber);
                jo.Add("員工編號", item.EmpID);
                jo.Add("員工姓名", item.EmpName);
                jo.Add("員工部門", item.EmpDept);
                jo.Add("卡片別名", item.CardName);
                jo.Add("卡片部門", item.CardDept);
                jo.Add("借用時間", item.TimeLendDataFormat);
                jo.Add("歸還時間", item.TimeReturnDataFormat);
                jo.Add("借用狀態", item.RecordState);
                jo.Add("借用天數", item.UseDay);
                jo.Add("總花費", item.TotalSpent);
                jObjects.Add(jo);
            }
            return jObjects;
        }
    }

    
}
