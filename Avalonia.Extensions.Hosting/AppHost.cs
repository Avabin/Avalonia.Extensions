using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avalonia.Extensions.Hosting;

public abstract class AppHost
{
    private readonly Lazy<IHost> _host;
    private readonly Lazy<IHostBuilder> _builder;

    private Lazy<string> _path = new(() =>
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDirectory = System.IO.Path.Combine(appData, "Vizier");
        if (!Directory.Exists(appDirectory))
        {
            Directory.CreateDirectory(appDirectory);
        }

        Directory.SetCurrentDirectory(appDirectory);
        return appDirectory;
    });

    private string Path => _path.Value;
    
    
    protected IHost Host => _host.Value;
    protected IHostBuilder Builder => _builder.Value;
    public IServiceProvider Services { get; private set; } = null!;
    public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();
    
    public AppHost()
    {
        _host = new Lazy<IHost>(() => Builder.Build(), LazyThreadSafetyMode.ExecutionAndPublication);
        _builder = new Lazy<IHostBuilder>(CreateHostBuilder, LazyThreadSafetyMode.ExecutionAndPublication);
    }
    
    private IHostBuilder CreateHostBuilder()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder();
        builder.UseContentRoot(Path);
        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.ConfigureContainer<ContainerBuilder>(ConfigureContainer);
        builder.ConfigureLogging(ConfigureLogging);
        builder.ConfigureServices((Action<HostBuilderContext, IServiceCollection>)ConfigureServices);
        builder.ConfigureHostConfiguration(ConfigureHostConfiguration);


        return builder;
    }
    
    public void AddModule<TModule>() where TModule : IModule, new()
    {
        Builder.ConfigureContainer<ContainerBuilder>(builder =>
        {
            builder.RegisterModule<TModule>();
        });
    }
    
    public void AddModule<TModule>(TModule module) where TModule : IModule
    {
        Builder.ConfigureContainer<ContainerBuilder>(builder =>
        {
            builder.RegisterModule(module);
        });
    }
    
    public virtual async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await StartAsync(cancellationToken);
        await Host.WaitForShutdownAsync(cancellationToken);
    }
    
    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await Host.StartAsync(cancellationToken);
        Services = Host.Services;
    }
    
    public virtual async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await Host.StopAsync(cancellationToken);
        Host.Dispose();
    }
    
    protected virtual void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterType<AppViewLocator>().AsImplementedInterfaces();
    }
    
    protected virtual void ConfigureHostConfiguration(IConfigurationBuilder builder)
    {
        // try to find appsettings.json in the current directory or in the app executable directory
        var currentDirectory = Directory.GetCurrentDirectory();
        var appDirectory = AppContext.BaseDirectory;
        var appSettings = "appsettings.json";

        var currentDirAppsettings = System.IO.Path.Combine(currentDirectory, appSettings);
        var appDirAppsettings = System.IO.Path.Combine(appDirectory, appSettings);
        
        if (File.Exists(currentDirAppsettings))
        {
            builder.AddJsonFile(currentDirAppsettings);
        }
        else if (File.Exists(appDirAppsettings))
        {
            builder.AddJsonFile(appDirAppsettings);
        }
        else
        {
            Console.WriteLine($"Could not find {appSettings} in {currentDirectory} or {appDirectory}");
        }
    }

    protected virtual void ConfigureLogging(HostBuilderContext ctx, ILoggingBuilder builder)
    {
        builder.ClearProviders();
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "hh:mm:ss ";
        });

        builder.SetMinimumLevel(LogLevel.Debug);
    }
    
    protected virtual void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddSingleton(ctx.HostingEnvironment.ContentRootFileProvider);

    }

}