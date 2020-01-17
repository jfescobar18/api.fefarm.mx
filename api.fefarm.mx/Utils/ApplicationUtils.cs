using api.fefarm.mx.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web;
using api.fefarm.mx.Entity;
using Newtonsoft.Json;
using System.Text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf.draw;

namespace Utils
{
    public class ApplicationUtils
    {
        static public int GetScore(List<RequestModel> requestResults)
        {
            int finalScore = 0;

            var scoredQuestions = requestResults.Where(x => x.points.@string.Length > 0).ToList();

            int Index = int.Parse(requestResults.Where(y => y.label.Contains("cambiar su ciudad de residencia")).Select(x => x.answers[0]).FirstOrDefault());

            if (Index == 1)
            {
                List<RequestModel> exclude1 = scoredQuestions.Where(x => x.label == "La casa donde vive actualmente es:").ToList();
                List<RequestModel> exclude2 = scoredQuestions.Where(x => x.label == "Tipo de vivienda:").ToList();
                List<RequestModel> exclude3 = scoredQuestions.Where(x => x.label == "La vivienda cuenta con los siguientes servicios:").ToList();

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
                }

            }

            return finalScore;
        }

        static public void CreateReport(List<RequestModel> requestResults, int Application_Id)
        {
            CMS_fefarmEntities entity = new CMS_fefarmEntities();

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
            }
        }

        private static StringBuilder CreateHtmlTemplate(List<RequestModel> requestResults, int Application_Id)
        {
            StringBuilder sb = new StringBuilder();

            string profileImagePath = Directory.GetFiles(HttpContext.Current.Server.MapPath($"~/ApplicationFiles/Id-{Application_Id}"), "*.*", SearchOption.AllDirectories)
                .Where(file => new string[] { ".jpg", ".jpeg", ".gif", ".png" }
                .Contains(Path.GetExtension(file)))
                .FirstOrDefault();

            sb.Append("<div style='display: block;padding: 4px;margin-bottom: 20px;line-height: 1.42857143;background-color: #fff;border: 1px solid #ddd;border-radius: 4px;'>");
            sb.Append("<div style='float: left;position: relative;min-height: 1px;padding-right: 15px;padding-left: 15px;padding-top: 1.25rem;width: 16.66666667%;margin-left: 83.33333333%;'>");
            sb.Append($"<img src='{profileImagePath}' alt='profile' class='profile-picture' />");
            sb.Append($"</div>");
            sb.Append($"<div class='col-md-2 col-md-offset-10 pt-5'><img src='{profileImagePath}' alt='profile' class='profile-picture' /></div>");

            

            sb.Append($"</div>");

            return sb;
        }

        private static string GetColWidth(int size)
        {
            float totalWidth = (100 / 12) * size;
            return totalWidth.ToString().Length > 5 ? totalWidth.ToString().Substring(0, 4) : totalWidth.ToString();
        }
    }
}