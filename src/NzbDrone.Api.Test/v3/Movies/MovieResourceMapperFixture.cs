using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Test.Common;
using Radarr.Api.V3.MovieFiles;
using Radarr.Api.V3.Movies;

namespace NzbDrone.Api.Test.v3.Movies
{
    [TestFixture]
    public class MovieResourceMapperFixture : TestBase
    {
        [Test]
        public void should_expose_the_resolved_movie_file_path_and_parent_directory()
        {
            var moviePath = @"C:\media\radarr\Full.Movie.Folder";
            var relativePath = "Movie.Name.mkv";
            var sourceFilePath = Path.Combine(moviePath, relativePath);
            var resolvedFilePath = @"D:\plex\X264\Full.Movie.Folder\Movie.Name.mkv";
            var movie = new Movie
            {
                Path = moviePath,
                Title = "Full Movie",
                MovieFileId = 1,
                MovieFile = new MovieFile
                {
                    MovieId = 1,
                    RelativePath = relativePath
                }
            };

            Mocker.GetMock<IConfigService>()
                .SetupGet(v => v.CopyUsingSymlinks)
                .Returns(true);
            Mocker.GetMock<IMediaPathResolver>()
                .Setup(v => v.Resolve(sourceFilePath))
                .Returns(resolvedFilePath);

            var resource = movie.ToResource(0, pathResolver: Mocker.GetMock<IMediaPathResolver>().Object, configService: Mocker.GetMock<IConfigService>().Object);
            var directMovieFileResource = movie.MovieFile.ToResource(movie, null, null, Mocker.GetMock<IMediaPathResolver>().Object, Mocker.GetMock<IConfigService>().Object);

            resource.MovieFile.Path.Should().Be(resolvedFilePath);
            resource.Path.Should().Be(Path.GetDirectoryName(resolvedFilePath));
            directMovieFileResource.Path.Should().Be(resolvedFilePath);
        }
    }
}
