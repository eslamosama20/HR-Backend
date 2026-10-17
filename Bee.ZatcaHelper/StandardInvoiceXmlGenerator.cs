using System.Xml;
using Bee.ZatcaHelper.Model;
using Bee.ZatcaHelper.Util;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Numerics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
namespace Bee.ZatcaHelper;

public static partial class StandardInvoiceXmlGenerator
{
    private const string StandardInvoiceXmlFormat = "Standard_Invoice_Template.xml";
    private const string SimplifiedInvoiceXmlFormat = "Simplified_Invoice_Template.xml";
    private const string SimplifiedCreditNoteXmlFormat = "Simplified_Credit_Note_Template.xml";
    private const string SimplifiedDebitNoteXmlFormat = "Simplified_Debit_Note_Template.xml";
    private static string? _currentHash;
    private static string? _signedHashWithPrivateKey;
    public static object GenerateForDebitNote(StandardInvoiceAdjustment standardInvoiceAdjustment, string? pemPrivateKey = null, string? prodCsidToken = null)
    {
        const string debitNoteXmlFormat = "Standard_Debit_Note_Template.xml";
        return GenerateForAdjustment(standardInvoiceAdjustment, debitNoteXmlFormat, pemPrivateKey, prodCsidToken);
    }

    public static object GenerateForCreditNote(StandardInvoiceAdjustment standardInvoiceAdjustment, string? pemPrivateKey = null, string? prodCsidToken = null)
    {
        const string creditNoteXmlFormat = "Standard_Credit_Note_Template.xml";
        return GenerateForAdjustment(standardInvoiceAdjustment, creditNoteXmlFormat, pemPrivateKey, prodCsidToken);
    }

    private static object GenerateForAdjustment(StandardInvoiceAdjustment standardInvoiceAdjustment,
        string noteXmlFormat, string privateKeyPem, string prodCsidToken)
    {
        var xmlDoc = new XmlDocument { PreserveWhitespace = true };
        var xmlString = typeof(StandardInvoiceXmlGenerator).GetFileContent(noteXmlFormat);
        xmlDoc.LoadXml(xmlString);

        var standardInvoice = (StandardInvoice)standardInvoiceAdjustment;
        PopulateInvoiceBasicInfo(xmlDoc, standardInvoice);
        PopulateSupplierInfo(xmlDoc, standardInvoice);
        PopulateCustomerInfo(xmlDoc, standardInvoice);
        PopulateDiscountInfo(xmlDoc, standardInvoice);
        PopulateTaxTotals(xmlDoc, standardInvoice);
        PopulateInvoiceLineItems(xmlDoc, standardInvoice);
        PopulateTotals(xmlDoc, standardInvoice);
        xmlDoc.SetNodeValue(InvoiceReferenceXpath, standardInvoiceAdjustment.AdjsutmentInvoiceNumber);
        xmlDoc.SetNodeValue(AdjustmentReasonXpath, standardInvoiceAdjustment.AdjustmentReason);
        PopulateIcvAdditionalDocumentReference(xmlDoc, standardInvoice);
        // --- QR Code Generation and Embedding for Standard Documents ---
        PopulateInvoiceHash(xmlDoc);

        if (_currentHash != null && privateKeyPem != null && prodCsidToken != null)
        {
            //SetQrCodeNode(xmlDoc, standardInvoice, _currentHash, prodCsidToken, privateKeyPem);
            SignHashWithPrivateKey(_currentHash, privateKeyPem);
            GenerateAndSetDigitalSignature(xmlDoc, _currentHash, privateKeyPem, prodCsidToken);
        }



        var xmlBytes = System.Text.Encoding.UTF8.GetBytes(xmlDoc.OuterXml);
        var resultAPiCall = new XmlGenerationResult
        {
            invoiceHash = _currentHash ?? string.Empty,
            uuid = standardInvoice.UUID,
            invoice = Convert.ToBase64String(xmlBytes),
            invoiceXml = xmlDoc
        };
        return resultAPiCall;
    }

    public static XmlGenerationResult GenerateSimplifiedInvoice(StandardInvoice standardInvoice, string? pemPrivateKey = null, string? prodCsidToken = null)
    {
        return GenerateSimplified(SimplifiedInvoiceXmlFormat, standardInvoice, pemPrivateKey, prodCsidToken);
    }

    public static XmlGenerationResult GenerateSimplifiedCreditNote(StandardInvoiceAdjustment standardInvoiceAdjustment, string? pemPrivateKey = null, string? prodCsidToken = null)
    {
        return GenerateSimplified(SimplifiedCreditNoteXmlFormat, standardInvoiceAdjustment, pemPrivateKey, prodCsidToken);
    }

    public static XmlGenerationResult GenerateSimplifiedDebitNote(StandardInvoiceAdjustment standardInvoiceAdjustment, string? pemPrivateKey = null, string? prodCsidToken = null)
    {
        return GenerateSimplified(SimplifiedDebitNoteXmlFormat, standardInvoiceAdjustment, pemPrivateKey, prodCsidToken);
    }

