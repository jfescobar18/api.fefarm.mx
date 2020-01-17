﻿using api.fefarm.mx.Entity;
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
    public class NewsPDFController : ApiController
    {
        [HttpPost]
        [Route("NewsPDF/AddPDF")]
        public async Task<HttpResponseMessage> AddPDF()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            List<string> filenames = new List<string>();

            try
            {
                string jsonString = HttpContext.Current.Request.Form["cat_News_PDF"];
                cat_News_PDF json = JsonConvert.DeserializeObject<cat_News_PDF>(jsonString);

                FileUtils.UploadFiles(HttpContext.Current.Request, "~/PDFs/NewsPDF", ref statusCode, ref dict, ref filenames);

                var pdf = new cat_News_PDF()
                {
                    New_PDF_Name = json.New_PDF_Name,
                    New_PDF_Path = "PDFs/NewsPDF/" + filenames[0],
                    New_PDF_Upload_Date = DateTime.Now
                };

                entity.cat_News_PDF.Add(pdf);
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
        [Route("NewsPDF/UpdatePDF")]
        public async Task<HttpResponseMessage> UpdatePDF()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            List<string> filenames = new List<string>();

            try
            {
                string jsonString = HttpContext.Current.Request.Form["cat_News_PDF"];
                cat_News_PDF json = JsonConvert.DeserializeObject<cat_News_PDF>(jsonString);

                var pdf = entity.cat_News_PDF.SingleOrDefault(x => x.New_PDF_Id == json.New_PDF_Id);

                if (HttpContext.Current.Request.Files.Count > 0)
                {
                    FileUtils.ReplaceFile(pdf.New_PDF_Path, HttpContext.Current.Request, "~/PDFs/NewsPDF", ref statusCode, ref dict, ref filenames);
                    pdf.New_PDF_Path = "PDFs/NewsPDF/" + filenames[0];
                }

                pdf.New_PDF_Name = json.New_PDF_Name;
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
        [Route("NewsPDF/DeletePDF")]
        public async Task<HttpResponseMessage> DeleteSliderImage([FromBody]cat_News_PDF json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities entity = new CMS_fefarmEntities();
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                var pdf = entity.cat_News_PDF.SingleOrDefault(x => x.New_PDF_Id == json.New_PDF_Id);

                FileUtils.DeleteFile(pdf.New_PDF_Path);

                entity.cat_News_PDF.Remove(pdf);
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
        [Route("NewsPDF/GetPDFs")]
        public async Task<List<cat_News_PDF>> GetPDFs()
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            await Task.CompletedTask;
            return entity.cat_News_PDF.ToList();
        }
    }
}