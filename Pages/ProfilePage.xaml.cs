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
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
   
        public partial class ProfilePage : Page
        {
            public ProfilePage()
            {
                InitializeComponent();
                LoadUserInfo();
                LoadUserReviews();
            }

            private void LoadUserInfo()
            {
                // Проверка, что пользователь авторизован
                if (Core.CurrentUser == null)
                {
                    UserInfo.Text = "Пользователь не авторизован";
                    RequestAuthorBtn.Visibility = Visibility.Collapsed;
                    return;
                }

                var user = Core.CurrentUser;

                // Загружаем роль пользователя (если нужно)
                var roleName = "Неизвестно";
                if (user.RoleId > 0)
                {
                    var role = Core.Context.Roles.FirstOrDefault(r => r.Id == user.RoleId);
                    roleName = role?.Name ?? "Неизвестно";
                }

                UserInfo.Text = $"Имя: {user.Username}\nEmail: {user.Email}\nРоль: {roleName}";

                // Кнопка заявки видна только если роль не Author
                if (roleName == "Author")
                    RequestAuthorBtn.Visibility = Visibility.Collapsed;
                else
                    RequestAuthorBtn.Visibility = Visibility.Visible;
            }

            private void LoadUserReviews()
            {
                if (Core.CurrentUser == null) return;

                var reviews = Core.Context.Reviews
                    .Where(r => r.UserId == Core.CurrentUser.Id)
                    .Select(r => new
                    {
                        r.Text,
                        r.Rating,
                        BookTitle = r.Books != null ? r.Books.Title : "Неизвестно",
                        r.CreatedAt
                    })
                    .ToList();

                MyReviewsList.ItemsSource = reviews;
            }

            private void RequestAuthor_Click(object sender, RoutedEventArgs e)
            {
                if (Core.CurrentUser == null)
                {
                    MessageBox.Show("Авторизуйтесь для подачи заявки");
                    return;
                }

                if (Core.Context.AuthorRequests.Any(ar => ar.UserId == Core.CurrentUser.Id && ar.Status == "Pending"))
                {
                    MessageBox.Show("Заявка уже отправлена и ожидает рассмотрения.");
                    return;
                }

                var request = new AuthorRequests
                {
                    UserId = Core.CurrentUser.Id,
                    Status = "Pending",
                    RequestDate = DateTime.Now
                };
                Core.Context.AuthorRequests.Add(request);
                Core.Context.SaveChanges();
                MessageBox.Show("Заявка на роль автора отправлена администратору.");
            }
        }
    }
