namespace APBD_08.Models_DTOS.DTOs;

public class TripDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<CountryDTO> Countries { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }
}

public class CountryDTO
{
    public int IdCountry { get; set; }
    public string Name { get; set; }
}