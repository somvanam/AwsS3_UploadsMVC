using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http.Features;
using AwsS3_UploadsMVC.Interfaces;
using AwsS3_UploadsMVC.Services;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configure the maximum request size limit
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = long.MaxValue; // Set it to the maximum possible value
        });
        services.AddScoped<IAwsS3Upload, AwsS3UploadService>();
        // Other service configurations
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Your middleware configuration
    }
}
