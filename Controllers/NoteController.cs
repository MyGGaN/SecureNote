using Microsoft.AspNetCore.Mvc;
using SecureNote.Filters;
using SecureNote.Models;
using SecureNote.Services;

namespace SecureNote.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NoteController(AppDbContext db)
        {
            _db = db;
        }

        // POST api/<RegisterController>
        [HttpPost("{noteId}")]
        [Auth]
        public ActionResult Post(int noteId, [FromForm] Note note)
        {
            // Ensure user is authorized
            User? user = Authentication.AuthorizedUser(User.Identity, _db, Response);
            if (user == null)
            {
                return Unauthorized();
            }

            // Ensure the user is the true owner of this note
            if (note.UserId != user.Id ||
                _db.Notes.Where(_ => _.Id == note.Id && _.UserId == user.Id).FirstOrDefault() == null)
            {
                return Unauthorized("You can only update your own notes!");
            }

            // Update note
            Note? n = _db.Notes.Find(noteId);
            if (n != null)
            {
                n.Title = note.Title ?? n.Title;
                n.Content = note.Content ?? n.Content;
                n.IsPublic = note.IsPublic;
                _db.Notes.Update(n);
                _db.SaveChanges();
            }

            return Ok();
        }
    }
}