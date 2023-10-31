using System;
using Autofac.Core;
using Avalonia.Controls.ApplicationLifetimes;

namespace Avalonia.Extensions.Hosting;

public static class AppBuilderExtensions
{
    private static Lazy<AvaloniaAppHost> _appHost;
    private static AvaloniaAppHost AppHost => _appHost.Value;

    static AppBuilderExtensions()
    {
        _appHost = new Lazy<AvaloniaAppHost>(() => new AvaloniaAppHost());
    }
    
    public static AppBuilder AddAutofacModule<TModule>(this AppBuilder builder) where TModule : IModule, new()
    {
        AppHost.AddModule<TModule>();
        
        return builder;
    }
    
    public static AppBuilder AddAutofacModule<TModule>(this AppBuilder builder,TModule module) where TModule : IModule
    {
        AppHost.AddModule(module);
        
        return builder;
    }
    
    
    public static AppBuilder WithGenericHost(this AppBuilder builder)
    {
        builder.AfterPlatformServicesSetup(b =>
        {
            AppHost.StartAsync().Wait();
        });
        if (builder?.Instance?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit += (sender, args) => { AppHost.StopAsync().Wait(); };
        }
        else
        {
            throw new NotSupportedException("Only desktop applications are supported");
        }
        
        return builder;
    }
}