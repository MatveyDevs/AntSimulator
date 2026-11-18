
using System.Windows;
using AntSimulator.ViewModels;

namespace AntSimulator;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();

        DataContext = _viewModel;

        FieldView.CellClicked += OnCellClicked;
    }

    private void OnCellClicked(int x, int y)
    {
        _viewModel.ToggleCell(x, y);
    }
}
