namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Price Channel [PC]
/// </summary>
public partial class PriceChannel : Indicator
{
	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 20;

	[Plot("Main")]
	public PlotSeries Main { get; set; } = new(Color.Gray, LineStyle.Dash);

	[Plot("Upper")]
	public PlotSeries Upper { get; set; } = new(Color.Blue, LineStyle.Solid);

	[Plot("Lower")]
	public PlotSeries Lower { get; set; } = new(Color.Blue, LineStyle.Solid);

	public PriceChannel()
	{
		Name = "Price Channel";
		ShortName = "PC";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var highestHigh = double.MinValue;
		var lowestLow = double.MaxValue;

		for (var i = Math.Min(index, Period); i >= 0; i--)
		{
			highestHigh = Math.Max(highestHigh, Bars[index - i].High);
			lowestLow = Math.Min(lowestLow, Bars[index - i].Low);
		}

		Upper[index] = highestHigh;
		Lower[index] = lowestLow;
		Main[index] = (highestHigh + lowestLow) / 2.0;
	}
}
