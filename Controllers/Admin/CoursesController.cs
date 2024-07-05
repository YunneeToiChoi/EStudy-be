using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using study4_be.Services;
namespace study4_be.Controllers.Admin
{
    public class CoursesController : Controller
    {
        private readonly ILogger<CoursesController> _logger;
        private FireBaseServices _fireBaseServices;
        public CoursesController(ILogger<CoursesController> logger, FireBaseServices fireBaseServices)
        {
            _logger = logger;
            _fireBaseServices = fireBaseServices;
        }
        private readonly CourseRepository _coursesRepository = new CourseRepository();
        public STUDY4Context _context = new STUDY4Context();

        [HttpGet("GetAllCourses")]
        public async Task<ActionResult<IEnumerable<Course>>> GetAllCourses()
        {
            var courses = await _coursesRepository.GetAllCoursesAsync();
            return Json(new { status = 200, message = "Get Courses Successful", courses });

        }
        //development enviroment
        [HttpDelete("DeleteAllCourses")]
        public async Task<IActionResult> DeleteAllCourses()
        {
            await _coursesRepository.DeleteAllCoursesAsync();
            return Json(new { status = 200, message = "Delete Courses Successful" });
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Course_List()
        {
            var courses = await _coursesRepository.GetAllCoursesAsync(); // Retrieve list of courses from repository
            return View(courses); // Pass the list of courses to the view
        }
        public IActionResult Course_Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Course_Create(Course course, IFormFile CourseImage)
        {
            if (!ModelState.IsValid)
            {

                return View(course);    //show form with value input and show errors
            }
            try
            {
                // Handle file upload to Firebase Storage
                if (CourseImage != null && CourseImage.Length > 0)
                {
                    var firebaseBucketName = _fireBaseServices.GetFirebaseBucketName();
                    var uniqueId = Guid.NewGuid().ToString(); // Tạo một UUID ngẫu nhiên
                    var imgFilePath = ($"IMG{uniqueId}.jpg");
                    // Upload file to Firebase Storage
                    string firebaseUrl = await _fireBaseServices.UploadFileToFirebaseStorageAsync(CourseImage, imgFilePath, firebaseBucketName);
                    // Save firebaseUrl to your course object or database
                    course.CourseImage = firebaseUrl;
                }
                await _context.AddAsync(course);
                await _context.SaveChangesAsync();
                CreatedAtAction(nameof(GetCourseById), new { id = course.CourseId }, course);
                return RedirectToAction("Course_List", "Courses"); // nav to main home when add successfull, after change nav to index create Courses
            }
            catch (Exception ex)
            {
                // show log
                _logger.LogError(ex, "Error occurred while creating new course.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(course);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }
        [HttpGet]
        public IActionResult Course_Edit(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Course_Edit(Course course, IFormFile CourseImage)
        {
                var courseToUpdate = _context.Courses.FirstOrDefault(c => c.CourseId == course.CourseId);
                if (CourseImage != null && CourseImage.Length > 0)
                {
                    var firebaseBucketName = _fireBaseServices.GetFirebaseBucketName();
                    // Delete the old image from Firebase Storage
                    if (!string.IsNullOrEmpty(courseToUpdate.CourseImage))
                    {
                        // Extract the file name from the URL
                        var oldFileName = Path.GetFileName(new Uri(courseToUpdate.CourseImage).LocalPath);
                        await _fireBaseServices.DeleteFileFromFirebaseStorageAsync(oldFileName, firebaseBucketName);
                    }
                    var uniqueId = Guid.NewGuid().ToString();
                    var imgFilePath = ($"IMG{uniqueId}.jpg");
                    string firebaseUrl = await _fireBaseServices.UploadFileToFirebaseStorageAsync(CourseImage, imgFilePath, firebaseBucketName);
                    courseToUpdate.CourseName = course.CourseName;
                    courseToUpdate.CourseDescription = course.CourseDescription;
                    courseToUpdate.CoursePrice = course.CoursePrice;
                    courseToUpdate.CourseTag = course.CourseTag;
                    courseToUpdate.CourseImage = firebaseUrl;
                }
                else
                {
                    courseToUpdate.CourseName = course.CourseName;
                    courseToUpdate.CourseDescription = course.CourseDescription;
                    courseToUpdate.CoursePrice = course.CoursePrice;
                    courseToUpdate.CourseTag = course.CourseTag;
                    courseToUpdate.CourseImage = courseToUpdate.CourseImage;
                }
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Course_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {course.CourseId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            return View(course);
        }
        [HttpGet]
        public IActionResult Course_Delete(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == id);
            if (course == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(course);
        }

        [HttpPost, ActionName("Course_Delete")]
        public IActionResult Course_DeleteConfirmed(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == id);
            if (course == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
                return RedirectToAction("Course_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(course);
            }
        }


        public IActionResult Course_Details(int id)
        {
            return View(_context.Courses.FirstOrDefault(c => c.CourseId == id));
        }
    }
}
