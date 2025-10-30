using LogicLayer;
using LogicLayerInterface;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace FileCompressionCSharp
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            // Register interfaces and their implementations
            services.AddSingleton<IArchiveTypeChecker, ArchiveTypeChecker>();
            services.AddSingleton<IHuffman, Huffman>();
            services.AddSingleton<ISlidingWindow, SlidingWindow>();

            // Register your MainWindow with its dependencies
            services.AddSingleton<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            // Retrieve and show the main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
