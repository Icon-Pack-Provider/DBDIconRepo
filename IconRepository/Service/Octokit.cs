using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using IconRepository.Model;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IconRepository.String.Terms;

namespace IconRepository.Service
{
    public partial class Octokit : ObservableObject, IGit
    {
        public Octokit()
        {
            string token = App.Config.GitToken;
            if (!string.IsNullOrEmpty(token))
            {
                ReLogin(token);
            }
            else
            {
                Username = "Login";
                Avatar = null;
            }
        }

        public Octokit(string token)
        {
            ReLogin(token);
        }

        public void ReLogin(string token)
        {
            Client = new(new ProductHeaderValue(PHV));
            Client.Credentials = new(token);
            IconPack.Helper.PackHelper.InitializeGitService(Client);
            LoadUserInfo().Await(() =>
            {
                OnPropertyChanged(nameof(Username));
                OnPropertyChanged(nameof(Avatar));
            }, 
            (e) =>
            {
                Username = "Logged in";
                Avatar = null;
            });
        }

        public async Task LoadUserInfo()
        {
            var user = await Client.User.Current();
            var moreInfo = await Client.User.Get(user.Login);
            Username = moreInfo.Name is null ? user.Login : $"{moreInfo.Name} ({moreInfo.Login})";
            Avatar = moreInfo.AvatarUrl;
        }

        [ObservableProperty]
        string username = "Login";

        [ObservableProperty]
        string? avatar = null;

        public GitHubClient? Client { get; private set; }
    }

    public interface IGit
    {
        GitHubClient Client { get; }
        void ReLogin(string token);

        string Username { get; set; }
    }
}
