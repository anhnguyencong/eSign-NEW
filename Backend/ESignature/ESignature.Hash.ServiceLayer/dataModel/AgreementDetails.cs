using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SdkTester.dataModel
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AgreementDetails
    {
        [JsonProperty("personalName")]
        public string PersonalName { get; set; }

        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("organizationUnit")]
        public string OrganizationUnit { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("telephoneNumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("stateOrProvince")]
        public string StateOrProvince { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("personalID")]
        public string PersonalID { get; set; }

        [JsonProperty("passportID")]
        public string PassportID { get; set; }

        [JsonProperty("citizenID")]
        public string CitizenID { get; set; }

        [JsonProperty("taxID")]
        public string TaxID { get; set; }

        [JsonProperty("budgetID")]
        public string BudgetID { get; set; }

        [JsonProperty("applicationForm")]
        public byte[] ApplicationForm { get; set; }

        [JsonProperty("requestForm")]
        public byte[] RequestForm { get; set; }

        [JsonProperty("authorizeLetter")]
        public byte[] AuthorizeLetter { get; set; }

        [JsonProperty("photoIDCard")]
        public byte[] PhotoIDCard { get; set; }

        [JsonProperty("photoFrontSideIDCard")]
        public byte[] PhotoFrontSideIDCard { get; set; }

        [JsonProperty("photoBackSideIDCard")]
        public byte[] PhotoBackSideIDCard { get; set; }

        [JsonProperty("photoActivityDeclaration")]
        public byte[] PhotoActivityDeclaration { get; set; }

        [JsonProperty("photoAuthorizeDelegate")]
        public byte[] PhotoAuthorizeDelegate { get; set; }
    }   
}
