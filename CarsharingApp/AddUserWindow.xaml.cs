using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarsharingApp
{
    public partial class AddUserWindow : Window
    {
        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string firstname = Fname_user.Text.Trim();
            string lastname = LName_user.Text.Trim();
            string username = Username.Text.Trim();
            string password = UserPasssword.Password;
            string role = (RoleCombobox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) ||
                string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Пожалуйста, заполните все поля");
                return;
            }

            try
            {
                using (var context = new CarsharingdbEntities())
                {
               
                    var existingUser = context.Users
                        .FirstOrDefault(u => u.username == username);

                    if (existingUser != null)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует");
                        return;
                    }

                    var newUser = new Users
                    {
                        firstname = firstname,
                        lastname = lastname,
                        username = username,
                        password = password,
                        role = role,
                        IsLoced = false,
                        isFirstLogin = true,
                        lastloginDate = null,
                        FailledLoginAttempts = 0
                    };

                    context.Users.Add(newUser);

                  
                    int result = context.SaveChanges();

                    MessageBox.Show($"Пользователь успешно добавлен! (Затронуто строк: {result})", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                string fullError = ex.Message;

                if (ex.InnerException != null)
                {
                    fullError += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";

                    if (ex.InnerException.InnerException != null)
                    {
                        fullError += $"\n\nSQL ошибка: {ex.InnerException.InnerException.Message}";
                    }
                }

                MessageBox.Show(fullError, "Ошибка при сохранении",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}