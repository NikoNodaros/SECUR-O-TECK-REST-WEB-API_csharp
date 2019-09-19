using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SecuroteckWebApplication.Models;
using Newtonsoft.Json.Linq;
using static SecuroteckWebApplication.Models.User;

namespace SecuroteckWebApplication.Controllers
{
    public class UserController : ApiController
    {
        [APIAuthorise]
        [ActionName("RemoveUser")]
        public HttpResponseMessage Delete([FromUri] string username, HttpRequestMessage httpRequest)
        {
            try
            {
                if (httpRequest.Headers.TryGetValues("ApiKey", out IEnumerable<string> headerValues))
                {
                    if (UserDatabaseAccess.UserExists(new Guid(headerValues.FirstOrDefault()).ToString(), username))
                    {
                        UserDatabaseAccess.DeleteUser(new Guid(headerValues.FirstOrDefault()).ToString());
                        return Request.CreateResponse(HttpStatusCode.OK, true);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, false);
            }
            catch (Exception e) { return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); }
        }

        [ActionName("New")]
        public HttpResponseMessage Get([FromUri] string username)
        {
            try
            {
                if (UserDatabaseAccess.UserExists(username))
                    return Request.CreateResponse(HttpStatusCode.OK, "True - User Does Exist! Did you mean to do a POST to create a new user?");
                return Request.CreateResponse(HttpStatusCode.OK, "False - User Does Not Exist! Did you mean to do a POST to create a new user?");
            }
            catch (Exception e) { return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); }
        }

        [ActionName("New")]
        public HttpResponseMessage Post([FromBody]string username)
        {
            try
            {
                if (UserDatabaseAccess.UserExists(username))
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Oops. This username is already in use. Please try again with a new username.");
                else if (string.IsNullOrEmpty(username))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json");
                else
                    return Request.CreateResponse(HttpStatusCode.OK, UserDatabaseAccess.CreateUser(username).ApiKey);
            }
            catch (Exception e) { return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); }
        }
        [APIAuthorise]
        [AdminRole]
        [ActionName("ChangeRole")]
        public HttpResponseMessage ChangeRole([FromBody] JObject jObject, HttpRequestMessage httpRequest)
        {
            try
            {
                string username = jObject["username"].ToString();
                Roles newRole;
                try { newRole = (Roles)Enum.Parse(typeof(Roles), jObject["role"].ToString()); }
                catch (Exception) { return Request.CreateResponse(HttpStatusCode.BadRequest, "NOT DONE: Role does not exist"); }
                if (!UserDatabaseAccess.UserExists(username)) return Request.CreateResponse(HttpStatusCode.BadRequest, "NOT DONE: Username does not exist");
                if (!UserDatabaseAccess.ChangeRole(username, newRole)) return Request.CreateResponse(HttpStatusCode.BadRequest, "NOT DONE: An error occurred");
                return Request.CreateResponse(HttpStatusCode.OK, "DONE");
            }
            catch (Exception e) { return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); }
        }

    }
}
