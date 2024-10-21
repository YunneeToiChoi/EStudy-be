using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using study4_be.Services;
namespace study4_be.Controllers.Admin
{

    [Route("CourseManager/[controller]/[action]")]
    public class CoursesController : Controller
    {
        private readonly ILogger<CoursesController> _logger;
        public  readonly  Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;
        private readonly CourseRepository _coursesRepository;
        public CoursesController(ILogger<CoursesController> logger, FireBaseServices fireBaseServices, Study4Context context)
        {
            _logger = logger;
            _fireBaseServices = fireBaseServices;
            _context = context;
            _coursesRepository = new(context);
        }
        
        public async Task<ActionResult<IEnumerable<Course>>> GetAllCourses()
        {
            var courses = await _coursesRepository.GetAllCoursesAsync();
            return Json(new { status = 200, message = "Get Courses Successful", courses });

        }
        //development enviroment
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

        public async Task<IActionResult> GetCourseById(int id)  
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new {message = "Id is invalid"});
            }
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound(new {message ="Course is not found"});
            }

            return Ok(course);
        }
        [HttpGet]
        public async Task<IActionResult> Course_Edit(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "courseId is invalid" });
            }
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null)
            {
                return NotFound(new {message = "Course is not found"});
            }
            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Course_Edit(Course course, IFormFile? CourseImage)
        {
            if (!ModelState.IsValid) 
            {
                _logger.LogError("Course not found");
                return NotFound();
            }
            var courseToUpdate = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == course.CourseId);
            if (courseToUpdate == null)
            {
                _logger.LogError($"Course not found");
                return RedirectToAction("Course_List");
            }
            try
            {
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
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Course_List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
            }
            return View(course);
        }
        [HttpGet]
        public async Task<IActionResult> Course_Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Course not found for deletion.");
                return NotFound($"Course not found.");
            }
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null)
            {
                _logger.LogError($"Course not found for delete.");
                return NotFound($"Course not found.");
            }
            return View(course);
        }

        [HttpPost, ActionName("Course_Delete")]
        public async Task<IActionResult> Course_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Course not found for deletion.");
                return NotFound($"Course not found.");
            }
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null)
            {
                _logger.LogError($"Course not found for deletion.");
                return NotFound($"Course not found.");
            }
            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("Course_List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(course);
            }
        }


        public async Task<IActionResult> Course_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid course ID.");
                TempData["ErrorMessage"] = "The specified course was not found.";
                return RedirectToAction("Course_List", "Course");
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);

            // If no container is found, return to the list with an error
            if (course == null)
            {
                TempData["ErrorMessage"] = "The specified course was not found.";
                return RedirectToAction("Course_List", "Course");
            }
            return View(course);
        }
    }
}
