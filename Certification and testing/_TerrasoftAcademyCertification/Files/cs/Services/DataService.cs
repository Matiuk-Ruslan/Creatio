using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using Terrasoft.Core;
using Terrasoft.Web.Common;
using Terrasoft.Web.Http.Abstractions;
using Newtonsoft.Json;
using Terrasoft.Core.DB;
using ExternalSystemsIntegration.Files.cs;

namespace _TerrasoftAcademyCertification.Files.cs.Services
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class DataService : BaseService
    {
		private DataServiceResponse response;

        private readonly UserConnection userConnection = HttpContext.Current.Session["UserConnection"] as UserConnection;

        /// <summary> Endpoint: {{siteAddress}}/0/rest/DataService/GetTotalPrice </summary>
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        public DataServiceResponse GetTotalPrice(DataServiceRequest request)
        {
            response = new DataServiceResponse { Success = "-1" };

            if (string.IsNullOrEmpty(request.RealtyType) || string.IsNullOrEmpty(request.OfferTypeRealty))
            {
                response.Success = "-1";
                response.Error = "Отсутствуют значения требуемых параметров";
                response.TotalPrice = null;
                return response;
            }

            try
            {
                Select select = new Select(userConnection)
                .Column(Func.Sum("UsrPrice"))
                .From("UsrRealty")
                .Where("UsrRealtyTypeId").IsEqual(Column.Parameter(new Guid(request.RealtyType)))
                .And("UsrOfferTypeRealtyId").IsEqual(Column.Parameter(new Guid(request.OfferTypeRealty))) as Select;
                decimal totalPrice = select.ExecuteScalar<decimal>();
                
                response.TotalPrice = totalPrice;
                response.Success = "null";
                response.Error = null;

                Logger.WriteToLog("DataServiceResponse.GetTotalPrice", $"RealtyType: {request.RealtyType}, OfferTypeRealty: {request.OfferTypeRealty}, TotalPrice:{totalPrice}", userConnection);

                return response;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("DataServiceResponse.GetTotalPrice", $"RealtyType: {request.RealtyType}, OfferTypeRealty: {request.OfferTypeRealty}, Exception: {ex.Message}", userConnection);
                response.Success = "-1";
                response.Error = ex.Message;
                response.TotalPrice = null;
                return response;
            }
        }
    }

	[DataContract]
    public class DataServiceRequest
    {
        [DataMember]
        [JsonProperty("RealtyType")]
        public string RealtyType { get; set; }

        [DataMember]
        [JsonProperty("OfferTypeRealty")]
        public string OfferTypeRealty { get; set; }
    }

	[DataContract]
    public class DataServiceResponse
    {
        [DataMember]
        [JsonProperty("Success")]
        public string Success { get; set; }

        [DataMember]
        [JsonProperty("Error")]
        public string Error { get; set; }

        [DataMember]
        [JsonProperty("TotalPrice")]
        public decimal? TotalPrice { get; set; }
    }
}
