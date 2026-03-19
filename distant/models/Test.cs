using System.Collections.Generic;

namespace distant.Models
{
    public class Test
    {
        public int Id { get; set; }
        public string Title { get; set; }

        // Внешний ключ для связи с курсом
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // Связь с результатами прохождения
        public List<Result> Results { get; set; } = new List<Result>();

        public List<Question> Questions { get; set; } = new List<Question>();
    }
}