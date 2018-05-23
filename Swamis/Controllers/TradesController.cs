using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
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

namespace Swamis.Controllers
{
    public class TradesController : BaseController
    {
        // GET: Trades
        public ActionResult Index()
        {
            SearchModel model = new SearchModel();

            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    model.EndDate = DateTime.Today.AddDays(3);
                    break;
                case DayOfWeek.Saturday:
                    model.StartDate = DateTime.Today.AddDays(-1);
                    model.EndDate = DateTime.Today.AddDays(2);
                    break;
                case DayOfWeek.Sunday:
                    model.StartDate = DateTime.Today.AddDays(-2);
                    break;
                default:
                    model.StartDate = DateTime.Today;
                    model.EndDate = DateTime.Today.AddDays(1);
                    break;
            }

            List<DailyTradesModel> dailies = new List<DailyTradesModel>();
            dailies.Add(new DailyTradesModel { Symbol = StaticRoutines.GetSymbol("GC", DateTime.Today) });
            dailies.Add(new DailyTradesModel { Symbol = StaticRoutines.GetSymbol("SP", DateTime.Today) });
            dailies.Add(new DailyTradesModel { Symbol = StaticRoutines.GetSymbol("ZB", DateTime.Today) });
            model.DailyTradesModel = dailies;
            model.DoQuote = false;
            model.DoTrigger = false;
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SearchModel model)
        {
            //TODO:  add notification to page
            StringBuilder sb = new StringBuilder();
            if (model.DoQuote)
            {
                try
                {
                    MongoEngine db = new MongoEngine();
                    Quoter qs = new Quoter();
                    foreach (DailyTradesModel mm in model.DailyTradesModel)
                    {
                        List<DailyTrades> tt = qs.GetBarchartHistory(model.StartDate, mm.Symbol);
                        DailyTrades dd = tt.FirstOrDefault(x => x.CloseDate == model.StartDate);
                        sb.AppendLine(string.Format("{0}: Open: {1}, Hi: {2}, Low: {3}, Close: {4}\n",
                            dd.Symbol, dd.OpenPrice.ToString(), dd.HiPrice.ToString(), dd.LowPrice.ToString(), dd.ClosePrice.ToString()));
                        db.UpdateTrade(dd);
                    }

                }
                catch (Exception exc)
                {
                    //MessageBox.Show("Error during quote retrieval: " + exc.Message);
                    string msg = exc.Message;
                    sb.AppendLine(exc.Message);
                    sb.AppendLine(exc.StackTrace);
                }

            }
            if(model.DoTrigger)
            {
                try
                {
                    Sawtooth.GV_Bus.BusBase bus = new Sawtooth.GV_Bus.BusBase();
                    List<PricePattern> pp = bus.ProcessTDWSymbol(model.DailyTradesModel[1].Symbol,
                        model.DailyTradesModel[2].Symbol,
                        model.DailyTradesModel[0].Symbol,
                        model.EndDate);
                    return RedirectToAction("ViewTriggers", "Triggers", new {tradeDate = model.EndDate});
                }
                catch (Exception exc)
                {
                    sb.AppendLine(exc.Message);
                    sb.AppendLine(exc.StackTrace);
                }
            }
            ViewBag.Message = sb.ToString();
            //return RedirectToAction("ViewTrades", new { startDate = model.StartDate, endDate = model.EndDate });
            return View(model);
        }

