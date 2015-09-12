using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace Viewer {
    public partial class frmSettings : Form {
        public frmSettings() {
            InitializeComponent();
        }

        private void btnDefault_Click(object sender, EventArgs e) {
            string[] exts = { ".jpg", ".jpeg", ".png", ".gif", ".wmf", ".bmp" };
            foreach (var ext in exts) {
                registerExt(ext);
            }
        }

        private void registerExt(string ext) {
            RegistryKey key = Registry.ClassesRoot.CreateSubKey(ext);
            key.SetValue("", Application.ProductName);
            key.Close();

            key = Registry.ClassesRoot.CreateSubKey(ext + @"\Shell\Open\command");
            key.SetValue("", "\"" + Application.ExecutablePath + "\" \"%L\"");
            key.Close();
        }

        private void frmSettings_Load(object sender, EventArgs e) {
            // TODO: Check if app is default image viewer and disable btnDefault
        }

        private void btnClose_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
