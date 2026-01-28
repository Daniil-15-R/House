using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace House.Pages
{
    public partial class ReadyApplicationsPage : Page
    {
        private Entities _context;
        private List<List_of_housing_stock> _allAddresses;
        private List<Users> _allEmployees;

        public ReadyApplicationsPage()
        {
            InitializeComponent();
            _context = new Entities();
            LoadFilters();
            LoadReadyApplications();
        }

        private void LoadFilters()
        {
            try
            {
                // Загружаем все адреса
                _allAddresses = _context.List_of_housing_stock
                    .OrderBy(a => a.Address)
                    .ToList();

                // Добавляем пустой элемент для сброса фильтра
                var addressesWithEmpty = new List<List_of_housing_stock>
                {
                    new List_of_housing_stock { Id = 0, Address = "Все адреса" }
                };
                addressesWithEmpty.AddRange(_allAddresses);

                AddressFilterComboBox.ItemsSource = addressesWithEmpty;
                AddressFilterComboBox.SelectedIndex = 0;

                // Загружаем всех сотрудников с ролью "Работник"
                var workerRole = _context.Roles
                    .FirstOrDefault(r => r.Role != null &&
                                         r.Role.ToLower().Contains("работник"));

                if (workerRole != null)
                {
                    _allEmployees = _context.Users
                        .Where(u => u.Role == workerRole.Id)
                        .OrderBy(u => u.Name)
                        .ToList();
                }
                else
                {
                    // Если роль "Работник" не найдена, загружаем всех пользователей
                    _allEmployees = _context.Users
                        .OrderBy(u => u.Name)
                        .ToList();
                }

                // Добавляем пустой элемент для сброса фильтра
                var employeesWithEmpty = new List<Users>
                {
                    new Users { Id = 0, Name = "Все исполнители" }
                };
                employeesWithEmpty.AddRange(_allEmployees);

                EmployeeFilterComboBox.ItemsSource = employeesWithEmpty;
                EmployeeFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке фильтров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadReadyApplications()
        {
            try
            {
                // Загружаем только заявки со статусом "Заявка закрыта"
                var closedStatus = _context.Status
                    .FirstOrDefault(s => s.Status1 == "Заявка закрыта");

                if (closedStatus != null)
                {
                    IQueryable<Applications> query = _context.Applications
                        .Include("List_of_housing_stock")
                        .Include("Status1")
                        .Include("Users") 
                        .Where(a => a.Status == closedStatus.Id);

                    // Применяем фильтр по адресу
                    if (AddressFilterComboBox.SelectedItem is List_of_housing_stock selectedAddress
                        && selectedAddress.Id != 0)
                    {
                        query = query.Where(a => a.Address == selectedAddress.Id);
                    }

                    // Применяем фильтр по исполнителю
                    if (EmployeeFilterComboBox.SelectedItem is Users selectedEmployee
                        && selectedEmployee.Id != 0)
                    {
                        // Фильтруем по полю Executor, если оно существует
                        // Или по связи Users1 (исполнитель)
                        query = query.Where(a => a.Users != null && a.Users.Id == selectedEmployee.Id);
                    }

                    var readyApplications = query
                        .OrderByDescending(a => a.Id) // Сначала новые
                        .ToList();

                    ApplicationsListBox.ItemsSource = readyApplications;

                    // Обновляем текст с количеством заявок
                    UpdateApplicationsCount(readyApplications.Count);
                }
                else
                {
                    MessageBox.Show("Статус 'Заявка закрыта' не найден в базе данных",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке завершённых заявок: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateApplicationsCount(int count)
        {
            string addressText = "Все адреса";
            if (AddressFilterComboBox.SelectedItem is List_of_housing_stock selectedAddress && selectedAddress.Id != 0)
            {
                addressText = selectedAddress.Address;
            }

            string employeeText = "Все исполнители";
            if (EmployeeFilterComboBox.SelectedItem is Users selectedEmployee && selectedEmployee.Id != 0)
            {
                employeeText = selectedEmployee.Name;
            }

            // Можно обновить заголовок или добавить текстовый блок для отображения информации
            // Например:
            // FilterInfoTextBlock.Text = $"Найдено заявок: {count} | Адрес: {addressText} | Исполнитель: {employeeText}";
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadReadyApplications();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            AddressFilterComboBox.SelectedIndex = 0;
            EmployeeFilterComboBox.SelectedIndex = 0;
            LoadReadyApplications();
        }

        private void AddressFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadReadyApplications();
        }

        private void EmployeeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadReadyApplications();
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int applicationId)
            {
                var viewWindow = new ViewApplicationWindow(applicationId);
                viewWindow.ShowDialog();
            }
        }

        private void ReopenButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int applicationId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите открыть эту заявку заново?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var application = _context.Applications.Find(applicationId);
                        if (application != null)
                        {
                            // Находим статус "Новая" или другой подходящий статус
                            var newStatus = _context.Status
                                .FirstOrDefault(s => s.Status1 == "Новая");

                            if (newStatus == null)
                            {
                                // Если статус "Новая" не найден, берем первый доступный
                                newStatus = _context.Status.FirstOrDefault();
                            }

                            if (newStatus != null)
                            {
                                application.Status = newStatus.Id;
                                _context.SaveChanges();

                                MessageBox.Show("Заявка успешно открыта заново",
                                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                                LoadReadyApplications(); // Обновляем список
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии заявки: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}