using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Windows.System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.AccessCache;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class newpage : Page
    {
        public newpage()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
        }
        //单例模式
        ViewModels.TodoItemViewModel ViewModel = ViewModels.TodoItemViewModel.GetInstance();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            SystemNavigationManager navigationManager = SystemNavigationManager.GetForCurrentView();

            //判断是否需要重新加载图片
            if (ApplicationData.Current.LocalSettings.Values["TempImage"] != null)
            {
                StorageFile tempimg;
                tempimg = await StorageApplicationPermissions.FutureAccessList.GetFileAsync((string)ApplicationData.Current.LocalSettings.Values["TempImage"]);
                IRandomAccessStream ir = await tempimg.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                image.Source = bi;
                ApplicationData.Current.LocalSettings.Values["TempImage"] = null;
            }


            //判断是否是重新加载
            if (e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NewPage"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["NewPage"] as ApplicationDataCompositeValue;
                    textbox1.Text = (string)composite["textbox1"];
                    textbox2.Text = (string)composite["textbox2"];
                    date.Date = (DateTimeOffset)composite["date"];
                    ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
                }
            }

            //var i = new MessageDialog("Welcome to the new page!").ShowAsync();


            //改为单例模式去掉的代码
            //ViewModel = ((ViewModels.TodoItemViewModel)e.Parameter);
            if (ViewModel.SelectedItem == null)
            {
                CreateButton.Content = "Create";
            }
            else
            {
                CreateButton.Content = "Update";
                CreateButton.Click -= Create;
                CreateButton.Click += Update;
                textbox1.Text = ViewModel.SelectedItem.title;
                textbox2.Text = ViewModel.SelectedItem.description;
                date.Date = ViewModel.SelectedItem.duedate;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= NavigationManager_BackRequested;
            //ViewModel.SelectedItem = null;

            bool suspending = ((App)App.Current).issuspend;
            if (suspending)
            {
                var composite = new ApplicationDataCompositeValue();
                composite["textbox1"] = textbox1.Text;
                composite["textbox2"] = textbox2.Text;
                composite["date"] = date.Date;
                ApplicationData.Current.LocalSettings.Values["NewPage"] = composite;
            }

        }

        private void NavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                ViewModel.RemoveTodoItem(ViewModel.SelectedItem.Id);
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void Update(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                ViewModel.UpdateTodoItem(ViewModel.SelectedItem.Id, textbox1.Text, textbox2.Text, date.Date.Date);
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void Create(object sender, RoutedEventArgs e)
        {
            if (textbox1.Text == "")
            {
                var i = new MessageDialog("The description cannot be empty!").ShowAsync();
                return;
            }
            if (textbox2.Text == "")
            {
                var i = new MessageDialog("The description cannot be empty!").ShowAsync();
                return;
            }
            if (date.Date < DateTimeOffset.Now)
            {
                var i = new MessageDialog("The date you set cannot exceed the due date!").ShowAsync();
                return;
            }
            ViewModel.AddTodoItem(textbox1.Text, textbox2.Text, date.Date.Date);
            textbox1.Text = "";
            textbox2.Text = "";
            date.Date = DateTimeOffset.Now;
            Frame.Navigate(typeof(MainPage));
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            textbox1.Text = "";
            textbox2.Text = "";
            date.Date = DateTimeOffset.Now;
        }

        private async void Select(object sender, RoutedEventArgs e)
        {
          
            //创建和自定义 FileOpenPicker  
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail; //可通过使用图片缩略图创建丰富的视觉显示，以显示文件选取器中的文件  
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

            //选取单个文件  
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            //文件处理  
            if (file != null)
            {
                var inputFile = SharedStorageAccessManager.AddFile(file);
                var destination = await ApplicationData.Current.LocalFolder.CreateFileAsync("Cropped.jpg", CreationCollisionOption.ReplaceExisting);//在应用文件夹中建立文件用来存储裁剪后的图像  
                var destinationFile = SharedStorageAccessManager.AddFile(destination);
                var options = new LauncherOptions();
                options.TargetApplicationPackageFamilyName = "Microsoft.Windows.Photos_8wekyb3d8bbwe";

                //待会要传入的参数  
                var parameters = new ValueSet();
                parameters.Add("InputToken", inputFile);                //输入文件  
                parameters.Add("DestinationToken", destinationFile);    //输出文件  
                parameters.Add("ShowCamera", false);                    //它允许我们显示一个按钮，以允许用户采取当场图象(但是好像并没有什么卵用)  
                parameters.Add("EllipticalCrop", true);                 //截图区域显示为圆（最后截出来还是方形）  
                parameters.Add("CropWidthPixals", 300);
                parameters.Add("CropHeightPixals", 300);

                //调用系统自带截图并返回结果  
                var result = await Launcher.LaunchUriForResultsAsync(new Uri("microsoft.windows.photos.crop:"), options, parameters);

                //储存选择的图片以便重载的时候加载出来
                StorageFile File = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    ApplicationData.Current.LocalSettings.Values["TempImage"] = StorageApplicationPermissions.FutureAccessList.Add(file);
                    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        var srcImage = new BitmapImage();
                        await srcImage.SetSourceAsync(stream);
                        image.Source = srcImage;
                    }
                }
                //按理说下面这个判断应该没问题呀，但是如果裁剪界面点了取消的话后面会出现异常，所以后面我加了try catch  
                if (result.Status == LaunchUriStatus.Success && result.Result != null)
                {
                    //对裁剪后图像的下一步处理  
                    try
                    {
                        // 载入已保存的裁剪后图片  
                        var stream = await destination.OpenReadAsync();
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);

                        // 显示  
                        image.Source = bitmap;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + ex.StackTrace);
                    }
                }
            }
        }
    }
}
