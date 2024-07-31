namespace PokemonAPI.Models
{
    public class HabilidadResponse
    {
        public List<string> Ocultas { get; set; }
    }

    public class PokemonHabilidadResponse
    {
        public HabilidadResponse Habilidades { get; set; }
    }
}