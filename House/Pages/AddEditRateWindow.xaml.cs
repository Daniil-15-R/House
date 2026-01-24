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
    public partial class AddEditRateWindow : Window
    {
        private Entities _db;
        private Rate _rate;

        public AddEditRateWindow(Rate rate, Entities db)
        {
            InitializeComponent();
            _db = db;
            _rate = rate;
            LoadRateData();
        }

        private void LoadRateData()
        {
            if (_rate != null)
            {
                tbTitle.Text = _rate.Title;
                tbPrice.Text = _rate.Price.ToString();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbTitle.Text) ||
                !double.TryParse(tbPrice.Text, out double price)) // Исправлено на double
            {
                MessageBox.Show("Введите корректные данные", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_rate == null)
                {
                    // Добавление нового тарифа
                    _rate = new Rate
                    {
                        Title = tbTitle.Text,
                        Price = price // Исправлено на double
                    };
                    _db.Rate.Add(_rate);
                }
                else
                {
                    // Редактирование существующего
                    _rate.Title = tbTitle.Text;
                    _rate.Price = price; // Исправлено на double
                }

                _db.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
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