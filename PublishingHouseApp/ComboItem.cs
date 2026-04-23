namespace PublishingHouseApp
{
    /// <summary>
    /// Элемент выпадающего списка с числовым ID и отображаемым текстом
    /// </summary>
    public class ComboItem
    {
        public int    Id   { get; }
        public string Text { get; }

        public ComboItem(int id, string text)
        {
            Id   = id;
            Text = text;
        }

        public override string ToString() => Text;
    }
}
