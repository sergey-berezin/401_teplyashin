using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Contracts;
using System.Net.Http;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        const string url = "https://localhost:7200/api/images";
        CancellationToken token;
        CancellationTokenSource source;
        ObservableCollection<Image> neutral = new();
        ObservableCollection<Image> happiness = new();
        ObservableCollection<Image> surprise = new();
        ObservableCollection<Image> sadness = new();
        ObservableCollection<Image> anger = new();
        ObservableCollection<Image> disgust = new();
        ObservableCollection<Image> fear = new();
        ObservableCollection<Image> contempt = new();
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            lv_neutral.ItemsSource = neutral;
            lv_happiness.ItemsSource = happiness;
            lv_surprise.ItemsSource = surprise;
            lv_sadness.ItemsSource = sadness;
            lv_anger.ItemsSource = anger;
            lv_disgust.ItemsSource = disgust;
            lv_fear.ItemsSource = fear;
            lv_contempt.ItemsSource = contempt;
        }

        public async void LoadData()
        {
            HttpClient client = new HttpClient();
            string result = await client.GetStringAsync(url);
            var ids = JsonConvert.DeserializeObject<List<int>>(result);
            foreach (var id in ids)
            {
                var status = await client.GetAsync($"{url}/{id}");
                if (status.StatusCode != System.Net.HttpStatusCode.OK)
                    continue;
                var res = await client.GetStringAsync($"{url}/{id}");
                var img = JsonConvert.DeserializeObject<Image>(res);
                switch (img.category)
                {
                    case "neutral":
                        neutral.Add(img);
                        break;
                    case "happiness":
                        happiness.Add(img);
                        break;
                    case "surprise":
                        surprise.Add(img);
                        break;
                    case "sadness":
                        sadness.Add(img);
                        break;
                    case "anger":
                        anger.Add(img);
                        break;
                    case "disgust":
                        disgust.Add(img);
                        break;
                    case "fear":
                        fear.Add(img);
                        break;
                    case "contempt":
                        contempt.Add(img);
                        break;
                    default:
                        break;
                }
            }
        }

        private async void command_open(object sender, RoutedEventArgs e)
        {
            Files_selection.IsEnabled = false;
            del.IsEnabled = false;
            pbStatus.Value = 0;
            this.source = new CancellationTokenSource();
            this.token = source.Token;
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "(*.jpg, *.png)|*.jpg; *.png";
            var result = dialog.ShowDialog();
            int len = dialog.FileNames.Length;
            if (len == 0)
            {
                Files_selection.IsEnabled = true;
                return;
            }
            int step = 1000 / len;
            bool flag = true;
            HttpClient client = new HttpClient();
            for (int i = 0; i < len; i++)
            {
                try
                {
                    string path = dialog.FileNames[i];
                    byte[] img_ = File.ReadAllBytes(path);
                    Data obj = new Data(img_, path);
                    var s = JsonConvert.SerializeObject(obj);
                    var c = new StringContent(s);
                    c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    var status_1 = await client.PostAsync(url, c, token);
                    if (status_1.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        pbStatus.Value += step;
                        continue;
                    }
                    var id = JsonConvert.DeserializeObject<int>(status_1.Content.ReadAsStringAsync().Result);
                    if (id == -1)
                    {
                        flag = false;
                        break;
                    }
                    var status_2 = await client.GetAsync($"{url}/{id}");
                    if (status_2.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        pbStatus.Value += step;
                        continue;
                    }
                    string res = await client.GetStringAsync($"{url}/{id}");
                    var img = JsonConvert.DeserializeObject<Image>(res);
                    switch (img.category)
                    {
                        case "neutral":
                            neutral.Add(img);
                            break;
                        case "happiness":
                            happiness.Add(img);
                            break;
                        case "surprise":
                            surprise.Add(img);
                            break;
                        case "sadness":
                            sadness.Add(img);
                            break;
                        case "anger":
                            anger.Add(img);
                            break;
                        case "disgust":
                            disgust.Add(img);
                            break;
                        case "fear":
                            fear.Add(img);
                            break;
                        case "contempt":
                            contempt.Add(img);
                            break;
                        default:
                            break;
                    }
                    pbStatus.Value += step;
                }
                catch (OperationCanceledException e_)
                {
                    continue;
                }
            }
            
            if (flag)
                pbStatus.Value = 1000;
            Files_selection.IsEnabled = true;
            del.IsEnabled = true;
        }

        private void command_stop(object sender, RoutedEventArgs e)
        {
            source.Cancel();
            MessageBox.Show("Cancelled");
        }

        private async void command_del(object sender, RoutedEventArgs e)
        {
            del.IsEnabled = false;
            tab_ctrl.IsEnabled = false;
            HttpClient client = new HttpClient();
            var status = await client.DeleteAsync(url);
            neutral.Clear();
            happiness.Clear();
            surprise.Clear();
            sadness.Clear();
            anger.Clear();
            disgust.Clear();
            fear.Clear();
            contempt.Clear();
            del.IsEnabled = true;
            tab_ctrl.IsEnabled = true;
        }
    }
}
