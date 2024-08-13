using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSVVV.Data;
using QLSVVV.Models;

namespace QLSVVV.Controllers
{
    [Authorize(Roles = "Admin,Student,Teacher")]
    public class CoursesController : Controller
    {
        private readonly ICourseContext _courseContext;
        private readonly IStudentContext _studentContext;

        public CoursesController(ICourseContext courseContext, IStudentContext studentContext)
        {
            _courseContext = courseContext;
            _studentContext = studentContext;
        }
        // GET: Courses
        public async Task<IActionResult> Index()
        {
            return _courseContext.Courses != null ?
                        View(await _courseContext.Courses.ToListAsync()) :
                        Problem("Entity set 'QLSVVVContext.Course' is null.");
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _courseContext.Courses == null)
            {
                return NotFound();
            }
            // Get course based on ID
            var course = await _courseContext.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Class,DateStart,DateEnd,Major,Lecturer")] Course course)
        {
            if (ModelState.IsValid)
            {
                _courseContext.Add(course);
                await _courseContext.SaveChangesAsync();
                return RedirectToAction(nameof(ManageCourse));
            }
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _courseContext.Courses == null)
            {
                return NotFound();
            }

            var course = await _courseContext.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Class,DateStart,DateEnd,Major,Lecturer")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _courseContext.Update(course);
                    await _courseContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ManageCourse));
            }
            return View(course);
        }

        public IActionResult ViewStudentlnCourse(int id)
        {
            // Get the course and include student information in the course
            var course = _courseContext.Courses
                .Include(c => c.CourseStudent)
                .ThenInclude(cs => cs.Student)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }
            // Get a list of all students
            ViewBag.AllStudents = _studentContext.Students.ToList();

            return View(course);
        }
        public IActionResult ViewStudentnnCourse(int id)
        {
            // Get the course and include student information in the course
            var course = _courseContext.Courses
                .Include(c => c.CourseStudent)
                .ThenInclude(cs => cs.Student)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound();  
            }
            // Get a list of all students
            ViewBag.AllStudents = _studentContext.Students.ToList();

            return View(course);
        }
        public IActionResult EditCourse(int id)
        {
            var course = _courseContext.Courses
                .Include(c => c.CourseStudent)
                .ThenInclude(cs => cs.Student)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            ViewBag.AllStudents = _studentContext.Students.ToList();

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentToCourse(int courseId, int studentId)
        {
            var course = _courseContext.Courses
                .Include(c => c.CourseStudent)
                .FirstOrDefault(c => c.Id == courseId);

            var student = _studentContext.Students.FirstOrDefault(s => s.Id == studentId);

            if (course != null && student != null)
            {
                course.CourseStudent.Add(new CourseStudent { CourseId = courseId, StudentId = studentId });
                await _courseContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(EditCourse), new { id = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudentFromCourse(int courseId, int studentId)
        {
            var course = _courseContext.Courses
                .Include(c => c.CourseStudent)
                .FirstOrDefault(c => c.Id == courseId);

            var student = _studentContext.Students.FirstOrDefault(s => s.Id == studentId);

            if (course != null && student != null)
            {
                var courseStudent = course.CourseStudent.FirstOrDefault(cs => cs.StudentId == studentId);
                if (courseStudent != null)
                {
                    course.CourseStudent.Remove(courseStudent);
                    await _courseContext.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(EditCourse), new { id = courseId });
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _courseContext.Courses == null)
            {
                return NotFound();
            }

            var course = await _courseContext.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_courseContext.Courses == null)
            {
                return Problem("Entity set 'QLSVVVContext.Course' is null.");
            }
            var course = await _courseContext.Courses.FindAsync(id);
            if (course != null)
            {
                _courseContext.Courses.Remove(course);
            }

            await _courseContext.SaveChangesAsync();
            return RedirectToAction(nameof(ManageCourse));
        }

        private bool CourseExists(int id)
        {
            return (_courseContext.Courses?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> ManageCourse()
        {
            return View(await _courseContext.Courses.ToListAsync());
        }

        public async Task<IActionResult> ViewCourse()
        {
            return View(await _courseContext.Courses.ToListAsync());
        }

        public async Task<IActionResult> ViewCourseStudent()
        {
            return View(await _courseContext.Courses.ToListAsync());
        }
    }

}
