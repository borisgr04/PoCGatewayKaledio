using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Resolvers;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyFunctionApp
{
    public class PostFunction
    {
        private readonly ILogger<PostFunction> _logger;

        public PostFunction(ILogger<PostFunction> log)
        {
            _logger = log;
        }

        [FunctionName("PostFunction")]
        [OpenApiOperation(operationId: "PostFunction", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(List<ResponseTrueMovementLog>))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequest req)
        {
            //var tupo = req.Headers["ServiceType"];

            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            //var data = JsonConvert.DeserializeObject<List<ResponseTrueMovementLog>>(requestBody);
            //var end=data.FirstOrDefault().signature.IndexOf("(");
            //var evento = data.FirstOrDefault().signature.Substring(0, end);


            //var dataTrue = JsonConvert.DeserializeObject<List<ResponseTrueLog>>(requestBody);
            //var endTrue = dataTrue.FirstOrDefault().signature.IndexOf("(");
            //var eventoTrue = dataTrue.FirstOrDefault().signature.Substring(0, end);

            //Aca el sistema debe tomar el transaccion id =>  "Id": "0x290d38a37c9275149c866a7e781aa7e10532ae5cca060c7987e20e48cc37b168",
            //Convertirlo en un numero normal 
            //bytes32 id = keccak256(bytes(transactionId)); reversion en c#
            //Lamar a la cola de envio de hash a la base  datos enviando el Hash y Blocknumber
            //ir a la tabla de Reply y actualizar el estado a finalizado, adicionando el hash y blocknumber

            string responseMessage = string.IsNullOrEmpty(requestBody)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {requestBody}. This HTTP triggered function executed successfully.";

            _logger.LogInformation(responseMessage);
            return new OkObjectResult(responseMessage);
        }


        [OpenApiExample(typeof(ParametersExample))]
        public class Parameters
        {
            /// <summary>The id of the customer in the context. This is also called payer, sub_account_id.</summary>
            [OpenApiProperty(Description ="The id of the customer in the context. This is also called payer, sub_account_id.")]
            [Newtonsoft.Json.JsonProperty("customerId", Required = Newtonsoft.Json.Required.Always)]
            [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
            public string CustomerId { get; set; }

            /// <summary>The order number. Used to uniquely identify a group of order lines.</summary>
            [OpenApiProperty(Description = "The order number. Used to uniquely identify a group of order lines.")]
            [Newtonsoft.Json.JsonProperty("orderNumber", Required = Newtonsoft.Json.Required.Always)]
            [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
            public string OrderNumber { get; set; }
        }

        public class ParametersExample : OpenApiExample<Parameters>
        {
            public override IOpenApiExample<Parameters> Build(NamingStrategy namingStrategy = null)
            {
                this.Examples.Add(
                    OpenApiExampleResolver.Resolve(
                        "ParametersExample",
                        new Parameters()
                        {
                            CustomerId = "CUST12345",
                            OrderNumber = "ORD001"
                        },
                        namingStrategy
                    ));

                return this;
            }
        }


        /// <summary>
        /// [
        ///      {
        ///        "address": "0x3978b8F38782d061EB7d215dD7796C2ee3f54419",
        ///        "blockNumber": "98950",
        ///        "blockHash": "0x28ebf52aa8e1d3e263f8e15bbbb111cca55b104a2c9922fa8ba621bf25709a4d",
        ///        "transactionIndex": "0x0",
        ///        "transactionHash": "0x9935863e6f431f29a967135146787f6e7cb780f364640958c2720aae012aa0c7",
        ///        "data": {
        ///          "ActionType": "0",
        ///          "Id": "0x290d38a37c9275149c866a7e781aa7e10532ae5cca060c7987e20e48cc37b168",
        ///          "Metadata": "campo1,campo2, campo2",
        ///          "MovementId": "0x9d84d8f264496b4481f692ca5e9e41bb58db2b39aa228d86d643615a4b9dfac9",
        ///          "OperationalDate": "7878",
        ///          "Timestamp": "1712357145",
        ///          "Unit": "unidad-medidad poc 16",
        ///          "Version": "2",
        ///          "Volume": "33"
        ///        },
        ///        "subId": "sb-d5d01a47-15d2-4a0d-5fbb-99758a3f8e0d",
        ///        "signature": "TrueMovementLog(bytes32,bytes32,int64,uint8,int256,uint8,string,string,uint256)",
        ///        "logIndex": "0"
        ///      }
        ///    ]
        /// </summary>
     
        public class ResponseTrueMovementLog
        {
  
            public string address { get; set; }
            public string blockNumber { get; set; }
            public string blockHash { get; set; }
            public string transactionIndex { get; set; }
            public string transactionHash { get; set; }
            public TrueMovementLog data { get; set; }
            public string subId { get; set; }
            public string signature { get; set; }
            public string logIndex { get; set; }
        }

        public class TrueMovementLog
        {
            public string ActionType { get; set; }
            public string Id { get; set; }
            public string Metadata { get; set; }
            public string MovementId { get; set; }
            public string OperationalDate { get; set; }
            public string Timestamp { get; set; }
            public string Unit { get; set; }
            public string Version { get; set; }
            public string Volume { get; set; }
        }

        //Log Event Generico
        /// <summary>
        /// [
        ///      {
        ///        "address": "0x3978b8F38782d061EB7d215dD7796C2ee3f54419",
        ///        "blockNumber": "98950",
        ///        "blockHash": "0x28ebf52aa8e1d3e263f8e15bbbb111cca55b104a2c9922fa8ba621bf25709a4d",
        ///        "transactionIndex": "0x0",
        ///        "transactionHash": "0x9935863e6f431f29a967135146787f6e7cb780f364640958c2720aae012aa0c7",
        ///        "data": {
        ///          "ActionType": "0",
        ///          "Id": "0x290d38a37c9275149c866a7e781aa7e10532ae5cca060c7987e20e48cc37b168",
        ///          "Metadata": "meta data en json poc",
        ///          "MovementId": "0x9d84d8f264496b4481f692ca5e9e41bb58db2b39aa228d86d643615a4b9dfac9",
        ///          "OperationalDate": "7878",
        ///          "Timestamp": "1712357145",
        ///          "Unit": "unidad-medidad poc 16",
        ///          "Version": "2",
        ///          "Volume": "33"
        ///        },
        ///        "subId": "sb-d5d01a47-15d2-4a0d-5fbb-99758a3f8e0d",
        ///        "signature": "TrueMovementLog(bytes32,bytes32,int64,uint8,int256,uint8,string,string,uint256)",
        ///        "logIndex": "0"
        ///      }
        ///    ]
        /// </summary>

        public class ResponseTrueLog
        {

            public string address { get; set; }
            public string blockNumber { get; set; }
            public string blockHash { get; set; }
            public string transactionIndex { get; set; }
            public string transactionHash { get; set; }
            public TrueLog data { get; set; }
            public string signature { get; set; }
            
        }

        public class TrueLog
        {
            public string ActionType { get; set; }
            public string Id { get; set; }
        }



    }
}

