using System;

namespace distant.Models
{
    public class CourseResult
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string StudentId { get; set; }
        public double FinalGrade { get; set; } // Итоговая оценка
        public DateTime CompletionDate { get; set; }
    }
}