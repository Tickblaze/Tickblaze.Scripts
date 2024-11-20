namespace Tickblaze.Scripts.Drawings;

public sealed class ExtendedLine : TrendLine
{
	public ExtendedLine()
	{
		Name = "Extended Line";
		ExtendLeft = true;
		ExtendRight = true;
	}
}