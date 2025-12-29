using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VideoEditor.Services;
using VideoEditor.ViewModels;

namespace VideoEditor
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Setup dependency injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Apply saved theme
            var themeManager = _serviceProvider.GetRequiredService<ThemeManager>();
            themeManager.ApplyTheme(themeManager.CurrentTheme);

            // Create and show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<ThemeManager>();
            services.AddSingleton<ProjectManager>();
            services.AddSingleton<VideoProcessingService>();
            services.AddSingleton<ExportService>();
            services.AddSingleton<TimelineService>();

            // ViewModels
            services.AddTransient<PropertiesViewModel>();
            services.AddTransient<TimelineViewModel>(sp => 
                new TimelineViewModel(sp.GetRequiredService<TimelineService>()));
            services.AddTransient<MainViewModel>(sp =>
                new MainViewModel(
                    sp.GetRequiredService<ThemeManager>(),
                    sp.GetRequiredService<ProjectManager>(),
                    sp.GetRequiredService<VideoProcessingService>(),
                    sp.GetRequiredService<ExportService>(),
                    sp.GetRequiredService<TimelineService>(),
                    sp.GetRequiredService<TimelineViewModel>(),
                    sp.GetRequiredService<PropertiesViewModel>()));

            // Views
            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}

