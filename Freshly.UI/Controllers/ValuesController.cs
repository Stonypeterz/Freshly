using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Freshly.Identity;
using Freshly.UI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Freshly.UI.Controllers
{
    [Authorize(AuthenticationSchemes = AuthScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private const string AuthScheme = JwtBearerDefaults.AuthenticationScheme;

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var n = from m in User.Claims select $"{m.Type}: {m.Value}";
            return n.ToArray();//new string[] { "value1", "value2", User.GetFullName(), User.Claims.Count().ToString(), User.Identity.IsAuthenticated.ToString(), User.Identity.Name };
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
