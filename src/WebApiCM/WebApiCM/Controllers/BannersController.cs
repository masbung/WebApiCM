using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using WebApiCM.Models;

namespace WebApiCM.Controllers
{
    public class BannersController : ApiController
    {
        // GET api/banners        
        [SwaggerOperation("GetAll")]
        public IEnumerable<Banner> Get() {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            return dbContext.Banners.ToArray();
        }

        // GET api/banners/1
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get(int id) {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var result = dbContext.Banners.SingleOrDefault(c => c.BannerId == id);
            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        // POST api/banners
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public IHttpActionResult Post([FromBody]Banner banner) {
            try {
                ApplicationDbContext dbContext = new ApplicationDbContext();
                dbContext.Banners.Add(banner);
                dbContext.SaveChanges();                
                return Created(new Uri(Request.RequestUri + banner.BannerId.ToString()), banner);
            } catch {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
                return ResponseMessage(response);
            }
        }
      
        // PUT api/banners/1
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Put(int id, [FromBody]Banner updatedBanner) {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var banner = dbContext.Banners.SingleOrDefault(c => c.BannerId == id);
            if (banner != null) {
                if (!string.IsNullOrEmpty(updatedBanner.ExpireAt))
                    banner.ExpireAt = updatedBanner.ExpireAt;
                if (!string.IsNullOrEmpty(updatedBanner.Order.ToString()))
                    banner.Order = updatedBanner.Order;
                if (!string.IsNullOrEmpty(updatedBanner.LinkType.ToString()))
                    banner.LinkType = updatedBanner.LinkType;
                if (!string.IsNullOrEmpty(updatedBanner.Image))
                    banner.Image = updatedBanner.Image;
                if (!string.IsNullOrEmpty(updatedBanner.Publish.ToString()))
                    banner.Publish = updatedBanner.Publish;
                dbContext.SaveChanges();
                return Ok();
            } else
                return NotFound(); 
        }

        // DELETE api/banners/1
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Delete(int id) {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var result = dbContext.Banners.SingleOrDefault(c => c.BannerId == id);
            if (result != null) {
                dbContext.Banners.Remove(result);
                dbContext.SaveChanges();
                return Ok();
            } else
                return NotFound();
        }
    }
}