namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Projection Band [PB]
/// </summary>
public partial class ProjectionBand : Indicator
{
	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 20;

	[Plot("Main")]
	public PlotSeries Main { get; set; } = new(Color.Gray, LineStyle.Dash);

	[Plot("Upper")]
	public PlotSeries Upper { get; set; } = new(Color.Blue, LineStyle.Solid);

	[Plot("Lower")]
	public PlotSeries Lower { get; set; } = new(Color.Blue, LineStyle.Solid);

	public ProjectionBand()
	{
		Name = "Projection Band";
		ShortName = "PB";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		if (index < Period)
		{
			Upper[index] = Bars[index].High;
			Lower[index] = Bars[index].Low;
			Main[index] = Bars[index].Close;
			return;
		}

		var sumXY = 0.0;
		var sumX = 0.0;
		var sumY = 0.0;
		var sumXPower = 0;

		for (var i = Period - 1; i >= 0; i--)
		{
			sumXY += i * Bars[index - i].High;
			sumX += i;
			sumY += Bars[index - i].High;
			sumXPower += i * i;
		}

		var slope = -1 * ((Period * sumXY) - (sumX * sumY)) / (Period * sumXPower - (sumX * sumX));
		var value = 0.0;

		for (var i = Period - 1; i >= 0; i--)
		{
			value = Math.Max(value, Bars[index - i].High + slope * i);
		}

		Upper[index] = value;

		sumXY = 0;
		sumX = 0;
		sumY = 0;
		sumXPower = 0;

		for (var i = Period - 1; i >= 0; i--)
		{
			sumXY += i * Bars[index - i].Low;
			sumX += i;
			sumY += Bars[index - i].Low;
			sumXPower += i * i;
		}

		slope = -1 * ((Period * sumXY) - (sumX * sumY)) / (Period * sumXPower - (sumX * sumX));
		value = double.MaxValue;

		for (var i = Period - 1; i >= 0; i--)
		{
			value = Math.Min(value, Bars[index - i].Low + slope * i);
		}

		Lower[index] = value;

		Main[index] = (Upper[index] + Lower[index]) / 2.0;
	}
}