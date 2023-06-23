using System.Security.Principal;
using Google.Cloud.Vision.V1;

namespace RevSharp.Xenia.Models.ContentDetection;

public class AnalysisResult
{
    public decimal Adult { get; set; }
    public decimal Spoof { get; set; }
    public decimal Medical { get; set; }
    public decimal Violence { get; set; }
    public decimal Racy { get; set; }
    public decimal AdultAverage { get; set; }
    public decimal SpoofAverage { get; set; }
    public decimal MedicalAverage { get; set; }
    public decimal ViolenceAverage { get; set; }
    public decimal RacyAverage { get; set; }
    public decimal Total { get; set; }
    public decimal Average { get; set; }
    public List<(SafeSearchAnnotation, string)> Annotations { get; set; }
    public void AddAnnotation(SafeSearchAnnotation annotation, string tag)
    {
        Annotations.Add((annotation, tag));
        UpdateData();
    }

    public void UpdateData()
    {
        decimal adult = 0;
        decimal spoof = 0;
        decimal medical = 0;
        decimal violence = 0;
        decimal racy = 0;

        var workingAnnotations = Annotations.ToArray();
        foreach (var (item, note) in workingAnnotations)
        {
            adult += (int)item.Adult;
            spoof += (int)item.Spoof;
            medical += (int)item.Medical;
            violence += (int)item.Violence;
            racy += (int)item.Racy;
        }

        decimal total = new decimal[]
        {
            adult,
            spoof,
            medical,
            violence,
            racy
        }.Sum();
        
        #region Average Calculation
        decimal avg_adult = adult / workingAnnotations.Length;
        decimal avg_spoof = spoof / workingAnnotations.Length;
        decimal avg_medical = medical / workingAnnotations.Length;
        decimal avg_violence = violence / workingAnnotations.Length;
        decimal avg_racy = racy / workingAnnotations.Length;
        
        decimal[] average_arr = new decimal[]
        {
            avg_adult,
            avg_spoof,
            avg_medical,
            avg_violence,
            avg_racy
        };
        decimal average = average_arr.Sum() / average_arr.Length;
        #endregion

        Adult = adult;
        Spoof = spoof;
        Medical = medical;
        Violence = violence;
        Racy = racy;
        AdultAverage = avg_adult;
        SpoofAverage = avg_spoof;
        MedicalAverage = avg_medical;
        ViolenceAverage = avg_violence;
        RacyAverage = avg_racy;
        Total = total;
        Average = average;
    }
    
    public AnalysisResult()
    {
        Annotations = new List<(SafeSearchAnnotation, string)>();
    }
}