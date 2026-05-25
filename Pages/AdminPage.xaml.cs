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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {


        public AdminPage()
        {
            InitializeComponent();
            LoadAllTabs();
        }

        private void LoadAllTabs()
        {
            LoadReports();
            LoadUnfreezeRequests();
            LoadAuthorRequests();
            LoadFrozenItems();
            LoadUsers();
        }

        // ===================== Жалобы =====================
        private void LoadReports()
        {
            var reports = Core.Context.Reports.Where(r => !r.IsResolved).ToList();

            var result = new List<object>();
            foreach (var report in reports)
            {
                var reporter = Core.Context.Users.FirstOrDefault(u => u.Id == report.ReporterId);
                string bookTitle = null;
                if (report.BookId != null)
                {
                    var book = Core.Context.Books.FirstOrDefault(b => b.Id == report.BookId);
                    if (book != null) bookTitle = book.Title;
                }
                string reviewText = null;
                if (report.ReviewId != null)
                {
                    var review = Core.Context.Reviews.FirstOrDefault(r => r.Id == report.ReviewId);
                    if (review != null) reviewText = review.Text;
                }

                result.Add(new
                {
                    report.Id,
                    report.Reason,
                    ReporterName = reporter != null ? reporter.Username : "",
                    BookTitle = bookTitle,
                    ReviewText = reviewText
                });
            }
            ReportsList.ItemsSource = result;
        }

        private void ApproveReport_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var report = Core.Context.Reports.Find(id);

            if (report == null) return;

            // ПРОВЕРКА: это жалоба на автора?
            if (report.Reason != null && report.Reason.Contains("жалоба на автора"))
            {
                // НАХОДИМ АВТОРА через книгу
                var book = Core.Context.Books.Find(report.BookId);
                if (book != null)
                {
                    var author = Core.Context.Users.Find(book.AuthorId);
                    if (author != null)
                    {
                        // БЛОКИРУЕМ АВТОРА, А НЕ КНИГУ!
                        author.IsFrozen = true;
                        author.FreezeReason = $"Заморожено по жалобе #{report.Id}. Причина: {report.Reason}";
                        Core.Context.SaveChanges();

                        MessageBox.Show($"АВТОР {author.Username} ЗАМОРОЖЕН!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            // Жалоба на книгу
            else if (report.BookId != null && report.ReviewId == null)
            {
                var book = Core.Context.Books.Find(report.BookId);
                if (book != null)
                {
                    book.IsFrozen = true;
                    book.FreezeReason = $"Заморожено по жалобе #{report.Id}";
                    Core.Context.SaveChanges();
                    MessageBox.Show($"Книга '{book.Title}' заморожена.", "Успех");
                }
            }
            // Жалоба на отзыв
            else if (report.ReviewId != null)
            {
                var review = Core.Context.Reviews.Find(report.ReviewId);
                if (review != null)
                {
                    review.IsFrozen = true;
                    review.FreezeReason = $"Заморожено по жалобе #{report.Id}";
                    Core.Context.SaveChanges();
                    MessageBox.Show("Отзыв заморожен.", "Успех");
                }
            }

            report.IsResolved = true;
            Core.Context.SaveChanges();

            LoadReports();
            LoadFrozenItems();
            LoadUsers();
        }

        private void RejectReport_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var report = Core.Context.Reports.Find(id);
            if (report != null)
            {
                report.IsResolved = true;
                Core.Context.SaveChanges();
                LoadReports();
            }
        }


        // ===================== Заявки на разморозку =====================
        private void LoadUnfreezeRequests()
        {
            var requests = Core.Context.FreezeAppeals
                .Where(r => r.Status == "Pending")
                .Select(r => new
                {
                    r.Id,
                    r.TargetType,
                    r.TargetId,
                    r.Reason,
                    UserName = r.Users.Username,
                    TargetName = r.TargetType == "Book"
                        ? Core.Context.Books.Where(b => b.Id == r.TargetId).Select(b => b.Title).FirstOrDefault()
                        : (r.TargetType == "Review"
                            ? Core.Context.Reviews.Where(rv => rv.Id == r.TargetId).Select(rv => rv.Text).FirstOrDefault()
                            : r.Users.Username)
                })
                .ToList();

            FreezeAppealsList.ItemsSource = requests;
        }

        private void ApproveUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var request = Core.Context.FreezeAppeals.Find(id);

            if (request == null) return;

            if (request.TargetType == "User")
            {
                var user = Core.Context.Users.Find(request.TargetId);
                if (user != null)
                {
                    user.IsFrozen = false;
                    user.FreezeReason = null;
                    MessageBox.Show($"✅ Пользователь {user.Username} разморожен.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (request.TargetType == "Book")
            {
                var book = Core.Context.Books.Find(request.TargetId);
                if (book != null)
                {
                    book.IsFrozen = false;
                    book.FreezeReason = null;
                    MessageBox.Show($"✅ Книга '{book.Title}' разморожена.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (request.TargetType == "Review")
            {
                var review = Core.Context.Reviews.Find(request.TargetId);
                if (review != null)
                {
                    review.IsFrozen = false;
                    review.FreezeReason = null;
                    MessageBox.Show($"✅ Отзыв разморожен.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            request.Status = "Approved";
            Core.Context.SaveChanges();

            LoadUnfreezeRequests();
            LoadFrozenItems();
        }

        private void RejectUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var request = Core.Context.FreezeAppeals.Find(id);

            if (request != null)
            {
                request.Status = "Rejected";
                Core.Context.SaveChanges();
                LoadUnfreezeRequests();

                MessageBox.Show("✅ Заявка отклонена.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        // ===================== Заявки на разморозку =====================
        private void LoadFreezeAppeals()
        {
            var appeals = Core.Context.FreezeAppeals
                .Where(a => a.Status == "Pending")
                .Select(a => new
                {
                    a.Id,
                    a.TargetType,
                    a.TargetId,
                    a.Reason,
                    UserName = a.Users.Username
                })
                .ToList();
            FreezeAppealsList.ItemsSource = appeals;
        }

        private void ApproveFreezeAppeal_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var appeal = Core.Context.FreezeAppeals.Find(id);

            if (appeal != null)
            {
                if (appeal.TargetType == "User")
                {
                    var user = Core.Context.Users.Find(appeal.TargetId);
                    if (user != null) user.IsFrozen = false;
                }
                else if (appeal.TargetType == "Book")
                {
                    var book = Core.Context.Books.Find(appeal.TargetId);
                    if (book != null) book.IsFrozen = false;
                }
                else if (appeal.TargetType == "Review")
                {
                    var review = Core.Context.Reviews.Find(appeal.TargetId);
                    if (review != null) review.IsFrozen = false;
                }

                appeal.Status = "Approved";
                Core.Context.SaveChanges();
                LoadFreezeAppeals();
                LoadFrozenItems();

                MessageBox.Show("Объект разморожен.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RejectFreezeAppeal_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var appeal = Core.Context.FreezeAppeals.Find(id);

            if (appeal != null)
            {
                appeal.Status = "Rejected";
                Core.Context.SaveChanges();
                LoadFreezeAppeals();

                MessageBox.Show("Заявка отклонена.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        // ===================== Заявки на роль автора =====================
        private void LoadAuthorRequests()
        {
            var requests = Core.Context.AuthorRequests.Where(r => r.Status == "Pending").ToList();

            var result = new List<object>();
            foreach (var request in requests)
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.Id == request.UserId);
                result.Add(new
                {
                    request.Id,
                    RequestDate = request.RequestDate.ToString("dd.MM.yyyy"),
                    UserName = user != null ? user.Username : ""
                });
            }
            AuthorRequestsList.ItemsSource = result;
        }

        private void ApproveAuthorRequest_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var request = Core.Context.AuthorRequests.Find(id);
            if (request != null)
            {
                var user = Core.Context.Users.Find(request.UserId);
                if (user != null)
                {
                    var authorRole = Core.Context.Roles.FirstOrDefault(r => r.Name == "Author");
                    if (authorRole != null)
                        user.RoleId = authorRole.Id;
                }
                request.Status = "Approved";
                Core.Context.SaveChanges();
                LoadAuthorRequests();
                LoadUsers();
            }
        }

        private void RejectAuthorRequest_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var request = Core.Context.AuthorRequests.Find(id);
            if (request != null)
            {
                request.Status = "Rejected";
                Core.Context.SaveChanges();
                LoadAuthorRequests();
            }
        }

        private void LoadFrozenItems()
        {
            var frozenItems = new List<string>();

            var frozenUsers = Core.Context.Users.Where(u => u.IsFrozen).ToList();
            foreach (var user in frozenUsers)
            {
                frozenItems.Add($"Пользователь: {user.Username} (причина: {user.FreezeReason})");
            }

            var frozenBooks = Core.Context.Books.Where(b => b.IsFrozen).ToList();
            foreach (var book in frozenBooks)
            {
                frozenItems.Add($"Книга: {book.Title} (причина: {book.FreezeReason})");
            }

            var frozenReviews = Core.Context.Reviews.Where(r => r.IsFrozen).ToList();
            foreach (var review in frozenReviews)
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.Id == review.UserId);
                string userName = user != null ? user.Username : "Неизвестно";
                string shortText = review.Text.Length > 50 ? review.Text.Substring(0, 50) + "..." : review.Text;
                frozenItems.Add($"Отзыв от {userName}: {shortText}");
            }

            FrozenItemsList.ItemsSource = frozenItems;
        }

        private void LoadUsers()
        {
            var users = Core.Context.Users.ToList();

            var result = new List<object>();
            foreach (var user in users)
            {
                var role = Core.Context.Roles.FirstOrDefault(r => r.Id == user.RoleId);
                result.Add(new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    RoleName = role != null ? role.Name : "",
                    user.IsFrozen
                });
            }
            UsersList.ItemsSource = result;
        }

        private void ChangeUserRole_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;
            var user = Core.Context.Users.Find(userId);
            if (user == null) return;

            var roles = Core.Context.Roles.ToList();
            var dialog = new ChooseRoleDialog(roles, user.RoleId, userId);

            if (NavigationService != null)
            {
                NavigationService.Navigated += OnRoleDialogReturn;
                NavigationService.Navigate(dialog);
            }
        }

        private void OnRoleDialogReturn(object sender, NavigationEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigated -= OnRoleDialogReturn;
            }

            // ПРОВЕРКА ФЛАГА
            if (ChooseRoleDialog.IsConfirmed)
            {
                int userId = ChooseRoleDialog.TargetUserId;
                int newRoleId = ChooseRoleDialog.SelectedRoleId;

                var user = Core.Context.Users.Find(userId);
                if (user != null)
                {
                    user.RoleId = newRoleId;
                    Core.Context.SaveChanges();
                    LoadUsers();
                    MessageBox.Show($"Роль пользователя {user.Username} изменена", "Успех");
                }
            }
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;
            var user = Core.Context.Users.Find(userId);
            if (user != null)
            {
                string newPass = "newpassword";
                user.PasswordHash = newPass;
                Core.Context.SaveChanges();
                MessageBox.Show($"Пароль для {user.Username} изменён на {newPass}");
            }
        }
    }
}