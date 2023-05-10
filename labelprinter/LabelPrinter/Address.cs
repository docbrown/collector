using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LabelPrinter;

public abstract class Address
{
    private string[]? _lines;

    public string[] Lines
    {
        get
        {
            if (_lines == null)
            {
                _lines = FormatLines();
            }
            return _lines;
        }
    }

    protected abstract string[] FormatLines();

    protected void SetProperty<T>(ref T field, T value)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            _lines = null;
        }
    }
}

public class DomesticAddress : Address
{
    private string _name = "";
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _line1 = "";
    public string Line1
    {
        get => _line1;
        set => SetProperty(ref _line1, value);
    }

    private string _line2 = "";
    public string Line2
    {
        get => _line2;
        set => SetProperty(ref _line2, value);
    }

    private string _city = "";
    public string City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }

    private string _state = "";
    public string State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    private string _zip5 = "";
    public string Zip5
    {
        get => _zip5;
        set => SetProperty(ref _zip5, value);
    }

    private string _zip4 = "";
    public string Zip4
    {
        get => _zip4;
        set => SetProperty(ref _zip4, value);
    }

    public string FullZip
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Zip4))
            {
                return Zip5;
            }
            else
            {
                return Zip5 + "-" + Zip4;
            }
        }
        set
        {
            (Zip5, Zip4) = SplitZip(value);
        }
    }

    public static (string, string) SplitZip(string zip)
    {
        var m = Regex.Match(zip.Trim(), "^(\\d{5})(?:-(\\d{4}))?$");
        if (m.Success)
        {
            return (m.Groups[1].Value, m.Groups[2].Value);
        }
        else
        {
            return (zip, "");
        }
    }

    protected override string[] FormatLines()
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(Name))
        {
            lines.Add(Name);
        }
        if (!string.IsNullOrWhiteSpace(Line1))
        {
            lines.Add(Line1);
        }
        if (!string.IsNullOrWhiteSpace(Line2))
        {
            lines.Add(Line2);
        }
        lines.Add($"{City} {State}  {FullZip}");
        return lines.ToArray();
    }

    public InternationalAddress ToInternationalAddress()
    {
        return new InternationalAddress
        {
            Text = ToString(),
            Country = "UNITED STATES"
        };
    }

    public override string ToString()
    {
        return string.Join("\n", Lines);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        var other = (DomesticAddress)obj;
        return this.Name == other.Name &&
            this.Line1 == other.Line1 &&
            this.Line2 == other.Line2 &&
            this.City == other.City &&
            this.State == other.State &&
            this.Zip5 == other.Zip5 &&
            this.Zip4 == other.Zip4;
    }
}

/// <summary>
/// A <see cref="DomesticAddress"/> that has been CASS-validated.
/// </summary>
public class ValidatedDomesticAddress : DomesticAddress
{
    public DomesticAddress OriginalAddress { get; set; }
    public string DeliveryPoint { get; set; }
    public string CarrierRoute { get; set; }
    public bool IsBusiness { get; set; }
    public bool IsCentralDeliveryPoint { get; set; }
    public bool IsVacant { get; set; }

    internal ValidatedDomesticAddress(UspsWebTools.AddressValidateResponseModel.AddressModel model, DomesticAddress originalAddress)
    {
        OriginalAddress = originalAddress;
        Name = model.FirmName ?? "";
        Line1 = model.Address1 ?? "";
        Line2 = model.Address2 ?? "";
        City = model.City ?? "";
        State = model.State ?? "";
        Zip5 = model.Zip5 ?? "";
        Zip4 = model.Zip4 ?? "";
        DeliveryPoint = model.DeliveryPoint ?? "";
        CarrierRoute = model.CarrierRoute ?? "";
        IsBusiness = model.Business == "Y";
        IsCentralDeliveryPoint = model.CentralDeliveryPoint == "Y";
        IsVacant = model.Vacant == "Y";
    }
}

public class InternationalAddress : Address
{
    private string _text = "";
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    private string _country = "";
    public string Country
    {
        get => _country;
        set => SetProperty(ref _country, value);
    }

    protected override string[] FormatLines()
    {
        var lines = _text.Split('\n').ToList();
        lines.Add(_country);
        return lines.ToArray();
    }
}
