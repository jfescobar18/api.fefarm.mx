using api.fefarm.mx.Entity;
using api.fefarm.mx.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace api.fefarm.mx.Controllers
{
    [System.Web.Mvc.SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class RequestsController : ApiController
    {
        #region Request Templates
        [HttpPost]
        [Route("Requests/AddRequestTemplate")]
        public async Task<HttpResponseMessage> AddRequestTemplate([FromBody] cat_Requests json)
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
        [Route("Requests/UpdateRequestTemplate")]
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
        [Route("Requests/DeleteRequestTemplate")]
        public async Task<HttpResponseMessage> DeleteRequestTemplate([FromBody]cat_Requests json)
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
        [Route("Requests/GetRequestTemplates")]
        public async Task<List<vw_Requests>> GetRequestTemplates()
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            await Task.CompletedTask;
            return entity.vw_Requests.ToList();
        }

        [HttpGet]
        [Route("Requests/GetOpenRequestTemplates")]
        public async Task<List<vw_OpenRequestTemplates>> GetOpenRequestTemplates()
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            await Task.CompletedTask;
            return entity.vw_OpenRequestTemplates.OrderBy(x => x.Request_Start_Date).ToList();
        }

        [HttpGet]
        [Route("Requests/GetRequestTemplate/{Request_Id}")]
        public async Task<vw_Requests> GetRequestTemplate(int Request_Id)
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            await Task.CompletedTask;
            return entity.vw_Requests.FirstOrDefault(x => x.Request_Id == Request_Id);
        }

        [HttpGet]
        [Route("Requests/CheckTemplate/{Request_Id}")]
        public async Task<HttpResponseMessage> CheckTemplate(int Request_Id)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var request = entity.cat_Requests.SingleOrDefault(x => x.Request_Id == Request_Id);

                List<RequestModel> requestResults = JsonConvert.DeserializeObject<List<RequestModel>>(request.Request_JSON_Body);
                dict.Add("FailedInput", new Dictionary<string, object>());

                foreach (var input in requestResults)
                {
                    int totalValues = input.values.array.Count;
                    int totalPoint = input.points.array.Count;

                    if (totalValues != totalPoint && input.points.@string.Length > 0)
                    {
                        string values = string.Join(",", input.values.array);
                        string points = string.Join(",", input.points.array);
                        dict.Add($"FailedInput-{input.id}", $"Label: {input.label}, Values Count: {totalValues}, Points Count: {totalPoint}, Values: {values}, Points: {points}");
                    }
                }

                statusCode = HttpStatusCode.OK;
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
        #endregion
    }
}
