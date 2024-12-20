using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

[System.ComponentModel.Browsable(false)]
public class CrossoverStrategy : BaseStopsAndTargetsStrategy
{
	private const string FixedValueGroupName = "Fixed Value";
	private const string FirstMaGroupName = "MA #1";
	private const string FirstMacdGroupName = "MACD #1";
	private const string FirstAtrGroupName = "ATR #1";
	private const string FirstCciGroupName = "CCI #1";
	private const string FirstMfiGroupName = "MFI #1";
	private const string FirstRsiGroupName = "RSI #1";
	private const string FirstStochGroupName = "STOCH #1";
	private const string SecondMaGroupName = "MA #2";
	private const string SecondMacdGroupName = "MACD #2";
	private const string SecondAtrGroupName = "ATR #2";
	private const string SecondCciGroupName = "CCI #2";
	private const string SecondMfiGroupName = "MFI #2";
	private const string SecondRsiGroupName = "RSI #2";
	private const string SecondStochGroupName = "STOCH #2";

	[Parameter("First Source")]
	public SourceType FirstSourceType { get; set; } = SourceType.CommodityChannelIndex;

	[Parameter("Second Source")]
	public SourceType SecondSourceType { get; set; } = SourceType.CommodityChannelIndex;

	[Parameter("First Value", GroupName = FixedValueGroupName)]
	public double FirstFixedValue { get; set; } = 0;

	[Parameter("Second Value", GroupName = FixedValueGroupName)]
	public double SecondFixedValue { get; set; } = 0;

	[Parameter("Period", GroupName = FirstMaGroupName)]
	public int FirstMavgPeriod { get; set; } = 14;

	[Parameter("Type", GroupName = FirstMaGroupName)]
	public MovingAverageType FirstMavgType { get; set; } = MovingAverageType.Simple;

	[Parameter("Fast Period", GroupName = FirstMacdGroupName), NumericRange(1, int.MaxValue)]
	public int FirstMacdFastPeriod { get; set; } = 12;

	[Parameter("Slow Period", GroupName = FirstMacdGroupName), NumericRange(1, int.MaxValue)]
	public int FirstMacdSlowPeriod { get; set; } = 26;

	[Parameter("Signal Period", GroupName = FirstMacdGroupName), NumericRange(1, int.MaxValue)]
	public int FirstMacdSignalPeriod { get; set; } = 9;

	[Parameter("Output", GroupName = FirstMacdGroupName)]
	public MacdOutputType FirstMacdOutput { get; set; } = MacdOutputType.Macd;

	[Parameter("Period", GroupName = FirstAtrGroupName), NumericRange(1, int.MaxValue)]
	public int FirstAtrPeriod { get; set; } = 14;

	[Parameter("Smoothing Type", GroupName = FirstAtrGroupName)]
	public MovingAverageType FirstAtrSmoothingType { get; set; } = MovingAverageType.Simple;

	[Parameter("Period", GroupName = FirstCciGroupName), NumericRange(1, int.MaxValue)]
	public int FirstCciPeriod { get; set; } = 20;

	[Parameter("Period", GroupName = FirstMfiGroupName), NumericRange(2, int.MaxValue)]
	public int FirstMfiPeriod { get; set; } = 14;

	[Parameter("Period", GroupName = FirstRsiGroupName), NumericRange(1, int.MaxValue)]
	public int FirstRsiPeriod { get; set; } = 14;

	[Parameter("Signal Type", GroupName = FirstRsiGroupName)]
	public MovingAverageType FirstRsiSignalType { get; set; } = MovingAverageType.Simple;

	[Parameter("Signal Period", GroupName = FirstRsiGroupName), NumericRange(1, int.MaxValue)]
	public int FirstRsiSignalPeriod { get; set; } = 14;

	[Parameter("Output", GroupName = FirstRsiGroupName)]
	public RsiOutputType FirstRsiOutput { get; set; } = RsiOutputType.Result;

	[Parameter("%K Periods", GroupName = FirstStochGroupName), NumericRange(1, int.MaxValue)]
	public int FirstStochKPeriods { get; set; } = 9;

	[Parameter("%K Slowing", GroupName = FirstStochGroupName), NumericRange(1, int.MaxValue)]
	public int FirstStochKSlowing { get; set; } = 3;

	[Parameter("%D Periods", GroupName = FirstStochGroupName), NumericRange(1, int.MaxValue)]
	public int FirstStochDPeriods { get; set; } = 9;

