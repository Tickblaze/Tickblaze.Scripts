namespace Tickblaze.Scripts.Drawings;

public class VolumeProfileExtended : VolumeProfile
{
	protected override bool ExtendRight => true;

	public VolumeProfileExtended()
	{
		Name = "Volume Profile - Extended";
	}
}

public class VolumeProfile : Drawing
{
	public const string InputsGroupName = "Inputs";
	public const string StyleGroupName = "style";

	[Parameter("Rows Layout", GroupName = InputsGroupName)]
	public RowsLayoutType RowsLayout { get; set; } = RowsLayoutType.NumberOfRows;

	[Parameter("Row Size", GroupName = InputsGroupName)]
	public int RowSize { get; set; } = 24;

	[Parameter("Volume")]
	public VolumeType VolumeDisplay { get; set; } = VolumeType.UpDown;

	[NumericRange(0, 100)]
	[Parameter("Value Area %", GroupName = InputsGroupName)]
	public double ValueAreaPercent { get; set; } = 70;

	[NumericRange(0, 100)]
	[Parameter("Width (% of the box)", GroupName = StyleGroupName)]
	public double WidthPercent { get; set; } = 30;

	[Parameter("Placement", GroupName = StyleGroupName)]
	public PlacementType Placement { get; set; } = PlacementType.Left;

	[Parameter("Box Line Color", GroupName = StyleGroupName)]
	public Color BoxLineColor { get; set; } = "#80ffffff";

	[Parameter("Box Line Thickness", GroupName = StyleGroupName)]
	public int BoxLineThickness { get; set; } = 1;

	[Parameter("Box Line Style", GroupName = StyleGroupName)]
	public LineStyle BoxLineStyle { get; set; } = LineStyle.Dot;

	[Parameter("Up Volume", GroupName = StyleGroupName)]
	public Color UpVolumeColor { get; set; } = "#8026c6da";

	[Parameter("Down Volume", GroupName = StyleGroupName)]
	public Color DownVolumeColor { get; set; } = "#80ec407a";

	[Parameter("Value Area Up", GroupName = StyleGroupName)]
	public Color ValueAreaUpColor { get; set; } = "#bf26c6da";

	[Parameter("Value Area Down", GroupName = StyleGroupName)]
	public Color ValueAreaDownColor { get; set; } = "#bfec407a";

	[Parameter("VAH Line Visible?", GroupName = StyleGroupName)]
	public bool VahLineVisible { get; set; } = false;

	[Parameter("VAH Line Color", GroupName = StyleGroupName)]
	public Color VahLineColor { get; set; } = Color.White;

	[Parameter("VAH Line Thickness", GroupName = StyleGroupName)]
	public int VahLineThickness { get; set; } = 2;

