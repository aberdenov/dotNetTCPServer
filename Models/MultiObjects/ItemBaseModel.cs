using System.Collections.Generic;

namespace Models
{
    public class ItemBaseModel
    {
        public string name { get; set; }
        public string characterClassId { get; set; }
        public IconModel icon { get; set; }
        public float itemLvl { get; set; }
        public string itemType { get; set; }
        public string quality { get; set; }
        public string characterClass { get; set; }
        public float price { get; set; }
        public List<ItemPropertyModel> properties { get; set; }
        public List<StoneModel> stones { get; set; }
        public float upgradeStoneLvl { get; set; }
        public float upgradeStioneQuantity { get; set; }
        public float reagentQuantity { get; set;}
        public float socketsQuantity;
    }
}