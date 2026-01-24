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
        private string _currentUserRole;
        private List<List_of_housing_stock> _allAddresses;
        private bool _isOwner;

        public ApplicationsPage(int userId = 0)
        {
            InitializeComponent();
            _context = new Entities();

            // Получаем ID пользователя и его роль
            _currentUserId = userId;

            // Получаем роль из MainWindow
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                _currentUserRole = mainWindow.GetCurrentUserRole();
                _isOwner = !string.IsNullOrEmpty(_currentUserRole) &&
                           _currentUserRole.ToLower().Contains("собственник");
            }

            LoadAddresses();
            LoadApplications();
            ConfigureFilterAccessibility();
        }

        private void ConfigureFilterAccessibility()
        {
            if (_isOwner)
            {
                // Отключаем элементы фильтра для собственников
                AddressFilterComboBox.IsEnabled = false;
                ClearFilterButton.IsEnabled = false;

                // Устанавливаем серый цвет для визуального обозначения
                AddressFilterComboBox.Background = Brushes.LightGray;
                AddressFilterComboBox.Foreground = Brushes.DarkGray;

                // Добавляем подсказку
                AddressFilterComboBox.ToolTip = "Фильтр отключен для собственников. Показываются только ваши заявки.";
                ClearFilterButton.ToolTip = "Фильтр отключен для собственников";
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
                IQueryable<Applications> query = _context.Applications
                    .Include("List_of_housing_stock")
                    .Include("Status1")
                    .Include("Users")
                    .Include("Users1");

                // Применяем фильтр по адресу только если пользователь не собственник
                if (!_isOwner && AddressFilterComboBox.SelectedItem is List_of_housing_stock selectedAddress
                    && selectedAddress.Id != 0)
                {
                    query = query.Where(a => a.Address == selectedAddress.Id);
                }

                // Если пользователь - собственник или клиент, показываем только его заявки
                if (!string.IsNullOrEmpty(_currentUserRole) &&
                    (_currentUserRole.ToLower().Contains("собственник") ||
                     _currentUserRole.ToLower().Contains("клиент")))
                {
                    var currentUser = _context.Users.Find(_currentUserId);
                    if (currentUser != null)
                    {
                        query = query.Where(a => a.Owner == currentUser.Name);
                    }
                }

                var applications = query
                    .OrderByDescending(a => a.Id)
                    .ToList();

                ApplicationsListBox.ItemsSource = applications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заявок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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