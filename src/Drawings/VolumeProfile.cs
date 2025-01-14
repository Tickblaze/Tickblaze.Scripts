namespace Tickblaze.Scripts.Drawings;

public class VolumeProfileExtended : VolumeProfile
{
	protected override bool ExtendRight => true;

	public VolumeProfileExtended()
	{
		Name = "Volume Profile - Realtime";
	}
}

public class VolumeProfile : Drawing, VolumeProfile.ISettings
{
	[Parameter("Data Source", Description = "Data source for the volume profile")]
	public SourceDataType SourceData { get; set; } = SourceDataType.Chart;

	[Parameter("Histo Size Type", Description = "Determines how histogram rows are calculated (by count or ticks)")]
	public RowsLayoutType RowsLayout { get; set; } = RowsLayoutType.Count;

	[NumericRange(1, int.MaxValue)]
	[Parameter("Histo Size Value", Description = "Defines the size of the histogram rows")]
	public int RowsSize { get; set; } = 24;

	public const string StyleGroupName = "Style";

	[NumericRange(0, 100)]
	[Parameter("Histo Width %", Description = "Width of the histogram as a percentage of box width", GroupName = StyleGroupName)]
	public double RowsWidthPercent { get; set; } = 30;

	[Parameter("Histo Location", Description = "Location of histogram (left or right side of box)", GroupName = StyleGroupName)]
	public PlacementType RowsPlacement { get; set; } = PlacementType.Left;

	[NumericRange(0, 100)]
	[Parameter("Value Area %", Description = "Percentage of total volume considered in the value area", GroupName = StyleGroupName)]
	public double ValueAreaPercent { get; set; } = 70;

	[Parameter("Value Area Color", Description = "Color of the value area", GroupName = StyleGroupName)]
	public Color ValueAreaColor { get; set; } = "#bf808080";

	[Parameter("Above Value Area Color", Description = "Color of area above the value area", GroupName = StyleGroupName)]
	public Color ValueAreaAboveColor { get; set; } = "#80ff0000";

	[Parameter("Below Value Area Color", Description = "Color of area below the value area", GroupName = StyleGroupName)]
	public Color ValueAreaBelowColor { get; set; } = "#80ff0000";

	[Parameter("Show Outline?", Description = "Show/Hide the outline", GroupName = StyleGroupName)]
	public bool BoxVisible { get; set; } = true;

	[Parameter("Outline Color", Description = "Color of the volume profile outline box", GroupName = StyleGroupName)]
	public Color BoxLineColor { get; set; } = "#80ffffff";

	[Parameter("Outline Thickness", Description = "Thickness of the volume profile outline box", GroupName = StyleGroupName)]
	public int BoxLineThickness { get; set; } = 1;

	[Parameter("Outline Style", Description = "Style of the volume profile outline box (solid, dashed, etc.)", GroupName = StyleGroupName)]
	public LineStyle BoxLineStyle { get; set; } = LineStyle.Dot;

