using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eazyonrent.Model
{
    public class ApiResponseItem<T>
    {
        public string? ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
        public T? ItemList { get; set; }
    }
    public class ApiResponseCat<T>
    {
        public string? ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
        public T? CategoriesList { get; set; }
    }
    public class ListerItemProfileResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public List<ListerItemProfileResult> ItemList { get; set; } = new();
    }
    public class AddItmApiResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public int? ListerItemId { get; set; }


    }
    public class  AddItemImagesResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public List<int>? ImageIds { get; set; }
    }
}
