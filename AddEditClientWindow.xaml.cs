using Microsoft.Win32;
using MuhametshinLanguage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MuhametshinLanguage
{
    /// <summary>
    /// Логика взаимодействия для AddEditPageWindow.xaml
    /// </summary>
    public partial class AddEditClientWindow : Window
    {
        private MuhametshinLanguageEntities _context;
        private Client _currentClient;

        private MessageService _messageService = new MessageService();

        public AddEditClientWindow(Client client)
        {
            InitializeComponent();

            _context = MuhametshinLanguageEntities.GetContext();

            if (client != null)
            {
                _currentClient = client;
                Title = "Редактирование клиента";
            }
            else
            {
                _currentClient = new Client()
                {
                    Birthday = new DateTime(2000, 1, 1)
                };
                Title = "Добавление клиента";
                PanelClientID.Visibility = Visibility.Collapsed;
            }

            SetupUI();

            DataContext = _currentClient;
        }

        private void SetupUI()
        {
            if (_currentClient.PhotoPath != null)
            {
                BtnLogoDelete.Visibility = Visibility.Visible;
            }
            else
            {
                BtnLogoDelete.Visibility = Visibility.Collapsed;
            }



            if (_currentClient.GenderCode != null)
            {
                var gender = _context.Gender.FirstOrDefault(g =>
                    g.Code == _currentClient.GenderCode);

                if (gender != null)
                {
                    if (gender.Code == "м")
                    {
                        RButtonUp.IsChecked = true;
                    }
                    else if (gender.Code == "ж")
                    {
                        RButtonDown.IsChecked = true;
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!DataValidation())
            {
                return;
            }

            if (_context.Entry(_currentClient).State == System.Data.Entity.EntityState.Detached)
            {
                _context.Client.Add(_currentClient);
            }

            DialogResult = true;
        }

        private bool DataValidation()
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(TBoxClientLastName.Text))
            {
                errors.AppendLine("Укажите имя");
            }

            if (string.IsNullOrWhiteSpace(TBoxClientFirstName.Text))
            {
                errors.AppendLine("Укажите фамилию");
            }

            if (string.IsNullOrWhiteSpace(TBoxClientPatronymic.Text))
            {
                errors.AppendLine("Укажите отчество");
            }

            if (string.IsNullOrWhiteSpace(TBoxClientEmail.Text))
            {
                errors.AppendLine("Укажите email");
            }

            if (string.IsNullOrWhiteSpace(TBoxClientPhone.Text))
            {
                errors.AppendLine("Укажите телефон");
            }

            if (DPickerBirthday.SelectedDate == null)
            {
                errors.AppendLine("Укажите дату рождения");
            }
            else
            {
                var birthday = DPickerBirthday.SelectedDate.Value;

                if (birthday > DateTime.Now)
                {
                    errors.AppendLine("Дата рождения не может быть в будущем");
                }
            }

            if (_currentClient.GenderCode == null)
            {
                errors.AppendLine("Укажите пол клиента");
            }

            if (errors.Length > 0)
            {
                _messageService.ShowError(errors.ToString());
                return false;
            }

            return true;
        }

        private void BtnLogoEdit_Click(object sender, RoutedEventArgs e)
        {
            string clientsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Клиенты");

            var fileDialogWindow = new OpenFileDialog
            {
                InitialDirectory = clientsFolder
            };

            if (fileDialogWindow.ShowDialog() == true)
            {
                string sourcePath = fileDialogWindow.FileName;
                string targetPath = Path.Combine(clientsFolder, Path.GetFileName(sourcePath));

                if (sourcePath != targetPath)
                {
                    if (File.Exists(targetPath))
                    {
                        _messageService.ShowError($"Установить изображение \"{Path.GetFileName(targetPath)}\" невозможно, т.к. изображение с таким именем уже существует в системе");
                        return;
                    }

                    try
                    {
                        File.Copy(sourcePath, targetPath);
                    }
                    catch (Exception ex)
                    {
                        _messageService.ShowError($"Ошибка копирования файла.\n\n{ex.Message}");
                        return;
                    }
                }

                _currentClient.PhotoPath = $"/Клиенты/{fileDialogWindow.SafeFileName}";

                ImgLogo.GetBindingExpression(Image.SourceProperty)?.UpdateTarget();
                TBlockLogoPath.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();

                SetupUI();
            }
        }

        private void BtnLogoDelete_Click(object sender, RoutedEventArgs e)
        {
            var messageResult = _messageService.ShowWarningExtended("Удалить логотип клиента?");

            if (messageResult == MessageBoxResult.Yes)
            {
                _currentClient.PhotoPath = null;

                ImgLogo.GetBindingExpression(Image.SourceProperty)?.UpdateTarget();
                TBlockLogoPath.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();

                SetupUI();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            if (_currentClient != null)
            {
                var maleGender = _context.Gender.FirstOrDefault(g => g.Code == "м");

                if (maleGender != null)
                {
                    _currentClient.GenderCode = maleGender.Code;
                }

            }
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            if (_currentClient != null)
            {
                var femaleGender = _context.Gender.FirstOrDefault(g => g.Code == "ж");

                if (femaleGender != null)
                {
                    _currentClient.GenderCode = femaleGender.Code;
                }
            }
        }
    }
}
