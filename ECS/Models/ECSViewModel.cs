using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECS.Models
{
    public class ECSViewModel
    {
        public string EmpID { get; set; }
        public string EmpPwd { get; set; }
        public string EmpSN { get; set; }
        public string EmpDept { get; set; }
        public string EmpName { get; set; }
        public string Email { get; set; }
        public string EmpState { get; set; }
        public string Role { get; set; }
        public string CardID { get; set; }
        public string CardDept { get; set; }
        public string CardName { get; set; }
        public string CardState { get; set; }
        public Nullable<int> CardAmount { get; set; }
        public string RecordState { get; set; }
        public int RecordNumber { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public Nullable<System.DateTime> TimeLend { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public Nullable<System.DateTime> TimeReturn { get; set; }
        public string UseDay { get; set; }
        public Nullable<int> TotalSpent { get; set; }

        public string TimeLendDataFormat //EmpBirthday to String("yyyy/MM/dd") 
        {            
            get
            {
                //DateTime.TryParse()
                string formatafter = string.Empty;
                if (this.TimeLend == null)       //不能有NULL
                    formatafter = "";
                else
                    formatafter = Convert.ToDateTime(this.TimeLend).ToString("yyyy/MM/dd");
                return formatafter;
            }
        }

        public string TimeReturnDataFormat
        {
            get
            {
                //DateTime.TryParse()
                string formatafter = string.Empty;
                if (this.TimeReturn == null)       //不能有NULL
                    formatafter = "";
                else
                    formatafter = Convert.ToDateTime(this.TimeReturn).ToString("yyyy/MM/dd");
                return formatafter;
            }
        }
    }
    public class FormViewModel
    {
        public List<ECSViewModel> ECSdata;

        public List<ExportDataViewModel> ExportData;
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }

        public FormViewModel()
        {
            ECSdata = new List<ECSViewModel>();
            ExportData = new List<ExportDataViewModel>();
        }
        
    }

    public class ExportDataViewModel
    {
        public int RecordNumber { get; set; }
        public string EmpID { get; set; }
        public string EmpDept { get; set; }
        public string EmpName { get; set; }
        public string CardID { get; set; }
        public string CardDept { get; set; }
        public string CardName { get; set; }        
        public Nullable<System.DateTime> TimeLend { get; set; }
        public Nullable<System.DateTime> TimeReturn { get; set; }
        public string RecordState { get; set; }
        public string UseDay { get; set; }
        public Nullable<int> CardAmount { get; set; }
        public Nullable<int> TotalSpent { get; set; }


        public string TimeLendDataFormat //EmpBirthday to String("yyyy/MM/dd") 
        {
            get
            {
                //DateTime.TryParse()
                string formatafter = string.Empty;
                if (this.TimeLend == null)       //不能有NULL
                    formatafter = "";
                else
                    formatafter = Convert.ToDateTime(this.TimeLend).ToString("yyyy/MM/dd");
                return formatafter;
            }
        }

        public string TimeReturnDataFormat
        {
            get
            {
                //DateTime.TryParse()
                string formatafter = string.Empty;
                if (this.TimeReturn == null)       //不能有NULL
                    formatafter = "";
                else
                    formatafter = Convert.ToDateTime(this.TimeReturn).ToString("yyyy/MM/dd");
                return formatafter;
            }
        }
    }
}