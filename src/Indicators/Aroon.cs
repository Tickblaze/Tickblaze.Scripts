namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Aroon [ARN]
/// </summary>
public partial class Aroon : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Aroon Up")]
	public PlotSeries Up { get; set; } = new(Color.Orange, PlotStyle.Line);

	[Plot("Aroon Down")]
	public PlotSeries Down { get; set; } = new(Color.Blue, PlotStyle.Line);

	public Aroon()
	{
		Name = "Aroon";
		ShortName = "ARN";
		IsPercentage = true;
	}

	protected override void Calculate(int index)
	{
		if (index < Period)
		{
			return;
		}

		var currentHigh = Bars.High[index];
		var currentLow = Bars.Low[index];
		var barsSinceHigh = 0;
		var barsSinceLow = 0;

		for (var i = 0; i < Period; i++)
		{
			if (Bars.High[index - i] >= currentHigh)
			{
				currentHigh = Bars.High[index - i];
				barsSinceHigh = i;
			}

			if (Bars.Low[index - i] <= currentLow)
			{
				currentLow = Bars.Low[index - i];
				barsSinceLow = i;
			}
		}

		Up[index] = (Period - barsSinceHigh - 1) * 100.0 / (Period - 1);
		Down[index] = (Period - barsSinceLow - 1) * 100.0 / (Period - 1);
	}
}
