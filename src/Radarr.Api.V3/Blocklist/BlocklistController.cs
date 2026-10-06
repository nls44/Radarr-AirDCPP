using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.MediaFiles;
using Radarr.Http;
using Radarr.Http.Extensions;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Blocklist
{
    [V3ApiController]
    public class BlocklistController : Controller
    {
        private readonly IBlocklistService _blocklistService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IMediaPathResolver _pathResolver;
        private readonly IConfigService _configService;

        public BlocklistController(IBlocklistService blocklistService,
                                   ICustomFormatCalculationService formatCalculator,
                                   IMediaPathResolver pathResolver,
                                   IConfigService configService)
        {
            _blocklistService = blocklistService;
            _formatCalculator = formatCalculator;
            _pathResolver = pathResolver;
            _configService = configService;
        }

        [HttpGet]
        [Produces("application/json")]
        public PagingResource<BlocklistResource> GetBlocklist([FromQuery] PagingRequestResource paging, [FromQuery] int[] movieIds = null, [FromQuery] DownloadProtocol[] protocols = null)
        {
            var pagingResource = new PagingResource<BlocklistResource>(paging);
            var pagingSpec = pagingResource.MapToPagingSpec<BlocklistResource, NzbDrone.Core.Blocklisting.Blocklist>(
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "date",
                    "indexer",
                    "languages",
                    "movieMetadata.sortTitle",
                    "quality",
                    "sourceTitle"
                },
                "date",
                SortDirection.Descending);

            if (movieIds?.Any() == true)
            {
                pagingSpec.FilterExpressions.Add(b => movieIds.Contains(b.MovieId));
            }

            if (protocols?.Any() == true)
            {
                pagingSpec.FilterExpressions.Add(b => protocols.Contains(b.Protocol));
            }

            return pagingSpec.ApplyToPage(b => _blocklistService.Paged(pagingSpec), b => BlocklistResourceMapper.MapToResource(b, _formatCalculator, _pathResolver, _configService));
        }

        [HttpGet("movie")]
        public List<BlocklistResource> GetMovieBlocklist(int movieId)
        {
            return _blocklistService.GetByMovieId(movieId).Select(h => BlocklistResourceMapper.MapToResource(h, _formatCalculator, _pathResolver, _configService)).ToList();
        }

        [RestDeleteById]
        public void DeleteBlocklist(int id)
        {
            _blocklistService.Delete(id);
        }

        [HttpDelete("bulk")]
        [Produces("application/json")]
        public object Remove([FromBody] BlocklistBulkResource resource)
        {
            _blocklistService.Delete(resource.Ids);

            return new { };
        }
    }
}
