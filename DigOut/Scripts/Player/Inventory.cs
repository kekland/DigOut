using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigOut
{
	public class Inventory
	{
		public int Slots;

		public Item[] Items;

		public Inventory(int slots)
		{
			Slots = slots;
		}

		public void AddItem(int itemID, int Count)
		{
			for(int i = 0; i < Slots; i++)
			{
				if(Items[i].ItemID == itemID)
				{
					Items[i].Amount += Count;
					return;
				}
				else if(Items[i].ItemID == 0 && Items[i].Amount == 0)
				{
					Items[i].ItemID = itemID;
					Items[i].Amount = Count;
					return;
				}
			}
		}
	}
	public class Item
	{
		public int ItemID;
		public int Amount;
	}
}
