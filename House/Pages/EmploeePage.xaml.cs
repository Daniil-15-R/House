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

        public EmploeePage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new Entities();

                // Загружаем сотрудников
                _employees = _context.Users
                    .Include(u => u.Roles)
                    .Where(u => u.Roles.Role == "Работники" || u.Role == 2)
                    .ToList();

                // Загружаем ВСЕ заявки с ВСЕМИ связанными данными
                _allApplications = _context.Applications
                    .Include(a => a.List_of_housing_stock)     // Адрес
                    .Include(a => a.Status1)                   // Статус
                    .Include(a => a.Users)                     // Создатель
                    .Include(a => a.Users1)                    // Исполнитель
                    .ToList();

                _allServices = _context.Service.ToList();
                lvEmployees.ItemsSource = _employees;

                if (_employees.Any())
                {
                    lvEmployees.SelectedIndex = 0;
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
            if (lvEmployees.SelectedItem is Users selectedEmployee)
            {
                tbSelectedEmployee.Text = $"{selectedEmployee.Name} (ID: {selectedEmployee.Id})";

                // ВАРИАНТ 1: Заявки, где сотрудник - исполнитель (Employer)
                var employeeApplications = _allApplications
                    .Where(a => a.Employer == selectedEmployee.Id)
                    .ToList();

                // ВАРИАНТ 2: Если нужно показывать заявки, созданные сотрудником
                // var employeeApplications = _allApplications
                //     .Where(a => a.Users != null && a.Users.Id == selectedEmployee.Id)
                //     .ToList();

                dgApplications.ItemsSource = employeeApplications;

                // Статистика
                tbTotalApplications.Text = employeeApplications.Count.ToString();

                var servicesCount = _allServices.Count(s => s.Employeer == selectedEmployee.Id);
                tbServicesCount.Text = servicesCount.ToString();

                // Дополнительная информация о статусах
                ShowApplicationsStatistics(employeeApplications);
            }
            else
            {
                tbSelectedEmployee.Text = "Не выбран";
                dgApplications.ItemsSource = null;
                tbTotalApplications.Text = "0";
                tbServicesCount.Text = "0";
            }
        }

        private void ShowApplicationsStatistics(List<Applications> applications)
        {
            if (applications == null || !applications.Any()) return;

            // Пример: можно добавить текстовый блок для отображения статистики по статусам
            var statusGroups = applications
                .GroupBy(a => a.Status1?.Status1 ?? "Неизвестно")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            // Можно вывести в отладочную консоль или добавить новый элемент UI
            Console.WriteLine($"Статистика по заявкам:");
            foreach (var group in statusGroups)
            {
                Console.WriteLine($"{group.Status}: {group.Count}");
            }
        }
    }
}