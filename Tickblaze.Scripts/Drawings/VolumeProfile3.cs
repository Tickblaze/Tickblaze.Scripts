namespace Tickblaze.Scripts.Drawings;

public class VolumeProfile3 : Drawing
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

	[Parameter("Extend Right", GroupName = InputsGroupName)]
	public bool ExtendRight { get; set; } = false;

	[NumericRange(0, 100)]
	[Parameter("Width (% of the box)", GroupName = StyleGroupName)]
	public double WidthPercent { get; set; } = 30;

	[Parameter("Placement", GroupName = StyleGroupName)]
	public PlacementType Placement { get; set; } = PlacementType.Left;

	[Parameter("Up Volume", GroupName = StyleGroupName)]
	public Color UpVolumeColor { get; set; } = "#8026c6da";

	[Parameter("Down Volume", GroupName = StyleGroupName)]
	public Color DownVolumeColor { get; set; } = "#80ec407a";

	[Parameter("Value Area Up", GroupName = StyleGroupName)]
	public Color ValueAreaUpColor { get; set; } = "#bf26c6da";

	[Parameter("Value Area Down", GroupName = StyleGroupName)]
	public Color ValueAreaDownColor { get; set; } = "#bfec407a";

	public override int PointsCount => 2;

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

		public static implicit operator double(Volume volume) => volume.Total;
	}

	private Area _area;
	private int _pocIndex, _vahIndex, _valIndex;

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		if (Points.Count >= PointsCount)
		{
			CalculateArea();
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		var area = CalculateArea();

		if (_area is null || _area != area)
		{
			_area = area;
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

		if (area.RowSize <= 0)
		{
			area.Volumes = [];
		}
		else
		{
			area.Low = Math.Floor(area.Low / area.RowSize) * area.RowSize;
			area.High = Math.Ceiling(area.High / area.RowSize) * area.RowSize;
			area.Volumes = new Volume[(int)Math.Round(area.Range / area.RowSize)];

			if (Points[0].Value.Equals(area.High) is false)
			{
				Points[0].Value = area.High;
			}

			if (Points[1].Value.Equals(area.Low) is false)
			{
				Points[1].Value = area.Low;
			}
		}

		return area;
	}

	private void CalculateProfile(Area area)
	{
		for (var index = area.FromIndex; index <= area.ToIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				continue;
			}

			var startLevel = Math.Max(0, (int)Math.Round((bar.Low - area.Low) / area.RowSize));
			var endLevel = Math.Min(area.Volumes.Length - 1, (int)Math.Round((bar.High - area.Low) / area.RowSize));
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

		var targetVolume = area.Volume * (ValueAreaPercent / 100);
		var accumulatedVolume = area.Volumes[_pocIndex].Total;

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
		var pixelsPerUnitY = Math.Abs(highY - lowY) / _area.Range;
		var adjustSpacing = area.RowSize * pixelsPerUnitY >= 2;
		var lineThickness = adjustSpacing ? 2 : 1;
		var pointA = new Point(Chart.GetXCoordinateByBarIndex(_area.FromIndex), highY);
		var pointB = new Point(Chart.GetXCoordinateByBarIndex(_area.ToIndex), lowY);

		context.DrawRectangle(pointA, pointB, null, Color.White);

		for (var i = 0; i < area.Volumes.Length; i++)
		{
			var volume = area.Volumes[i];
			var y = lowY - i * area.RowSize * pixelsPerUnitY;
			var volumeRatio = volume / area.Volumes[_pocIndex];
			var barWidth = (pointB.X - pointA.X) * WidthPercent / 100 * volumeRatio;

			var startPoint = new Point(pointA.X, y);
			var endPoint = new Point(pointA.X + barWidth, y - area.RowSize * pixelsPerUnitY + (adjustSpacing ? 1 : 0));
			var isValueArea = i >= _valIndex && i <= _vahIndex;

			if (VolumeDisplay is VolumeType.UpDown)
			{
				var buyWidth = barWidth * (volume.Buy / volume.Total);
				var buyColor = isValueArea ? ValueAreaUpColor : UpVolumeColor;
				var sellColor = isValueArea ? ValueAreaDownColor : DownVolumeColor;

				context.DrawRectangle(startPoint, new Point(startPoint.X + buyWidth, endPoint.Y), buyColor, null, lineThickness);
				context.DrawRectangle(new Point(startPoint.X + buyWidth, startPoint.Y), endPoint, sellColor, null, lineThickness);
			}
			else if (VolumeDisplay is VolumeType.Total)
			{
				var fillColor = isValueArea ? ValueAreaUpColor : UpVolumeColor;

				context.DrawRectangle(startPoint, endPoint, fillColor, null, lineThickness);
			}
		}
	}
}
