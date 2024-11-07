namespace Tickblaze.Scripts.Drawings;

public class VolumeProfile3 : Drawing
{
	[Parameter("Rows Layout")]
	public RowsLayoutType RowsLayout { get; set; }

	[Parameter("Row Size")]
	public int RowSize { get; set; } = 24;

	[Parameter("Width %"), NumericRange(0, 100)]
	public double WidthPercent { get; set; } = 30;

	[Parameter("Value Area %"), NumericRange(0, 100)]
	public double ValueAreaPercent { get; set; } = 70;

	[Parameter("Extend Right")]
	public bool ExtendRight { get; set; }

	public override int PointsCount => 2;

	public enum RowsLayoutType
	{
		NumberOfRows,
		TicksPerRow
	}

	private record Area()
	{
		public int FromIndex { get; set; }
		public int ToIndex { get; set; }
		public double High { get; set; } = double.MinValue;
		public double Low { get; set; } = double.MaxValue;
		public double Volume { get; set; }
		public double Range => High - Low;
	}

	private Area _area;
	private double _rowSize;
	private double[] _volumes;
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

		if (Points[0].Value.Equals(area.High) is false)
		{
			Points[0].Value = area.High;
		}

		if (Points[1].Value.Equals(area.Low) is false)
		{
			Points[1].Value = area.Low;
		}

		return area;
	}

	private void CalculateProfile(Area area)
	{
		_rowSize = RowsLayout is RowsLayoutType.NumberOfRows
			? Math.Max(Symbol.TickSize, area.Range / RowSize)
			: Symbol.TickSize * RowSize;

		var rows = (int)Math.Round(area.Range / _rowSize);
		_volumes = new double[rows];
	}

	private void Render(IDrawingContext context)
	{
		var highY = ChartScale.GetYCoordinateByValue(_area.High);
		var lowY = ChartScale.GetYCoordinateByValue(_area.Low);
		var pixelsPerUnitY = Math.Abs(highY - lowY) / _area.Range;
		var pointA = new Point(Chart.GetXCoordinateByBarIndex(_area.FromIndex), highY);
		var pointB = new Point(Chart.GetXCoordinateByBarIndex(_area.ToIndex), lowY);

		context.DrawRectangle(pointA, pointB, null, Color.Red);

		for (var i = 0; i < _volumes.Length; i++)
		{
			var y = pointA.Y + _rowSize * pixelsPerUnitY * i;
			context.DrawLine(new Point(pointA.X, y), new Point(pointB.X, y), Color.White);
		}

		//var separateRows = _volumes.Length < Math.Abs(highY - lowY) / 3;

		//for (var i = 0; i < _volumes.Length; i++)
		//{
		//	var level = _volumes[i];
		//	if (level is null)
		//	{
		//		continue;
		//	}

		//	var volumeRatio = _pocIndex.Volume > 0 ? level.Volume / _pocIndex.Volume : 0;
		//	var width = (pointB.X - pointA.X) * volumeRatio * WidthPercent / 100;

		//	var y = ChartScale.GetYCoordinateByValue(level.Price);
		//	if (y > Chart.Height)
		//	{
		//		continue;
		//	}

		//	var startPoint = new Point(pointA.X, y);
		//	var endPoint = new Point(pointA.X + width, y);

		//	var color = level == _pocIndex ? Color.Red : level == _high ? Color.White : Color.Gray;
		//	var alpha = (byte)(_valIndex.Index <= level.Index && level.Index <= _vahIndex.Index ? 128 : 64);

		//	color = new(alpha, color.R, color.G, color.B);
		//	endPoint.Y += Math.Max(0, _rowSize * pixelsPerUnitY - (separateRows ? 1 : 0));

		//	if (endPoint.Y < 0)
		//	{
		//		break;
		//	}

		//	context.DrawRectangle(startPoint, endPoint, color, Color.Black, 0);
		//}

		//context.DrawText(new Point(0, 0), $"Row size: {_rowSize}\nRow size px:{_rowSize * pixelsPerUnitY}\nRows: {_volumes.Length}", Color.White);
	}
}
