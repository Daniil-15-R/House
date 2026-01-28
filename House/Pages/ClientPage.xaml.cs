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

namespace House.Pages
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        private int _userId;
        private string _userName; // Добавляем поле для имени

        public ClientPage(int userId)
        {
            InitializeComponent();
            _userId = userId;

            // Получаем имя пользователя из MainWindow
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                _userName = mainWindow.GetCurrentUserName();
            }
        }

        private void ApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            // Передаем и userId, и userName в ApplicationsPage
            MainFrame.Navigate(new ApplicationsPage(_userId, _userName));
        }
    }
}
