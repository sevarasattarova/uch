using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace uch.Pages
{
    public partial class BookPage : Page
    {
        private int _bookId;

        public BookPage(int bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            Loaded += BookPage_Loaded; // Подписываемся на событие загрузки страницы
        }

        private void BookPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBook();
            DataContextChanged += (s, ev) => UpdateAdminControls();
        }

        private void LoadBook()
        {
            try
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
                        Genres = b.Genres.Select(g => g.Name).ToList(),
                        Reviews = b.Reviews
                            .Where(r => !r.IsFrozen)
                            .Select(r => new
                            {
                                r.Id,
                                r.Text,
                                r.Rating,
                                r.CreatedAt,
                                UserName = r.Users.Username
                            })
                            .ToList()
                    })
                    .FirstOrDefault();

                if (bookData == null)
                {
                    MessageBox.Show("Книга не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    GoBack();
                    return;
                }

                // Проверка заморозки
                if (bookData.IsFrozen && !Core.IsAdmin)
                {
                    MessageBox.Show("Эта книга заморожена администратором и недоступна для чтения.",
                        "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                    GoBack();
                    return;
                }

                // Устанавливаем DataContext
                DataContext = bookData;

                // Устанавливаем список отзывов
                ReviewsList.ItemsSource = bookData.Reviews;

                // Обновляем кнопки для администратора
                UpdateAdminControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книги: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                GoBack();
            }
        }

        private void GoBack()
        {
            try
            {
                if (NavigationService != null && NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    // Если NavigationService недоступен, закрываем через Frame
                    var frame = this.Parent as Frame;
                    if (frame != null && frame.CanGoBack)
                    {
                        frame.GoBack();
                    }
                    else
                    {
                        // Если ничего не помогает, закрываем окно
                        Window.GetWindow(this)?.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка навигации: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateAdminControls()
        {
            if (Core.IsAdmin && FreezeBookBtn != null)
            {
                FreezeBookBtn.Visibility = Visibility.Visible;
            }
        }

        private void ReadText_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(new ReadBookPage(_bookId));
            }
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
            // Проверка авторизации
            if (!Core.IsAuthenticated)
            {
                MessageBox.Show("Войдите, чтобы отправить жалобу.");
                return;
            }

            // ПРОВЕРКА: существует ли книга
            if (_bookId <= 0)
            {
                MessageBox.Show("Ошибка: не выбрана книга. Пожалуйста, откройте страницу книги и попробуйте снова.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var book = Core.Context.Books.FirstOrDefault(b => b.Id == _bookId);

            if (book == null)
            {
                MessageBox.Show("Книга не найдена. Возможно, она была удалена.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (book.Users == null)
            {
                MessageBox.Show("Автор книги не найден.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var report = new Reports
            {
                ReporterId = Core.CurrentUser.Id,
                BookId = _bookId,
                Reason = $"жалоба на автора: {book.Users.Username}",  // ← ВАЖНО: именно так начинается
                IsResolved = false,
                CreatedAt = DateTime.Now
            };

            Core.Context.Reports.Add(report);
            Core.Context.SaveChanges();

            MessageBox.Show($"Жалоба на автора отправлена.", "Успех");
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
            if (!Core.IsAdmin)
            {
                MessageBox.Show("Только администратор может замораживать книги.", "Доступ запрещён",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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

            if (MessageBox.Show($"Вы уверены, что хотите заморозить книгу '{book.Title}'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                book.IsFrozen = true;
                book.FreezeReason = $"Заморожено администратором {Core.CurrentUser?.Username}";
                Core.Context.SaveChanges();

                MessageBox.Show($"Книга '{book.Title}' заморожена.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadBook();
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

            if (string.IsNullOrWhiteSpace(ReviewText.Text))
            {
                MessageBox.Show("Введите текст отзыва.");
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

            // Обновляем средний рейтинг книги
            var book = Core.Context.Books.Find(_bookId);
            if (book != null)
            {
                var avg = Core.Context.Reviews
                    .Where(r => r.BookId == _bookId && !r.IsFrozen)
                    .Average(r => (double)r.Rating);
                book.Rating = (decimal)avg;
                Core.Context.SaveChanges();
            }

            ReviewText.Text = "";
            LoadBook();
            MessageBox.Show("Отзыв добавлен.");
        }
    }
}