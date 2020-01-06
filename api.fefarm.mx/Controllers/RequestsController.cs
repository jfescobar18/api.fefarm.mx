using api.fefarm.mx.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Utils;

namespace api.fefarm.mx.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class RequestsController : ApiController
    {
        [HttpPost]
        [Route("Requests/AddRequest")]
        public async Task<HttpResponseMessage> AddRequest([FromBody] cat_Requests json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var request = new cat_Requests()
                {
                    Request_Name = json.Request_Name,
                    Request_JSON_Body = json.Request_JSON_Body,
                    Request_Creation_Date = DateTime.Now,
                    Request_Start_Date = json.Request_Start_Date,
                    Request_Finish_Date = json.Request_Finish_Date,
                    Request_Max_Applications = json.Request_Max_Applications,
                    Request_Max_Beneficiaries = json.Request_Max_Beneficiaries
                };

                entity.cat_Requests.Add(request);
                entity.SaveChanges();

                statusCode = HttpStatusCode.OK;
                dict.Add("message", "Request added successfully");
            }
            catch (Exception ex)
            {
                if (dict.Keys.Count > 0)
                {
                    dict = new Dictionary<string, object>();
                    dict.Add("message", ex.Message);
                }
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpPost]
        [Route("Requests/UpdateRequest")]
        public async Task<HttpResponseMessage> UpdateRequest([FromBody] cat_Requests json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var request = entity.cat_Requests.SingleOrDefault(x => x.Request_Id == json.Request_Id);

                request.Request_Name = json.Request_Name;
                request.Request_JSON_Body = json.Request_JSON_Body;
                request.Request_Start_Date = json.Request_Start_Date;
                request.Request_Finish_Date = json.Request_Finish_Date;
                request.Request_Max_Applications = json.Request_Max_Applications;
                request.Request_Max_Beneficiaries = json.Request_Max_Beneficiaries;
                entity.SaveChanges();

                statusCode = HttpStatusCode.OK;
                dict.Add("message", "Request updated successfully");
            }
            catch (Exception ex)
            {
                if (dict.Keys.Count > 0)
                {
                    dict = new Dictionary<string, object>();
                    dict.Add("message", ex.Message);
                }
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpPost]
        [Route("Requests/DeleteRequest")]
        public async Task<HttpResponseMessage> DeleteRequest([FromBody]cat_Requests json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var request = entity.cat_Requests.SingleOrDefault(x => x.Request_Id == json.Request_Id);
                entity.cat_Requests.Remove(request);
                entity.SaveChanges();

                statusCode = HttpStatusCode.OK;
                dict.Add("message", "PDF deleted successfully");
            }
            catch (Exception ex)
            {
                if (dict.Keys.Count > 0)
                {
                    dict = new Dictionary<string, object>();
                    dict.Add("message", ex.Message);
                }
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpGet]
        [Route("Requests/GetRequests")]
        public async Task<List<vw_Requests>> GetRequests()
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            await Task.CompletedTask;
            return entity.vw_Requests.ToList();
        }
    }
}