    public static XmlGenerationResult Generate(StandardInvoice standardInvoice, string? pemPrivateKey = null, string? prodCsidToken = null)
    {

        var xmlDoc = new XmlDocument
        {
            PreserveWhitespace = true
        };
        var xmlString = typeof(StandardInvoiceXmlGenerator).GetFileContent(StandardInvoiceXmlFormat);


        xmlDoc.LoadXml(xmlString);
        PopulateInvoiceBasicInfo(xmlDoc, standardInvoice);
        PopulateSupplierInfo(xmlDoc, standardInvoice);
        PopulateCustomerInfo(xmlDoc, standardInvoice);
        PopulateDiscountInfo(xmlDoc, standardInvoice);
        PopulateTaxTotals(xmlDoc, standardInvoice);
        PopulateInvoiceLineItems(xmlDoc, standardInvoice);
        PopulateTotals(xmlDoc, standardInvoice);
        PopulateIcvAdditionalDocumentReference(xmlDoc, standardInvoice);
        PopulateInvoiceHash(xmlDoc);
        if (_currentHash != null && pemPrivateKey != null && prodCsidToken != null)
        {
            SignHashWithPrivateKey(_currentHash, pemPrivateKey);
            GenerateAndSetDigitalSignature(xmlDoc, _currentHash, pemPrivateKey, prodCsidToken);
        }


        var xmlBytes = System.Text.Encoding.UTF8.GetBytes(xmlDoc.OuterXml);
        var resultAPiCall = new XmlGenerationResult
        {
            invoiceHash = _currentHash,
            uuid = standardInvoice.UUID,
            invoice = Convert.ToBase64String(xmlBytes),
            invoiceXml = xmlDoc
        };


        return resultAPiCall;
    }



    private static XmlGenerationResult GenerateSimplified(string templateFile, StandardInvoice standardInvoice, string? pemPrivateKey = null, string? prodCsidToken = null)
    {
        var xmlDoc = new XmlDocument { PreserveWhitespace = true };
        var xmlString = typeof(StandardInvoiceXmlGenerator).GetFileContent(templateFile);
        if (string.IsNullOrEmpty(xmlString))
            throw new InvalidOperationException($"Template XML '{templateFile}' not found or empty.");
        xmlDoc.LoadXml(xmlString);
        PopulateInvoiceBasicInfo(xmlDoc, standardInvoice);
        PopulateSupplierInfo(xmlDoc, standardInvoice);
        PopulateCustomerInfo(xmlDoc, standardInvoice);
        PopulateDiscountInfo(xmlDoc, standardInvoice);
        PopulateTaxTotals(xmlDoc, standardInvoice);
        PopulateInvoiceLineItems(xmlDoc, standardInvoice);
        PopulateTotals(xmlDoc, standardInvoice);
        PopulateIcvAdditionalDocumentReference(xmlDoc, standardInvoice);
        PopulateInvoiceHash(xmlDoc);

        var generatedQr = string.Empty;
        if (_currentHash != null && pemPrivateKey != null && prodCsidToken != null)
        {
            SignHashWithPrivateKey(_currentHash, pemPrivateKey);
            GenerateAndSetDigitalSignature(xmlDoc, _currentHash, pemPrivateKey, prodCsidToken);
            generatedQr = SetQrCodeNode(xmlDoc, standardInvoice, _currentHash, prodCsidToken, pemPrivateKey, true);
        }


        var xmlBytes = System.Text.Encoding.UTF8.GetBytes(xmlDoc.OuterXml);
        var resultAPiCall = new XmlGenerationResult
        {
            invoiceHash = _currentHash ?? string.Empty,
            uuid = standardInvoice.UUID,
            invoice = Convert.ToBase64String(xmlBytes),
            invoiceXml = xmlDoc,
            GeneratedQR = generatedQr
        };
        return resultAPiCall;
    }

    private static void PopulateInvoiceHash(XmlDocument xmlDoc)
    {
        _currentHash = InvoiceHashHelper.GenerateEInvoiceHashing(xmlDoc.OuterXml);
        xmlDoc.SetNodeValue(HashXpath, _currentHash);
    }

    private static void PopulateTotals(XmlDocument xmlDoc, StandardInvoice standardInvoice)
    {
        xmlDoc.SetCurrencyNodeValue(LegalMonetaryTotalLineExtensionAmountXpath, standardInvoice.LegalMonetaryTotal.LineExtensionAmount);
        xmlDoc.SetCurrencyNodeValue(LegalMonetaryTotalTaxExclusiveAmountXpath, standardInvoice.LegalMonetaryTotal.TaxExclusiveAmount);
        xmlDoc.SetCurrencyNodeValue(LegalMonetaryTotalTaxInclusiveAmountXpath, standardInvoice.LegalMonetaryTotal.TaxInclusiveAmount);
        xmlDoc.SetCurrencyNodeValue(LegalMonetaryTotalAllowanceTotalAmountXpath, standardInvoice.LegalMonetaryTotal.AllowanceTotalAmount);
        xmlDoc.SetCurrencyNodeValue(LegalMonetaryTotalAllowanceTotalAmountXpath, standardInvoice.LegalMonetaryTotal.AllowanceTotalAmount);
        xmlDoc.SetCurrencyNodeValue(LegalMonetaryTotalPrepaidAmountXpath, standardInvoice.LegalMonetaryTotal.PrepaidAmount);
        xmlDoc.SetCurrencyNodeValue(LegalMonetaryTotalPayableAmountXpath, standardInvoice.LegalMonetaryTotal.PayableAmount);
    }

