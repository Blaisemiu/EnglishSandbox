namespace EnglishSandbox.Api.Models;

public class ExerciseElement
{
    public int Id { get; set; }

    public int ExerciseId { get; set; }

    public string Text { get; private set; } = "";

    public int CorrectPosition { get; private set; }

    public void Update(string text, int correctPosition)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Текст элемента не должен быть пустым.");

        if (correctPosition < 1)
            throw new ArgumentException("Позиция должна начинаться с 1.");

        Text = text.Trim();
        CorrectPosition = correctPosition;
    }
}