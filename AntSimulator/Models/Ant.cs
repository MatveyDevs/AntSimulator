
namespace AntSimulator.Models;

public class Ant
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public Direction Direction { get; private set; }

    public Ant(int x, int y, Direction direction = Direction.Up)
    {
        X = x;
        Y = y;
        Direction = direction;
    }

    public void TurnLeft()
    {
        Direction = (Direction)(((int)Direction + 3) % 4);
    }

    public void TurnRight()
    {
        Direction = (Direction)(((int)Direction + 1) % 4);
    }

    public void TurnAround()
    {
        Direction = (Direction)(((int)Direction + 2) % 4);
    }

    public void MoveForward()
    {
        switch (Direction)
        {
            case Direction.Up:
                Y--;
                break;

            case Direction.Right:
                X++;
                break;

            case Direction.Down:
                Y++;
                break;

            case Direction.Left:
                X--;
                break;
        }
    }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Reset(int x, int y, Direction direction = Direction.Up)
    {
        X = x;
        Y = y;
        Direction = direction;
    }
}
