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
using System.Windows.Shapes;

namespace House.Pages
{
    public partial class AddEditUserWindow : Window
    {
        private Entities _db;
        private Users _user;

        public AddEditUserWindow(Users user, Entities db)
        {
            InitializeComponent();
            _db = db;
            _user = user;

            LoadRoles();
            LoadUserData();
        }

        private void LoadRoles()
        {
            cbRole.ItemsSource = _db.Roles.ToList();
            if (cbRole.Items.Count > 0)
                cbRole.SelectedIndex = 0;
        }

        private void LoadUserData()
        {
            if (_user != null)
            {
                tbName.Text = _user.Name;
                tbLogin.Text = _user.Login;
                cbRole.SelectedValue = _user.Role;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text) ||
                string.IsNullOrWhiteSpace(tbLogin.Text))
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_user == null)
                {
                    // Добавление нового пользователя
                    _user = new Users
                    {
                        Name = tbName.Text,
                        Login = tbLogin.Text,
                        Password = pbPassword.Password,
                        Role = (cbRole.SelectedItem as Roles)?.Id ?? 0
                    };
                    _db.Users.Add(_user);
                }
                else
                {
                    // Редактирование существующего
                    _user.Name = tbName.Text;
                    _user.Login = tbLogin.Text;
                    if (!string.IsNullOrWhiteSpace(pbPassword.Password))
                        _user.Password = pbPassword.Password;
                    _user.Role = (cbRole.SelectedItem as Roles)?.Id ?? 0;
                }

                _db.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
