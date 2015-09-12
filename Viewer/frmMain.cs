using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;

namespace Viewer {
    public partial class frmMain : Form {

        private string[] files;
        private int index;

        public frmMain() {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e) {
            // Get image from cli args, if any
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 1) {
                if (File.Exists(args[args.Length - 1])) {
                    string filePath = args[args.Length - 1];
                    openImage(filePath);
                }
            }

            // Initialize hotkeys
            KeyPreview = true;
            KeyDown += new KeyEventHandler(frmMain_KeyDown);

            // Handle events needing a resize
            Resize += new EventHandler(frmMain_Resize);
            pbox.LoadCompleted += new AsyncCompletedEventHandler(pbox_LoadCompleted);
        }

        private void frmMain_Resize(object sender, EventArgs e) {
            resizePBox();
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e) {
            if (e.Control && e.KeyCode == Keys.O) { // Open
                e.SuppressKeyPress = true;
                showOpenDialog();
            }
            if (e.KeyCode == Keys.Left) {
                e.SuppressKeyPress = true;
                switchImage(-1);
            }
            if (e.KeyCode == Keys.Right) {
                e.SuppressKeyPress = true;
                switchImage(1);
            }
            if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0) {
                e.SuppressKeyPress = true;
                pbox.SizeMode = PictureBoxSizeMode.Zoom;
                resizePBox();
            }
            if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.OemPeriod) {
                e.SuppressKeyPress = true;
                pbox.SizeMode = PictureBoxSizeMode.AutoSize;
                resizePBox();
            }
            if(e.Alt && e.KeyCode == Keys.Enter) {
                e.SuppressKeyPress = true;
                showImageProperties();
            }
        }

        public void openImage(string path) {
            // Get other files in directory
            string[] files = Directory.GetFiles(Path.GetDirectoryName(path));

            // Filter to only images
            string[] images = files.Where(s => s.Length > 4 && (
                    s.Substring(s.Length - 4, 4) == ".jpg" ||
                    s.Substring(s.Length - 4, 4) == ".png" ||
                    s.Substring(s.Length - 4, 4) == ".gif" ||
                    s.Substring(s.Length - 4, 4) == ".bmp")
                ).ToArray();
            
            // Sort resulting array
            Array.Sort(images, StringComparer.CurrentCultureIgnoreCase);
            this.files = images;

            // Set this.index to position of selected image in directory
            index = Array.IndexOf(images, path);

            // Load image into pbox
            pbox.ImageLocation = path;
            lblLoading.Visible = true;
        }

        public void switchImage(int change) {
            index += change;
            if (index < 0) {
                index = files.Length - 1;
            } else if (index >= files.Length) {
                index = 0;
            }
            pbox.ImageLocation = files[index];
            lblLoading.Visible = true;
        }

        public void resizePBox() {
            if (pbox.SizeMode == PictureBoxSizeMode.AutoSize) {
                bool needsScroll = false;
                if (pbox.Width <= ClientSize.Width) {
                    pbox.Left = (ClientSize.Width - pbox.Width) / 2;
                } else {
                    pbox.Left = 0;
                    needsScroll = true;
                }
                if (pbox.Height <= ClientSize.Height - toolStrip1.Height) {
                    pbox.Top = (ClientSize.Height - pbox.Height) / 2 + toolStrip1.Height;
                } else {
                    pbox.Top = toolStrip1.Height;
                    needsScroll = true;
                }
                AutoScroll = needsScroll;
            } else if(pbox.SizeMode == PictureBoxSizeMode.Zoom) {
                pbox.Top = toolStrip1.Height;
                pbox.Left = 0;
                pbox.Width = ClientSize.Width;
                pbox.Height = ClientSize.Height - toolStrip1.Height;
                AutoScroll = false;
            }
        }

        public void showOpenDialog() {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Images|*.jpg;*.png;*.gif;*.bmp"; // |All files|*.*
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK) {
                openImage(dlg.FileName);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e) {
            showOpenDialog();
        }

        private void pbox_LoadCompleted(object sender, AsyncCompletedEventArgs e) {
            resizePBox();
            if (files.ElementAtOrDefault(index) != null) {
                Text = Path.GetFileName(files[index]) + " - Viewer";
            }
            lblLoading.Visible = false;
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e) {
            if (files.ElementAtOrDefault(index) != null) {
                Process.Start("explorer.exe", string.Format("/select,\"{0}\"", pbox.ImageLocation));
            }
        }

        private void btnPrev_Click(object sender, EventArgs e) {
            switchImage(-1);
        }

        private void btnNext_Click(object sender, EventArgs e) {
            switchImage(1);
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e) {
            showImageProperties();
        }

        public void showImageProperties() {
            if (files.ElementAtOrDefault(index) != null) {
                FileInfo info = new FileInfo(pbox.ImageLocation);
                string text = Path.GetFileName(pbox.ImageLocation);
                text += "\r\n";
                text += "\r\nCreated: " + info.CreationTime;
                text += "\r\nSize: " + (info.Length / 1024) + " KB";
                text += "\r\nDimensions: " + pbox.Image.Width + "x" + pbox.Image.Height;
                MessageBox.Show(text, Path.GetFileName(pbox.ImageLocation) + " Properties");
            }
        }
    }
}
