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
    [System.Web.Mvc.SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EvidencesPDFController : ApiController
    {
        [HttpPost]
        [Route("EvidencesPDF/AddPDF")]
        public async Task<HttpResponseMessage> AddPDF()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            List<string> filenames = new List<string>();

            try
            {
                string jsonString = HttpContext.Current.Request.Form["cat_Evidences_PDF"];
                cat_Evidences_PDF json = JsonConvert.DeserializeObject<cat_Evidences_PDF>(jsonString);

                FileUtils.UploadFiles(HttpContext.Current.Request, "~/PDFs/EvidencesPDF", ref statusCode, ref dict, ref filenames);

                var pdf = new cat_Evidences_PDF()
                {
                    Evidences_PDF_Name = json.Evidences_PDF_Name,
                    Evidences_PDF_Path = "PDFs/EvidencesPDF/" + filenames[0],
                    Evidences_PDF_Upload_Date = DateTime.Now
                };

                entity.cat_Evidences_PDF.Add(pdf);
                entity.SaveChanges();
            }
            catch (Exception ex)
            {
                if (dict.Keys.Count > 0)
                {
                    dict = new Dictionary<string, object>();
                    dict.Add("message", ex.Message);
                    Logs.AddLog(ex.Message);
                }
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpPost]
        [Route("EvidencesPDF/UpdatePDF")]
        public async Task<HttpResponseMessage> UpdatePDF()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            List<string> filenames = new List<string>();

            try
            {
                string jsonString = HttpContext.Current.Request.Form["cat_Evidences_PDF"];
                cat_Evidences_PDF json = JsonConvert.DeserializeObject<cat_Evidences_PDF>(jsonString);

                var pdf = entity.cat_Evidences_PDF.SingleOrDefault(x => x.Evidences_PDF_Id == json.Evidences_PDF_Id);

                if (HttpContext.Current.Request.Files.Count > 0)
                {
                    FileUtils.ReplaceFile(pdf.Evidences_PDF_Path, HttpContext.Current.Request, "~/PDFs/EvidencesPDF", ref statusCode, ref dict, ref filenames);
                    pdf.Evidences_PDF_Path = "PDFs/EvidencesPDF/" + filenames[0];
                }

                pdf.Evidences_PDF_Name = json.Evidences_PDF_Name;
                entity.SaveChanges();

                statusCode = HttpStatusCode.OK;
                if (dict.Keys.Count == 0)
                {
                    dict.Add("message", "PDF updated successfully");
                }
            }
            catch (Exception ex)
            {
                if (dict.Keys.Count > 0)
                {
                    dict = new Dictionary<string, object>();
                    dict.Add("message", ex.Message);
                    Logs.AddLog(ex.Message);
                }
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpPost]
        [Route("EvidencesPDF/DeletePDF")]
        public async Task<HttpResponseMessage> DeleteSliderImage([FromBody]cat_Evidences_PDF json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var pdf = entity.cat_Evidences_PDF.SingleOrDefault(x => x.Evidences_PDF_Id == json.Evidences_PDF_Id);

                FileUtils.DeleteFile(pdf.Evidences_PDF_Path);

                entity.cat_Evidences_PDF.Remove(pdf);
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
                    Logs.AddLog(ex.Message);
                }
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpGet]
        [Route("EvidencesPDF/GetPDFs")]
        public async Task<List<cat_Evidences_PDF>> GetPDFs()
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            await Task.CompletedTask;
            return entity.cat_Evidences_PDF.ToList();
        }
    }
}
