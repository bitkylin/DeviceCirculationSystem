using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using DeviceCirculationSystem.bean;
using DeviceCirculationSystem.Util;

namespace DeviceCirculationSystem.view
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var userName = TextBoxUser.Text.Trim();
            var password = PasswordBox.Password.Trim();
            if (userName == "" || password == "")
                MessageBox.Show("请输入用户名和密码!", "警告");
            else
            {
                var havePermission = BitkyMySql.VerifyPermission_WorkManager(userName, password);
                if (havePermission)
                {
                    var window = new MainWindow(new User(userName));
                    window.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("用户名或密码错误，请重新输入！", "提示");
                }
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}