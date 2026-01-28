using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;

namespace House.Pages
{
    public partial class EmploeePage : Page
    {
        private Entities _context;
        private List<Users> _employees;
        private List<Applications> _allApplications;
        private List<Service> _allServices;
        private string _currentUserRole;
        private int _currentUserId;

        public EmploeePage()
        {
            InitializeComponent();
            GetCurrentUserInfo();
            LoadData();
        }

        private void GetCurrentUserInfo()
        {
            try
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    _currentUserRole = mainWindow.GetCurrentUserRole();
                    _currentUserId = mainWindow.GetCurrentUserId();
                }
                else
                {
                    _currentUserRole = "Работник";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения информации о пользователе: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _currentUserRole = "Работник";
            }
        }

        private void LoadData()
        {
            try
            {
                _context = new Entities();

                _employees = _context.Users
                    .Include(u => u.Roles)
                    .Where(u => u.Roles.Role == "Работник" || u.Role == 4)
                    .ToList();

                lvEmployees.ItemsSource = _employees;

                _allApplications = _context.Applications
                    .Include(a => a.List_of_housing_stock)
                    .Include(a => a.Status1)
                    .Include(a => a.Users)
                    .ToList();

                _allServices = _context.Service.ToList();

                if (_currentUserRole.ToLower() == "администратор")
                {
                    lvEmployees.Visibility = Visibility.Visible;
                    lblSelectEmployee.Visibility = Visibility.Visible;
                    tbSelectedEmployee.Visibility = Visibility.Visible;

                    if (_employees.Any())
                    {
                        lvEmployees.SelectedIndex = 0;
                    }
                }
                else
                {
                    lvEmployees.Visibility = Visibility.Collapsed;
                    lblSelectEmployee.Visibility = Visibility.Collapsed;
                    tbSelectedEmployee.Visibility = Visibility.Collapsed;

                    var currentEmployee = _employees.FirstOrDefault(e => e.Id == _currentUserId);
                    if (currentEmployee != null)
                    {
                        ShowEmployeeApplications(currentEmployee);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lvEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentUserRole.ToLower() != "администратор")
                return;

            if (lvEmployees.SelectedItem is Users selectedEmployee)
            {
                ShowEmployeeApplications(selectedEmployee);
            }
            else
            {
                ClearEmployeeInfo();
            }
        }

        private void ShowEmployeeApplications(Users employee)
        {
            try
            {
                tbSelectedEmployee.Text = $"{employee.Name} (ID: {employee.Id})";

                List<Applications> employeeApplications;

                if (_currentUserRole.ToLower() == "администратор")
                {
                    // Администратор видит все заявки выбранного сотрудника
                    employeeApplications = _allApplications
                        .Where(a => a.Employer == employee.Id)
                        .ToList();
                }
                else
                {
                    // Обычный сотрудник видит только свои заявки
                    employeeApplications = _allApplications
                        .Where(a => a.Employer == _currentUserId)
                        .ToList();
                }

                dgApplications.ItemsSource = employeeApplications;

                // Статистика
                tbTotalApplications.Text = employeeApplications.Count.ToString();

                var servicesCount = _allServices.Count(s => s.Employeer == employee.Id);
                tbServicesCount.Text = servicesCount.ToString();

                ShowApplicationsStatistics(employeeApplications);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отображения заявок: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearEmployeeInfo()
        {
            tbSelectedEmployee.Text = "Не выбран";
            dgApplications.ItemsSource = null;
            tbTotalApplications.Text = "0";
            tbServicesCount.Text = "0";
        }

        private void ShowApplicationsStatistics(List<Applications> applications)
        {
            if (applications == null || !applications.Any()) return;

            var statusGroups = applications
                .GroupBy(a => a.Status1?.Status1 ?? "Неизвестно")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            Console.WriteLine($"Статистика по заявкам:");
            foreach (var group in statusGroups)
            {
                Console.WriteLine($"{group.Status}: {group.Count}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}