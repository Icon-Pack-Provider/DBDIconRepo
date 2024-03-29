﻿using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using System;
using System.Windows;

namespace DBDIconRepo.Dialog;

/// <summary>
/// Interaction logic for Login.xaml
/// </summary>
public partial class Login : Window
{
    public Login()
    {
        InitializeComponent();
    }

    private void Open2FAInstruction(object sender, RoutedEventArgs e)
    {
        URL.OpenURL($"https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token");
    }

    private async void LoginToGit(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(passwordBox.Password))
        {
            DialogHelper.Show("No login information", "Error", DialogSymbol.Error);
            return;
        }

        if (!string.IsNullOrEmpty(passwordBox.Password))
        {
            //Token login
            //Check if valid
            try
            {
                Octokit.GitHubClient test = new(new Octokit.ProductHeaderValue("credential-check"));
                var cred = new Octokit.Credentials(passwordBox.Password);
                test.Credentials = cred;
                var isValid = await test.User.Current();
                if (!string.IsNullOrEmpty(isValid.Login))
                {
                    SettingManager.Instance.GitUsername = isValid.Login;
                    new SecureSettingService().SaveSecurePassword(passwordBox.Password);
                    OctokitService.Instance.InitializeGit();
                }
            }
            catch
            {
                DialogHelper.Show("Either token has no user & repository query access, or invalid token!",
                    "Bad credentials error:", DialogSymbol.Error);
                return;
            }            
        }

        //Finalize
        DialogResult = true;
        this.Close();
    }
}
