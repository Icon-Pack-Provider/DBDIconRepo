using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using IconRepository.Model;
using IconRepository.Service;
using System;

namespace IconRepository.ViewModel
{
    public partial class AllPackViewModel : ObservableObject
    {
        [ObservableProperty]
        public Service.Octokit git;

        [ObservableProperty]
        bool showSomething;

        [ObservableProperty]
        Setting? appConfig;

        public AllPackViewModel()
        {
            AppConfig = Ioc.Default.GetService<Setting>();
            Git = (Service.Octokit?)Ioc.Default.GetService<IGit>();
        }
    }
}
