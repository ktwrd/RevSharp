using RevSharp.Core.Models;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.AdminPanel.Models;

public class CDAdminDetailModel : BaseModel
{
    public Server Server { get; set; }
    public AnalysisServerConfig Config { get; set; }
}