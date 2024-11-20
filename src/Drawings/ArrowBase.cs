namespace Tickblaze.Scripts.Drawings;

public abstract class ArrowBase : Drawing
{
	[Parameter("Color", Description = "Arrow fill color")]
	public Color Color { get; set; } = Color.Red;

	[Parameter("Size", Description = "Arrow size, from 6 to 100"), NumericRange(6, 100)]
	public int Size { get; set; } = 18;

	protected ArrowType Type { get; set; }

	protected enum ArrowType
	{
		Up,
		Down
	}

	public override int PointsCount => 1;

	public override void OnRender(IDrawingContext context)
	{
		var pointWidth = Size / 2;
		var pointHeight = Size / 2;
		var wickWidth = Size / 6;
		var arrowSize = Size;
		var direction = Type is ArrowType.Up ? 1 : -1;

		var x = Points[0].X;
		var y = Points[0].Y + 2 * direction;

		var points = new Point[]
		{
			new(x, y),
			new(x - pointWidth, y + pointHeight * direction),
			new(x - wickWidth, y + pointHeight * direction),
			new(x - wickWidth, y + arrowSize * direction),
			new(x + wickWidth, y + arrowSize * direction),
			new(x + wickWidth, y + pointHeight * direction),
			new(x + pointWidth, y + pointHeight * direction),
		};

		context.DrawPolygon(points, Color);
	}
}
