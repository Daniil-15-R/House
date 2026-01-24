using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace House.Pages
{
    public partial class ViewApplicationWindow : Window
    {
        private Entities _context;
        private int _applicationId;
        private Applications _application;

        public ViewApplicationWindow(int applicationId)
        {
            InitializeComponent();
            _context = new Entities();
            _applicationId = applicationId;
            LoadApplication();
        }

        private void LoadApplication()
        {
            try
            {
                _application = _context.Applications
                    .Include("List_of_housing_stock")
                    .Include("Status1")
                    .Include("Users")
                    .Include("Users1")
                    .FirstOrDefault(a => a.Id == _applicationId);

                if (_application == null)
                {
                    MessageBox.Show("Заявка не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                ApplicationIdText.Text = $"ID: {_application.Id}";
                Title = $"Заявка #{_application.Id} - {_application.Owner}";

                IdText.Text = _application.Id.ToString();
                OwnerText.Text = _application.Owner ?? "Не указано";
                DescriptionText.Text = _application.Descrition ?? "Нет описания";
                StatusText.Text = _application.Status1?.Status1 ?? "Неизвестно";

                if (_application.List_of_housing_stock != null)
                {
                    AddressText.Text = _application.List_of_housing_stock.Address ?? "Не указан";
                }
                else
                {
                    AddressText.Text = "Не указан";
                }

                CreatedDateText.Text = "Не указана";

                if (_application.Telephone != null && _application.Telephone.Length > 0)
                {
                    try
                    {
                        string phone = Encoding.UTF8.GetString(_application.Telephone);
                        PhoneText.Text = FormatPhoneNumber(phone);
                    }
                    catch
                    {
                        PhoneText.Text = "Ошибка чтения телефона";
                    }
                }
                else
                {
                    PhoneText.Text = "Не указан";
                }

                if (_application.Users != null)
                {
                    EmployerText.Text = _application.Users.Name ?? "Не назначен";
                }
                else
                {
                    EmployerText.Text = "Не назначен";
                }

                CompletionDateText.Text = "Не указана";

                CommentText.Text = "Нет комментария";

                UpdateStatusColor();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заявки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Ошибка: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length == 11 && digitsOnly.StartsWith("7"))
            {
                return $"+7 ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, 2)}-{digitsOnly.Substring(9, 2)}";
            }
            else if (digitsOnly.Length == 10)
            {
                return $"+7 ({digitsOnly.Substring(0, 3)}) {digitsOnly.Substring(3, 3)}-{digitsOnly.Substring(6, 2)}-{digitsOnly.Substring(8, 2)}";
            }
            else
            {
                return phone;
            }
        }

        private void UpdateStatusColor()
        {
            if (_application?.Status1 == null)
                return;

            string status = _application.Status1.Status1?.ToLower() ?? "";

            var statusBorder = StatusText.Parent as Border;
            if (statusBorder != null)
            {
                if (status.Contains("новая") || status.Contains("в работе"))
                {
                    statusBorder.Background = Brushes.LightYellow;
                    StatusText.Foreground = Brushes.DarkOrange;
                }
                else if (status.Contains("выполнена") || status.Contains("закрыта"))
                {
                    statusBorder.Background = Brushes.LightGreen;
                    StatusText.Foreground = Brushes.DarkGreen;
                }
                else if (status.Contains("отменена") || status.Contains("отклонена"))
                {
                    statusBorder.Background = Brushes.LightCoral;
                    StatusText.Foreground = Brushes.DarkRed;
                }
                else
                {
                    statusBorder.Background = Brushes.LightGray;
                    StatusText.Foreground = Brushes.Black;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AddApplicationsWindow(_applicationId);
            if (editWindow.ShowDialog() == true)
            {
                _context = new Entities();
                LoadApplication();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}