	[Parameter("Output", GroupName = FirstStochGroupName)]
	public StochOutputType FirstStochOutput { get; set; } = StochOutputType.PercentK;

	[Parameter("MA Type", GroupName = FirstStochGroupName)]
	public MovingAverageType FirstStochSmoothingType { get; set; } = MovingAverageType.Simple;

	[Parameter("Period", GroupName = SecondMaGroupName)]
	public int SecondMavgPeriod { get; set; } = 14;

	[Parameter("Type", GroupName = SecondMaGroupName)]
	public MovingAverageType SecondMavgType { get; set; } = MovingAverageType.Simple;

	[Parameter("Fast Period", GroupName = SecondMacdGroupName), NumericRange(1, int.MaxValue)]
	public int SecondMacdFastPeriod { get; set; } = 12;

	[Parameter("Slow Period", GroupName = SecondMacdGroupName), NumericRange(1, int.MaxValue)]
	public int SecondMacdSlowPeriod { get; set; } = 26;

	[Parameter("Signal Period", GroupName = SecondMacdGroupName), NumericRange(1, int.MaxValue)]
	public int SecondMacdSignalPeriod { get; set; } = 9;

	[Parameter("Output", GroupName = SecondMacdGroupName)]
	public MacdOutputType SecondMacdOutput { get; set; } = MacdOutputType.Macd;

	[Parameter("Period", GroupName = SecondAtrGroupName), NumericRange(1, int.MaxValue)]
	public int SecondAtrPeriod { get; set; } = 14;

	[Parameter("Smoothing Type", GroupName = SecondAtrGroupName)]
	public MovingAverageType SecondAtrSmoothingType { get; set; } = MovingAverageType.Simple;

	[Parameter("Period", GroupName = SecondCciGroupName), NumericRange(1, int.MaxValue)]
	public int SecondCciPeriod { get; set; } = 20;

	[Parameter("Period", GroupName = SecondMfiGroupName), NumericRange(2, int.MaxValue)]
	public int SecondMfiPeriod { get; set; } = 14;

	[Parameter("Period", GroupName = SecondRsiGroupName), NumericRange(1, int.MaxValue)]
	public int SecondRsiPeriod { get; set; } = 14;

	[Parameter("Signal Type", GroupName = SecondRsiGroupName)]
	public MovingAverageType SecondRsiSignalType { get; set; } = MovingAverageType.Simple;

	[Parameter("Signal Period", GroupName = SecondRsiGroupName), NumericRange(1, int.MaxValue)]
	public int SecondRsiSignalPeriod { get; set; } = 14;

	[Parameter("Output", GroupName = SecondRsiGroupName)]
	public RsiOutputType SecondRsiOutput { get; set; } = RsiOutputType.Result;

	[Parameter("%K Periods", GroupName = SecondStochGroupName), NumericRange(1, int.MaxValue)]
	public int SecondStochKPeriods { get; set; } = 9;

	[Parameter("%K Slowing", GroupName = SecondStochGroupName), NumericRange(1, int.MaxValue)]
	public int SecondStochKSlowing { get; set; } = 3;

	[Parameter("%D Periods", GroupName = SecondStochGroupName), NumericRange(1, int.MaxValue)]
	public int SecondStochDPeriods { get; set; } = 9;

	[Parameter("Output", GroupName = SecondStochGroupName)]
	public StochOutputType SecondStochOutput { get; set; } = StochOutputType.PercentK;

	[Parameter("MA Type", GroupName = SecondStochGroupName)]
	public MovingAverageType SecondStochSmoothingType { get; set; } = MovingAverageType.Simple;

	public enum SourceType
	{
		[DisplayName("Open Prices")]
		OpenPrices,

		[DisplayName("High Prices")]
		HighPrices,

		[DisplayName("Low Prices")]
		LowPrices,

		[DisplayName("Close Prices")]
		ClosePrices,

		[DisplayName("Volume")]
		Volume,

		[DisplayName("Moving Average [MA]")]
		MovingAverage,

		[DisplayName("MACD [MACD]")]
		MovingAverageConvergenceDivergence,

		[DisplayName("Average True Range [ATR]")]
		AverageTrueRange,

		[DisplayName("Commodity Channel Index [CCI]")]
		CommodityChannelIndex,

		[DisplayName("Money Flow Index [MFI]")]
		MoneyFlowIndex,

		[DisplayName("Relative Strength Index [RSI]")]
		RelativeStrengthIndex,

		[DisplayName("Stochastic Oscillator")]
		StochasticOscillator,

