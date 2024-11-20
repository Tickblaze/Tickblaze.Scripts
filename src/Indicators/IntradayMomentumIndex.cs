namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Intraday Momentum Index [IMI]
/// </summary>
public partial class IntradayMomentuIndex : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	[Plot("Zero")]
	public PlotLevel ZeroLevel { get; set; } = new(0, "#000", LineStyle.Dash, 1);

	public IntradayMomentuIndex()
	{
		Name = "Intraday Momentum Index";
		ShortName = "IMI";
	}

	protected override void Calculate(int index)
	{
		if (index <= Period)
		{
			Result[index] = Bars[index].Close;
			return;
		}

		var sumUp = 0.0;
		var sumDown = 0.0;

		for (var i = Period - 1; i >= 0; i--)
		{
			var bar = Bars[index - i];
			if (bar.Close > bar.Open)
			{
				sumUp += bar.Close - bar.Open;
			}
			else
			{
				sumDown += bar.Open - bar.Close;
			}
		}

		Result[index] = 100 * sumUp / (sumUp + sumDown);
	}
}
