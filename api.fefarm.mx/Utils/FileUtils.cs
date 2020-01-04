using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;

namespace Utils
{
    public class FileUtils
    {
        public static void UploadPDF(HttpRequest httpRequest, string folder, ref HttpStatusCode statusCode, ref Dictionary<string, object> dict, ref List<string> filenames)
        {
            try
            {
                if (!Directory.Exists(HttpContext.Current.Server.MapPath($"~/PDFs/{folder}")))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath($"~/PDFs/{folder}"));
                }

                if (httpRequest.Files.Count > 0)
                {
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            int MaxContentLength = 1024 * 1024 * 10;
                            IList<string> AllowedFileExtensions = new List<string> { ".pdf" };
                            string ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.')),
                            extension = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(extension))
                            {
                                dict.Add("message", "Please upload files of type .pdf");
                                statusCode = HttpStatusCode.BadRequest;
                            }
                            else if (postedFile.ContentLength > MaxContentLength)
                            {
                                dict.Add("message", "Please upload a PDF upto 10 mb");
                                statusCode = HttpStatusCode.BadRequest;
                            }
                            else
                            {
                                postedFile.SaveAs(HttpContext.Current.Server.MapPath($"~/PDFs/{folder}/" + postedFile.FileName));
                                filenames.Add(postedFile.FileName);
                                dict.Add("message", "PDF updated Successfully");
                                statusCode = HttpStatusCode.OK;
                            }
                        }
                    }
                }
                else
                {
                    dict.Add("message", "Please upload a PDF");
                    statusCode = HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                dict.Add("message", ex.Message);
                statusCode = HttpStatusCode.BadRequest;
            }
        }

        public static void DeleteFile(string path)
        {
            try
            {
                string fullPath = HttpContext.Current.Server.MapPath("~/" + path);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (IOException ioExp)
            {
                throw ioExp;
            }
        }

        public static void ReplaceFile(string path, HttpRequest httpRequest, string folder, ref HttpStatusCode statusCode, ref Dictionary<string, object> dict, ref List<string> filenames)
        {
            DeleteFile(path);
            UploadPDF(httpRequest, folder, ref statusCode, ref dict, ref filenames);
        }
    }
}