    private static void PopulateInvoiceLineItems(XmlDocument xmlDoc, StandardInvoice standardInvoice)
    {
        const string CBC_NS = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        const string CAC_NS = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        // 1) Locate the parent node that holds all InvoiceLine entries
        var invoiceLinesParent = xmlDoc.SelectSingleNode("/*[local-name() = 'Invoice']")
                                 ?? throw new InvalidOperationException("InvoiceLine parent node not found.");

        // 2) Locate the template line from within the parent
        var templateLine = invoiceLinesParent.SelectSingleNode(LineItemNodeXpath)
                            ?? throw new InvalidOperationException("InvoiceLine template node not found.");


        // 4) For each business line item, clone & populate
        foreach (var invoiceLineItem in standardInvoice.InvoiceLineItems)
        {
            var newLine = templateLine.CloneNode(true);
            newLine.RemoveAll();
            var ID = xmlDoc.CreateElement("cbc", "ID", CBC_NS);
            ID.InnerText = invoiceLineItem.ID;
            var InvoicedQuantity = xmlDoc.CreateElement("cbc", "InvoicedQuantity", CBC_NS);
            InvoicedQuantity.InnerText = invoiceLineItem.InvoicedQuantity;
            var LineExtensionAmount = xmlDoc.CreateElement("cbc", "LineExtensionAmount", CBC_NS);
            LineExtensionAmount.InnerText = invoiceLineItem.LineExtensionAmount.GetAmountString();
            LineExtensionAmount.SetAttribute("currencyID", "SAR");
            newLine.AppendChild(ID);
            newLine.AppendChild(InvoicedQuantity);
            newLine.AppendChild(LineExtensionAmount);

            var TaxTotal = xmlDoc.CreateElement("cac", "TaxTotal", CAC_NS);
            var TaxAmount = xmlDoc.CreateElement("cbc", "TaxAmount", CBC_NS);
            TaxAmount.SetAttribute("currencyID", "SAR");
            TaxAmount.InnerText = invoiceLineItem.TaxAmount.GetAmountString();
            var RoundingAmount = xmlDoc.CreateElement("cbc", "RoundingAmount", CBC_NS);
            RoundingAmount.InnerText = invoiceLineItem.RoundingAmount.GetAmountString();
            RoundingAmount.SetAttribute("currencyID", "SAR");
            TaxTotal.AppendChild(TaxAmount);
            TaxTotal.AppendChild(RoundingAmount);
            newLine.AppendChild(TaxTotal);

            var Item = xmlDoc.CreateElement("cac", "Item", CAC_NS);
            var Name = xmlDoc.CreateElement("cbc", "Name", CBC_NS);
            Name.InnerText = invoiceLineItem.ItemName;
            Item.AppendChild(Name);
            var ClassifiedTaxCategory = xmlDoc.CreateElement("cac", "ClassifiedTaxCategory", CAC_NS);
            var ID2 = xmlDoc.CreateElement("cbc", "ID", CBC_NS);
            ID2.InnerText = invoiceLineItem.TaxSchemeId;
            var Percent = xmlDoc.CreateElement("cbc", "Percent", CBC_NS);
            Percent.InnerText = invoiceLineItem.TaxPercent;
            ClassifiedTaxCategory.AppendChild(ID2);
            ClassifiedTaxCategory.AppendChild(Percent);
            var TaxScheme = xmlDoc.CreateElement("cac", "TaxScheme", CAC_NS);
            var ID3 = xmlDoc.CreateElement("cbc", "ID", CBC_NS);
            ID3.InnerText = invoiceLineItem.TaxScheme;
            TaxScheme.AppendChild(ID3);
            ClassifiedTaxCategory.AppendChild(TaxScheme);
            Item.AppendChild(ClassifiedTaxCategory);
            newLine.AppendChild(Item);

            var Price = xmlDoc.CreateElement("cac", "Price", CAC_NS);
            var PriceAmount = xmlDoc.CreateElement("cbc", "PriceAmount", CBC_NS);
            PriceAmount.InnerText = invoiceLineItem.PriceAmount.GetAmountString();
            PriceAmount.SetAttribute("currencyID", "SAR");
            Price.AppendChild(PriceAmount);
            newLine.AppendChild(Price);
            // Populate each field inside the newLine, not globally on xmlDoc
            invoiceLinesParent.AppendChild(newLine);
        }
        invoiceLinesParent.RemoveChild(templateLine);
        // 5) Clean-up stale nodes if necessary
        xmlDoc.RemoveNode(LineItemAllowanceXpath);
    }



    private static void PopulateTaxTotals(XmlDocument xmlDoc, StandardInvoice standardInvoice)
    {
        //TODO: Need to handle tax exemption

        xmlDoc.SetCurrencyNodeValue(TaxTotalTaxAmountXpath, standardInvoice.TaxTotal.TaxAmount);
        xmlDoc.SetCurrencyNodeValue(TaxTotalSubTotalTaxableAmountXpath, standardInvoice.TaxTotal.TaxableAmount);
        xmlDoc.SetCurrencyNodeValue(TaxTotalSubTotalTaxAmountXpath, standardInvoice.TaxTotal.TaxAmount);
        xmlDoc.SetNodeValue(TaxTotalSubTotalTaxCategoryIdTaxAmountXpath, standardInvoice.TaxTotal.TaxCategory);
        xmlDoc.SetNodeValue(TaxTotalSubTotalTaxPercentTaxAmountXpath, standardInvoice.TaxTotal.Percent);

        xmlDoc.SetCurrencyNodeValue(TaxTotalTaxAmountSecondTagXpath, standardInvoice.TaxTotal.TaxAmount);

    }

