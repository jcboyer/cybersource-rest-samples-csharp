using System;
using System.Collections.Generic;
using System.Globalization;

using CyberSource.Api;
using CyberSource.Model;

namespace Cybersource_rest_samples_dotnet.Samples.Webhooks.CreateNewWebhooks
{
	public class StoreOAuthCredentials
	{
		public static void Run(string vCcorrelationId, string vCsenderOrganizationId, string vCpermissions)
		{
			string clientRequestAction = "STORE";
			string keyInformationProvider = "merchantName";
			string keyInformationTenant = "nrtd";
			string keyInformationKeyType = "oAuthClientCredentials";
			string keyInformationOrganizationId = "merchantName";
			string keyInformationClientKeyId = "client username";
			string keyInformationKey = "client secret";
			string keyInformationExpiryDuration = "365";
			Kmsegressv2keyssymKeyInformation keyInformation = new Kmsegressv2keyssymKeyInformation(
				Provider: keyInformationProvider,
				Tenant: keyInformationTenant,
				KeyType: keyInformationKeyType,
				OrganizationId: keyInformationOrganizationId,
				ClientKeyId: keyInformationClientKeyId,
				Key: keyInformationKey,
				ExpiryDuration: keyInformationExpiryDuration
			);

			var requestObj = new SaveSymEgressKey(
				ClientRequestAction: clientRequestAction,
				KeyInformation: keyInformation
			);

			try
			{
				var configDictionary = new Configuration().GetConfiguration();
				var clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);

				var apiInstance = new CreateNewWebhooksApi(clientConfig);
				apiInstance.SaveSymEgressKey(vC-senderOrganizationId, vC-permissions, requestObj, vC-correlationId);
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception on calling the API : " + e.Message);
			}
		}
	}
}