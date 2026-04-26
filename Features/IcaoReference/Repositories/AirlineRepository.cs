using ZdcReference.Features.IcaoReference.Models;

namespace ZdcReference.Features.IcaoReference.Repositories;

public class AirlineRepository
{
    public IEnumerable<Airline> AllAirlines => _repository;
    
    private readonly List<Airline> _repository = [];

    public void AddAirline(Airline airline) => _repository.Add(airline);
    
    public void AddAirlines(IEnumerable<Airline> airlines) => _repository.AddRange(airlines);

    public void ClearAirlines() => _repository.Clear();
}