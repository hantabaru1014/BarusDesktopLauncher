using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;

namespace BarusDesktopLauncher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string[] cmds = Environment.GetCommandLineArgs();
            if (cmds.Length > 1)
            {
                _mySettingsJsonPath = cmds[1];
            }
            else
            {
                _mySettingsJsonPath = "mainSettings.json";
            }

            if (File.Exists(_mySettingsJsonPath))
            {
                appSettings = new MyAppSettings(_mySettingsJsonPath);
            }
            else
            {
                appSettings = new MyAppSettings();
            }

            this.Location = appSettings.windowPosition;
            this.Size = appSettings.windowSize;
            this.Opacity = appSettings.windowOpacity;
            if (appSettings.windowMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
            }
        }

        public string _mySettingsJsonPath;
        private MyAppSettings appSettings;
        public class MyAppSettings
        {

            public int configVersion;
            public string comment;
            public Point windowPosition;
            public Size windowSize;
            public bool windowMinimized;
            public float windowOpacity;
            public bool minimizeDeactived;
            public string[] folders;
            public string[] hideFileNames;
            public int iconSize;
            public SortOrder sortOrder;
            public View view;

            /// <summary>
            /// 初期化
            /// </summary>
            public MyAppSettings()
            {
                configVersion = 1;
                comment = "メモ用です。自由に編集して大丈夫です。";
                windowPosition = new Point(50, 50);
                windowSize = new Size(620, 500);
                windowMinimized = false;
                windowOpacity = 0.8f;
                minimizeDeactived = true;
                folders = new string[2];
                iconSize = 32;
                folders[0] = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                folders[1] = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                hideFileNames = new string[1];
                hideFileNames[0] = "desktop.ini";
                sortOrder = SortOrder.None;
                view = View.List;
            }
            /// <summary>
            /// jsonファイルから設定を読み込む
            /// </summary>
            /// <param name="path"></param>
            public MyAppSettings(string path)
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new DataContractJsonSerializer(typeof(MyAppSettings));
                    var deserialized = (MyAppSettings)serializer.ReadObject(fs);
                    configVersion = deserialized.configVersion;
                    comment = deserialized.comment;
                    windowPosition = deserialized.windowPosition;
                    windowSize = deserialized.windowSize;
                    windowMinimized = deserialized.windowMinimized;
                    windowOpacity = deserialized.windowOpacity;
                    minimizeDeactived = deserialized.minimizeDeactived;
                    folders = deserialized.folders;
                    hideFileNames = deserialized.hideFileNames;
                    iconSize = deserialized.iconSize;
                    sortOrder = deserialized.sortOrder;
                    view = deserialized.view;
                }
            }
            /// <summary>
            /// jsonファイルに出力する
            /// </summary>
            /// <param name="path"></param>
            public void WriteToFile(string path)
            {
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var serializer = new DataContractJsonSerializer(typeof(MyAppSettings));
                    serializer.WriteObject(fs, this);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {   
            InitializeListView();
        }

        /// <summary>
        /// 設定ファイルの再読み込みをする
        /// </summary>
        void ReloadSettings()
        {
            appSettings = new MyAppSettings(_mySettingsJsonPath);
            this.Location = appSettings.windowPosition;
            this.Size = appSettings.windowSize;
            this.Opacity = appSettings.windowOpacity;
            InitializeListView();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //SaveSettings();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            SaveSettings();
        }

        /// <summary>
        /// 設定をすべて保存する
        /// </summary>
        void SaveSettings()
        {
            appSettings.windowPosition = Location;
            appSettings.windowSize = Size;
            appSettings.sortOrder = listView1.Sorting;
            appSettings.view = listView1.View;

            appSettings.WriteToFile(_mySettingsJsonPath);
        }

        /// <summary>
        /// リストビューの初期化
        /// </summary>
        private void InitializeListView()
        {
            listView1.FullRowSelect = false;
            listView1.GridLines = true;
            listView1.Sorting = appSettings.sortOrder;
            listView1.Alignment = ListViewAlignment.Default;
            listView1.View = appSettings.view;
            
            RefreshListView();
        }

        /// <summary>
        /// リストビューを再描画
        /// </summary>
        private void RefreshListView()
        {
            listView1.Clear();
            ImageList iconList = new ImageList();
            iconList.ImageSize = new Size(appSettings.iconSize, appSettings.iconSize);
            foreach (var folder in appSettings.folders)
            {
                //フォルダー読み込み
                if (folder == "") { continue; }
                if (!Directory.Exists(folder)) { continue; }
                DirectoryInfo dinfo = new DirectoryInfo(folder);
                DirectoryInfo[] directories = dinfo.GetDirectories();
                foreach (var directory in directories)
                {
                    iconList.Images.Add(GetFileIcon(directory.FullName, true));

                    ListViewItem item = new ListViewItem();
                    item.Name = directory.FullName;
                    item.Text = Path.GetFileName(directory.FullName);
                    item.ImageIndex = iconList.Images.Count - 1;

                    listView1.Items.Add(item);
                }
            }
            foreach (var folder in appSettings.folders)
            {
                //ファイル読み込み
                if (folder == "") { continue; }
                if (!Directory.Exists(folder)) { continue; }
                DirectoryInfo dinfo = new DirectoryInfo(folder);
                FileInfo[] files = dinfo.GetFiles();
                foreach (var file in files)
                {
                    if (0 <= Array.IndexOf(appSettings.hideFileNames, Path.GetFileName(file.FullName))){
                        //隠すファイルなのでリストに含めない
                        continue;
                    }

                    iconList.Images.Add(GetFileIcon(file.FullName, true));

                    ListViewItem item = new ListViewItem();
                    item.Name = file.FullName;
                    item.Text = Path.GetFileName(file.FullName);
                    item.ImageIndex = iconList.Images.Count - 1;

                    listView1.Items.Add(item);
                }
            }

            listView1.SmallImageList = iconList;
            listView1.StateImageList = iconList;
            listView1.LargeImageList = iconList;
        }

        private void mouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OpenSelected();
            }
        }

        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                OpenSelected();
            }
        }

        /// <summary>
        /// 選択状態のフォルダ・ファイルを開く
        /// </summary>
        private void OpenSelected()
        {
            if (listView1.SelectedItems.Count < 1) { return; }
            try
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(listView1.SelectedItems[0].Name);
                p.Dispose();
            }catch (Exception)
            {

            }
            this.WindowState = FormWindowState.Minimized;
        }

        private void リストToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.List;
        }

        private void 大きなアイコンToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        private void タイルToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Tile;
        }

        bool _openingSettingPanel;
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSettings();
            _openingSettingPanel = true;
            SettingsPanel settingsPanel = new SettingsPanel(_mySettingsJsonPath);
            settingsPanel.StartPosition = FormStartPosition.CenterParent;
            settingsPanel.ShowDialog();
            _openingSettingPanel = false;
            ReloadSettings();
            settingsPanel.Dispose();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _openingSettingPanel = true;
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
            _openingSettingPanel = false;
            ab.Dispose();
        }

        //ウィンドウがデアクティブになったときに最小化する
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (appSettings.minimizeDeactived && !_openingSettingPanel)
            {
                this.WindowState = FormWindowState.Minimized;
            }
        }

        // SHGetFileInfo関数
        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        // SHGetFileInfo関数で使用するフラグ
        private const uint SHGFI_ICON = 0x100; // アイコン・リソースの取得
        private const uint SHGFI_LARGEICON = 0x0; // 大きいアイコン
        private const uint SHGFI_SMALLICON = 0x1; // 小さいアイコン

        // SHGetFileInfo関数で使用する構造体
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        /// <summary>
        /// ファイル・フォルダのアイコンを取得する
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        /// <param name="largeIcon">大きいサイズのアイコンを取得する</param>
        /// <returns></returns>
        Icon GetFileIcon(string path, bool largeIcon)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hSuccess;
            if (largeIcon)
            {
                hSuccess = SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);
            }
            else
            {
                hSuccess = SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);
            }
            if (hSuccess != IntPtr.Zero)
            {
                return Icon.FromHandle(shinfo.hIcon);
            }else
            {
                return null;
            }
        }

        private void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string currentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            if (!File.Exists(currentDirectory + "\\BarusUpdater.exe")) {
                System.Diagnostics.Process.Start("https://barusupdateservice.firebaseapp.com/products.html#bdl");
                return;
            }
            string jsonLink = "https://barusupdateservice.firebaseapp.com/BDL-AutoUpdate.json";
            if (File.Exists(currentDirectory + "\\.distribution"))
            {
                using (StreamReader reader = new StreamReader(currentDirectory + "\\.distribute", System.Text.Encoding.UTF8))
                {
                    jsonLink = reader.ReadToEnd();
                }
            }
            var updater_p = new System.Diagnostics.Process();
            updater_p.StartInfo.FileName = currentDirectory + "\\BarusUpdater.exe";
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string verString = "" + version.Major + "." + version.Minor;
            updater_p.StartInfo.Arguments = "/AppName \"BarusDesktopLauncher\" /URL \"" + jsonLink + "\" /CurrentVersion \"" + verString + "\" /Wait 1 /Mode onRunning /ShowNoUpdate /ExePath \"" + Application.ExecutablePath + "\"";
            updater_p.Start();
        }
    }
}
