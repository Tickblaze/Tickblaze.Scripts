namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Absolute Value [AB]
/// </summary>
public partial class AbsoluteValue : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Plot("Result")]
	public PlotSeries Result { get; set; }

	public AbsoluteValue()
	{
		Name = "Absolute Value";
		ShortName = "ABS";
	}

	protected override void Calculate(int index)
	{
		Result[index] = Math.Abs(Source[index]);
	}
}
