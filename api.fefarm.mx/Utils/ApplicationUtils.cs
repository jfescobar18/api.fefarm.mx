using api.fefarm.mx.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web;
using api.fefarm.mx.Entity;
using iTextSharp.text.pdf.draw;
using System.IO.Compression;
using OfficeOpenXml;
using Newtonsoft.Json;
using OfficeOpenXml.Style;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace Utils
{
    public class ApplicationUtils
    {
        static public int GetScore(List<RequestModel> requestResults)
        {
            int finalScore = 0;

            var scoredQuestions = requestResults.Where(x => x.points.@string.Length > 0).ToList();

            string strIndex = requestResults.Where(y => y.label.Contains("cambiar su ciudad de residencia") || y.label.Contains("cambiar de residencia")).Select(x => x.answers[0]).FirstOrDefault();
            int Index = int.Parse(strIndex.Length > 0 ? strIndex : "0");

            if (Index == 1)
            {
                List<RequestModel> exclude1 = scoredQuestions.Where(x => x.label == "La casa donde vive actualmente es:" || x.label == "La casa donde vive actualmente es").ToList();
                List<RequestModel> exclude2 = scoredQuestions.Where(x => x.label == "Tipo de vivienda:" || x.label == "Tipo de vivienda").ToList();
                List<RequestModel> exclude3 = scoredQuestions.Where(x => x.label == "La vivienda cuenta con los siguientes servicios:" || x.label == "La vivienda cuenta con los siguientes servicios").ToList();

                if(exclude1.Count > 0)
                {
                    scoredQuestions = scoredQuestions.Where(x => x.id != exclude1[0].id).ToList();
                }
                if (exclude2.Count > 0)
                {
                    scoredQuestions = scoredQuestions.Where(x => x.id != exclude2[1].id).ToList();
                }
                if (exclude3.Count > 0)
                {
                    scoredQuestions = scoredQuestions.Where(x => x.id != exclude3[1].id).ToList();
                }
            }

            var prevChecks = new List<bool>();

            foreach (var question in scoredQuestions)
            {
                try
                {
                    switch ((InputTypes)int.Parse(question.type))
                    {
                        case InputTypes.Dropdown:
                            int scoreIndex = int.Parse(question.answers[0]);
                            finalScore += int.Parse(question.points.array[scoreIndex]);
                            break;
                        case InputTypes.Checklist:
                            for (int i = 0; i < question.answers.Count; i++)
                            {
                                if (question.answers[i] == "true")
                                {
                                    finalScore += int.Parse(question.points.array[i]);
                                }
                            }
                            break;
                        case InputTypes.Check:
                            if (question.answers[0] == "true")
                            {
                                finalScore += int.Parse(question.points.array[0]);
                            }

                            if (question.label == "Discapacidad" && question.answers[0] == "false")
                            {
                                prevChecks.Add(false);
                            }
                            else if (question.label == "Trastornos mentales" && question.answers[0] == "false")
                            {
                                prevChecks.Add(false);
                            }
                            else if (question.label == "Trastornos alimenticios" && question.answers[0] == "false")
                            {
                                prevChecks.Add(false);
                            }
                            else if (question.label == "No aplica" && question.answers[0] == "false" && prevChecks.Count == 3)
                            {
                                finalScore += int.Parse(question.points.array[0]);
                            }

                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Logs.AddLog(ex.Message);
                }

            }
            return finalScore;
        }

        static public void CreateReport(List<RequestModel> requestResults, int Application_Id)
        {
            string filePath = HttpContext.Current.Server.MapPath($"~/PDF-Applications/ApplicationId-{Application_Id}/");
            string fileName = "application.pdf";

            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                if (File.Exists(filePath + fileName))
                {
                    File.Delete(filePath + fileName);
                }

                using (FileStream fs = new FileStream(filePath + fileName, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4, 30, 30, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);

                    document.AddAuthor("FEFARM");
                    document.AddCreator("FEFARM");
                    document.AddKeywords("FEFARM, Reporte de solicitud");
                    document.AddSubject("Reporte de solicitud");
                    document.AddTitle("Reporte de solicitud");
                    document.Open();

                    string profileImagePath = Directory.GetFiles(HttpContext.Current.Server.MapPath($"~/ApplicationFiles/Id-{Application_Id}"), "*.*", SearchOption.AllDirectories)
                        .Where(file => new string[] { ".jpg", ".JPG", ".jpeg", ".JPEG",".gif", ".GIF",".png", ".PNG" }
                        .Contains(Path.GetExtension(file)))
                        .FirstOrDefault();

                    Image img = Image.GetInstance(profileImagePath ?? HttpContext.Current.Server.MapPath("~/img/profile.png"));
                    // var scalePercent = (((document.PageSize.Width / img.Width) * 100) - 15);
                    img.ScalePercent(11f);
                    img.Border = 0;

                    PdfPTable table = new PdfPTable(1);
                    table.DefaultCell.Border = Rectangle.NO_BORDER;
                    table.AddCell(new PdfPCell(img) { BorderWidthBottom = 0, BorderWidthLeft = 0, BorderWidthTop = 0,BorderWidthRight = 0, PaddingLeft = -30});
                    document.Add(table);
                    document.Add(new Paragraph(" "));

                    foreach (var question in requestResults)
                    {
                        try
                        {
                            switch ((InputTypes)int.Parse(question.type))
                            {
                                case InputTypes.Text:
                                case InputTypes.Date:
                                    string textAnswer = question.answers[0];

                                    Font f = new Font();
                                    f.SetStyle(Font.BOLD);
                                    Paragraph p = new Paragraph(question.label, f);
                                    p.Alignment = Element.ALIGN_LEFT;
                                    document.Add(p);

                                    f = new Font();
                                    f.SetStyle(Font.NORMAL);
                                    p = new Paragraph(textAnswer, f);
                                    p.Alignment = Element.ALIGN_LEFT;
                                    document.Add(p);
                                    break;
                                case InputTypes.Textarea:
                                    string textareaAnswer = question.answers[0];

                                    f = new Font();
                                    f.SetStyle(Font.BOLD);
                                    p = new Paragraph(question.label, f);
                                    p.Alignment = Element.ALIGN_LEFT;
                                    document.Add(p);

                                    f = new Font();
                                    f.SetStyle(Font.NORMAL);
                                    p = new Paragraph(textareaAnswer, f);
                                    p.Alignment = Element.ALIGN_LEFT;
                                    document.Add(p);
                                    break;
                                case InputTypes.Dropdown:
                                    int answerIndex = int.Parse(question.answers[0]);
                                    string dropdownAnswer = (string)question.values.array[answerIndex];

                                    f = new Font();
                                    f.SetStyle(Font.BOLD);
                                    p = new Paragraph(question.label, f);
                                    p.Alignment = Element.ALIGN_LEFT;
                                    document.Add(p);

                                    f = new Font();
                                    f.SetStyle(Font.NORMAL);
                                    p = new Paragraph(dropdownAnswer, f);
                                    p.Alignment = Element.ALIGN_LEFT;
                                    document.Add(p);
                                    break;
                                case InputTypes.Checklist:
                                    for (int i = 0; i < question.answers.Count; i++)
                                    {
                                        f = new Font();
                                        f.SetStyle(Font.BOLD);
                                        p = new Paragraph(question.label, f);
                                        p.Alignment = Element.ALIGN_LEFT;
                                        document.Add(p);

                                        f = new Font();
                                        f.SetStyle(Font.NORMAL);

                                        if (question.answers[i] == "true")
                                        {
                                            f.SetColor(23, 197, 166);
                                        }
                                        else
                                        {
                                            f.SetColor(197, 23, 54);
                                        }

                                        p = new Paragraph("- " + (string)question.values.array[i], f);
                                        p.Alignment = Element.ALIGN_LEFT;
                                        document.Add(p);
                                    }
                                    break;
                                case InputTypes.Check:
                                    f = new Font();
                                    f.SetStyle(Font.BOLD);

                                    if (question.answers[0] == "true")
                                    {
                                        f.SetColor(23, 197, 166);
                                    }
                                    else
                                    {
                                        f.SetColor(197, 23, 54);
                                    }

                                    p = new Paragraph(question.label, f);
                                    p.Alignment = Element.ALIGN_LEFT;
                                    document.Add(p);
                                    break;
                                case InputTypes.Title:
                                    f = new Font();
                                    f.SetStyle(Font.BOLD);
                                    f.Size = 18;
                                    p = new Paragraph(question.label, f);
                                    p.Alignment = Element.ALIGN_LEFT;
                                    document.Add(p);
                                    break;
                                case InputTypes.Subtitle:
                                    f = new Font();
                                    f.SetStyle(Font.BOLD);
                                    f.Size = 15;
                                    p = new Paragraph(question.label, f);
                                    p.Alignment = Element.ALIGN_LEFT;
                                    document.Add(p);
                                    break;
                                case InputTypes.Small:
                                    f = new Font();
                                    f.SetStyle(Font.NORMAL);
                                    f.Size = 10;
                                    p = new Paragraph(question.label, f);
                                    p.Alignment = Element.ALIGN_LEFT;
                                    document.Add(p);
                                    break;
                            }

                            document.Add(new Paragraph(" "));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                    Logs.AddLog(ex.Message);
                        }
                    }

                    Chunk linebreak = new Chunk(new LineSeparator(4f, 00f, new BaseColor(210, 210, 210), Element.ALIGN_CENTER, -1));
                    document.Add(linebreak);

                    document.Add(new Paragraph("Nombre y firma del padre o tutor"));
                    Image @checked = Image.GetInstance(HttpContext.Current.Server.MapPath("~/img/checked.png"));
                    @checked.ScalePercent(12f);
                    document.Add(@checked);

                    document.Add(new Paragraph(" "));

                    document.Add(new Paragraph("Nombre y firma del aspirante"));
                    document.Add(@checked);

                    document.Close();
                    writer.Close();
                    fs.Close();
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                    Logs.AddLog(ex.Message);
            }
        }

        static public void CreateExcel(List<cat_Applications> applications, int Request_Id)
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

            string filePath = HttpContext.Current.Server.MapPath($"~/ExcelReport/");
            string fileName = "report.xlsx";

            var request = entity.cat_Requests.FirstOrDefault(x => x.Request_Id == Request_Id);
            List<RequestModel> requestContent = JsonConvert.DeserializeObject<List<RequestModel>>(request.Request_JSON_Body);

            requestContent = requestContent.Where(x => int.Parse(x.type) != (int)InputTypes.Title &&
                                                        int.Parse(x.type) != (int)InputTypes.Subtitle &&
                                                        int.Parse(x.type) != (int)InputTypes.Small &&
                                                        int.Parse(x.type) != (int)InputTypes.Paragraph).ToList();

            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                if (File.Exists(filePath + fileName))
                {
                    File.Delete(filePath + fileName);
                }

                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    excelPackage.Workbook.Properties.Author = "FEFARM";
                    excelPackage.Workbook.Properties.Title = "Reporte de solicitudes";
                    excelPackage.Workbook.Properties.Subject = $"Solicitud: {request.Request_Name}";
                    excelPackage.Workbook.Properties.Created = DateTime.Now;

                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Resultados");

                    worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Row(1).Style.Font.Bold = true;
                    worksheet.Row(1).Height = 23;

                    worksheet.Cells[1, 1].Style.Font.Name = "Calibri";
                    worksheet.Cells[1, 1].Value = "Puntaje Obtenido";
                    worksheet.Cells[1, 1].AutoFitColumns();

                    for (int i = 0; i < requestContent.Count; i++)
                    {
                        worksheet.Cells[1, i + 2].Style.Font.Name = "Calibri";
                        worksheet.Cells[1, i + 2].Value = requestContent[i].label;
                        worksheet.Cells[1, i + 2].AutoFitColumns();
                    }

                    int Row = 2;

                    foreach (var application in applications)
                    {
                        string Application_JSON_Body = application.Application_JSON_Body;

                        List<RequestModel> requestResults = JsonConvert.DeserializeObject<List<RequestModel>>(Application_JSON_Body);

                        requestResults = requestResults.Where(x => int.Parse(x.type) != (int)InputTypes.Title &&
                                                        int.Parse(x.type) != (int)InputTypes.Subtitle &&
                                                        int.Parse(x.type) != (int)InputTypes.Small &&
                                                        int.Parse(x.type) != (int)InputTypes.Paragraph).ToList();

                        worksheet.Cells[Row, 1].Style.Font.Name = "Calibri";
                        worksheet.Cells[Row, 1].Value = application.Application_Score;
                        worksheet.Cells[Row, 1].Style.Numberformat.Format = "#";

                        for (int i = 0; i < requestResults.Count; i++)
                        {
                            try
                            {
                                switch ((InputTypes)int.Parse(requestResults[i].type))
                                {
                                    case InputTypes.Text:
                                        string textAnswer = requestResults[i].answers[0];
                                        worksheet.Cells[Row, i + 2].Style.Font.Name = "Calibri";
                                        worksheet.Cells[Row, i + 2].Value = textAnswer;
                                        worksheet.Cells[Row, i + 2].Style.Numberformat.Format = "@";
                                        break;
                                    case InputTypes.Date:
                                        string dateAnswer = requestResults[i].answers[0];
                                        worksheet.Cells[Row, i + 2].Style.Font.Name = "Calibri";
                                        worksheet.Cells[Row, i + 2].Value = dateAnswer;
                                        worksheet.Cells[Row, i + 2].Style.Numberformat.Format = "dd-MM-yyyy";
                                        break;
                                    case InputTypes.Textarea:
                                        string textareaAnswer = requestResults[i].answers[0];
                                        worksheet.Cells[Row, i + 2].Style.Font.Name = "Calibri";
                                        worksheet.Cells[Row, i + 2].Value = textareaAnswer;
                                        worksheet.Cells[Row, i + 2].Style.Numberformat.Format = "@";
                                        break;
                                    case InputTypes.Dropdown:
                                        int answerIndex = int.Parse(requestResults[i].answers[0]);
                                        string dropdownAnswer = (string)requestResults[i].values.array[answerIndex];
                                        worksheet.Cells[Row, i + 2].Style.Font.Name = "Calibri";
                                        worksheet.Cells[Row, i + 2].Value = dropdownAnswer.Replace(";", ",");
                                        worksheet.Cells[Row, i + 2].Style.Numberformat.Format = "@";
                                        break;
                                    case InputTypes.Checklist:
                                        for (int j = 0; j < requestResults[i].answers.Count; j++)
                                        {
                                            worksheet.Cells[Row, i + 2].Style.Font.Name = "Calibri";

                                            if (requestResults[i].answers[j] == "true")
                                            {
                                                worksheet.Cells[Row, i + 2].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(23, 197, 166));
                                            }
                                            else
                                            {
                                                worksheet.Cells[Row, i + 2].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(197, 23, 54));
                                            }
                                            worksheet.Cells[Row, i + 2].RichText.Add("- " + (string)requestResults[i].values.array[j] + "\r\n");
                                        }
                                        break;
                                    case InputTypes.Check:

                                        worksheet.Cells[Row, i + 2].Style.Font.Name = "Calibri";
                                        if (requestResults[i].answers[0] == "true")
                                        {
                                            worksheet.Cells[Row, i + 2].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(23, 197, 166));
                                        }
                                        else
                                        {
                                            worksheet.Cells[Row, i + 2].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(197, 23, 54));
                                        }
                                        worksheet.Cells[Row, i + 2].Value = requestResults[i].label;
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                    Logs.AddLog(ex.Message);
                            }
                        }

                        Row++;
                    }

                    FileInfo fi = new FileInfo(filePath + fileName);
                    excelPackage.SaveAs(fi);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                    Logs.AddLog(ex.Message);
            }
        }

        static public void CreateZipFile(int Application_Id)
        {
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

                ZipFile.CreateFromDirectory(zippedPath, zipDestinationPath, System.IO.Compression.CompressionLevel.Fastest, true);

                var dataBytes = File.ReadAllBytes(zipDestinationPath);
                var dataStream = new MemoryStream(dataBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                    Logs.AddLog(ex.Message);
            }
        }
    }
}