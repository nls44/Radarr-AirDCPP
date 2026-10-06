using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Webhook;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Test.Framework;
using CoreWebhook = NzbDrone.Core.Notifications.Webhook.Webhook;

namespace NzbDrone.Core.Test.NotificationTests.Webhook
{
    [TestFixture]
    public class WebhookPathFixture : CoreTest<CoreWebhook>
    {
        [Test]
        public void should_send_resolved_movie_and_movie_file_paths()
        {
            var moviePath = "/mnt/ext_3/radarr/Nicely Formatted Movie Name (Year)";
            var movieFilePath = Path.Combine(moviePath, "Movie Name.mkv");
            var resolvedMoviePath = "/mnt/plex/X264/Full.Movie.Folder";
            var resolvedMovieFilePath = "/mnt/plex/X264/Full.Movie.Folder/Movie.Name.mkv";
            var movie = new Movie
            {
                Id = 1,
                Path = moviePath,
                MovieMetadata = new MovieMetadata
                {
                    Title = "Movie Name",
                    Images = new List<MediaCover.MediaCover>()
                }
            };
            var movieFile = new MovieFile
            {
                Movie = movie,
                RelativePath = "Movie Name.mkv",
                Quality = new QualityModel()
            };
            WebhookImportPayload payload = null;

            Subject.Definition = new NotificationDefinition
            {
                Settings = new WebhookSettings()
            };

            Mocker.GetMock<ITagRepository>()
                  .Setup(v => v.GetTags(It.IsAny<HashSet<int>>()))
                  .Returns(new List<Tag>());
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetRealPath(moviePath))
                  .Returns(resolvedMoviePath);
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetRealPath(movieFilePath))
                  .Returns(resolvedMovieFilePath);
            Mocker.GetMock<IWebhookProxy>()
                  .Setup(v => v.SendWebhook(It.IsAny<WebhookPayload>(), It.IsAny<WebhookSettings>()))
                  .Callback<WebhookPayload, WebhookSettings>((p, _) => payload = (WebhookImportPayload)p);

            Subject.OnDownload(new DownloadMessage
            {
                Movie = movie,
                MovieInfo = new LocalMovie
                {
                    Movie = movie
                },
                MovieFile = movieFile,
                OldMovieFiles = new List<DeletedMovieFile>()
            });

            payload.Should().NotBeNull();
            payload.Movie.FolderPath.Should().Be(resolvedMoviePath);
            payload.MovieFile.Path.Should().Be(resolvedMovieFilePath);
        }
    }
}
