using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Splat;
using Splat.Autofac;

namespace Avalonia.Extensions.Hosting;

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
        
        
        base.ConfigureContainer(builder);
    }

    protected override void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        base.ConfigureServices(ctx, services);
        
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await base.StartAsync(cancellationToken);
        ServiceLocator.Instance = Services;
    }
}