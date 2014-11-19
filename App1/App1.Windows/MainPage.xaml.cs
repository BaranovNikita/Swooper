using System.Collections.Generic;
using System.Linq;
using Windows.Media.Capture;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Storage.Streams;
using Swooper.Common;

namespace Swooper
{
    public sealed partial class MainPage
    {
        private TextBox _tb;
        private StorageFile _file;
        private Vk _vk;
        private ImageHelper helper = new ImageHelper();
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
            save.IsEnabled = false;
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
            var scale = Math.Ceiling(image.PixelWidth > image.PixelHeight
                ? helper.getWidthForImage(image.PixelWidth, 0)
                : helper.getWidthForImage(image.PixelHeight, 1));
            myPicture.Width = Math.Ceiling(image.PixelWidth / scale);
            myPicture.Height = Math.Ceiling(image.PixelHeight / scale);
            myPicture.Source = image;
            border.Background = new SolidColorBrush(Colors.Black);
            border.Width = myPicture.Width + 100;
            border.Height = myPicture.Height + 150;
            Canvas.SetLeft(myPicture, 50);
            Canvas.SetTop(myPicture, 30);
            Canvas.SetLeft(bigTextBox, 10);
            Canvas.SetTop(bigTextBox, myPicture.Height + 35);
            Canvas.SetLeft(smallTextBox, 10);
            Canvas.SetTop(smallTextBox, myPicture.Height + 95);
            bigTextBox.Width = border.Width - 20;
            smallTextBox.Width = border.Width - 20;
            bigTextBox.Visibility = Visibility.Visible;
            smallTextBox.Visibility = Visibility.Visible;
            bigTextBox.Focus(FocusState.Keyboard);
            FontSlider.Margin = new Thickness(5, 40, 0, 0);
            FontSlider.Height = border.Height - 50;
            FontSliderSmall.Margin = new Thickness(border.Width - 50, 40, 0, 0);
            FontSliderSmall.Height = border.Height - 50;
            myFriends.Margin = new Thickness(Window.Current.Bounds.Width - 300, 10, 0, 0);
            FontSlider.Visibility = Visibility.Visible;
            FontSliderSmall.Visibility = Visibility.Visible;
            tbB.Visibility = Visibility.Visible;
            tbS.Visibility = Visibility.Visible;
            tbS.Margin = new Thickness(border.Width - 50, 0, 0, 0);
            save.IsEnabled = true;
        }
        

        private async void save_button_click(object sender, RoutedEventArgs e)
        {
            VisibleElements(false);
            await helper.CreateSaveBitmapAsync(border);
            VisibleElements(true);
        }        

        private async void vk_click(object sender, RoutedEventArgs e)
        {
            _vk = new Vk();
            var temp = new ListView();
            if (await _vk.OAuthVk() == "Cancel") return;
            try
            {
                temp = await _vk.GetFriends();
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
                temp.Items.RemoveAt(0);
                if (myFriends.Items != null)
                    myFriends.Items.Add(tempitem);
            }
            myFriends.Header = _vk._online + " друзей онлайн";
        }


        
        public void VisibleElements(bool flag)
        {
            if (!flag)
            {
                FontSlider.Visibility = Visibility.Collapsed;
                FontSliderSmall.Visibility = Visibility.Collapsed;
                tbB.Visibility = Visibility.Collapsed;
                tbS.Visibility = Visibility.Collapsed;
            }
            else
            {
                FontSlider.Visibility = Visibility.Visible;
                FontSliderSmall.Visibility = Visibility.Visible;
                tbB.Visibility = Visibility.Visible;
                tbS.Visibility = Visibility.Visible;
         
            }
        }

        private async void ToMessage(object sender, RoutedEventArgs e)
        {
            var pressed = (Button)sender;
            if (_file == null)
            {
                var noFile = new MessageDialog("Картинка не выбрана!");
                noFile.ShowAsync();
            }
            else
            {
                VisibleElements(false);
                await helper.CreateSaveBitmapAsync(border);
                LoginDialog.IsOpen = false;
                foreach (var friend in _vk.Friends.Where(friend => friend.Name == LoginDialog.Title.Substring(25)))
                {
                    await _vk.PhotoTo(friend.Id, helper.ReadFile(_file).Result, _file, pressed.Content != null && pressed.Content.ToString() == "Отправить на стену" ? 1 : 2, myText.Text);
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
            if (smallTextBox.FontSize < FontSliderSmall.Value)
                smallTextBox.Height += 2;
            smallTextBox.FontSize = FontSliderSmall.Value;
        }
        private void ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (FontSlider == null) return;
            if (bigTextBox.FontSize < FontSlider.Value)
                bigTextBox.Height += 2;
            bigTextBox.FontSize = FontSlider.Value;
        }

        private void photo_click(object sender, RoutedEventArgs e)
        {
            CameraCapture();
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

        private void OpenContextB(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
            _tb = (TextBox)sender;

            var f = new Popup
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

            var b = new Button
            {
                FontSize = 15,
                Width = 50,
                Height = 50,
                Content = "B",
                FontWeight = FontWeights.Bold
            };
            b.Click += b_Click;
            sp.Children.Add(b);
            var k = new Button
            {
                FontSize = 15,
                Width = 50,
                Height = 50,
                Content = "K",
                FontStyle = FontStyle.Italic
            };
            k.Click += k_Click;
            sp.Children.Add(k);
            var fonts = new List<string> { "Tahoma", "Times New Roman", "Comic Sans" };
            var cb = new ComboBox
            {
                Width = 150,
                ItemsSource = fonts,
                SelectedValue = _tb.FontFamily.Source
            };
            cb.SelectionChanged += cb_SelectionChanged;
            sp.Children.Add(cb);
            f.Child = sp;
        }

        private void cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _tb.FontFamily = new FontFamily(e.AddedItems[0].ToString());
        }

        void k_Click(object sender, RoutedEventArgs e)
        {
            _tb.FontStyle = bigTextBox.FontStyle == FontStyle.Normal ? FontStyle.Italic : FontStyle.Normal;
        }

        void b_Click(object sender, RoutedEventArgs e)
        {
            _tb.FontWeight = bigTextBox.FontWeight.Weight == 400 ? FontWeights.Bold : FontWeights.Normal;
        }
    }
}