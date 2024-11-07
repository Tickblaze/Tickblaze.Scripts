namespace Tickblaze.Scripts.Drawings;

public class VolumeProfile : Drawing
{
	[Parameter("Width %"), NumericRange(0, 100)]
	public double WidthPercent { get; set; } = 100;

	[Parameter("Value Area %"), NumericRange(0, 100)]
	public double ValueAreaPercent { get; set; } = 70;

	public override int PointsCount => 2;

	private int FromIndex { get; set; } = -1;
	private int ToIndex { get; set; } = -1;
	private double RowSize { get; set; }
	private PriceLevel[] Levels { get; set; } = [];
	private PriceLevel High { get; set; }
	private PriceLevel Low { get; set; }
	private PriceLevel PointOfControl { get; set; }
	private PriceLevel ValueAreaHigh { get; set; }
	private PriceLevel ValueAreaLow { get; set; }

	private record PriceLevel(int Index, double Price)
	{
		public double Volume { get; set; }
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		//return;
		if (Points.Count < PointsCount)
		{
			return;
		}

		var fromIndex = Chart.GetBarIndexByXCoordinate(Points[0].X);
		var toIndex = Chart.GetBarIndexByXCoordinate(Points[1].X);

		if (fromIndex > toIndex)
		{
			(fromIndex, toIndex) = (toIndex, fromIndex);
		}

		if (fromIndex != FromIndex || toIndex != ToIndex)
		{
			Calculate(fromIndex, toIndex);
		}

		var midPrice = (High.Price + Low.Price) / 2;
		if (midPrice.Equals(Points[0].Value) is false || midPrice.Equals(Points[1].Value) is false)
		{
			Points[0].Value = Points[1].Value = midPrice;
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		var fromIndex = Chart.GetBarIndexByXCoordinate(Points[0].X);
		var toIndex = Chart.GetBarIndexByXCoordinate(Points[1].X);

		if (fromIndex > toIndex)
		{
			(fromIndex, toIndex) = (toIndex, fromIndex);
		}

		Calculate(fromIndex, toIndex);

		if (Levels?.Length > 0)
		{
			Render(context);
		}
	}

	private void Calculate(int fromIndex, int toIndex)
	{
		var high = double.MinValue;
		var low = double.MaxValue;

		for (var index = fromIndex; index <= toIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				return;
			}

			high = Math.Max(high, bar.High);
			low = Math.Min(low, bar.Low);
		}

		var range = high - low;
		var highY = ChartScale.GetYCoordinateByValue(high);
		var lowY = ChartScale.GetYCoordinateByValue(low);
		var pixelsPerUnitY = Math.Abs(highY - lowY) / range;
		var priceIncrement = Math.Max(Symbol.TickSize, Math.Pow(10, -Symbol.Decimals));
		var rowSize = priceIncrement * Math.Ceiling(1 / (priceIncrement * pixelsPerUnitY));

		var count = (int)(range / rowSize) + 1;
		var levels = new PriceLevel[count];
		var volumeTotal = 0.0;

		for (var index = fromIndex; index <= toIndex; index++)
		{
			var bar = Bars[index];
			if (bar is null)
			{
				continue;
			}

			var priceLevelsInBar = (int)((bar.High - bar.Low) / rowSize) + 1;
			var volumePerPriceLevel = bar.Volume / priceLevelsInBar;

			for (var price = bar.Low; price <= bar.High + rowSize / 2; price += rowSize)
			{
				var priceIndex = Math.Clamp((int)((price - low) / rowSize), 0, count - 1);

				if (levels[priceIndex] == null)
				{
					levels[priceIndex] = new(priceIndex, price);
				}

				levels[priceIndex].Volume += volumePerPriceLevel;
				volumeTotal += volumePerPriceLevel;
			}
		}

		FromIndex = fromIndex;
		ToIndex = toIndex;
		Levels = levels;
		RowSize = rowSize;
		High = null;
		Low = null;
		PointOfControl = null;

		foreach (var level in levels)
		{
			if (level is null)
			{
				continue;
			}

			if (High is null || High.Price < level.Price)
			{
				High = level;
			}

			if (Low is null || Low.Price > level.Price)
			{
				Low = level;
			}

			if (PointOfControl is null || PointOfControl.Volume < level.Volume)
			{
				PointOfControl = level;
			}
		}

		// Step 4: Calculate Value Area based on target percentage (e.g., 70%)
		var valueAreaVolume = PointOfControl.Volume;
		var valueAreaVolumeThreshold = volumeTotal * ValueAreaPercent / 100;

		ValueAreaLow = PointOfControl;
		ValueAreaHigh = PointOfControl;

		while (valueAreaVolume < valueAreaVolumeThreshold)
		{
			var expanded = false;

			if (ValueAreaLow != Low && (ValueAreaHigh == High || levels[ValueAreaLow.Index - 1]?.Volume >= levels[ValueAreaHigh.Index + 1]?.Volume))
			{
				ValueAreaLow = levels[ValueAreaLow.Index - 1];

				valueAreaVolume += ValueAreaLow?.Volume ?? 0;
				expanded = true;
			}

			if (ValueAreaHigh != High && (ValueAreaLow == Low || levels[ValueAreaHigh.Index + 1]?.Volume >= levels[ValueAreaLow.Index - 1]?.Volume))
			{
				ValueAreaHigh = Levels[ValueAreaHigh.Index + 1];

				valueAreaVolume += ValueAreaHigh?.Volume ?? 0;
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
		var highY = ChartScale.GetYCoordinateByValue(High.Price);
		var lowY = ChartScale.GetYCoordinateByValue(Low.Price);
		var pixelsPerUnitY = Math.Abs(highY - lowY) / (High.Price - Low.Price);
		var pointA = new Point(Chart.GetXCoordinateByBarIndex(FromIndex), highY);
		var pointB = new Point(Chart.GetXCoordinateByBarIndex(ToIndex), lowY);

		if (true)
		{
			pointA.X = Chart.GetXCoordinateByBarIndex(Chart.GetBarIndexByXCoordinate(Points[0].X));
			pointB.X = Chart.GetXCoordinateByBarIndex(Chart.GetBarIndexByXCoordinate(Points[1].X));
		}

		context.DrawRectangle(pointA, pointB, null, Color.Red);

		var separateRows = Levels.Length < Math.Abs(highY - lowY) / 3;

		for (var i = 0; i < Levels.Length; i++)
		{
			var level = Levels[i];
			if (level is null)
			{
				continue;
			}

			var volumeRatio = PointOfControl.Volume > 0 ? level.Volume / PointOfControl.Volume : 0;
			var width = (pointB.X - pointA.X) * volumeRatio * WidthPercent / 100;

			var y = ChartScale.GetYCoordinateByValue(level.Price + (RowSize / 2));
			if (y > Chart.Height)
			{
				continue;
			}

			var startPoint = new Point(pointA.X, y);
			var endPoint = new Point(pointA.X + width, y);

			var color = level == PointOfControl ? Color.Red : level == High ? Color.White : Color.Gray;
			var alpha = (byte)(ValueAreaLow.Index <= level.Index && level.Index <= ValueAreaHigh.Index ? 128 : 64);

			color = new(alpha, color.R, color.G, color.B);
			endPoint.Y += Math.Max(0, RowSize * pixelsPerUnitY - (separateRows ? 1 : 0));

			if (endPoint.Y < 0)
			{
				break;
			}

			context.DrawRectangle(startPoint, endPoint, color, Color.Black, 0);
		}

		context.DrawText(new Point(0, 0), $"{RowSize * pixelsPerUnitY}", Color.White);
	}
}
