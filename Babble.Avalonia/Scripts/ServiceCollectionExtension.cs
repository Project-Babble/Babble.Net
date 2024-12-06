using Microsoft.Extensions.DependencyInjection;

namespace Babble.Avalonia.Scripts;

public static class ServiceCollectionExtension
{
    public static void RegisterTransient<TView, TViewModel>(this IServiceCollection services)
        where TView : class
        where TViewModel : class
    {
        services.AddTransient<TView>();
        services.AddTransient<TViewModel>();
    }
    public static void RegisterSingleton<TView, TViewModel>(this IServiceCollection services)
        where TView : class
        where TViewModel : class
    {
        services.AddSingleton<TView>();
        services.AddSingleton<TViewModel>();
    }
}