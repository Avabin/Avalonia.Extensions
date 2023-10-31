using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Splat;
using Splat.Autofac;
using YoloVision.UI.Infrastructure;

namespace YoloVision.UI.Avalonia.Infrastructure;

public class AvaloniaAppHost : AppHost
{
    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.UseAutofacDependencyResolver();
        Locator.CurrentMutable.InitializeSplat();
        Locator.CurrentMutable.InitializeReactiveUI();
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

        var appViewLocator = new AppViewLocator();
        Locator.CurrentMutable.RegisterConstant<IViewLocator>(appViewLocator);
        
        builder.RegisterModule<AppModule>();
        
        base.ConfigureContainer(builder);
    }

    protected override void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        base.ConfigureServices(ctx, services);
        services.AddMemoryCache();
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await base.StartAsync(cancellationToken);
        ServiceLocator.Instance = Services;
    }
}