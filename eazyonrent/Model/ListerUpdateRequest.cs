using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eazyonrent.Model
{
    public class ListerUpdateRequest
    {
        public int? ListerId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? CompanyName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        //public string? DefaultImage { get; set; }
        public FileResult? ImageFile { get; set; }
    }
}
