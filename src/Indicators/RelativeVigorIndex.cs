namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Relative Vigor Index [RVI]
/// </summary>
public sealed partial class RelativeVigorIndex : Indicator
{
	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 10;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Green);

	[Plot("Signal")]
	public PlotSeries Signal { get; set; } = new(Color.Red);

	private DataSeries _fullRange, _bodyRange, _fullRangeSwma, _bodyRangeSwma;

	public RelativeVigorIndex()
	{
		Name = "Relative Vigor Index";
		ShortName = "RVI";
		ScalePrecision = 4;
	}

	protected override void Initialize()
	{
		_fullRange = new DataSeries();
		_bodyRange = new DataSeries();
		_fullRangeSwma = new DataSeries();
		_bodyRangeSwma = new DataSeries();
	}

	protected override void Calculate(int index)
	{
		if (index < 3)
		{
			Result[index] = 0;
			Signal[index] = 0;

			return;
		}

		var bar = Bars[index];

		_bodyRange[index] = bar.Close - bar.Open;
		_fullRange[index] = bar.High - bar.Low;

		_bodyRangeSwma[index] = GetSymmetricalWeightedMovingAverage(_bodyRange, index);
		_fullRangeSwma[index] = GetSymmetricalWeightedMovingAverage(_fullRange, index);

		var period = Math.Min(index + 1, Period);
		var denomitor = 0.0;
		var numerator = 0.0;

		for (var i = 0; i < period; i++)
		{
			denomitor += _fullRangeSwma[index - i];
			numerator += _bodyRangeSwma[index - i];
		}

		Result[index] = numerator / denomitor;
		Signal[index] = GetSymmetricalWeightedMovingAverage(Result, index);
	}

	private static double GetSymmetricalWeightedMovingAverage(ISeries<double> series, int index)
	{
		return (series[index] + series[index - 1] * 2 + series[index - 2] * 2 + series[index - 3]) / 6;
	}
}