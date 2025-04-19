using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace Notepad_Checker
{
    class Program
    {
        private static string originalDebuggerValue = null;
        private const string ifeoRegistryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\notepad.exe";

        static int Main(string[] args)
        {
            bool bypass = false;
            foreach (var arg in args)
            {
                if (arg.Equals("-bypass", StringComparison.OrdinalIgnoreCase))
                {
                    bypass = true;
                    break;
                }
            }
            if (bypass)
            {
                Console.WriteLine("已检测到绕过标记，直接启动目标程序。");
                return LaunchNotepadDirectly();
            }

            Console.WriteLine("Notepad_Checker.exe 启动，处理启动请求。");

            if (args.Length < 1)
            {
                Console.WriteLine("错误：缺少启动参数。程序退出。");
                return -1;
            }

            string originalExePath = args[0];
            string originalArguments = string.Empty;
            Console.WriteLine("接收到启动请求：");
            Console.WriteLine("目标程序: " + originalExePath);
            
            bool envOk = CheckEnvironment();
            if (!envOk)
            {
                Console.WriteLine("环境校验失败，拒绝启动原始程序。");
                return -1;
            }
            Console.WriteLine("环境校验通过，启动记事本。");
            SaveAndRemoveDebuggerRegistryEntry();

            string targetProgram = @"C:\Windows\System32\notepad.exe";
            string combinedArgs = (string.IsNullOrWhiteSpace(originalArguments)
                                        ? "-bypass"
                                        : originalArguments + " -bypass");

            Console.WriteLine("启动参数: " + combinedArgs);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = targetProgram,
                Arguments = combinedArgs,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            int exitCode = -1;
            try
            {
                Process proc = Process.Start(psi);
                Console.WriteLine("成功启动记事本进程。");
                proc.WaitForExit();
                exitCode = proc.ExitCode;
                Console.WriteLine("记事本进程已退出，退出码：" + exitCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("启动目标程序失败，错误信息: " + ex.Message);
            }
            finally
            {
                RestoreDebuggerRegistryEntry();
            }
            return exitCode;
        }
        private static bool CheckEnvironment()
        {
            int sec = DateTime.Now.Second;
            Console.WriteLine("当前秒数：" + sec);
            bool pass = (sec % 2 == 0);
            Console.WriteLine("环境校验：" + (pass ? "通过" : "失败"));
            return pass;
        }
        private static int LaunchNotepadDirectly()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\notepad.exe",
                    UseShellExecute = false,
                    CreateNoWindow = false
                };
                Process proc = Process.Start(psi);
                proc.WaitForExit();
                return proc.ExitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine("直接启动记事本失败，错误：" + ex.Message);
                return -1;
            }
        }
        private static void SaveAndRemoveDebuggerRegistryEntry()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(ifeoRegistryPath, true);
                if (key != null)
                {
                    originalDebuggerValue = key.GetValue("Debugger") as string;
                    if (originalDebuggerValue != null)
                    {
                        Console.WriteLine("保存 Debugger 注册表键值。");
                    }
                    key.DeleteValue("Debugger", false);
                    Console.WriteLine("已删除 Debugger 注册表键值。");
                    key.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("删除 Debugger 注册表键值时出错: " + ex.Message);
            }
        }
        private static void RestoreDebuggerRegistryEntry()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.CreateSubKey(ifeoRegistryPath, true);
                if (key != null)
                {
                    if (!string.IsNullOrEmpty(originalDebuggerValue))
                    {
                        key.SetValue("Debugger", originalDebuggerValue, RegistryValueKind.String);
                        Console.WriteLine("已恢复 Debugger 注册表键值。");
                    }
                    key.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("恢复 Debugger 注册表键值时出错: " + ex.Message);
            }
        }
    }
}
