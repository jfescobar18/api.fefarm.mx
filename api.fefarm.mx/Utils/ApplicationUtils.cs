using api.fefarm.mx.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Utils
{
    public class ApplicationUtils
    {
        static public int GetScore(List<RequestModel> requestResults)
        {
            int finalScore = 0;
            var scoredQuestions = requestResults.Where(x => x.points.@string.Length > 0).ToList();

            int Index = int.Parse(requestResults.Where(y => y.label.Contains("cambiar su ciudad de residencia")).Select(x => x.answers[0]).FirstOrDefault());

            if(Index == 1)
            {
                string[] exlcudedLabels = new string[] { "La casa donde vive actualmente es:", "Tipo de vivienda:", "La vivienda cuenta con los siguientes servicios:" };
                scoredQuestions = scoredQuestions.Where(x => !exlcudedLabels.Contains(x.label) && x.id > 140).ToList();
            }

            foreach (var question in scoredQuestions)
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
                        break;
                }
            }

            return finalScore;
        }

        static public string CreateReport(List<RequestModel> requestResults)
        {
            string path = string.Empty;
            return path;
        }
    }
}