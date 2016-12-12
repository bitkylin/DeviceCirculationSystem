using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using DeviceCirculationSystem.bean;
using DeviceCirculationSystem.bean.@enum;
using DeviceCirculationSystem.Util;

namespace DeviceCirculationSystem.view
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private FacilityChangeWindow _facilityChangeWindow;
        private readonly RepositoryPresenter _presenter = new RepositoryPresenter();
        private DeviceStatus _queryLogStatus;
        private QueryStatus _queryStatus;
        private readonly User _user;

        public MainWindow(User user)
        {
            InitializeComponent();
            _user = user;
            LabelUserName.Content = user.Name;
            RadioButtonQueryLab.IsChecked = true;
            RadioButtonQueryOutputLog.IsChecked = true;
        }

        /// <summary>
        ///     实验室设备的库存情况查询
        /// </summary>
        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            var deviceName = ComboBoxQueryDevice.Text.Trim();
            var facility = new Facility(DeviceStatus.Exist);
            facility.Name = deviceName;

            if (_queryStatus == QueryStatus.Lab)
                facility.OwnUser = "实验室";
            else if (_queryStatus == QueryStatus.Own)
                facility.OwnUser = _user.Name;
            else if (_queryStatus == QueryStatus.All)
                facility.OwnUser = "";
            else throw new Exception("查询状态设置错误");

            try
            {
                DataGridViewQuery.ItemsSource = RepositoryPresenter.QueryStorageLimitUser(facility).DefaultView;

                //被查询器件计数及计价
                var psum = 0; // 定义总数为0         显示 总库存量和总价值
                double msum = 0; //定义总价为0
                Debug.WriteLine(DataGridViewQuery.Items.Count);
                foreach (var itemRaw in DataGridViewQuery.Items)
                {
                    var item = itemRaw as DataRowView;
                    if (item == null)
                        continue;
                    psum = psum + Convert.ToInt32(item.Row[4].ToString());
                    msum = msum + Convert.ToDouble(item.Row[8].ToString());
                }
                LabelDevicesSum.Content = psum.ToString();
                LabelPriceSum.Content = msum.ToString(CultureInfo.CurrentCulture);
            }
            catch (NotFoundFacilityException)
            {
                DataGridViewQuery.ItemsSource = null;
                MessageBox.Show("未找到任何设备!", "提示");
            }
            if (DataGridViewQuery.Items.Count == 0)
            {
                BtnOutputStorage.IsEnabled = false;
                BtnInputStorage.IsEnabled = false;
            }
            else if (_queryStatus == QueryStatus.Lab)
            {
                BtnOutputStorage.IsEnabled = true;
                BtnInputStorage.IsEnabled = false;
            }
            else if (_queryStatus == QueryStatus.Own)
            {
                BtnOutputStorage.IsEnabled = false;
                BtnInputStorage.IsEnabled = true;
            }
        }

        /// <summary>
        ///     设备借出操作按钮
        /// </summary>
        private void BtnOutputStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var facilityChangeWindow = new FacilityChangeWindow(BuildDevice(DeviceStatus.Output));
                facilityChangeWindow.Show();
            }
            catch (NotFoundFacilityException)
            {
                MessageBox.Show("请选择表格中正确的条目!", "提示");
            }

            BtnInputStorage.IsEnabled = false;
            BtnOutputStorage.IsEnabled = false;
        }

        /// <summary>
        ///     设备归还操作按钮
        /// </summary>
        private void BtnInputStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _facilityChangeWindow = new FacilityChangeWindow(BuildDevice(DeviceStatus.Input));
                _facilityChangeWindow.Show();
            }
            catch (NotFoundFacilityException)
            {
                MessageBox.Show("请选择表格中正确的条目!", "提示");
            }
            BtnInputStorage.IsEnabled = false;
            BtnOutputStorage.IsEnabled = false;
        }

        /// <summary>
        ///     库存历史查询
        /// </summary>
        private void btnQueryLog_Click(object sender, RoutedEventArgs e)
        {
            var deviceName = ComboBoxQueryDeviceLog.Text.Trim();
            var facility = new Facility(_queryLogStatus)
            {
                Name = deviceName,
                OwnUser = _user.Name
            };
            try
            {
                DataGridViewLog.ItemsSource = _presenter.QueryDeviceInputOutputLog(facility).DefaultView;
            }
            catch (NotFoundFacilityException)
            {
                DataGridViewLog.ItemsSource = null;
                MessageBox.Show("没有找到历史记录！", "提示");
            }
        }

        /// <summary>
        ///     库存查询单选按钮，选择查询本人或实验室的库存器件
        /// </summary>
        private void radioButtonQuery_Checked(object sender, RoutedEventArgs e)
        {
            BtnInputStorage.IsEnabled = false;
            BtnOutputStorage.IsEnabled = false;
            if (RadioButtonQueryLab.IsChecked == true)
            {
                _queryStatus = QueryStatus.Lab;
            }
            else if (RadioButtonQueryOwn.IsChecked == true)
            {
                _queryStatus = QueryStatus.Own;
            }
            else if (RadioButtonQueryAll.IsChecked == true)
            {
                _queryStatus = QueryStatus.All;
            }
            else
                throw new Exception("单选按钮(库存查询选择)改变事件异常");
        }

        /// <summary>
        ///     借出归还历史记录查询单选按钮，选择查询本人的借出归还历史记录
        /// </summary>
        private void radioButtonQueryLog_Checked(object sender, RoutedEventArgs e)
        {
            if (RadioButtonQueryInputLog.IsChecked == true)
            {
                _queryLogStatus = DeviceStatus.Input;
            }
            else if (RadioButtonQueryOutputLog.IsChecked == true)
            {
                _queryLogStatus = DeviceStatus.Output;
            }
            else
                throw new Exception("单选按钮(器件历史查询选择)改变事件异常");
        }

        private Facility BuildDevice(DeviceStatus status)
        {
            var facility = new Facility(status);
            var item = DataGridViewQuery.SelectedItem as DataRowView;

            if (item == null)
                throw new NotFoundFacilityException();

            facility.Id = item.Row[0].ToString(); //器件编号
            facility.Name = item.Row[1].ToString(); //器件名称
            facility.ModelNum = item.Row[2].ToString(); //器件封装
            facility.Parameter = item.Row[3].ToString(); //器件规格
            facility.Num = int.Parse(item.Row[4].ToString()); //库存数量
            facility.OwnUser = item.Row[5].ToString(); //当前拥有者

            facility.DateTime = DateTime.Now; //当前操作的时间戳
            facility.Price = double.Parse(item.Row[7].ToString()); //器件单价
            facility.Note = item.Row[9].ToString();//备注

            if (facility.OwnUser.Equals("实验室"))
                facility.ToUser = _user.Name;
            else
                facility.ToUser = "实验室";
            return facility;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuItemChangePassword_Click(object sender, EventArgs e)
        {
            var changePasswordWindow = new ChangePasswordWindow(_user);
            changePasswordWindow.Show();
        }

        private void MenuItemAbout_Click(object sender, EventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }
    }
}