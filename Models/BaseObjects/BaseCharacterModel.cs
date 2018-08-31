namespace Models
{
    public class BaseCharacterModel
    {
        public string _id { get; set; }
        public string characterClassId { get; set; }
        public bool isMonster { get; set; }
        public bool isRange { get; set; }
        public string userId { get; set; }
        public string charClass { get; set; }
        public string name { get; set; }
        public float characterLvl { get; set; }
        public float hp { get; set; }
        public float mp { get; set; }
        public float minDamage { get; set; }
        public float maxDamage { get; set; }
        public float armor { get; set; }
        public float strength { get; set; }
        public float agility { get; set; }
        public float intelligence { get; set; }
        public float stamina { get; set; }
        public float accuracy { get; set; }
        public float evasion { get; set; }
        public float critChance { get; set; }
        public float critDamage { get; set; }
        public float regenHp { get; set; }
        public float regenMp { get; set; }

    }
}