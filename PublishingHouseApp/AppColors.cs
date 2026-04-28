using System.Drawing;

namespace PublishingHouseApp
{
    // Единая цветовая палитра приложения — все цвета определены в одном месте,
    // чтобы легко менять оформление не трогая каждый элемент отдельно
    public static class AppColors
    {
        // Фон форм и неактивных полей ввода
        public static readonly Color FormBackground    = Color.FromArgb(240, 244, 248);

        // Боковая навигационная панель и шапка
        public static readonly Color NavBackground     = Color.FromArgb(44,  62,  80);
        public static readonly Color NavText           = Color.FromArgb(236, 240, 241);
        public static readonly Color NavHover          = Color.FromArgb(52,  73,  94);  // при наведении
        public static readonly Color NavSelected       = Color.FromArgb(41, 128, 185);  // активный пункт

        // Кнопки действий (синие — основные, красные — удаление)
        public static readonly Color ButtonPrimary     = Color.FromArgb(41, 128, 185);
        public static readonly Color ButtonPrimaryHover= Color.FromArgb(52, 152, 219);
        public static readonly Color ButtonDanger      = Color.FromArgb(192,  57,  43);
        public static readonly Color ButtonDangerHover = Color.FromArgb(231,  76,  60);
        public static readonly Color ButtonText        = Color.White;

        // Контентная область — фон белый, граница серая
        public static readonly Color PanelBackground   = Color.White;
        public static readonly Color PanelBorder       = Color.FromArgb(189, 195, 199);

        // DataGridView — цвета заголовков, чередования строк и выделения
        public static readonly Color GridHeader        = Color.FromArgb(44,  62,  80);
        public static readonly Color GridHeaderText    = Color.White;
        public static readonly Color GridRowAlt        = Color.FromArgb(245, 248, 250);
        public static readonly Color GridSelection     = Color.FromArgb(174, 214, 241);

        // Цвета текста — основной тёмный и серый для подписей
        public static readonly Color TextPrimary       = Color.FromArgb(44,  62,  80);
        public static readonly Color TextSecondary     = Color.FromArgb(127, 140, 141);

        // Акцентный цвет для заголовков разделов
        public static readonly Color SectionTitle      = Color.FromArgb(41, 128, 185);
    }
}
