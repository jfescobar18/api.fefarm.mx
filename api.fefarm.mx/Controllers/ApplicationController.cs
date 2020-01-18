using api.fefarm.mx.Entity;
using api.fefarm.mx.Models;
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
using System.IO.Compression;
using Utils;
using System.IO;
using System.Net.Http.Headers;

namespace api.fefarm.mx.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ApplicationController : ApiController
    {
        #region Application Filling
        [HttpPost]
        [Route("Application/AddApplication")]
        public async Task<HttpResponseMessage> AddApplication()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            List<string> filenames = new List<string>();

            int Request_Id = int.Parse(HttpContext.Current.Request.Form["Request_Id"]);
            string Application_JSON_Body = HttpContext.Current.Request.Form["Application_JSON_Body"];

            List<RequestModel> requestResults = JsonConvert.DeserializeObject<List<RequestModel>>(Application_JSON_Body);

            string applicantName = requestResults.Where(x => x.label == "Nombre").Select(x => x.answers[0]).FirstOrDefault();
            string applicantEmail = requestResults.Where(x => x.label == "Correo electrónico").Select(x => x.answers[0]).FirstOrDefault();
            string applicantPhone = requestResults.Where(x => x.label == "Teléfono celular").Select(x => x.answers[0]).FirstOrDefault();
            string CURP = requestResults.Where(x => x.label == "CURP").Select(x => x.answers[0]).FirstOrDefault();

            var applicationsLsit = entity.cat_Applications.Where(x => x.Application_Applicant_CURP == CURP).ToList();
            if(applicationsLsit != null)
            {
                statusCode = HttpStatusCode.BadRequest;
                dict.Add("error", "NotNull");
                await Task.CompletedTask;
                return Request.CreateResponse(statusCode, dict);
            }

            int Score = ApplicationUtils.GetScore(requestResults);
            try
            {
                var application = new cat_Applications()
                {
                    Application_Date = DateTime.Now,
                    Application_Stage_Date = DateTime.Now,
                    Application_Stage_Id = 1,
                    Application_JSON_Body = Application_JSON_Body,
                    Application_Applicant_Name = applicantName,
                    Application_Applicant_Email = applicantEmail,
                    Application_Applicant_Phone = applicantPhone,
                    Application_Applicant_CURP = CURP,
                    Application_Score = Score,
                    Application_PDF_Path = string.Empty,
                    Request_Id = Request_Id
                };

                entity.cat_Applications.Add(application);
                entity.SaveChanges();

                FileUtils.UploadFiles(HttpContext.Current.Request, $"~/ApplicationFiles/Id-{application.Application_Id}", ref statusCode, ref dict, ref filenames);
                ApplicationUtils.CreateReport(requestResults, application.Application_Id);

                foreach (string filename in filenames)
                {
                    var applicationFile = new cat_Application_Files()
                    {
                        Application_File_Path = $"ApplicationFiles/Id-{application.Application_Id}/{filename}",
                        Application_File_Name = filename.Replace(".pdf", ""),
                        Application_File_Upload_Date = DateTime.Now,
                        Application_Id = application.Application_Id
                    };
                    entity.cat_Application_Files.Add(applicationFile);
                }

                application.Application_PDF_Path = $"PDF-Applications/ApplicationId-{application.Application_Id}/application.pdf";
                entity.SaveChanges();

                statusCode = HttpStatusCode.OK;
                if (!dict.ContainsKey("message"))
                {
                    dict.Add("message", "Application added successfully");
                }
                dict.Add("IdNumber", DateTime.Now.ToString("yyyyMMdd") + application.Application_Id);
            }
            catch (Exception ex)
            {
                if (dict.Keys.Count > 0)
                {
                    dict = new Dictionary<string, object>();
                    dict.Add("messageException", ex.Message);
                }
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpPost]
        [Route("Application/UpdateApplication")]
        public async Task<HttpResponseMessage> UpdateApplication([FromBody] cat_Applications json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var application = entity.cat_Applications.SingleOrDefault(x => x.Application_Id == json.Application_Id);
                
                application.Application_JSON_Body = json.Application_JSON_Body;
                entity.SaveChanges();

                statusCode = HttpStatusCode.OK;
                dict.Add("message", "Application updated successfully");
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
        [Route("Application/UpdateApplicationStage")]
        public async Task<HttpResponseMessage> UpdateApplicationStage([FromBody] cat_Applications json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var application = entity.cat_Applications.SingleOrDefault(x => x.Application_Id == json.Application_Id);

                application.Application_Stage_Date = DateTime.Now;
                application.Application_Stage_Id = json.Application_Stage_Id;
                entity.SaveChanges();

                statusCode = HttpStatusCode.OK;
                dict.Add("message", "Application stage updated successfully");
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
        [Route("Application/GetApplications/{Request_Id}")]
        public async Task<List<vw_Applications>> GetApplications(int Request_Id)
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            await Task.CompletedTask;
            return entity.vw_Applications.OrderBy(x => x.Application_Score).Where(x => x.Request_Id == Request_Id).ToList();
        }

        [HttpGet]
        [Route("Application/GetApplication/{Application_Id}")]
        public async Task<cat_Applications> GetApplication(int Application_Id)
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            await Task.CompletedTask;
            return entity.cat_Applications.FirstOrDefault(x => x.Application_Id == Application_Id);
        }

        [HttpGet]
        [Route("Application/GetApplicationDocumentation/{Application_Id}")]
        public async Task<HttpResponseMessage> GetApplicationDocumentation(int Application_Id)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                string zippedPath = HttpContext.Current.Server.MapPath($"~/ApplicationFiles/Id-{Application_Id}");
                string baseDirectory = HttpContext.Current.Server.MapPath($"~/ApplicationFilesZip/Id-{Application_Id}/");
                string zipDestinationPath = baseDirectory + $"DocsId{Application_Id}.zip";

                if (!Directory.Exists(baseDirectory))
                {
                    Directory.CreateDirectory(baseDirectory);
                }
                
                if (File.Exists(zipDestinationPath))
                {
                    File.Delete(zipDestinationPath);
                }

                ZipFile.CreateFromDirectory(zippedPath, zipDestinationPath, CompressionLevel.Fastest, true);

                var dataBytes = File.ReadAllBytes(zipDestinationPath); 
                var dataStream = new MemoryStream(dataBytes);

                statusCode = HttpStatusCode.OK;
                dict.Add("path", $"ApplicationFilesZip/Id-{Application_Id}/DocsId{Application_Id}.zip");
                dict.Add("filename", $"DocsId{Application_Id}.zip");
            }
            catch (Exception ex)
            {
                dict.Add("message", ex.Message);
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpGet]
        [Route("Application/GetApplicationsExcel")]
        public async Task<HttpResponseMessage> GetApplicationsExcel()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                List<cat_Applications> Applications = entity.cat_Applications.ToList();

                statusCode = HttpStatusCode.OK;
                dict.Add("path", $"ExcelReport/report.xlsx");
                dict.Add("filename", $"report.xlsx");
            }
            catch (Exception ex)
            {
                dict.Add("message", ex.Message);
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpGet]
        [Route("Application/FixApplications")]
        public async Task<HttpResponseMessage> FixApplications()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                List<cat_Applications> Applications = entity.cat_Applications.ToList();

                foreach (var application in Applications)
                {
                    string Application_JSON_Body = application.Application_JSON_Body;

                    List<RequestModel> requestResults = JsonConvert.DeserializeObject<List<RequestModel>>(Application_JSON_Body);

                    if(requestResults.Count > 0)
                    {
                        //var badInput = requestResults.FirstOrDefault(x => x.id == 130);
                        //badInput.points.@string = "3,3,5,1";
                        //badInput.points.array = new List<string>() { "3","3","5","1" };

                        application.Application_JSON_Body = JsonConvert.SerializeObject(requestResults);
                        entity.SaveChanges();
                    }
                }

                statusCode = HttpStatusCode.OK;
                dict.Add("message", "Inputs updated");
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
        [Route("Application/FixScore/{Application_Id}")]
        public async Task<HttpResponseMessage> FixScore(int Application_Id)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var application = entity.cat_Applications.FirstOrDefault(x => x.Application_Id == Application_Id);
                string Application_JSON_Body = application.Application_JSON_Body;

                List<RequestModel> requestResults = JsonConvert.DeserializeObject<List<RequestModel>>(Application_JSON_Body);
                int score = ApplicationUtils.GetScore(requestResults);

                application.Application_Score = score;
                entity.SaveChanges();

                statusCode = HttpStatusCode.OK;
                dict.Add("message", "Score updated");
            }
            catch (Exception ex)
            {
                dict = new Dictionary<string, object>();
                dict.Add("error", ex.Message);
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpGet]
        [Route("Application/FixScores")]
        public async Task<HttpResponseMessage> FixScores()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                List<cat_Applications> Applications = entity.cat_Applications.ToList();

                foreach (var application in Applications)
                {
                    string Application_JSON_Body = application.Application_JSON_Body;

                    List<RequestModel> requestResults = JsonConvert.DeserializeObject<List<RequestModel>>(Application_JSON_Body);
                    int score = ApplicationUtils.GetScore(requestResults);

                    application.Application_Score = score;
                    entity.SaveChanges();
                }

                statusCode = HttpStatusCode.OK;
                dict.Add("message", "Scores updated");
            }
            catch (Exception ex)
            {
                dict = new Dictionary<string, object>();
                dict.Add("error", ex.Message);
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpGet]
        [Route("Application/FixCURP")]
        public async Task<HttpResponseMessage> FixCURP()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                List<cat_Applications> Applications = entity.cat_Applications.ToList();

                foreach (var application in Applications)
                {
                    string Application_JSON_Body = application.Application_JSON_Body;

                    List<RequestModel> requestResults = JsonConvert.DeserializeObject<List<RequestModel>>(Application_JSON_Body);

                    string CURP = requestResults.Where(x => x.label == "CURP").Select(x => x.answers[0]).FirstOrDefault();

                    application.Application_Applicant_CURP = CURP;
                    entity.SaveChanges();
                }

                statusCode = HttpStatusCode.OK;
                dict.Add("message", "Scores updated");
            }
            catch (Exception ex)
            {
                dict = new Dictionary<string, object>();
                dict.Add("error", ex.Message);
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }
        #endregion
    }
}
