using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Collections.ObjectModel;

using ClassLibrary2;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class Image
    {

        private string descr;
        public string Descr
        {
            get { return this.descr; }
            set { this.descr = value; }
        }
        private string path;
        public string Path
        {
            get { return this.path; }
            set { this.path = value; }
        }
        public Image(string path, List<(float, string)> lst)
        {
            this.path = path;
            string str = "   Stats:\n";
            for (int i = 0; i < lst.Count; i++)
                str += "   " + lst[i].Item2 + ": " + lst[i].Item1.ToString() + "\n";
            this.descr = str;
        }
    }

    public partial class MainWindow : Window
    {
        Class1 cls;
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
            this.cls = new Class1();
            lv_neutral.ItemsSource = neutral;
            lv_happiness.ItemsSource = happiness;
            lv_surprise.ItemsSource = surprise;
            lv_sadness.ItemsSource = sadness;
            lv_anger.ItemsSource = anger;
            lv_disgust.ItemsSource = disgust;
            lv_fear.ItemsSource = fear;
            lv_contempt.ItemsSource = contempt;
        }

        private async void command_open(object sender, RoutedEventArgs e)
        {
            pbStatus.Value = 0;
            this.source = new CancellationTokenSource();
            this.token = source.Token;
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "(*.jpg, *.png)|*.jpg; *.png";
            var result = dialog.ShowDialog();
            int len = dialog.FileNames.Length;
            if (len == 0)
                return;
            int step = 500 / len;
            var tasks = new List<Task<(string, float)[]>>();
            var paths = dialog.FileNames;
            if (result == true)
            {
                for (int i = 0; i < len; i++)
                {
                    var path = dialog.FileNames[i];
                    var tmp = cls.process(path, token);
                    tasks.Add(tmp);
                    pbStatus.Value += step;
                }
            }
            bool flag = true;
            for (int i = 0; i < len; i++)
            {
                await tasks[i];
                (string, float)[] res = tasks[i].Result;
                var res_list = new List<(float, string)>();
                for (int j = 0; j < res.Length; j++)
                    res_list.Add((res[j].Item2, res[j].Item1));
                res_list.Sort();
                res_list.Reverse();
                if (res_list[0].Item1 == 0)
                {
                    flag = false;
                    break;
                }
                switch(res_list[0].Item2)
                {
                    case "neutral":
                        neutral.Add(new Image(paths[i], res_list));
                        break;
                    case "happiness":
                        happiness.Add(new Image(paths[i], res_list));
                        break;
                    case "surprise":
                        surprise.Add(new Image(paths[i], res_list));
                        break;
                    case "sadness":
                        sadness.Add(new Image(paths[i], res_list));
                        break;
                    case "anger":
                        anger.Add(new Image(paths[i], res_list));
                        break;
                    case "disgust":
                        disgust.Add(new Image(paths[i], res_list));
                        break;
                    case "fear":
                        fear.Add(new Image(paths[i], res_list));
                        break;
                    case "contempt":
                        contempt.Add(new Image(paths[i], res_list));
                        break;
                    default:
                        break;
                }
                pbStatus.Value += step;
            }
            if (flag)
                pbStatus.Value = 1000;
        }

        private void command_stop(object sender, RoutedEventArgs e)
        {
            source.Cancel();
            MessageBox.Show("Cancelled");
        }

    }
}
