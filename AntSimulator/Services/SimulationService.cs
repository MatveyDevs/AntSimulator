
using AntSimulator.Models;

namespace AntSimulator.Services;

public class SimulationService
{
    private readonly Random _random = new();

    public bool TryExecuteStep(
        Cell[,] field,
        Ant ant,
        Rule whiteRule,
        Rule blackRule,
        out string? errorMessage)
    {
        errorMessage = null;

        int width = field.GetLength(1);
        int height = field.GetLength(0);

        if (ant.X < 0 || ant.X >= width ||
            ant.Y < 0 || ant.Y >= height)
        {
            errorMessage = "Муравей находится за границами поля.";
            return false;
        }

        // Получаем клетку, на которой находится муравей.
        Cell currentCell = field[ant.Y, ant.X];

        // Выбираем правило в зависимости от цвета клетки.
        Rule rule = currentCell.IsBlack
            ? blackRule
            : whiteRule;

        // 1. Выполняем поворот.
        switch (rule.Turn)
        {
            case TurnAction.None:
                break;

            case TurnAction.Left:
                ant.TurnLeft();
                break;

            case TurnAction.Right:
                ant.TurnRight();
                break;

            case TurnAction.Around:
                ant.TurnAround();
                break;
        }

        // 2. Изменяем цвет текущей клетки.
        ApplyColorAction(currentCell, rule.Color);

        // 3. Проверяем необходимость движения.
        if (rule.Movement == MovementAction.Stay)
        {
            return true;
        }

        // 4. Вычисляем следующую позицию.
        (int nextX, int nextY) = GetNextPosition(ant);

        // 5. Проверяем границы поля.
        if (nextX < 0 || nextX >= width ||
            nextY < 0 || nextY >= height)
        {
            errorMessage = "Муравей достиг границы поля.";
            return false;
        }

        // 6. Перемещаем муравья.
        ant.MoveForward();

        return true;
    }

    private void ApplyColorAction(Cell cell, ColorAction action)
    {
        switch (action)
        {
            case ColorAction.Keep:
                break;

            case ColorAction.Black:
                cell.IsBlack = true;
                break;

            case ColorAction.White:
                cell.IsBlack = false;
                break;

            case ColorAction.Random:
                cell.IsBlack = _random.Next(2) == 1;
                break;
        }
    }

    private static (int X, int Y) GetNextPosition(Ant ant)
    {
        return ant.Direction switch
        {
            Direction.Up => (ant.X, ant.Y - 1),
            Direction.Right => (ant.X + 1, ant.Y),
            Direction.Down => (ant.X, ant.Y + 1),
            Direction.Left => (ant.X - 1, ant.Y),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
