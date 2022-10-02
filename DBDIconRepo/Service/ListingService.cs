﻿using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using SelectionListing;
using SelectionListing.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDIconRepo.Service
{    
    public partial class ListingService : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<SelectionMenuItem> listing = new();

        public void InitializeService()
        {
            Task.Run(async () =>
            {
                await Lists.CheckCatagoryRepo();
                Listing = Lists.GetListings();
            }).Await(() =>
            {

            });
        }

        public static ListingService Instance
        {
            get
            {
                if (!Singleton<ListingService>.HasInitialize)
                    Singleton<ListingService>.Instance.InitializeService();
                return Singleton<ListingService>.Instance;
            }
        }
    }
}