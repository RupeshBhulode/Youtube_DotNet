using YoutubeDotNet.Models;

namespace YoutubeDotNet.Services
{
    public class CommentAnalysisService
    {
        private readonly YouTubeApiService _youtubeService;
        private readonly MLModelService _mlService;

        public CommentAnalysisService(YouTubeApiService y, MLModelService m)
        {
            _youtubeService=y;
            _mlService=m;
        }


        public async Task<CommentCounts> AnalyzeVideoCommentAsync(string videoId, int maxComments = 200)
        {
            var comments = await _youtubeService.FetchCommentsAsync(videoId, maxComments);

            if (comments.Count == 0)
            {
                return new CommentCounts
                {
                    HateCount = 0,
                    RequestCount = 0,
                    QuestionCount = 0,
                    FeedbackCount = 0
                };
            }


            var hatePreds = _mlService.PredictHate(comments);
            var questionPreds = _mlService.PredictQuestion(comments);
            var requestPreds = _mlService.PredictRequest(comments);
            var feedbackPreds = _mlService.PredictFeedback(comments);


            var counts = new CommentCounts();
            for (int i = 0; i < comments.Count; i++)
            {
                if (hatePreds[i] == 1)
                    counts.HateCount++;
                else if (questionPreds[i] == 1)
                    counts.QuestionCount++;
                else if (requestPreds[i] == 1)
                    counts.RequestCount++;
                else if (feedbackPreds[i] == 1)
                    counts.FeedbackCount++;
            }

            return counts;

        }



        public List<string> RankComments(List<string> comments, int topK = 10)
        {
            
            if (comments.Count <= topK)
                return comments;

            var chunkSize = Math.Max(1, comments.Count / topK);
            var ranked = new List<string>();

            for (int i = 0; i < topK; i++)
            {
                var start = i * chunkSize;
                if (start >= comments.Count)
                    break;
                ranked.Add(comments[start]);
            }

            return ranked;
        }
    }
}