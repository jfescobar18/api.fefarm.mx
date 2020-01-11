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
    public class RequirementsPDFController : ApiController
    {
        [HttpPost]
        [Route("RequirementsPDF/AddPDF")]
        public async Task<HttpResponseMessage> AddPDF()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            List<string> filenames = new List<string>();

            try
            {
                string jsonString = HttpContext.Current.Request.Form["cat_Requirements_PDF"];
                cat_Requirements_PDF json = JsonConvert.DeserializeObject<cat_Requirements_PDF>(jsonString);

                FileUtils.UploadFiles(HttpContext.Current.Request, "~/PDFs/RequirementsPDF", ref statusCode, ref dict, ref filenames);

                var pdf = new cat_Requirements_PDF()
                {
                    Requirements_PDF_Name = json.Requirements_PDF_Name,
                    Requirements_PDF_Path = "PDFs/RequirementsPDF/" + filenames[0],
                    Requirements_PDF_Upload_Date = DateTime.Now
                };

                entity.cat_Requirements_PDF.Add(pdf);
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
        [Route("RequirementsPDF/UpdatePDF")]
        public async Task<HttpResponseMessage> UpdatePDF()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            List<string> filenames = new List<string>();

            try
            {
                string jsonString = HttpContext.Current.Request.Form["cat_Requirements_PDF"];
                cat_Requirements_PDF json = JsonConvert.DeserializeObject<cat_Requirements_PDF>(jsonString);

                var pdf = entity.cat_Requirements_PDF.SingleOrDefault(x => x.Requirements_PDF_Id == json.Requirements_PDF_Id);

                if (HttpContext.Current.Request.Files.Count > 0)
                {
                    FileUtils.ReplaceFile(pdf.Requirements_PDF_Path, HttpContext.Current.Request, "~/PDFs/RequirementsPDF", ref statusCode, ref dict, ref filenames);
                    pdf.Requirements_PDF_Path = "PDFs/RequirementsPDF/" + filenames[0];
                }

                pdf.Requirements_PDF_Name = json.Requirements_PDF_Name;
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
                }
            }

            await Task.CompletedTask;
            return Request.CreateResponse(statusCode, dict);
        }

        [HttpPost]
        [Route("RequirementsPDF/DeletePDF")]
        public async Task<HttpResponseMessage> DeleteSliderImage([FromBody]cat_Requirements_PDF json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var pdf = entity.cat_Requirements_PDF.SingleOrDefault(x => x.Requirements_PDF_Id == json.Requirements_PDF_Id);

                FileUtils.DeleteFile(pdf.Requirements_PDF_Path);

                entity.cat_Requirements_PDF.Remove(pdf);
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
        [Route("RequirementsPDF/GetPDFs")]
        public async Task<List<cat_Requirements_PDF>> GetPDFs()
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            await Task.CompletedTask;
            return entity.cat_Requirements_PDF.ToList();
        }
    }
}
