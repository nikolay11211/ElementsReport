#nullable enable

namespace dLab.General.ElementsReport.Services;

using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using dLab.General.ElementsReport.Abstractions;
using dLab.General.ElementsReport.Helpers;
using dLab.General.ElementsReport.Models;

/// <inheritdoc />
public class ElementsReportService : IElementsReportService
{
    /// <summary>
    /// Категория элементов, попадающих в ведомость.
    /// </summary>
    

    /// <summary>
    /// Документ, по которому строится ведомость.
    /// </summary>
    private readonly Document _document;

    /// <summary>
    /// Creates the report building service.
    /// </summary>
    /// <param name="document">Документ открытой модели.</param>
    public ElementsReportService(Document document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <inheritdoc />
    public ElementsReportResult BuildReport(
    ReportCategory category)
    {
        var rows = new List<ElementRow>();
        double totalArea = 0;

        foreach (var element in CollectElements(category))
        {
            var levelName = GetLevelName(element);
            var area = GetArea(element);

            if (area <= 0)
                continue;

            var areaSqM = UnitHelper.ToSquareMeters(area);

            rows.Add(new ElementRow(element.Name, areaSqM, levelName));

            totalArea += areaSqM;
        }

        return new ElementsReportResult(rows, totalArea);
    }

    /// <summary>
    /// Возвращает элементы модели, попадающие в ведомость.
    /// </summary>
    /// <returns>Экземпляры элементов заданной категории.</returns>
    private IEnumerable<Element> CollectElements(
    ReportCategory category)
    {
        BuiltInCategory builtInCategory =
            GetBuiltInCategory(category);

        return new FilteredElementCollector(_document)
            .OfCategory(builtInCategory)
            .WhereElementIsNotElementType()
            .ToElements();
    }

    private BuiltInCategory GetBuiltInCategory(
    ReportCategory category)
    {
        switch (category)
        {
            case ReportCategory.Walls:
                return BuiltInCategory.OST_Walls;

            case ReportCategory.Floors:
                return BuiltInCategory.OST_Floors;

            case ReportCategory.Roofs:
                return BuiltInCategory.OST_Roofs;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(category));
        }
    }

    /// <summary>
    /// Возвращает площадь элемента во внутренних единицах Revit.
    /// </summary>
    /// <param name="element">Элемент модели.</param>
    /// <returns>Площадь во внутренних единицах или ноль, если параметр не заполнен.</returns>
    private double GetArea(Element element)
    {
        var parameter = element.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
        if (parameter is null || !parameter.HasValue)
            return 0;

        return parameter.AsDouble();
    }


    private string GetLevelName(Element element)
    {
        if (element.LevelId == ElementId.InvalidElementId)
            return Lang.ElementsReport.NoLevelValue;

        var level = _document.GetElement(element.LevelId) as Level;

        if (level is null)
            return Lang.ElementsReport.NoLevelValue;

        return level.Name;
    }
}