    private static void PopulateDiscountInfo(XmlDocument xmlDoc, StandardInvoice standardInvoice)
    {
        const string CBC_NS = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        const string CAC_NS = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

        var allowanceNode = xmlDoc.SelectSingleNode(AllowanceXpath);
        if (allowanceNode != null)
        {
            while (allowanceNode.HasChildNodes)
            {
                allowanceNode.RemoveChild(allowanceNode.FirstChild!);
            }
            var allowanceChargeId = xmlDoc.CreateElement("cbc", "ID", CBC_NS);
            allowanceChargeId.InnerText = "1";
            var chargeIndicator = xmlDoc.CreateElement("cbc", "ChargeIndicator", CBC_NS);
            chargeIndicator.InnerText = "false";
            allowanceNode.AppendChild(chargeIndicator);
            var reason = xmlDoc.CreateElement("cbc", "AllowanceChargeReason", CBC_NS);
            reason.InnerText = standardInvoice.DocumentLevelDiscountReason;
            allowanceNode.AppendChild(reason);
            var amount = xmlDoc.CreateElement("cbc", "Amount", CBC_NS);
            amount.SetAttribute("currencyID", standardInvoice.DocumentCurrencyCode);
            amount.InnerText = (standardInvoice.DocumentLevelDiscountAmount ?? 0m).ToString("F4");
            allowanceNode.AppendChild(amount);
            var taxCategory = xmlDoc.CreateElement("cac", "TaxCategory", CAC_NS);
            var id = xmlDoc.CreateElement("cbc", "ID", CBC_NS);
            id.SetAttribute("schemeID", "UN/ECE 5305");
            id.SetAttribute("schemeAgencyID", "6");
            id.InnerText = "S";
            taxCategory.AppendChild(id);
            var percent = xmlDoc.CreateElement("cbc", "Percent", CBC_NS);
            percent.InnerText = "15";
            taxCategory.AppendChild(percent);
            var taxScheme = xmlDoc.CreateElement("cac", "TaxScheme", CAC_NS);
            var schemeId = xmlDoc.CreateElement("cbc", "ID", CBC_NS);
            schemeId.InnerText = "VAT";
            schemeId.SetAttribute("schemeID", "UN/ECE 5153");
            schemeId.SetAttribute("schemeAgencyID", "6");
            taxScheme.AppendChild(schemeId);
            taxCategory.AppendChild(taxScheme);
            allowanceNode.AppendChild(taxCategory);
            // Insert before LegalMonetaryTotal:
            var parentNode = xmlDoc.SelectSingleNode("//cac:Invoice", GetNamespaceManager(xmlDoc));
            parentNode?.InsertBefore(allowanceNode,
                parentNode.SelectSingleNode("cac:LegalMonetaryTotal", GetNamespaceManager(xmlDoc)));
        }
    }

    private static XmlNamespaceManager GetNamespaceManager(XmlDocument xmlDoc)
    {
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        return nsmgr;
    }

    private static void PopulateCustomerInfo(XmlDocument xmlDoc, StandardInvoice standardInvoice)
    {
        if (standardInvoice.CustomerInfo.PartyPostalAddress != null)
        {
            xmlDoc.SetNodeValue(CustomerStreetNameXpath, standardInvoice.CustomerInfo.PartyPostalAddress.StreetName);
            xmlDoc.SetNodeValue(CustomerBuildingNumberXpath, standardInvoice.CustomerInfo.PartyPostalAddress.BuildingNumber);
            xmlDoc.SetNodeValue(CustomerPlotIdentificationXpath, standardInvoice.CustomerInfo.PartyPostalAddress.PlotIdentification);
            xmlDoc.SetNodeValue(CustomerCitySubdivisionNameXpath, standardInvoice.CustomerInfo.PartyPostalAddress.CitySubdivisionName);
            xmlDoc.SetNodeValue(CustomerCityNameXpath, standardInvoice.CustomerInfo.PartyPostalAddress.CityName);
            xmlDoc.SetNodeValue(CustomerPostalZoneXpath, standardInvoice.CustomerInfo.PartyPostalAddress.PostalZone);
            xmlDoc.SetNodeValue(CustomerCountrySubEntityXpath, standardInvoice.CustomerInfo.PartyPostalAddress.CountrySubentity);
            xmlDoc.SetNodeValue(CustomerCountryXpath, standardInvoice.CustomerInfo.PartyPostalAddress.Country);
        }
        xmlDoc.SetNodeValue(CustomerRegistrationNameXpath, standardInvoice.CustomerInfo.RegistrationName);
        xmlDoc.SetNodeValue(CustomerNatXpath, standardInvoice.CustomerInfo.NAT);
        xmlDoc.SetNodeValue(CustomerTaxSchemeCompanyIdXpath, standardInvoice.CustomerInfo.VAT);
        xmlDoc.SetNodeValue(CustomerTaxSchemeXpath, "VAT");
    }

