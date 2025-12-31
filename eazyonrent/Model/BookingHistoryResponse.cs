using System;
using System.Collections.Generic;

namespace eazyonrent.Model
{
    // API Response Model
    public class BookedItemHistoryResponse
    {
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public List<BookedItemHistory> data { get; set; }
    }

    public class BookedItemHistory
    {
        public int RenterItemId { get; set; }
        public int? RenterId { get; set; }
        public int? ItemId { get; set; }
        public DateTime? RentFromDate { get; set; }
        public DateTime? RentToDate { get; set; }
        public bool? BookingStatus { get; set; }
        public string ItemName { get; set; }
        public decimal? ItemCost { get; set; }
        public string ItemDescriptions { get; set; }
        public int? ListerId { get; set; }
        public string ListerName { get; set; }
        public string CompanyName { get; set; }
        public List<string> ItemImages { get; set; } = new List<string>();
        public int? Rating { get; set; }
        public string Review { get; set; }

        public string FormattedFromDate => (RentFromDate ?? DateTime.MinValue).ToString("dd MMM yyyy");
        public string FormattedToDate => (RentToDate ?? DateTime.MinValue).ToString("dd MMM yyyy");
        public string DateRange => $"{FormattedFromDate} - {FormattedToDate}";
        public string StatusText => (BookingStatus ?? false) ? "Active" : "Completed";
        public string StatusColor => (BookingStatus ?? false) ? "#4CAF50" : "#9E9E9E";
    }
}