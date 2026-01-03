using System;
using System.IO;
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
            // Add global exception handlers FIRST, before any other initialization
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                LogException("Base startup error", ex);
                MessageBox.Show(
                    $"Failed to initialize application: {ex.Message}\n\nThe application will now close.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
                return;
            }

            try
            {
                // Check FFmpeg availability at startup
                if (!FFmpegHelper.IsFFmpegAvailable())
                {
                    var message = FFmpegHelper.GetAvailabilityMessage();
                    MessageBox.Show(
                        message,
                        "FFmpeg Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                // Setup dependency injection
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                _serviceProvider = serviceCollection.BuildServiceProvider();

                // Apply saved theme
                try
                {
                    var themeManager = _serviceProvider.GetRequiredService<ThemeManager>();
                    if (themeManager?.CurrentTheme != null)
                    {
                        themeManager.ApplyTheme(themeManager.CurrentTheme);
                    }
                }
                catch (Exception ex)
                {
                    LogException("Theme application error", ex);
                    // Continue without theme - not critical
                }

                // Create and show main window
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                if (mainWindow != null)
                {
                    mainWindow.Show();
                }
                else
                {
                    throw new InvalidOperationException("Failed to create main window");
                }
            }
            catch (Exception ex)
            {
                LogException("Startup error", ex);
                MessageBox.Show(
                    $"Failed to start application: {ex.Message}\n\nStack trace:\n{ex.StackTrace}\n\nThe application will now close.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void LogException(string context, Exception ex)
        {
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}: {ex.GetType().Name}\n" +
                           $"Message: {ex.Message}\n" +
                           $"Stack Trace:\n{ex.StackTrace}\n";
            
            if (ex.InnerException != null)
            {
                logMessage += $"Inner Exception: {ex.InnerException.Message}\n";
            }
            
            System.Diagnostics.Debug.WriteLine(logMessage);
            
            // Also try to write to a log file
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "VideoEditor", "error.log");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, logMessage + "\n");
            }
            catch
            {
                // Ignore log file errors
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException("Unhandled UI thread exception", e.Exception);
            
            // Show user-friendly message
            try
            {
                MessageBox.Show(
                    $"An error occurred: {e.Exception.GetType().Name}\n\n{e.Exception.Message}\n\nThe application will continue, but some features may not work correctly.\n\nSee error.log for details.",
                    "Application Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch
            {
                // If MessageBox fails, at least log it
                System.Diagnostics.Debug.WriteLine("Failed to show error message box");
            }
            
            // Mark as handled to prevent crash
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException("Unhandled background thread exception", ex);
                
                // If it's a terminating exception, show error and exit
                if (e.IsTerminating)
                {
                    try
                    {
                        MessageBox.Show(
                            $"A critical error occurred: {ex.GetType().Name}\n\n{ex.Message}\n\nThe application must close.\n\nSee error.log for details.",
                            "Critical Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    catch
                    {
                        // Ignore if MessageBox fails during shutdown
                    }
                }
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<ThemeManager>();
            services.AddSingleton<ProjectManager>();
            services.AddSingleton<VideoProcessingService>();
            services.AddSingleton<ExportService>();
            services.AddSingleton<TimelineService>();
            services.AddSingleton<Commands.UndoRedoManager>();

            // ViewModels
            services.AddTransient<PropertiesViewModel>();
            services.AddTransient<TimelineViewModel>(sp => 
                new TimelineViewModel(
                    sp.GetRequiredService<TimelineService>(),
                    sp.GetRequiredService<Commands.UndoRedoManager>()));
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

