using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using DeviceCirculationSystem.bean;
using DeviceCirculationSystem.bean.@enum;
using DeviceCirculationSystem.Util;

namespace DeviceCirculationSystem.view
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : IMainView
    {
        private readonly RepositoryPresenter _presenter = new RepositoryPresenter();
        private readonly User _user;
        private FacilityChangeWindow _facilityChangeWindow;
        private DeviceStatus _queryLogStatus;
        private QueryStatus _queryStatus;

        public MainWindow(User user)
        {
            InitializeComponent();
            _user = user;
            LabelUserName.Content = user.Name;
            RadioButtonQueryLab.IsChecked = true;
            RadioButtonQueryOutputLog.IsChecked = true;
            InitComboBox();
        }

        /// <summary>
        /// 初始化ComboBox
        /// </summary>
        private void InitComboBox()
        {
            var comboBoxDeviceList = RepositoryPresenter.GetDefaultDevices();

            ComboBoxQueryUser.LabelText = "所有用户";
            ComboBoxQueryDevice.LabelText = "所有类别";
            ComboBoxQueryUserLog.LabelText = "当前用户";
            ComboBoxQueryCategoryLog.LabelText = "所有类别";

            comboBoxDeviceList.ForEach(str =>
            {
                ComboBoxQueryDevice.addItem(str);
                ComboBoxQueryCategoryLog.addItem(str);
            });

            ComboBoxQueryUserLog.addItem("当前用户");
            ComboBoxQueryUser.addItem("全部");

            var userNameList = RepositoryPresenter.QueryUserNameAll();
            userNameList.ForEach(str =>
            {
                ComboBoxQueryUserLog.addItem(str);
                ComboBoxQueryUser.addItem(str);
            });
        }

        /// <summary>
        ///     实验室设备的库存情况查询
        /// </summary>
        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            var facilityCategory = ComboBoxQueryDevice.Text.Trim();
            var facilityUser = ComboBoxQueryUser.Text.Trim();
            var facility = new Facility(DeviceStatus.EXIST);
            facility.Category = facilityCategory;

            switch (_queryStatus)
            {
                case QueryStatus.LABORARY:
                    facility.OwnUser = "实验室";
                    break;
                case QueryStatus.ONESELF:
                    facility.OwnUser = _user.Name;
                    break;
                case QueryStatus.WHOLE:
                    facility.OwnUser = facilityUser;
                    break;
                default:
                    throw new Exception("查询状态设置错误");
            }

            try
            {
                DataGridViewQuery.ItemsSource = RepositoryPresenter.QueryStorageLimitUser(facility).DefaultView;

                //被查询器件计数及计价
                var psum = 0; // 定义总数为0         显示 总库存量和总价值
                double msum = 0; //定义总价为0
                LabelDevicesSum.Content = psum.ToString();
                LabelPriceSum.Content = msum.ToString(CultureInfo.CurrentCulture);

                foreach (var itemRaw in DataGridViewQuery.Items)
                {
                    var item = itemRaw as DataRowView;
                    if (item == null)
                        continue;
                    psum = psum + Convert.ToInt32(item.Row[5].ToString());
                    msum = msum + Convert.ToDouble(item.Row[9].ToString());
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
                SetButtonEnabled(false, false, true, false);
            }
            else if (_queryStatus == QueryStatus.LABORARY)
            {
                SetButtonEnabled(true, false, true, false);
            }
            else if (_queryStatus == QueryStatus.ONESELF)
            {
                SetButtonEnabled(false, true, true, true);
            }
        }

        /// <summary>
        ///     设备借出操作按钮
        /// </summary>
        private void BtnLoanFromStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _facilityChangeWindow = new FacilityChangeWindow(BuildDevice(DeviceStatus.LOAN), this);
                _facilityChangeWindow.Show();
            }
            catch (NotFoundFacilityException)
            {
                MessageBox.Show("请选择表格中正确的条目!", "提示");
            }
            SetButtonEnabled(false, false, false, false);
        }

        /// <summary>
        ///     设备归还操作按钮
        /// </summary>
        private void BtnReturnToStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _facilityChangeWindow = new FacilityChangeWindow(BuildDevice(DeviceStatus.RETURN), this);
                _facilityChangeWindow.Show();
            }
            catch (NotFoundFacilityException)
            {
                MessageBox.Show("请选择表格中正确的条目!", "提示");
            }
            SetButtonEnabled(false, false, false, false);
        }

        /// <summary>
        ///     设备入库操作按钮
        /// </summary>
        private void BtnInputStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _facilityChangeWindow = new FacilityChangeWindow(BuildDevice(DeviceStatus.INPUT), this);
                _facilityChangeWindow.Show();
            }
            catch (NotFoundFacilityException)
            {
                MessageBox.Show("请选择表格中正确的条目!", "提示");
            }
            SetButtonEnabled(false, false, false, false);
        }

        /// <summary>
        ///     设备出库操作按钮
        /// </summary>
        private void BtnOutputStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _facilityChangeWindow = new FacilityChangeWindow(BuildDevice(DeviceStatus.OUTPUT), this);
                _facilityChangeWindow.Show();
            }
            catch (NotFoundFacilityException)
            {
                MessageBox.Show("请选择表格中正确的条目!", "提示");
            }
            SetButtonEnabled(false, false, false, false);
        }

        /// <summary>
        ///     库存历史查询
        /// </summary>
        private void btnQueryLog_Click(object sender, RoutedEventArgs e)
        {
            var deviceCategory = ComboBoxQueryCategoryLog.Text.Trim();
            var deviceUser = ComboBoxQueryUserLog.Text.Trim();
            var facility = new Facility(_queryLogStatus);

            facility.Category = deviceCategory;
            if (deviceUser.Equals("") || deviceUser.Equals("当前用户"))
            {
                facility.OwnUser = _user.Name;
            }
            else
            {
                facility.OwnUser = deviceUser;
            }

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
            SetButtonEnabled(false, false, true, false);

            if (RadioButtonQueryLab.IsChecked == true)
                _queryStatus = QueryStatus.LABORARY;
            else if (RadioButtonQueryOwn.IsChecked == true)
                _queryStatus = QueryStatus.ONESELF;
            else if (RadioButtonQueryAll.IsChecked == true)
                _queryStatus = QueryStatus.WHOLE;
            else
                throw new Exception("单选按钮(库存查询选择)改变事件异常");
        }

        /// <summary>
        ///     借出归还历史记录查询单选按钮，选择查询本人的借出归还历史记录
        /// </summary>
        private void radioButtonQueryLog_Checked(object sender, RoutedEventArgs e)
        {
            if (RadioButtonQueryInputLog.IsChecked == true)
                _queryLogStatus = DeviceStatus.INPUT;
            else if (RadioButtonQueryOutputLog.IsChecked == true)
                _queryLogStatus = DeviceStatus.OUTPUT;
            else if (RadioButtonQueryReturnLog.IsChecked == true)
                _queryLogStatus = DeviceStatus.RETURN;
            else if (RadioButtonQueryLoanLog.IsChecked == true)
                _queryLogStatus = DeviceStatus.LOAN;
            else
                throw new Exception("单选按钮(器件历史查询选择)改变事件异常");
        }

        private Facility BuildDevice(DeviceStatus status)
        {
            var facility = new Facility(status);
            var item = DataGridViewQuery.SelectedItem as DataRowView;


            if (item != null)
            {
                facility.Id = item.Row[0].ToString(); //器件编号
                facility.Category = item.Row[1].ToString(); //器件类别
                facility.Name = item.Row[2].ToString(); //器件名称
                facility.ModelNum = item.Row[3].ToString(); //器件封装
                facility.Parameter = item.Row[4].ToString(); //器件规格
                facility.Num = int.Parse(item.Row[5].ToString()); //库存数量
                facility.OwnUser = item.Row[6].ToString(); //当前操作者

                facility.DateTime = DateTime.Now; //当前操作的时间戳
                facility.Price = double.Parse(item.Row[8].ToString()); //器件单价
                facility.Note = item.Row[10].ToString(); //备注
            }
            else
            {
                if (status != DeviceStatus.INPUT)
                    throw new NotFoundFacilityException();
            }
            switch (status)
            {
                case DeviceStatus.RETURN:
                case DeviceStatus.INPUT:
                    facility.OwnUser = _user.Name;
                    facility.ToUser = "实验室";
                    break;
                case DeviceStatus.LOAN:
                    facility.OwnUser = "实验室";
                    facility.ToUser = _user.Name;
                    break;
                case DeviceStatus.OUTPUT:
                    facility.OwnUser = _user.Name;
                    facility.ToUser = "已消耗掉";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            return facility;
        }

        private void SetButtonEnabled(bool isLoan, bool isReturn, bool isInput, bool isOutput)
        {
            BtnLoanFromStorage.IsEnabled = isLoan;
            BtnReturnToStorage.IsEnabled = isReturn;
            BtnInputStorage.IsEnabled = isInput;
            BtnOutputStorage.IsEnabled = isOutput;
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

        /// <summary>
        /// 在用户下拉菜单中输入后自动更改单选框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxQueryUser_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            RadioButtonQueryAll.IsChecked = true;
        }

        /// <summary>
        /// 在用户下拉菜单中点击后自动更改单选框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxQueryUser_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RadioButtonQueryAll.IsChecked = true;
        }

        /// <summary>
        /// 刷新库存信息
        /// </summary>
        public void refreshQueryStorage()
        {
            BtnQuery_Click(null, null);
        }
    }
}