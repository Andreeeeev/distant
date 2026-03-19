namespace distant.Models
{
    public class Lecture
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; } // Текст лекции или ссылка на файл

        // Внешний ключ для связи с курсом
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}