	[Parameter("VAH Line Style", GroupName = StyleGroupName)]
	public LineStyle VahLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("VAL Line Visible?", GroupName = StyleGroupName)]
	public bool ValLineVisible { get; set; } = false;

	[Parameter("VAL Line Color", GroupName = StyleGroupName)]
	public Color ValLineColor { get; set; } = Color.White;

	[Parameter("VAL Line Thickness", GroupName = StyleGroupName)]
	public int ValLineThickness { get; set; } = 2;

	[Parameter("VAL Line Style", GroupName = StyleGroupName)]
	public LineStyle ValLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("POC Line Visible?", GroupName = StyleGroupName)]
	public bool PocLineVisible { get; set; } = true;

	[Parameter("POC Line Color", GroupName = StyleGroupName)]
	public Color PocLineColor { get; set; } = Color.White;

	[Parameter("POC Line Thickness", GroupName = StyleGroupName)]
	public int PocLineThickness { get; set; } = 2;

	[Parameter("POC Line Style", GroupName = StyleGroupName)]
	public LineStyle PocLineStyle { get; set; } = LineStyle.Solid;

	public override int PointsCount => ExtendRight ? 1 : 2;

	protected virtual bool ExtendRight => false;

	public enum RowsLayoutType
	{
		NumberOfRows,
		TicksPerRow
	}

	public enum VolumeType
	{
		UpDown,
		Total,
		Delta
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
		public Volume[] Volumes { get; set; }
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
	private int _pocIndex, _vahIndex, _valIndex;

	public VolumeProfile()
	{
		Name = "Volume Profile";
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

		area.RowSize = Symbol.RoundToTick(RowsLayout is RowsLayoutType.NumberOfRows
			? Math.Max(Symbol.TickSize, area.Range / RowSize)
			: Symbol.TickSize * RowSize);

		var rows = (int)Math.Round(area.Range / area.RowSize);
		var rowsMaximum = 2_000;

		if (rows > rowsMaximum)
		{
			area.RowSize = Symbol.RoundToTick(area.Range / rowsMaximum);
			rows = rowsMaximum;
		}

		if (area.RowSize <= 0)
		{
			area.Volumes = [];
		}
		else
		{
			area.Low = Math.Floor(area.Low / area.RowSize) * area.RowSize;
			area.High = Math.Ceiling(area.High / area.RowSize) * area.RowSize;
			area.Volumes = new Volume[rows];
		}

		return area;
	}

	private void CalculateProfile(Area area)
	{
		if (area.Volumes.Length == 0)
		{
			return;
		}

		for (var index = area.FromIndex; index <= area.ToIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				continue;
			}

			var startLevel = Math.Max(0, (int)Math.Round((bar.Low - area.Low) / area.RowSize));
			var endLevel = Math.Min(area.Volumes.Length - 1, (int)Math.Floor((bar.High - area.Low - Symbol.TickSize / 2) / area.RowSize));
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
				if (area.Volumes[level] is null)
				{
					area.Volumes[level] = new();
				}

				area.Volumes[level].Buy += buyVolume;
				area.Volumes[level].Sell += sellVolume;
			}
		}

		_pocIndex = 0;

		for (var i = 0; i < area.Volumes.Length; i++)
		{
			if (area.Volumes[i] is null)
			{
				area.Volumes[i] = new();
			}

			if (area.Volumes[_pocIndex] < area.Volumes[i])
			{
				_pocIndex = i;
			}
		}

		var accumulatedVolume = area.Volumes[_pocIndex].Total;
		var targetVolume = area.Volume * (ValueAreaPercent / 100);

		_vahIndex = _pocIndex;
		_valIndex = _pocIndex;

		while (accumulatedVolume < targetVolume)
		{
			var expanded = false;

			if (_valIndex > 0 && (_vahIndex == area.Volumes.Length - 1 || area.Volumes[_valIndex - 1] >= area.Volumes[_vahIndex + 1]))
			{
				accumulatedVolume += area.Volumes[--_valIndex];
				expanded = true;
			}

			if (_vahIndex < area.Volumes.Length - 1 && (_valIndex == 0 || area.Volumes[_vahIndex + 1] >= area.Volumes[_valIndex - 1]))
			{
				accumulatedVolume += area.Volumes[++_vahIndex];
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
		var highY = ChartScale.GetYCoordinateByValue(_area.High);
		var lowY = ChartScale.GetYCoordinateByValue(_area.Low);
		var leftX = Chart.GetXCoordinateByBarIndex(_area.FromIndex);
		var rightX = Chart.GetXCoordinateByBarIndex(_area.ToIndex);

		context.DrawRectangle(new Point(leftX, highY), new Point(rightX, lowY), null, BoxLineColor, BoxLineThickness, BoxLineStyle);

		if (area.Volumes.Length == 0)
		{
			return;
		}

		var pixelsPerUnitY = Math.Abs(highY - lowY) / _area.Range;
		var adjustSpacing = area.RowSize * pixelsPerUnitY > 5;
		var lineThickness = adjustSpacing ? 2 : 1;
		var x = Placement is PlacementType.Left ? leftX : rightX;
		var boxWidth = Placement is PlacementType.Left ? rightX - leftX : leftX - rightX;

		for (var i = 0; i < area.Volumes.Length; i++)
		{
			var volume = area.Volumes[i];
			var y = lowY - i * area.RowSize * pixelsPerUnitY;
			var volumeRatio = volume.Total / area.Volumes[_pocIndex].Total;
			var barWidth = boxWidth * (WidthPercent / 100) * volumeRatio;
			var barHeight = area.RowSize * pixelsPerUnitY - (adjustSpacing ? 1 : 0);
			var isValueArea = i >= _valIndex && i <= _vahIndex;

			if (PocLineVisible && i == _pocIndex)
			{
				var pointA = new Point(leftX, y - barHeight / 2);
				var pointB = new Point(rightX, pointA.Y);

				context.DrawLine(pointA, pointB, PocLineColor, PocLineThickness, PocLineStyle);
			}

			if (ValLineVisible && i == _valIndex)
			{
				var pointA = new Point(leftX, y);
				var pointB = new Point(rightX, pointA.Y);

				context.DrawLine(pointA, pointB, PocLineColor, PocLineThickness, PocLineStyle);
			}

			if (VahLineVisible && i == _vahIndex)
			{
				var pointA = new Point(leftX, y - barHeight);
				var pointB = new Point(rightX, pointA.Y);

				context.DrawLine(pointA, pointB, PocLineColor, PocLineThickness, PocLineStyle);
			}

			if (VolumeDisplay is VolumeType.Total)
			{
				var fillColor = isValueArea ? ValueAreaUpColor : UpVolumeColor;

				DrawColumn(context, new(x, y), barWidth, barHeight, fillColor, lineThickness);
			}
			else
			{
				var buyWidth = barWidth * (volume.Buy / volume.Total);
				var sellWidth = barWidth * (volume.Sell / volume.Total);
				var buyColor = isValueArea ? ValueAreaUpColor : UpVolumeColor;
				var sellColor = isValueArea ? ValueAreaDownColor : DownVolumeColor;

				if (VolumeDisplay is VolumeType.UpDown)
				{
					DrawColumn(context, new(x, y), buyWidth, barHeight, buyColor, lineThickness);
					DrawColumn(context, new(x + buyWidth, y), sellWidth, barHeight, sellColor, lineThickness);
				}
				else if (VolumeDisplay is VolumeType.Delta)
				{
					var point = new Point(x, y);
					var deltaWidth = barWidth * Math.Abs(volume.Delta / volume.Total);
					var color = GetColorWithOpacity(volume.Delta > 0 ? buyColor : sellColor, 0.5);

					DrawColumn(context, point, barWidth, barHeight, color, lineThickness);
					DrawColumn(context, point, buyWidth, barHeight, color, lineThickness);
					DrawColumn(context, point, deltaWidth, barHeight, color, lineThickness);
				}
			}
		}
	}

	private static Color GetColorWithOpacity(Color color, double opacity)
	{
		return new((byte)Math.Round(color.A * opacity), color.R, color.G, color.B);
	}

	private static void DrawColumn(IDrawingContext context, Point point, double width, double height, Color color, int lineThickness)
	{
		context.DrawRectangle(point, new Point(point.X + width, point.Y - height), color, null, lineThickness);
	}
}
