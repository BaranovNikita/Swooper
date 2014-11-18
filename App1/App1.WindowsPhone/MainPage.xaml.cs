using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Popups;
using Windows.Storage.Streams;
// Документацию по шаблону элемента "Основная страница" см. по адресу http://go.microsoft.com/fwlink/?LinkID=390556
using Swooper.Common;

namespace Swooper
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFile _file;
        private Vk _vk;
        private readonly ObservableDictionary _defaultViewModel = new ObservableDictionary();

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public MainPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Получает объект <see cref="NavigationHelper"/>, связанный с данным объектом <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Получает модель представлений для данного объекта <see cref="Page"/>.
        /// Эту настройку можно изменить на модель строго типизированных представлений.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Заполняет страницу содержимым, передаваемым в процессе навигации.  Также предоставляется любое сохраненное состояние
        /// при повторном создании страницы из предыдущего сеанса.
        /// </summary>
        /// <param name="sender">
        /// Источник события; как правило, <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Данные события, предоставляющие параметр навигации, который передается
        /// <see cref="Frame.Navigate(Type, Object)"/> при первоначальном запросе этой страницы и
        /// словарь состояний, сохраненных этой страницей в ходе предыдущего
        /// сеанса.  Это состояние будет равно NULL при первом посещении страницы.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Сохраняет состояние, связанное с данной страницей, в случае приостановки приложения или
        /// удаления страницы из кэша навигации.  Значения должны соответствовать требованиям сериализации
        /// <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">Источник события; как правило, <see cref="NavigationHelper"/></param>
        /// <param name="e">Данные события, которые предоставляют пустой словарь для заполнения
        /// сериализуемым состоянием.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region Регистрация NavigationHelper

        /// <summary>
        /// Методы, предоставленные в этом разделе, используются исключительно для того, чтобы
        /// NavigationHelper для отклика на методы навигации страницы.
        /// <para>
        /// Логика страницы должна быть размещена в обработчиках событий для 
        /// <see cref="NavigationHelper.LoadState"/>
        /// и <see cref="NavigationHelper.SaveState"/>.
        /// Параметр навигации доступен в методе LoadState 
        /// в дополнение к состоянию страницы, сохраненному в ходе предыдущего сеанса.
        /// </para>
        /// </summary>
        /// <param name="e">Предоставляет данные для методов навигации и обработчики
        /// событий, которые не могут отменить запрос навигации.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
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
                ? getWidthForImage(image.PixelWidth, 0)
                : getWidthForImage(image.PixelHeight, 1));
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
        }
        private double getWidthForImage(double widthh, int type)
        {
            var scale = 0;
            var width = widthh;
            while ((width / 1.1) > (type == 1 ? Window.Current.Bounds.Height - 300 : 600))
            {
                scale++;
                width = width / 1.1;
            }
            width /= 1.1;
            return scale == 0 ? 1 : widthh / width;

        }

        private async void save_button_click(object sender, RoutedEventArgs e)
        {
            FontSlider.Visibility = Visibility.Collapsed;
            FontSliderSmall.Visibility = Visibility.Collapsed;
            tbB.Visibility = Visibility.Collapsed;
            tbS.Visibility = Visibility.Collapsed;
            await CreateSaveBitmapAsync(border);
        }

        private async Task CreateSaveBitmapAsync(FrameworkElement canvas)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(canvas);

            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("JPEG Image", new[] { ".jpg" });
            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                var pixels = await renderTargetBitmap.GetPixelsAsync();
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await
                        BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    var bytes = pixels.ToArray();
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                         BitmapAlphaMode.Ignore,
                                         (uint)canvas.Width, (uint)canvas.Height,
                                         96, 96, bytes);

                    await encoder.FlushAsync();
                }
            }
            _file = file;
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
                if (myFriends.Items != null) myFriends.Items.Add(tempitem);
            }
        }


        public async Task<byte[]> ReadFile(StorageFile file)
        {
            byte[] fileBytes;
            using (var stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            return fileBytes;
        }


        private async void ToWall(object sender, RoutedEventArgs e)
        {
            if (_file == null)
            {
                var noFile = new MessageDialog("Картинка не выбрана!");
                noFile.ShowAsync();
            }
            else
            {
                await CreateSaveBitmapAsync(border);
                LoginDialog.IsOpen = false;
                foreach (var friend in _vk.Friends.Where(friend => friend.Name == LoginDialog.Title.Substring(25)))
                {
                    await _vk.PhotoTo(friend.Id, ReadFile(_file).Result, _file, 1, myText.Text);
                }

                var dialogSuccess = new MessageDialog("Успешно отправлено!");
                dialogSuccess.ShowAsync();
            }
        }
        private async void ToMessage(object sender, RoutedEventArgs e)
        {
            if (_file == null)
            {
                var noFile = new MessageDialog("Картинка не выбрана!");
                noFile.ShowAsync();
            }
            else
            {
                await CreateSaveBitmapAsync(border);
                LoginDialog.IsOpen = false;
                foreach (var friend in _vk.Friends.Where(friend => friend.Name == LoginDialog.Title.Substring(25)))
                {
                    await _vk.PhotoTo(friend.Id, ReadFile(_file).Result, _file, 2, myText.Text);
                }
                var dialogSuccess = new MessageDialog("Успешно отправлено!");
                dialogSuccess.ShowAsync();
            }
        }

        private void click_item(object sender, ItemClickEventArgs e)
        {
            LoginDialog.Title = "Отправка изображения для " + e.ClickedItem;
            LoginDialog.IsOpen = true;
        }

        private void ValueChanged_sm(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (FontSlider == null) return;
            if (smallTextBox.FontSize < FontSliderSmall.Value)
                smallTextBox.Height += 2;
            else
                smallTextBox.Height -= 2;
            smallTextBox.FontSize = FontSliderSmall.Value;
            smallTextBox.Text = smallTextBox.Text;
        }
        private void ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (FontSlider == null) return;
            if (bigTextBox.FontSize < FontSlider.Value)
                bigTextBox.Height += 2;
            else
                bigTextBox.Height -= 2;
            bigTextBox.FontSize = FontSlider.Value;
            bigTextBox.Text = bigTextBox.Text;
        }

        private void photo_click(object sender, RoutedEventArgs e)
        {
            CameraCapture();
        }
        async private void CameraCapture()
        {
            //var cameraUi = new CameraCaptureUI();
            //cameraUi.PhotoSettings.AllowCropping = false;
            //cameraUi.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.MediumXga;

            //var capturedMedia =
            //    await cameraUi.CaptureFileAsync(CameraCaptureUIMode.Photo);

            //if (capturedMedia != null)
            //{
            //    using (var streamCamera = await capturedMedia.OpenAsync(FileAccessMode.Read))
            //    {
            //        var bitmapCamera = new BitmapImage();
            //        bitmapCamera.SetSource(streamCamera);
            //        var width = bitmapCamera.PixelWidth;
            //        var height = bitmapCamera.PixelHeight;
            //        var wBitmap = new WriteableBitmap(width, height);
            //        using (var stream = await capturedMedia.OpenAsync(FileAccessMode.Read))
            //        {
            //            wBitmap.SetSource(stream);
            //        }
            //    }
            //}
            //_file = capturedMedia;
            //SetCanvas();
        }

    }
}
