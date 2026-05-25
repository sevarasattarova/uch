using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace uch.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var user = Core.Context.Users
                .FirstOrDefault(u => u.Username == UsernameBox.Text && u.PasswordHash == PasswordBox.Password);

            if (user != null)
            {
                // ПРОВЕРКА НА ЗАМОРОЗКУ
                if (user.IsFrozen)
                {
                    // Сохраняем замороженного пользователя временно
                    Core.TempFrozenUser = user;

                    // Открываем окно с информацией о заморозке
                    var freezeWindow = new Window
                    {
                        Title = "Аккаунт заморожен",
                        Content = new AppealPage(),
                        Width = 450,
                        Height = 350,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    freezeWindow.Show();
                    Window.GetWindow(this).Close();
                    return;
                }

                Core.CurrentUser = user;
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Window.GetWindow(this).Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}