    private static void PopulateSupplierInfo(XmlDocument xmlDoc, StandardInvoice standardInvoice)
    {
        xmlDoc.SetNodeValue(SupplierStreetNameXpath, standardInvoice.SupplierInfo.PartyPostalAddress.StreetName);
        xmlDoc.SetNodeValue(SupplierBuildingNumberXpath, standardInvoice.SupplierInfo.PartyPostalAddress.BuildingNumber);
        xmlDoc.SetNodeValue(SupplierPlotIdentificationXpath, standardInvoice.SupplierInfo.PartyPostalAddress.PlotIdentification);
        xmlDoc.SetNodeValue(SupplierCitySubdivisionNameXpath, standardInvoice.SupplierInfo.PartyPostalAddress.CitySubdivisionName);
        xmlDoc.SetNodeValue(SupplierCityNameXpath, standardInvoice.SupplierInfo.PartyPostalAddress.CityName);
        xmlDoc.SetNodeValue(SupplierPostalZoneXpath, standardInvoice.SupplierInfo.PartyPostalAddress.PostalZone);
        xmlDoc.SetNodeValue(SupplierCountrySubEntityXpath, standardInvoice.SupplierInfo.PartyPostalAddress.CountrySubentity);
        xmlDoc.SetNodeValue(SupplierCountryXpath, standardInvoice.SupplierInfo.PartyPostalAddress.Country);
        xmlDoc.SetNodeValue(SupplierRegistrationNameXpath, standardInvoice.SupplierInfo.RegistrationName);
        xmlDoc.SetNodeValue(SupplierTaxSchemeXpath, "VAT");
        xmlDoc.SetNodeValue(SupplierTaxSchemeCompanyIdXpath, standardInvoice.SupplierInfo.VAT);
        xmlDoc.SetNodeValue(SupplierCrnXpath, standardInvoice.SupplierInfo.CRN);
    }

    private static void PopulateInvoiceBasicInfo(XmlDocument xmlDoc, StandardInvoice standardInvoice)
    {
        xmlDoc.SetNodeValue(InvoiceIdXpath, standardInvoice.Id);
        xmlDoc.SetNodeValue(InvoiceUuidXpath, standardInvoice.UUID);
        xmlDoc.SetNodeValue(InvoiceDocumentCurrencyCodeXpath, standardInvoice.DocumentCurrencyCode);
        xmlDoc.SetNodeValue(InvoiceTaxCurrencyCodeXpath, standardInvoice.TaxCurrencyCode);
        xmlDoc.SetNodeValue(InvoiceIssueDateXpath, standardInvoice.IssuedDate);
        xmlDoc.SetNodeValue(InvoiceIssueTimeXpath, standardInvoice.IssuedTime);

        xmlDoc.SetNodeValue(InvoiceLatestDeliveryDateXpath, standardInvoice.LatestDeliveryDate);
        xmlDoc.SetNodeValue(InvoiceActualDeliveryDateXpath, standardInvoice.ActualDeliveryDate);

        xmlDoc.SetNodeValue(InvoicePreviousInvoiceHashXpath, standardInvoice.PreviousInvoiceHash);
    }

    private static void PopulateIcvAdditionalDocumentReference(XmlDocument xmlDoc, StandardInvoice standardInvoice)
    {
        // Find the AdditionalDocumentReference node with <cbc:ID>ICV</cbc:ID>
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        var icvNode = xmlDoc.SelectSingleNode("//cac:AdditionalDocumentReference[cbc:ID='ICV']", nsmgr);
        if (icvNode != null)
        {
            // Extract the numeric part from the invoice id (format ##-000)
            var id = standardInvoice.Id;
            string icvValue = "";
            if (!string.IsNullOrEmpty(id))
            {
                var parts = id.Split('-');
                if (parts.Length > 1)
                {
                    icvValue = parts[1];
                }
            }
            var uuidNode = icvNode.SelectSingleNode("cbc:UUID", nsmgr);
            if (uuidNode != null)
            {
                uuidNode.InnerText = icvValue;
            }
            else
            {
                // If UUID node doesn't exist, create it
                var uuidElem = xmlDoc.CreateElement("cbc", "UUID", nsmgr.LookupNamespace("cbc"));
                uuidElem.InnerText = icvValue;
                icvNode.AppendChild(uuidElem);
            }
        }
    }


    // Removes the SignatureInformation block from the template XML.
    private static void ClearSignatureInformation(XmlDocument xmlDoc)
    {
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sig", "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2");
        nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
        var signatureInfoNode = xmlDoc.SelectSingleNode("//*[local-name()='SignatureInformation']", nsmgr);
        if (signatureInfoNode != null && signatureInfoNode.ParentNode != null)
        {
            signatureInfoNode.ParentNode.RemoveChild(signatureInfoNode);
        }
    }

    // Inserts a new SignatureInformation block with the provided XML fragment (as string or XmlNode).
    public static void SetSignatureInformation(XmlDocument xmlDoc, string signatureInformationXml)
    {
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("sig", "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2");
        nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
        nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        var ublDocSigs = xmlDoc.SelectSingleNode("//*[local-name()='UBLDocumentSignatures']", nsmgr);
        if (ublDocSigs == null)
            throw new InvalidOperationException("UBLDocumentSignatures node not found in XML");
        var sigDoc = new XmlDocument { PreserveWhitespace = true };
        sigDoc.LoadXml(signatureInformationXml);
        if (sigDoc.DocumentElement == null)
            throw new InvalidOperationException("SignatureInformation XML fragment is invalid or empty.");
        var imported = xmlDoc.ImportNode(sigDoc.DocumentElement, true);
        ublDocSigs.AppendChild(imported);
    }
    
