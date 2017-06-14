using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiCM.Models
{
    public class Section
    {
        public int SectionId { get; set; }
        public int Parent { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
    }
}