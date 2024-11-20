namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Ultimate Oscillator [UO]
/// </summary>
public sealed partial class UltimateOscillator : Indicator
{
	[Parameter("Cycle 1"), NumericRange(1)]
	public int Cycle1 { get; set; } = 7;

	[Parameter("Cycle 2"), NumericRange(1)]
	public int Cycle2 { get; set; } = 14;

	[Parameter("Cycle 3"), NumericRange(1)]
	public int Cycle3 { get; set; } = 28;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	[Plot("Overbought Level")]
	public PlotLevel OverboughtLevel { get; set; } = new(70, Color.Red, LineStyle.Dash, 1);

	[Plot("Oversold Level")]
	public PlotLevel OversoldLevel { get; set; } = new(30, Color.Green, LineStyle.Dash, 1);

	private int _barsRequiredToCalculate;

	public UltimateOscillator()
	{
		Name = "Ultimate Oscillator";
		ShortName = "UO";
		ScalePrecision = 2;
		IsOverlay = false;
	}

	protected override void Initialize()
	{
		_barsRequiredToCalculate = Math.Max(Cycle1, Math.Max(Cycle2, Cycle3));
	}

	protected override void Calculate(int index)
	{
		if (index <= _barsRequiredToCalculate)
		{
			Result[index] = 0;
			return;
		}

		var averageBuyingPressure = new double[]
		{
			CalculateAverageBuyingPressure(index, Cycle1),
			CalculateAverageBuyingPressure(index, Cycle2),
			CalculateAverageBuyingPressure(index, Cycle3)
		};

		Result[index] = (4.0 * averageBuyingPressure[0] + 2.0 * averageBuyingPressure[1] + averageBuyingPressure[2]) / 7.0 * 100.0;
	}

	private double CalculateAverageBuyingPressure(int index, int periods)
	{
		var buyingPressureSum = 0.0;
		var trueRangeSum = 0.0;

		for (var i = periods - 1; i >= 0; i--)
		{
			var buyingPressure = CalculateBuyingPressure(index - i);
			var trueRange = CalculateTrueRange(index - i);

			buyingPressureSum += buyingPressure;
			trueRangeSum += trueRange;
		}

		return buyingPressureSum / trueRangeSum;
	}

	private double CalculateTrueRange(int period)
	{
		return Math.Max(Bars.High[period], Bars.Close[period - 1]) - Math.Min(Bars.Low[period], Bars.Close[period - 1]);
	}

	private double CalculateBuyingPressure(int period)
	{
		return Bars.Close[period] - Math.Min(Bars.Low[period], Bars.Close[period - 1]);
	}
}
