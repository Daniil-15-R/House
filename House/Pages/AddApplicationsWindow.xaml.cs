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

        public AddApplicationsWindow()
        {
            InitializeComponent();
            _context = new Entities();
            LoadData();
        }

        public AddApplicationsWindow(int applicationId) : this()
        {
            _isEditMode = true;
            Title = "Редактирование заявки";
            SaveButton.Content = "Обновить";

            var mainWindow = Application.Current.MainWindow as MainWindow;
            string userRole = "";
            if (mainWindow != null)
            {
                userRole = mainWindow.GetCurrentUserRole();
            }

            if (userRole != null &&
                (userRole.ToLower() == "собственник" || userRole.ToLower() == "клиент"))
            {
                StatusComboBox.IsEnabled = false;
            }

            LoadApplicationForEdit(applicationId);
        }

        private void LoadData()
        {
            try
            {
                var addresses = _context.List_of_housing_stock.ToList();
                AddressComboBox.ItemsSource = addresses;

                var allUsers = _context.Users.ToList();
                var workers = allUsers
                    .Where(u => u.Roles != null && u.Roles.Role == "Работник")
                    .OrderBy(u => u.Name)
                    .ToList();

                EmployerComboBox.ItemsSource = workers;

                var statuses = _context.Status.ToList();

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    int userId = mainWindow.GetCurrentUserId();
                    string userRole = mainWindow.GetCurrentUserRole();

                    if (userRole != null &&
                        (userRole.ToLower() == "собственник" || userRole.ToLower() == "клиент"))
                    {
                        var currentUser = _context.Users.Find(userId);
                        if (currentUser != null && !_isEditMode)
                        {
                            OwnerTextBox.Text = currentUser.Name;
                            OwnerTextBox.IsEnabled = false;
                        }
                    }

                    if (userRole != null &&
                    (userRole.ToLower() == "собственник" || userRole.ToLower() == "клиент"))
                    {
                        var newStatus = statuses.FirstOrDefault(s => s.Status1 == "Новая");
                        if (newStatus != null)
                        {
                            StatusComboBox.SelectedItem = newStatus;
                        }
                        else if (statuses.Count > 0)
                        {
                            StatusComboBox.SelectedIndex = 0;
                        }

                        StatusComboBox.IsEnabled = false;
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
                            }
                            else if (statuses.Count > 0)
                            {
                                StatusComboBox.SelectedIndex = 0;
                            }
                        }
                    }
                }
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
                _editingApplication = _context.Applications
                    .FirstOrDefault(a => a.Id == applicationId);

                if (_editingApplication == null)
                {
                    MessageBox.Show("Заявка не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                AddressComboBox.SelectedItem = _context.List_of_housing_stock
                    .FirstOrDefault(l => l.Id == _editingApplication.Address);

                OwnerTextBox.Text = _editingApplication.Owner;

                if (_editingApplication.Telephone != null)
                {
                    PhoneTextBox.Text = System.Text.Encoding.UTF8.GetString(_editingApplication.Telephone);
                }

                DescriptionTextBox.Text = _editingApplication.Descrition;

                if (_editingApplication.Employer > 0)
                {
                    var employer = _context.Users
                        .FirstOrDefault(u => u.Id == _editingApplication.Employer);
                    if (employer != null)
                    {
                        EmployerComboBox.SelectedItem = employer;
                    }
                }

                var status = _context.Status.FirstOrDefault(s => s.Id == _editingApplication.Status);
                if (status != null)
                {
                    StatusComboBox.SelectedItem = status;
                }
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
                }
                else
                {
                    application = new Applications();
                    _context.Applications.Add(application);
                }

                // Адрес
                if (AddressComboBox.SelectedItem is List_of_housing_stock selectedAddress)
                {
                    application.Address = selectedAddress.Id;
                }
                else
                {
                    ShowError("Адрес не выбран");
                    return;
                }

                application.Owner = OwnerTextBox.Text.Trim();
                application.Descrition = DescriptionTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(PhoneTextBox.Text))
                {
                    application.Telephone = System.Text.Encoding.UTF8.GetBytes(PhoneTextBox.Text.Trim());
                }
                else
                {
                    application.Telephone = null;
                }

                if (EmployerComboBox.SelectedItem is Users selectedEmployer)
                {
                    application.Employer = selectedEmployer.Id;
                }
                else
                {
                    if (!_isEditMode)
                    {
                        application.Employer = 0;
                    }
                }

                if (StatusComboBox.SelectedItem is Status selectedStatus)
                {
                    application.Status = selectedStatus.Id;
                }
                else if (!_isEditMode)
                {
                    var newStatus = _context.Status.FirstOrDefault(s => s.Status1 == "Новая");
                    if (newStatus != null)
                    {
                        application.Status = newStatus.Id;
                    }
                    else if (_context.Status.Any())
                    {
                        application.Status = _context.Status.First().Id;
                    }
                }

                _context.SaveChanges();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}\n{ex.InnerException?.Message}");
                Console.WriteLine($"Ошибка в SaveButton_Click: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private bool ValidateForm()
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
            ErrorTextBlock.Text = "";

            if (AddressComboBox.SelectedItem == null)
            {
                ShowError("Пожалуйста, выберите адрес");
                AddressComboBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(OwnerTextBox.Text))
            {
                ShowError("Пожалуйста, введите ФИО заявителя");
                OwnerTextBox.Focus();
                return false;
            }

            if (OwnerTextBox.Text.Length < 3)
            {
                ShowError("ФИО должно содержать минимум 3 символа");
                OwnerTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                ShowError("Пожалуйста, введите контактный телефон");
                PhoneTextBox.Focus();
                return false;
            }

            if (!IsValidPhoneNumber(PhoneTextBox.Text))
            {
                ShowError("Некорректный формат телефона. Используйте цифры, +, - и пробелы");
                PhoneTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                ShowError("Пожалуйста, введите описание проблемы");
                DescriptionTextBox.Focus();
                return false;
            }

            if (DescriptionTextBox.Text.Length < 10)
            {
                ShowError("Описание должно содержать минимум 10 символов");
                DescriptionTextBox.Focus();
                return false;
            }

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