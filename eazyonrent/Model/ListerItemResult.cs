using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eazyonrent.Model
{
    public class ListerItemResult : INotifyPropertyChanged
    {
        private string _location = "Noida";
        private List<string> _Images;
        private List<ItemImage> _itemImageList;

        public int ListerItemId { get; set; }
        public string? ItemName { get; set; }
        public string? CompanyName { get; set; }
        public int ListerId { get; set; }
        public int? viewCount { get; set; }
        public decimal? ItemCost { get; set; }
        public string? ItemDescriptions { get; set; }
        public DateTime? Availablefrom { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? CategoryId { get; set; }

        //public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Additional properties for UI
        public string Location
        {
            get => _location;
            set
            {
                _location = value ?? "Noida";
                OnPropertyChanged();
            }
        }

        public List<string> Images
        {
            get => _Images;
            set
            {
                _Images = value;
                OnPropertyChanged();
            }
        }

        // Add this property with proper backing field
        public List<ItemImage> ItemImageList
        {
            get => _itemImageList;
            set
            {
                _itemImageList = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Move ItemImage class outside of ListerItemResult class
    public class ItemImage
    {
        public int ImageId { get; set; }
        public int? ListerItemId { get; set; }
        public string ImageName { get; set; }
        public object ImageFiles { get; set; }
    }
}