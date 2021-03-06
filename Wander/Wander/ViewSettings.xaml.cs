﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Resources.Core;
using Windows.ApplicationModel.Resources;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Wander
{
    public sealed partial class ViewSettings : UserControl
    {
        private string language = "Talen";
        private int selectedIndex = 0;
        private List<WanderLib.Language> languages;
        MainPage page;
        private DataController datacontroller;
        private Boolean refresh = false;
        public ViewSettings(MainPage page)
        {
            this.InitializeComponent();
            GridSetting.DataContext = language;
            this.page = page;
            fillLanguageList();
            
        }

        private void fillLanguageList()
        {
            datacontroller = DataController.getInstance();
            languages = datacontroller.giveAllLanguages();
            List<string> languageNames = new List<string>();

            foreach(WanderLib.Language l in languages)
            {
                languageNames.Add(l.name);
            }

            if (datacontroller.selectedLanguage == null)
                selectedIndex = 0;
            else
                selectedIndex = datacontroller.selectedLanguage;
            LanguageComboBox.ItemsSource = languageNames;
            LanguageComboBox.SelectedIndex = selectedIndex;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            datacontroller.selectedLanguage = selectedIndex;
            page.updateStringsWithCurrentLanguage();
            page.setHelp(refresh);
            System.Diagnostics.Debug.WriteLine(Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (refresh)
            {
                //var _Frame = Window.Current.Content as Frame;
                //_Frame.Navigate(_Frame.Content.GetType());
                //_Frame.GoBack();
                datacontroller.selectedLanguage = selectedIndex;
                refresh = false;
                page.updateStringsWithCurrentLanguage();
            }
            page.removeChild(this);
            
        }

        public List<string> ListLanguages { get; set; }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int tempIndex = selectedIndex;
            selectedIndex = LanguageComboBox.SelectedIndex;
            WanderLib.Language chosenLanguage = languages[selectedIndex];
            if (chosenLanguage.name == "English")
            {
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en";
            }
            else if(chosenLanguage.name == "Nederlands")
            {
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "nl";
            }
            datacontroller.session.settings.language  = new WanderLib.Language(chosenLanguage.name);
            datacontroller.saveSession();

            if (tempIndex != selectedIndex)
                refresh = true;
            
        }

        public void updateWithCurrentLanguage()
        {
            ResourceLoader rl = new ResourceLoader();
            languageBox.DataContext = rl.GetString("taalSetting");
        }
    }
}
