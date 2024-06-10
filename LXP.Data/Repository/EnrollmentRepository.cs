using LXP.Common.Entities;
using LXP.Data.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Presentation;
using Spire.Doc;
using MySqlX.XDevAPI.Common;

namespace LXP.Data.Repository
{


    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly LXPDbContext _lXPDbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _contextAccessor;

        public EnrollmentRepository(LXPDbContext lXPDbContext, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _lXPDbContext = lXPDbContext;
            _environment = webHostEnvironment;
            _contextAccessor = httpContextAccessor;

        }

        public async Task Addenroll(Enrollment enrollment)
        {
            await _lXPDbContext.Enrollments.AddAsync(enrollment);
            await _lXPDbContext.SaveChangesAsync();

        }

        public bool AnyEnrollmentByLearnerAndCourse(Guid LearnerId, Guid CourseId)
        {
            return _lXPDbContext.Enrollments.Any(enrollment => enrollment.LearnerId == LearnerId && enrollment.CourseId == CourseId);
        }

        public object GetCourseandTopicsByLearnerId(Guid learnerId)
        {
            var result = from enrollment in _lXPDbContext.Enrollments
                         where enrollment.LearnerId == learnerId
                         select new
                         {
                             enrollmentid = enrollment.EnrollmentId,
                             enrolledCourseId = enrollment.CourseId,
                             enrolledCoursename = enrollment.Course.Title,
                             enrolledcoursedescription = enrollment.Course.Description,
                             enrolledcoursecategory = enrollment.Course.Category.Category,
                             enrolledcourselevels = enrollment.Course.Level.Level,
                             Thumbnailimage = String.Format("{0}://{1}{2}/wwwroot/CourseThumbnailImages/{3}",
                             _contextAccessor.HttpContext.Request.Scheme,
                             _contextAccessor.HttpContext.Request.Host,
                             _contextAccessor.HttpContext.Request.PathBase,
                             enrollment.Course.Thumbnail),

                             Topics = (from topic in _lXPDbContext.Topics
                                       where topic.CourseId == enrollment.CourseId && topic.IsActive == true
                                       select new
                                       {
                                           TopicName = topic.Name,
                                           TopicDescription = topic.Description,
                                           TopicId = topic.TopicId,
                                           TopicIsActive = topic.IsActive,
                                           Materials = (from material in _lXPDbContext.Materials
                                                        join materialType in _lXPDbContext.MaterialTypes on material.MaterialTypeId equals materialType.MaterialTypeId

                                                        where material.TopicId == topic.TopicId 
                                                        select new
                                                        {
                                                            MaterialId = material.MaterialId,
                                                            MaterialName = material.Name,
                                                            MaterialType = materialType.Type,
                                                            //Material = ConvertPptToPdfAsync( String.Format("{0}://{1}{2}/wwwroot/CourseMaterial/{3}",
                                                            Material =materialType.Type=="TEXT"? ConvertDocxToPdf(String.Format("{0}://{1}{2}/wwwroot/CourseMaterial/{3}",
                                                            _contextAccessor.HttpContext.Request.Scheme,
                                                            _contextAccessor.HttpContext.Request.Host,
                                                            _contextAccessor.HttpContext.Request.PathBase,
                                                            material.FilePath),
                                                            _environment,
                                                            _contextAccessor): materialType.Type == "PPT"? ConvertPptToPdf(String.Format("{0}://{1}{2}/wwwroot/CourseMaterial/{3}",
                                                            _contextAccessor.HttpContext.Request.Scheme,
                                                            _contextAccessor.HttpContext.Request.Host,
                                                            _contextAccessor.HttpContext.Request.PathBase,
                                                            material.FilePath),
                                                            _environment,
                                                            _contextAccessor): String.Format("{0}://{1}{2}/wwwroot/CourseMaterial/{3}",
                                                            _contextAccessor.HttpContext.Request.Scheme,
                                                            _contextAccessor.HttpContext.Request.Host,
                                                            _contextAccessor.HttpContext.Request.PathBase,
                                                            material.FilePath,
                                                            _environment,
                                                            _contextAccessor),
                                                            MaterialDuration = material.Duration
                                                        }).ToList(),
                                           //MaterialType =(from materialType in _lXPDbContext.MaterialTypes select new
                                           //{
                                           //    MaterialType=materialType.Type,
                                           //    MaterialTypeId=materialType.MaterialTypeId,
                                               
                                           //}).ToList(),
                                          
                                       }).ToList()
                         };
            return result;


        }

        public Enrollment FindEnrollmentId(Guid enrollmentId)
        {
            return _lXPDbContext.Enrollments.Find(enrollmentId);
        }

        public async Task DeleteEnrollment(Enrollment enrollment)
        {
            _lXPDbContext.Enrollments.Remove(enrollment);
            await _lXPDbContext.SaveChangesAsync();

        }

        public static string ConvertPptToPdf(string pptFilePath, IWebHostEnvironment environment, IHttpContextAccessor contextAccessor)
        {
            // Assuming 'pptFilePath' is a relative path like "CourseMaterial/file.pptx"
            string webRootPath = environment.WebRootPath;

            // Correct the file path by removing the URL part and ensuring proper separators
            string correctedFilePath = pptFilePath.Replace("http://localhost:5199/wwwroot/", "").Replace("/", "\\");
            string fullFilePath = Path.Combine(webRootPath, correctedFilePath);

            // Check if the file exists
            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException($"The file was not found: {fullFilePath}");
            }
            // Initialize a new Presentation object
            Presentation presentation = new Presentation();

            // Load the PPT file
            presentation.LoadFromFile(fullFilePath);

            // Define the output PDF file path
            string pdfFilePath = Path.ChangeExtension(fullFilePath, ".pdf");


            // Save the presentation as a PDF
            presentation.SaveToFile(pdfFilePath, Spire.Presentation.FileFormat.PDF);

            // Return the relative path for the PDF to be used as a URL
            string relativePdfPath = pdfFilePath.Replace(webRootPath, "").Replace("\\", "/");
            return $"{contextAccessor.HttpContext.Request.Scheme}://{contextAccessor.HttpContext.Request.Host}{contextAccessor.HttpContext.Request.PathBase}/wwwroot{relativePdfPath}";
        }

        
        public static string ConvertDocxToPdf(string docxFilePath, IWebHostEnvironment environment, IHttpContextAccessor contextAccessor)
        {
            // Assuming 'docxFilePath' is a relative path like "CourseMaterial/file.docx"
            string webRootPath = environment.WebRootPath;

            // Correct the file path by removing the URL part and ensuring proper separators
            string correctedFilePath = docxFilePath.Replace("http://localhost:5199/wwwroot/", "").Replace("/", "\\");
            string fullFilePath = Path.Combine(webRootPath, correctedFilePath);

            // Check if the file exists
            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException($"The file was not found: {fullFilePath}");
            }

            // Initialize a new Document object from Spire.Doc
            Document document = new Document();

            // Load the DOCX file
            document.LoadFromFile(fullFilePath);

            // Define the output PDF file path
            string pdfFilePath = Path.ChangeExtension(fullFilePath, ".pdf");

            // Save the document as a PDF
            document.SaveToFile(pdfFilePath, Spire.Doc.FileFormat.PDF);

            // Return the relative path for the PDF to be used as a URL
            string relativePdfPath = pdfFilePath.Replace(webRootPath, "").Replace("\\", "/");
            return $"{contextAccessor.HttpContext.Request.Scheme}://{contextAccessor.HttpContext.Request.Host}{contextAccessor.HttpContext.Request.PathBase}/wwwroot{relativePdfPath}";
        }
    }
}
