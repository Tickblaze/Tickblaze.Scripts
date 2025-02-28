﻿using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class MaCrossover : CrossoverStrategyBase
{
	private const string FastMaGroupName = "Fast Moving Average";
	private const string SlowMaGroupName = "Slow Moving Average";

	[Parameter("Source", GroupName = FastMaGroupName)]
	public SourceType FastSource { get; set; } = SourceType.ClosePrices;

	[Parameter("Period", GroupName = FastMaGroupName), NumericRange(1, 999, 1)]
	public int FastPeriod { get; set; } = 12;

	[Parameter("Type", GroupName = FastMaGroupName)]
	public MovingAverageType FastType { get; set; } = MovingAverageType.Simple;

	[Parameter("Source", GroupName = SlowMaGroupName)]
	public SourceType SlowSource { get; set; } = SourceType.ClosePrices;

	[Parameter("Period", GroupName = SlowMaGroupName), NumericRange(1, 999, 1)]
	public int SlowPeriod { get; set; } = 26;

	[Parameter("Type", GroupName = SlowMaGroupName)]
	public MovingAverageType SlowType { get; set; } = MovingAverageType.Simple;

	public enum SourceType
	{
		[DisplayName("Open Prices")]
		OpenPrices,

		[DisplayName("High Prices")]
		HighPrices,

		[DisplayName("Low Prices")]
		LowPrices,

		[DisplayName("Close Prices")]
		ClosePrices
	}

	protected override ISeries<double> FastSeries => _maFast.Result;
	protected override ISeries<double> SlowSeries => _maSlow.Result;

	private MovingAverage _maFast, _maSlow;

	public MaCrossover()
	{
		Name = "MA Crossover";
		Description = "The Moving Average Crossover Strategy detects trends by tracking crossovers between fast and slow moving averages. A bullish crossover triggers a buy order, while a bearish crossover triggers a sell order, aiming to capture early trend changes.";
	}

	protected override void Initialize()
	{
		var fastSource = GetSourceSeries(FastSource);
		var slowSource = GetSourceSeries(SlowSource);

		_maFast = new(fastSource, FastPeriod, FastType)
		{
			ShowOnChart = true,
			Result = new("Fast MA", Color.Blue),
		};
		
		_maSlow = new(slowSource, SlowPeriod, SlowType)
		{
			ShowOnChart = true,
			Result = new("Slow MA", Color.Orange),
		};
	}

	private ISeries<double> GetSourceSeries(SourceType source) => source switch
	{
		SourceType.OpenPrices => Bars.Open,
		SourceType.HighPrices => Bars.High,
		SourceType.LowPrices => Bars.Low,
		_ => Bars.Close
	};
}