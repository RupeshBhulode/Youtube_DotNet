namespace YoutubeDotNet.Services
{
    public class MLModelService
    {
        private readonly string[] _questionKeywords=new[]
        {
            "how", "what", "why", "when", "where", "which", "who", "is it", 
            "can i", "do i", "should i", "kaise", "kyu", "kab", "kaha", "kya" 
        };
        private readonly string[] _requestKeywords = new[] { 
            "please make", "please upload", "can you", "could you", "pls make", 
            "banaiye", "banao", "le aaiye", "sikhaiye", "video chahiye" 
        };
        
        private readonly string[] _feedbackKeywords = new[] { 
            "thank you", "thanks", "helpful", "great", "awesome", "amazing", 
            "bahut badiya", "mast", "op", "fire", "legend" 
        };
        private readonly string[] _hateKeywords = new[] { 
            "hate", "stupid", "idiot", "worst", "terrible", "useless", "bakwas" 
        };
        

        public int[] PredictHate(List<string>comments)
        {
            return comments.Select(c => 
                _hateKeywords.Any(k => c.ToLower().Contains(k))? 1:0).ToArray();
        }


        public int[] PredictQuestion(List<string> comments)
        {
            return comments.Select(c => 
                _questionKeywords.Any(k => c.ToLower().Contains(k)) ? 1 : 0
            ).ToArray();
        }

        public int[] PredictRequest(List<string> comments)
        {
            return comments.Select(c => 
                _requestKeywords.Any(k => c.ToLower().Contains(k)) ? 1 : 0
            ).ToArray();
        }

        public int[] PredictFeedback(List<string> comments)
        {
            return comments.Select(c => 
                _feedbackKeywords.Any(k => c.ToLower().Contains(k)) ? 1 : 0
            ).ToArray();
        }

        

        

    }
}