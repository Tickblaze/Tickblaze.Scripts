namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Rate Of Change [ROC]
/// </summary>
public partial class RateOfChange : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 20;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	public RateOfChange()
	{
		Name = "Rate Of Change";
		ShortName = "ROC";
		IsOverlay = false;
	}

	protected override void Calculate(int index)
	{
		Result[index] = index <= Period ? 0 : 100.0 * (Source[index] - Source[index - Period]) / Source[index - Period];
	}
}