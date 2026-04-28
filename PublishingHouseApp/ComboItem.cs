namespace PublishingHouseApp
{
    // Элемент выпадающего списка (ComboBox).
    // Хранит числовой ID записи из БД и отображаемый текст.
    // ToString() возвращает Text — именно это видит пользователь в списке.
    public class ComboItem
    {
        // Первичный ключ записи в БД (используется при сохранении)
        public int    Id   { get; }

        // Текст который видит пользователь в выпадающем списке
        public string Text { get; }

        public ComboItem(int id, string text)
        {
            Id   = id;
            Text = text;
        }

        // ComboBox вызывает ToString() для отображения — возвращаем человекочитаемый текст
        public override string ToString() => Text;
    }
}
