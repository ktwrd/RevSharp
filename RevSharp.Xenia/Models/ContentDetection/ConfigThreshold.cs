namespace RevSharp.Xenia.Models.ContentDetection;

public class ConfigThreshold
{
    /// <summary>
    /// Range from 0-5. Use -1 to ignore
    /// </summary>
    public int Adult { get; set; }
    /// <summary>
    /// Range from 0-5. Use -1 to ignore
    /// </summary>
    public int Spoof { get; set; }
    /// <summary>
    /// Range from 0-5. Use -1 to ignore
    /// </summary>
    public int Medical { get; set; }
    /// <summary>
    /// Range from 0-5. Use -1 to ignore
    /// </summary>
    public int Violence { get; set; }
    /// <summary>
    /// Range from 0-5. Use -1 to ignore
    /// </summary>
    public int Racy { get; set; }
}