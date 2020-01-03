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
    public class RulesPDFController : ApiController
    {
        [HttpPost]
        [Route("RulesPDF/AddPDF")]
        public async Task<HttpResponseMessage> AddPDF()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            List<string> filenames = new List<string>();

            try
            {
                string jsonString = HttpContext.Current.Request.Form["cat_Rules_PDF"];
                cat_Rules_PDF json = JsonConvert.DeserializeObject<cat_Rules_PDF>(jsonString);

                FileUtils.UploadPDF(HttpContext.Current.Request, "RulesPDF", ref statusCode, ref dict, ref filenames);

                var pdf = new cat_Rules_PDF()
                {
                    Rules_PDF_Name = json.Rules_PDF_Name,
                    Rules_PDF_Path = "RulesPDF/" + filenames[0],
                    Rules_PDF_Upload_Date = DateTime.Now
                };

                entity.cat_Rules_PDF.Add(pdf);
                entity.SaveChanges();
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
        [Route("RulesPDF/UpdatePDF")]
        public async Task<HttpResponseMessage> UpdatePDF()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            List<string> filenames = new List<string>();

            try
            {
                string jsonString = HttpContext.Current.Request.Form["cat_Rules_PDF"];
                cat_Rules_PDF json = JsonConvert.DeserializeObject<cat_Rules_PDF>(jsonString);

                var pdf = entity.cat_Rules_PDF.SingleOrDefault(x => x.Rules_PDF_Id == json.Rules_PDF_Id);

                FileUtils.ReplaceFile(pdf.Rules_PDF_Path, HttpContext.Current.Request, "RulesPDF", ref statusCode, ref dict, ref filenames);

                pdf.Rules_PDF_Name = json.Rules_PDF_Name;
                pdf.Rules_PDF_Path = "RulesPDF/" + filenames[0];
                entity.SaveChanges();
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
        [Route("RulesPDF/DeletePDF")]
        public async Task<HttpResponseMessage> DeleteSliderImage([FromBody]cat_Rules_PDF json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var pdf = entity.cat_Rules_PDF.SingleOrDefault(x => x.Rules_PDF_Id == json.Rules_PDF_Id);

                FileUtils.DeleteFile(pdf.Rules_PDF_Path);

                entity.cat_Rules_PDF.Remove(pdf);
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
        [Route("RulesPDF/GetPDFs")]
        public async Task<List<cat_Rules_PDF>> GetPDFs()
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            await Task.CompletedTask;
            return entity.cat_Rules_PDF.ToList();
        }
    }
}
