﻿namespace Tickblaze.Scripts.BarTypes;

public sealed class RenkoBxt : BarType
{
	[Parameter("Bar Size (Ticks)")]
	public int BarSize { get; set; } = 4;

	[Parameter("Reversal Size (Ticks)")]
	public int ReversalSize { get; set; } = 8;

	[Parameter("Open Offset (Ticks)")]
	public int Offset { get; set; } = 2;

	private int _trend;

	private IExchangeSession? _currentSession;

	public RenkoBxt()
	{
		Source = SourceDataType.Tick;
	}

	public override void OnDataPoint(Bar bar)
	{
		if (Bars.Count == 0 || bar.Time > _currentSession?.EndUtcDateTime)
		{
			_currentSession = Symbol.ExchangeCalendar.GetSession(bar.Time);
			_trend = 0;
			AddBar(bar);
			return;
		}

		var curLastBar = Bars[^1]!;
		
		// Handle invalid configurations as one giant bar
		if (Offset >= BarSize || ReversalSize <= Offset)
		{
			UpdateBar(curLastBar! with
			{
				High = Math.Max(curLastBar!.High, bar.High),
				Low = Math.Min(curLastBar!.Low, bar.Low),
				Close = bar.Close,
				Volume = curLastBar.Volume + bar.Volume,
				EndTime = bar.EndTime
			});

			return;
		}

		var addedNewBars = false;
		while (true)
		{
			// Calculate the levels at which a new bar would form
			var trigUp = Bars.Symbol.RoundToTick(_trend == -1 ? curLastBar.Low + ReversalSize * Bars.Symbol.TickSize : curLastBar.Open + BarSize * Bars.Symbol.TickSize);
			var trigDown = Bars.Symbol.RoundToTick(_trend == 1 ? curLastBar.High - ReversalSize * Bars.Symbol.TickSize : curLastBar.Open - BarSize * Bars.Symbol.TickSize);

			// If we haven't moved enough to create a new bar, update the last bar instead
			var direction = bar.Close.CompareTo(trigUp) == 1 ? 1 : bar.Close.CompareTo(trigDown) == -1 ? -1 : 0;
			if (direction == 0)
			{
				// Since every bar starts with min volume, we need to account for that
				var barVolume = curLastBar.Volume + bar.Volume;
				if (addedNewBars)
					barVolume -= (double) Symbol.MinimumVolume;

				var newHigh = Math.Max(curLastBar.High, bar.High);
				var newLow = Math.Min(curLastBar.Low, bar.Low);
				UpdateBar(curLastBar! with
				{
					High = newHigh,
					Low = newLow,
					Close = bar.Close,
					Volume = barVolume,
					EndTime = bar.EndTime
				});

				return;
			}

			_trend = direction;

			// Calculate new close and adjust it for tick size
			var newClose = direction == 1 ? trigUp : trigDown;

			// Finalize the current last bar
			UpdateBar(curLastBar with
			{
				High = direction == 1 ? newClose : curLastBar.High,
				Low = direction == 1 ? curLastBar.Low : newClose,
				Close = newClose,
				EndTime = bar.EndTime
			});

			// Start a new last bar (give it the minimum possible volume, as zero volume bars aren't displayed at all)
			addedNewBars = true;
			newClose = (direction == 1 ? trigUp : trigDown) + direction * Symbol.TickSize;
			var newOpen = newClose - direction * BarSize * Symbol.TickSize;
			AddBar(curLastBar = new Bar(bar.Time, newOpen, direction == 1 ? newClose : newOpen, direction == 1 ? newOpen : newClose, newClose, (double) Symbol.MinimumVolume)
			{
				EndTime = bar.EndTime
			});
		}
	}
}