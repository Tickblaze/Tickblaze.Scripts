namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Hull Moving Average [HMA]
/// </summary>
public partial class HullMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new("#2962ff", LineStyle.Solid, 1);

	private DataSeries _series;
	private WeightedMovingAverage _wma1, _wma2, _wma3;

	public HullMovingAverage()
	{
		Name = "Hull Moving Average";
		ShortName = "HMA";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_series = new DataSeries();
		_wma1 = new WeightedMovingAverage(Source, Period / 2);
		_wma2 = new WeightedMovingAverage(Source, Period);
		_wma3 = new WeightedMovingAverage(_series, (int)Math.Sqrt(Period));
	}

	protected override void Calculate(int index)
	{
		_series[index] = 2 * _wma1[index] - _wma2[index];
		Result[index] = _wma3[index];
	}
}
