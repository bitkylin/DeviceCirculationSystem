﻿using System;
using System.Data;
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
    public partial class MainWindow
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
        }

        /// <summary>
        ///     实验室设备的库存情况查询
        /// </summary>
        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            var facilityCategory = ComboBoxQueryDevice.Text.Trim();
            var facility = new Facility(DeviceStatus.Exist);
            facility.Category = facilityCategory;

            switch (_queryStatus)
            {
                case QueryStatus.Lab:
                    facility.OwnUser = "实验室";
                    break;
                case QueryStatus.Own:
                    facility.OwnUser = _user.Name;
                    break;
                case QueryStatus.All:
                    facility.OwnUser = "";
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
                setButtonEnabled(false, false, true, false);
            }
            else if (_queryStatus == QueryStatus.Lab)
            {
                setButtonEnabled(true, false, true, false);
            }
            else if (_queryStatus == QueryStatus.Own)
            {
                setButtonEnabled(false, true, true, true);
            }
        }

        /// <summary>
        ///     设备借出操作按钮
        /// </summary>
        private void BtnLoanFromStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _facilityChangeWindow = new FacilityChangeWindow(BuildDevice(DeviceStatus.Loan));
                _facilityChangeWindow.Show();
            }
            catch (NotFoundFacilityException)
            {
                MessageBox.Show("请选择表格中正确的条目!", "提示");
            }
            setButtonEnabled(false, false, false, false);
        }

        /// <summary>
        ///     设备归还操作按钮
        /// </summary>
        private void BtnReturnToStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _facilityChangeWindow = new FacilityChangeWindow(BuildDevice(DeviceStatus.Return));
                _facilityChangeWindow.Show();
            }
            catch (NotFoundFacilityException)
            {
                MessageBox.Show("请选择表格中正确的条目!", "提示");
            }
            setButtonEnabled(false, false, false, false);
        }

        /// <summary>
        ///     设备入库操作按钮
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
            setButtonEnabled(false, false, false, false);
        }

        /// <summary>
        ///     设备出库操作按钮
        /// </summary>
        private void BtnOutputStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _facilityChangeWindow = new FacilityChangeWindow(BuildDevice(DeviceStatus.Output));
                _facilityChangeWindow.Show();
            }
            catch (NotFoundFacilityException)
            {
                MessageBox.Show("请选择表格中正确的条目!", "提示");
            }
            setButtonEnabled(false, false, false, false);
        }

        /// <summary>
        ///     库存历史查询
        /// </summary>
        private void btnQueryLog_Click(object sender, RoutedEventArgs e)
        {
            var deviceCategory = ComboBoxQueryDeviceLog.Text.Trim();
            var facility = new Facility(_queryLogStatus)
            {
                Category = deviceCategory,
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
            setButtonEnabled(false, false, true, false);

            if (RadioButtonQueryLab.IsChecked == true)
                _queryStatus = QueryStatus.Lab;
            else if (RadioButtonQueryOwn.IsChecked == true)
                _queryStatus = QueryStatus.Own;
            else if (RadioButtonQueryAll.IsChecked == true)
                _queryStatus = QueryStatus.All;
            else
                throw new Exception("单选按钮(库存查询选择)改变事件异常");
        }

        /// <summary>
        ///     借出归还历史记录查询单选按钮，选择查询本人的借出归还历史记录
        /// </summary>
        private void radioButtonQueryLog_Checked(object sender, RoutedEventArgs e)
        {
            if (RadioButtonQueryInputLog.IsChecked == true)
                _queryLogStatus = DeviceStatus.Return;
            else if (RadioButtonQueryOutputLog.IsChecked == true)
                _queryLogStatus = DeviceStatus.Loan;
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
                if (status != DeviceStatus.Input)
                    throw new NotFoundFacilityException();
            }
            switch (status)
            {
                case DeviceStatus.Return:
                case DeviceStatus.Input:
                    facility.OwnUser = _user.Name;
                    facility.ToUser = "实验室";
                    break;
                case DeviceStatus.Loan:
                    facility.OwnUser = "实验室";
                    facility.ToUser = _user.Name;
                    break;
                case DeviceStatus.Output:
                    facility.OwnUser = _user.Name;
                    facility.ToUser = "已消耗掉";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            return facility;
        }

        private void setButtonEnabled(bool isLoan, bool isReturn, bool isInput, bool isOutput)
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
    }
}