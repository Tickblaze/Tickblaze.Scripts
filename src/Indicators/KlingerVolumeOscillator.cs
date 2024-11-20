namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Klinger Volume Oscillator [KVO]
/// </summary>
public partial class KlingerVolumeOscillator : Indicator
{
	[Parameter("Up Color")]
	public Color UpColor { get; set; } = Color.Green;

	[Parameter("Down Color")]
	public Color DownColor { get; set; } = Color.Red;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Histogram);

	private DataSeries _longData;
	private DataSeries _shortData;
	private double _longSmoothFactor;
	private double _shortSmoothFactor;
	public KlingerVolumeOscillator()
	{
		Name = "Klinger Volume Oscillator";
		ShortName = "KVO";
	}

	protected override void Initialize()
	{
		_longData = new DataSeries();
		_shortData = new DataSeries();
		_longSmoothFactor = 2.0 / 56;
		_shortSmoothFactor = 2.0 / 35;
	}

	protected override void Calculate(int index)
	{
		var volume0 = Bars[index].Volume;

		if (index < 1)
		{
			Result[index] = 0;
			_longData[index] = volume0;
			_shortData[index] = volume0;
			return;
		}

		if (Bars.TypicalPrice[index] < Bars.TypicalPrice[index - 1])
		{
			volume0 = -Bars[index].Volume;
		}

		if (_longData[index - 1] == 0)
		{
			_longData[index] = volume0;
			_shortData[index] = volume0;
		}
		else
		{
			_longData[index] = (1 - _longSmoothFactor) * _longData[index - 1] + _longSmoothFactor * volume0;
			_shortData[index] = (1 - _shortSmoothFactor) * _shortData[index - 1] + _shortSmoothFactor * volume0;
		}

		Result[index] = _shortData[index] - _longData[index];
		if (index > 0)
		{
			Result.Colors[index] = Result[index] > Result[index - 1] ? UpColor : DownColor;
		}
	}
}