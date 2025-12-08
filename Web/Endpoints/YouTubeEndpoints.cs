using Microsoft.AspNetCore.Mvc;
using YoutubeDotNet.Models;
using YoutubeDotNet.Services;


namespace YoutubeDotNet.Endpoints;

public static class YouTubeEndpoints
{
    public static void MapYouTubeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/channel_info" , async(
            string channelName,
            bool isPremium,
            YouTubeApiService youtube
        ) =>
        {
            var result=await youtube.GetChannelInfoAsync(channelName,isPremium);
            return result == null
                ? Results.NotFound(new { detail = "Channel not found" })
                : Results.Ok(result);
        });



       app.MapGet("/api/multi_video_trend", async(
           string channelName,
           bool isPremium,
           YouTubeApiService youtube,
           CommentAnalysisService analysis
       ) =>
       {
           var channelInfo = await youtube.GetChannelInfoAsync(channelName, isPremium);
           if (channelInfo == null)
                return Results.NotFound(new { detail = "Channel not found" });

           var trendData = new List<VideoTrend>();


           foreach (var video in channelInfo.LatestVideos)
           {
               var counts = await analysis.AnalyzeVideoCommentAsync(video.VideoId);
               trendData.Add(new VideoTrend
                {
                    VideoId = video.VideoId,
                    Title = video.Title,
                    HateCount = counts.HateCount,
                    RequestCount = counts.RequestCount,
                    QuestionCount = counts.QuestionCount,
                    FeedbackCount = counts.FeedbackCount
                });
           }   

           return Results.Ok( new MultiVideoTrendResponse {TrendData = trendData}) ; 
       });



       app.MapGet("/api/video_analysis", async(
           string videoId,
           bool isPremium,
           int batchSize,
           YouTubeApiService youtube,
           MLModelService ml,
           CommentAnalysisService analysis

       ) =>
       {
           var (title, thumbnailUrl) = await youtube.GetVideoInfoAsync(videoId);
           if (title == null)
                return Results.NotFound(new { detail = "Video not found" });

           var maxComments = isPremium ? 1000 : 200;
           var comments = await youtube.FetchCommentsAsync(videoId, maxComments);
           var pieChart = new PieChartData();  
           var questions = new List<string>();
           var requests = new List<string>();
            var feedbacks = new List<string>();


           if (batchSize <= 0) batchSize = 50;
           for (int i = 0; i < comments.Count; i += batchSize)
           {
                 var batch = comments.Skip(i).Take(batchSize).ToList();
                 var hatePreds = ml.PredictHate(batch);
                var questionPreds = ml.PredictQuestion(batch);
                var requestPreds = ml.PredictRequest(batch);
                var feedbackPreds = ml.PredictFeedback(batch);


                for (int j = 0; j < batch.Count; j++)
               {
                   if (hatePreds[j] == 1)
                        pieChart.HateSpeech++;
                    else if (questionPreds[j] == 1)
                    {
                        pieChart.Questions++;
                        questions.Add(batch[j]);
                    }
                    else if (requestPreds[j] == 1)
                    {
                        pieChart.Requests++;
                        requests.Add(batch[j]);
                    }
                    else if (feedbackPreds[j] == 1)
                    {
                        pieChart.Feedback++;
                        feedbacks.Add(batch[j]);
                    }
                    else
                        pieChart.Neutral++;
               } 
           }


           var summaries = new CategorySummaries
           {
               Questions = analysis.RankComments(questions),
                Requests = analysis.RankComments(requests),
                Feedback = analysis.RankComments(feedbacks)
           };


           var response = new VideoAnalysisResponse
            {
                VideoId = videoId,
                Title = title,
                ThumbnailUrl = thumbnailUrl,
                PieChartData = pieChart,
                Summaries = summaries
            };

            return Results.Ok(response);   


       });




   

        app.MapGet("/api/most_liked",async(
            string videoId,
            bool isPremium,
            YouTubeApiService youtube,
            MLModelService ml,
            CommentAnalysisService analysis
        ) =>
        {
            var comments = await youtube.FetchCommentsWithLikesAsync(videoId, 1000);
            if (comments.Count == 0)
            {
                var emptyTop = new TopComments
                {
                    MostLikedQuestion = new LikedComment { Text = null, LikeCount = 0 },
                    MostLikedRequest = new LikedComment { Text = null, LikeCount = 0 },
                    MostLikedFeedback = new LikedComment { Text = null, LikeCount = 0 }
                };

                return Results.Ok(new MostLikedResponse { TopComments = emptyTop });
            }
            
            var texts = comments.Select(c => c.text).ToList();
            var questionPreds = ml.PredictQuestion(texts);
            var requestPreds = ml.PredictRequest(texts);
            var feedbackPreds = ml.PredictFeedback(texts);

            var mostLiked = new TopComments
            {
                MostLikedQuestion = new LikedComment { Text = "", LikeCount = -1 },
                MostLikedRequest = new LikedComment { Text = "", LikeCount = -1 },
                MostLikedFeedback = new LikedComment { Text = "", LikeCount = -1 }
            };
            for (int i = 0; i < texts.Count; i++)
            {
                var likeCount = comments[i].likes;
                if (questionPreds[i] == 1 && likeCount > mostLiked.MostLikedQuestion.LikeCount)
                {
                    mostLiked.MostLikedQuestion= new LikedComment {Text = texts[i], LikeCount = likeCount };
                }
                else if (requestPreds[i] == 1 && likeCount > mostLiked.MostLikedRequest.LikeCount)
                {
                    mostLiked.MostLikedRequest = new LikedComment { Text = texts[i], LikeCount = likeCount };
                }
                else if (feedbackPreds[i] == 1 && likeCount > mostLiked.MostLikedFeedback.LikeCount)
                {
                    mostLiked.MostLikedFeedback = new LikedComment { Text = texts[i], LikeCount = likeCount };
                }
            }

            
            return Results.Ok(
                new MostLikedResponse
                {
                    TopComments= mostLiked
                }
            );

        });








        app.MapGet("/api/comment_trend",async(
            string videoId,
            bool isPremium,
            YouTubeApiService youtube
        )=>
        {
             var days = isPremium ? 28 : 7;
            var today = DateTime.UtcNow.Date;

            var trend = new List<DailyCount>();
            for (int i = 0; i < days; i++)
            {
                var date = today.AddDays(-i);
                trend.Add(new DailyCount
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    CommentCount = 0
                });
            }
            trend.Reverse();

            return Results.Ok(new CommentTrendResponse
            {
                VideoId = videoId,
                Days = trend
            });

        });



    }
   
}
