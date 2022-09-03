using CommunityToolkit.Mvvm.DependencyInjection;
using IconRepository.Model;
using IconRepository.Service;
using IconRepository.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Windows;
using static IconRepository.String.Terms;

namespace IconRepository
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddSingleton(SettingHelper.Load())
                .AddSingleton<IGit, Service.Octokit>()
                .AddSingleton<AllPackViewModel>()                
                .BuildServiceProvider());
        }
    }
}
