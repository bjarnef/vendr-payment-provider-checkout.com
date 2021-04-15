using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom
{
    public class CheckoutSettingsBase
    {
        [PaymentProviderSetting(Name = "Continue URL",
            Description = "The URL to continue to after this provider has done processing. eg: /continue/",
            SortOrder = 100)]
        public string ContinueUrl { get; set; }

        [PaymentProviderSetting(Name = "Cancel URL",
            Description = "The URL to return to if the payment attempt is canceled. eg: /cancel/",
            SortOrder = 200)]
        public string CancelUrl { get; set; }

        [PaymentProviderSetting(Name = "Error URL",
            Description = "The URL to return to if the payment attempt errors. eg: /error/",
            SortOrder = 300)]
        public string ErrorUrl { get; set; }

        [PaymentProviderSetting(Name = "Billing Address (Line 1) Property Alias",
            Description = "The order property alias containing line 1 of the billing address",
            SortOrder = 400)]
        public string BillingAddressLine1PropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Billing Address (Line 2) Property Alias",
            Description = "The order property alias containing line 2 of the billing address",
            SortOrder = 500)]
        public string BillingAddressLine2PropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Billing Address City Property Alias",
            Description = "The order property alias containing the city of the billing address",
            SortOrder = 600)]
        public string BillingAddressCityPropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Billing Address ZipCode Property Alias",
            Description = "The order property alias containing the zip code of the billing address",
            SortOrder = 700)]
        public string BillingAddressZipCodePropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Billing Address State Property Alias",
            Description = "The order property alias containing the state of the billing address",
            SortOrder = 800)]
        public string BillingAddressStatePropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Billing Phone Property Alias",
            Description = "The order property alias containing the phone of the billing address",
            SortOrder = 900)]
        public string BillingPhonePropertyAlias { get; set; }

        [PaymentProviderSetting(Name = "Secret Key",
            Description = "Secret key",
            SortOrder = 400)]
        public string SecretKey { get; set; }

        [PaymentProviderSetting(Name = "Test Mode",
            Description = "Set whether to process payments in test mode.",
            SortOrder = 10000)]
        public bool TestMode { get; set; }
    }
}
