using api.fefarm.mx.Entity;
using api.fefarm.mx.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Cors;
using System.Web.Http;

namespace api.fefarm.mx.Controllers
{
    [System.Web.Mvc.SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UsersController : ApiController
    {
        [HttpPost]
        [Route("Users/Login")]
        public async Task<HttpResponseMessage> Login([FromBody] LoginModel json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities context = new CMS_fefarmEntities();
            
            try
            {
                var PasswordHash = ComputeSha256Hash(json.UserAdmin_User_Password);
                var User = context.cat_Admin_Users.FirstOrDefault(x => x.Admin_User_Username == json.UserAdmin_User_Username && x.Admin_User_Password == PasswordHash);
                if(User != null)
                {
                    dict.Add("message", "Valid Credentials");
                    return Request.CreateResponse(HttpStatusCode.OK, dict);
                }
                else
                {
                    dict.Add("message", "Invalid Credentials");
                    return Request.CreateResponse(HttpStatusCode.Forbidden, dict);
                }
            }
            catch (Exception ex)
            {
                dict.Add("message", ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, dict);
            }
        }

        private byte[] ComputeSha256Hash(object rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData.ToString()));
            }
        }
    }
}
