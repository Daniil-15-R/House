using House.Pages;
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

namespace House
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _currentUserRole = "";
        private int _currentUserId = 0;
        public MainWindow()
        {
            InitializeComponent();
            ShowAuthPage();
        }

        public void ShowAuthPage()
        {
            _currentUserRole = null;

            MainFrame.Navigate(new AuthPage());
        }

        private void Btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
            else
            {
                MessageBox.Show("Невозможно вернуться назад", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void SetUserRole(string role)
        {
            _currentUserRole = role;
        }
        public void SetUserId(int userId)
        {
            _currentUserId = userId;
        }
        public int GetCurrentUserId()
        {
            return _currentUserId;
        }

        public string GetCurrentUserRole()
        {
            return _currentUserRole;
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowAuthPage();
            }
        }
    }
}
