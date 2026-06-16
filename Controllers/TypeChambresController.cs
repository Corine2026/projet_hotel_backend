using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBackend.Data; // Remplace par le namespace de ton DbContext
using HotelBackend.Models;

namespace HotelBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Générera automatiquement /api/TypeChambres si la classe s'appelle TypeChambresController
    public class TypeChambresController : ControllerBase
    {
        private readonly HotelDbContext _context; // Remplace par le nom de ton DbContext

        public TypeChambresController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/TypeChambres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeChambre>>> GetTypeChambres()
        {
            return await _context.TypeChambres.ToListAsync();
        }
    }
}