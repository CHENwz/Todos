using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;

            //单例模式
            this.ViewModel =  ViewModels.TodoItemViewModel.GetInstance();
        }

        ViewModels.TodoItemViewModel ViewModel { get; set; }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool suspending = ((App)App.Current).issuspend;
            if (suspending)
            {
                var composite = new ApplicationDataCompositeValue();
                composite["title"] = title.Text;
                composite["description"] = description.Text;
                composite["duedate"] = duedate.Date;
                composite["Visible"] = ViewModel.AllItems[0].completed;
                ApplicationData.Current.LocalSettings.Values["MainPage"] = composite;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("MainPage");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("MainPage"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["MainPage"] as ApplicationDataCompositeValue;
                    title.Text = (string)composite["title"];
                    description.Text = (string)composite["description"];
                    duedate.Date = (DateTimeOffset)composite["duedate"];
                    ViewModel.AllItems[0].completed = (bool)composite["Visible"];
                    ApplicationData.Current.LocalSettings.Values.Remove("MainPage");
                }
            }


        }

        private void TodoItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            //将selectitem赋值，如果点击了item事件了的话
            ViewModel.SelectedItem = (Models.TodoItem)(e.ClickedItem);
            if (grid.ActualWidth <= 800)
            {
                Frame.Navigate(typeof(newpage));
            }
            else
            {
                if (ViewModel.SelectedItem == null)
                {
                    CreateButton.Content = "Create";
                    CreateButton.Click += createClick;
                }
                else
                {
                    CreateButton.Content = "Update";
                    CreateButton.Click -= createClick;
                    CreateButton.Click += updateClick;
                    title.Text = ViewModel.SelectedItem.title;
                    description.Text = ViewModel.SelectedItem.description;
                    duedate.Date = ViewModel.SelectedItem.duedate;
                }
            }
        }

        private void change(object sender, RoutedEventArgs e)
        {
            if (All.ActualWidth < 801)
            {
                ViewModel.SelectedItem = null;
                this.Frame.Navigate(typeof(newpage));
            }
            
        }


        private void createClick(object sender, RoutedEventArgs e)
        {
            if (title.Text == "")
            {
                var i = new MessageDialog("The title cannot be empty!").ShowAsync();
                return;
            }
            if (description.Text == "")
            {
                var i = new MessageDialog("The description cannot be empty!").ShowAsync();
                return;
            }
            if (duedate.Date < DateTimeOffset.Now)
            {
                var i = new MessageDialog("The date you set cannot exceed the due date!").ShowAsync();
                return;
            }
            ViewModel.AddTodoItem(title.Text, description.Text, duedate.Date.Date);
            title.Text = "";
            description.Text = "";
            duedate.Date = DateTimeOffset.Now;
        }

        private void updateClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                ViewModel.UpdateTodoItem(ViewModel.SelectedItem.Id, title.Text, description.Text, duedate.Date.Date);
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void cancelClick(object sender, RoutedEventArgs e)
        {
            title.Text = "";
            description.Text = "";
            duedate.Date = DateTimeOffset.Now;
        }
    }
}
