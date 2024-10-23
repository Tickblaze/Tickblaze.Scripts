using System.ComponentModel;
using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.PositionSizers;

/// <summary>
/// This position sizing script assigns a fixed size to each order.
/// </summary>
[Browsable(false)]
public class FixedSize : PositionSizer
{
	[Parameter("Quantity per order", Description = "The fixed quantity of shares / contracts / units for each order.")]
	public double QuantityPerOrder { get; set; } = 100;

	[Parameter("Enable exit sizing", Description = "Use to indicate whether to enable the position sizing script to determine the size of exit orders.")]
	public bool EnableExitSizing { get; set; } = true;

	public FixedSize()
	{
		Name = "Fixed Size";
		ShortName = "FS";
	}

	protected override double GetPositionSize(IOrder order)
	{
		if (EnableExitSizing)
		{
			foreach (var position in Positions)
			{
				if (position.Status is PositionStatus.Close || position.Direction == order.Direction)
				{
					continue;
				}

				if (position.Quantity < order.Quantity)
				{
					return order.Quantity;
				}

				return position.Quantity;
			}
		}

		return QuantityPerOrder;
	}
}
