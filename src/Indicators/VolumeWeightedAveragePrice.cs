﻿namespace Tickblaze.Scripts.Indicators;
/// <summary>
/// Volume Weighted Average Price [VWAP]
/// </summary>
public partial class VolumeWeightedAveragePrice : Indicator
{
	[Parameter("Start Time (local)")]
	public int StartTimeLocal { get; set; } = 1700;

	[Parameter("Show Band 1")]
	public bool ShowBand1 { get; set; } = false;
	[Parameter("Band 1 deviations"), NumericRange(0, double.MaxValue)]
	public double Band1Multiplier { get; set; } = 0.75;

	[Parameter("Show Band 2")]
	public bool ShowBand2 { get; set; } = false;
	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double Band2Multiplier { get; set; } = 1.75;

	[Parameter("Show Band 3")]
	public bool ShowBand3 { get; set; } = false;

	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double Band3Multiplier { get; set; } = 2.75;

	[Plot("Vwap")]
	public PlotSeries Result { get; set; } = new(Color.Cyan);

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

	private Series<double> _volumeSum;
	private Series<double> _typicalVolumeSum;
	private Series<double> _varianceSum;

	public VolumeWeightedAveragePrice()
	{
		Name = "Volume Weighted Average Price";
		ShortName = "VWAP";
		IsOverlay = true;
		AutoRescale = false;
	}

	protected override void Initialize()
	{
		_volumeSum = new DataSeries();
		_typicalVolumeSum = new DataSeries();
		_varianceSum = new DataSeries();
	}

	protected override void Calculate(int index)
	{
		if (index < 1)
		{
			return;
		}

		var time0 = ToInteger(Bars[index].Time.ToLocalTime()) / 100;
		var time1 = ToInteger(Bars[index - 1].Time.ToLocalTime()) / 100;

		var isNewDay = time1 < StartTimeLocal && time0 >= StartTimeLocal || time1 < StartTimeLocal && time0 < time1;
		if (isNewDay)
		{
			_volumeSum[index - 1] = 0;
			_typicalVolumeSum[index - 1] = 0;
			_varianceSum[index - 1] = 0;
		}

		var bar = Bars[index];
		var typicalPrice = Bars.TypicalPrice[index];

		_typicalVolumeSum[index] = _typicalVolumeSum[index - 1] + bar.Volume * typicalPrice;
		_volumeSum[index] = _volumeSum[index - 1] + bar.Volume;

		var curVWAP = _typicalVolumeSum[index] / _volumeSum[index];
		var diff = typicalPrice - curVWAP;

		_varianceSum[index] = _varianceSum[index - 1] + diff * diff;

		var deviation = Math.Sqrt(Math.Max(_varianceSum[index] / (index + 1), 0));

		Result[index] = curVWAP;

		if (ShowBand1)
		{
			Band1Upper[index] = curVWAP + deviation * Band1Multiplier;
			Band1Lower[index] = curVWAP - deviation * Band1Multiplier;
		}

		if (ShowBand2)
		{
			Band2Upper[index] = curVWAP + deviation * Band2Multiplier;
			Band2Lower[index] = curVWAP - deviation * Band2Multiplier;
		}

		if (ShowBand3)
		{
			Band3Upper[index] = curVWAP + deviation * Band3Multiplier;
			Band3Lower[index] = curVWAP - deviation * Band3Multiplier;
		}
	}

	private static int ToInteger(DateTime t)
	{
		var hr = t.Hour * 10000;
		var min = t.Minute * 100;
		return hr + min + t.Second;
	}
}