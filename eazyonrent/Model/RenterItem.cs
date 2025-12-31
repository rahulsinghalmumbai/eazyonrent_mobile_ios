using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eazyonrent.Model
{
    public class RenterItem
    {
        public int RenterItemId { get; set; }
        public int? RenterID { get; set; }
        public double? Rating { get; set; }
        public string? Review { get; set; }
        public int? ItemId { get; set; }
        public DateTime? RentFromDate { get; set; }
        public DateTime? RentToDate { get; set; }
        public bool? Status { get; set; }
    }
}
