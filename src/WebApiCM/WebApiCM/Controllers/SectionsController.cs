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
    public class SectionsController : ApiController
    {
        // GET api/sections        
        [SwaggerOperation("GetAll")]
        public IEnumerable<Section> Get() {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            return dbContext.Sections.ToArray();
        }

        // GET api/sections/1
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get(int id) {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var result = dbContext.Sections.SingleOrDefault(c => c.SectionId == id);
            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        // POST api/sections
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public IHttpActionResult Post([FromBody]Section section) {
            try {
                ApplicationDbContext dbContext = new ApplicationDbContext();
                dbContext.Sections.Add(section);
                dbContext.SaveChanges();
                return Created(new Uri(Request.RequestUri + section.SectionId.ToString()), section);
            } catch {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
                return ResponseMessage(response);
            }
        }

        // PUT api/banners/1
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Put(int id, [FromBody]Section updatedSection) {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var section = dbContext.Sections.SingleOrDefault(c => c.SectionId == id);
            if (section != null) {
                if (!string.IsNullOrEmpty(updatedSection.Parent.ToString()))
                    section.Parent = updatedSection.Parent;
                if (!string.IsNullOrEmpty(updatedSection.Order.ToString()))
                    section.Order = updatedSection.Order;
                if (!string.IsNullOrEmpty(updatedSection.Name))
                    section.Name = updatedSection.Name;
                dbContext.SaveChanges();
                return Ok();
            } else
                return NotFound();
        }

        // DELETE api/sections/1
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Delete(int id) {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            var result = dbContext.Sections.SingleOrDefault(c => c.SectionId == id);
            if (result != null) {
                dbContext.Sections.Remove(result);
                dbContext.SaveChanges();
                return Ok();
            } else
                return NotFound();
        }
    }
}