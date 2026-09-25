namespace LexiFlow.Models.QuizModels;

public record TestQuestion(string Prompt, string Answer, List<string> Choices);
