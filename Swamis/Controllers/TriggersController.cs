using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script;
using System.Web.Script.Serialization;

using Sawtooth.GV_Entities;
using Sawtooth.Common;
using SawTooth.GV_Bus;

using System.Threading.Tasks;
using Sawtooth.Swamis.Controllers;
using Sawtooth.Swamis.Models;
using System.Net.Http;

namespace Sawtooth.Swamis.Controllers
{
    public class TriggersController : BaseController
    {
        // GET: Triggers
        public ActionResult Index()
        {
            DailyTriggersModel dt = new DailyTriggersModel();
            DateTime tradeDate = DateTime.Today;
            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    tradeDate = DateTime.Today.AddDays(3);
                    break;
                case DayOfWeek.Saturday:
                    tradeDate = DateTime.Today.AddDays(2);
                    break;
                default:
                    tradeDate = DateTime.Today.AddDays(1);
                    break;
            }
            dt.TradeDate = tradeDate;
            return View(dt);
        }

        [HttpPost]
        public ActionResult Index(DailyTriggersModel model)
        {
            return RedirectToAction("ViewTriggers", new { tradeDate = model.TradeDate });
        }

        public ActionResult ViewTriggers(DateTime tradeDate)
        {
            DailyTriggersModel dt = new DailyTriggersModel();
            MongoEngine db = new MongoEngine();
            DailyTriggers trigger = new DailyTriggers { TradeDate = tradeDate };
            List<DailyTriggers> triggers = db.GetTrigger(trigger);
            if (triggers.Count > 0)
            {
                dt.TradeDate = triggers.First().TradeDate;
                dt.Patterns = triggers.First().Patterns;
            }
            //dt.Patterns = dt.Patterns.OrderBy(pp => pp.Contract).ThenBy(pp => pp.TradeType) as List<PricePattern>;
            return View(dt);
        }

        public ActionResult ViewTriggersSvc(DateTime tradeDate)
        {
            DailyTriggersModel dt = new DailyTriggersModel();
            string path = ConfigurationManager.AppSettings["triggersSvc"];
            string action = "GetTrigger/" +
                tradeDate.ToString("yyyy-MM-dd");
            using (WebClient wc = new WebClient())
            {
                string result = wc.DownloadString(path + action);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    List<DailyTriggersModel> triggers = new JavaScriptSerializer().Deserialize<List<DailyTriggersModel>>(result);
                    if (triggers.Count > 0)
                    {
                        dt = triggers[0];
                    }
                }
            }

            return View(dt);

        }


        [HttpPost]
        public ActionResult ViewTriggers(DailyTriggersModel model)
        {
            return RedirectToAction("ViewTriggers", new { tradeDate = model.TradeDate });
        }

    }
}