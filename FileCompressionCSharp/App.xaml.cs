using LogicLayerInterface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LogicLayer;


namespace FileCompressionCSharp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IArchiveTypeChecker checker = new ArchiveTypeChecker(); // LogicLayer implementation

            var mainWindow = new MainWindow(checker);
            mainWindow.Show();
        }
    }
}
