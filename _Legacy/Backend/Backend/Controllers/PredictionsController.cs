namespace Backend.Controllers
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Models;
    using Domain;

    [Authorize(Roles = "Admin")]
    public class PredictionsController : ApiController
    {
        private readonly DataContextLocal _db = new DataContextLocal();

        // GET: api/Predictions
        public IQueryable<Prediction> GetPredictions()
        {
            return _db.Predictions;
        }

        // GET: api/Predictions/5
        [ResponseType(typeof(Prediction))]
        public async Task<IHttpActionResult> GetPrediction(int id)
        {
            var prediction = await _db.Predictions.FindAsync(id);
            if (prediction == null)
            {
                return NotFound();
            }

            return Ok(prediction);
        }

        // PUT: api/Predictions/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutPrediction(int id, Prediction prediction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != prediction.PredictionId)
            {
                return BadRequest();
            }

            _db.Entry(prediction).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PredictionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Predictions
        [ResponseType(typeof(Prediction))]
        public async Task<IHttpActionResult> PostPrediction(Prediction prediction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Predictions.Add(prediction);
            await _db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = prediction.PredictionId }, prediction);
        }

        // DELETE: api/Predictions/5
        [ResponseType(typeof(Prediction))]
        public async Task<IHttpActionResult> DeletePrediction(int id)
        {
            Prediction prediction = await _db.Predictions.FindAsync(id);
            if (prediction == null)
            {
                return NotFound();
            }

            _db.Predictions.Remove(prediction);
            await _db.SaveChangesAsync();

            return Ok(prediction);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PredictionExists(int id)
        {
            return _db.Predictions.Count(e => e.PredictionId == id) > 0;
        }
    }
}