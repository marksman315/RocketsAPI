using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
        [HttpPut]
        [Produces("application/json")]
        public async Task<ObjectResult> PostAsync([FromBody]dynamic value)
        {
            await DocumentHandler.UpsertDocumentAsync(value);

            return new OkObjectResult("Ok");
        }      

        // DELETE api/values/
        [HttpDelete("{id}")]
        public ObjectResult Delete(string id)
        {
            return new OkObjectResult("Denied! You cannot delete me!");
        }
    }
}
