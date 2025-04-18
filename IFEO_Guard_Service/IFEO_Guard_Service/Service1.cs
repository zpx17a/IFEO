using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace IFEO_Guard_Service
{
    public partial class Service1 : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegNotifyChangeKeyValue(
            IntPtr hKey,
            bool watchSubtree,
            RegChangeNotifyFilter notifyFilter,
            IntPtr hEvent,
            bool asynchronous
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ResetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        [Flags]
        public enum RegChangeNotifyFilter
        {
            Key = 0x0001,
            Attribute = 0x0002,
            Value = 0x0004,
            Security = 0x0008
        }

        private const uint INFINITE = 0xFFFFFFFF;
        private const string WhitelistPath = @"C:\Program Files\IFEOGuard\whitelist.txt";
        private const string LogPath = @"C:\Program Files\IFEOGuard\service.log";
        private const string IFEOPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
        private RegistryKey _ifeoKey;
        private IntPtr _registryEventHandle;
        private HashSet<string> _whitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _logLock = new object();
        private static readonly HashSet<string> _logWhitelistEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _logWhitelistLock = new object();

        public Service1()
        {
            InitializeComponent();
            ConfigureEventLog();
        }

        private void ConfigureEventLog()
        {
            if (!EventLog.SourceExists("IFEO Guard Service"))
            {
                EventLog.CreateEventSource("IFEO Guard Service", "Application");
            }
            EventLog.Source = "IFEO Guard Service";
            EventLog.Log = "Application";
        }

        protected override void OnStart(string[] args)
        {
            _logWhitelistEntries.Clear();
            FileLog("服务已成功启动。", EventLogEntryType.Information);
            EnsureDirectoryExists();
            EnsureWhitelistFileExists();
            EnsureLogFileExists();
            StartMonitoring();
        }

        protected override void OnStop()
        {
            FileLog("服务已成功停止。", EventLogEntryType.Information);
            ReleaseResources();
        }

        private void EnsureDirectoryExists()
        {
            try
            {
                var directory = Path.GetDirectoryName(WhitelistPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Log($"创建目录: {directory}", EventLogEntryType.Information);
                }
            }
            catch (Exception ex)
            {
                Log($"目录创建失败: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        private void EnsureWhitelistFileExists()
        {
            try
            {
                if (!File.Exists(WhitelistPath))
                {
                    File.WriteAllText(WhitelistPath, "# 白名单列表（每行一个路径）\n");
                    Log("白名单文件已创建", EventLogEntryType.Information);
                }
            }
            catch (Exception ex)
            {
                Log($"白名单初始化失败: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        private void EnsureLogFileExists()
        {
            try
            {
                if (!File.Exists(LogPath))
                {
                    File.Create(LogPath).Close();
                    Log("日志文件已创建", EventLogEntryType.Information);
                }
            }
            catch (Exception ex)
            {
                Log($"日志文件初始化失败: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        private void ReleaseResources()
        {
            if (_registryEventHandle != IntPtr.Zero)
            {
                CloseHandle(_registryEventHandle);
                _registryEventHandle = IntPtr.Zero;
            }
            _ifeoKey?.Close();
        }

        private void StartMonitoring()
        {
            try
            {
                _ifeoKey = Registry.LocalMachine.OpenSubKey(IFEOPath, true);
                Task.Run(() => MonitorRegistryChanges());
            }
            catch (Exception ex)
            {
                Log($"监控初始化失败: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void MonitorRegistryChanges()
        {
            _registryEventHandle = CreateEvent(IntPtr.Zero, true, false, null);

            while (true)
            {
                RegNotifyChangeKeyValue(
                    _ifeoKey.Handle.DangerousGetHandle(),
                    true,
                    RegChangeNotifyFilter.Key | RegChangeNotifyFilter.Value,
                    _registryEventHandle,
                    true
                );

                WaitForSingleObject(_registryEventHandle, INFINITE);
                ResetEvent(_registryEventHandle);
                ProcessRegistryChanges();
            }
        }

        private void ProcessRegistryChanges()
        {
            //Log("检测到注册表变更。", EventLogEntryType.Information);
            LoadWhitelist();

            foreach (string subKeyName in _ifeoKey.GetSubKeyNames())
            {
                using (RegistryKey subKey = _ifeoKey.OpenSubKey(subKeyName))
                {
                    ProcessSubKey(subKeyName, subKey);
                }
            }
        }

        private void ProcessSubKey(string subKeyName, RegistryKey subKey)
        {
            string debuggerValue = subKey.GetValue("Debugger")?.ToString();
            if (!string.IsNullOrEmpty(debuggerValue))
            {
                string debuggerPath = ExtractExecutablePath(debuggerValue);
                if (ShouldBlock(debuggerPath))
                {
                    BlockHijack(subKeyName, debuggerPath);
                }
            }
        }

        private string ExtractExecutablePath(string debuggerValue)
        {
            try
            {
                string trimmedValue = debuggerValue.Trim();
                if (trimmedValue.StartsWith("\"") && trimmedValue.IndexOf('"', 1) is int endQuoteIndex && endQuoteIndex > 0)
                {
                    return trimmedValue.Substring(1, endQuoteIndex - 1);
                }
                return trimmedValue.Split(new[] { ' ' }, 2)[0];
            }
            catch
            {
                return debuggerValue;
            }
        }

        private bool ShouldBlock(string debuggerPath)
        {
            try
            {
                string fullPath = Path.GetFullPath(debuggerPath);

                if (_whitelist.Contains(fullPath))
                {
                    lock (_logWhitelistLock)
                    {
                        if (!_logWhitelistEntries.Contains(fullPath))
                        {
                            Log($"放行白名单程序: {fullPath}", EventLogEntryType.Information);
                            _logWhitelistEntries.Add(fullPath);
                        }
                    }
                    return false;
                }
                return !VerifySignature(fullPath);
            }
            catch
            {
                return true;
            }
        }

        private bool VerifySignature(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Log($"文件不存在: {filePath}", EventLogEntryType.Warning);
                    return false;
                }

                using (var cert = new X509Certificate2(filePath))
                using (var chain = new X509Chain())
                {
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    return chain.Build(cert);
                }
            }
            catch (Exception ex)
            {
                Log($"签名验证失败: {ex.Message}", EventLogEntryType.Warning);
                return false;
            }
        }

        private void BlockHijack(string subKeyName, string debuggerPath)
        {
            try
            {
                _ifeoKey.DeleteSubKeyTree(subKeyName);
                Log($"拦截恶意劫持: {subKeyName} → {debuggerPath}", EventLogEntryType.Warning);
            }
            catch (Exception ex)
            {
                Log($"拦截失败 ({subKeyName}): {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void LoadWhitelist()
        {
            _whitelist.Clear();
            try
            {
                foreach (string line in File.ReadLines(WhitelistPath))
                {
                    string trimmedLine = line.Trim();
                    if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("#"))
                    {
                        _whitelist.Add(Path.GetFullPath(trimmedLine));
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"加载白名单失败: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void FileLog(string message, EventLogEntryType type)
        {
            lock (_logLock)
            {
                try
                {
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {type.ToString().ToUpper()}: {message}";
                    // string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] " + $"[{Environment.CurrentManagedThreadId:D4}] " + $"{type.ToString().ToUpper().PadRight(11)}: {message}";
                    File.AppendAllText(LogPath, logEntry + Environment.NewLine);
                }
                catch (Exception) // Exception ex
                {
                    // 此处完全脱离系统事件日志
                }
            }
        }

        private void Log(string message, EventLogEntryType type)
        {
            try
            {
                EventLog.WriteEntry(message, type);
                FileLog(message, type);
            }
            catch (Exception ex)
            {
                FileLog($"日志系统故障: {ex.Message}", EventLogEntryType.Error);
            }
        }
    }
}
