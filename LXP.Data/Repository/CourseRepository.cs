using LXP.Common.Entities;
using LXP.Data.IRepository;
using System;
using System.Collections.Generic;                                                                      
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using LXP.Common.ViewModels;

namespace LXP.Data.Repository
{
    public class CourseRepository : ICourseRepository
    {
        private readonly LXPDbContext _lXPDbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _contextAccessor;
        public CourseRepository(LXPDbContext lXPDbContext, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            this._lXPDbContext = lXPDbContext;
            _environment = environment;
            _contextAccessor = httpContextAccessor;
        }
        public Course GetCourseDetailsByCourseId(Guid CourseId)
        {
            return _lXPDbContext.Courses.Find(CourseId);


        }
        public async Task AddCourse(Course course)
        {
            await _lXPDbContext.Courses.AddAsync(course);
            await _lXPDbContext.SaveChangesAsync();
        }
        public bool AnyCourseByCourseTitle(string CourseTitle)
        {
            return _lXPDbContext.Courses.Any(course => course.Title == CourseTitle);
        }

        //public IEnumerable<CourseListViewModel> GetAllCourseDetails()
        //{
        //    return _lXPDbContext.Courses
        //       .Select(c => new CourseListViewModel
        //       {
        //           CourseId = c.CourseId,
        //           Title = c.Title,
        //           Description = c.Description,
        //           Level = c.Level.Level,
        //           Category = c.Category.Category,
        //           Duration = c.Duration,

        //           CreatedAt = c.CreatedAt,
        //           CreatedBy = c.CreatedBy,
        //           Thumbnailimage = String.Format("{0}://{1}{2}/wwwroot/CourseThumbnailImages/{3}",
        //                                            _contextAccessor.HttpContext.Request.Scheme,
        //                                            _contextAccessor.HttpContext.Request.Host,
        //                                            _contextAccessor.HttpContext.Request.PathBase,
        //                                            c.Thumbnail),


        //       })
        //     .ToList();


        //}


        public IEnumerable<CourseListViewModel> GetAllCourseDetails()
        {
            return _lXPDbContext.Courses
                .Select(c => new CourseListViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Level = c.Level.Level,
                    Category = c.Category.Category,
                    // Convert TimeSpan to Decimal representing total minutes
                    Duration = c.Duration,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy,
                    Thumbnailimage = String.Format("{0}://{1}{2}/wwwroot/CourseThumbnailImages/{3}",
                                                    _contextAccessor.HttpContext.Request.Scheme,
                                                    _contextAccessor.HttpContext.Request.Host,
                                                    _contextAccessor.HttpContext.Request.PathBase,
                                                    c.Thumbnail),
                })
                .ToList();
        }


        public async Task<dynamic> GetAllCourseDetailsByLearnerId(Guid learnerId)
        {

            var query = from course in _lXPDbContext.Courses
                        join enrollment in _lXPDbContext.Enrollments
                        on new { course.CourseId, LearnerId = learnerId } equals new { enrollment.CourseId, enrollment.LearnerId }
                        into enrollments
                        from enrollment in enrollments.DefaultIfEmpty()
                        where course.IsAvailable && course.IsActive
                        orderby course.CourseId, enrollment.EnrollmentId
                        select new
                        {

                            CourseId = course.CourseId,
                            Catagory = course.Category.Category,
                            Level = course.Level.Level,
                            Title = course.Title,
                            Description = course.Description,
                            Duration = course.Duration,
                            Thumbnailimage = String.Format("{0}://{1}{2}/wwwroot/CourseThumbnailImages/{3}",  //name changed
                                             _contextAccessor.HttpContext.Request.Scheme,
                                             _contextAccessor.HttpContext.Request.Host,
                                             _contextAccessor.HttpContext.Request.PathBase,
                                             course.Thumbnail),
                            CreatedBy = "Admin",
                            CreatedAt = new DateTime(),
                            IsActive = true,
                            IsAvailable = true,
                            ModifiedAt = new DateTime(),
                            ModifiedBy = "Admin",


                            EnrollStatus = enrollment.EnrollStatus == null ? false : enrollment.EnrollStatus,

                        };

            //var query = from course in _lXPDbContext.Courses
            //            join enrollment in _lXPDbContext.Enrollments
            //            on new { course.CourseId, LearnerId = learnerId } equals new { enrollment.CourseId, enrollment.LearnerId }
            //            into enrollments
            //            from enrollment in enrollments.DefaultIfEmpty()
            //            where course.IsAvailable && course.IsActive
            //            orderby course.CourseId, enrollment?.EnrollmentId ?? 0 // Handle nullable EnrollmentId
            //            select new
            //            {
            //                Course = course,
            //                EnrollmentId = enrollment?.EnrollmentId ?? 0, // Handle nullable EnrollmentId
            //                LearnerId = enrollment.LearnerId,
            //                EnrollmentDate = enrollment.EnrollmentDate,
            //                EnrollStatus = enrollment.EnrollStatus,
            //                EnrollRequestStatus = enrollment.EnrollRequestStatus,
            //                EnrollmentCreatedBy = enrollment.CreatedBy,
            //                EnrollmentCreatedAt = enrollment.CreatedAt,
            //                EnrollmentModifiedBy = enrollment.ModifiedBy,
            //                EnrollmentModifiedAt = enrollment.ModifiedAt                                          // Other fields...
            //            };

            //return query.ToList();


            // Execute the query or further manipulate the results as needed
            return query.ToList();

        }

    }

}
