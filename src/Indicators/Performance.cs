namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Performance [PER]
/// </summary>
public partial class Performance : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 20;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	public Performance()
	{
		Name = "Performance";
		ShortName = "PER";
		IsOverlay = false;
	}

	protected override void Calculate(int index)
	{
		Result[index] = index <= Period ? 0 : 100.0 * (Source[index] - Source[index - Period]) / Source[index - Period];
	}
}