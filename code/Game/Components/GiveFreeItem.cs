using Sandbox;

namespace Home;

[Title("Home - Give Free Item")]
[Category("Home")]
[Icon("local_atm", "red", "white")]
public sealed class GiveFreeItem : BaseComponent
{
	[Property] string ItemId { get; set; } = "";
	[Property] int Amount { get; set; } = 1;

	public override void OnAwake()
	{
		var interactable = GetComponent<Interactable>();
		if (interactable is not null)
		{
			interactable.OnInteract += GiveItem;
		}
	}

	public void GiveItem()
	{
		var player = HomePlayer.Local;
		if (player is not null)
		{
			var inventory = player.GetComponent<Inventory>();
			if (inventory is not null)
			{
				inventory.AddItem(ItemId, Amount);
			}
		}
	}
}