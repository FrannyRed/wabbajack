﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wabbajack
{
    /// <summary>
    /// Interaction logic for ModlistPropertiesWindow.xaml
    /// </summary>
    public partial class ModlistPropertiesWindow : Window
    {
        internal string newBannerFile;
        internal readonly AppState state;
        internal ModlistPropertiesWindow(AppState _state)
        {
            InitializeComponent();
            var bannerImage = UIUtils.BitmapImageFromResource("Wabbajack.banner.png");
            SplashScreenProperty.Source = bannerImage;

            newBannerFile = null;
            state = _state;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //Hide();
        }

        private void SetSplashScreen_Click(object sender, RoutedEventArgs e)
        {
            var file = UIUtils.OpenFileDialog("Banner image|*.png");
            if(file != null)
            {
                newBannerFile = file;
                SplashScreenProperty.Source = new BitmapImage(new Uri(file));
            }
        }

        private void SaveProperties_Click(object sender, RoutedEventArgs e)
        {
            if (state.UIReady)
            {
                BitmapImage splashScreen = null;
                if (newBannerFile != null)
                {
                    splashScreen = new BitmapImage(new Uri(newBannerFile));
                }
                string modListName = ModlistNameProperty.Text;
                string modListAuthor = ModlistAuthorProperty.Text;
                string modListDescription = ModlistDescriptionProperty.Text;

                state.SplashScreenImage = splashScreen;
                state.SplashScreenModName = modListName;
                state.SplashScreenSummary = modListDescription;
                state.SplashScreenAuthorName = modListAuthor;

                Hide();
            }
        }
    }
}
