using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Zeptobyte.Net
{
    public enum DownloadStatus
    {
        Stopped,
        Waiting,
        Connecting,
        Downloading,
        Stopping
    }

    [Serializable]
    public class HttpWebDownloader
    {
        public static long BufferSize
        {
            get; set;
        }

        string url, dest, user, pass, cookie;

        Timer updateTimer;

        public HttpWebDownloader(string src, string dst, bool replace = false)
        {
            if (!src.StartsWith("http"))
                src = "http://" + src;
            url = src;
            dest = dst;
            size = WebRequest.CreateHttp(src).GetResponse().ContentLength;
            updateTimer = new Timer(o =>
            {
                Update();
            }, null, Timeout.Infinite, 10000);
            Status = DownloadStatus.Stopped;
            if (File.Exists(dst))
            {
                var json = JObject.Parse(File.ReadAllText(dst));
                if ((string)json["url"] != src || size != long.Parse((string)json["size"]))
                    if (!replace)
                        throw new FileLoadException("Destination \"" + dst + "\" exists with different URL.");
                    else
                    {
                        File.Delete(dst);
                        if (Directory.Exists(TemporaryDir))
                            Directory.Delete(TemporaryDir);
                        goto newfile;
                    }
                else
                {
                    helpers = new List<HttpWebDlHelper>(json["connections"].Count());
                    completed = 0;
                    foreach (var con in json["connections"])
                    {
                        helpers.Add(new HttpWebDlHelper(this,
                            long.Parse((string)con["begin"]),
                            long.Parse((string)con["end"])));
                        completed += helpers.Last().Completed;
                    }
                }
            }
            else
            {
                goto newfile;
            }
            newfile:
            {
                if (!Directory.Exists(TemporaryDir))
                    Directory.CreateDirectory(TemporaryDir);
                completed = 0;
                helpers = new List<HttpWebDlHelper>();
                helpers.Add(new HttpWebDlHelper(this, 0L, size - 1));
                Update();
            }
        }

        void Update()
        {
            if (helpers.TrueForAll(h => h.Status == DownloadStatus.Stopped) &&
                helpers.TrueForAll(h => h.Completed == h.Size))
            {
                helpers.Sort((x, y) => (int)(x.Begin - y.Begin));
                File.Move(dest, Path.Combine(TemporaryDir, "~"));
                File.Move(helpers[0].WorkingFile, dest);
                helpers.RemoveAt(0);
                var fs = File.OpenWrite(dest);
                fs.Seek(0, SeekOrigin.End);
                int readBytes;
                byte[] buffer = new byte[BufferSize];
                while (helpers.Count != 0)
                {
                    var rfs = File.OpenRead(helpers[0].WorkingFile);
                    while ((readBytes = rfs.Read(buffer, 0, (int)BufferSize)) != 0)
                        fs.Write(buffer, 0, readBytes);
                }
                return;
            }

            Speed = 0;
            foreach (var h in helpers)
                Speed += h.Speed;

            var json = new JObject();
            json["url"] = url;
            json["size"] = size;
            var helps = new JArray();
            json["connections"] = helps;
            foreach (var h in helpers)
            {
                var con = new JObject();
                con["begin"] = h.Begin;
                con["end"] = h.End;
                helps.Add(con);
            }
            File.WriteAllText(dest, json.ToString());
        }

        public void Start()
        {
            Status = DownloadStatus.Connecting;
            foreach (var h in helpers)
            {
                h.Start();
            }
            Status = DownloadStatus.Downloading;
        }

        public void Stop()
        {
            Status = DownloadStatus.Stopping;
            foreach (var h in helpers)
            {
                h.Stop();
            }
            Status = DownloadStatus.Stopped;
        }

        public DownloadStatus Status
        {
            get; private set;
        }

        public decimal MaxSpeed
        {
            get; set;
        }

        public decimal Speed
        {
            get; private set;
        }

        public TimeSpan UpdateTime
        {
            get; set;
        }

        long size, completed;

        public long Size
        {
            get { return size; }
        }

        public long Completed
        {
            get { return completed; }
        }

        List<HttpWebDlHelper> helpers;

        public List<HttpWebDlHelper> Connections
        {
            get { return helpers; }
        }

        public string Url
        {
            get { return url; }
        }

        public string Destination
        {
            get { return dest; }
        }

        public string TemporaryDir
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(dest),
              ".~" + Path.GetFileName(dest));
            }
        }
    }
}
