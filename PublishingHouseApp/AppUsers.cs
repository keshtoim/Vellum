namespace PublishingHouseApp
{
    /// <summary>
    /// Роли пользователей системы
    /// </summary>
    public enum UserRole
    {
        None,
        Admin,
        Editor,
        Manager
    }

    /// <summary>
    /// Хранит учётные данные пользователей (проверка на уровне приложения)
    /// </summary>
    public static class AppUsers
    {
        // -------------------------------------------------------
        // Логины и пароли
        // -------------------------------------------------------
        private const string AdminLogin    = "admin";
        private const string AdminPassword = "admin123";

        private const string EditorLogin    = "editor";
        private const string EditorPassword = "editor123";

        private const string ManagerLogin    = "manager";
        private const string ManagerPassword = "manager123";
        // -------------------------------------------------------

        /// <summary>
        /// Проверяет логин/пароль и возвращает роль пользователя.
        /// Если данные неверны — возвращает UserRole.None
        /// </summary>
        public static UserRole Authenticate(string login, string password)
        {
            if (login == AdminLogin   && password == AdminPassword)   return UserRole.Admin;
            if (login == EditorLogin  && password == EditorPassword)  return UserRole.Editor;
            if (login == ManagerLogin && password == ManagerPassword) return UserRole.Manager;

            return UserRole.None;
        }

        /// <summary>
        /// Возвращает отображаемое имя роли на русском
        /// </summary>
        public static string GetRoleDisplayName(UserRole role)
        {
            switch (role)
            {
                case UserRole.Admin:   return "Администратор";
                case UserRole.Editor:  return "Редактор";
                case UserRole.Manager: return "Менеджер";
                default:               return "Неизвестно";
            }
        }
    }
}
