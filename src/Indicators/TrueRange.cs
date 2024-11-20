namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// True Range [TR]
/// </summary>
public partial class TrueRange : Indicator
{
	[Plot("Result")]
	public PlotSeries Result { get; set; }

	public TrueRange()
	{
		Name = "True Range";
		ShortName = "TR";
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];

		if (index == 0)
		{
			Result[index] = bar.High - bar.Low;
		}
		else
		{
			var previousClose = Bars[index - 1].Close;

			Result[index] = Math.Max(bar.High, previousClose) - Math.Min(bar.Low, previousClose);
		}
	}
}
