using LexiFlow.Enums.LearningEnums;

namespace LexiFlow.Models.LearningModels;

public record AddLearningWordResult(AddLearningWordStatus Status, string? CollectionName = null);
