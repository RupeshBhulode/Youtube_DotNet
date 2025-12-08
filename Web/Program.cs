using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YoutubeDotNet.Services;
using YoutubeDotNet.Endpoints;
var builder=WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});



builder.Services.AddSingleton<YouTubeApiService>();
builder.Services.AddSingleton<MLModelService>();
builder.Services.AddSingleton<CommentAnalysisService>();

var app=builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthorization();
app.MapYouTubeEndpoints();


app.Run();





