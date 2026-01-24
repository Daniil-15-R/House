using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using House;

namespace House.Pages
{
    public partial class DirectorPage : Page
    {
        private Entities db;

        public DirectorPage()
        {
            InitializeComponent();
            db = new Entities();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                cbRoleFilter.ItemsSource = db.Roles.ToList();
                cbRoleFilter.SelectedIndex = 0;

                cbStatusFilter.ItemsSource = db.Status.ToList();
                cbStatusFilter.SelectedIndex = 0;
                LoadPaymentReports();
                LoadDebts();
                LoadUsers();
                LoadJobs();
                LoadRates();
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (double.TryParse(tbProfit.Text, out double profit) &&
                    double.TryParse(tbCosts.Text, out double costs))
                {
                    if (costs == 0)
                    {
                        MessageBox.Show("Затраты не могут быть равны нулю", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    double roi = (profit / costs) * 100;

                    tbResultProfit.Text = $"{profit:N2} руб.";
                    tbResultCosts.Text = $"{costs:N2} руб.";
                    tbResultROI.Text = $"{roi:N2}%";
                    string comment;
                    if (roi > 50)
                        comment = "Отличная рентабельность! Компания эффективно использует ресурсы.";
                    else if (roi > 25)
                        comment = "Хорошая рентабельность. Есть потенциал для улучшения.";
                    else if (roi > 10)
                        comment = "Удовлетворительная рентабельность. Рекомендуется оптимизация.";
                    else
                        comment = "Низкая рентабельность. Требуется анализ и оптимизация затрат.";

                    tbROIComment.Text = comment;
                }
                else
                {
                    MessageBox.Show("Введите корректные числовые значения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLoadPaymentReports_Click(object sender, RoutedEventArgs e)
        {
            LoadPaymentReports();
        }

        private void LoadPaymentReports()
        {
            try
            {
                var reports = db.Payment_Report
                    .Include("List_of_housing_stock")
                    .Include("Owner1")
                    .ToList();

                if (!string.IsNullOrWhiteSpace(tbPeriodFilter.Text))
                {
                    reports = reports.Where(r => r.Period.Contains(tbPeriodFilter.Text)).ToList();
                }

                dgPaymentReports.ItemsSource = reports;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчетов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDebts()
        {
            try
            {
                var debts = db.List_of_debts
                    .Include("List_of_housing_stock")
                    .Include("Owner1")
                    .ToList();

                dgDebts.ItemsSource = debts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки долгов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLoadUsers_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var users = db.Users.Include("Roles").ToList();

                if (cbRoleFilter.SelectedItem is Roles selectedRole)
                {
                    users = users.Where(u => u.Roles.Id == selectedRole.Id).ToList();
                }

                dgUsers.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddEditUserWindow(null, db);
            if (addUserWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int userId)
            {
                var user = db.Users.Find(userId);
                if (user != null)
                {
                    var editWindow = new AddEditUserWindow(user, db);
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadUsers();
                    }
                }
            }
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int userId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var user = db.Users.Find(userId);
                        if (user != null)
                        {
                            db.Users.Remove(user);
                            db.SaveChanges();
                            LoadUsers();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnLoadJobs_Click(object sender, RoutedEventArgs e)
        {
            LoadJobs();
        }

        private void LoadJobs()
        {
            try
            {
                var jobs = db.ReportOfJob
                    .Include("StatusReportJob")
                    .Include("Applications")
                    .ToList();

                if (dpDateFilter.SelectedDate.HasValue)
                {
                    jobs = jobs.Where(j => j.Date.Date == dpDateFilter.SelectedDate.Value.Date).ToList();
                }

                if (cbStatusFilter.SelectedItem is Status selectedStatus)
                {
                    jobs = jobs.Where(j => j.Status == selectedStatus.Id).ToList();
                }

                dgJobs.ItemsSource = jobs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLoadRates_Click(object sender, RoutedEventArgs e)
        {
            LoadRates();
        }

        private void LoadRates()
        {
            try
            {
                var rates = db.Rate.ToList();
                dgRates.ItemsSource = rates;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тарифов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddRate_Click(object sender, RoutedEventArgs e)
        {
            var addRateWindow = new AddEditRateWindow(null, db);
            if (addRateWindow.ShowDialog() == true)
            {
                LoadRates();
            }
        }

        private void btnEditRate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int rateId)
            {
                var rate = db.Rate.Find(rateId);
                if (rate != null)
                {
                    var editWindow = new AddEditRateWindow(rate, db);
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadRates();
                    }
                }
            }
        }

        private void btnDeleteRate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int rateId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот тариф?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var rate = db.Rate.Find(rateId);
                        if (rate != null)
                        {
                            db.Rate.Remove(rate);
                            db.SaveChanges();
                            LoadRates();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CalculateStatistics()
        {
            try
            {
                double totalAccrued = db.Payment_Report.Sum(p => p.Accrued);
                tbTotalAccrued.Text = $"{totalAccrued:N2} руб.";
                double totalPaid = db.Payment_Report.Sum(p => p.Paid_for) ?? 0;
                tbTotalPaid.Text = $"{totalPaid:N2} руб.";
                var debts = db.List_of_debts.ToList();
                double totalDebts = debts.Sum(d => d.Water + (d.Electricpower ?? 0));
                tbTotalDebts.Text = $"{totalDebts:N2} руб.";
                tbProfit.Text = totalPaid.ToString("F2");
                tbCosts.Text = (totalPaid * 0.7).ToString("F2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета статистики: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
    }
}