using System;
using Microsoft.Extensions.DependencyInjection;

namespace YoloVision.UI.Avalonia.Infrastructure;

public static class ServiceLocator
{
    internal static IServiceProvider? Instance { get; set; }

    public static T? Get<T>()
    {
        return Instance!.GetService<T>();
    }
    

    public static object? Get(Type type)
    {
        var service = Instance!.GetService(type);
        return service;
    }

    public static T GetRequired<T>() where T : notnull
    {
        return Instance!.GetRequiredService<T>();
    }
}