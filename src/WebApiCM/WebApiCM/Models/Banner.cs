using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiCM.Models
{
    public class Banner
    {
        public int BannerId { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string ExpireAt { get; set; }
        public int Order { get; set; }
        public int LinkType { get; set; }
        public string Image { get; set; }
        public int Publish { get; set; }
    }
}