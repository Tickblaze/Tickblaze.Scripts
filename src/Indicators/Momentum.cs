namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Momentum [MOM]
/// </summary>
public partial class Momentum : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 20;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	public Momentum()
	{
		Name = "Momentum";
		ShortName = "MOM";
		IsOverlay = false;
	}

	protected override void Calculate(int index)
	{
		Result[index] = index > Period ? Source[index] - Source[index - Period] : 0;
	}
}