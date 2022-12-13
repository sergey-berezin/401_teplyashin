using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Database;

namespace LibraryServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private IImageDb db;

        public ImagesController(IImageDb db)
        {
            this.db = db;
        }

        [HttpPost]
        public async Task<int> AddImage([FromBody] Data obj, CancellationToken token)
        {
            var img = obj.img;
            var path = obj.path;
            return await db.PostImage(img, path, token);
        }

        [HttpGet]
        public async Task<List<int>> GetImages()
        {
            return await db.GetAllImages();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImage(int id)
        {
            var res = await db.GetImageById(id);
            if (res != null)
                return res;
            else
                return StatusCode(404, "Image with given id is not found");
        }

        [HttpDelete]
        public async Task<int> DeleteImages()
        {
            return await db.DeleteAllImages();
        }
    }
}