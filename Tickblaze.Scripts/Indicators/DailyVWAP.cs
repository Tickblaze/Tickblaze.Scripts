
namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Daily VWAP [DV]
/// </summary>
public partial class DailyVWAP : Indicator
{
	[Parameter("Band 1 deviations"), NumericRange(0, double.MaxValue)]
	public double Band1Multiplier { get; set; } = 0.75;

	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double Band2Multiplier { get; set; } = 1.75;

	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double Band3Multiplier { get; set; } = 2.75;

	[Plot("Vwap")]
	public PlotSeries VWAP { get; set; } = new(Color.Cyan);

	[Plot("Band1 Upper")]
	public PlotSeries Band1Upper { get; set; } = new(Color.Green);

	[Plot("Band1 Lower")]
	public PlotSeries Band1Lower { get; set; } = new(Color.Green);

	[Plot("Band2 Upper")]
	public PlotSeries Band2Upper { get; set; } = new(Color.Yellow);

	[Plot("Band2 Lower")]
	public PlotSeries Band2Lower { get; set; } = new(Color.Yellow);

	[Plot("Band3 Upper")]
	public PlotSeries Band3Upper { get; set; } = new(Color.Red);

	[Plot("Band3 Lower")]
	public PlotSeries Band3Lower { get; set; } = new(Color.Red);

	private double _volumeSum;
	private double _typicalVolumeSum;
	private double _varianceSum;
	private bool _isNewDay;

	public DailyVWAP()
	{
		Name = "Daily VWAP";
		ShortName = "DV";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		_isNewDay = index > 2 && Bars[index].Time.Day != Bars[index - 1].Time.Day;

		if (_isNewDay)
		{
			_volumeSum = 0;
			_typicalVolumeSum = 0;
			_varianceSum = 0;
		}

		var bar = Bars[index];
		var typicalPrice = Bars.TypicalPrice[index];// (bar.High + bar.Low + bar.Close) / 3;
		_typicalVolumeSum += bar.Volume * typicalPrice;
		_volumeSum += bar.Volume;
		var curVWAP = _typicalVolumeSum / _volumeSum;
		var diff = typicalPrice - curVWAP;
		_varianceSum += diff * diff;
		var deviation = Math.Sqrt(Math.Max(_varianceSum / (index + 1), 0));

		VWAP[index] = curVWAP;
		Band1Upper[index] = curVWAP + deviation * Band1Multiplier;
		Band1Lower[index] = curVWAP - deviation * Band1Multiplier;
		Band2Upper[index] = curVWAP + deviation * Band2Multiplier;
		Band2Lower[index] = curVWAP - deviation * Band2Multiplier;
		Band3Upper[index] = curVWAP + deviation * Band3Multiplier;
		Band3Lower[index] = curVWAP - deviation * Band3Multiplier;
	}
}
