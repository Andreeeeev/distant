namespace distant.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } // Сам вопрос

        // Оставляем опции для тестов с выбором ответа (для текстовых они будут просто пустыми)
        public string? Option1 { get; set; }
        public string? Option2 { get; set; }
        public string? Option3 { get; set; }

        // 0 - Один ответ (Radio)
        // 1 - Несколько ответов (Checkbox)
        // 2 - Текст (Точное совпадение)
        // 3 - Текст (Ключевые слова)
        public int QuestionType { get; set; }

        // Универсальное поле для правильного ответа. 
        // Может быть "1", или "1,3", или "Архитектура", или "база,данные,sql"
        public string CorrectAnswerText { get; set; }

        public int TestId { get; set; }
        public Test Test { get; set; }
    }
}