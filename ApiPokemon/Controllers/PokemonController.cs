using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PokemonAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PokemonController> _logger;

        public PokemonController(HttpClient httpClient, ILogger<PokemonController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet("habilidadesOcultas/")]
        public async Task<IActionResult> GetHabilidadesOcultas(string pokemon)
        {
            if (string.IsNullOrWhiteSpace(pokemon) || !Regex.IsMatch(pokemon, @"^[a-zA-Z]+$"))
            {
                _logger.LogWarning("Solicitud inválida: el nombre del Pokémon no es válido.");
                return BadRequest("El nombre del Pokémon no es válido.");
            }

            try
            {
                _logger.LogInformation("Consultando habilidades ocultas para el Pokémon: {Pokemon}", pokemon);
                var apiUrl = $"https://pokeapi.co/api/v2/pokemon/{pokemon.ToLower()}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No se encontró el Pokémon: {Pokemon}", pokemon);
                    return NotFound();
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var habilidadesOcultas = json["abilities"]
                    .Where(ability => (bool)ability["is_hidden"])
                    .Select(ability => ability["ability"]["name"].ToString())
                    .ToList();

                _logger.LogInformation("Habilidades ocultas encontradas para el Pokémon: {Pokemon}", pokemon);
                return Ok(habilidadesOcultas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar las habilidades ocultas para el Pokémon: {Pokemon}", pokemon);
                return StatusCode(500, "Se produjo un error interno en el servidor.");
            }
        }
    }
}