using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zeptobyte.Net
{


    public class HttpWebDlHelper
    {
        HttpWebDownloader parent;

        long begin, end, completed;

        string workingFile;
        public string WorkingFile { get { return workingFile; } }

        public HttpWebDlHelper(HttpWebDownloader parent, long begin, long end)
        {
            this.parent = parent;
            this.begin = begin;
            this.end = end;

            Status = DownloadStatus.Stopped;

            workingFile = Path.Combine(parent.TemporaryDir,
                "." + begin + "~" + end + ".abdownpart");

            if (File.Exists(workingFile))
            {
                Completed = new FileInfo(workingFile).Length;
            }
            else
            {
                File.Create(workingFile).Dispose();
                Completed = 0;
            }
        }

        public void Start()
        {
            Thread t = new Thread(start);
            t.Start();
        }

        void start()
        {
            Status = DownloadStatus.Connecting;
            var destStream = new FileStream(workingFile, FileMode.Create,
            FileAccess.Write, FileShare.ReadWrite);
            var request = WebRequest.CreateHttp(parent.Url);
            request.AddRange(begin, end);
            var srcStream = request.GetResponse().GetResponseStream();
            int loadedBytes;
            byte[] downBuffer = new byte[HttpWebDownloader.BufferSize];
            Status = DownloadStatus.Downloading;
            while (Completed < Size)
            {
                var timer = new Stopwatch();
                timer.Start();
                loadedBytes = srcStream.Read(downBuffer, 0,
                    (int)HttpWebDownloader.BufferSize);
                timer.Stop();
                Speed = loadedBytes * 1000m / timer.ElapsedMilliseconds;
                Completed += loadedBytes;
                destStream.Write(downBuffer, 0, loadedBytes);
                destStream.Flush();
                if (Status == DownloadStatus.Stopping)
                    break;
                /*parent.WorkerProgressChanged(this, new ProgressEventArgs(completed,
                    completed += iByteSize, stop - start + 1, speed,
                    (speed = (decimal)(iByteSize /
                    (DateTime.Now - timer).TotalMilliseconds))));/**/
            }
            /*destStream.Close();
            destStream.Dispose();
            srcStream.Close();
            srcStream.Dispose();*/
            Status = DownloadStatus.Stopped;
        }

        public void Stop()
        {
            Status = DownloadStatus.Stopping;
        }

        public DownloadStatus Status
        {
            get; private set;
        }

        public long Size
        {
            get { return end - begin + 1; }
        }

        public long Completed
        {
            get { var d = completed; return d; }
            private set { completed = value; }
        }

        public long Begin
        {
            get { return begin; }
        }

        public long End
        {
            get { return end; }
        }

        public HttpWebDownloader Parent
        {
            get { return parent; }
        }

        decimal speed;

        public decimal Speed
        {
            get { var d = speed; return d; }
            private set { speed = value; }
        }
    }
}
