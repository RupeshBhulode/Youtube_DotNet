namespace YoutubeDotNet.Models
{
    public class VideoInfo
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class ChannelInfoResponse
    {
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string ProfileImage { get; set; }
        public int SubscriberCount { get; set; }
        public List<VideoInfo> LatestVideos { get; set; }
    }

    public class VideoTrend
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public int HateCount { get; set; }
        public int RequestCount { get; set; }
        public int QuestionCount { get; set; }
        public int FeedbackCount { get; set; }
    }

    public class MultiVideoTrendResponse
    {
        public List<VideoTrend> TrendData { get; set; }
    }


    public class PieChartData
    {
        public int HateSpeech { get; set; }
        public int Questions { get; set; }
        public int Requests { get; set; }
        public int Feedback { get; set; }
        public int Neutral { get; set; }
    }
    
    public class CategorySummaries
    {
        public List<string> Questions { get; set; }
        public List<string> Requests { get; set; }
        public List<string> Feedback { get; set; }
    }

    public class VideoAnalysisResponse
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public PieChartData PieChartData { get; set; }
        public CategorySummaries Summaries { get; set; }
    }



    public class LikedComment
    {
        public string Text { get; set; }
        public int LikeCount { get; set; }
    }


    public class TopComments
    {
        public LikedComment MostLikedQuestion { get; set; }
        public LikedComment MostLikedRequest { get; set; }
        public LikedComment MostLikedFeedback { get; set; }
    }

    public class MostLikedResponse
    {
        public TopComments TopComments { get; set; }
    }
     
    public class DailyCount
    {
        public string Date { get; set; }
        public int CommentCount { get; set; }
    } 

    public class CommentTrendResponse
    {
        public string VideoId { get; set; }
        public List<DailyCount> Days { get; set; }
    }

    public class CommentCounts
    {
        public int HateCount { get; set; }
        public int RequestCount { get; set; }
        public int QuestionCount { get; set; }
        public int FeedbackCount { get; set; }
    }




}