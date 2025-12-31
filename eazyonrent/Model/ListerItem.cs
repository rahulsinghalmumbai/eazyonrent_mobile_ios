using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace eazyonrent.Model
{
    public class ListerItem
    {
        public int ListerItemId { get; set; }
        public string? ItemName { get; set; }
        public int ListerId { get; set; }
        public decimal? ItemCost { get; set; }
        public string? ItemDescriptions { get; set; }
        public DateTime? Availablefrom { get; set; }
        public int? Status { get; set; }
        public bool? AvailabilityType { get; set; } = true;
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? CategoryId { get; set; }
    }
}
