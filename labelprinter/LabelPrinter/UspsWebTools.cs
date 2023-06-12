using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace LabelPrinter;

public class UspsWebToolsException : Exception
{
    internal UspsWebToolsException(UspsWebTools.ErrorModel model)
        : this(model.Description)
    {
        Source = model.Source;
    }

    public UspsWebToolsException()
    {
    }

    public UspsWebToolsException(string? message) : base(message)
    {
    }

    public UspsWebToolsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public static class UspsWebTools
{
    public static string? UserId { get; set; }

    [XmlRoot("Error")]
    public class ErrorModel
    {
        [XmlElement]
        public string? Source;

        [XmlElement]
        public string? Description;
    }

    [XmlRoot(ElementName = "AddressValidateResponse")]
    public class AddressValidateResponseModel
    {
        public class AddressModel
        {
            [XmlAttribute]
            public int ID;

            [XmlElement]
            public ErrorModel? Error;

            [XmlElement]
            public string? FirmName;

            [XmlElement]
            public string? Address1;

            [XmlElement]
            public string? Address2;

            [XmlElement]
            public string? City;

            [XmlElement]
            public string? State;

            [XmlElement]
            public string? Zip5;

            [XmlElement]
            public string? Zip4;

            [XmlElement]
            public string? DeliveryPoint;

            [XmlElement]
            public string? CarrierRoute;

            [XmlElement]
            public string? Footnotes;

            [XmlElement]
            public string? DPVConfirmation;

            [XmlElement]
            public string? DPVCMRA;

            [XmlElement]
            public string? DPVFootnotes;

            [XmlElement]
            public string? Business;

            [XmlElement]
            public string? CentralDeliveryPoint;

            [XmlElement]
            public string? Vacant;
        }

        [XmlElement("Address")]
        public AddressModel[]? Addresses;

        public static AddressValidateResponseModel? Parse(string xml)
        {
            var doc = XDocument.Parse(xml);
            using var reader = doc.CreateReader();
            switch (doc.Root?.Name.LocalName ?? "")
            {
                case "AddressValidateResponse":
                    {
                        var serializer = new XmlSerializer(typeof(AddressValidateResponseModel));
                        return serializer.Deserialize(reader) as AddressValidateResponseModel;
                    }
                case "Error":
                    {
                        var serializer = new XmlSerializer(typeof(ErrorModel));
                        var model = serializer.Deserialize(reader) as ErrorModel;
                        throw new UspsWebToolsException(model);
                    }
                default:
                    throw new ArgumentException("Unrecognized response XML root element", nameof(xml));
            }
        }
    }

    private static XDocument CreateAddressValidateRequest(DomesticAddress address)
    {
        var doc = new XDocument();
        var request = new XElement("AddressValidateRequest");
        request.SetAttributeValue("USERID", UserId);
        request.Add(new XElement("Revision", "1"));
        var elem = new XElement("Address");
        elem.SetAttributeValue("ID", "0");
        elem.Add(new XElement("FirmName", address.Name),
            new XElement("Address1", address.Line1),
            new XElement("Address2", address.Line2),
            new XElement("City", address.City),
            new XElement("State", address.State),
            new XElement("Zip5", address.Zip5),
            new XElement("Zip4", address.Zip4));
        request.Add(elem);
        doc.Add(request);
        return doc;
    }

    public static async Task<ValidatedDomesticAddress?> ValidateAddressAsync(DomesticAddress address)
    {
        var xml = CreateAddressValidateRequest(address).ToString(SaveOptions.DisableFormatting);
        var uri = new UriBuilder("https://secure.shippingapis.com/ShippingAPI.dll")
        {
            Query = "API=Verify&XML=" + Uri.EscapeDataString(xml)
        }.Uri;
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        var response = await client.GetAsync(uri);
        xml = await response.Content.ReadAsStringAsync();
        var model = AddressValidateResponseModel.Parse(xml);
        var result = model?.Addresses?[0];
        if (result == null)
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(result.Address1))
        {
            result.Address1 = result.Address2;
            result.Address2 = "";
        }
        return new ValidatedDomesticAddress(result, address);
    }
}
