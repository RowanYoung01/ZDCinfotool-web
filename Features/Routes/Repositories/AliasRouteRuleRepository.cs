using ZdcReference.Features.Routes.Models;

namespace ZdcReference.Features.Routes.Repositories;

public class AliasRouteRuleRepository
{
    private List<AliasRouteRule> _repository = [];

    public void AddRule(AliasRouteRule rule) => _repository.Add(rule);
    
    public void AddRules(IEnumerable<AliasRouteRule> rules) => _repository.AddRange(rules);

    public IEnumerable<AliasRouteRule> GetAllRules() => _repository;

    public void ClearRules() => _repository.Clear();
}