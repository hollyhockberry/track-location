using Livet;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace WinApp
{
    public partial class App : Application
    {
        private readonly ServiceProvider serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection collection)
        {
            collection.AddSingleton<Models.IRepository, Repository>();
            collection.AddSingleton<Models.Model>();
            collection.AddSingleton<ViewModels.MainWindowViewModel>();
            collection.AddSingleton<Views.MainWindow>();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            serviceProvider.GetService<Views.MainWindow>().Show();
        }

        // Application level error handling
        //private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    //TODO: Logging
        //    MessageBox.Show(
        //        "Something errors were occurred.",
        //        "Error",
        //        MessageBoxButton.OK,
        //        MessageBoxImage.Error);
        //
        //    Environment.Exit(1);
        //}
    }
}
