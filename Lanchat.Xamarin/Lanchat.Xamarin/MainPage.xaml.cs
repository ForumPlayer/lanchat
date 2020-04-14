﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Lanchat.Xamarin
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(250);
                Input.Focus();
            });
        }

        void OnSendClicked(object sender, EventArgs args)
        {
            Log.Text = Log.Text += $"{Environment.NewLine}[{DateTime.Now:HH:mm}] {Input.Text}";
            Input.Text = string.Empty;
        }
    }
}
