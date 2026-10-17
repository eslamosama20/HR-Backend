using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.ValueObjects;

public record Address : IValueObject
{

    public string Street { get; }
    
    public string City { get; }
    
    public string Country { get; }
    
    public string ZipCode { get; }

    public string? ShortAddress { get; }
    public string? BuildingNumber { get; }
    public string? SubNumber { get; }
    public string? District { get; }
    public string FullAddress => $"{Street.Trim()}, {City.Trim()}, {Country.Trim()}, {ZipCode.Trim()}";
    
    public Address()
    {
        
    }
    
    public Address(string street, string city, string country, string zipCode)
    {
        Street = street;
        City = city;
        Country = country;
        ZipCode = zipCode;
    }

    public Address(
        string street,
        string city,
        string country,
        string zipCode,
        string? shortAddress,
        string? buildingNumber,
        string? subNumber,
        string? district)
    {
        Street = street;
        City = city;
        Country = country;
        ZipCode = zipCode;
        ShortAddress = shortAddress;
        BuildingNumber = buildingNumber;
        SubNumber = subNumber;
        District = district;
    }
    
    
    
    public override string ToString()
    {
        return FullAddress;
    }
    
    public static implicit operator string(Address address)
    {
        return address.ToString();
    }
    
    public static implicit operator Address(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return new Address(string.Empty, string.Empty, string.Empty, string.Empty);
        }

        var rawParts = address.Split(',').Select(p => p.Trim()).ToArray();
        
        // Handle legacy or comma-joined country strings where street and city are empty
        if (rawParts.Length >= 4 && string.IsNullOrWhiteSpace(rawParts[0]) && string.IsNullOrWhiteSpace(rawParts[1]))
        {
            var countryParts = rawParts.Skip(2).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (countryParts.Count > 0)
            {
                // Check if last element is a short numeric/code zip code, else include it as country
                string zip = string.Empty;
                if (countryParts.Count > 1)
                {
                    var last = countryParts.Last();
                    if (last.All(char.IsDigit) && last.Length <= 10)
                    {
                        zip = last;
                        countryParts.RemoveAt(countryParts.Count - 1);
                    }
                }
                string combinedCountry = string.Join(" - ", countryParts);
                return new Address(string.Empty, string.Empty, combinedCountry, zip);
            }
        }

        // Standard 4-part layout: street, city, country, zipCode
        if (rawParts.Length >= 4)
        {
            var street = rawParts[0];
            var city = rawParts[1];
            var country = rawParts[2];
            var zipCode = rawParts[3];

            if (string.IsNullOrWhiteSpace(country))
            {
                var nonEmpties = rawParts.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                if (nonEmpties.Length > 0)
                {
                    country = nonEmpties.Last();
                }
            }

            return new Address(street, city, country, zipCode);
        }

        var nonEmptyParts = rawParts.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
        if (nonEmptyParts.Length == 1)
        {
            return new Address(string.Empty, string.Empty, nonEmptyParts[0], string.Empty);
        }
        if (nonEmptyParts.Length == 2)
        {
            return new Address(nonEmptyParts[0], string.Empty, nonEmptyParts[1], string.Empty);
        }
        if (nonEmptyParts.Length == 3)
        {
            return new Address(nonEmptyParts[0], nonEmptyParts[1], nonEmptyParts[2], string.Empty);
        }

        return new Address(string.Empty, string.Empty, string.Empty, string.Empty);
    }

}
