using Python.Runtime;
using System.IO;
using Microsoft.AspNetCore.Hosting; 


namespace YoutubeDotNet.Services;

public class ModelsLoader
{
    private dynamic _hateModel;
    private dynamic _requestModel;
    private dynamic _questionModel;
    private dynamic _feedbackModel;
    private dynamic _joblib;


    public ModelsLoader(IWebHostEnvironment environment)
{
    InitializePythonEngine();
    LoadModels(environment.ContentRootPath);
}

private void InitializePythonEngine()
    {
        if (!PythonEngine.IsInitialized)
        {
            Runtime.PythonDLL = "python39.dll";
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
    }


    private void LoadModels(string rootPath)
    {
        using (Py.GIL())
        {
            _joblib = Py.Import("joblib");
            var modelsPath = Path.Combine(rootPath, "MLModels");
            var hateModelPath = Path.Combine(modelsPath, "hate_speech_model2.pkl");
            var requestModelPath = Path.Combine(modelsPath, "request_0_1_newest_model.pkl");
            var questionModelPath = Path.Combine(modelsPath, "question_0_1_newest_model.pkl");
            var feedbackModelPath = Path.Combine(modelsPath, "feedback_0_1_newest_model.pkl");
            
            _hateModel = _joblib.load(hateModelPath);
                _requestModel = _joblib.load(requestModelPath);
                _questionModel = _joblib.load(questionModelPath);
                _feedbackModel = _joblib.load(feedbackModelPath);

              Console.WriteLine("✅ All ML models loaded successfully!");  

        }
    }

    public int[] PredictHate(List<string> comments)
        {
            using (Py.GIL())
            {
                dynamic predictions = _hateModel.predict(comments);
                return ConvertToIntArray(predictions);
            }
        }


       public int[] PredictQuestion(List<string> comments)
        {
            using (Py.GIL())
            {
                dynamic predictions = _questionModel.predict(comments);
                return ConvertToIntArray(predictions);
            }
        }

        public int[] PredictRequest(List<string> comments)
        {
            using (Py.GIL())
            {
                dynamic predictions = _requestModel.predict(comments);
                return ConvertToIntArray(predictions);
            }
        }

        public int[] PredictFeedback(List<string> comments)
        {
            using (Py.GIL())
            {
                dynamic predictions = _feedbackModel.predict(comments);
                return ConvertToIntArray(predictions);
            }
        }


         private int[] ConvertToIntArray(dynamic pythonArray)
        {
           
            var list = new List<int>();
            foreach (var item in pythonArray)
            {
                list.Add((int)item);
            }
            return list.ToArray();
        }


      ~ModelsLoader()
      {
            // Cleanup Python engine when done
            if (PythonEngine.IsInitialized)
            {
                PythonEngine.Shutdown();
            }
        }




}