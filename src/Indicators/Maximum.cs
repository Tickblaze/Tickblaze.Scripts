namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Maximum [MAX]
/// </summary>
public partial class Maximum : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; }

	public Maximum()
	{
		Name = "Maximum";
		ShortName = "MAX";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var period = Math.Min(Period, index + 1);
		var maximum = double.MinValue;

		for (var i = 0; i < period; i++)
		{
			maximum = Math.Max(maximum, Source[index - i]);
		}

		Result[index] = maximum;
	}
}
