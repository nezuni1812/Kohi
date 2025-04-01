using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();
            ContentFrame.Navigate(
                       typeof(Views.HomePage),
                       null,
                       new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()
                       );

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            CopyImagesIfNotExist();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            //myButton.Content = "Clicked";
        }

        public string GetAppTitleFromSystem()
        {
            //try
            //{
            //    if (Windows.ApplicationModel.Package.Current != null)
            //    {
            //        return Windows.ApplicationModel.Package.Current.DisplayName;
            //    }
            //}
            //catch
            //{

            //}
            return "Kohi";
        }


        private void NavigationViewControl_ItemInvoked(NavigationView sender,
                      NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                ContentFrame.Navigate(typeof(Views.SettingsPage), null, args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
            {
                Type newPage = Type.GetType(args.InvokedItemContainer.Tag.ToString());
                ContentFrame.Navigate(newPage, null, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack) ContentFrame.GoBack();
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavigationViewControl.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(Views.SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems and doesn't have a Tag.
                NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;
            }
            else if (ContentFrame.SourcePageType != null)
            {
                var matchingItem = NavigationViewControl.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(n => n.Tag?.ToString() == ContentFrame.SourcePageType.FullName);

                if (matchingItem != null)
                {
                    NavigationViewControl.SelectedItem = matchingItem;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No match found for {ContentFrame.SourcePageType.FullName}");
                }
            }

            //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
        }

        public async Task CopyImagesIfNotExist()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            var filesToCopy = new (string sourceUri, string destinationFileName)[]
            {
                ("ms-appx:///Assets/kohi_logo.png", "kohi_logo.png"),
                ("ms-appx:///Assets/coffee.jpg", "coffee.jpg"),
                ("ms-appx:///Assets/milk_tea.jpg", "milk_tea.jpg"),
                ("ms-appx:///Assets/freeze.jpg", "freeze.jpg"),
                ("ms-appx:///Assets/tea.jpg", "tea.jpg")
            };

            foreach (var file in filesToCopy)
            {
                Uri imageUri = new Uri(file.sourceUri);
                string imageFileName = file.destinationFileName;

                var existingFile = await localFolder.TryGetItemAsync(imageFileName) as StorageFile;
                if (existingFile == null)
                {
                    try
                    {
                        StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(imageUri);
                        await imageFile.CopyAsync(localFolder, imageFileName, NameCollisionOption.ReplaceExisting);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error while copy {imageFileName}: {ex.Message}");
                    }
                }
                else
                {
                    Debug.WriteLine($"{imageFileName} exists in {localFolder.Path}. No need to copy.");
                }
            }
        }

    }


}