        public ActionResult ViewTrades(DateTime startDate, DateTime endDate)
        {
            SearchModel model = new SearchModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            string result = "";
            string path = ConfigurationManager.AppSettings["tradesSvc"];
            string action = "GetTradeByDates/" +
                startDate.ToString("yyyy-MM-dd") + "/" +
                endDate.ToString("yyyy-MM-dd");
            List<DailyTradesModel> trades = new List<DailyTradesModel>();
            using (WebClient wc = new WebClient())
            {
                result = wc.DownloadString(path + action);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    trades = new JavaScriptSerializer().Deserialize<List<DailyTradesModel>>(result);
                }
            }
            foreach (DailyTradesModel dt in trades)
            {
                dt.CloseDate = dt.CloseDate.AddHours(AddOffSet());
                if (dt.Symbol.Contains("ZB"))
                {

                    dt.sClosePrice = StaticRoutines.DecTo32Display(dt.ClosePrice);
                    dt.sHiPrice = StaticRoutines.DecTo32Display(dt.HiPrice);
                    dt.sLowPrice = StaticRoutines.DecTo32Display(dt.LowPrice);
                    dt.sOpenPrice = StaticRoutines.DecTo32Display(dt.OpenPrice);
                }
                else
                {
                    dt.sClosePrice = dt.ClosePrice.ToString("0.00");
                    dt.sHiPrice = dt.HiPrice.ToString("0.00");
                    dt.sLowPrice = dt.LowPrice.ToString("0.00");
                    dt.sOpenPrice = dt.OpenPrice.ToString("0.00");
                }
            }
            model.DailyTradesModel = trades;
            return View(model);

        }

        [HttpPost]
        public ActionResult ViewTrades(SearchModel model)
        {
            return RedirectToAction("ViewTrades", new { startDate = model.StartDate, endDate = model.EndDate });
        }


        // GET: Trades/Details/5
        public ActionResult Details(string id)
        {
            string result = "";
            string path = ConfigurationManager.AppSettings["tradesSvc"];
            string action = "GetTrade/x/" + id;
            List<DailyTrades> trades = new List<DailyTrades>();
            using (WebClient wc = new WebClient())
            {
                result = wc.DownloadString(path + action);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    trades = new JavaScriptSerializer().Deserialize<List<DailyTrades>>(result);
                }
            }
            return View(trades[1]);
        }

        // GET: Trades/Create
        public ActionResult Create()
        {
            //ViewBag.Symbols = GetSymbols();
            DailyTradesModel model = new DailyTradesModel();
            model.TradeSymbols = GetSymbols();
            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    model.CloseDate = DateTime.Today.AddDays(-2);
                    break;
                case DayOfWeek.Saturday:
                    model.CloseDate = DateTime.Today.AddDays(-1);
                    break;
                default:
                    model.CloseDate = DateTime.Today;
                    break;
            }
            return View(model);
        }

        // POST: Trades/Create
        [HttpPost]
        public ActionResult Create(DailyTradesModel model)
        {
            try
            {
                string msg = ValidateTrade(model);
                if (msg == "")
                {
                    DailyTrades dt = new DailyTrades
                    {
                        CloseDate = model.CloseDate,
                        ClosePrice = model.ClosePrice,
                        HiPrice = model.HiPrice,
                        Id = model.Id,
                        LowPrice = model.LowPrice,
                        OpenPrice = model.OpenPrice,
                        Volume = model.Volume
                    };
                    dt.Symbol = model.Symbol;
                    if (model.Symbol == "ZB")
                    {
                        dt.ClosePrice = StaticRoutines.ThirtyTwoToDec(model.ClosePrice);
                        dt.HiPrice = StaticRoutines.ThirtyTwoToDec(model.HiPrice);
                        dt.LowPrice = StaticRoutines.ThirtyTwoToDec(model.LowPrice);
                        dt.OpenPrice = StaticRoutines.ThirtyTwoToDec(model.OpenPrice);
                    }
                    MongoEngine db = new MongoEngine();
                    List<DailyTrades> trades = new List<DailyTrades>();
                    trades = db.UpdateTrade(dt);
                    return View();
                }
                else
                {

                }

                return RedirectToAction("Create");
            }
            catch (Exception exc)
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult CreateSvc(DailyTradesModel model)
        {
            try
            {
                string msg = ValidateTrade(model);
                if (msg == "")
                {
                    DailyTrades dt = new DailyTrades
                    {
                        CloseDate = model.CloseDate,
                        ClosePrice = model.ClosePrice,
                        HiPrice = model.HiPrice,
                        Id = model.Id,
                        LowPrice = model.LowPrice,
                        OpenPrice = model.OpenPrice,
                        Volume = model.Volume
                    };
                    dt.Symbol = StaticRoutines.GetSymbol(model.Symbol, model.CloseDate);
                    if (model.Symbol == "ZB")
                    {
                        dt.ClosePrice = StaticRoutines.ThirtyTwoToDec(model.ClosePrice);
                        dt.HiPrice = StaticRoutines.ThirtyTwoToDec(model.HiPrice);
                        dt.LowPrice = StaticRoutines.ThirtyTwoToDec(model.LowPrice);
                        dt.OpenPrice = StaticRoutines.ThirtyTwoToDec(model.OpenPrice);
                    }
                    string result = "";
                    string req = new JavaScriptSerializer().Serialize(model);
                    var client = new HttpClient();
                    string path = ConfigurationManager.AppSettings["tradesSvc"] + "UpdateTrade/" + req;
                    using (WebClient wc = new WebClient())
                    {
                        result = wc.DownloadString(path);
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            DailyTrades reTrade = new JavaScriptSerializer().Deserialize<DailyTrades>(result);
                        }
                    }
                    return View();
                }
                else
                {

                }

                return RedirectToAction("Create");
            }
            catch (Exception exc)
            {
                return View();
            }
        }

        private void Poster()
        {
            //var client = new HttpClient();
            //string path = ConfigurationManager.AppSettings["tradesSvc"] + "DailyTrades/UpdateTrade";
            //var task = client.PostAsJsonAsync(path, dt).ContinueWith(taskWresponse =>
            //{
            //    var response = taskWresponse.Result;
            //    var readtask = response.Content.ReadAsAsync<bool>();
            //    readtask.Wait();
            //});
            //string jj = "";

        }

        [HttpPost]
        public ActionResult Update(DailyTrades model)
        {
            if (ModelState.IsValid)
            {
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Symbols = GetSymbols();
            return View(model);
        }


        // GET: Trades/Edit/5
        public ActionResult Edit(string id)
        {
            string result = "";
            string path = ConfigurationManager.AppSettings["tradesSvc"];
            string action = "GetTrade/x/" + id;
            DailyTradesModel trades = new DailyTradesModel();
            using (WebClient wc = new WebClient())
            {
                result = wc.DownloadString(path + action);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    trades = new JavaScriptSerializer().Deserialize<DailyTradesModel>(result);
                    if (trades.Symbol.Contains("Z"))
                    {
                        trades.ClosePrice = StaticRoutines.DecTo32(trades.ClosePrice);
                        trades.HiPrice = StaticRoutines.DecTo32(trades.HiPrice);
                        trades.LowPrice = StaticRoutines.DecTo32(trades.LowPrice);
                        trades.OpenPrice = StaticRoutines.DecTo32(trades.OpenPrice);
                    }
                }
            }
            return View(trades);
        }

        // POST: Trades/Edit/5
        [HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        public ActionResult Edit(DailyTradesModel model)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

    }
}
