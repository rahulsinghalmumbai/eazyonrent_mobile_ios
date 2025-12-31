using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eazyonrent.Model
{
    public class ItemImageResult
    {
        public int? ImageId { get; set; }
        public int? ListerItemId { get; set; }
        public string? ImageName { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
    }
}
