using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PotPlayerApiLib;
using WinApiRemoteLib;
using DouyuBarrage;
using log4net;
using System.Runtime.InteropServices;

namespace barrageVOD
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string roomid;
        CrawlerThread craw;
        IEnumerable<Process> processes;
        Process process;
        static PotPlayerRemote remote;
        static ILog log = log4net.LogManager.GetLogger(typeof(CrawlerThread));
        static Dictionary<int, DateTime> userlastDanmu;
        static HashSet<string> adminsset;
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);
        #region 读Ini文件

        public static string ReadIniData(string Section, string Key, string NoText, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                StringBuilder temp = new StringBuilder(1024);
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);
                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        #endregion
        #region 写Ini文件

        public static bool WriteIniData(string Section, string Key, string Value, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                long OpStation = WritePrivateProfileString(Section, Key, Value, iniFilePath);
                if (OpStation == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {
            string filePath1 = Application.StartupPath + "\\config.ini";
            roomid = ReadIniData("settings", "roomid","783827", filePath1);
            textBox2.Text = roomid;
            processes = Process.GetProcesses().Where(t => t.ProcessName.ToLower().Contains("ai"));
            process = Process.GetProcesses().FirstOrDefault(t => t.ProcessName.StartsWith("PotPlayerMini64", StringComparison.CurrentCultureIgnoreCase));
            remote = new PotPlayerRemote(new ProcessWindow(process));
            userlastDanmu = new Dictionary<int, DateTime>();
            adminsset = new HashSet<string>();
            string[] admins = new string[10000];
            string filePath0 = Application.StartupPath + "\\admins.txt";
            try
            {
                if (File.Exists(filePath0))
                {
                    admins = File.ReadAllLines(filePath0, Encoding.GetEncoding("GB2312"));
                    //byte[] mybyte = Encoding.UTF8.GetBytes(textBox1.Text);
                    //textBox1.Text = Encoding.UTF8.GetString(mybyte);
                    foreach(string admin in admins)
                    {
                        adminsset.Add(admin);
                    }
                }
                else
                {
                    MessageBox.Show("文件不存在");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //文件路径
            string filePath = Application.StartupPath + "\\使用说明.txt";
            try
            {
                if (File.Exists(filePath))
                {
                    textBox1.Text = File.ReadAllText(filePath,Encoding.GetEncoding("GB2312"));
                    //byte[] mybyte = Encoding.UTF8.GetBytes(textBox1.Text);
                    //textBox1.Text = Encoding.UTF8.GetString(mybyte);
                }
                else
                {
                    MessageBox.Show("文件不存在");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender0, EventArgs e0)
        {
            string filePath1 = Application.StartupPath + "\\config.ini";
            WriteIniData("setings", "roomid", textBox2.Text, filePath1);
            int rid;
            if(!int.TryParse(roomid, out rid))
            {
                MessageBox.Show("房间号不正确");
                return;
            }
            DouyuConfig.room = rid;
            log4net.Config.XmlConfigurator.Configure();
            log.InfoFormat("弹幕捕获程序启动");
            AuthSocket auth = new AuthSocket();
            auth.OnReady += (sender, e) => {

                craw = new CrawlerThread(auth.DanmakuServers, auth.GID, auth.RID);
                craw.DisConnectHandler += Craw_DisConnectHandler;
                craw.ErrorHandler += Auth_ErrorHandler;
                craw.LogHandler += Auth_LogHandler;
                craw.OnDanmaku += Craw_OnDanmaku;
                craw.Start();

            };
            auth.Start();
            auth.ErrorHandler += Auth_ErrorHandler;
            auth.LogHandler += Auth_LogHandler;

            //Console.Read();
        }
        private static void Craw_OnDanmaku(object sender, DanmakuEventArgs e)
        {
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine(e.Danmaku.content+"  uid:"+e.Danmaku.uid+ "  snick:" + e.Danmaku.snick);
            if (e.Danmaku.content[0] == '#')
            {
                log.DebugFormat(e.Danmaku.content + "  uid:" + e.Danmaku.uid + "  snick:" + e.Danmaku.snick);
                if (!userlastDanmu.ContainsKey(e.Danmaku.uid))
                    userlastDanmu.Add(e.Danmaku.uid, DateTime.Now);
                else
                {
                    if (DateTime.Now - userlastDanmu[e.Danmaku.uid] < TimeSpan.FromMinutes(30) && !adminsset.Contains(e.Danmaku.snick))
                    {
                        Console.WriteLine("小于30分钟的点播弹幕或不是管理员："+e.Danmaku.content + "  uid:" + e.Danmaku.uid + "  snick:" + e.Danmaku.snick);
                        return;
                    }
                }
                //MessageBox.Show(e.Danmaku.content);
                if (remote == null)
                    Console.WriteLine("未找到播放器");
                int count = 0;
                if (e.Danmaku.content.Length > 3 && e.Danmaku.content.Substring(0, 3) == "#快进")
                {
                    int.TryParse(e.Danmaku.content.Substring(3), out count);
                    if (count <= 100 && count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            remote.Forward();
                        }
                        System.Threading.Thread.Sleep(500);
                    }
                }
                else if (e.Danmaku.content.Length > 3 && e.Danmaku.content.Length>3&&e.Danmaku.content.Substring(0, 3) == "#快退")
                {
                    int.TryParse(e.Danmaku.content.Substring(3), out count);
                    if (count <= 100 && count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            remote.Rewind();
                        }
                        System.Threading.Thread.Sleep(500);
                    }
                }
                else if (e.Danmaku.content.Length > 3 && e.Danmaku.content.Substring(0, 4) == "#上一集")
                {
                    remote.PreviousFile();
                    System.Threading.Thread.Sleep(500);
                }
                else if (e.Danmaku.content.Length > 3 && e.Danmaku.content.Substring(0, 4) == "#下一集")
                {
                    remote.NextFile();
                    System.Threading.Thread.Sleep(500);
                }
                else if (e.Danmaku.content.Length > 3 && e.Danmaku.content.Substring(0, 3) == "#上集")
                {
                    int.TryParse(e.Danmaku.content.Substring(3), out count);
                    if (count <= 10 && count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            remote.PreviousFile();
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                }
                else if (e.Danmaku.content.Length > 3 && e.Danmaku.content.Substring(0, 3) == "#下集")
                {
                    int.TryParse(e.Danmaku.content.Substring(3), out count);
                    if (count <= 10 && count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            remote.NextFile();
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                }
            }
            Console.WriteLine("-------------------------------------------------");

        }

        private static void Craw_DisConnectHandler(object sender, EventArgs e)
        {
            Console.WriteLine("断开链接");
        }

        private static void Auth_LogHandler(object sender, Events e)
        {
            Console.WriteLine("info:" + e.message);
        }

        private static void Auth_ErrorHandler(object sender, Events e)
        {
            Console.WriteLine("Error:" + e.message);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            craw.DisConnect();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string filePath = Application.StartupPath + "\\admins.txt";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "notepad";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.Arguments = filePath;
            Process.Start(psi);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string filePath1 = Application.StartupPath + "\\config.ini";
            WriteIniData("settings", "roomid", textBox2.Text, filePath1);
        }
    }
}
