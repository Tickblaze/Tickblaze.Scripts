namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Keltner Bands [KB]
/// </summary>
public partial class KeltnerChannels : Indicator
{
	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 20;

	[Parameter("Multiplier"), NumericRange(0, int.MaxValue, 0.01)]
	public double Multiplier { get; set; } = 2;

	[Plot("Main")]
	public PlotSeries Main { get; set; } = new(Color.Gray, LineStyle.Dash);

	[Plot("Upper")]
	public PlotSeries Upper { get; set; } = new(Color.Blue, LineStyle.Solid);

	[Plot("Lower")]
	public PlotSeries Lower { get; set; } = new(Color.Blue, LineStyle.Solid);

	private SimpleMovingAverage _smaTypical;
	private SimpleMovingAverage _smaRanges;
	private DataSeries _ranges;

	public KeltnerChannels()
	{
		Name = "Keltner Channels";
		ShortName = "KC";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_ranges = new DataSeries();
		_smaTypical = new SimpleMovingAverage(Bars.TypicalPrice, Period);
		_smaRanges = new SimpleMovingAverage(_ranges, Period);
	}

	protected override void Calculate(int index)
	{
		_ranges[index] = Bars[index].High - Bars[index].Low;

		var channelSize = _smaRanges[index] * Multiplier;

		Main[index] = _smaTypical[index];
		Upper[index] = Main[index] + channelSize;
		Lower[index] = Main[index] - channelSize;
	}
}
