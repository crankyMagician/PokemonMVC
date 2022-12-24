namespace Pokemon.Models
{
    /*
     * A model with the pokemon stats 
     */
    public class PokemonModel
    {
        public int Id { get; set; }
        public string PName { get; set; }
        public string TypeOne { get; set; }
        public string? TypeTwo { get; set; }
        public int Total { get; set; }
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpAttack { get; set; }
        public int SpDefense { get; set; }
        public int Gen { get; set; }
        public int Speed { get; set; }
        public bool Legendary { get; set; }
    }
}
