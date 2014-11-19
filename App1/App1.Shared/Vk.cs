using System.IO;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using HttpClient = System.Net.Http.HttpClient;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Swooper
{
    class Vk
    {
        private int _idUser;
        private string _tokenUser;
        public Friend[] Friends;
        private ListView _lv;
        public int Online;

        public async Task<string> OAuthVk()
        {
            const string vkUri = "https://oauth.vk.com/authorize?client_id=4608665&scope=wall,photos,friends,messages&" +
                                 "redirect_uri=http://oauth.vk.com/blank.html&display=popup&v=5.26&response_type=token";
            var requestUri = new Uri(vkUri);
            var callbackUri = new Uri("http://oauth.vk.com/blank.html");

            var result = await WebAuthenticationBroker.AuthenticateAsync(
                WebAuthenticationOptions.None, requestUri, callbackUri);

            switch (result.ResponseStatus)
            {
                case WebAuthenticationStatus.ErrorHttp:
                    var dialogError = new MessageDialog("Не удалось открыть страницу сервиса\n" +
                                                        "Попробуйте войти в приложения позже!", "Ошибка");
                    dialogError.ShowAsync();
                    break;
                case WebAuthenticationStatus.Success:
                    var responseString = result.ResponseData;
                    char[] separators = { '=', '&' };
                    var responseContent = responseString.Split(separators);
                    var accessToken = responseContent[1];
                    var userId = Int32.Parse(responseContent[5]);
                    _idUser = userId;
                    _tokenUser = accessToken;
                    break;
                case WebAuthenticationStatus.UserCancel:
                    return "Cancel";
            }
            return "ok";
        }

        public async Task<ListView> GetFriends()
        {
            _lv = new ListView();
            var myfriends =
                        await
                            MakeWebRequest("https://api.vk.com/method/friends.get?user_id=" + _idUser +
                                           "&v=5.25&order=hints&fields=nickname&access_token=" + _tokenUser);
            try
            {
                var json = JObject.Parse(myfriends);
                var count = json["response"]["count"].ToObject<int>();
                var j = 0;
                Friends = new Friend[count];

                for (var i = 0; i < count; i++)
                {
                    if (json["response"]["items"][i]["online"].ToObject<int>() != 1) continue;
                    Friends[j] =
                        new Friend(
                            json["response"]["items"][i]["first_name"] + " " +
                            json["response"]["items"][i]["last_name"],
                            json["response"]["items"][i]["id"].ToObject<int>(),
                            json["response"]["items"][i]["online"].ToObject<int>());
                    j++;
                    Online++;
                }
                for (var i = 0; i < count; i++)
                {
                    if (json["response"]["items"][i]["online"].ToObject<int>() != 0) continue;
                    Friends[j] =
                        new Friend(
                            json["response"]["items"][i]["first_name"] + " " +
                            json["response"]["items"][i]["last_name"],
                            json["response"]["items"][i]["id"].ToObject<int>(),
                            json["response"]["items"][i]["online"].ToObject<int>());
                    j++;
                }

                for (var i = 0; i < count; i++)
                {
                    var li = new ListViewItem
                    {
                        Background =
                            new SolidColorBrush(Friends[i].Online == 1 ? Colors.MediumSeaGreen : Colors.LightPink),
                        Content = Friends[i].Name
                    };
                    if (_lv.Items != null) _lv.Items.Add(li);
                }
                return _lv;
            }
            catch
            {
                var dialog = new MessageDialog("Произошла ошибка");
                dialog.ShowAsync();
                return null;
            }

        }

        public async Task<string> SavePhoto(string post, int id, int var)
        {
            var variant = (var == 1) ? "saveWallPhoto" : "saveMessagesPhoto";
            try
            {
                var json = JObject.Parse(post);
                var server = json["server"].ToString();
                var photo = json["photo"].ToString();
                var hash = json["hash"].ToString();
                var result = await
                    MakeWebRequest("https://api.vk.com/method/photos." + variant + "?user_id=" + id +
                                   "&v=5.26&photo=" + photo + "&server=" + server + "&hash=" + hash + "&access_token=" +
                                   _tokenUser);
                return result;
            }
            catch
            {
                var dialog = new MessageDialog("Произошла ошибка");
                dialog.ShowAsync();
                return null;
            }

        }

        private async Task<string> PhotosGetWallUploadServer(int groupId)
        {
            var requestPath = "https://api.vk.com/method/photos.getWallUploadServer?";
            requestPath += "user_id=" + groupId;
            requestPath += "&v=5.26";
            requestPath += "&access_token=" + _tokenUser;
            try
            {
                var json = JObject.Parse(await MakeWebRequest(requestPath));
                return json["response"]["upload_url"].ToString();
            }
            catch
            {
                var dialog = new MessageDialog("Произошла ошибка");
                dialog.ShowAsync();
                return null;
            }

        }
        private async Task<string> PhotosGetMessageUploadServer()
        {
            var requestPath = "https://api.vk.com/method/photos.getMessagesUploadServer?";
            requestPath += "&v=5.26";
            requestPath += "&access_token=" + _tokenUser;
            try
            {
                var json = JObject.Parse(await MakeWebRequest(requestPath));
                return json["response"]["upload_url"].ToString();
            }
            catch
            {
                var dialog = new MessageDialog("Произошла ошибка");
                dialog.ShowAsync();
                return null;
            }
        }
        public async Task<string> MakeWebRequest(string url)
        {
            var http = new HttpClient();
            var response = await http.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> UploadFile(byte[] pic, string uploadUrl, StorageFile file)
        {
            var request = (HttpWebRequest)WebRequest.Create(uploadUrl);
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            const string templateFile = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n";
            const string templateEnd = "--{0}--\r\n\r\n";
            request.Method = "POST";
            request.ContentType = String.Format("multipart/form-data; boundary={0}", boundary);
            var stream = await request.GetRequestStreamAsync();
            var filePath = file.Name;
            const string fileType = "image/jpeg";
            const string name = "photo";
            var contentFile = Encoding.UTF8.GetBytes(String.Format(templateFile, boundary, name, filePath, fileType));
            stream.Write(contentFile, 0, contentFile.Length);
            stream.Write(pic, 0, pic.Length);
            var lineFeed = Encoding.UTF8.GetBytes("\r\n");
            stream.Write(lineFeed, 0, lineFeed.Length);
            var contentEnd = Encoding.UTF8.GetBytes(String.Format(templateEnd, boundary));
            stream.Write(contentEnd, 0, contentEnd.Length);
            var webResponse = (HttpWebResponse)await request.GetResponseAsync();
            var read = new StreamReader(webResponse.GetResponseStream());
            return read.ReadToEnd();
        }
        public async Task<string> PostPhoto(int idUser, string a, int var, string message)
        {
            var variant = (var == 1) ? "wall.post?" : "messages.send?";
            var variantSend = (var == 1) ? "owner_id" : "user_id";
            var variantAttach = (var == 1) ? "attachments" : "attachment";
            var id = idUser;
            var requestPath = "https://api.vk.com/method/" + variant;
            requestPath += variantSend + "=" + id;
            requestPath += "&v=5.26";
            requestPath += "&message=" + message;
            try
            {
                requestPath += "&" + variantAttach + "=photo" + JObject.Parse(a)["response"][0]["owner_id"] + "_" +
                               JObject.Parse(a)["response"][0]["id"];
                requestPath += "&access_token=" + _tokenUser;

                return await MakeWebRequest(requestPath);
            }
            catch
            {
                var dialog = new MessageDialog("Произошла ошибка");
                dialog.ShowAsync();
                return null;
            }
        }

        public async Task<string> PhotoTo(int id, byte[] file, StorageFile stfile, int var, string message)
        {
            var lul = var == 1 ? await PhotosGetWallUploadServer(id) : await PhotosGetMessageUploadServer();
            var post = await UploadFile(file, lul, stfile);
            var save = await SavePhoto(post, id, var);
            return await PostPhoto(id, save, var, message);
        }
    }
}