    public static void GenerateAndSetDigitalSignature(XmlDocument xmlDoc, string invoiceHash, string privateKeyPem, string prodCsidToken)
    {
        if (_signedHashWithPrivateKey == null) throw new Exception("Hash not signed");

        string signatureValue = _signedHashWithPrivateKey;
        var (x509Certificate2, x509CertificateBase64, x509IssuerName, x509SerialNumber) = ExtractCertificateFromCsr(prodCsidToken);
        
        //string certHashBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(InvoiceHashHelper.HashSha256AsString(prodCsidTokenDecoded)));
        var prodCsidTokenBytes = Convert.FromBase64String(prodCsidToken);
        var prodCsidTokenDecoded = Encoding.UTF8.GetString(prodCsidTokenBytes);
        string certHexString = InvoiceHashHelper.HashSha256AsString(prodCsidTokenDecoded);
        byte[] certHexBytes = Encoding.UTF8.GetBytes(certHexString);
        string certHashBase64 = Convert.ToBase64String(certHexBytes);
        
        string signingTime = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss");

        // Update all values in the document
        xmlDoc.SetNodeValue("//*[local-name()='Reference' and @Id='invoiceSignedData']/*[local-name()='DigestValue']", invoiceHash);
        xmlDoc.SetNodeValue("//*[local-name()='SignatureValue']", signatureValue);
        xmlDoc.SetNodeValue("//*[local-name()='X509Certificate']", x509CertificateBase64);
        xmlDoc.SetNodeValue("//*[local-name()='X509IssuerName']", x509IssuerName.Split(",")[0]);
        xmlDoc.SetNodeValue("//*[local-name()='X509SerialNumber']", x509SerialNumber);
        xmlDoc.SetNodeValue("//*[local-name()='SigningTime']", signingTime);
        xmlDoc.SetNodeValue("//*[local-name()='CertDigest']/*[local-name()='DigestValue']", certHashBase64);

        
        var signedPropsXml = XmlHelper
            .GetNodeInnerXML(xmlDoc, "//*[local-name()='Invoice']//*[local-name()='UBLExtensions']//*[local-name()='ExtensionContent']//*[local-name()='UBLDocumentSignatures']//*[local-name()='SignatureInformation']//*[local-name()='Signature']//*[local-name()='QualifyingProperties']//*[local-name()='SignedProperties']")
            .Replace(" />", "/>").Replace("></ds:DigestMethod>", "/>").Replace("\r", "");
        if (signedPropsXml != null)
        {
            string hexHash = InvoiceHashHelper.HashSha256AsString(signedPropsXml);
            byte[] hexBytes = Encoding.UTF8.GetBytes(hexHash);
            string base64Hash = Convert.ToBase64String(hexBytes);
            xmlDoc.SetNodeValue("//*[local-name()='Reference' and @Type='http://www.w3.org/2000/09/xmldsig#SignatureProperties']/*[local-name()='DigestValue']", base64Hash);
            
        }
    }

    // Placeholder for ECDSA-SHA256 signing. In production, use a secure library.
    private static void SignHashWithPrivateKey(string hash, string privateKeyPem)
    {
        byte[] hashBytes = Convert.FromBase64String(hash);
        AsymmetricKeyParameter? privateKeyParams = null;
        using (var reader = new StringReader(privateKeyPem))
        {
            var pemReader = new PemReader(reader);
            var pemObject = pemReader.ReadObject();
            if (pemObject is AsymmetricCipherKeyPair keyPair)
            {
                privateKeyParams = keyPair.Private;
            }
            else if (pemObject is AsymmetricKeyParameter keyParam && keyParam.IsPrivate)
            {
                privateKeyParams = keyParam;
            }
            else
            {
                throw new Exception("Private key PEM does not contain a valid EC private key");
            }
        }
        if (privateKeyParams is not ECPrivateKeyParameters ecPrivateKey)
            throw new Exception("Private key is not EC. Please provide a valid EC private key in PEM format.");
        var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
        signer.Init(true, ecPrivateKey);
        signer.BlockUpdate(hashBytes, 0, hashBytes.Length);
        var signature = signer.GenerateSignature();
        _signedHashWithPrivateKey = Convert.ToBase64String(signature);
    }

    // Extracts the base64 certificate, X509IssuerName, and X509SerialNumber from the PEM certificate
    private static (X509Certificate2 x509Certificate2, string Base64Certificate, string IssuerName, string SerialNumber) ExtractCertificateFromCsr(string pemCertificate)
    {
        var pemCertificateBase64 = Convert.FromBase64String(pemCertificate);
        var cert = Encoding.UTF8.GetString(pemCertificateBase64);
        string base64 = cert
            .Replace("-----BEGIN CERTIFICATE-----", "")
            .Replace("-----END CERTIFICATE-----", "")
            .Replace("\r", "")
            .Replace("\n", "")
            .Trim();
        byte[] certBytes = Convert.FromBase64String(base64);
        var certificate2 = new X509Certificate2(certBytes);
        string base64Cert = Convert.ToBase64String(certificate2.RawData);
        string issuer = certificate2.IssuerName.Name;
        string serial = new BigInteger(certificate2.GetSerialNumber()).ToString();
        // ZATCA expects serial as a decimal string, not hex
        //var serialDecimal = BigInteger.Parse(certificate2.SerialNumber, System.Globalization.NumberStyles.HexNumber).ToString();
        return (certificate2, base64Cert, issuer, serial);
    }

