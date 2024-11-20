namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Commodity Channel Index [CCI]
/// </summary>
public partial class CommodityChannelIndex : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 20;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	[Plot("Overbought")]
	public PlotLevel OverboughtLevel { get; set; } = new(100, Color.Red, LineStyle.Dash, 1);

	[Plot("Zero")]
	public PlotLevel ZeroLevel { get; set; } = new(0, Color.Gray, LineStyle.Dash, 1);

	[Plot("Oversold")]
	public PlotLevel OversoldLevel { get; set; } = new(-100, Color.Green, LineStyle.Dash, 1);

	private SimpleMovingAverage _simpleMovingAverage;

	public CommodityChannelIndex()
	{
		Name = "Commodity Channel Index";
		ShortName = "CCI";
	}

	protected override void Initialize()
	{
		_simpleMovingAverage = new SimpleMovingAverage(Bars.TypicalPrice, Period);
	}

	protected override void Calculate(int index)
	{
		var sum = 0.0;
		var sma = _simpleMovingAverage[index];

		for (var i = Math.Max(0, index - Period); i < index; i++)
		{
			sum += Math.Abs(Bars.TypicalPrice[i] - sma);
		}

		Result[index] = (Bars.TypicalPrice[index] - sma) / (sum / Period * 0.015);
	}
}