		[DisplayName("Fixed Value")]
		FixedValue,
	}

	public enum MacdOutputType
	{
		[DisplayName("MACD")]
		Macd,

		[DisplayName("Signal")]
		Signal,

		[DisplayName("Histogram")]
		Histogram,
	}

	public enum RsiOutputType
	{
		[DisplayName("Result")]
		Result,

		[DisplayName("Average")]
		Average,
	}

	public enum StochOutputType
	{
		[DisplayName("%K")]
		PercentK,

		[DisplayName("%D")]
		PercentD,
	}

	private ISeries<double> _series1, _series2;

	public CrossoverStrategy()
	{
		Name = "Crossover Strategy";
		ShortName = "CS";
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		var sourceTypes = new Dictionary<string, SourceType>
		{
			{ "First", FirstSourceType },
			{ "Second", SecondSourceType }
		};

		foreach (var (sourceTypePrefix, type) in sourceTypes)
		{
			var parameterPrefix = $"{sourceTypePrefix}{GetParameterPrefix(type)}";

			foreach (var parameterName in parameters.Keys.ToArray())
			{
				// Ignore the source type parameter
				if (parameterName.Equals(nameof(FirstSourceType)) || parameterName.Equals(nameof(SecondSourceType)))
				{
					continue;
				}

				// Ignore the parameter if it doesn't belong to the source type
				if (parameterName.StartsWith(sourceTypePrefix) is false)
				{
					continue;
				}

				// Remove the parameter if it doesn't belong to the source type
				if (parameterName.StartsWith(parameterPrefix) is false)
				{
					parameters.Remove(parameterName);
				}
			}
		}

		return parameters;
	}

	private static string GetParameterPrefix(SourceType type) => type switch
	{
		SourceType.OpenPrices or SourceType.HighPrices or SourceType.LowPrices or SourceType.ClosePrices or SourceType.Volume => type.ToString(),
		SourceType.MovingAverage => "Mavg",
		SourceType.MovingAverageConvergenceDivergence => "Macd",
		SourceType.AverageTrueRange => "Atr",
		SourceType.CommodityChannelIndex => "Cci",
		SourceType.MoneyFlowIndex => "Mfi",
		SourceType.RelativeStrengthIndex => "Rsi",
		SourceType.StochasticOscillator => "Stoch",
		SourceType.FixedValue => "FixedValue",
		_ => throw new NotImplementedException(),
	};

	protected override void Initialize()
	{
		_series1 = GetFirstSeries();
		_series2 = GetSecondSeries();
	}

	private ISeries<double> GetFirstSeries() => FirstSourceType switch
	{
		SourceType.OpenPrices => Bars.Open,
		SourceType.HighPrices => Bars.High,
		SourceType.LowPrices => Bars.Low,
		SourceType.ClosePrices => Bars.Close,
		SourceType.Volume => Bars.Volume,
		SourceType.MovingAverage => GetMovingAverage(Bars.Close, FirstMavgPeriod, FirstMavgType),
		SourceType.MovingAverageConvergenceDivergence => GetMacd(Bars.Close, FirstMacdFastPeriod, FirstMacdSlowPeriod, FirstMacdSignalPeriod, FirstMacdOutput),
		SourceType.AverageTrueRange => GetAverageTrueRange(FirstAtrPeriod, FirstAtrSmoothingType),
		SourceType.CommodityChannelIndex => GetCommodityChannelIndex(FirstCciPeriod),
		SourceType.MoneyFlowIndex => GetMoneyFlowIndex(FirstMfiPeriod),
		SourceType.RelativeStrengthIndex => GetRelativeStrengthIndex(Bars.Close, FirstRsiPeriod, FirstRsiSignalType, FirstRsiSignalPeriod, FirstRsiOutput),
		SourceType.StochasticOscillator => GetStochasticOscillator(FirstStochKPeriods, FirstStochKSlowing, FirstStochDPeriods, FirstStochSmoothingType, FirstStochOutput),
		SourceType.FixedValue => new Series<double>(),
		_ => throw new NotImplementedException(),
	};