    // Generates the ZATCA TLV QR code, encodes as base64, and sets it in the XML node for QR (AdditionalDocumentReference/ID='QR')
    private static string SetQrCodeNode(XmlDocument xmlDoc, StandardInvoice standardInvoice, string invoiceHash, string prodCsidToken, string privateKeyPem, bool isSimplified = false)
    {

        var pemCertificateBase64 = Convert.FromBase64String(prodCsidToken);
        var cert = Encoding.UTF8.GetString(pemCertificateBase64);
        if (_signedHashWithPrivateKey == null) throw new Exception("Hash not signed");
        string signatureValue = _signedHashWithPrivateKey;

        // Build TLV fields as per ZATCA: Seller Name, VAT, Timestamp, Invoice Total, VAT Total, Invoice Hash, Signature, Public Key, Certificate Signature
        string sellerName = standardInvoice.SupplierInfo.RegistrationName ?? string.Empty;
        string vatNumber = standardInvoice.SupplierInfo.VAT ?? string.Empty;
        string timestamp = standardInvoice.IssuedDate + "T" + standardInvoice.IssuedTime;

        string invoiceTotal = (standardInvoice.LegalMonetaryTotal?.PayableAmount?.Amount ?? 0.0).ToString("F2", CultureInfo.InvariantCulture);
        string vatTotal = (standardInvoice.TaxTotal?.TaxAmount?.Amount ?? 0.0).ToString("F2", CultureInfo.InvariantCulture);

        SortedDictionary<int, byte[]> invoiceDetails = new SortedDictionary<int, byte[]>()
        {
            {
                1,
                Encoding.UTF8.GetBytes(sellerName)
            },
            {
                2,
                Encoding.UTF8.GetBytes(vatNumber)
            },
            {
                3,
                Encoding.UTF8.GetBytes(timestamp)
            },
            {
                4,
                Encoding.UTF8.GetBytes(invoiceTotal)
            },
            {
                5,
                Encoding.UTF8.GetBytes(vatTotal)
            },
            {
                6,
                Encoding.UTF8.GetBytes(invoiceHash)
            },
            {
                7,
                Encoding.UTF8.GetBytes(signatureValue)
            },
        };
        byte[] publicKey;
        byte[] certificateSignature;
        TryGetPublicKeySByteArray(cert, out publicKey, out certificateSignature);
        invoiceDetails.Add(8, publicKey);
        if (isSimplified)
        {
            invoiceDetails.Add(9, certificateSignature);
        }
        var generatedQr = GenerateQrCodeFromValues(invoiceDetails);
        //byte[] tlv = BuildZatcaTlvQr(sellerName, vatNumber, timestamp, invoiceTotal, vatTotal,invoiceHash, signatureValue, publicKey,certificateSignature );
        //string base64Qr = Convert.ToBase64String(tlv);

        // Find the AdditionalDocumentReference node with <cbc:ID>QR</cbc:ID>
        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        var qrNode = xmlDoc.SelectSingleNode(QrCodeXpath, nsmgr);
        if (qrNode != null)
        {
            qrNode.InnerText = generatedQr;
        }
        return generatedQr;
    }
    private static string GenerateQrCodeFromValues(SortedDictionary<int, byte[]> invoiceDetails)
    {
        List<byte> byteList = new List<byte>();
        foreach (int key in invoiceDetails.Keys)
            byteList.AddRange((IEnumerable<byte>)WriteTlv((uint)key, invoiceDetails[key]));
        return Convert.ToBase64String(byteList.ToArray());
    }
    private static byte[] WriteTlv(uint tag, byte[] value)
    {
        if (value == null)
            throw new Exception("[Error] Please provide a value!");
        using (MemoryStream memoryStream = new MemoryStream())
        {
            WriteTag((Stream)memoryStream, tag);
            int length = value != null ? value.Length : 0;
            WriteLength((Stream)memoryStream, new int?(length));
            memoryStream.Write(value, 0, length);
            return memoryStream.ToArray();
        }
    }
    private static void WriteTag(Stream stream, uint tag)
    {
        bool flag = true;
        for (int index = 3; index >= 0; --index)
        {
            byte num = (byte)(tag >> 8 * index);
            if (!(num == (byte)0 & flag) || index <= 0)
            {
                if (flag)
                {
                    if (index == 0)
                    {
                        if (((int)num & 31) == 31)
                            throw new Exception("[Error] Invalid tag value: first octet indicates subsequent octets, but no subsequent octets found");
                    }
                    else if (((int)num & 31) != 31)
                        throw new Exception("[Error] Invalid tag value: first octet indicates no subsequent octets, but subsequent octets found");
                }
                else if (index == 0)
                {
                    if (((int)num & 128) == 128)
                        throw new Exception("[Error] Invalid tag value: last octet indicates subsequent octets");
                }
                else if (((int)num & 128) != 128)
                    throw new Exception("[Error] Invalid tag value: non-last octet indicates no subsequent octets");
                stream.WriteByte(num);
                flag = false;
            }
        }
    }

