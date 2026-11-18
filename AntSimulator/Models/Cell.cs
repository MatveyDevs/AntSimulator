namespace AntSimulator.Models;

public class Cell
{
    public bool IsBlack { get; set; }

    public Cell(bool isBlack = false)
    {
        IsBlack = isBlack;
    }

    public void Toggle()
    {
        IsBlack = !IsBlack;
    }
}