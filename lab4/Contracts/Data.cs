using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class Data
    {
        public byte[] img { get; set; }
        public string path { get; set; }

        public Data(byte[] img, string path)
        {
            this.img = img;
            this.path = path;
        }
    }
}
