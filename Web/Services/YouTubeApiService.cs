using Google.Apis.Services;
using Google.Apis.YouTube.v3;

using YoutubeDotNet.Models;

namespace YoutubeDotNet.Services
{
    public class YouTubeApiService
    {
        private readonly YouTubeService _youtubeClient;
        private readonly string[] _apiKeys=new[]
        {
            "AIzaSyDYjOKHNOf-fY55p_MQA93Eaj13Uvv4puY",
            "AIzaSyA0s8gvFZJBQHgSlYdF4lG78FM0YLb4wm0",
            "AIzaSyCTFcusoP1DJb_nwSMwftESGyFchn_Kdgo"
        };

        public YouTubeApiService()
        {
            var currentKey = GetCurrentApiKey();
            _youtubeClient = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = currentKey,
                ApplicationName = "YouTubeAnalytics"
            });
        }

        private string GetCurrentApiKey()
        {
            var hour = DateTime.UtcNow.Hour;
            var keyIndex = (hour / 1) % _apiKeys.Length;
            return _apiKeys[keyIndex];
        }


        public async Task<ChannelInfoResponse> GetChannelInfoAsync(string channelName, bool isPremium)
        {
            var searchRequest = _youtubeClient.Search.List("id,snippet");
            searchRequest.Q=channelName;
            searchRequest.Type="channel";
            searchRequest.MaxResults=1;

            var searchResponse = await searchRequest.ExecuteAsync();
            if (searchResponse.Items.Count == 0)
                return null;
            
            var channel = searchResponse.Items[0];
            var channelId = channel.Id.ChannelId;
            var channelTitle = channel.Snippet.Title;
            var profileImage = channel.Snippet.Thumbnails?.High?.Url ?? 
                             channel.Snippet.Thumbnails?.Default__?.Url;

            var statsRequest = _youtubeClient.Channels.List("statistics");
            statsRequest.Id=channelId;
            var statsResponse = await statsRequest.ExecuteAsync();
            var subscriberCount = (int)(statsResponse.Items[0].Statistics.SubscriberCount ?? 0);


            var contentRequest = _youtubeClient.Channels.List("contentDetails");
            contentRequest.Id = channelId;
            var contentResponse = await contentRequest.ExecuteAsync();
            var uploadsPlaylistId = contentResponse.Items[0].ContentDetails.RelatedPlaylists.Uploads;

            var maxVideos = isPremium ? 10 : 3;
            var playlistRequest = _youtubeClient.PlaylistItems.List("snippet");
            playlistRequest.PlaylistId = uploadsPlaylistId;
            playlistRequest.MaxResults = maxVideos;
            var playlistResponse = await playlistRequest.ExecuteAsync();



             var latestVideos = playlistResponse.Items.Select(item => new VideoInfo
            {
                VideoId = item.Snippet.ResourceId.VideoId,
                Title = item.Snippet.Title,
                ThumbnailUrl = item.Snippet.Thumbnails.High.Url
            }).ToList();


             return new ChannelInfoResponse
             {
                 ChannelId= channelId,
                 ChannelName=channelTitle,
                 ProfileImage=profileImage,
                 SubscriberCount=subscriberCount,
                 LatestVideos=latestVideos
             };          


        }


        public async Task<List<string>> FetchCommentsAsync(string videoId, int maxComments)
        {
            var comments = new List<string>();
            string nextPageToken = null;
            while (comments.Count < maxComments)
            {
                var request = _youtubeClient.CommentThreads.List("snippet");
                request.VideoId = videoId;
                request.MaxResults = 100;
                request.PageToken = nextPageToken;
                request.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;

                var response = await request.ExecuteAsync();

                foreach (var item in response.Items)
                {
                    var comment = item.Snippet.TopLevelComment.Snippet.TextDisplay;
                    comments.Add(comment);
                    if (comments.Count >= maxComments)
                        break;
                }

                nextPageToken = response.NextPageToken;
                if (string.IsNullOrEmpty(nextPageToken))
                    break;
            }
             return comments;



        }

        public async Task<List<(string text, int likes)>> FetchCommentsWithLikesAsync(string videoId, int maxComments)
        {
            var comments = new List<(string, int)>();
            string nextPageToken = null;

            while (comments.Count < maxComments)
            {
                var request = _youtubeClient.CommentThreads.List("snippet");
                request.VideoId = videoId;
                request.MaxResults = 100;
                request.PageToken = nextPageToken;
                request.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;

                var response = await request.ExecuteAsync();

                foreach (var item in response.Items)
                {
                    var snippet = item.Snippet.TopLevelComment.Snippet;
                    comments.Add((snippet.TextDisplay, (int)snippet.LikeCount));
                    if (comments.Count >= maxComments)
                        break;
                }
                nextPageToken = response.NextPageToken;
                if (string.IsNullOrEmpty(nextPageToken))
                    break;
            }

            return comments;
        }

        public async Task<(string title, string thumbnailUrl)> GetVideoInfoAsync(string videoId)
        {
            var request = _youtubeClient.Videos.List("snippet");
            request.Id = videoId;
            var response = await request.ExecuteAsync();

            if (response.Items.Count == 0)
                return (null, null);

            var snippet = response.Items[0].Snippet;
            var title = snippet.Title;
            var thumbnailUrl = snippet.Thumbnails?.High?.Url ?? 
                             snippet.Thumbnails?.Default__?.Url;

            return (title, thumbnailUrl);
        }





    }
}