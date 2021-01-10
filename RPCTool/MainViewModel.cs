using System;
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
            this.appConfig = AppConfig.Read();
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
        public ExecuteCommand GetInfoCommand { get; set; }

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

        private void GetInfo()
        {
            this.IsLoading = true;
            this.Go(() =>
            {
                try
                {
                    RPCClient client = new RPCClient();
                    client.Connect(new RRQMSocket.ConnectSetting() { TargetIP = this.IP, TargetPort = this.Port });
                    client.GetRPCReferencedAssemblies(this.DirPath);
                    Log("完成下载");
                }
                catch (Exception e)
                {
                    Log(e.Message);
                }
                finally
                {
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

        #endregion 绑定方法
    }
}