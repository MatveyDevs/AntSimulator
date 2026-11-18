
namespace AntSimulator.Models;

public class Rule
{
    public TurnAction Turn { get; set; }

    public ColorAction Color { get; set; }

    public MovementAction Movement { get; set; }

    public Rule(
        TurnAction turn = TurnAction.None,
        ColorAction color = ColorAction.Keep,
        MovementAction movement = MovementAction.Forward)
    {
        Turn = turn;
        Color = color;
        Movement = movement;
    }
}
