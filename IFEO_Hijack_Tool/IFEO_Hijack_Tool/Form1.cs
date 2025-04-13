using Microsoft.Win32;
using System.Windows.Forms;

namespace IFEO_Hijack_Tool
{
    public partial class IFEO : Form
    {
        public IFEO()
        {
            InitializeComponent();
        }

        private void btnHijack_Click(object sender, EventArgs e)
        {
            string targetApp = txtTargetApp.Text.Trim();
            string debuggerPath = txtDebugger.Text.Trim();

            if (string.IsNullOrEmpty(targetApp) || string.IsNullOrEmpty(debuggerPath))
            {
                MessageBox.Show("请输入目标程序名和劫持路径！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 创建或打开注册表项
                RegistryKey key = Registry.LocalMachine.CreateSubKey(
                    $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{targetApp}"
                );
                key.SetValue("Debugger", debuggerPath, RegistryValueKind.String);
                key.Close();

                MessageBox.Show($"劫持成功：{targetApp} → {debuggerPath}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("权限不足！请以管理员身份运行本程序。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"劫持失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            string targetApp = txtTargetApp.Text.Trim();

            if (string.IsNullOrEmpty(targetApp))
            {
                MessageBox.Show("请输入目标程序名！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 删除注册表项
                RegistryKey key = Registry.LocalMachine.OpenSubKey(
                    $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options",
                    writable: true
                );
                key.DeleteSubKeyTree(targetApp, throwOnMissingSubKey: false);
                key.Close();

                MessageBox.Show($"恢复成功：{targetApp}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("权限不足！请以管理员身份运行本程序。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"恢复失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void IFEO_Load(object sender, EventArgs e)
        {

        }
    }
}
