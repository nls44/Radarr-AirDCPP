using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Plex.Server;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests.Plex
{
    [TestFixture]
    public class PlexServerServiceFixture : CoreTest<PlexServerService>
    {
        [SetUp]
        public void SetUp()
        {
            Mocker.SetConstant<ICacheManager>(Mocker.Resolve<CacheManager>());
        }

        [Test]
        public void should_update_the_movie_section_containing_the_resolved_movie_path()
        {
            var settings = new PlexServerSettings
            {
                Host = "plex"
            };
            var moviePath = "/mnt/ext_3/radarr/Nicely Formatted Movie Name (Year)";
            var resolvedMoviePath = "/mnt/plex/X264/Full.Movie.Folder";
            var movie = new Movie
            {
                Path = moviePath
            };
            var sections = new List<PlexSection>
            {
                new PlexSection
                {
                    Id = 1,
                    Type = "movie",
                    Locations = new List<PlexSectionLocation>
                    {
                        new PlexSectionLocation { Path = "/mnt/plex/Movies" }
                    }
                },
                new PlexSection
                {
                    Id = 2,
                    Type = "movie",
                    Locations = new List<PlexSectionLocation>
                    {
                        new PlexSectionLocation { Path = "/mnt/plex/X264" }
                    }
                }
            };

            Mocker.GetMock<IPlexServerProxy>()
                  .Setup(v => v.Version(settings))
                  .Returns("1.2.3.4.");
            Mocker.GetMock<IPlexServerProxy>()
                  .Setup(v => v.GetMovieSections(settings))
                  .Returns(sections);
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetRealPath(moviePath))
                  .Returns(resolvedMoviePath);

            Subject.UpdateLibrary(movie, settings);

            Mocker.GetMock<IPlexServerProxy>()
                  .Verify(v => v.Update(2, resolvedMoviePath, settings), Times.Once());
            Mocker.GetMock<IPlexServerProxy>()
                  .Verify(v => v.Update(1, It.IsAny<string>(), settings), Times.Never());
            Mocker.GetMock<IRootFolderService>()
                  .Verify(v => v.GetBestRootFolderPath(It.IsAny<string>(), It.IsAny<List<RootFolder>>()), Times.Never());
        }
    }
}
