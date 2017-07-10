using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.MediaServices.Client.Live;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace WebApiCM.CM.Controllers
{
    [RoutePrefix("api/cm/endpoint")]  
    public class EndpointController : ApiController
    {       

        // Read values from the App.config file.
        private static readonly string _mediaServicesAccountName = ConfigurationManager.AppSettings["MediaServicesAccountName"];
        private static readonly string _mediaServicesAccountKey = ConfigurationManager.AppSettings["MediaServicesAccountKey"];

        // Field for service context.
        private static CloudMediaContext _context = null;
        private static MediaServicesCredentials _cachedCredentials = null;

        // GET api/cm/endpoint/start  
        [SwaggerOperation("Start or stop endpoint")]    
        [Route("{id:int}")]
        public IHttpActionResult GetState(int id) {
            // Create and cache the Media Services credentials in a static class variable.
            _cachedCredentials = new MediaServicesCredentials(_mediaServicesAccountName, _mediaServicesAccountKey);

            // Used the cached credentials to create CloudMediaContext.
            _context = new CloudMediaContext(_cachedCredentials);

            //Console.WriteLine("Starting Endpoint...");
            //Console.WriteLine(DateTime.Now);
            var defaultStreamingEndpoint = _context.StreamingEndpoints.Where(s => s.Name.Contains("default")).FirstOrDefault();                        
            
            switch (id) {
                case 1: defaultStreamingEndpoint.Start();
                    break;
                case 2: defaultStreamingEndpoint.Stop();
                    break;
                case 3:
                    break;
                default: return NotFound();                    
            }

            string endpointName = defaultStreamingEndpoint.Name.ToString();
            string endpointState = defaultStreamingEndpoint.State.ToString();
            string result = string.Format("Name: {0}, State: {1}", endpointName, endpointState);                          

            return Ok(result);
        }
       
    }
}