	private ISeries<double> GetSecondSeries() => SecondSourceType switch
	{
		SourceType.OpenPrices => Bars.Open,
		SourceType.HighPrices => Bars.High,
		SourceType.LowPrices => Bars.Low,
		SourceType.ClosePrices => Bars.Close,
		SourceType.Volume => Bars.Volume,
		SourceType.MovingAverage => GetMovingAverage(Bars.Close, SecondMavgPeriod, SecondMavgType),
		SourceType.MovingAverageConvergenceDivergence => GetMacd(Bars.Close, SecondMacdFastPeriod, SecondMacdSlowPeriod, SecondMacdSignalPeriod, SecondMacdOutput),
		SourceType.AverageTrueRange => GetAverageTrueRange(SecondAtrPeriod, SecondAtrSmoothingType),
		SourceType.CommodityChannelIndex => GetCommodityChannelIndex(SecondCciPeriod),
		SourceType.MoneyFlowIndex => GetMoneyFlowIndex(SecondMfiPeriod),
		SourceType.RelativeStrengthIndex => GetRelativeStrengthIndex(Bars.Close, SecondRsiPeriod, SecondRsiSignalType, SecondRsiSignalPeriod, SecondRsiOutput),
		SourceType.StochasticOscillator => GetStochasticOscillator(SecondStochKPeriods, SecondStochKSlowing, SecondStochDPeriods, SecondStochSmoothingType, SecondStochOutput),
		SourceType.FixedValue => new Series<double>(),
		_ => throw new NotImplementedException(),
	};

	private static PlotSeries GetMovingAverage(ISeries<double> source, int period, MovingAverageType type)
	{
		var movingAverage = new MovingAverage(source, period, type)
		{
			ShowOnChart = true
		};

		return movingAverage.Result;
	}

	private static PlotSeries GetMacd(ISeries<double> source, int fastPeriod, int slowPeriod, int signalPeriod, MacdOutputType output)
	{
		var macd = new MovingAverageConvergenceDivergence(source, fastPeriod, slowPeriod, signalPeriod, Color.Green, Color.Red)
		{
			ShowOnChart = true
		};

		return output switch
		{
			MacdOutputType.Macd => macd.Result,
			MacdOutputType.Signal => macd.Signal,
			MacdOutputType.Histogram => macd.Histogram,
			_ => throw new NotImplementedException(),
		};
	}

	private static PlotSeries GetAverageTrueRange(int period, MovingAverageType smoothingType)
	{
		var averageTrueRange = new AverageTrueRange(period, smoothingType)
		{
			ShowOnChart = true
		};

		return averageTrueRange.Result;
	}

	private static PlotSeries GetCommodityChannelIndex(int period)
	{
		var commodityChannelIndex = new CommodityChannelIndex(period)
		{
			ShowOnChart = true
		};

		return commodityChannelIndex.Result;
	}

	private static PlotSeries GetMoneyFlowIndex(int period)
	{
		var moneyFlowIndex = new MoneyFlowIndex(period)
		{
			ShowOnChart = true
		};

		return moneyFlowIndex.Result;
	}

	private static PlotSeries GetRelativeStrengthIndex(ISeries<double> source, int period, MovingAverageType signalType, int signalPeriod, RsiOutputType output)
	{
		var relativeStrengthIndex = new RelativeStrengthIndex(source, period, signalType, signalPeriod)
		{
			ShowOnChart = true
		};

		return output switch
		{
			RsiOutputType.Result => relativeStrengthIndex.Result,
			RsiOutputType.Average => relativeStrengthIndex.Average,
			_ => throw new NotImplementedException(),
		};
	}

	private static PlotSeries GetStochasticOscillator(int kPeriods, int kSlowing, int dPeriods, MovingAverageType smoothingType, StochOutputType output)
	{
		var stochasticOscillator = new StochasticOscillator(kPeriods, kSlowing, dPeriods, smoothingType)
		{
			ShowOnChart = true
		};

		return output switch
		{
			StochOutputType.PercentK => stochasticOscillator.PercentK,
			StochOutputType.PercentD => stochasticOscillator.PercentD,
			_ => throw new NotImplementedException(),
		};
	}

	protected override void OnBar(int index)
	{
		if (FirstSourceType is SourceType.FixedValue && _series1 is Series<double> firstSeries)
		{
			firstSeries[index] = FirstFixedValue;
		}

		if (SecondSourceType is SourceType.FixedValue && _series2 is Series<double> secondSeries)
		{
			secondSeries[index] = FirstFixedValue;
		}

		if (index == 0)
		{
			return;
		}

		if (_series1[index] >= _series2[index] && _series1[index - 1] < _series2[index - 1])
		{
			TryEnterMarket(OrderDirection.Long);
		}
		else if(_series1[index] <= _series2[index] && _series1[index - 1] > _series2[index - 1])
		{
			TryEnterMarket(OrderDirection.Short);
		}
	}
}
