using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Attobyte.Net
{
    public class HttpWebDownloader
    {
        string url, dest, user, pass, cookie;
        
        public decimal MaxSpeed
        {
            get; set;
        }

        public decimal Speed
        {
            get;
        }

        long size;
        public long Size
        {
            get { return size; }
        }


    }
}
