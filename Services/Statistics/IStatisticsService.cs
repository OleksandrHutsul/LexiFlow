using LexiFlow.Models.LearningModels;

namespace LexiFlow.Services.Statistics;

public interface IStatisticsService
{
    Task<LearningStats> GetAsync();
}
