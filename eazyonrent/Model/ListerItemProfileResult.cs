using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eazyonrent.Model
{
    public class ListerItemProfileResult : INotifyPropertyChanged
    {
        public ListerItemProfileResult()
        {
            ItemImageList = new List<ItemImageResult>();
            Review = new List<string>();
        }
        public int ListerItemId { get; set; }
        public string? ItemName { get; set; }
        public string? CompanyName { get; set; }
        public int ListerId { get; set; }
        public int? bookCount { get; set; }
        public List<string>? Review { get; set; }
        public double? StarRating { get; set; }
        public int? viewCount { get; set; }
        public decimal? ItemCost { get; set; }
        public string? ItemDescriptions { get; set; }
        public DateTime? Availablefrom { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? CategoryId { get; set; }
        public List<ItemImageResult> ItemImageList { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
