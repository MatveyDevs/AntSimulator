using System.Windows.Threading;
using AntSimulator.Models;
using AntSimulator.Services;

namespace AntSimulator.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly FieldService _fieldService = new();
    private readonly SimulationService _simulationService = new();
    private readonly DispatcherTimer _timer;

    private Cell[,] _field;
    private Ant _ant;

    private int _fieldWidth = 30;
    private int _fieldHeight = 20;

    private int _startX = 15;
    private int _startY = 10;
    private Direction _startDirection = Direction.Up;

    private int _steps;
    private int _speedMilliseconds = 200;
    private int _blackProbability = 30;
    private int _renderVersion;

    private bool _isRunning;
    private string _statusMessage = "Готов к работе.";

    private TurnAction _whiteTurn = TurnAction.Left;
    private ColorAction _whiteColor = ColorAction.Black;
    private MovementAction _whiteMovement = MovementAction.Forward;

    private TurnAction _blackTurn = TurnAction.Right;
    private ColorAction _blackColor = ColorAction.White;
    private MovementAction _blackMovement = MovementAction.Forward;

    public MainViewModel()
    {
        _field = _fieldService.CreateField(_fieldWidth, _fieldHeight);
        _ant = new Ant(_startX, _startY, _startDirection);

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(_speedMilliseconds)
        };

        _timer.Tick += Timer_Tick;

        StartCommand = new RelayCommand(StartSimulation, () => !IsRunning);
        PauseCommand = new RelayCommand(PauseSimulation, () => IsRunning);
        StopCommand = new RelayCommand(StopSimulation, () => IsRunning);
        StepCommand = new RelayCommand(ExecuteSingleStep, () => !IsRunning);

        ResetCommand = new RelayCommand(ResetSimulation);
        CreateFieldCommand = new RelayCommand(CreateNewField);
        ClearFieldCommand = new RelayCommand(ClearField);
        RandomizeFieldCommand = new RelayCommand(RandomizeField);
    }

    // ---------------------------------------------------------
    // Команды
    // ---------------------------------------------------------

    public RelayCommand StartCommand { get; }
    public RelayCommand PauseCommand { get; }
    public RelayCommand StopCommand { get; }
    public RelayCommand StepCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand CreateFieldCommand { get; }
    public RelayCommand ClearFieldCommand { get; }
    public RelayCommand RandomizeFieldCommand { get; }

    // ---------------------------------------------------------
    // Игровое поле
    // ---------------------------------------------------------

    public Cell[,] Field
    {
        get => _field;
        private set => SetProperty(ref _field, value);
    }

    public int FieldWidth
    {
        get => _fieldWidth;
        set => SetProperty(ref _fieldWidth, value);
    }

    public int FieldHeight
    {
        get => _fieldHeight;
        set => SetProperty(ref _fieldHeight, value);
    }

    // Изменение этого свойства заставляет FieldControl
    // перерисовать игровое поле.
    public int RenderVersion
    {
        get => _renderVersion;
        private set => SetProperty(ref _renderVersion, value);
    }

    // ---------------------------------------------------------
    // Муравей
    // ---------------------------------------------------------

    public int AntX => _ant.X;
    public int AntY => _ant.Y;
    public Direction AntDirection => _ant.Direction;

    public int StartX
    {
        get => _startX;
        set => SetProperty(ref _startX, value);
    }

    public int StartY
    {
        get => _startY;
        set => SetProperty(ref _startY, value);
    }

    public Direction StartDirection
    {
        get => _startDirection;
        set => SetProperty(ref _startDirection, value);
    }

    public Array DirectionOptions => Enum.GetValues(typeof(Direction));

    // ---------------------------------------------------------
    // Правила поведения
    // ---------------------------------------------------------

    public Array TurnOptions => Enum.GetValues(typeof(TurnAction));

    public Array ColorOptions => Enum.GetValues(typeof(ColorAction));
    public Array MovementOptions =>
    Enum.GetValues(typeof(MovementAction));

    public TurnAction WhiteTurn
    {
        get => _whiteTurn;
        set => SetProperty(ref _whiteTurn, value);
    }

    public ColorAction WhiteColor
    {
        get => _whiteColor;
        set => SetProperty(ref _whiteColor, value);
    }

    public TurnAction BlackTurn
    {
        get => _blackTurn;
        set => SetProperty(ref _blackTurn, value);
    }

    public ColorAction BlackColor
    {
        get => _blackColor;
        set => SetProperty(ref _blackColor, value);
    }
    public MovementAction WhiteMovement
    {
        get => _whiteMovement;
        set => SetProperty(ref _whiteMovement, value);
    }

    public MovementAction BlackMovement
    {
        get => _blackMovement;
        set => SetProperty(ref _blackMovement, value);
    }

    private Rule GetWhiteRule()
    {
        return new Rule(
            WhiteTurn,
            WhiteColor,
            WhiteMovement);
    }

    private Rule GetBlackRule()
    {
        return new Rule(
            BlackTurn,
            BlackColor,
            BlackMovement);
    }

    // ---------------------------------------------------------
    // Состояние симуляции
    // ---------------------------------------------------------

    public int Steps
    {
        get => _steps;
        private set => SetProperty(ref _steps, value);
    }

    public int SpeedMilliseconds
    {
        get => _speedMilliseconds;
        set
        {
            if (SetProperty(ref _speedMilliseconds, value))
            {
                _timer.Interval =
                    TimeSpan.FromMilliseconds(_speedMilliseconds);
            }
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetProperty(ref _isRunning, value))
            {
                StartCommand.RaiseCanExecuteChanged();
                PauseCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
                StepCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    // ---------------------------------------------------------
    // Случайная раскраска
    // ---------------------------------------------------------

    public int BlackProbability
    {
        get => _blackProbability;
        set => SetProperty(ref _blackProbability, value);
    }

    // ---------------------------------------------------------
    // Управление симуляцией
    // ---------------------------------------------------------

    private void StartSimulation()
    {
        if (_ant.X < 0 || _ant.X >= Field.GetLength(1) ||
            _ant.Y < 0 || _ant.Y >= Field.GetLength(0))
        {
            StatusMessage = "Начальная позиция муравья вне поля.";
            return;
        }

        _timer.Start();
        IsRunning = true;
        StatusMessage = "Симуляция выполняется.";
    }

    private void PauseSimulation()
    {
        _timer.Stop();
        IsRunning = false;
        StatusMessage = "Симуляция приостановлена.";
    }

    private void StopSimulation()
    {
        _timer.Stop();
        IsRunning = false;
        StatusMessage = "Симуляция остановлена. Позиция и поле сохранены.";
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        ExecuteSingleStepInternal();
    }

    private void ExecuteSingleStep()
    {
        ExecuteSingleStepInternal();
    }

    private void ExecuteSingleStepInternal()
    {
        bool moved = _simulationService.TryExecuteStep(
            Field,
            _ant,
            GetWhiteRule(),
            GetBlackRule(),
            out string? errorMessage);

        if (moved)
        {
            Steps++;
            StatusMessage = "Симуляция выполняется.";
        }
        else
        {
            _timer.Stop();
            IsRunning = false;

            StatusMessage = errorMessage ?? "Не удалось выполнить шаг.";
        }

        RefreshField();
    }

    // ---------------------------------------------------------
    // Создание и изменение поля
    // ---------------------------------------------------------

    private void CreateNewField()
    {
        _timer.Stop();
        IsRunning = false;

        try
        {
            Field = _fieldService.CreateField(FieldWidth, FieldHeight);

            StartX = FieldWidth / 2;
            StartY = FieldHeight / 2;

            _ant.Reset(StartX, StartY, StartDirection);

            Steps = 0;
            StatusMessage = "Новое поле создано.";

            RefreshField();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private void ClearField()
    {
        _timer.Stop();
        IsRunning = false;

        _fieldService.ClearField(Field);

        Steps = 0;
        StatusMessage = "Поле очищено.";

        RefreshField();
    }

    private void RandomizeField()
    {
        _timer.Stop();
        IsRunning = false;

        if (BlackProbability is < 0 or > 100)
        {
            StatusMessage = "Вероятность чёрных клеток должна быть от 0 до 100.";
            return;
        }

        _fieldService.RandomizeField(
            Field,
            BlackProbability / 100.0);

        Steps = 0;
        StatusMessage = "Поле случайно раскрашено.";

        RefreshField();
    }

    public void ToggleCell(int x, int y)
    {
        if (IsRunning)
            return;

        _fieldService.ToggleCell(Field, x, y);
        RefreshField();
    }

    // ---------------------------------------------------------
    // Сброс симуляции
    // ---------------------------------------------------------

    private void ResetSimulation()
    {
        _timer.Stop();
        IsRunning = false;

        _fieldService.ClearField(Field);

        int x = StartX;
        int y = StartY;

        if (!_fieldService.IsInside(Field, x, y))
        {
            x = Field.GetLength(1) / 2;
            y = Field.GetLength(0) / 2;

            StartX = x;
            StartY = y;
        }

        _ant.Reset(x, y, StartDirection);

        Steps = 0;
        StatusMessage = "Симуляция сброшена.";

        RefreshField();
    }

    // ---------------------------------------------------------
    // Обновление отображения
    // ---------------------------------------------------------

    private void RefreshField()
    {
        OnPropertyChanged(nameof(AntX));
        OnPropertyChanged(nameof(AntY));
        OnPropertyChanged(nameof(AntDirection));

        RenderVersion++;
    }
}