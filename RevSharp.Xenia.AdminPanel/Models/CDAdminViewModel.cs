using RevSharp.Core.Models;
using RevSharp.Xenia.AdminPanel.Controllers;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.AdminPanel.Models;

public class CDAdminViewModel : BaseModel
{
    private readonly CDAdminController _controller;
    public CDAdminViewModel(CDAdminController controller)
    {
        _controller = controller;
    }
    public List<AnalysisServerConfig> ServerConfigs { get; set; }
    public AnalysisServerConfig[] EnabledServers => ServerConfigs.Where(v => v.Enabled).ToArray();

}