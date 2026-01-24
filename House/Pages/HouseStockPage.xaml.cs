using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.Entity;

namespace House.Pages
{
    public partial class HouseStockPage : Page
    {
        private Entities _context;

        public HouseStockPage()
        {
            InitializeComponent();
            _context = new Entities();
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadHousingData();
        }

        private void LoadHousingData()
        {
            try
            {
                var housingList = _context.List_of_housing_stock
                    .Include(h => h.Applications)
                    .OrderBy(h => h.Address)
                    .ToList();

                if (housingList.Any())
                {
                    HousingListBox.ItemsSource = housingList;
                    NoDataTextBlock.Visibility = Visibility.Collapsed;
                    HousingListBox.Visibility = Visibility.Visible;
                    Console.WriteLine($"Загружено {housingList.Count} записей:");
                    foreach (var item in housingList)
                    {
                        Console.WriteLine($"- {item.Address}, Заявок: {item.Applications?.Count ?? 0}");
                    }
                }
                else
                {
                    HousingListBox.Visibility = Visibility.Collapsed;
                    NoDataTextBlock.Visibility = Visibility.Visible;
                    Console.WriteLine("Нет данных в таблице List_of_housing_stock");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\n\nДетали: {ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                TryLoadSimpleData();
            }
        }

        private void TryLoadSimpleData()
        {
            try
            {
                var simpleList = _context.List_of_housing_stock
                    .OrderBy(h => h.Address)
                    .ToList();

                if (simpleList.Any())
                {
                    HousingListBox.ItemsSource = simpleList;
                    NoDataTextBlock.Visibility = Visibility.Collapsed;
                    HousingListBox.Visibility = Visibility.Visible;
                    Console.WriteLine($"Успешно загружено {simpleList.Count} записей (без загрузки Applications)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка даже при простой загрузке: {ex.Message}");
                TestDatabaseConnection();
            }
        }

        private void TestDatabaseConnection()
        {
            try
            {
                var count = _context.List_of_housing_stock.Count();
                MessageBox.Show($"В базе данных найдено {count} записей в таблице List_of_housing_stock",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удается подключиться к базе данных:\n{ex.Message}",
                    "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}