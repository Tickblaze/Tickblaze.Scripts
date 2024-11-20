namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// AccumulationDistributionLine [AD]
/// </summary>
public partial class AccumulationDistributionLine : Indicator
{
	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	public AccumulationDistributionLine()
	{
		Name = "Accumulation Distribution Line";
		ShortName = "AD";
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];
		var previousResult = index > 0 ? Result[index - 1] : 0;

		Result[index] = previousResult + (bar.High > bar.Low ? (2 * bar.Close - bar.Low - bar.High) / (bar.High - bar.Low) * bar.Volume : 0);
	}
}
