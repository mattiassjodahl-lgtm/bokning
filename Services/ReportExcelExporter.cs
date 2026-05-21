using BookingDemo.Models;
using ClosedXML.Excel;

namespace BookingDemo.Services;

/// <summary>
/// Konverterar en <see cref="AgentReport"/> till en .xlsx-fil.
///
/// Struktur:
///   • Flik "Översikt": titel, sammanfattning + samlade nyckeltal (KeyFigures-block).
///   • En flik per icke-KeyFigure-block (Tabell / BarChart / LineChart).
///     – Tabeller: kolumnrubriker + rader.
///     – BarChart / LineChart: kategorier + en kolumn per serie, plus en Excel-graf.
///
/// Designat lättviktigt — ingen extra stylebibliotek, inga externa beroenden
/// utöver ClosedXML (MIT-licens).
/// </summary>
public class ReportExcelExporter
{
    public byte[] Export(AgentReport report)
    {
        using var wb = new XLWorkbook();

        // ── Flik 1: Översikt ──────────────────────────────────────────────
        var overview = wb.Worksheets.Add("Översikt");
        BuildOverviewSheet(overview, report);

        // ── Flik 2..N: En per block (utom KeyFigures, som ligger i Översikt) ─
        int sheetIndex = 1;
        foreach (var block in report.Blocks)
        {
            if (block.Kind == BlockKind.KeyFigures) continue;

            var name = SanitizeSheetName(block.Heading, sheetIndex);
            var ws = wb.Worksheets.Add(name);

            switch (block.Kind)
            {
                case BlockKind.Table:
                    BuildTableSheet(ws, block);
                    break;
                case BlockKind.BarChart:
                    BuildChartSheet(ws, block, XLChartType.Column);
                    break;
                case BlockKind.LineChart:
                    BuildChartSheet(ws, block, XLChartType.Line);
                    break;
            }

            sheetIndex++;
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Översikt: titel, sammanfattning, alla nyckeltal ───────────────────
    private static void BuildOverviewSheet(IXLWorksheet ws, AgentReport report)
    {
        int row = 1;

        // Titel
        var titleCell = ws.Cell(row, 1);
        titleCell.Value = string.IsNullOrWhiteSpace(report.Title) ? "Rapport" : report.Title;
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 16;
        ws.Range(row, 1, row, 4).Merge();
        row += 2;

        // Sammanfattning
        if (!string.IsNullOrWhiteSpace(report.Summary))
        {
            var summaryCell = ws.Cell(row, 1);
            summaryCell.Value = report.Summary;
            summaryCell.Style.Alignment.WrapText = true;
            ws.Range(row, 1, row, 4).Merge();
            ws.Row(row).Height = 40;
            row += 2;
        }

        // Alla KeyFigures-block samlat
        var keyFigureBlocks = report.Blocks.Where(b => b.Kind == BlockKind.KeyFigures).ToList();
        foreach (var block in keyFigureBlocks)
        {
            if (!string.IsNullOrWhiteSpace(block.Heading))
            {
                var headingCell = ws.Cell(row, 1);
                headingCell.Value = block.Heading;
                headingCell.Style.Font.Bold = true;
                headingCell.Style.Font.FontSize = 12;
                row++;
            }

            // Header-rad
            ws.Cell(row, 1).Value = "Nyckeltal";
            ws.Cell(row, 2).Value = "Värde";
            ws.Cell(row, 3).Value = "Trend";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            row++;

            foreach (var kf in block.Figures)
            {
                ws.Cell(row, 1).Value = kf.Label;
                ws.Cell(row, 2).Value = kf.Value;
                ws.Cell(row, 3).Value = kf.Trend ?? "";
                row++;
            }

            row++; // blank rad mellan block
        }

        ws.Columns().AdjustToContents();
        // Sätt en minsta bredd så det ser snyggt ut även med korta värden
        if (ws.Column(1).Width < 20) ws.Column(1).Width = 20;
        if (ws.Column(2).Width < 15) ws.Column(2).Width = 15;
    }

    // ── Tabell-flik ───────────────────────────────────────────────────────
    private static void BuildTableSheet(IXLWorksheet ws, ReportBlock block)
    {
        int row = 1;

        if (!string.IsNullOrWhiteSpace(block.Heading))
        {
            var h = ws.Cell(row, 1);
            h.Value = block.Heading;
            h.Style.Font.Bold = true;
            h.Style.Font.FontSize = 14;
            if (block.Columns.Count > 1)
                ws.Range(row, 1, row, block.Columns.Count).Merge();
            row += 2;
        }

        // Kolumnrubriker
        for (int c = 0; c < block.Columns.Count; c++)
        {
            var cell = ws.Cell(row, c + 1);
            cell.Value = block.Columns[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        row++;

        // Rader
        foreach (var dataRow in block.Rows)
        {
            for (int c = 0; c < dataRow.Count; c++)
            {
                ws.Cell(row, c + 1).Value = dataRow[c];
            }
            row++;
        }

        ws.Columns().AdjustToContents();
    }

    // ── Chart-flik (data + Excel-graf) ────────────────────────────────────
    private static void BuildChartSheet(IXLWorksheet ws, ReportBlock block, XLChartType chartType)
    {
        int row = 1;

        if (!string.IsNullOrWhiteSpace(block.Heading))
        {
            var h = ws.Cell(row, 1);
            h.Value = block.Heading;
            h.Style.Font.Bold = true;
            h.Style.Font.FontSize = 14;
            row += 2;
        }

        // Header-rad: "Kategori" + en kolumn per serie
        int headerRow = row;
        ws.Cell(row, 1).Value = "Kategori";
        for (int s = 0; s < block.Series.Count; s++)
        {
            ws.Cell(row, s + 2).Value = block.Series[s].Name;
        }
        ws.Range(row, 1, row, block.Series.Count + 1).Style.Font.Bold = true;
        ws.Range(row, 1, row, block.Series.Count + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;

        int dataStartRow = row;
        // Datarader
        for (int i = 0; i < block.Categories.Count; i++)
        {
            ws.Cell(row, 1).Value = block.Categories[i];
            for (int s = 0; s < block.Series.Count; s++)
            {
                var values = block.Series[s].Values;
                if (i < values.Count)
                    ws.Cell(row, s + 2).Value = values[i];
            }
            row++;
        }
        int dataEndRow = row - 1;

        ws.Columns().AdjustToContents();

        // Excel-graf (ClosedXML stödjer detta via OpenXML — enkla chart-typer fungerar bra).
        // Notera: vi använder Try/Catch eftersom chart-stöd i ClosedXML har varit lite ostadigt
        // historiskt. Datan finns kvar på fliken oavsett.
        try
        {
            if (dataEndRow >= dataStartRow && block.Series.Count > 0)
            {
                int chartTopRow = headerRow;
                int chartLeftCol = block.Series.Count + 3;

                var chart = ws.AddChart(chartType,
                    chartTopRow, chartLeftCol,
                    chartTopRow + 18, chartLeftCol + 9);

                chart.SetTitle(string.IsNullOrWhiteSpace(block.Heading) ? "Diagram" : block.Heading);

                var categoriesRange = ws.Range(dataStartRow, 1, dataEndRow, 1);
                for (int s = 0; s < block.Series.Count; s++)
                {
                    var valuesRange = ws.Range(dataStartRow, s + 2, dataEndRow, s + 2);
                    var series = chart.AddSeries(valuesRange);
                    series.SetName(block.Series[s].Name);
                    series.SetCategories(categoriesRange);
                }
            }
        }
        catch
        {
            // Datan finns ändå tillgänglig i tabellform — användaren kan skapa
            // egen graf manuellt om något går snett med chart-renderingen.
        }
    }

    // ── Hjälpare: Excel-flikar har begränsningar på namn ──────────────────
    private static readonly char[] InvalidSheetChars = { '\\', '/', '?', '*', '[', ']', ':' };

    private static string SanitizeSheetName(string heading, int fallbackIndex)
    {
        if (string.IsNullOrWhiteSpace(heading))
            return $"Block {fallbackIndex}";

        var clean = heading;
        foreach (var c in InvalidSheetChars)
            clean = clean.Replace(c, ' ');

        clean = clean.Trim();
        if (clean.Length > 31)
            clean = clean.Substring(0, 31);

        return string.IsNullOrWhiteSpace(clean) ? $"Block {fallbackIndex}" : clean;
    }
}
