using System.Globalization;
using System.Text;

namespace BookingDemo.Helpers;

public static class PieChartHelper
{
    public static string PieChartSvg(int approved, int ok, int inProgress, int notStarted, int total)
    {
        if (total == 0) return "";
        var colors = new[] { "#4caf50", "#ff9800", "#2196F3", "#e0e0e0" };
        var values = new[] { approved, ok, inProgress, notStarted };
        double cx = 32, cy = 32, r = 28;
        var sb = new StringBuilder();
        sb.Append("<svg width='64' height='64' viewBox='0 0 64 64' xmlns='http://www.w3.org/2000/svg'>");
        double startAngle = -90;
        for (int i = 0; i < 4; i++)
        {
            if (values[i] == 0) continue;
            double pct = (double)values[i] / total;
            double angle = pct * 360;
            double endAngle = startAngle + angle;
            double x1 = cx + r * Math.Cos(startAngle * Math.PI / 180);
            double y1 = cy + r * Math.Sin(startAngle * Math.PI / 180);
            double x2 = cx + r * Math.Cos(endAngle * Math.PI / 180);
            double y2 = cy + r * Math.Sin(endAngle * Math.PI / 180);
            int largeArc = angle > 180 ? 1 : 0;
            if (pct >= 1.0)
                sb.Append($"<circle cx='{cx}' cy='{cy}' r='{r}' fill='{colors[i]}'/>");
            else
                sb.Append($"<path d='M{cx},{cy} L{x1.ToString(CultureInfo.InvariantCulture)},{y1.ToString(CultureInfo.InvariantCulture)} A{r},{r} 0 {largeArc},1 {x2.ToString(CultureInfo.InvariantCulture)},{y2.ToString(CultureInfo.InvariantCulture)} Z' fill='{colors[i]}'/>");
            startAngle = endAngle;
        }
        sb.Append("<circle cx='32' cy='32' r='16' fill='white'/>");
        sb.Append($"<text x='32' y='36' text-anchor='middle' font-size='12' fill='#424242'>{approved}/{total}</text>");
        sb.Append("</svg>");
        return sb.ToString();
    }
}
