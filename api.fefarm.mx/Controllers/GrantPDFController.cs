using api.fefarm.mx.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace api.fefarm.mx.Controllers
{
    public class GrantPDFController : ApiController
    {
        [HttpPost]
        [Route("GrantPDF/AddGrantPDF/{Grant_PDF_Name}")]
        public async Task<HttpResponseMessage> AddGrantPDF(string Grant_PDF_Name)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities context = new CMS_fefarmEntities();
            
            try
            {
                var httpRequest = HttpContext.Current.Request;
                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                    var postedFile = httpRequest.Files[file];
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        int MaxContentLength = 1024 * 1024 * 10;
                        IList<string> AllowedFileExtensions = new List<string> { ".pdf" };
                        string ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.')),
                        extension = ext.ToLower();
                        if (!AllowedFileExtensions.Contains(extension))
                        {
                            dict.Add("message", "Please Upload files of type .pdf");
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {
                            dict.Add("message", "Please Upload a PDF upto 10 mb");
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else
                        {
                            postedFile.SaveAs(HttpContext.Current.Server.MapPath("~/PDFs/" + postedFile.FileName));

                            var pdf = new cat_Grant_PDF
                            {
                                Grant_PDF_Name = Grant_PDF_Name,
                                Grant_PDF_Path = "PDFs/" + postedFile.FileName,
                                Grant_PDF_Upload_Date = DateTime.Now
                            };

                            context.cat_Grant_PDF.Add(pdf);
                            context.SaveChanges();
                        }
                    }

                    dict.Add("message", "PDF Created Successfully");
                    return Request.CreateResponse(HttpStatusCode.Created, dict);
                }
                dict.Add("message", "Please Upload a PDF");
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
            catch (Exception ex)
            {
                dict.Add("message", ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, dict);
            }
        }

        [HttpPost]
        [Route("GrantPDF/DeleteGrantPDF/{Grant_PDF_Id}")]
        public async Task<HttpResponseMessage> DeleteGrantPDF(int Grant_PDF_Id)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities context = new CMS_fefarmEntities();
            
            try
            {
                var pdf = context.cat_Grant_PDF.SingleOrDefault(x => x.Grant_PDF_Id == Grant_PDF_Id);
                context.cat_Grant_PDF.Attach(pdf);
                context.cat_Grant_PDF.Remove(pdf);
                context.SaveChanges();

                dict.Add("message", "PDF Deleted Successfully");
                return Request.CreateResponse(HttpStatusCode.OK, dict);
            }
            catch (Exception ex)
            {
                dict.Add("message", ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, dict);
            }
        }

        [HttpPost]
        [Route("GrantPDF/UpdateGrantPDF/{Grant_PDF_Id}/{Grant_PDF_Name}")]
        public async Task<HttpResponseMessage> UpdateGrantPDF(int Grant_PDF_Id, string Grant_PDF_Name)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            CMS_fefarmEntities context = new CMS_fefarmEntities();

            try
            {
                var pdf = context.cat_Grant_PDF.SingleOrDefault(x => x.Grant_PDF_Id == Grant_PDF_Id);
                var httpRequest = HttpContext.Current.Request;

                if(httpRequest.Files.Count > 0)
                {
                    foreach (string file in httpRequest.Files)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                        var postedFile = httpRequest.Files[file];
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            int MaxContentLength = 1024 * 1024 * 10;
                            IList<string> AllowedFileExtensions = new List<string> { ".pdf" };
                            string ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.')),
                            extension = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(extension))
                            {
                                dict.Add("message", "Please Upload files of type .pdf");
                                return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                            }
                            else if (postedFile.ContentLength > MaxContentLength)
                            {
                                dict.Add("message", "Please Upload a PDF upto 10 mb");
                                return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                            }
                            else
                            {
                                postedFile.SaveAs(HttpContext.Current.Server.MapPath("~/PDFs/" + postedFile.FileName));

                                pdf.Grant_PDF_Name = Grant_PDF_Name;
                                pdf.Grant_PDF_Path = "PDFs/" + postedFile.FileName;
                                context.SaveChanges();
                            }
                        }
                    }
                }
                else
                {
                    pdf.Grant_PDF_Name = Grant_PDF_Name;
                    context.SaveChanges();
                }

                dict.Add("message", "PDF Updated Successfully");
                return Request.CreateResponse(HttpStatusCode.Created, dict);
            }
            catch (Exception ex)
            {
                dict.Add("message", ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, dict);
            }
        }

        [HttpGet]
        [Route ("GrantPDF/GetGrantPDFs")]
        public List<cat_Grant_PDF> GetGrantPDFs ()
        {
            CMS_fefarmEntities context = new CMS_fefarmEntities();

            return context.cat_Grant_PDF.ToList();
        }
    }
}
