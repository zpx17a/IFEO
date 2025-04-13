using System;
using System.IO;
using System.Windows;

namespace IFEO_Guard_UI
{
    /// <summary>
    /// WhitelistWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WhitelistWindow : Window
    {
        private readonly string whitelistPath = @"C:\Program Files\IFEOGuard\whitelist.txt";

        public WhitelistWindow()
        {
            InitializeComponent();
            LoadWhitelist();
        }

        private void LoadWhitelist()
        {
            try
            {
                if (!File.Exists(whitelistPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(whitelistPath));
                    File.Create(whitelistPath).Close();
                }

                txtContent.Text = File.ReadAllText(whitelistPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载白名单失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText(whitelistPath, txtContent.Text);
                MessageBox.Show("保存成功", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
