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
using Microsoft.EntityFrameworkCore;

using ClassLibrary2;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class LibraryContext : DbContext
    {
        public DbSet<Image> Images { get; set; }
        public DbSet<Hash> Hashes { get; set; }
        public DbSet<ByteImage> ByteImages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite("Data Source=D:\\VStudio\\4_курс\\lab2\\WpfApp\\ImageDataBase.db");
    }

    public class Image
    {
        [Key]
        public int imageID { get; set; }
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
        public string category { set; get; }
        public Image(string path)
        {
            this.path = path;
        }
        public Image(string path, List<(float, string)> lst)
        {
            this.path = path;
            string str = "   Stats:\n";
            this.category = lst[0].Item2;
            for (int i = 0; i < lst.Count; i++)
                str += "   " + lst[i].Item2 + ": " + lst[i].Item1.ToString() + "\n";
            this.descr = str;
        }
    }

    public class Hash
    {
        [Key]
        [ForeignKey(nameof(Image))]
        public int imageID { get; set; }
        public int hash { get; set; }
    }

    public class ByteImage
    {
        [Key]
        [ForeignKey(nameof(Image))]
        public int imageID { get; set; }
        public byte[] byteImage { get; set; }
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
            LoadData();
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

        public void LoadData()
        {
            using(var db = new LibraryContext())
            {
                if (db.Images.Any())
                {
                    foreach (var item in db.Images)
                    {
                        switch (item.category)
                        {
                            case "neutral":
                                neutral.Add(item);
                                break;
                            case "happiness":
                                happiness.Add(item);
                                break;
                            case "surprise":
                                surprise.Add(item);
                                break;
                            case "sadness":
                                sadness.Add(item);
                                break;
                            case "anger":
                                anger.Add(item);
                                break;
                            case "disgust":
                                disgust.Add(item);
                                break;
                            case "fear":
                                fear.Add(item);
                                break;
                            case "contempt":
                                contempt.Add(item);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private async void command_open(object sender, RoutedEventArgs e)
        {
            Files_selection.IsEnabled = false;
            tab_ctrl.IsEnabled = false;
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
            int step = 500 / len;
            var tasks = new List<Task<(string, float)[]>>();
            var hashes_ = new List<int>();
            var byteimgs = new List<byte[]>();
            var paths = new List<string>();
            bool flag = true;
            using (var db = new LibraryContext())
            {
                if (result == true)
                {
                    for (int i = 0; i < len; i++)
                    {
                        var path = dialog.FileNames[i];
                        var img_ = File.ReadAllBytes(path);
                        var hash_ = img_.GetHashCode();
                        if (!db.Hashes.Any(x => x.hash == hash_))
                        {
                            if (!db.ByteImages.Any(x => x.byteImage.Equals(img_)))
                            {
                                hashes_.Add(hash_);
                                byteimgs.Add(img_);
                                paths.Add(path);
                                var tmp = cls.process(path, token);
                                tasks.Add(tmp);
                            }
                        }
                        pbStatus.Value += step;
                    }
                }
                for (int i = 0; i < tasks.Count; i++)
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
                    Image img = new Image(paths[i], res_list);
                    db.Images.Add(img);
                    db.SaveChanges();
                    int id = db.Images.OrderByDescending(x => x.imageID).First().imageID;
                    db.Hashes.Add(new Hash { imageID = id, hash = hashes_[i] });
                    db.ByteImages.Add(new ByteImage { imageID = id, byteImage = byteimgs[i] });
                    db.SaveChanges();
                    switch (res_list[0].Item2)
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
            }
            if (flag)
                pbStatus.Value = 1000;
            Files_selection.IsEnabled = true;
            tab_ctrl.IsEnabled = true;
            del.IsEnabled = true;
        }

        private void command_stop(object sender, RoutedEventArgs e)
        {
            source.Cancel();
            MessageBox.Show("Cancelled");
        }

        private void command_del(object sender, RoutedEventArgs e)
        {
            del.IsEnabled = false;
            tab_ctrl.IsEnabled = false;
            Image? img = null;
            switch (tab_ctrl.SelectedIndex)
            {
                case 1:
                    if (lv_neutral.SelectedItem != null)
                    {
                        img = lv_neutral.SelectedItem as Image;
                        neutral.RemoveAt(lv_neutral.SelectedIndex);
                    }
                    break;
                case 2:
                    if (lv_happiness.SelectedItem != null)
                    {
                        img = lv_happiness.SelectedItem as Image;
                        happiness.RemoveAt(lv_happiness.SelectedIndex);
                    }
                    break;
                case 3:
                    if (lv_surprise.SelectedItem != null)
                    {
                        img = lv_surprise.SelectedItem as Image;
                        surprise.RemoveAt(lv_surprise.SelectedIndex);
                    }
                    break;
                case 4:
                    if (lv_sadness.SelectedItem != null)
                    {
                        img = lv_sadness.SelectedItem as Image;
                        sadness.RemoveAt(lv_sadness.SelectedIndex);
                    }
                    break;
                case 5:
                    if (lv_anger.SelectedItem != null)
                    {
                        img = lv_anger.SelectedItem as Image;
                        anger.RemoveAt(lv_anger.SelectedIndex);
                    }
                    break;
                case 6:
                    if (lv_disgust.SelectedItem != null)
                    {
                        img = lv_disgust.SelectedItem as Image;
                        disgust.RemoveAt(lv_disgust.SelectedIndex);
                    }
                    break;
                case 7:
                    if (lv_fear.SelectedItem != null)
                    {
                        img = lv_fear.SelectedItem as Image;
                        fear.RemoveAt(lv_fear.SelectedIndex);
                    }
                    break;
                case 8:
                    if (lv_contempt.SelectedItem != null)
                    {
                        img = lv_contempt.SelectedItem as Image;
                        contempt.RemoveAt(lv_contempt.SelectedIndex);
                    }
                    break;
                default:
                    break;
            }
            if (img != null)
            {
                int id = img.imageID;
                using (var db = new LibraryContext())
                {
                    var query_1 = db.Images.Where(x => x.imageID == id);
                    db.Images.Remove(query_1.First());
                    var query_2 = db.Hashes.Where(x => x.imageID == id);
                    db.Hashes.Remove(query_2.First());
                    var query_3 = db.ByteImages.Where(x => x.imageID == id);
                    db.ByteImages.Remove(query_3.First());
                    db.SaveChanges();
                }
            }
            del.IsEnabled = true;
            tab_ctrl.IsEnabled = true;
        }
    }
}
