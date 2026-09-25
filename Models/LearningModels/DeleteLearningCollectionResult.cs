namespace LexiFlow.Models.LearningModels;

public record DeleteLearningCollectionResult(bool IsSuccess, bool DeletedDefault, string? Error = null)
{
    public static DeleteLearningCollectionResult Success(bool deletedDefault) => new(true, deletedDefault);
    public static DeleteLearningCollectionResult Failure(string error) => new(false, false, error);
}
