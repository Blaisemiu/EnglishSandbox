namespace EnglishSandbox.Api.Models;

public class Exercise
{
    public int Id { get; set; }

    public int AuthorId { get; set; }

    public string Title { get; private set; } = "";

    public string Instruction { get; private set; } = "";

    public bool IsPublished { get; private set; }

    public List<ExerciseElement> Elements { get; set; } = new();

    public void Update(string title, string instruction)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название не должно быть пустым.");

        if (string.IsNullOrWhiteSpace(instruction))
            throw new ArgumentException("Инструкция не должна быть пустой.");

        Title = title.Trim();
        Instruction = instruction.Trim();

        Unpublish();
    }

    public bool IsReadyForPublication()
    {
        if (string.IsNullOrWhiteSpace(Title) ||
            string.IsNullOrWhiteSpace(Instruction))
        {
            return false;
        }

        if (Elements.Count < 2)
            return false;

        if (Elements.Any(element => string.IsNullOrWhiteSpace(element.Text)))
            return false;

        var positions = Elements
            .OrderBy(element => element.CorrectPosition)
            .Select(element => element.CorrectPosition);

        return positions.SequenceEqual(
            Enumerable.Range(1, Elements.Count));
    }

    public void Publish()
    {
        if (!IsReadyForPublication())
        {
            throw new InvalidOperationException(
                "Для публикации заполни название, инструкцию и минимум " +
                "два элемента с позициями от 1 без пропусков и повторений.");
        }

        IsPublished = true;
    }

    public void Unpublish()
    {
        IsPublished = false;
    }

    public bool CheckAnswer(int[] elementIds)
    {
        if (elementIds == null)
            throw new ArgumentException("Передай порядок элементов.");

        if (!IsReadyForPublication())
        {
            throw new InvalidOperationException(
                "Сначала заполни задание и правильные позиции элементов.");
        }

        var correctOrder = Elements
            .OrderBy(element => element.CorrectPosition)
            .Select(element => element.Id);

        return correctOrder.SequenceEqual(elementIds);
    }
}