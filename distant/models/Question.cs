namespace distant.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } // вопрос
        public string Option1 { get; set; } // Вариант ответа 1
        public string Option2 { get; set; } // Вариант ответа 2
        public string Option3 { get; set; } // Вариант ответа 3
        public int CorrectOption { get; set; } // Номер правильного варианта

        // Связь с тестом
        public int TestId { get; set; }
        public Test Test { get; set; }
    }
}