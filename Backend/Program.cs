using MongoDB.Bson.Serialization;
using Backend.Repositories;
using Backend.Repositories.Interface;
using backend.Services;
using Shared;

namespace backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
     
        builder.Services.AddControllers();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("policy", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddSingleton<LoginRepositoryMongoDB>();
        builder.Services.AddSingleton<ILoginRepository, LoginRepositoryMongoDB>();
        builder.Services.AddSingleton<IElevplanRepository, ElevplanRepositoryMongoDB>();
        builder.Services.AddSingleton<IBrugereRepository, BrugereRepositoryMongoDB>();
        builder.Services.AddSingleton<ILearningRepository, LearningRepositoryMongoDB>();
        builder.Services.AddSingleton<ILokationRepository, LokationRepositoryMongoDB>();
        builder.Services.AddScoped<ExcelEksportService>();
 

        builder.Services.AddOpenApi();

        var app = builder.Build();
        app.UseStaticFiles(); 

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
       
        app.UseHttpsRedirection();
        app.UseCors("policy");
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}