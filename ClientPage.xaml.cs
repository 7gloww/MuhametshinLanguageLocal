
using MuhametshinLanguage;

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

namespace MuhametshinLanguage
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        private MuhametshinLanguageEntities _context;

        private MessageService _messageService = new MessageService();

        private List<Client> _filteredClients;
        private int pageSize = 10;
        private int currentPage = 1;

        public ClientPage()
        {
            InitializeComponent();

            _context = MuhametshinLanguageEntities.GetContext();

            CBoxNumRecordsPage.SelectedIndex = 0;

            GenderSort.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;

            UpdateClients();
        }

        private void UpdateClients()
        {
            var currentClients = _context.Client.ToList();

            int totalNumAgents = currentClients.Count;

            string searchDigits = new string(TBoxSearch.Text.Where(char.IsDigit).ToArray());



            currentClients = currentClients
            .Where(a =>
                a.FullName.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
                (!string.IsNullOrEmpty(searchDigits) && new string(a.Phone.Where(char.IsDigit).ToArray()).Contains(searchDigits)) ||
                a.Email.Contains(TBoxSearch.Text.ToLower())
                )
            .ToList();



            if (GenderSort.SelectedIndex == 1)
            {
                currentClients = currentClients.Where(a => a.Gender.Name == "женский").ToList();
            }
            if (GenderSort.SelectedIndex == 2)
            {
                currentClients = currentClients.Where(a => a.Gender.Name == "мужской").ToList();
            }



            if (ComboSort.SelectedIndex == 1)
            {
                currentClients = currentClients.OrderBy(a => a.FirstName).ToList();
            }
            if (ComboSort.SelectedIndex == 2)
            {
                currentClients = currentClients.OrderByDescending(a => a.LastVisitTime).ToList();
            }
            if (ComboSort.SelectedIndex == 3)
            {
                currentClients = currentClients.OrderByDescending(a => a.NumVisits).ToList();
            }



            int currentNumAgents = currentClients.Count;

            TBlockNumRecords.Text = $"{currentNumAgents} из {totalNumAgents}";

            _filteredClients = currentClients;
            currentPage = 1;
            ChangePage();

        }

        private void ChangePage()
        {
            LBoxPages.Items.Clear();

            int totalPages = (_filteredClients.Count + pageSize - 1) / pageSize;

            for (int i = 1; i <= totalPages; i++)
            {
                LBoxPages.Items.Add(i);
            }

            LBoxPages.SelectedItem = currentPage;

            var agentsPage = _filteredClients
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize).ToList();

            ListViewClients.ItemsSource = agentsPage;
        }

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                ChangePage();
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (_filteredClients.Count + pageSize - 1) / pageSize;
            if (currentPage < totalPages)
            {
                currentPage++;
                ChangePage();
            }
        }

        private void LBoxPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBoxPages.SelectedItem is int page && page != currentPage)
            {
                currentPage = page;
                ChangePage();
            }
        }

        private void CBoxNumRecordsPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_filteredClients == null) return;

            switch (CBoxNumRecordsPage.SelectedIndex)
            {
                case 0:
                    pageSize = 10;
                    break;
                case 1:
                    pageSize = 50;
                    break;
                case 2:
                    pageSize = 200;
                    break;
                case 3:
                    if (_filteredClients.Count > 0)
                    {
                        pageSize = _filteredClients.Count;
                    }
                    else
                    {
                        pageSize = 1;
                    }
                    break;
            }

            currentPage = 1;

            ChangePage();
        }

        private void BtnDeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Client client)
            {
                var messageResult = _messageService.ShowWarningExtended($"Удалить клиента {client.FullName}");

                if (messageResult == MessageBoxResult.Yes)
                {
                    if (!client.ClientService.Any())
                    {
                        _context.Client.Remove(client);

                        _context.SaveChanges();

                        UpdateClients();
                    }
                    else
                    {
                        _messageService.ShowWarning($"Невозможно удалить клиента {client.FullName}, т.к. у него есть информация о посещениях");
                    }
                }
            }
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateClients();
        }

        private void GenderSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }
    }
}