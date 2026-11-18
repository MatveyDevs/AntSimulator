using AntSimulator.Models;

namespace AntSimulator.Services;

public class FieldService
{
    private readonly Random _random = new();

    public const int MinSize = 1;
    public const int MaxSize = 100;

    public Cell[,] CreateField(int width, int height)
    {
        ValidateSize(width, height);

        var field = new Cell[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                field[y, x] = new Cell();
            }
        }

        return field;
    }

    public void ClearField(Cell[,] field)
    {
        foreach (Cell cell in field)
        {
            cell.IsBlack = false;
        }
    }

    public void RandomizeField(
        Cell[,] field,
        double blackProbability = 0.3)
    {
        if (blackProbability is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(blackProbability));
        }

        foreach (Cell cell in field)
        {
            cell.IsBlack = _random.NextDouble() < blackProbability;
        }
    }

    public void ToggleCell(Cell[,] field, int x, int y)
    {
        if (!IsInside(field, x, y))
            return;

        field[y, x].Toggle();
    }

    public bool IsInside(Cell[,] field, int x, int y)
    {
        return x >= 0 &&
               y >= 0 &&
               x < field.GetLength(1) &&
               y < field.GetLength(0);
    }

    private static void ValidateSize(int width, int height)
    {
        if (width < MinSize || width > MaxSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                $"Ширина должна быть от {MinSize} до {MaxSize}.");
        }

        if (height < MinSize || height > MaxSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                $"Высота должна быть от {MinSize} до {MaxSize}.");
        }
    }
}