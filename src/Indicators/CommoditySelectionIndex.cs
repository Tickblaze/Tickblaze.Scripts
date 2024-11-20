namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Commodity Selection Index [CSI]
/// Reference URL: https://forex-indicators.net/trend-indicators/commodity-selection-index
/// </summary>
public partial class CommoditySelectionIndex : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Interval"), NumericRange(1, int.MaxValue)]
	public int Interval { get; set; } = 14;

	[Parameter("Margin"), NumericRange(1, double.MaxValue)]
	public double Margin { get; set; } = 50;

	[Parameter("Commission"), NumericRange(1, double.MaxValue)]
	public double Commission { get; set; } = 5;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	private AverageTrueRange _atr;
	private AverageDirectionalMovementIndex _adx;
	private double _factor;

	public CommoditySelectionIndex()
	{
		Name = "Commodity Selection Index";
		ShortName = "CSI";
	}

	protected override void Initialize()
	{
		var dollarsPerTick = Bars.Symbol.PointValue * Bars.Symbol.TickSize;

		_atr = new AverageTrueRange(14, MovingAverageType.Simple);
		_adx = new AverageDirectionalMovementIndex(Period);
		_factor = 100 * (dollarsPerTick / Math.Sqrt(Margin)) / (150 + Commission);
	}

	protected override void Calculate(int index)
	{
		Result[index] = index < Interval ? 0 : _factor * _atr[index] * Bars.Symbol.TickSize * (_adx[index] + _adx[index - Interval]) / 2.0;
	}
}
