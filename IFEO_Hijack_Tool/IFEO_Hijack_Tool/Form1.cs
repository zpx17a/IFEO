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
                MessageBox.Show("������Ŀ��������ͽٳ�·����", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // �������ע�����
                RegistryKey key = Registry.LocalMachine.CreateSubKey(
                    $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{targetApp}"
                );
                key.SetValue("Debugger", debuggerPath, RegistryValueKind.String);
                key.Close();

                MessageBox.Show($"�ٳֳɹ���{targetApp} �� {debuggerPath}", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Ȩ�޲��㣡���Թ���Ա������б�����", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�ٳ�ʧ�ܣ�{ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            string targetApp = txtTargetApp.Text.Trim();

            if (string.IsNullOrEmpty(targetApp))
            {
                MessageBox.Show("������Ŀ���������", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // ɾ��ע�����
                RegistryKey key = Registry.LocalMachine.OpenSubKey(
                    $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options",
                    writable: true
                );
                key.DeleteSubKeyTree(targetApp, throwOnMissingSubKey: false);
                key.Close();

                MessageBox.Show($"�ָ��ɹ���{targetApp}", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Ȩ�޲��㣡���Թ���Ա������б�����", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�ָ�ʧ�ܣ�{ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void IFEO_Load(object sender, EventArgs e)
        {

        }
    }
}
