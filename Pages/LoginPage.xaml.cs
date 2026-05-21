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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
      

            private void Login_Click(object sender, RoutedEventArgs e)
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.Username == UsernameBox.Text && u.PasswordHash == PasswordBox.Password);
                if (user != null)
                {
                    if (user.IsFrozen)
                    {
                        MessageBox.Show($"Ваш аккаунт заморожен. Причина: {user.FreezeReason}", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    Core.CurrentUser = user;
                    var mainWin = new MainWindow();
                    mainWin.Show();
                    Window.GetWindow(this).Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void Register_Click(object sender, RoutedEventArgs e)
            {
                NavigationService.Navigate(new RegisterPage());
            }
        }
    }
