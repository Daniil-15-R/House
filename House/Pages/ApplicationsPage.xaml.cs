using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace House.Pages
{
    public partial class ApplicationsPage : Page
    {
        private Entities _context;
        private int _currentUserId;
        private string _currentUserName;
        private string _currentUserRole;
        private List<List_of_housing_stock> _allAddresses;
        private bool _isOwner;

        public ApplicationsPage(int userId = 0, string userName = null)
        {
            InitializeComponent();
            _context = new Entities();
            _currentUserId = userId;
            _currentUserName = userName;

            // Получаем роль из MainWindow
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                _currentUserRole = mainWindow.GetCurrentUserRole();
                _isOwner = !string.IsNullOrEmpty(_currentUserRole) &&
                           (_currentUserRole.ToLower().Contains("собственник") ||
                            _currentUserRole.ToLower().Contains("клиент"));

                // Если имя не передано, получаем его из MainWindow
                if (string.IsNullOrEmpty(_currentUserName))
                {
                    _currentUserName = mainWindow.GetCurrentUserName();
                }
            }

            Console.WriteLine($"=== Информация о пользователе ===");
            Console.WriteLine($"ID: {_currentUserId}");
            Console.WriteLine($"Имя: {_currentUserName}");
            Console.WriteLine($"Роль: {_currentUserRole}");
            Console.WriteLine($"Владелец/клиент: {_isOwner}");

            LoadAddresses();
            LoadApplications();
            ConfigureFilterAccessibility();
        }

        private void ConfigureFilterAccessibility()
        {
            if (_isOwner)
            {
                // Отключаем элементы фильтра для собственников/клиентов
                AddressFilterComboBox.IsEnabled = false;
                ClearFilterButton.IsEnabled = false;

                // Устанавливаем серый цвет для визуального обозначения
                AddressFilterComboBox.Background = Brushes.LightGray;
                AddressFilterComboBox.Foreground = Brushes.DarkGray;

                // Добавляем подсказку
                AddressFilterComboBox.ToolTip = "Фильтр отключен для собственников. Показываются только ваши заявки.";
                ClearFilterButton.ToolTip = "Фильтр отключен для собственников";

                Console.WriteLine("Фильтр отключен для владельца/клиента");
            }
        }

        private void LoadAddresses()
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке адресов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadApplications()
        {
            try
            {
                Console.WriteLine($"=== Начало загрузки заявок ===");

                IQueryable<Applications> query = _context.Applications
                    .Include("List_of_housing_stock")
                    .Include("Status1")
                    .Include("Users");

                // Применяем фильтр по адресу только если пользователь не собственник/клиент
                if (!_isOwner && AddressFilterComboBox.SelectedItem is List_of_housing_stock selectedAddress
                    && selectedAddress.Id != 0)
                {
                    query = query.Where(a => a.Address == selectedAddress.Id);
                    Console.WriteLine($"Применен фильтр по адресу ID: {selectedAddress.Id}");
                }

                // Если пользователь - собственник или клиент, показываем только его заявки
                if (_isOwner && !string.IsNullOrEmpty(_currentUserName))
                {
                    Console.WriteLine($"Фильтрация заявок для пользователя: '{_currentUserName}'");

                    // Для отладки: получаем все заявки
                    var allApps = query.ToList();
                    Console.WriteLine($"Всего заявок в базе (до фильтрации): {allApps.Count}");

                    // Выводим информацию о всех заявках для отладки
                    foreach (var app in allApps)
                    {
                        Console.WriteLine($"Заявка ID: {app.Id}, Владелец: '{app.Owner}'");
                    }

                    // Фильтруем по имени владельца (без учета регистра и лишних пробелов)
                    var cleanUserName = _currentUserName.Trim().ToLower();
                    query = query.Where(a => a.Owner != null &&
                        a.Owner.Trim().ToLower() == cleanUserName);

                    Console.WriteLine($"Имя для сравнения (очищенное): '{cleanUserName}'");
                }
                else if (!_isOwner)
                {
                    Console.WriteLine("Пользователь не собственник - показываем все заявки");
                }
                else
                {
                    Console.WriteLine("Имя пользователя не определено");
                }

                var applications = query
                    .OrderByDescending(a => a.Id)
                    .ToList();

                Console.WriteLine($"Загружено заявок после фильтрации: {applications.Count}");

                // Выводим информацию о загруженных заявках
                foreach (var app in applications)
                {
                    Console.WriteLine($"ID: {app.Id}, Адрес: {app.List_of_housing_stock?.Address}, " +
                        $"Владелец: '{app.Owner}', Статус: {app.Status1?.Status1}");
                }

                ApplicationsListBox.ItemsSource = applications;

                // Если заявок нет, показываем сообщение
                if (applications.Count == 0)
                {
                    if (_isOwner)
                    {
                        Console.WriteLine($"Для пользователя '{_currentUserName}' заявок не найдено");
                        // Можно добавить информационное сообщение для пользователя
                        // MessageBox.Show($"У вас еще нет заявок. Нажмите 'Добавить заявку' для создания первой заявки.", 
                        //    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Console.WriteLine("Заявок не найдено");
                    }
                }

                Console.WriteLine($"=== Загрузка заявок завершена ===\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заявок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Ошибка в LoadApplications: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void AddAplications_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddApplicationsWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadApplications();
                LoadAddresses();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadApplications();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isOwner)
            {
                AddressFilterComboBox.SelectedIndex = 0;
                LoadApplications();
            }
        }

        private void AddressFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isOwner)
            {
                LoadApplications();
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int applicationId)
            {
                var viewWindow = new ViewApplicationWindow(applicationId);
                viewWindow.ShowDialog();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int applicationId)
            {
                var editWindow = new AddApplicationsWindow(applicationId);
                if (editWindow.ShowDialog() == true)
                {
                    LoadApplications();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int applicationId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту заявку?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var application = _context.Applications.Find(applicationId);
                        if (application != null)
                        {
                            _context.Applications.Remove(application);
                            _context.SaveChanges();
                            LoadApplications();

                            MessageBox.Show("Заявка успешно удалена", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}