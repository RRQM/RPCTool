using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RRQMMVVM;
using RRQMSocket.RPC;

namespace RPCTool
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            this.logString = new StringBuilder();
            this.SelectDirCommand = new ExecuteCommand(SelectDir);
            this.GetInfoCommand = new ExecuteCommand(GetInfo);
            this.OutFileCommand = new ExecuteCommand(OutFile);
            this.OutCodeCommand = new ExecuteCommand(OutCode);
            this.appConfig = AppConfig.Read();

            this.OpenDirCommand = new ExecuteCommand(()=> 
            {
                if (Directory.Exists(this.DirPath))
                {
                    Process.Start(this.DirPath);
                }
                else
                {
                    Log("文件路径不存在");
                }         
            });
        }

        ~MainViewModel()
        {
            this.appConfig.Save();
        }

        #region 变量

        private AppConfig appConfig;

        #endregion 变量

        #region Command

        public ExecuteCommand SelectDirCommand { get; set; }
        public ExecuteCommand OpenDirCommand { get; set; }
        public ExecuteCommand GetInfoCommand { get; set; }
        public ExecuteCommand OutFileCommand { get; set; }
        public ExecuteCommand OutCodeCommand { get; set; }

        #endregion Command

        #region 属性

        public string DirPath
        {
            get { return appConfig.DirPath; }
            set { appConfig.DirPath = value; OnPropertyChanged(); }
        }

        public string IP
        {
            get { return appConfig.IP; }
            set { appConfig.IP = value; OnPropertyChanged(); }
        }

        public int Port
        {
            get { return appConfig.Port; }
            set { appConfig.Port = value; OnPropertyChanged(); }
        }

        private bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; OnPropertyChanged(); }
        }

        private StringBuilder logString;

        public string LogString
        {
            get { return logString.ToString(); }
        }

        #endregion 属性

        #region 绑定方法

        private void SelectDir()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择Txt所在文件夹";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    return;
                }
                this.DirPath = dialog.SelectedPath;
            }
        }
        RPCProxyInfo proxyInfo;
        RPCClient client;
        private void GetInfo()
        {
            this.IsLoading = true;
            this.Go(() =>
            {
                try
                {
                    client = new RPCClient();
                    client.Connect(new RRQMSocket.ConnectSetting() { TargetIP = this.IP, TargetPort = this.Port });
                    client.GetProxyInfo(out proxyInfo,5);
                    Log("获取到信息：");
                    Log($"RPC版本号：{proxyInfo.Version}");
                    Log($"RPC开放源代码：{(proxyInfo.Codes==null?false:true)}");
                }
                catch (Exception e)
                {
                    Log(e.Message);
                }
                finally
                {
                    client.Dispose();
                    client = null;
                    this.IsLoading = false;
                }
            });
        }

        private void Go(Action action)
        {
            Task.Run(action);
        }

        private void Log(string mes)
        {
            this.logString.AppendLine(mes);
            OnPropertyChanged("LogString");
        }
        private void OutFile()
        {
            if (this.proxyInfo==null)
            {
                return;
            }
            try
            {
                File.WriteAllBytes(Path.Combine(this.DirPath,this.proxyInfo.AssemblyName),this.proxyInfo.AssemblyData);
                Log("导出成功");
            }
            catch (Exception e)
            {
                Log(e.Message);
            }
        }
        
        private void OutCode()
        {
            if (this.proxyInfo == null)
            {
                return;
            }
            try
            {
                if (this.proxyInfo.Codes==null)
                {
                    Log("未开放源代码");
                    return;
                }

                File.Delete(Path.Combine(this.DirPath, "Code.txt"));
                foreach (var item in this.proxyInfo.Codes)
                {
                    File.AppendAllText(Path.Combine(this.DirPath, "Code.txt"), item+"\r\n\r\n");
                }
                
                Log("导出成功");
            }
            catch (Exception e)
            {
                Log(e.Message);
            }
        }
        #endregion 绑定方法
    }
}