using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.AirDCPP;
using NzbDrone.Core.Indexers.AirDCPP.Responses;

namespace NzbDrone.Core.Indexers.AirDCPP
{
    public interface IAirDCPPProxy
    {
        HttpRequest PerformSearch(AirDCPPSettings settings, string searchTerm);

        string DownloadBySearchInstanceAndResultId(AirDCPPClientSettings settings, string id, string title);

        List<QueueResult> GetQueueHistory(AirDCPPClientSettings settings);
    }
}
