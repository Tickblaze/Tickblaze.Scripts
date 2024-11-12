namespace Tickblaze.Scripts.Drawings;

public class VolumeProfileExtended : VolumeProfile
{
	protected override bool ExtendRight => true;

	public VolumeProfileExtended()
	{
		Name = "Volume Profile - Realtime";
	}
}

public class VolumeProfile : Drawing
{
	public const string InputsGroupName = "Inputs";
	public const string StyleGroupName = "style";

	[Parameter("Histo Size Type", Description = "Determines how histogram rows are calculated (by count or ticks)", GroupName = InputsGroupName)]
	public RowsLayoutType RowsLayout { get; set; } = RowsLayoutType.Count;

	[Parameter("Histo Size Value", Description = "Defines the size of the histogram rows", GroupName = InputsGroupName)]
	public int RowsSize { get; set; } = 24;

	[NumericRange(0, 100)]
	[Parameter("Histo Width %", Description = "Width of the histogram as a percentage of box width", GroupName = StyleGroupName)]
	public double RowsWidthPercent { get; set; } = 30;

	[Parameter("Histo Location", Description = "Location of histogram (left or right side of box)", GroupName = StyleGroupName)]
	public PlacementType RowsPlacement { get; set; } = PlacementType.Left;

	[NumericRange(0, 100)]
	[Parameter("Value Area %", Description = "Percentage of total volume considered in the value area", GroupName = InputsGroupName)]
	public double ValueAreaPercent { get; set; } = 70;

	[Parameter("Value Area Color", Description = "Color of the value area", GroupName = StyleGroupName)]
	public Color ValueAreaColor { get; set; } = "#bf808080";

	[Parameter("Above Value Area Color", Description = "Color of area above the value area", GroupName = StyleGroupName)]
	public Color ValueAreaAboveColor { get; set; } = "#80ff0000";

	[Parameter("Below Value Area Color", Description = "Color of area below the value area", GroupName = StyleGroupName)]
	public Color ValueAreaBelowColor { get; set; } = "#80ff0000";

	[Parameter("Outline Color", Description = "Color of the volume profile outline box", GroupName = StyleGroupName)]
	public Color BoxLineColor { get; set; } = "#80ffffff";

	[Parameter("Outline Thickness", Description = "Thickness of the volume profile outline box", GroupName = StyleGroupName)]
	public int BoxLineThickness { get; set; } = 1;

	[Parameter("Outline Style", Description = "Style of the volume profile outline box (solid, dashed, etc.)", GroupName = StyleGroupName)]
	public LineStyle BoxLineStyle { get; set; } = LineStyle.Dot;

