﻿using System.Collections.Generic;
using System.Linq;
using Windows.Media.Capture;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Swooper.Common;

namespace Swooper
{
    public sealed partial class MainPage
    {
        public double DefaultWidth;
        private bool _first = true;
        private TextBox _titles;
        private StorageFile _file;
        private Vk _vk;
        private readonly ImageHelper _helper = new ImageHelper();
        private readonly ObservableDictionary _defaultViewModel = new ObservableDictionary();
        public ObservableDictionary DefaultViewModel
        {
            get { return _defaultViewModel; }
        }
        public NavigationHelper NavigationHelper { get; private set; }


        public MainPage()
        {
            InitializeComponent();
            NavigationHelper = new NavigationHelper(this);
            NavigationHelper.LoadState += navigationHelper_LoadState;
            NavigationHelper.SaveState += navigationHelper_SaveState;
            if (_first)
            {
                FirstStart();
            }
        }
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void FirstStart()
        {
            DefaultWidth = Window.Current.Bounds.Width;
            _file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Yappi.jpg"));
        }
        private async void open_picture_click(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.List
            };
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".bmp");
            var asyncOp = filePicker.PickSingleFileAsync();
            _file = await asyncOp;
            SetCanvas();
        }

        private async void SetCanvas()
        {
            if (_file == null) return;
            var stream = await _file.OpenAsync(FileAccessMode.Read);
            var image = new BitmapImage();
            image.SetSource(stream);
            var scale = image.PixelWidth > image.PixelHeight
                ? _helper.GetWidthForImage(image.PixelWidth, 0)
                : _helper.GetWidthForImage(image.PixelHeight, 1);
            MyPicture.Width = image.PixelWidth / scale;
            MyPicture.Height = image.PixelHeight / scale;
            MyPicture.Width = Math.Ceiling(MyPicture.Width);
            MyPicture.Height = Math.Ceiling(MyPicture.Height);
            MyPicture.Source = image;
            Border.Background = new SolidColorBrush(Colors.Black);
            Border.Width = MyPicture.Width + 100;
            Border.Height = MyPicture.Height + 150;
            Canvas.SetLeft(MyPicture, 50);
            Canvas.SetTop(MyPicture, 30);
            Canvas.SetLeft(BigTextBox, 10);
            Canvas.SetTop(BigTextBox, MyPicture.Height + 35);
            Canvas.SetLeft(SmallTextBox, 10);
            Canvas.SetTop(SmallTextBox, MyPicture.Height + 95);
            BigTextBox.Text = _first ? "Добро пожаловать" : "Введите текст";
            SmallTextBox.Text = _first ? "На Yappi Days!" : "Введите текст"; ;
            BigTextBox.Width = Border.Width - 20;
            SmallTextBox.Width = Border.Width - 20;
            BigTextBox.Visibility = Visibility.Visible;
            SmallTextBox.Visibility = Visibility.Visible;
            BigTextBox.Focus(FocusState.Keyboard);
            FontSlider.Margin = new Thickness(5, 40, 0, 0);
            FontSlider.Height = Border.Height - 50;
            FontSliderSmall.Margin = new Thickness(Border.Width - 50, 40, 0, 0);
            FontSliderSmall.Height = Border.Height - 50;
            MyFriends.Margin = new Thickness(Window.Current.Bounds.Width - 300, 10, 0, 0);
            VisibleElements(true);
            TitleRight.Margin = new Thickness(Border.Width - 50, 0, 0, 0);
            _first = false;
        }


        private async void save_button_click(object sender, RoutedEventArgs e)
        {
            if (_file == null) return;
            VisibleElements(false);
            _file = await _helper.CreateSaveBitmapAsync(Border);
            VisibleElements(true);
        }

        public void VisibleElements(bool flag)
        {
            if (!flag)
            {
                FontSlider.Visibility = Visibility.Collapsed;
                FontSliderSmall.Visibility = Visibility.Collapsed;
                TitleLeft.Visibility = Visibility.Collapsed;
                TitleRight.Visibility = Visibility.Collapsed;
            }
            else
            {
                FontSlider.Visibility = Visibility.Visible;
                FontSliderSmall.Visibility = Visibility.Visible;
                TitleLeft.Visibility = Visibility.Visible;
                TitleLeft.Foreground = new SolidColorBrush(Colors.FloralWhite);
                TitleRight.Visibility = Visibility.Visible;
                TitleRight.Foreground = new SolidColorBrush(Colors.FloralWhite);
            }
        }

        private async void ToMessage(object sender, RoutedEventArgs e)
        {
            var pressed = (Button)sender;
            if (pressed.Name == "BackInLogin")
            {
                LoginDialog.IsOpen = false;
                return;
            }
            if (_file == null)
            {
                var noFile = new MessageDialog("Картинка не выбрана!");
                noFile.ShowAsync();
            }
            else
            {
                VisibleElements(false);
                _file = await _helper.CreateSaveBitmapAsync(Border);
                LoginDialog.IsOpen = false;
                foreach (var friend in _vk.Friends.Where(friend => friend.Name == LoginDialog.Title.Substring(25)))
                {
                    await _vk.PhotoTo(friend.Id, _helper.ReadFile(_file).Result, _file, pressed.Content != null && pressed.Content.ToString() == "Отправить на стену" ? 1 : 2, MyText.Text);
                }
                var dialogSuccess = new MessageDialog("Успешно отправлено!");
                await dialogSuccess.ShowAsync();
                VisibleElements(true);
            }
        }

        private void click_item(object sender, ItemClickEventArgs e)
        {
            if (_file == null) return;
            LoginDialog.Title = "Отправка изображения для " + e.ClickedItem;
            LoginDialog.IsOpen = true;
        }

        private void ValueChanged_sm(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (FontSlider == null) return;
            if (SmallTextBox.FontSize < FontSliderSmall.Value)
                SmallTextBox.Height += 2;
            SmallTextBox.FontSize = FontSliderSmall.Value;
        }
        private void ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (FontSlider == null) return;
            if (BigTextBox.FontSize < FontSlider.Value)
                BigTextBox.Height += 2;
            BigTextBox.FontSize = FontSlider.Value;
        }

        async private void CameraCapture()
        {
            var cameraUi = new CameraCaptureUI();
            cameraUi.PhotoSettings.AllowCropping = false;
            cameraUi.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.MediumXga;
            try
            {
                var capturedMedia =
                    await cameraUi.CaptureFileAsync(CameraCaptureUIMode.Photo);

                if (capturedMedia != null)
                {
                    using (var streamCamera = await capturedMedia.OpenAsync(FileAccessMode.Read))
                    {
                        var bitmapCamera = new BitmapImage();
                        bitmapCamera.SetSource(streamCamera);
                        var width = bitmapCamera.PixelWidth;
                        var height = bitmapCamera.PixelHeight;
                        var wBitmap = new WriteableBitmap(width, height);
                        using (var stream = await capturedMedia.OpenAsync(FileAccessMode.Read))
                        {
                            wBitmap.SetSource(stream);
                        }
                    }
                }
                _file = capturedMedia;
            }
            catch (Exception)
            {
                var errorDialog = new MessageDialog("Камера отсутствует");
                errorDialog.ShowAsync();
            }
            SetCanvas();
        }

        private void OpenContext(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
            _titles = (TextBox)sender;

            var contextMenu = new Popup
            {
                IsLightDismissEnabled = true,
                VerticalOffset = e.CursorTop - 50,
                HorizontalOffset = e.CursorLeft,
                Height = 350,
                IsOpen = true
            };
            var sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Background = new SolidColorBrush(Colors.Gray),
                Width = 300,
                Height = 50
            };

            var bold = new Button
            {
                FontSize = 15,
                Width = 50,
                Height = 50,
                Content = "B",
                FontWeight = FontWeights.Bold
            };
            bold.Click += BoldText;
            sp.Children.Add(bold);
            var italic = new Button
            {
                FontSize = 15,
                Width = 50,
                Height = 50,
                Content = "K",
                FontStyle = FontStyle.Italic
            };
            italic.Click += ItalicText;
            sp.Children.Add(italic);
            var fonts = new ComboBox
            {
                Width = 150,
                ItemsSource = new List<string> { "Tahoma", "Times New Roman", "Comic Sans" },
                SelectedValue = _titles.FontFamily.Source
            };
            fonts.SelectionChanged += ChangeFont;
            sp.Children.Add(fonts);
            contextMenu.Child = sp;
        }

        private void ChangeFont(object sender, SelectionChangedEventArgs e)
        {
            _titles.FontFamily = new FontFamily(e.AddedItems[0].ToString());
        }

        void ItalicText(object sender, RoutedEventArgs e)
        {
            _titles.FontStyle = BigTextBox.FontStyle == FontStyle.Normal ? FontStyle.Italic : FontStyle.Normal;
        }

        void BoldText(object sender, RoutedEventArgs e)
        {
            _titles.FontWeight = BigTextBox.FontWeight.Weight == 400 ? FontWeights.Bold : FontWeights.Normal;
        }

        private void UseCamera(object sender, RoutedEventArgs e)
        {
            CameraCapture();
        }

        private async void VkontakteClick(object sender, RoutedEventArgs e)
        {
            _vk = new Vk();
            var temp = new ListView();
            var tempForCombo = new ComboBox();
            try
            {
                if (await _vk.OAuthVk() == "Cancel") return;
                var array =  await _vk.GetFriends();
                temp = (ListView) array[0];
                tempForCombo = (ComboBox) array[1];
            }
            catch (Exception)
            {
                var dialog = new MessageDialog("Произошла ошибка");
                dialog.ShowAsync();
            }
            if (temp.Items == null) return;
            for (var i = 0; i < temp.Items.Count; i++)
            {
                var tempitem = temp.Items[0];
                if (tempForCombo.Items == null) continue;
                var tempCombo = tempForCombo.Items[0];
                temp.Items.RemoveAt(0);
                tempForCombo.Items.RemoveAt(0);
                if (MyFriends.Items != null)
                    MyFriends.Items.Add(tempitem);
                if (ComboFriends.Items != null)
                    ComboFriends.Items.Add(tempCombo);
            }
            MyFriends.Header = _vk.Online + " друзей онлайн";
            ShowFriends();
        }

        private void Size_Changed(object sender, SizeChangedEventArgs e)
        {
            SetCanvas();
            ShowFriends();
        }

        private void ShowFriends()
        {
            if (_vk == null) return;
            if (Window.Current.Bounds.Width < DefaultWidth)
            {
                ComboFriends.Visibility = Visibility.Visible;
                BackInLogin.Visibility = Visibility.Visible;
                LoginDialog.Width = Window.Current.Bounds.Width - 50;
                LoginDialog.BackButtonVisibility = Visibility.Collapsed;
                MyFriends.Visibility = Visibility.Collapsed;
            }
            else
            {
                BackInLogin.Visibility = Visibility.Collapsed;
                MyFriends.Visibility = Visibility.Visible;
                LoginDialog.BackButtonVisibility = Visibility.Visible;
                ComboFriends.Visibility = Visibility.Collapsed;
            }
        }

        private void ChooseCombo(object sender, SelectionChangedEventArgs e)
        {
            var typeItem = (ComboBoxItem)ComboFriends.SelectedItem;
            var value = typeItem.Content.ToString();
            LoginDialog.Title = "Отправка изображения для " + value;
            LoginDialog.IsOpen = true;
        }
    }
}