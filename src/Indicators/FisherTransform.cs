namespace Tickblaze.Scripts.Indicators;

public partial class FisherTransform : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new("#2962ff", LineStyle.Solid, 1);

	[Plot("Zero")]
	public PlotLevel ZeroLevel { get; set; } = new(0, "#000", LineStyle.Dash, 1);

	private Maximum _maximum;
	private Minimum _minimum;
	private int _priorIndex = -1;
	private double _result, _resultPrev;

	public FisherTransform()
	{
		Name = "Fisher Transform";
	}

	protected override void Initialize()
	{
		_maximum = new Maximum(Source, Period);
		_minimum = new Minimum(Source, Period);
	}

	protected override void Calculate(int index)
	{
		if (_priorIndex != index)
		{
			_priorIndex = index;
			_resultPrev = _result;
		}

		var minLow = _minimum[index];
		var num1 = _maximum[index] - minLow;

		// Guard against infinite numbers and div by zero
		num1 = num1 < Bars.Symbol.TickSize / 10 ? Bars.Symbol.TickSize / 10 : num1;
		_result = 0.66 * ((Source[index] - minLow) / num1 - 0.5) + 0.67 * _resultPrev;

		if (_result > 0.99)
		{
			_result = 0.999;
		}
		else if (_result < -0.99)
		{
			_result = -0.999;
		}

		Result[index] = index > 0 ? 0.5 * Math.Log((1 + _result) / (1 - _result)) + 0.5 * Result[index - 1] : 0;
	}
}