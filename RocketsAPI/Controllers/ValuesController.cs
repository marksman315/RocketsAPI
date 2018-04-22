using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RocketsAPI.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet("{name}/{value}")]
        [Produces("application/json")]        
        public ObjectResult Get(string name, string value)
        {
            var documents = DocumentHandler.GetDocuments(name, value);
            
            return new OkObjectResult(documents);
        }

        // POST api/values
        [HttpPost]
        [Produces("application/json")]
        public async Task<ObjectResult> PostAsync([FromBody]string value)
        {
            var json = JObject.Parse(value);

            await DocumentHandler.UpsertDocumentAsync(json);

            return new OkObjectResult("Ok");
        }

        // PUT api/values
        [HttpPut]
        [Produces("application/json")]
        public async Task<ObjectResult> PutAsync([FromBody]string value)
        {
            var json = JObject.Parse(value);

            await DocumentHandler.UpsertDocumentAsync(json);

            return new OkObjectResult("Ok");
        }

        // DELETE api/values/
        [HttpDelete("{id}")]
        public ObjectResult Delete(string id)
        {
            return new OkObjectResult("Delete not implemented");
        }
    }
}
