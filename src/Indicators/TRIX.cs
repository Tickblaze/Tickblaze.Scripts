namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// TRIX [TRIX]
/// </summary>
public partial class TRIX : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Signal Period"), NumericRange(1, int.MaxValue)]
	public int SignalPeriod { get; set; } = 3;

	[Plot("Signal")]
	public PlotSeries Signal { get; set; } = new(Color.Yellow);

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	[Plot("Zero")]
	public PlotLevel Zero { get; set; } = new(0, "#787b86", LineStyle.Dash, 1);

	private ExponentialMovingAverage _ema1, _ema2, _ema3, _emaSignal;

	public TRIX()
	{
		Name = "TRIX";
		ShortName = "TRIX";
		IsOverlay = false;
	}

	protected override void Initialize()
	{
		_ema1 = new ExponentialMovingAverage(Bars.Close, Period);
		_ema2 = new ExponentialMovingAverage(_ema1.Result, Period);
		_ema3 = new ExponentialMovingAverage(_ema2.Result, Period);
		_emaSignal = new ExponentialMovingAverage(Result, SignalPeriod);
	}

	protected override void Calculate(int index)
	{
		if (index <= Period)
		{
			Result[index] = 0;
			Signal[index] = 0;
		}
		else
		{
			Result[index] = 100.0 * ((_ema3[index] - _ema3[index - 1]) / _ema3[index]);
			Signal[index] = _emaSignal[index];
		}
	}
}
