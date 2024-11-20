namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Detrended Price Oscillator [DPO]
/// </summary>
public partial class DetrendedPriceOscillator : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	[Plot("Zero")]
	public PlotLevel ZeroLevel { get; set; } = new(0, "#000", LineStyle.Dash, 1);

	private SimpleMovingAverage _sma;
	private int _shiftPeriod;
	public DetrendedPriceOscillator()
	{
		Name = "Detrended Price Oscillator";
		ShortName = "DPO";
	}

	protected override void Initialize()
	{
		_shiftPeriod = (int)(Period / 2.0 + 1);
		_sma = new SimpleMovingAverage(Bars.Close, Period);
	}

	protected override void Calculate(int index)
	{
		Result[index] = index > _shiftPeriod
			? Source[index - _shiftPeriod] - _sma[index]
			: 0;
	}
}
