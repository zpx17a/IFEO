using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace IFEO_Guard_UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string servicePath = @"..\..\..\..\IFEO_Guard_Service\IFEO_Guard_Service\bin\Debug\IFEO_Guard_Service.exe";
        private readonly string logPath = @"C:\Program Files\IFEOGuard\service.log";
        private FileSystemWatcher logWatcher;

        public MainWindow()
        {
            InitializeComponent();
            InitializeLogWatcher();
            LoadLogContent();
        }

        private void InitializeLogWatcher()
        {
            var logDir = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

            logWatcher = new FileSystemWatcher
            {
                Path = logDir,
                Filter = Path.GetFileName(logPath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            logWatcher.Changed += (s, e) => Dispatcher.Invoke(LoadLogContent);
            logWatcher.EnableRaisingEvents = true;
        }

        private void LoadLogContent()
        {
            try
            {
                if (File.Exists(logPath))
                {
                    using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fs))
                    {
                        txtLog.Text = reader.ReadToEnd();
                    }
                    txtLog.ScrollToEnd();
                }
            }
            catch (Exception ex)
            {
                AppendLog($"读取日志失败: {ex.Message}");
            }
        }

        private void AppendLog(string message)
        {
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
        }

        private void ExecuteCommand(string command)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                Process.Start(startInfo)?.WaitForExit();
            }
            catch (Exception ex)
            {
                AppendLog($"执行命令失败: {ex.Message}");
            }
        }

        // 按钮点击事件
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var fullPath = Path.GetFullPath(servicePath);
            ExecuteCommand($"sc create IFEOGuard binPath= \"{fullPath}\" start= auto && sc start IFEOGuard");
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("sc stop IFEOGuard && sc delete IFEOGuard");
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText(logPath, string.Empty);
                AppendLog("日志已清空");
            }
            catch (Exception ex)
            {
                AppendLog($"清空日志失败: {ex.Message}");
            }
        }

        private void BtnManageWhitelist_Click(object sender, RoutedEventArgs e)
        {
            var window = new WhitelistWindow();
            window.ShowDialog();
        }
    }
}
