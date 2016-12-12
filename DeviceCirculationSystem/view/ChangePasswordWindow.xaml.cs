using System.Windows;
using System.Windows.Input;
using DeviceCirculationSystem.bean;
using DeviceCirculationSystem.Util;

namespace DeviceCirculationSystem.view
{
    /// <summary>
    /// ChangePasswordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow(User user)
        {
            InitializeComponent();
            TextBoxUserName.Text = user.Name;
            TextBoxUserName.IsEnabled = false;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var userName = TextBoxUserName.Text.Trim();
            var passwordOrigin = PasswordBoxPasswordOrigin.Password.Trim();
            var passwordFuture = PasswordBoxPasswordFuture.Password.Trim();

            if (passwordOrigin == "" || passwordFuture == "")
            {
                MessageBox.Show("请输入密码！", "警告");
                return;
            }


            if (BitkyMySql.VerifyPermission_WorkManager(userName, passwordOrigin))
            {
                BitkyMySql.ChangePassword_WorkManager(userName, passwordFuture);
                MessageBox.Show("密码修改成功！", "提示");
                Close();
            }
            else
                MessageBox.Show("原密码输入有误，请重新输入！", "警告");
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}