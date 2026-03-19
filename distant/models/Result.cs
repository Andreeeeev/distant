using System;

namespace distant.Models
{
    public class Result
    {
        public int Id { get; set; }
        public int Score { get; set; } // Итоговый балл 
        public DateTime AttemptDate { get; set; } // Дата прохождения 

        // Связь с тестом
        public int TestId { get; set; }
        public Test Test { get; set; }

        // Связь со студентом
        public string StudentId { get; set; }
    }
}