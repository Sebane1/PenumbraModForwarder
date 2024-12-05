namespace PenumbraModForwarder.UI.Models;

    public class InfoItem
    {
        public string Text { get; set; }
        public int Number { get; set; }

        public InfoItem(string text, int number)
        {
            Text = text;
            Number = number;
        }
    }