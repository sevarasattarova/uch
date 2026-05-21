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
    /// Логика взаимодействия для BookPage.xaml
    /// </summary>
    public partial class BookPage : Page
    {
      
            private int _bookId;
            public BookPage(int bookId)
            {
                InitializeComponent();
                _bookId = bookId;
                LoadBook();
                DataContextChanged += (s, e) => UpdateAdminControls();
            }

        private void LoadBook()
        {
            var bookData = Core.Context.Books
                .Where(b => b.Id == _bookId)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Description,
                    b.CoverImageUrl,
                    b.TextContent,
                    b.Rating,
                    b.IsFrozen,
                    b.FreezeReason,
                    AuthorName = b.Users.Username,
                    Genres = b.Genres.Select(bg => bg.Name).ToList(),
                    Reviews = b.Reviews
                        .Where(r => !r.IsFrozen)
                        .Select(r => new
                        {
                            r.Id,
                            r.Text,
                            r.Rating,
                            r.CreatedAt,
                            r.IsFrozen,
                            UserName = r.Users.Username
                        })
                        .ToList()
                })
                .FirstOrDefault();

            if (bookData == null) return;

            // Устанавливаем DataContext на анонимный объект
            DataContext = bookData;
            ReviewsList.ItemsSource = bookData.Reviews;
            UpdateAdminControls();
        }

        private void UpdateAdminControls()
            {
                if (Core.IsAdmin)
                {
                    FreezeBookBtn.Visibility = Visibility.Visible;
                }
            }

        private void ReadText_Click(object sender, RoutedEventArgs e)
                 {
                NavigationService.Navigate(new ReadBookPage(_bookId));
            
        }

        private void ReportBook_Click(object sender, RoutedEventArgs e)
            {
                if (!Core.IsAuthenticated)
                {
                    MessageBox.Show("Войдите, чтобы отправить жалобу.");
                    return;
                }
                var report = new Reports
                {
                    ReporterId = Core.CurrentUser.Id,
                    BookId = _bookId,
                    Reason = "Жалоба на книгу",
                    IsResolved = false,
                    CreatedAt = DateTime.Now
                };
                Core.Context.Reports.Add(report);
                Core.Context.SaveChanges();
                MessageBox.Show("Жалоба отправлена администратору.");
            }

        private void ReportAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (!Core.IsAuthenticated)
            {
                MessageBox.Show("Войдите, чтобы отправить жалобу.");
                return;
            }

            var book = Core.Context.Books.Find(_bookId);
            if (book == null) return;

            var report = new Reports
            {
                ReporterId = Core.CurrentUser.Id,
                BookId = _bookId,
                Reason = $"Жалоба на автора (книга: {book.Title})",
                IsResolved = false,
                CreatedAt = DateTime.Now
            };
            Core.Context.Reports.Add(report);
            Core.Context.SaveChanges();
            MessageBox.Show("Жалоба на автора отправлена администратору.");
        }

        private void ReportReview_Click(object sender, RoutedEventArgs e)
            {
                int reviewId = (int)((Button)sender).Tag;
                var report = new Reports
                {
                    ReporterId = Core.CurrentUser.Id,
                    ReviewId = reviewId,
                    Reason = "Жалоба на отзыв",
                    IsResolved = false,
                    CreatedAt = DateTime.Now
                };
                Core.Context.Reports.Add(report);
                Core.Context.SaveChanges();
                MessageBox.Show("Жалоба на отзыв отправлена.");
            }

        private void FreezeBook_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на администратора
            if (!Core.IsAdmin)
            {
                MessageBox.Show("Только администратор может замораживать книги.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var book = Core.Context.Books.Find(_bookId);
            if (book == null)
            {
                MessageBox.Show("Книга не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (book.IsFrozen)
            {
                MessageBox.Show("Книга уже заморожена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Запрашиваем подтверждение
            if (MessageBox.Show($"Вы уверены, что хотите заморозить книгу '{book.Title}'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                book.IsFrozen = true;
                book.FreezeReason = $"Заморожено администратором {Core.CurrentUser?.Username}";
                Core.Context.SaveChanges();

                MessageBox.Show($"Книга '{book.Title}' заморожена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadBook(); // Обновляем страницу
            }
        }

        private void FreezeReview_Click(object sender, RoutedEventArgs e)
            {
                int reviewId = (int)((Button)sender).Tag;
                var review = Core.Context.Reviews.Find(reviewId);
                if (review != null)
                {
                    review.IsFrozen = true;
                    review.FreezeReason = "Заморожено администратором";
                    Core.Context.SaveChanges();
                    MessageBox.Show("Отзыв заморожен.");
                    LoadBook();
                }
            }

            private void AddReview_Click(object sender, RoutedEventArgs e)
            {
                if (!Core.IsAuthenticated)
                {
                    MessageBox.Show("Войдите, чтобы оставить отзыв.");
                    return;
                }
                int rating = RatingCombo.SelectedIndex + 1;
                var review = new Reviews
                {
                    UserId = Core.CurrentUser.Id,
                    BookId = _bookId,
                    Rating = rating,
                    Text = ReviewText.Text,
                    CreatedAt = DateTime.Now
                };
                Core.Context.Reviews.Add(review);
                Core.Context.SaveChanges();
                // обновляем средний рейтинг книги
                var book = Core.Context.Books.Find(_bookId);
                var avg = Core.Context.Reviews.Where(r => r.BookId == _bookId && !r.IsFrozen).Average(r => r.Rating);
                book.Rating = (decimal)avg;
                Core.Context.SaveChanges();
                LoadBook();
                ReviewText.Text = "";
                MessageBox.Show("Отзыв добавлен.");
            }
        }
    }
