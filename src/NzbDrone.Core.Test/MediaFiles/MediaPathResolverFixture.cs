using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class MediaPathResolverFixture : CoreTest<MediaPathResolver>
    {
        [Test]
        public void should_resolve_paths_through_the_disk_provider()
        {
            var path = "/mnt/ext_3/radarr/Movie";
            var resolvedPath = "/mnt/plex/X264/Movie";

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetRealPath(path))
                  .Returns(resolvedPath);

            Subject.Resolve(path).Should().Be(resolvedPath);
        }

        [Test]
        public void should_apply_mapping_to_the_resolved_path()
        {
            var path = "/mnt/ext_3/radarr/Movie";
            var resolvedPath = "/mnt/plex/X264/Movie";

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetRealPath(path))
                  .Returns(resolvedPath);

            Subject.ResolveMappedPath(path, "/mnt/plex", "/media").Should().Be("/media/X264/Movie");
        }

        [Test]
        public void should_resolve_movie_folder_from_the_resolved_movie_file()
        {
            var moviePath = "/mnt/ext_3/radarr/Test Movie (2026)";
            var relativePath = "Test.Movie.1080p.WEBRip.H264.AAC-GROUP.mkv";
            var movieFilePath = Path.Combine(moviePath, relativePath);
            var resolvedMovieFilePath = "/mnt/plex/X264/Test.Movie.1080p.WEBRip.H264.AAC-GROUP/Test.Movie.1080p.WEBRip.H264.AAC-GROUP.mkv";

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetRealPath(movieFilePath))
                  .Returns(resolvedMovieFilePath);

            Subject.ResolveMovieFolderPath(moviePath, relativePath)
                   .Should().Be(resolvedMovieFilePath.GetParentPath());
        }
    }
}
