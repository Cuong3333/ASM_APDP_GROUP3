using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using QLSVVV.Controllers;
using QLSVVV.Data;
using QLSVVV.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using Xunit;

namespace QLSVVV.Tests.Controllers
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly Mock<ILogger<AuthenticationController>> _mockLogger;
        private readonly Mock<ITempDataDictionaryFactory> _mockTempDataDictionaryFactory;
        private readonly Mock<ITempDataProvider> _mockTempDataProvider;
        private readonly Mock<IUrlHelperFactory> _mockUrlHelperFactory;
        private readonly Mock<IUrlHelper> _mockUrlHelper;
        private readonly AuthenticationController _controller;
        private readonly QLSVVVContext _context;

        public AuthenticationControllerTests()
        {
            var options = new DbContextOptionsBuilder<QLSVVVContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new QLSVVVContext(options);

            _mockHttpContext = new Mock<HttpContext>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockLogger = new Mock<ILogger<AuthenticationController>>();
            _mockTempDataDictionaryFactory = new Mock<ITempDataDictionaryFactory>();
            _mockTempDataProvider = new Mock<ITempDataProvider>();
            _mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            _mockUrlHelper = new Mock<IUrlHelper>();

            var tempData = new TempDataDictionary(_mockHttpContext.Object, _mockTempDataProvider.Object);
            _mockTempDataDictionaryFactory.Setup(factory => factory.GetTempData(It.IsAny<HttpContext>()))
                .Returns(tempData);

            _mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(_mockUrlHelper.Object);

            var authServiceProvider = new Mock<IServiceProvider>();
            authServiceProvider.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                               .Returns(_mockAuthService.Object);
            authServiceProvider.Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                               .Returns(_mockUrlHelperFactory.Object);

            _mockHttpContext.Setup(h => h.RequestServices)
                            .Returns(authServiceProvider.Object);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);

            _controller = new AuthenticationController(_context)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _mockHttpContext.Object
                },
                TempData = tempData,
                Url = _mockUrlHelper.Object
            };
        }

        [Fact]
        public async Task Login_WithValidCredentials_RedirectsToRoleBasedPage()
        {
            // Arrange
            var password = "123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Id = 7, UserName = "admin", Password = hashedPassword, Role = "Admin" };
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            // Verify user is added to the database
            var addedUser = await _context.User.FindAsync(user.Id);
            Assert.NotNull(addedUser);
            Assert.Equal(hashedPassword, addedUser.Password);

            var loginModel = new User { UserName = "admin", Password = password };

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            if (redirectResult == null)
            {
                var viewResult = result as ViewResult;
                if (viewResult != null)
                {
                    var error = viewResult.ViewData["error"];
                    throw new Exception($"Expected RedirectToActionResult but got ViewResult with error: {error}");
                }
                else
                {
                    throw new Exception("Expected RedirectToActionResult but got unexpected result.");
                }
            }

            Assert.NotNull(redirectResult);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Admin", redirectResult.ControllerName);
        }




        [Fact]
        public async Task Login_WithValidStudentCredentials_RedirectsToStudentPage()
        {
            // Arrange
            var password = "123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { UserName = "student", Password = hashedPassword, Role = "Student" };
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var loginModel = new User { UserName = "student", Password = password };

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            if (redirectResult == null)
            {
                var viewResult = result as ViewResult;
                if (viewResult != null)
                {
                    var error = viewResult.ViewData["error"];
                    throw new Exception($"Expected RedirectToActionResult but got ViewResult with error: {error}");
                }
                else
                {
                    throw new Exception("Expected RedirectToActionResult but got unexpected result.");
                }
            }

            Assert.NotNull(redirectResult);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Students", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_WithValidTeacherCredentials_RedirectsToTeacherPage()
        {
            // Arrange
            var password = "123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { UserName = "teacher", Password = hashedPassword, Role = "Teacher" };
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var loginModel = new User { UserName = "teacher", Password = password };

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            if (redirectResult == null)
            {
                var viewResult = result as ViewResult;
                if (viewResult != null)
                {
                    var error = viewResult.ViewData["error"];
                    throw new Exception($"Expected RedirectToActionResult but got ViewResult with error: {error}");
                }
                else
                {
                    throw new Exception("Expected RedirectToActionResult but got unexpected result.");
                }
            }

            Assert.NotNull(redirectResult);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Teachers", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var user = new User { UserName = "admin", Password = BCrypt.Net.BCrypt.HashPassword("password"), Role = "Admin" };
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var loginModel = new User { UserName = "admin", Password = "wrongpassword" };

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(loginModel, viewResult.Model);
            Assert.Equal("Invalid user!", viewResult.ViewData["error"]);
        }

        [Fact]
        public async Task Login_WithEmptyUsernameAndPassword_ReturnsViewWithError()
        {
            // Arrange
            var loginModel = new User { UserName = "", Password = "" };

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(loginModel, viewResult.Model);
            Assert.Equal("Please enter both username and password.", viewResult.ViewData["error"]);
        }

        [Fact]
        public async Task Logout_RedirectsToHomeIndex()
        {
            // Act
            var result = await _controller.Logout();

            // Assert
            _mockAuthService.Verify(a => a.SignOutAsync(It.IsAny<HttpContext>(), CookieAuthenticationDefaults.AuthenticationScheme, null), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }
    }
    public static class DbContextMock
    {
        public static Mock<DbSet<T>> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            return dbSet;
        }
    }
    public class TeachersControllerTests
    {
        private DbContextOptions<QLSVVVContext> _dbContextOptions;
        private QLSVVVContext _context;
        private TeachersController _controller;

        public TeachersControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<QLSVVVContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new QLSVVVContext(_dbContextOptions);
            _controller = new TeachersController(_context);
        }
        private async Task ResetDatabase()
        {
            var teachers = _context.Teachers.ToList();
            _context.Teachers.RemoveRange(teachers);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task Index_ReturnsViewWithTeachers()
        {
            // Arrange
            await ResetDatabase();
            _context.Teachers.Add(new Teacher { Id = 1, Name = "Dinh van dong", DoB = DateTime.Now, Major = "IT" });
            _context.Teachers.Add(new Teacher { Id = 2, Name = "HA NGOC LINH", DoB = DateTime.Now, Major = "IT" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Teacher>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }


        [Fact]
        public async Task Create_ValidModel_RedirectsToManageTeacher()
        {
            // Arrange
            var teacher = new Teacher { Id = 3, Name = "Teacher3", DoB = DateTime.Now, Major = "English" };

            // Act
            var result = await _controller.Create(teacher);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageTeacher", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_ValidModel_RedirectsToManageTeacher()
        {
            // Arrange
            var teacher = new Teacher { Id = 4, Name = "Teacher4", DoB = DateTime.Now, Major = "History" };
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            // Act
            teacher.Name = "UpdatedTeacher";
            var result = await _controller.Edit(teacher.Id, teacher);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageTeacher", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_ValidId_RedirectsToManageTeacher()
        {
            // Arrange
            var teacher = new Teacher { Id = 5, Name = "Teacher5", DoB = DateTime.Now, Major = "Geography" };
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(teacher.Id);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageTeacher", redirectToActionResult.ActionName);
        }
    }
    public class StudentsControllerTests
    {
        private DbContextOptions<QLSVVVContext> _dbContextOptions;
        private QLSVVVContext _context;
        private StudentsController _controller;

        public StudentsControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<QLSVVVContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new QLSVVVContext(_dbContextOptions);
            _controller = new StudentsController(_context);
        }

        private async Task ResetDatabase()
        {
            var students = _context.Students.ToList();
            _context.Students.RemoveRange(students);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task Index_ReturnsViewWithStudents()
        {
            // Arrange
            await ResetDatabase();
            _context.Students.Add(new Student { Id = 1, Name = "bienz", DoB = DateTime.Now, Address = "bavi", Major = "asdads" });
            _context.Students.Add(new Student { Id = 2, Name = "bien nguyen", DoB = DateTime.Now, Address = "coido", Major = "IT" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Student>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Create_ValidModel_RedirectsToManageStudent()
        {
            // Arrange
            var student = new Student { Id = 3, Name = "Student3", DoB = DateTime.Now, Address = "Address3", Major = "English" };

            // Act
            var result = await _controller.Create(student);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageStudent", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_ValidModel_RedirectsToManageStudent()
        {
            // Arrange
            var student = new Student { Id = 4, Name = "Student4", DoB = DateTime.Now, Address = "Address4", Major = "History" };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Act
            student.Name = "UpdatedStudent";
            var result = await _controller.Edit(student.Id, student);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageStudent", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_ValidId_RedirectsToManageStudent()
        {
            // Arrange
            var student = new Student { Id = 5, Name = "Student5", DoB = DateTime.Now, Address = "Address5", Major = "Geography" };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(student.Id);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageStudent", redirectToActionResult.ActionName);
        }
    }
}