	[Parameter("VAH Line Visible?", Description = "Show/Hide the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public bool VahLineVisible { get; set; } = false;

	[Parameter("VAH Line Color", Description = "Color of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public Color VahLineColor { get; set; } = Color.White;

	[Parameter("VAH Line Thickness", Description = "Thickness of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public int VahLineThickness { get; set; } = 2;

	[Parameter("VAH Line Style", Description = "Style of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public LineStyle VahLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("VAL Line Visible?", Description = "Show/Hide the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public bool ValLineVisible { get; set; } = false;

	[Parameter("VAL Line Color", Description = "Color of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public Color ValLineColor { get; set; } = Color.White;

	[Parameter("VAL Line Thickness", Description = "Thickness of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public int ValLineThickness { get; set; } = 2;

	[Parameter("VAL Line Style", Description = "Style of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public LineStyle ValLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("POC Line Visible?", Description = "Show/Hide the Point of Control (POC) line", GroupName = StyleGroupName)]
	public bool PocLineVisible { get; set; } = true;

	[Parameter("POC Line Color", Description = "Color of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public Color PocLineColor { get; set; } = Color.Yellow;

	[Parameter("POC Line Thickness", Description = "Thickness of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public int PocLineThickness { get; set; } = 2;

	[Parameter("POC Line Style", Description = "Style of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public LineStyle PocLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Show VAH/VAL/POC Prices?", Description = "Displays prices for VAH, VAL, and POC levels")]
	public bool ShowPrices { get; set; } = false;

	[Parameter("Font", Description = "Font for displaying VAH/VAL/POC prices")]
	public Font Font { get; set; } = new("Arial", 12);

	public override int PointsCount => ExtendRight ? 1 : 2;

	protected virtual bool ExtendRight => false;

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

	private record Area()
	{
		public int FromIndex { get; set; }
		public int ToIndex { get; set; }
		public double High { get; set; } = double.MinValue;
		public double Low { get; set; } = double.MaxValue;
		public double Volume { get; set; }
		public double Range => High - Low;
		public double RowSize { get; set; }
		public int Rows { get; set; }
	}

	private record Volume()
	{
		public double Buy { get; set; }
		public double Sell { get; set; }
		public double Total => Buy + Sell;
		public double Delta => Buy - Sell;

		public static implicit operator double(Volume volume) => volume.Total;
	}

	private Area _area;
	private Volume[] _volumes;
	private int _pocIndex, _vahIndex, _valIndex;

	public VolumeProfile()
	{
		Name = "Volume Profile - Manual";
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		if (Points.Count >= PointsCount)
		{
			CalculateArea();
			AdjustAnchorPoints();
		}
	}

	private void AdjustAnchorPoints()
	{
		if (_area is null)
		{
			return;
		}

		var midPrice = (_area.High + _area.Low) / 2;

		foreach (var point in Points)
		{
			if (point.Value.Equals(midPrice) is false)
			{
				point.Value = midPrice;
			}
		}

		var firstPoint = Points.OrderBy(x => x.X).First();
		var lastBarTime = Chart.GetTimeByXCoordinate(Chart.GetXCoordinateByBarIndex(Bars.Count - 1));

		if ((DateTime)firstPoint.Time > lastBarTime)
		{
			firstPoint.Time = lastBarTime;
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		var area = CalculateArea();

		if (_area is null || _area != area)
		{
			_area = area;

			AdjustAnchorPoints();
			CalculateProfile(area);
		}

		Render(context);
	}

	private Area CalculateArea()
	{
		var area = new Area()
		{
			FromIndex = Chart.GetBarIndexByXCoordinate(Points[0].X),
			ToIndex = ExtendRight ? Bars.Count - 1 : Chart.GetBarIndexByXCoordinate(Points[1].X)
		};

		if (area.FromIndex > area.ToIndex)
		{
			(area.FromIndex, area.ToIndex) = (area.ToIndex, area.FromIndex);
		}

		for (var index = area.FromIndex; index <= area.ToIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				continue;
			}

			if (area.High < bar.High)
			{
				area.High = bar.High;
			}

			if (area.Low > bar.Low)
			{
				area.Low = bar.Low;
			}

			area.Volume += bar.Volume;
		}

		area.RowSize = Symbol.RoundToTick(RowsLayout is RowsLayoutType.Count
			? Math.Max(Symbol.TickSize, area.Range / RowsSize)
			: Symbol.TickSize * RowsSize);

		var rows = (int)Math.Round(area.Range / area.RowSize);
		var rowsMaximum = 500;

		if (rows > rowsMaximum)
		{
			area.RowSize = Symbol.RoundToTick(area.Range / rowsMaximum);
		}

		if (area.RowSize <= 0)
		{
			area.Rows = 0;
		}
		else
		{
			area.Low = Math.Floor(area.Low / area.RowSize) * area.RowSize;
			area.High = Math.Ceiling(area.High / area.RowSize) * area.RowSize;
			area.Rows = (int)Math.Round(area.Range / area.RowSize);
		}

		return area;
	}

	private void CalculateProfile(Area area)
	{
		if (area.Rows == 0)
		{
			return;
		}

		_volumes = new Volume[area.Rows];

		for (var index = area.FromIndex; index <= area.ToIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				continue;
			}

			var startLevel = Math.Max(0, (int)Math.Floor((bar.Low - area.Low) / area.RowSize));
			var endLevel = Math.Min(_volumes.Length - 1, (int)Math.Floor((bar.High - area.Low - Symbol.TickSize / 2) / area.RowSize));
			var volumePerLevel = bar.Volume / (endLevel - startLevel + 1);
			var buyVolume = 0.0;
			var sellVolume = 0.0;

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
				if (_volumes[level] is null)
				{
					_volumes[level] = new();
				}

				_volumes[level].Buy += buyVolume;
				_volumes[level].Sell += sellVolume;
			}
		}

		_pocIndex = 0;

		for (var i = 0; i < _volumes.Length; i++)
		{
			if (_volumes[i] is null)
			{
				_volumes[i] = new();
			}

			if (_volumes[_pocIndex] < _volumes[i])
			{
				_pocIndex = i;
			}
		}

		var accumulatedVolume = _volumes[_pocIndex].Total;
		var targetVolume = area.Volume * (ValueAreaPercent / 100);

		_vahIndex = _pocIndex;
		_valIndex = _pocIndex;

		while (accumulatedVolume < targetVolume)
		{
			var expanded = false;

			if (_valIndex > 0 && (_vahIndex == _volumes.Length - 1 || _volumes[_valIndex - 1] >= _volumes[_vahIndex + 1]))
			{
				accumulatedVolume += _volumes[--_valIndex];
				expanded = true;
			}

			if (_vahIndex < _volumes.Length - 1 && (_valIndex == 0 || _volumes[_vahIndex + 1] >= _volumes[_valIndex - 1]))
			{
				accumulatedVolume += _volumes[++_vahIndex];
				expanded = true;
			}

			if (expanded is false)
			{
				break;
			}
		}
	}

	private void Render(IDrawingContext context)
	{
		var area = _area;
		if (area.FromIndex > Chart.LastVisibleBarIndex || area.ToIndex < Chart.FirstVisibleBarIndex)
		{
			System.Diagnostics.Debug.WriteLine("Hidden");
			return;
		}

		var highY = ChartScale.GetYCoordinateByValue(area.High);
		var lowY = ChartScale.GetYCoordinateByValue(area.Low);
		var leftX = Chart.GetXCoordinateByBarIndex(area.FromIndex);
		var rightX = ExtendRight ? Chart.GetXCoordinateByBarIndex(area.ToIndex) : Math.Max(Points[0].X, Points[1].X);

		context.DrawRectangle(new Point(leftX, highY), new Point(rightX, lowY), null, BoxLineColor, BoxLineThickness, BoxLineStyle);

		if (area.Rows == 0)
		{
			return;
		}

		var pixelsPerUnitY = Math.Abs(highY - lowY) / area.Range;
		var adjustSpacing = area.RowSize * pixelsPerUnitY > 5;
		var lineThickness = adjustSpacing ? 2 : 1;
		var x = RowsPlacement is PlacementType.Left ? leftX : rightX;
		var boxWidth = RowsPlacement is PlacementType.Left ? rightX - leftX : leftX - rightX;

		for (var i = 0; i < _volumes.Length; i++)
		{
			var volume = _volumes[i];
			var y = lowY - i * area.RowSize * pixelsPerUnitY;
			if (y < 0)
			{
				break;
			}

			var volumeRatio = volume.Total / _volumes[_pocIndex].Total;
			var barWidth = boxWidth * (RowsWidthPercent / 100) * volumeRatio;
			var barHeight = area.RowSize * pixelsPerUnitY - (adjustSpacing ? 1 : 0);

			if (y - barHeight > Chart.Height)
			{
				continue;
			}

			if (PocLineVisible && i == _pocIndex)
			{
				var pointA = new Point(leftX, y - barHeight / 2);
				var pointB = new Point(rightX, pointA.Y);

				DrawPriceLevel(context, pointA, pointB, PocLineColor, PocLineThickness, PocLineStyle);
			}

			if (ValLineVisible && i == _valIndex)
			{
				var pointA = new Point(leftX, y);
				var pointB = new Point(rightX, pointA.Y);

				DrawPriceLevel(context, pointA, pointB, ValLineColor, ValLineThickness, ValLineStyle);
			}

			if (VahLineVisible && i == _vahIndex)
			{
				var pointA = new Point(leftX, y - barHeight);
				var pointB = new Point(rightX, pointA.Y);

				DrawPriceLevel(context, pointA, pointB, VahLineColor, VahLineThickness, VahLineStyle);
			}

			var color = i > _vahIndex ? ValueAreaAboveColor : i < _valIndex ? ValueAreaBelowColor : ValueAreaColor;

			DrawColumn(context, new(x, y), barWidth, barHeight, color, lineThickness);
		}
	}

	private void DrawPriceLevel(IDrawingContext context, IPoint pointA, IPoint pointB, Color color, int thickness, LineStyle lineStyle)
	{
		context.DrawLine(pointA, pointB, color, thickness, lineStyle);

		if (ShowPrices is false)
		{
			return;
		}

		var price = Symbol.RoundToTick(ChartScale.GetValueByYCoordinate(pointA.Y));
		var text = Symbol.FormatPrice(price);
		var textOrigin = new Point(pointA);

		if (RowsPlacement is PlacementType.Left)
		{
			textOrigin.X = pointB.X - context.MeasureText(text, Font).Width;
		}

		context.DrawText(textOrigin, text, color, Font);
	}

	private static void DrawColumn(IDrawingContext context, Point point, double width, double height, Color color, int lineThickness)
	{
		context.DrawRectangle(point, new Point(point.X + width, point.Y - height), color, null, lineThickness);
	}
}
