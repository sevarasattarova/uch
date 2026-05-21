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

namespace uch.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
       
            public RegisterPage()
            {
                InitializeComponent();
            }

            private void Register_Click(object sender, RoutedEventArgs e)
            {
                // Проверка заполнения полей
                if (string.IsNullOrWhiteSpace(UsernameBox.Text))
                {
                    MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrWhiteSpace(EmailBox.Text))
                {
                    MessageBox.Show("Введите email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (PasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка уникальности логина
                if (Core.Context.Users.Any(u => u.Username == UsernameBox.Text))
                {
                    MessageBox.Show("Логин уже занят", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка уникальности email
                if (Core.Context.Users.Any(u => u.Email == EmailBox.Text))
                {
                    MessageBox.Show("Email уже зарегистрирован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    // Получаем роль "User"
                    var userRole = Core.Context.Roles.FirstOrDefault(r => r.Name == "User");
                    if (userRole == null)
                    {
                        MessageBox.Show("Роль 'User' не найдена в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var user = new Users
                    {
                        Username = UsernameBox.Text,
                        Email = EmailBox.Text,
                        PasswordHash = PasswordBox.Password,
                        RoleId = userRole.Id,
                        IsFrozen = false,
                        CreatedAt = DateTime.Now  // ← Добавляем дату создания
                    };

                    Core.Context.Users.Add(user);
                    Core.Context.SaveChanges();

                    MessageBox.Show("Регистрация успешна! Теперь войдите.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.Navigate(new LoginPage());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при регистрации: {ex.InnerException?.Message ?? ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void Back_Click(object sender, RoutedEventArgs e)
            {
                NavigationService.GoBack();
            }
        }
    }