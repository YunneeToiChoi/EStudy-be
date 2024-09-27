using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{
    public class ExamController : Controller
    {
        private readonly ILogger<ExamController> _logger;
        private FireBaseServices _fireBaseServices;
        public ExamController(ILogger<ExamController> logger, FireBaseServices fireBaseServices)
        {
            _logger = logger;
            _fireBaseServices = fireBaseServices;

        }
        private readonly ExamRepository _examsRepository = new ExamRepository();
        public Study4Context _context = new Study4Context();

        [HttpGet("GetAllExams")]
        public async Task<ActionResult<IEnumerable<Course>>> GetAllExams()
        {
            var courses = await _examsRepository.GetAllExamsAsync();
            return Json(new { status = 200, message = "Get Courses Successful", courses });

        }
        //development enviroment
        [HttpDelete("DeleteAllExams")]
        public async Task<IActionResult> DeleteAllCourses()
        {
            await _examsRepository.DeleteAllExamsAsync();
            return Json(new { status = 200, message = "Delete Courses Successful" });
        }
        public async Task<IActionResult> Exam_List()
        {
            var courses = await _examsRepository.GetAllExamsAsync(); // Retrieve list of courses from repository
            return View(courses); // Pass the list of courses to the view
        }
        public IActionResult Exam_Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Exam_Create(Exam exam, IFormFile ExamImage)
        {
            try
            {
                // Handle file upload to Firebase Storage
                if (ExamImage != null && ExamImage.Length > 0)
                {
                    var firebaseBucketName = _fireBaseServices.GetFirebaseBucketName();
                    var uniqueId = Guid.NewGuid().ToString(); // Tạo một UUID ngẫu nhiên
                    var imgFilePath = ($"IMG{uniqueId}.jpg");
                    // Upload file to Firebase Storage
                    string firebaseUrl = await _fireBaseServices.UploadFileToFirebaseStorageAsync(ExamImage, imgFilePath, firebaseBucketName);
                    // Save firebaseUrl to your course object or database
                    exam.ExamImage = firebaseUrl;
                }
                exam.ExamId = Guid.NewGuid().ToString();
                await _context.AddAsync(exam);
                await _context.SaveChangesAsync();
                CreatedAtAction(nameof(GetExamById), new { id = exam.ExamId }, exam);
                return RedirectToAction("Exam_List", "Exam"); // nav to main home when add successfull, after change nav to index create Courses
            }
            catch (Exception ex)
            {
                // show log
                _logger.LogError(ex, "Error occurred while creating new course.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(exam);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExamById(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Id is invalid" });
            }
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            return Ok(exam);
        }
        [HttpGet]
        public IActionResult Exam_Edit(string id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "examId is invalid" });
            }
            var exam = _context.Exams.FirstOrDefault(c => c.ExamId == id);
            if (exam == null)
            {
                return NotFound();
            }
            return View(exam);
        }

        [HttpPost]
        public async Task<IActionResult> Exam_Edit(Exam exam, IFormFile ExamImage)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = _context.Exams.FirstOrDefault(c => c.ExamId == exam.ExamId);
                if (ExamImage != null && ExamImage.Length > 0)
                {
                    var firebaseBucketName = _fireBaseServices.GetFirebaseBucketName();
                    // Delete the old image from Firebase Storage
                    if (!string.IsNullOrEmpty(courseToUpdate.ExamImage))
                    {
                        // Extract the file name from the URL
                        var oldFileName = Path.GetFileName(new Uri(courseToUpdate.ExamImage).LocalPath);
                        await _fireBaseServices.DeleteFileFromFirebaseStorageAsync(oldFileName, firebaseBucketName);
                    }
                    var uniqueId = Guid.NewGuid().ToString();
                    var imgFilePath = ($"IMG{uniqueId}.jpg");
                    string firebaseUrl = await _fireBaseServices.UploadFileToFirebaseStorageAsync(ExamImage, imgFilePath, firebaseBucketName);
                    exam.ExamImage = firebaseUrl;
                }
                courseToUpdate.ExamName = exam.ExamName;
                courseToUpdate.ExamImage = exam.ExamImage;
                courseToUpdate.ExamAudio = exam.ExamAudio;
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Exam_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {exam.ExamId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(exam);
        }
        [HttpGet]
        public IActionResult Exam_Delete(string id)
        {
            var exam = _context.Exams.FirstOrDefault(c => c.ExamId == id);
            if (exam == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(exam);
        }

        [HttpPost, ActionName("Exam_Delete")]
        public IActionResult Exam_DeleteConfirmed(string id)
        {
            var exam = _context.Exams.FirstOrDefault(c => c.ExamId == id);
            if (exam == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Exams.Remove(exam);
                _context.SaveChanges();
                return RedirectToAction("Exam_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(exam);
            }
        }


        public IActionResult Exam_Details(string id)
        {
            return View(_context.Exams.FirstOrDefault(c => c.ExamId == id));
        }
    }
}
