using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarusDesktopLauncher
{
    public partial class SettingsPanel : Form
    {
        string _jsonPath;
        public SettingsPanel(string jsonPath)
        {
            InitializeComponent();
            appSettings = new Form1.MyAppSettings(jsonPath);
            _jsonPath = jsonPath;
        }

        Form1.MyAppSettings appSettings;

        private void SettingsPanel_Load(object sender, EventArgs e)
        {
            string folders = "";
            foreach (var folder in appSettings.folders)
            {
                folders += folder + ",";
            }
            textBox1.Text = folders;

            string hideFiles = "";
            foreach (var file in appSettings.hideFileNames)
            {
                hideFiles += file + ",";
            }
            textBox2.Text = hideFiles;

            numericUpDown1.Value = (decimal)appSettings.iconSize;

            checkBox1.Checked = appSettings.windowMinimized;

            switch (appSettings.sortOrder)
            {
                case SortOrder.None:
                    comboBox1.SelectedIndex = 0;
                    break;
                case SortOrder.Ascending:
                    comboBox1.SelectedIndex = 1;
                    break;
                case SortOrder.Descending:
                    comboBox1.SelectedIndex = 2;
                    break;
                default:
                    break;
            }

            checkBox2.Checked = appSettings.minimizeDeactived;

            trackBar1.Value = (int)(appSettings.windowOpacity * 100);
            label9.Text = trackBar1.Value + "%";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> folders = new List<string>();
            foreach (var str in textBox1.Text.Split(','))
            {
                if (str != "")
                {
                    folders.Add(str);
                }
            }
            List<string> hideFiles = new List<string>();
            foreach (var str in textBox2.Text.Split(','))
            {
                if (str != "")
                {
                    hideFiles.Add(str);
                }
            }
            appSettings.folders = folders.ToArray();
            appSettings.hideFileNames = hideFiles.ToArray();
            appSettings.iconSize = (int)numericUpDown1.Value;
            appSettings.windowMinimized = checkBox1.Checked;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    appSettings.sortOrder = SortOrder.None;
                    break;
                case 1:
                    appSettings.sortOrder = SortOrder.Ascending;
                    break;
                case 2:
                    appSettings.sortOrder = SortOrder.Descending;
                    break;
                default:
                    break;
            }
            appSettings.minimizeDeactived = checkBox2.Checked;
            appSettings.windowOpacity = (float)trackBar1.Value / 100;

            appSettings.WriteToFile(_jsonPath);
            Close();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label9.Text = trackBar1.Value + "%";
        }
    }
}
