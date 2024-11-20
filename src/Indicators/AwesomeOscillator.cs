namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Awesome Oscillator [AO]
/// </summary>
public partial class AwesomeOscillator : Indicator
{
	[Parameter("Up Color")]
	public Color UpColor { get; set; } = Color.Green;

	[Parameter("Down Color")]
	public Color DownColor { get; set; } = Color.Red;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Histogram);

	private SimpleMovingAverage _sma5, _sma34;

	public AwesomeOscillator()
	{
		Name = "Awesome Oscillator";
		ShortName = "AO";
	}

	protected override void Initialize()
	{
		_sma5 = new SimpleMovingAverage(Bars.MedianPrice, 5);
		_sma34 = new SimpleMovingAverage(Bars.MedianPrice, 34);
	}

	protected override void Calculate(int index)
	{
		Result[index] = _sma5[index] - _sma34[index];

		if (index > 0)
		{
			Result.Colors[index] = Result[index] > Result[index - 1] ? UpColor : DownColor;
		}
	}
}
