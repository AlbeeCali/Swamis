using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Web.Mvc;

using Sawtooth.GV_Entities;

namespace Sawtooth.Swamis.Models
{
    public class EntExtender
    {
    }

    public class SearchModel
    {
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        public List<DailyTradesModel> DailyTradesModel { get; set; }


    }

    public class DailyTradesModel 
    {
        public IEnumerable<SelectListItem> TradeSymbols { get; set; }

        public enum TradeSymbol
        {
            [Description("Gold")]
            GC,
            [Description("S & P")]
            SP,
            [Description("T-Bill")]
            ZB,
            [Description("Williams")]
            WLM
        }


        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Symbol { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal OpenPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal HiPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public Int64 Volume { get; set; }

        [Display(Name = "Close Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CloseDate { get; set; }

        public string sOpenPrice { get; set; }
        public string sHiPrice { get; set; }
        public string sLowPrice { get; set; }
        public string sClosePrice { get; set; }


    }

    public class DailyTriggersModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Display(Name = "Trade Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TradeDate { get; set; }

        public List<PricePattern> Patterns { get; set; }

    }

}