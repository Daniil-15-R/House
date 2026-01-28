using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace House.Pages
{
    public partial class AuthPage : Page
    {
        private Entities _context;

        public AuthPage()
        {
            InitializeComponent();
            _context = new Entities();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorTextBlock.Text = "Заполните все поля";
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    var user = db.Users
                        .Include(u => u.Roles)
                        .FirstOrDefault(u => u.Login == login && u.Password == password);

                    if (user != null)
                    {
                        ErrorTextBlock.Visibility = Visibility.Collapsed;

                        string userRole = user.Roles?.Role ?? "Пользователь";

                        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.SetUserRole(userRole);
                            mainWindow.SetUserId(user.Id);
                            mainWindow.SetUserName(user.Name); // Добавляем сохранение имени
                        }

                        MessageBox.Show($"Добро пожаловать, {user.Name}!\nРоль: {userRole}",
                            "Успешный вход",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        switch (userRole.ToLower())
                        {
                            case "администратор":
                                this.NavigationService.Navigate(new AdminPage());
                                break;

                            case "руководитель":
                                this.NavigationService.Navigate(new DirectorPage());
                                break;

                            case "работник":
                            case "сотрудник":
                                this.NavigationService.Navigate(new EmploeePage());
                                break;

                            case "собственник":
                            case "клиент":
                                int userId = 0;
                                if (mainWindow != null)
                                {
                                    userId = mainWindow.GetCurrentUserId();
                                }
                                this.NavigationService.Navigate(new ClientPage(userId));
                                break;

                            default:
                                this.NavigationService.Content = null;
                                break;
                        }
                    }
                    else
                    {
                        ErrorTextBlock.Text = "Неверный логин или пароль";
                        ErrorTextBlock.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Ошибка: {ex.Message}";
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }
    }
}