	[Parameter("Show VAH Line?", Description = "Show/Hide the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public bool VahLineVisible { get; set; } = false;

	[Parameter("VAH Line Color", Description = "Color of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public Color VahLineColor { get; set; } = Color.White;

	[Parameter("VAH Line Thickness", Description = "Thickness of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public int VahLineThickness { get; set; } = 2;

	[Parameter("VAH Line Style", Description = "Style of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public LineStyle VahLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Show VAL Line?", Description = "Show/Hide the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public bool ValLineVisible { get; set; } = false;

	[Parameter("VAL Line Color", Description = "Color of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public Color ValLineColor { get; set; } = Color.White;

	[Parameter("VAL Line Thickness", Description = "Thickness of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public int ValLineThickness { get; set; } = 2;

	[Parameter("VAL Line Style", Description = "Style of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public LineStyle ValLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Show POC Line?", Description = "Show/Hide the Point of Control (POC) line", GroupName = StyleGroupName)]
	public bool PocLineVisible { get; set; } = true;

	[Parameter("POC Line Color", Description = "Color of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public Color PocLineColor { get; set; } = Color.Yellow;

	[Parameter("POC Line Thickness", Description = "Thickness of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public int PocLineThickness { get; set; } = 2;

	[Parameter("POC Line Style", Description = "Style of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public LineStyle PocLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Show VAH/VAL/POC Prices?", Description = "Displays prices for VAH, VAL, and POC levels", GroupName = StyleGroupName)]
	public bool ShowPrices { get; set; } = false;

	[Parameter("Font", Description = "Font for displaying VAH/VAL/POC prices", GroupName = StyleGroupName)]
	public Font Font { get; set; } = new("Arial", 12);

	[Parameter("VWAP Enabled?", Description = "Show/Hide the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public bool VwapEnabled { get; set; } = false;

	[Parameter("VWAP Line Color", Description = "Color of the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public Color VwapLineColor { get; set; } = Color.Blue;

	[Parameter("VWAP Line Thickness", Description = "Thickness of the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public int VwapLineThickness { get; set; } = 1;

	[Parameter("VWAP Line Style", Description = "Style of the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public LineStyle VwapLineStyle { get; set; } = LineStyle.Solid;

	public override int PointsCount => ExtendRight ? 1 : 2;

	protected virtual bool ExtendRight => false;

	private BarSeries _bars;
	private Area<VolumeProfile> _area;

	public VolumeProfile()
	{
		Name = "Volume Profile - Manual";
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		parameters[nameof(RowsSize)].Attributes.Name = RowsLayout is RowsLayoutType.Count 
			? "Histo Rows Count" 
			: "Histo Ticks Size";

		if (BoxVisible is false)
		{
			parameters.Remove(nameof(BoxLineColor));
			parameters.Remove(nameof(BoxLineThickness));
			parameters.Remove(nameof(BoxLineStyle));
		}

		if (VahLineVisible is false)
		{
			parameters.Remove(nameof(VahLineColor));
			parameters.Remove(nameof(VahLineThickness));
			parameters.Remove(nameof(VahLineStyle));
		}

		if (ValLineVisible is false)
		{
			parameters.Remove(nameof(ValLineColor));
			parameters.Remove(nameof(ValLineThickness));
			parameters.Remove(nameof(ValLineStyle));
		}

		if (PocLineVisible is false)
		{
			parameters.Remove(nameof(PocLineColor));
			parameters.Remove(nameof(PocLineThickness));
			parameters.Remove(nameof(PocLineStyle));
		}

		if (VwapEnabled is false)
		{
			parameters.Remove(nameof(VwapLineColor));
			parameters.Remove(nameof(VwapLineThickness));
			parameters.Remove(nameof(VwapLineStyle));
		}

		if (ShowPrices is false)
		{
			parameters.Remove(nameof(Font));
		}

		return parameters;
	}

	protected override void Initialize()
	{
		if (Bars == null)
		{
			return;
		}

		_bars = TryGetDataSeriesRequest(this, out var request) ? GetBarSeries(request) : Bars;
	}

	public static bool TryGetDataSeriesRequest<T>(T script, out BarSeriesRequest request)
		where T : SymbolScript, ISettings
	{
		request = null;

		if (script.SourceData is SourceDataType.Chart)
		{
			return false;
		}

		var barPeriod = script.SourceData switch
		{
			SourceDataType.Chart => script.Bars.Period,
			SourceDataType.Minute => new BarPeriod(BarPeriod.SourceType.Minute, BarPeriod.PeriodType.Minute, 1),
			SourceDataType.Second => new BarPeriod(BarPeriod.SourceType.Trade, BarPeriod.PeriodType.Second, 1),
			SourceDataType.Tick => new BarPeriod(BarPeriod.SourceType.Trade, BarPeriod.PeriodType.Tick, 1),
			_ => throw new ArgumentOutOfRangeException()
		};

		request = new BarSeriesRequest
		{
			Period = barPeriod,
			SymbolCode = script.Bars.Symbol.Code,
			Exchange = script.Bars.Symbol.Exchange,
			InstrumentType = script.Bars.Symbol.Type,
			Contract = script.Bars.ContractSettings,
			IsETH = script.Bars.IsETH
		};

		return true;
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		if (Points.Count >= PointsCount)
		{
			UpdateArea();
			AdjustAnchorPoints();
		}
	}

	private void AdjustAnchorPoints()
	{
		var midPrice = (_area.High + _area.Low) / 2;

		if (_bars.Count != 0)
		{
			foreach (var point in Points)
			{
				if (point.Value.Equals(midPrice) is false)
				{
					point.Value = midPrice;
				}
			}
		}

		var firstPoint = Points
			.OrderBy(x => x.X)
			.First();
		var lastBarTime = Chart.GetTimeByXCoordinate(Chart.GetXCoordinateByBarIndex(Bars.Count - 1));

		if ((DateTime)firstPoint.Time > lastBarTime)
		{
			firstPoint.Time = lastBarTime;
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		UpdateArea();
		if (Points.Count == 1)
		{
			AdjustAnchorPoints();
		}

		_area.Render(context);
	}

	private void UpdateArea()
	{
		var fromIndex = Chart.GetBarIndexByXCoordinate(Points[0].X);
		var toIndex = ExtendRight ? Bars.Count - 1 : Chart.GetBarIndexByXCoordinate(Points[1].X);

		if (fromIndex > toIndex)
		{
			(fromIndex, toIndex) = (toIndex, fromIndex);
		}

		if (_area == null || _area.Bars.Period != _bars.Period || _area.Bars.IsETH != _bars.IsETH)
		{
			_area = new Area<VolumeProfile>(this, fromIndex, toIndex, _bars);
		}

		_area.FromIndex = fromIndex;
		_area.ToIndex = toIndex;
	}

	public interface ISettings
	{
		SourceDataType SourceData { get; set; }
		RowsLayoutType RowsLayout { get; set; }
		int RowsSize { get; set; }
		double RowsWidthPercent { get; set; }
		PlacementType RowsPlacement { get; set; }
		double ValueAreaPercent { get; set; }
		Color ValueAreaColor { get; set; }
		Color ValueAreaAboveColor { get; set; }
		Color ValueAreaBelowColor { get; set; }
		public bool BoxVisible { get; set; }
		Color BoxLineColor { get; set; }
		int BoxLineThickness { get; set; }
		LineStyle BoxLineStyle { get; set; }
		bool VahLineVisible { get; set; }
		Color VahLineColor { get; set; }
		int VahLineThickness { get; set; }
		LineStyle VahLineStyle { get; set; }
		bool ValLineVisible { get; set; }
		Color ValLineColor { get; set; }
		int ValLineThickness { get; set; }
		LineStyle ValLineStyle { get; set; }
		bool PocLineVisible { get; set; }
		Color PocLineColor { get; set; }
		int PocLineThickness { get; set; }
		LineStyle PocLineStyle { get; set; }
		bool ShowPrices { get; set; }
		Font Font { get; set; }
		bool VwapEnabled { get; set; }
		Color VwapLineColor { get; set; }
		int VwapLineThickness { get; set; }
		LineStyle VwapLineStyle { get; set; }
	}

	public enum SourceDataType
	{
		Chart,
		Minute,
		Second,
		Tick
	}

	public enum RowsLayoutType
	{
		Count,
		Ticks
	}

	public enum PlacementType
	{
		Left,
		Right
	}

	public class Area<T>(T script, int fromIndex, int toIndex, BarSeries bars)
		where T : SymbolScript, IChartObject, ISettings
	{
		public int FromIndex
		{
			get => _fromIndex;
			set
			{
				if (_fromIndex == value)
				{
					return;
				}

				_isCalculated = false;
				_fromIndex = value;
			}
		}

		public int ToIndex
		{
			get => _toIndex;
			set
			{
				if (_toIndex == value)
				{
					return;
				}

				_isCalculated = false;
				_toIndex = value;
			}
		}

		public double High { get; private set; }
		public double Low { get; private set; }
		public double Range { get; private set; }

		private ISettings Settings => _script;
		private Symbol Symbol => Bars.Symbol;
		private double TickSize => Bars.Symbol.TickSize;

		private readonly T _script = script ?? throw new ArgumentNullException(nameof(script));
		private readonly Drawing _drawing = script as Drawing;
		public BarSeries Bars => bars ?? throw new ArgumentNullException(nameof(bars));
		private Volume[] _volumes;
		private BarsRange _range;
		private int _pocIndex, _vahIndex, _valIndex;
		private double _volume, _rowSize, _maximum, _minimum;
		private bool _isTickSize;
		private double[] _vwap;
		private SourceDataType _calculatedSourceDataType;
		private RowsLayoutType _calculatedRowsLayoutType;
		private int _calculatedRowsSize;
		private bool _isCalculated, _isHistorical;
		private int _fromIndex = fromIndex;
		private int _toIndex = toIndex;

		public void Render(IDrawingContext context)
		{
			if (IsCalculated() is false && _drawing is null or { IsUpdating: false })
			{
				CalculateRowSize();
				CalculateVolumes();
				CalculateVwap();

				_isCalculated = true;
				_calculatedSourceDataType = Settings.SourceData;
				_calculatedRowsLayoutType = Settings.RowsLayout;
				_calculatedRowsSize = Settings.RowsSize;
			}

			var chart = _script.Chart;
			var chartScale = _script.ChartScale;

			if (chart is null || chartScale is null)
			{
				return;
			}

			if (FromIndex > chart.LastVisibleBarIndex || ToIndex < chart.FirstVisibleBarIndex)
			{
				return;
			}

			var highY = chartScale.GetYCoordinateByValue(High);
			var lowY = chartScale.GetYCoordinateByValue(Low);
			var leftX = chart.GetXCoordinateByBarIndex(FromIndex);
			var rightX = chart.GetXCoordinateByBarIndex(ToIndex);

			if (_script.BoxVisible)
			{
				context.DrawRectangle(new Point(leftX, highY), new Point(rightX, lowY), null, Settings.BoxLineColor, Settings.BoxLineThickness, Settings.BoxLineStyle);
			}

			var rows = _volumes?.Length ?? 0;
			var barsUsed = _range is null ? "null" : $"{_range.ToIndex - _range.FromIndex}/{Bars.Count}";

			if (false)
			{
				context.DrawText(new Point(leftX, lowY),
					$"Rows: {rows}, Size: {Symbol.FormatPrice(_rowSize)}, Bars: {barsUsed}, Volume: {_volume}, VAH: {_vahIndex}, VAL: {_valIndex}, Historical: {_isHistorical}",
					Settings.BoxLineColor, Settings.Font);
			}

			if (rows == 0 || _range is null || _volumes is null || _volumes.Length == 0 || _drawing is { IsUpdating: true })
			{
				return;
			}

			var maximumY = chartScale.GetYCoordinateByValue(_maximum);
			var minimumY = chartScale.GetYCoordinateByValue(_minimum);
			var pixelsPerUnitY = Math.Abs(maximumY - minimumY) / (_maximum - _minimum);
			var adjustSpacing = _rowSize * pixelsPerUnitY > 5;
			var lineThickness = adjustSpacing ? 2 : 1;
			var x = Settings.RowsPlacement is PlacementType.Left ? leftX : rightX;
			var boxWidth = Settings.RowsPlacement is PlacementType.Left ? rightX - leftX : leftX - rightX;

			for (var i = 0; i < _volumes.Length; i++)
			{
				var volume = _volumes[i];
				var y = minimumY - i * _rowSize * pixelsPerUnitY - 1;
				if (y < 0)
				{
					break;
				}

				var volumeRatio = volume.Total / _volumes[_pocIndex].Total;
				var barWidth = boxWidth * (Settings.RowsWidthPercent / 100) * volumeRatio;
				var barHeight = _rowSize * pixelsPerUnitY - (adjustSpacing ? 1 : 0);

				if (y - barHeight > chart.Height)
				{
					continue;
				}

				if (Settings.PocLineVisible && i == _pocIndex)
				{
					var pointA = new Point(leftX, y - barHeight / 2);
					var pointB = new Point(rightX, pointA.Y);

					DrawPriceLevel(context, pointA, pointB, Settings.PocLineColor, Settings.PocLineThickness, Settings.PocLineStyle);
				}

				if (Settings.ValLineVisible && i == _valIndex)
				{
					var pointA = new Point(leftX, _isTickSize ? y - barHeight / 2 : y);
					var pointB = new Point(rightX, pointA.Y);

					DrawPriceLevel(context, pointA, pointB, Settings.ValLineColor, Settings.ValLineThickness, Settings.ValLineStyle);
				}

				if (Settings.VahLineVisible && i == _vahIndex)
				{
					var pointA = new Point(leftX, _isTickSize ? y - barHeight / 2 : y - barHeight);
					var pointB = new Point(rightX, pointA.Y);

					DrawPriceLevel(context, pointA, pointB, Settings.VahLineColor, Settings.VahLineThickness, Settings.VahLineStyle);
				}

				var color = i > _vahIndex
					? Settings.ValueAreaAboveColor
					: i < _valIndex
						? Settings.ValueAreaBelowColor
						: Settings.ValueAreaColor;

				DrawColumn(context, new Point(x, y), barWidth, barHeight, color, lineThickness);
			}

			if (Settings.VwapEnabled && _vwap.Length > 0)
			{
				var points = new Point[_vwap.Length];

				for (var i = 0; i < points.Length; i++)
				{
					points[i] = new Point
					{
						X = chart.GetXCoordinateByBarIndex(FromIndex + i),
						Y = chartScale.GetYCoordinateByValue(_vwap[i])
					};
				}

				context.DrawPolygon(points, null, Settings.VwapLineColor, Settings.VwapLineThickness, Settings.VwapLineStyle);
			}
		}

		private bool IsCalculated()
		{
			var isCalculated = _isCalculated && Settings.SourceData == _calculatedSourceDataType && Settings.RowsLayout == _calculatedRowsLayoutType && Settings.RowsSize == _calculatedRowsSize;
			if (isCalculated && _isHistorical)
			{
				return true;
			}

			var high = double.MinValue;
			var low = double.MaxValue;
			var volume = 0.0;

			for (var index = FromIndex; index <= ToIndex; index++)
			{
				var bar = _script.Bars[index];
				if (bar is null)
				{
					continue;
				}

				high = Math.Max(high, bar.High);
				low = Math.Min(low, bar.Low);
				volume += bar.Volume;
			}

			isCalculated = isCalculated && High == high && Low == low && _volume == volume;

			High = high;
			Low = low;
			Range = high - low;

			_volume = volume;

			return isCalculated;
		}

		private void CalculateRowSize()
		{
			_isTickSize = false;
			
			if (Settings.RowsLayout is RowsLayoutType.Ticks)
			{
				_rowSize = TickSize * Settings.RowsSize;
				_isTickSize = true;
			}
			else
			{
				_rowSize = Range / Settings.RowsSize;

				if (_rowSize <= TickSize)
				{
					_rowSize = TickSize;
					_isTickSize = true;
				}
			}

			var rows = (int)Math.Round(Range / _rowSize);
			var rowsMaximum = 500;

			if (rows > rowsMaximum)
			{
				_rowSize = Range / rowsMaximum;

				if (_isTickSize)
				{
					_rowSize = Symbol.RoundToTick(_rowSize);
				}
			}

			if (_rowSize <= 0)
			{
				_volumes = [];
			}
			else if (Settings.RowsLayout is RowsLayoutType.Ticks)
			{
				_minimum = Low - Symbol.TickSize / 2;
				_maximum = _minimum + _rowSize * rows;
				_volumes = new Volume[rows + 1];
			}
			else
			{
				_maximum = High;
				_minimum = Low;
				_volumes = new Volume[rows];
			}
		}

		private void CalculateVolumes()
		{
			if (_volumes.Length == 0)
			{
				return;
			}

			// Bar time offset by 1, temp fix for open time
			var barOffset = Settings.SourceData is SourceDataType.Chart ? 0 : 1;
			var fromTime = _script.Bars[Math.Min(_script.Bars.Count - 1, FromIndex + barOffset)]!.Time;
			var toTime = ToIndex >= _script.Bars.Count - (1 + barOffset)
				? DateTime.MaxValue
				: _script.Bars[ToIndex + (1 + barOffset)]!.Time;

			_isHistorical = toTime < DateTime.MaxValue;
			_range = null;

			for (var i = 0; i < Bars.Count; i++)
			{
				var bar = Bars[i];
				if (bar is null)
				{
					continue;
				}

				var barTime = bar.Time;
				if (barTime < fromTime)
				{
					continue;
				}

				if (barTime < toTime)
				{
					_range ??= new BarsRange { FromIndex = i };
					_range.ToIndex = i;
				}
				else
				{
					break;
				}
			}

			if (_range is null)
			{
				return;
			}

			for (var index = _range.FromIndex; index <= _range.ToIndex; index++)
			{
				var bar = Bars[index];
				var buyVolume = 0.0;
				var sellVolume = 0.0;

				if (Settings.SourceData is SourceDataType.Tick)
				{
					if (index > 0)
					{
						var currentPrice = bar.Close;
						var previousPrice = Bars[index - 1].Close;

						if (currentPrice > previousPrice)
						{
							buyVolume = bar.Volume;
						}
						else if (currentPrice < previousPrice)
						{
							sellVolume = bar.Volume;
						}
						else
						{
							buyVolume = sellVolume = bar.Volume / 2;
						}
					}
					else
					{
						buyVolume = sellVolume = bar.Volume / 2;
					}

					var level = (int)Math.Floor((bar.Close - _minimum) / _rowSize);
					level = Math.Max(0, Math.Min(_volumes.Length - 1, level));

					_volumes[level] ??= new Volume();
					_volumes[level].Buy += buyVolume;
					_volumes[level].Sell += sellVolume;
				}
				else
				{
					int startLevel, endLevel;

					if (bar.High == bar.Low)
					{
						startLevel = endLevel = Math.Max(0, Math.Min(_volumes.Length - 1, (int)Math.Floor((bar.High - _minimum) / _rowSize)));
					}
					else
					{
						startLevel = Math.Max(0, (int)Math.Floor((bar.Low - _minimum) / _rowSize));
						endLevel = Math.Min(_volumes.Length - 1, (int)Math.Floor((bar.High - _minimum - Symbol.TickSize / 2) / _rowSize));
					}

					var volumePerLevel = bar.Volume / (endLevel - startLevel + 1);

					if (bar.Close > bar.Open)
					{
						buyVolume = volumePerLevel;
					}
					else if (bar.Close < bar.Open)
					{
						sellVolume = volumePerLevel;
					}
					else
					{
						buyVolume = sellVolume = volumePerLevel / 2;
					}

					for (var level = startLevel; level <= endLevel; level++)
					{
						_volumes[level] ??= new Volume();
						_volumes[level].Buy += buyVolume;
						_volumes[level].Sell += sellVolume;
					}
				}
			}

			_pocIndex = 0;

			for (var i = 0; i < _volumes.Length; i++)
			{
				_volumes[i] ??= new Volume();

				if (_volumes[_pocIndex] < _volumes[i])
				{
					_pocIndex = i;
				}
			}

			var accumulatedVolume = _volumes[_pocIndex].Total;
			var totalVolume = _volumes.Sum(x => x.Total);
			var targetVolume = totalVolume * (Settings.ValueAreaPercent / 100);

			_vahIndex = _pocIndex;
			_valIndex = _pocIndex;

			while (accumulatedVolume < targetVolume)
			{
				var expanded = false;

				var valIndex = _valIndex;
				var vahIndex = _vahIndex;
				var volumeAbove = 0.0;
				var volumeBelow = 0.0;

				for (var i = 0; i < 2; i++)
				{
					if (valIndex > 0)
					{
						volumeBelow += _volumes[--valIndex].Total;
					}

					if (vahIndex < _volumes.Length - 1)
					{
						volumeAbove += _volumes[++vahIndex].Total;
					}
				}

				if (volumeAbove > volumeBelow)
				{
					accumulatedVolume += volumeAbove;

					if (_vahIndex < vahIndex)
					{
						_vahIndex = vahIndex;
						expanded = true;
					}
				}
				else
				{
					accumulatedVolume += volumeBelow;

					if (_valIndex > valIndex)
					{
						_valIndex = valIndex;
						expanded = true;
					}
				}

				if (expanded is false)
				{
					break;
				}
			}
		}

		private void CalculateVwap()
		{
			if (Settings.VwapEnabled is false || FromIndex == ToIndex)
			{
				_vwap = [];
				return;
			}

			var cumulativeVolume = 0.0;
			var cumulativeVolumePrice = 0.0;
			var vwap = new List<double>();

			for (var index = FromIndex; index <= ToIndex; index++)
			{
				var bar = _script.Bars[index];
				var typicalPrice = (bar.High + bar.Low + bar.Close) / 3;

				cumulativeVolume += bar.Volume;
				cumulativeVolumePrice += typicalPrice * bar.Volume;

				vwap.Add(cumulativeVolumePrice / cumulativeVolume);
			}

			_vwap = [.. vwap];
		}

		private void DrawPriceLevel(IDrawingContext context, Point pointA, Point pointB, Color color, int thickness, LineStyle lineStyle)
		{
			context.DrawLine(pointA, pointB, color, thickness, lineStyle);

			if (Settings.ShowPrices is false)
			{
				return;
			}

			var price = Symbol.RoundToTick(_script.ChartScale!.GetValueByYCoordinate(pointA.Y));
			var text = Symbol.FormatPrice(price);
			var textOrigin = new Point(pointA);

			if (Settings.RowsPlacement is PlacementType.Left)
			{
				textOrigin.X = pointB.X - context.MeasureText(text, Settings.Font).Width;
			}

			context.DrawText(textOrigin, text, color, Settings.Font);
		}

		private static void DrawColumn(IDrawingContext context, Point point, double width, double height, Color color, int lineThickness)
		{
			context.DrawRectangle(point, new Point(point.X + width, point.Y - height), color, null, lineThickness);
		}
	}

	public record BarsRange
	{
		public int FromIndex { get; set; }
		public int ToIndex { get; set; }
	}

	public record Volume
	{
		public double Buy { get; set; }
		public double Sell { get; set; }
		public double Total => Buy + Sell;
		public double Delta => Buy - Sell;

		public static implicit operator double(Volume volume) => volume.Total;
	}
}