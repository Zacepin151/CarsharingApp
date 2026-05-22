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
using System.Data.Entity;

namespace CarsharingApp
{
    /// <summary>
    /// Interaction logic for AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private Image firstButton;
        private bool isPuzzleSolved = false; 

        public AuthPage()
        {
            InitializeComponent();
            LoadPuzzle();
        }

        private void LoadPuzzle()
        {
            Random random = new Random();
            var pieces = Enumerable.Range(1, 4).OrderBy(x => random.Next()).ToList();
            pieces.ForEach(x =>
            {
                var img = new Image
                {
                    Source = new BitmapImage(new Uri($"Images/{x}.png", UriKind.Relative)),
                    Tag = x,
                    Stretch = Stretch.Fill
                };
                img.MouseLeftButtonUp += Pieces_Click;
                Puzzlegrid.Children.Add(img);
            });
        }

        private void CheckPuzzle()
        {
            bool isCorrect = Puzzlegrid.Children.OfType<Image>()
                .Select((img, i) => i + 1 == (int)img.Tag)
                .All(x => x);

            if (isCorrect)
            {
                isPuzzleSolved = true;
                MessageBox.Show("Капча решена! Теперь вы можете войти в систему.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                
                foreach (Image img in Puzzlegrid.Children)
                {
                    img.MouseLeftButtonUp -= Pieces_Click;
                }
            }
            else
            {
                isPuzzleSolved = false;
            }
        }

        private void Pieces_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image clicked)
            {
                if (firstButton == null)
                {
                    firstButton = clicked;
                    firstButton.Opacity = 0.5;
                    return;
                }

                if (clicked != firstButton)
                {
                    (firstButton.Source, clicked.Source) = (clicked.Source, firstButton.Source);
                    (firstButton.Tag, clicked.Tag) = (clicked.Tag, firstButton.Tag);
                }

                firstButton.Opacity = 1;
                firstButton = null;
                CheckPuzzle();
            }
        }

      
        private void ResetPuzzle()
        {
            Puzzlegrid.Children.Clear();
            isPuzzleSolved = false;
            LoadPuzzle();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = Username.Text.Trim();
            string password = Password.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!isPuzzleSolved)
            {
                MessageBox.Show("Пожалуйста, решите капчу (соберите пазл в правильном порядке!)",
                    "Требуется капча", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new CarsharingdbEntities())
            {
                var user = context.Users
                    .Where(u => u.username == username)
                    .FirstOrDefault();

                if (user == null)
                {
                    MessageBox.Show("Неправильный логин или пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                
                if (user.IsLoced == true)  
                {
                    MessageBox.Show("Вы заблокированы, пожалуйста обратитесь к администратору",
                        "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (user.lastloginDate.HasValue &&
                    (DateTime.Now - user.lastloginDate.Value).TotalDays > 30 &&
                    user.role != "Admin")
                {
                    user.IsLoced = true;  
                    context.SaveChanges();
                    MessageBox.Show("Вы заблокированы за 30 дней неактивности. Обратитесь к администратору",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (user.password == password)
                {
                    user.lastloginDate = DateTime.Now;
                    user.FailledLoginAttempts = 0;
                    context.SaveChanges();

                    MessageBox.Show("Вы успешно авторизировались!", "Добро пожаловать",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    isPuzzleSolved = false;

                    if (user.isFirstLogin == true)  
                    {
                        ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow();
                        changePasswordWindow.Owner = Window.GetWindow(this);
                        changePasswordWindow.ShowDialog();
                    }
                    else
                    {
                        Window mainWindow = Window.GetWindow(this);

                        if (user.role == "Admin")
                        {
                            AdminPage adminPage = new AdminPage();
                            mainWindow.Content = adminPage;
                        }
                        else if (user.role == "Manager" || user.role == "Managament")
                        {
                            CarsPage CarsPage = new CarsPage();
                            mainWindow.Content = CarsPage;
                        }
                        else
                        {
                            CarsharingPage carsharingPage = new CarsharingPage();
                            mainWindow.Content = carsharingPage;
                        }
                    }
                }
                else
                {
                    user.FailledLoginAttempts = (user.FailledLoginAttempts ?? 0) + 1;

                    if (user.FailledLoginAttempts >= 3)
                    {
                        user.IsLoced = true;
                        context.SaveChanges();
                        MessageBox.Show("Вы заблокированы. Обратитесь к администратору",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        int attemptsLeft = 3 - (user.FailledLoginAttempts ?? 0);
                        context.SaveChanges();
                        MessageBox.Show($"Неправильный пароль. Осталось попыток: {attemptsLeft}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}