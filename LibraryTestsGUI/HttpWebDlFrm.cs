using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Zeptobyte.Net;

namespace LibraryTests
{
    public partial class HttpWebDlFrm : Form
    {
        public HttpWebDlFrm()
        {
            InitializeComponent();
        }

        HttpWebDownloader downy;
        decimal progRatio = 1m;
        bool downing = false;

        private void HttpWebDlFrm_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.HttpWebDlUrls != null)
                foreach (var url in Properties.Settings.Default.HttpWebDlUrls)
                    urlIn.Items.Add(url);
            if (Properties.Settings.Default.HttpWebDlDests != null)
                foreach (var dest in Properties.Settings.Default.HttpWebDlDests)
                    destIn.Items.Add(dest);
        }

        private void HttpWebDlFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.HttpWebDlUrls.Clear();
            foreach (string url in urlIn.Items)
                Properties.Settings.Default.HttpWebDlUrls.Add(url);
            foreach (string dest in destIn.Items)
                Properties.Settings.Default.HttpWebDlDests.Add(dest);
            Application.Exit();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                destIn.Text = saveFile.FileName;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            progress.Value = (int)(downy.Completed * progRatio);
            lblPercent.Text = (downy.Completed * 100m / downy.Size).ToString("F3") + "%";
            if (downy.Speed < 1024m)
                lblSpeed.Text = downy.Speed.ToString("F3") + " B/s";
            else if (downy.Speed >= 1024m && downy.Speed < 1048576m)
                lblSpeed.Text = (downy.Speed / 1048576m).ToString("F3") + " KiB/s";
            else if (downy.Speed >= 1048576m && downy.Speed < 1073741824m)
                lblSpeed.Text = (downy.Speed / 1073741824m).ToString("F3") + " MiB/s";
            else
                lblSpeed.Text = (downy.Speed / 1099511627776m).ToString("F3") + " GiB/s";
        }

        private void btnDl_Click(object sender, EventArgs e)
        {
            if (!downing)
            {
                downy = new HttpWebDownloader(urlIn.Text, destIn.Text);
                //downy.ProgressChanged += Downy_ProgressChanged;
                downy.Start();
                downing = true;
                btnDl.Text = "Stop";
                timer.Enabled = true;
                if (downy.Size > int.MaxValue)
                {
                    progress.Maximum = int.MaxValue;
                    progRatio = (decimal)int.MaxValue / downy.Size;
                }
                else
                {
                    progress.Maximum = (int)downy.Size;
                    progRatio = 1m;
                }
            }
            else
            {
                downy.Stop();
                downing = false;
                btnDl.Text = "Start";
                timer.Enabled = false;
            }
        }
    }
}
