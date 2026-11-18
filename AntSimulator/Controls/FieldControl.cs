
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AntSimulator.Models;

namespace AntSimulator.Controls;

public class FieldControl : FrameworkElement
{
    public static readonly DependencyProperty FieldProperty =
        DependencyProperty.Register(
            nameof(Field),
            typeof(Cell[,]),
            typeof(FieldControl),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty AntXProperty =
        DependencyProperty.Register(
            nameof(AntX),
            typeof(int),
            typeof(FieldControl),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty AntYProperty =
        DependencyProperty.Register(
            nameof(AntY),
            typeof(int),
            typeof(FieldControl),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty AntDirectionProperty =
        DependencyProperty.Register(
            nameof(AntDirection),
            typeof(Direction),
            typeof(FieldControl),
            new FrameworkPropertyMetadata(
                Direction.Up,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RenderVersionProperty =
        DependencyProperty.Register(
            nameof(RenderVersion),
            typeof(int),
            typeof(FieldControl),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public Cell[,]? Field
    {
        get => (Cell[,]?)GetValue(FieldProperty);
        set => SetValue(FieldProperty, value);
    }

    public int AntX
    {
        get => (int)GetValue(AntXProperty);
        set => SetValue(AntXProperty, value);
    }

    public int AntY
    {
        get => (int)GetValue(AntYProperty);
        set => SetValue(AntYProperty, value);
    }

    public Direction AntDirection
    {
        get => (Direction)GetValue(AntDirectionProperty);
        set => SetValue(AntDirectionProperty, value);
    }

    public int RenderVersion
    {
        get => (int)GetValue(RenderVersionProperty);
        set => SetValue(RenderVersionProperty, value);
    }

    public event Action<int, int>? CellClicked;

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        Cell[,]? field = Field;

        if (field == null)
            return;

        int rows = field.GetLength(0);
        int columns = field.GetLength(1);

        if (rows == 0 || columns == 0)
            return;

        double cellWidth = ActualWidth / columns;
        double cellHeight = ActualHeight / rows;

        if (cellWidth <= 0 || cellHeight <= 0)
            return;

        drawingContext.DrawRectangle(
            Brushes.White,
            null,
            new Rect(0, 0, ActualWidth, ActualHeight));

        // Отрисовка клеток.
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Brush brush = field[y, x].IsBlack
                    ? Brushes.Black
                    : Brushes.White;

                drawingContext.DrawRectangle(
                    brush,
                    null,
                    new Rect(
                        x * cellWidth,
                        y * cellHeight,
                        cellWidth,
                        cellHeight));
            }
        }

        // Отрисовка сетки.
        Pen gridPen = new(
            new SolidColorBrush(Color.FromRgb(190, 190, 190)),
            0.5);

        for (int x = 0; x <= columns; x++)
        {
            double positionX = x * cellWidth;

            drawingContext.DrawLine(
                gridPen,
                new Point(positionX, 0),
                new Point(positionX, ActualHeight));
        }

        for (int y = 0; y <= rows; y++)
        {
            double positionY = y * cellHeight;

            drawingContext.DrawLine(
                gridPen,
                new Point(0, positionY),
                new Point(ActualWidth, positionY));
        }

        // Отрисовка муравья.
        if (AntX >= 0 && AntX < columns &&
            AntY >= 0 && AntY < rows)
        {
            DrawAnt(
                drawingContext,
                AntX * cellWidth,
                AntY * cellHeight,
                cellWidth,
                cellHeight,
                AntDirection);
        }
    }

    private static void DrawAnt(
        DrawingContext drawingContext,
        double x,
        double y,
        double width,
        double height,
        Direction direction)
    {
        Point center = new(
            x + width / 2,
            y + height / 2);

        double size = Math.Min(width, height) * 0.4;

        Point[] points =
        [
            new Point(center.X, center.Y - size),
            new Point(center.X - size * 0.7, center.Y + size * 0.7),
            new Point(center.X + size * 0.7, center.Y + size * 0.7)
        ];

        double angle = direction switch
        {
            Direction.Up => 0,
            Direction.Right => 90,
            Direction.Down => 180,
            Direction.Left => 270,
            _ => 0
        };

        StreamGeometry geometry = new();

        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(points[0], true, true);
            context.PolyLineTo(points.Skip(1).ToArray(), true, true);
        }

        geometry.Freeze();

        drawingContext.PushTransform(
            new RotateTransform(angle, center.X, center.Y));

        drawingContext.DrawGeometry(
            Brushes.Red,
            new Pen(Brushes.DarkRed, 1),
            geometry);

        drawingContext.Pop();
    }

    protected override void OnMouseLeftButtonDown(
        MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        Cell[,]? field = Field;

        if (field == null ||
            ActualWidth <= 0 ||
            ActualHeight <= 0)
        {
            return;
        }

        Point position = e.GetPosition(this);

        int columns = field.GetLength(1);
        int rows = field.GetLength(0);

        int x = (int)(position.X / (ActualWidth / columns));
        int y = (int)(position.Y / (ActualHeight / rows));

        if (x >= 0 && x < columns &&
            y >= 0 && y < rows)
        {
            CellClicked?.Invoke(x, y);
        }

        e.Handled = true;
    }
}
