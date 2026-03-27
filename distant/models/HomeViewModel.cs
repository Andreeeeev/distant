using System.Collections.Generic;

namespace distant.Models
{
    public class HomeViewModel
    {
        public List<CourseResult> CompletedCourses { get; set; } = new List<CourseResult>();
        public List<ActiveCourseInfo> ActiveCourses { get; set; } = new List<ActiveCourseInfo>();
    }

    public class ActiveCourseInfo
    {
        public Course Course { get; set; }
        public List<Test> RemainingTests { get; set; } = new List<Test>();
    }
}