using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApiCM.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection") {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, WebApiCM.Migrations.Configuration>("DefaultConnection"));
        }

        public virtual DbSet<Banner> Banners { get; set; }
        public virtual DbSet<Section> Sections { get; set; }
    }
}