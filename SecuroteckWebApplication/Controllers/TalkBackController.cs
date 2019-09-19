using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SecuroteckWebApplication.Models;

namespace SecuroteckWebApplication.Controllers
{
    public class TalkBackController : ApiController
    {
        [ActionName("Hello")]
        public string Get()
        {
            #region TASK1
            // TODO: add api/talkback/hello response
            return "Hello World";
            #endregion
        }

        [ActionName("Sort")]
        public HttpResponseMessage Get([FromUri]int[] integers)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                Array.Sort(integers);
                return Request.CreateResponse(HttpStatusCode.OK, integers);
            }
            catch (Exception e) { return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); }
        }

    }
}
