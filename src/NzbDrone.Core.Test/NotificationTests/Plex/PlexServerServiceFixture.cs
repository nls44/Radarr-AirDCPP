using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Core.MediaFiles;
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
        private const string RootFolderPath = @"C:\Test\Movies";

        private Movie _movie;
        private PlexServerSettings _settings;

        [SetUp]
        public void SetUp()
        {
            Mocker.SetConstant<ICacheManager>(Mocker.Resolve<CacheManager>());

            _movie = new Movie
            {
                Path = @"C:\Test\Movies\Movie Title (2020)".AsOsAgnostic()
            };

            _settings = new PlexServerSettings
            {
                Host = "127.0.0.1",
                Port = 32400
            };

            Mocker.GetMock<IMediaPathResolver>()
                .Setup(s => s.ResolveMovieFolderPath(_movie.Path, null))
                .Returns(@"C:\Test\ResolvedMovies\Movie Title (2020)".AsOsAgnostic());

            Mocker.GetMock<IRootFolderService>()
                .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>(), null))
                .Returns(RootFolderPath.AsOsAgnostic());

            Mocker.GetMock<IPlexServerProxy>()
                .Setup(s => s.Version(_settings))
                .Returns("1.20.0.12345-abcdef");
        }

        private PlexSection GivenSection(int id)
        {
            var section = new PlexSection
            {
                Id = id
            };

            section.Locations.Add(new PlexSectionLocation
            {
                Id = id,
                Path = RootFolderPath.AsOsAgnostic()
            });

            return section;
        }

        [Test]
        public void should_update_all_sections_matching_the_movie_path()
        {
            var sections = new List<PlexSection>
            {
                GivenSection(1),
                GivenSection(2)
            };

            Mocker.GetMock<IPlexServerProxy>()
                .Setup(s => s.GetMovieSections(_settings))
                .Returns(sections);

            Subject.UpdateLibrary(_movie, _settings);

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(1, It.IsAny<string>(), _settings), Times.Once());

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(2, It.IsAny<string>(), _settings), Times.Once());
        }

        [Test]
        public void should_update_only_the_matching_section_when_a_single_section_matches()
        {
            var matchingSection = GivenSection(1);
            var nonMatchingSection = new PlexSection
            {
                Id = 2
            };

            nonMatchingSection.Locations.Add(new PlexSectionLocation
            {
                Id = 2,
                Path = @"C:\Test\OtherMovies".AsOsAgnostic()
            });

            var sections = new List<PlexSection> { matchingSection, nonMatchingSection };

            Mocker.GetMock<IPlexServerProxy>()
                .Setup(s => s.GetMovieSections(_settings))
                .Returns(sections);

            Subject.UpdateLibrary(_movie, _settings);

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(1, It.IsAny<string>(), _settings), Times.Once());

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(2, It.IsAny<string>(), _settings), Times.Never());
        }

        [Test]
        public void should_only_update_a_section_once_when_multiple_locations_match()
        {
            var section = new PlexSection { Id = 1 };

            section.Locations.Add(new PlexSectionLocation { Id = 1, Path = RootFolderPath.AsOsAgnostic() });
            section.Locations.Add(new PlexSectionLocation { Id = 2, Path = RootFolderPath.AsOsAgnostic() });

            var sections = new List<PlexSection> { section };

            Mocker.GetMock<IPlexServerProxy>()
                .Setup(s => s.GetMovieSections(_settings))
                .Returns(sections);

            Subject.UpdateLibrary(_movie, _settings);

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(1, It.IsAny<string>(), _settings), Times.Once());
        }

        [Test]
        public void should_fall_back_to_updating_every_section_when_none_match()
        {
            var sections = new List<PlexSection>
            {
                new PlexSection { Id = 1 },
                new PlexSection { Id = 2 }
            };

            sections[0].Locations.Add(new PlexSectionLocation { Id = 1, Path = @"C:\Test\OtherMovies".AsOsAgnostic() });
            sections[1].Locations.Add(new PlexSectionLocation { Id = 2, Path = @"C:\Test\OtherMovies2".AsOsAgnostic() });

            Mocker.GetMock<IPlexServerProxy>()
                .Setup(s => s.GetMovieSections(_settings))
                .Returns(sections);

            Subject.UpdateLibrary(_movie, _settings);

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(1, It.IsAny<string>(), _settings), Times.Once());

            Mocker.GetMock<IPlexServerProxy>()
                .Verify(v => v.Update(2, It.IsAny<string>(), _settings), Times.Once());
        }

        [Test]
        public void should_update_the_movie_section_containing_the_resolved_movie_path()
        {
            var settings = new PlexServerSettings
            {
                Host = "plex"
            };
            var moviePath = "/mnt/ext_3/radarr/Nicely Formatted Movie Name (Year)";
            var relativePath = "Movie.Name.mkv";
            var resolvedMoviePath = "/mnt/plex/X264/Full.Movie.Folder";
            var movie = new Movie
            {
                Path = moviePath,
                MovieFile = new MovieFile
                {
                    RelativePath = relativePath
                }
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
            Mocker.GetMock<IMediaPathResolver>()
                  .Setup(v => v.ResolveMovieFolderPath(moviePath, relativePath))
                  .Returns(resolvedMoviePath);

            Subject.UpdateLibrary(movie, settings);

            Mocker.GetMock<IPlexServerProxy>()
                  .Verify(v => v.Update(2, resolvedMoviePath, settings), Times.Once());
            Mocker.GetMock<IPlexServerProxy>()
                  .Verify(v => v.Update(1, It.IsAny<string>(), settings), Times.Never());
            Mocker.GetMock<IRootFolderService>()
                  .Verify(v => v.GetBestRootFolderPath(It.IsAny<string>(), It.IsAny<List<RootFolder>>()), Times.Never());
        }

        [Test]
        public void should_not_construct_a_movie_path_from_the_stored_directory_when_the_resolved_folder_does_not_match()
        {
            var settings = new PlexServerSettings
            {
                Host = "plex"
            };
            var moviePath = "/mnt/ext_3/radarr/Test Movie (2026)";
            var relativePath = "Test.Movie.mkv";
            var movie = new Movie
            {
                Path = moviePath,
                MovieFile = new MovieFile
                {
                    RelativePath = relativePath
                }
            };
            var sections = new List<PlexSection>
            {
                new PlexSection
                {
                    Id = 1,
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
            Mocker.GetMock<IMediaPathResolver>()
                  .Setup(v => v.ResolveMovieFolderPath(moviePath, relativePath))
                  .Returns("/mnt/other/Unmatched.Movie");

            Subject.UpdateLibrary(movie, settings);

            Mocker.GetMock<IPlexServerProxy>()
                  .Verify(v => v.Update(1, It.Is<string>(path => path == "/mnt/plex/X264/"), settings), Times.Once());
            Mocker.GetMock<IPlexServerProxy>()
                  .Verify(v => v.Update(1, It.Is<string>(path => path.Contains("Test Movie")), settings), Times.Never());
            Mocker.GetMock<IRootFolderService>()
                  .Verify(v => v.GetBestRootFolderPath(It.IsAny<string>(), It.IsAny<List<RootFolder>>()), Times.Never());
        }
    }
}
