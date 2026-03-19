using System.Collections.Generic;

namespace distant.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } // Название предмета

        // Навигационные свойства для связи таблиц
        public List<Lecture> Lectures { get; set; } = new List<Lecture>();
        public List<Test> Tests { get; set; } = new List<Test>();
    }
}