    private static void WriteLength(Stream stream, int? length)
    {
        if (!length.HasValue)
        {
            stream.WriteByte((byte)128);
        }
        else
        {
            int? nullable1 = length;
            int num1 = 0;
            if (!(nullable1.GetValueOrDefault() < num1 & nullable1.HasValue))
            {
                nullable1 = length;
                long? nullable2 = nullable1.HasValue ? new long?((long)nullable1.GetValueOrDefault()) : new long?();
                long maxValue1 = (long)uint.MaxValue;
                if (!(nullable2.GetValueOrDefault() > maxValue1 & nullable2.HasValue))
                {
                    nullable1 = length;
                    int maxValue2 = (int)sbyte.MaxValue;
                    if (nullable1.GetValueOrDefault() <= maxValue2 & nullable1.HasValue)
                    {
                        stream.WriteByte(checked((byte)length.Value));
                        return;
                    }
                    nullable1 = length;
                    int maxValue3 = (int)byte.MaxValue;
                    byte num2;
                    if (nullable1.GetValueOrDefault() <= maxValue3 & nullable1.HasValue)
                    {
                        num2 = (byte)1;
                    }
                    else
                    {
                        nullable1 = length;
                        int maxValue4 = (int)ushort.MaxValue;
                        if (nullable1.GetValueOrDefault() <= maxValue4 & nullable1.HasValue)
                        {
                            num2 = (byte)2;
                        }
                        else
                        {
                            nullable1 = length;
                            int num3 = 16777215;
                            if (nullable1.GetValueOrDefault() <= num3 & nullable1.HasValue)
                            {
                                num2 = (byte)3;
                            }
                            else
                            {
                                nullable1 = length;
                                nullable2 = nullable1.HasValue ? new long?((long)nullable1.GetValueOrDefault()) : new long?();
                                long maxValue5 = (long)uint.MaxValue;
                                if (!(nullable2.GetValueOrDefault() <= maxValue5 & nullable2.HasValue))
                                {
                                    var interpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
                                    interpolatedStringHandler.AppendLiteral("[Error] Length value too big: ");
                                    interpolatedStringHandler.AppendFormatted<int?>(length);
                                    throw new Exception(interpolatedStringHandler.ToStringAndClear());
                                }
                                num2 = (byte)4;
                            }
                        }
                    }
                    stream.WriteByte((byte)((uint)num2 | 128U));
                    for (int index = (int)num2 - 1; index >= 0; --index)
                    {
                        nullable1 = length;
                        int num4 = 8 * index;
                        byte num5 = (byte)(nullable1.HasValue ? new int?(nullable1.GetValueOrDefault() >> num4) : new int?()).Value;
                        stream.WriteByte(num5);
                    }
                    return;
                }
            }
            var interpolatedStringHandler1 = new DefaultInterpolatedStringHandler(30, 1);
            interpolatedStringHandler1.AppendLiteral("[Error] Invalid length value: ");
            interpolatedStringHandler1.AppendFormatted<int?>(length);
            throw new Exception(interpolatedStringHandler1.ToStringAndClear());
        }
    }

    // Helper to build ZATCA TLV QR code as per spec
    private static byte[] BuildZatcaTlvQr(string sellerName, string vatNumber,
        string timestamp, string invoiceTotal, string vatTotal, string invoiceHash, string signatureValue,
        string publicKey, string certificateSignature)
    {
        using (var ms = new MemoryStream())
        {
            WriteTlv(ms, 1, sellerName);
            WriteTlv(ms, 2, vatNumber);
            WriteTlv(ms, 3, timestamp);
            WriteTlv(ms, 4, invoiceTotal);
            WriteTlv(ms, 5, vatTotal);
            WriteTlv(ms, 6, invoiceHash);
            WriteTlv(ms, 7, signatureValue);
            WriteTlv(ms, 8, publicKey);
            WriteTlv(ms, 9, certificateSignature);
            return ms.ToArray();
        }
    }




    private static void WriteTlv(Stream stream, byte tag, string value)
    {
        byte[] valueBytes = Encoding.UTF8.GetBytes(value);
        stream.WriteByte(tag);

        if (valueBytes.Length < 128)
        {
            stream.WriteByte((byte)valueBytes.Length);
        }
        else if (valueBytes.Length <= 255)
        {
            stream.WriteByte(0x81);
            stream.WriteByte((byte)valueBytes.Length);
        }
        else
        {
            stream.WriteByte(0x82);
            stream.WriteByte((byte)(valueBytes.Length >> 8)); // High byte
            stream.WriteByte((byte)(valueBytes.Length & 0xFF)); // Low byte
        }

        stream.Write(valueBytes, 0, valueBytes.Length);
    }


    // Generates the ZATCA-compliant QR code for simplified invoices.
    // All numeric values (invoiceTotal, vatTotal) must be formatted as strings with two decimal places using InvariantCulture.
    public static string GenerateZatcaQrCode(string sellerName, string vatRegistrationNumber, decimal invoiceTotal, decimal vatTotal, string invoiceDate)
    {
        // Format numbers as required by ZATCA: two decimal places, dot as decimal separator
        string invoiceTotalStr = invoiceTotal.ToString("F2", CultureInfo.InvariantCulture);
        string vatTotalStr = vatTotal.ToString("F2", CultureInfo.InvariantCulture);
        string invoiceDateStr = invoiceDate;

        // TLV encoding: [tag][length][value]
        var tlv = new List<byte>();
        void AddTlv(int tag, string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            tlv.Add((byte)tag);
            tlv.Add((byte)valueBytes.Length);
            tlv.AddRange(valueBytes);
        }
        AddTlv(1, sellerName);
        AddTlv(2, vatRegistrationNumber);
        AddTlv(3, invoiceDateStr);
        AddTlv(4, invoiceTotalStr);
        AddTlv(5, vatTotalStr);
        return Convert.ToBase64String(tlv.ToArray());
    }

    private static void TryGetPublicKeySByteArray(
        string certificate,
        out byte[] publicKey,
        out byte[] certificateSignature)
    {
        try
        {
            Org.BouncyCastle.X509.X509Certificate x509Certificate = DotNetUtilities.FromX509Certificate((System.Security.Cryptography.X509Certificates.X509Certificate)new X509Certificate2(Convert.FromBase64String(certificate)));
            SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(x509Certificate.GetPublicKey());
            publicKey = subjectPublicKeyInfo.GetEncoded();
            certificateSignature = x509Certificate.GetSignature();
        }
        catch
        {
            throw new Exception("[Error] Invalid Certificate");
        }
    }
}


