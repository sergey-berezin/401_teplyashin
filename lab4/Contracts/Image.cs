using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Contracts
{
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
        public Image() { }
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
}