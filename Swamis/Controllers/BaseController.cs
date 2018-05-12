using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

using Sawtooth.Swamis.Models;

namespace Sawtooth.Swamis.Controllers
{
    public class BaseController : Controller
    {
        public DateTime GetRawDate(DateTime inDate)
        {
            int offSet = SubtractOffSet();
            string ymd = inDate.ToString("yyyy-MM-dd");
            return Convert.ToDateTime(ymd).AddHours(offSet);
        }

        public int SubtractOffSet()
        {
            DateTime pac = DateTime.Now;
            DateTime utc = DateTime.UtcNow;
            TimeSpan ts = pac - utc;
            int offSet = ts.Hours;
            return offSet;
        }

        public int AddOffSet()
        {
            DateTime pac = DateTime.Now;
            DateTime utc = DateTime.UtcNow;
            TimeSpan ts = utc - pac;
            int offSet = ts.Hours;
            return offSet;
        }

        protected List<SelectListItem> GetSymbols()
        {
            string[] symbols = ConfigurationManager.AppSettings["symbols"].Split(';');
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach(string ss in symbols)
            {
                string[] ll = ss.Split(',');
                listItems.Add(new SelectListItem
                {
                    Text = ll[1],
                    Value = ll[0]
                });

            }

            return listItems;
        }

        protected string ValidateTrade(DailyTradesModel dt)
        {
            StringBuilder sb = new StringBuilder("");
            if (dt.HiPrice < dt.ClosePrice)
            {
                sb.AppendLine("High cannot be lower than Close");
            }
            if (dt.HiPrice < dt.LowPrice)
            {
                sb.AppendLine("High cannot be lower than Low");
            }
            if (dt.HiPrice < dt.OpenPrice)
            {
                sb.AppendLine("High cannot be lower than Open");
            }
            if (dt.LowPrice > dt.OpenPrice)
            {
                sb.AppendLine("Low cannot be higher than Open");
            }
            if (dt.LowPrice > dt.HiPrice)
            {
                sb.AppendLine("Low cannot be higher than High");
            }
            if (dt.LowPrice > dt.ClosePrice)
            {
                sb.AppendLine("Low cannot be higher than Close");
            }
            if(dt.CloseDate > DateTime.Today)
            {
                sb.AppendLine("Close date can't be in the future");
            }
            return sb.ToString();
        }

    }
}