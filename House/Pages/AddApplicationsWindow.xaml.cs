using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace House.Pages
{
    public partial class AddApplicationsWindow : Window
    {
        private Entities _context;
        private Applications _editingApplication;
        private bool _isEditMode = false;
        private string _currentUserRole = "";

        public AddApplicationsWindow()
        {
            InitializeComponent();
            _context = new Entities();

            // Тестируем соединение с базой данных
            TestDatabaseConnection();

            // Получаем роль пользователя при создании окна
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                _currentUserRole = mainWindow.GetCurrentUserRole();
            }

            LoadData();
        }

        public AddApplicationsWindow(int applicationId) : this()
        {
            _isEditMode = true;
            Title = "Редактирование заявки";
            SaveButton.Content = "Обновить";

            if (_currentUserRole != null &&
                (_currentUserRole.ToLower() == "собственник" || _currentUserRole.ToLower() == "клиент"))
            {
                StatusComboBox.SelectedItem = null;
                EmployerComboBox.SelectedItem = null;
            }

            LoadApplicationForEdit(applicationId);
        }

        private void TestDatabaseConnection()
        {
            try
            {
                Console.WriteLine("=== Тестирование соединения с базой данных ===");

                using (var testContext = new Entities())
                {
                    // Проверяем, что можем получить данные
                    var appCount = testContext.Applications.Count();
                    Console.WriteLine($"Найдено заявок в базе: {appCount}");

                    var addressCount = testContext.List_of_housing_stock.Count();
                    Console.WriteLine($"Найдено адресов: {addressCount}");

                    var userCount = testContext.Users.Count();
                    Console.WriteLine($"Найдено пользователей: {userCount}");

                    var statusCount = testContext.Status.Count();
                    Console.WriteLine($"Найдено статусов: {statusCount}");

                    // Проверяем схему таблицы Applications
                    Console.WriteLine("\nСвойства модели Applications:");
                    var properties = typeof(Applications).GetProperties();
                    foreach (var prop in properties)
                    {
                        Console.WriteLine($"- {prop.Name}: {prop.PropertyType}");
                    }
                }

                Console.WriteLine("=== Тестирование завершено ===\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка тестирования БД: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
        }

        private void LoadData()
        {
            try
            {
                Console.WriteLine("=== Загрузка данных ===");

                var addresses = _context.List_of_housing_stock.ToList();
                AddressComboBox.ItemsSource = addresses;
                Console.WriteLine($"Загружено адресов: {addresses.Count}");

                var allUsers = _context.Users.ToList();
                var workers = allUsers
                    .Where(u => u.Roles != null && u.Roles.Role == "Работник")
                    .OrderBy(u => u.Name)
                    .ToList();

                EmployerComboBox.ItemsSource = workers;
                Console.WriteLine($"Загружено работников: {workers.Count}");

                var statuses = _context.Status.ToList();
                Console.WriteLine($"Загружено статусов: {statuses.Count}");

                if (_currentUserRole != null &&
                    (_currentUserRole.ToLower() == "собственник" || _currentUserRole.ToLower() == "клиент"))
                {
                    Console.WriteLine($"Текущий пользователь: {_currentUserRole} (собственник/клиент)");

                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        int userId = mainWindow.GetCurrentUserId();
                        var currentUser = _context.Users.Find(userId);

                        if (currentUser != null && !_isEditMode)
                        {
                            OwnerTextBox.Text = currentUser.Name;
                            OwnerTextBox.IsEnabled = false;
                            Console.WriteLine($"ФИО установлено автоматически: {currentUser.Name}");
                        }
                    }

                    EmployerComboBox.IsEnabled = false;

                    var newStatus = statuses.FirstOrDefault(s => s.Status1 == "Новая");
                    if (newStatus != null)
                    {
                        StatusComboBox.SelectedItem = newStatus;
                        Console.WriteLine($"Статус установлен автоматически: {newStatus.Status1}");
                    }
                    else if (statuses.Count > 0)
                    {
                        StatusComboBox.SelectedIndex = 0;
                    }
                    StatusComboBox.IsEnabled = false;

                    if (!_isEditMode)
                    {
                        StatusComboBox.ToolTip = "Для собственников статус устанавливается автоматически";
                        EmployerComboBox.ToolTip = "Исполнитель будет назначен администратором";
                    }
                }
                else
                {
                    StatusComboBox.ItemsSource = statuses;

                    if (!_isEditMode)
                    {
                        var newStatus = statuses.FirstOrDefault(s => s.Status1 == "Новая");
                        if (newStatus != null)
                        {
                            StatusComboBox.SelectedItem = newStatus;
                            Console.WriteLine($"Статус по умолчанию: {newStatus.Status1}");
                        }
                        else if (statuses.Count > 0)
                        {
                            StatusComboBox.SelectedIndex = 0;
                        }
                    }
                }

                if (!_isEditMode)
                {
                    DatePicker.SelectedDate = DateTime.Now;
                    Console.WriteLine($"Дата установлена на: {DateTime.Now:dd.MM.yyyy}");
                }

                Console.WriteLine("=== Загрузка данных завершена ===\n");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
                Console.WriteLine($"Ошибка в LoadData: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void LoadApplicationForEdit(int applicationId)
        {
            try
            {
                Console.WriteLine($"=== Загрузка заявки для редактирования ID: {applicationId} ===");

                _editingApplication = _context.Applications
                    .FirstOrDefault(a => a.Id == applicationId);

                if (_editingApplication == null)
                {
                    MessageBox.Show("Заявка не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                Console.WriteLine($"Найдена заявка: ID={_editingApplication.Id}, Владелец={_editingApplication.Owner}");

                AddressComboBox.SelectedItem = _context.List_of_housing_stock
                    .FirstOrDefault(l => l.Id == _editingApplication.Address);

                OwnerTextBox.Text = _editingApplication.Owner;

                if (!string.IsNullOrEmpty(_editingApplication.Telephone))
                {
                    PhoneTextBox.Text = _editingApplication.Telephone;
                }

                DescriptionTextBox.Text = _editingApplication.Descrition;

                if (_editingApplication.Date.HasValue)
                {
                    DatePicker.SelectedDate = _editingApplication.Date.Value;
                    Console.WriteLine($"Дата заявки: {_editingApplication.Date.Value:dd.MM.yyyy}");
                }
                else
                {
                    DatePicker.SelectedDate = DateTime.Now;
                    Console.WriteLine("Дата заявки не установлена, используем текущую");
                }

                if (_currentUserRole != null &&
                    (_currentUserRole.ToLower() == "собственник" || _currentUserRole.ToLower() == "клиент"))
                {
                    EmployerComboBox.IsEnabled = false;
                    Console.WriteLine("Режим собственника: EmployerComboBox заблокирован");
                }

                if (_editingApplication.Employer.HasValue && _editingApplication.Employer.Value > 0)
                {
                    var employer = _context.Users
                        .FirstOrDefault(u => u.Id == _editingApplication.Employer);
                    if (employer != null)
                    {
                        EmployerComboBox.SelectedItem = employer;
                        Console.WriteLine($"Исполнитель: {employer.Name} (ID: {employer.Id})");
                    }
                    else
                    {
                        Console.WriteLine($"Исполнитель не найден (ID: {_editingApplication.Employer})");
                    }
                }
                else
                {
                    Console.WriteLine("Исполнитель не назначен");
                }

                var status = _context.Status.FirstOrDefault(s => s.Id == _editingApplication.Status);
                if (status != null)
                {
                    StatusComboBox.SelectedItem = status;
                    Console.WriteLine($"Статус: {status.Status1} (ID: {status.Id})");

                    if (_currentUserRole != null &&
                        (_currentUserRole.ToLower() == "собственник" || _currentUserRole.ToLower() == "клиент"))
                    {
                        StatusComboBox.IsEnabled = false;
                    }
                }
                else
                {
                    Console.WriteLine($"Статус не найден (ID: {_editingApplication.Status})");
                }

                Console.WriteLine("=== Загрузка для редактирования завершена ===\n");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки заявки: {ex.Message}");
                Console.WriteLine($"Ошибка в LoadApplicationForEdit: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                Applications application;

                if (_isEditMode)
                {
                    application = _context.Applications.Find(_editingApplication.Id);
                    if (application == null)
                    {
                        ShowError("Заявка не найдена в базе данных");
                        return;
                    }
                    Console.WriteLine($"Редактирование существующей заявки ID: {application.Id}");
                }
                else
                {
                    application = new Applications();
                    _context.Applications.Add(application);
                    Console.WriteLine("Создание новой заявки");
                }

                // 1. Адрес (обязательное поле)
                if (AddressComboBox.SelectedItem is List_of_housing_stock selectedAddress)
                {
                    application.Address = selectedAddress.Id;
                    Console.WriteLine($"Адрес установлен: {selectedAddress.Address} (ID: {selectedAddress.Id})");
                }
                else
                {
                    ShowError("Адрес не выбран");
                    return;
                }

                // 2. ФИО владельца (обязательное поле)
                application.Owner = OwnerTextBox.Text.Trim();
                Console.WriteLine($"Владелец: {application.Owner}");

                // 3. Описание (обязательное поле)
                application.Descrition = DescriptionTextBox.Text.Trim();
                Console.WriteLine($"Описание: {application.Descrition}");

                // 4. Дата (nullable)
                if (DatePicker.SelectedDate.HasValue)
                {
                    application.Date = DatePicker.SelectedDate.Value;
                    Console.WriteLine($"Дата установлена: {application.Date:dd.MM.yyyy}");
                }
                else
                {
                    application.Date = DateTime.Now;
                    Console.WriteLine($"Дата установлена на текущую: {application.Date:dd.MM.yyyy}");
                }

                // 5. Телефон (nullable)
                if (!string.IsNullOrEmpty(PhoneTextBox.Text))
                {
                    application.Telephone = PhoneTextBox.Text.Trim();
                    Console.WriteLine($"Телефон установлен: {PhoneTextBox.Text.Trim()}");
                }
                else
                {
                    application.Telephone = null;
                    Console.WriteLine("Телефон не установлен");
                }

                // 6. Исполнитель (nullable) - ВАЖНОЕ ИЗМЕНЕНИЕ
                if (EmployerComboBox.SelectedItem is Users selectedEmployer)
                {
                    application.Employer = selectedEmployer.Id; // Nullable<int> может принимать int
                    Console.WriteLine($"Исполнитель установлен: {selectedEmployer.Name} (ID: {selectedEmployer.Id})");
                }
                else
                {
                    application.Employer = null; // Устанавливаем null, а не 0
                    Console.WriteLine("Исполнитель не выбран, устанавливаем null");
                }

                // 7. Статус (nullable) - ВАЖНОЕ ИЗМЕНЕНИЕ
                if (StatusComboBox.SelectedItem is Status selectedStatus)
                {
                    application.Status = selectedStatus.Id; // Nullable<int> может принимать int
                    Console.WriteLine($"Статус установлен: {selectedStatus.Status1} (ID: {selectedStatus.Id})");
                }
                else if (!_isEditMode)
                {
                    // Для новой заявки устанавливаем статус "Новая" или null
                    var newStatus = _context.Status.FirstOrDefault(s => s.Status1 == "Новая");
                    if (newStatus != null)
                    {
                        application.Status = newStatus.Id;
                        Console.WriteLine($"Статус по умолчанию: {newStatus.Status1} (ID: {newStatus.Id})");
                    }
                    else if (_context.Status.Any())
                    {
                        application.Status = _context.Status.First().Id;
                        Console.WriteLine($"Статус установлен на первый в списке (ID: {application.Status})");
                    }
                    else
                    {
                        application.Status = null;
                        Console.WriteLine("Статусы не найдены, устанавливаем null");
                    }
                }

                Console.WriteLine("\n=== Данные перед сохранением ===");
                Console.WriteLine($"Address: {application.Address}");
                Console.WriteLine($"Owner: {application.Owner}");
                Console.WriteLine($"Description: {application.Descrition}");
                Console.WriteLine($"Date: {application.Date}");
                Console.WriteLine($"Employer: {(application.Employer.HasValue ? application.Employer.Value.ToString() : "null")}");
                Console.WriteLine($"Status: {(application.Status.HasValue ? application.Status.Value.ToString() : "null")}");
                Console.WriteLine($"Telephone: {(application.Telephone != null ? "Установлен" : "null")}");

                try
                {
                    Console.WriteLine("\nПопытка сохранения в базу данных...");
                    _context.SaveChanges();
                    Console.WriteLine("Сохранение успешно!");

                    this.DialogResult = true;
                    this.Close();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    Console.WriteLine("\n=== Ошибки валидации ===");
                    string errorMessage = "Ошибки валидации:\n";
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Console.WriteLine($"Свойство: {validationError.PropertyName} Ошибка: {validationError.ErrorMessage}");
                            errorMessage += $"- {validationError.PropertyName}: {validationError.ErrorMessage}\n";
                        }
                    }
                    ShowError(errorMessage);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException updateEx)
                {
                    Console.WriteLine("\n=== Ошибка обновления базы данных ===");
                    Console.WriteLine($"Сообщение: {updateEx.Message}");

                    string errorMessage = "Ошибка при сохранении в базу данных:\n";

                    if (updateEx.InnerException != null)
                    {
                        Console.WriteLine($"Внутренняя ошибка: {updateEx.InnerException.Message}");
                        errorMessage += updateEx.InnerException.Message + "\n";

                        if (updateEx.InnerException.InnerException != null)
                        {
                            Console.WriteLine($"Внутренняя-внутренняя ошибка: {updateEx.InnerException.InnerException.Message}");
                            errorMessage += updateEx.InnerException.InnerException.Message;
                        }
                    }

                    ShowError(errorMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n=== Ошибка сохранения ===");
                    Console.WriteLine($"Сообщение: {ex.Message}");
                    Console.WriteLine($"Тип: {ex.GetType().Name}");

                    ShowError($"Ошибка сохранения: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n=== ФИНАЛЬНАЯ ОШИБКА ===");
                Console.WriteLine($"Сообщение: {ex.Message}");
                Console.WriteLine($"Тип: {ex.GetType().Name}");

                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private bool ValidateForm()
        {
            Console.WriteLine("\n=== Начало валидации формы ===");

            ErrorTextBlock.Visibility = Visibility.Collapsed;
            ErrorTextBlock.Text = "";

            if (AddressComboBox.SelectedItem == null)
            {
                ShowError("Пожалуйста, выберите адрес");
                AddressComboBox.Focus();
                Console.WriteLine("Валидация не пройдена: адрес не выбран");
                return false;
            }

            if (string.IsNullOrWhiteSpace(OwnerTextBox.Text))
            {
                ShowError("Пожалуйста, введите ФИО заявителя");
                OwnerTextBox.Focus();
                Console.WriteLine("Валидация не пройдена: ФИО не введено");
                return false;
            }

            if (OwnerTextBox.Text.Length < 3)
            {
                ShowError("ФИО должно содержать минимум 3 символа");
                OwnerTextBox.Focus();
                Console.WriteLine($"Валидация не пройдена: ФИО слишком короткое ({OwnerTextBox.Text.Length} символов)");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                ShowError("Пожалуйста, введите контактный телефон");
                PhoneTextBox.Focus();
                Console.WriteLine("Валидация не пройдена: телефон не введен");
                return false;
            }

            if (!IsValidPhoneNumber(PhoneTextBox.Text))
            {
                ShowError("Некорректный формат телефона. Используйте цифры, +, - и пробелы");
                PhoneTextBox.Focus();
                Console.WriteLine($"Валидация не пройдена: некорректный телефон {PhoneTextBox.Text}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                ShowError("Пожалуйста, введите описание проблемы");
                DescriptionTextBox.Focus();
                Console.WriteLine("Валидация не пройдена: описание не введено");
                return false;
            }

            if (DescriptionTextBox.Text.Length < 10)
            {
                ShowError("Описание должно содержать минимум 10 символов");
                DescriptionTextBox.Focus();
                Console.WriteLine($"Валидация не пройдена: описание слишком короткое ({DescriptionTextBox.Text.Length} символов)");
                return false;
            }

            if (!DatePicker.SelectedDate.HasValue)
            {
                ShowError("Пожалуйста, выберите дату заявки");
                DatePicker.Focus();
                Console.WriteLine("Валидация не пройдена: дата не выбрана");
                return false;
            }

            if (DatePicker.SelectedDate.Value > DateTime.Now)
            {
                ShowError("Дата заявки не может быть в будущем");
                DatePicker.Focus();
                Console.WriteLine($"Валидация не пройдена: дата в будущем {DatePicker.SelectedDate.Value:dd.MM.yyyy}");
                return false;
            }

            if (DatePicker.SelectedDate.Value < DateTime.Now.AddYears(-10))
            {
                ShowError("Дата заявки не может быть старше 10 лет");
                DatePicker.Focus();
                Console.WriteLine($"Валидация не пройдена: дата слишком старая {DatePicker.SelectedDate.Value:dd.MM.yyyy}");
                return false;
            }

            Console.WriteLine("Валидация пройдена успешно");
            return true;
        }

        private bool IsValidPhoneNumber(string phone)
        {
            string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

            if (cleanPhone.Length < 5 || cleanPhone.Length > 15)
                return false;

            return cleanPhone.All(char.IsDigit);
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '+' && c != '-' && c != ' ' && c != '(' && c != ')')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void PhoneTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                string phone = PhoneTextBox.Text.Trim();
                string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

                if (digitsOnly.Length == 11 && digitsOnly.StartsWith("7"))
                {
                    PhoneTextBox.Text = $"+7 ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, 2)}-{digitsOnly.Substring(9, 2)}";
                }
                else if (digitsOnly.Length == 10)
                {
                    PhoneTextBox.Text = $"+7 ({digitsOnly.Substring(0, 3)}) {digitsOnly.Substring(3, 3)}-{digitsOnly.Substring(6, 2)}-{digitsOnly.Substring(8, 2)}";
                }
            }
        }
    }
}