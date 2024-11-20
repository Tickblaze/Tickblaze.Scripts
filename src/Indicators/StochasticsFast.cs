namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Stochastics Fast [StoF]
/// </summary>
public partial class StochasticsFast : Indicator
{
	[Parameter("%K Periods"), NumericRange(1, int.MaxValue)]
	public int KPeriods { get; set; } = 14;

	[Parameter("%D Periods"), NumericRange(1, int.MaxValue)]
	public int DPeriods { get; set; } = 3;

	[Parameter("MA Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	[Plot("%D")]
	public PlotSeries PercentD { get; set; } = new(Color.Red, LineStyle.Dash);

	[Plot("%K")]
	public PlotSeries PercentK { get; set; } = new(Color.Green);

	private Maximum _maximum;
	private Minimum _minimum;
	private MovingAverage _percentD;

	public StochasticsFast()
	{
		Name = "Stochastics Fast";
		ShortName = "StoF";
	}

	protected override void Initialize()
	{
		_maximum = new Maximum(Bars.High, KPeriods);
		_minimum = new Minimum(Bars.Low, KPeriods);
		_percentD = new MovingAverage(PercentK, DPeriods, SmoothingType);
	}

	protected override void Calculate(int index)
	{
		var minimum = _minimum[index];
		var numerator = Bars[index].Close - minimum;
		var denominator = _maximum[index] - minimum;

		if (denominator == 0)
		{
			PercentK[index] = index == 0 ? 50 : PercentK[index - 1];
		}
		else
		{
			PercentK[index] = Math.Min(100, Math.Max(0, 100 * numerator / denominator));
		}

		PercentD[index] = _percentD[index];
	}
}
