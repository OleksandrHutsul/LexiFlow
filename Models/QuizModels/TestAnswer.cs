namespace LexiFlow.Models.QuizModels;

public record TestAnswer(string Prompt, string Given, string Expected, bool Correct);
