using System.Drawing;

namespace PublishingHouseApp
{
    /// <summary>
    /// Единая палитра цветов приложения — спокойная сине-серая гамма
    /// </summary>
    public static class AppColors
    {
        // Основной фон форм
        public static readonly Color FormBackground    = Color.FromArgb(240, 244, 248);

        // Панель навигации / шапка
        public static readonly Color NavBackground     = Color.FromArgb(44,  62,  80);
        public static readonly Color NavText           = Color.FromArgb(236, 240, 241);
        public static readonly Color NavHover          = Color.FromArgb(52,  73,  94);
        public static readonly Color NavSelected       = Color.FromArgb(41, 128, 185);

        // Кнопки
        public static readonly Color ButtonPrimary     = Color.FromArgb(41, 128, 185);
        public static readonly Color ButtonPrimaryHover= Color.FromArgb(52, 152, 219);
        public static readonly Color ButtonDanger      = Color.FromArgb(192,  57,  43);
        public static readonly Color ButtonDangerHover = Color.FromArgb(231,  76,  60);
        public static readonly Color ButtonText        = Color.White;

        // Контентная область
        public static readonly Color PanelBackground   = Color.White;
        public static readonly Color PanelBorder       = Color.FromArgb(189, 195, 199);

        // DataGridView
        public static readonly Color GridHeader        = Color.FromArgb(44,  62,  80);
        public static readonly Color GridHeaderText    = Color.White;
        public static readonly Color GridRowAlt        = Color.FromArgb(245, 248, 250);
        public static readonly Color GridSelection     = Color.FromArgb(174, 214, 241);

        // Текст
        public static readonly Color TextPrimary       = Color.FromArgb(44,  62,  80);
        public static readonly Color TextSecondary     = Color.FromArgb(127, 140, 141);

        // Заголовок страницы (Label)
        public static readonly Color SectionTitle      = Color.FromArgb(41, 128, 185);
    }
}
