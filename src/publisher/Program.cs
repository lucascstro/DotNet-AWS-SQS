using Amazon.SQS;
using publisher.Services;
using Scalar.AspNetCore;

namespace publishApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        
        builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonSQS>();
        builder.Services.AddScoped<SqsService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
