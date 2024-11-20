namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Adaptive Price Zone [APZ]
/// </summary>
public partial class AdaptivePriceZone : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 20;

	[Parameter("Band %"), NumericRange(0, int.MaxValue, 0.01)]
	public double BandPct { get; set; } = 2;

	[Plot("Upper")]
	public PlotSeries Upper { get; set; } = new(Color.Blue, LineStyle.Solid, 1);

	[Plot("Lower")]
	public PlotSeries Lower { get; set; } = new(Color.Blue, LineStyle.Solid, 1);

	private DataSeries _range;
	private DoubleExponentialMovingAverage _dema;
	private ExponentialMovingAverage _emaRange;

	public AdaptivePriceZone()
	{
		Name = "Adaptive Price Zone";
		ShortName = "APZ";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_range = new DataSeries();
		_dema = new DoubleExponentialMovingAverage(Source, (int)Math.Sqrt(Period));
		_emaRange = new ExponentialMovingAverage(_range, Period);
	}

	protected override void Calculate(int index)
	{
		_range[index] = Bars[index].High - Bars[index].Low;

		var rangeOffset = BandPct * _emaRange[index];

		Lower[index] = _dema[index] - rangeOffset;
		Upper[index] = _dema[index] + rangeOffset;
	}
}
