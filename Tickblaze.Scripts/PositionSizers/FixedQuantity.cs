using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.PositionSizers;

/// <summary>
/// This position sizing script assigns a fixed size to each order.
/// </summary>
public class FixedQuantity : PositionSizer
{
	[Parameter("Quantity per order", Description = "The fixed quantity of shares / contracts / units for each order.")]
	public double QuantityPerOrder { get; set; } = 100;

	[Parameter("Enable exit sizing", Description = "Use to indicate whether to enable the position sizing script to determine the size of exit orders.")]
	public bool EnableExitSizing { get; set; } = true;

	public FixedQuantity()
	{
		Name = "Fixed Quantity";
		ShortName = "FQ";
		Description = "This position sizing script assigns a fixed size to each order.";
	}

	protected override double GetPositionSize(IOrder order)
	{
		if (EnableExitSizing && Position is not null)
		{
			if (Position.Quantity < order.Quantity)
			{
				return order.Quantity;
			}

			return Position.Quantity;
		}

		return QuantityPerOrder;
	}
}
