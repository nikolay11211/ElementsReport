#nullable enable

namespace dLab.General.ElementsReport.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using dLab.General.ElementsReport.Abstractions;
using dLab.General.ElementsReport.Commands;
using dLab.General.ElementsReport.Models;


/// <summary>
/// Модель представления главного окна: строки ведомости, итог и сообщение об ошибке.
/// О Revit не знает, данные получает от <see cref="IElementsReportService"/>.
/// </summary>
public class MainWindowVm : ObservableBase
{
    /// <summary>
    /// Сервис построения ведомости.
    /// </summary>
    private readonly IElementsReportService _reportService;

    private readonly List<ElementRow> _allRows;

    /// <summary>
    /// Команда загрузки данных, вызывается при открытии окна.
    /// </summary>
    private ICommand? _initializeCommand;

    private ICommand? _filterCommand;

    public ICommand FilterCommand => _filterCommand ??= new RelayCommand(ApplyFilter, onError: HandleError);

    /// <summary>
    /// Creates the view model of the main window.
    /// </summary>
    /// <param name="reportService">Сервис построения ведомости.</param>
    public MainWindowVm(IElementsReportService reportService)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        Rows = new ObservableCollection<ElementRow>();
        _allRows = new List<ElementRow>();
        Categories = new ObservableCollection<CategoryOption>();

        Categories.Add(new CategoryOption(ReportCategory.Walls, Lang.ElementsReport.WallsCategory));
        Categories.Add(new CategoryOption(ReportCategory.Floors, Lang.ElementsReport.FloorsCategory));
        Categories.Add(new CategoryOption(ReportCategory.Roofs, Lang.ElementsReport.RoofsCategory));

        SelectedCategory = Categories[0];

    }
    public string MinimumAreaText { get; set; } = string.Empty;
    public ObservableCollection<CategoryOption> Categories { get; }
    public CategoryOption SelectedCategory { get; set; }
    /// <summary>
    /// Строки ведомости.
    /// </summary>
    public ObservableCollection<ElementRow> Rows { get; }

    /// <summary>
    /// Команда загрузки данных, привязана к открытию окна.
    /// </summary>
    public ICommand InitializeCommand =>
        _initializeCommand ??= new RelayCommand(Load, onError: HandleError);

    /// <summary>
    /// Суммарная площадь по строкам ведомости в квадратных метрах.
    /// </summary>
    public double TotalArea { get; private set; }

    /// <summary>
    /// Текст сообщения об ошибке. Пустая строка означает, что ошибки нет.
    /// </summary>
    public string ErrorMessage { get; private set; } = string.Empty;

    /// <summary>
    /// Загружает ведомость и обновляет содержимое окна.
    /// </summary>
    private void Load()
    {
        ErrorMessage = string.Empty;
        _allRows.Clear();

        var report =
            _reportService.BuildReport(SelectedCategory.Value);

        foreach (var row in report.Rows)
            _allRows.Add(row);

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Rows.Clear();

        double minimumArea = 0;

        if (!string.IsNullOrWhiteSpace(MinimumAreaText))
            double.TryParse(MinimumAreaText, out minimumArea);

        double totalArea = 0;

        foreach (var row in _allRows)
        {
            if (row.AreaSqM < minimumArea)
                continue;

            Rows.Add(row);
            totalArea += row.AreaSqM;
        }

        TotalArea = totalArea;
    }

    /// <summary>
    /// Показывает пользователю сообщение об ошибке, возникшей в команде.
    /// </summary>
    /// <param name="exception">Возникшее исключение.</param>
    private void HandleError(Exception exception) => ErrorMessage = exception.Message;
}
