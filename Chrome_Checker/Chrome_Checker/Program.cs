using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace ChromeChecker
{
    class Program
    {
        const string IFEO_PATH = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\chrome.exe";
        static string savedDebugger = null;

        static int Main(string[] args)
        {
            string chromePath = LocateChromeExecutable();
            Console.WriteLine("使用 Chrome 可执行路径: " + chromePath);

            var origArgs = args.Skip(1).ToArray();
            string[] safeParams = { "--disable-extensions", "--incognito", "--new-window" };
            var filteredArgsArray = origArgs.Where(p => safeParams.Contains(p, StringComparer.OrdinalIgnoreCase)).ToArray();
            string filteredArgs = string.Join(" ", filteredArgsArray);
            if (origArgs.Length == 0)
            {
                Console.WriteLine("无输入参数。");
            }
            else
            {
                if (filteredArgsArray.Length == origArgs.Length)
                {
                    Console.WriteLine("参数全部安全。");
                }
                else
                {
                    Console.WriteLine($"已过滤参数: {string.Join(" ", origArgs)}");
                }
            }

            SaveAndRemoveDebugger();

            int exitCode;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = chromePath,
                    Arguments = filteredArgs,
                    UseShellExecute = false
                };
                var proc = Process.Start(psi);
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }
            finally
            {
                RestoreDebugger();
            }
            return exitCode;
        }

        private static string LocateChromeExecutable()
        {
            const string appPaths = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";
            var key = Registry.LocalMachine.OpenSubKey(appPaths, false);
            var regPath = key?.GetValue(null) as string;
            if (!string.IsNullOrEmpty(regPath) && File.Exists(regPath))
                return regPath;

            var candidates = new[]
            {
                //@"C:\Program Files\Google\Chrome\Application\chrome.exe",
                //@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Google\Chrome\Application\chrome.exe")
            };
            return candidates.FirstOrDefault(File.Exists)
                ?? throw new FileNotFoundException("无法定位 chrome.exe，请检查安装。");
        }

        private static void SaveAndRemoveDebugger()
        {
            var key = Registry.LocalMachine.OpenSubKey(IFEO_PATH, true);
            if (key != null)
            {
                savedDebugger = key.GetValue("Debugger") as string;
                key.DeleteValue("Debugger", false);
                Console.WriteLine("已删除 Debugger 注册表键值。");
            }
        }

        private static void RestoreDebugger()
        {
            if (savedDebugger == null) return;
            var key = Registry.LocalMachine.CreateSubKey(IFEO_PATH);
            key.SetValue("Debugger", savedDebugger, RegistryValueKind.String);
            Console.WriteLine("已恢复 Debugger 注册表键值。");
        }
    }
}
