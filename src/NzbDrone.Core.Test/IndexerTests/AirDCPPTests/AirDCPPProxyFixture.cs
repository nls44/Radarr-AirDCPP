using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.AirDCPP;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.AirDCPPTests
{
    [TestFixture]
    public class AirDCPPProxyFixture : CoreTest<AirDCPPProxy>
    {
        [Test]
        public void should_remove_diacritics_in_hub_search_pattern()
        {
            var requests = new List<HttpRequest>();
            var settings = new AirDCPPSettings
            {
                BaseUrl = "http://localhost:3121/",
                Username = "username",
                Password = "password",
                Delay = 0
            };

            Mocker.GetMock<IHttpClient>()
                .Setup(c => c.Execute(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(request =>
                {
                    requests.Add(request);

                    return requests.Count == 1
                        ? new HttpResponse(request, new HttpHeader(), "{\"id\":1}")
                        : new HttpResponse(request, new HttpHeader(), "{\"queue_time\":0}");
                });

            Subject.PerformSearch(settings, "Amélie");

            var body = Encoding.UTF8.GetString(requests[1].ContentData);
            var query = JsonConvert.DeserializeObject<AirDCPPProxy.CustomInfo>(body);

            query.query.pattern.Should().Be("Amelie");
        }
    }
}
