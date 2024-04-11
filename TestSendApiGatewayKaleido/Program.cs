// See https://aka.ms/new-console-template for more information
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Epoche;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using SHA3.Net;

//Esto se debe ajustar en todos los envios a blockchain
Console.WriteLine("PoC Api GateWay BlockChain!*** Envío a BlockChain");
var sendResponse= await SendAsync();

Console.WriteLine("Datos de la transaccion");
Console.WriteLine(JsonSerializer.Serialize(sendResponse));

//el send Response se debe almacenar en la base de datos y marcar cuando la respuesta este completa 
//ya sea por medio del webHook o por medio del Job de Recuperacion
Console.WriteLine("Presion tecla para ver respuesta!**** ");
Console.ReadKey();

//Esto se debe implementar en un Job Timer para recuperar las transacciones pendientes
Console.WriteLine("Respuesta!");
var reply=await ReplyAsync(sendResponse);

if (reply.HasError)
{
    Console.WriteLine($" Error code {reply.errorCode} Error description {reply.errorMessage}");
}
else 
{
    Console.WriteLine($" Transacction hash {reply.transactionHash} Block Number {reply.blockNumber}");
}

Console.ReadKey();

//https://nodogmablog.bryanhogan.net/2020/01/post-with-httpclient-and-basic-authorization/

static async Task<SendResponse> SendAsync()
{
    //esto debe venir del keyVault
    var config = new Config(UrlBase: "https://u0l2l09xrz:XM960UID1IxLUOqkYwXVIzi8HvB56LloKLW8gF3HBIk@u0m1l769ao-u0su1it3ix-connect.us0-aws.kaleido.io", "0x59a1e71b7f4aa9f40b99b61be76558c3349aeb41");

    //esta clase se encargaria seria un Proxy del ApiGateway para en enviar los datos a Kaleido por EthConnect
    var kld = new KaledioApiGateway(config);

    //esto se consulta al consulta del ContractFactory
    var contractAddress = "0x3978b8f38782d061eb7d215dd7796c2ee3f54419";
    //este el metodo que se envia dependiendo de la operacion que van a realizar
    var methodName = "Insert";

    //var id = "121b5ec1-ae30-4a5f-a745-bd391c7840ac";//Guid.NewGuid();
    var id = "11";//id.NewGuid();

    var idHash = GetTransactionHash3(id);

 

    var parameters = new Dictionary<string, object>
    {
      { "movementId", id  },
    };
    parameters.Add("netStandardVolume", "11");
    parameters.Add("transactionId", id);
    parameters.Add("operationalDate", "11");
    parameters.Add("measurementUnit", "11");
    parameters.Add("metadata", "{json}");

    string jsonString = JsonSerializer.Serialize(parameters);
    Console.WriteLine("Parametros enviados");
    Console.WriteLine(jsonString);

    //este es el objeto que se va a enviar al metodo del smart contract

    var sendResponse= await kld.PostAsync(new SendRequest(contractAddress, methodName, parameters));
    sendResponse.TransactionId = id;
    sendResponse.TransactionIdHash = idHash;
    return sendResponse;

}



static string GetTransactionHash3(string rawTransaction) 
{
    var hash = Keccak256.ComputeHash(rawTransaction).ToHex(false);
    return hash;
}

static async Task<ReplyResponse> ReplyAsync(SendResponse sendResponse) 
{
    //esto debe venir del keyVault
    var config = new Config(UrlBase: "https://u0l2l09xrz:XM960UID1IxLUOqkYwXVIzi8HvB56LloKLW8gF3HBIk@u0m1l769ao-u0su1it3ix-connect.us0-aws.kaleido.io", "0x59a1e71b7f4aa9f40b99b61be76558c3349aeb41");
    //esta clase se encargaria seria un Proxy del ApiGateway para en enviar los datos a Kaleido por EthConnect
    var kld = new KaledioApiGateway(config);
    var replyReponse= await kld.GetReplyAsync(sendResponse.Id);
    return replyReponse;
}

public static class ByteArrayExtensions
{
    public static string ToHex(this byte[] bytes, bool upperCase)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
        return result.ToString();
    }
}
public record Config(string UrlBase, string FromAdress) 
{
    public string BaseAddress
    {
        get
        {
            var uri = new Uri(this.UrlBase);
            return $"{uri.Scheme}://{uri.Authority}";
        }
    }
}
public class KaledioApiGateway 
{
    //Representa datos que vienen de la configuracion, usualmente de keyvaul y se tienen el QuorumProfile
    private Config _config;
    public KaledioApiGateway(Config config) 
    {
        _config = config;
    }
    public async Task<SendResponse> PostAsync(SendRequest sendRequest)
    {
        var client = CreateHttpClient();
        //esto se consulta al consultar el ContractFactory
        var contractAddress = sendRequest.FromAdress;// "0x3978b8f38782d061eb7d215dd7796c2ee3f54419";
        //este el metodo que se esta invocando
        var methodName = sendRequest.MethodName; //"Insert";
        var pathContract = GetContractPath(contractAddress, methodName);
        
        var response = await client.PostAsJsonAsync(pathContract, sendRequest.Body).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        var sendResponse = await response.Content.ReadFromJsonAsync<SendResponse>();
        return sendResponse;
    }
    public async Task<ReplyResponse> GetReplyAsync(string id)
    {
        var client = CreateHttpClient();
        var path = $"/replies/{id}";
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var replyResponse = await response.Content.ReadFromJsonAsync<ReplyResponse>();
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        return replyResponse;
    }
    private HttpClient CreateHttpClient()
    {
        HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri(_config.BaseAddress)
        };
        string base64Authorization = GetBase64Authorization();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Authorization);
        return httpClient;
    }
    private string GetContractPath(string contractAdress, string methodName, bool sync = false)
    {
        return $"/instances/{contractAdress}/{methodName}?kld-from={_config.FromAdress}&kld-sync={sync.ToString().ToLower()}";
    }
    private string GetBase64Authorization()
    {
        var byteArray = Encoding.ASCII.GetBytes(new Uri(_config.UrlBase).UserInfo);
        var base64Authorization = Convert.ToBase64String(byteArray);
        return base64Authorization;
    }
}
/// <summary>
/// Example SendRespuesta {"sent":true,"id":"ca7dfed5-fb95-4c7f-5068-1264f7f62baf","msg":"u0m1l769ao-u0su1it3ix-requests:0:16"}
/// </summary>
public record SendRequest(string FromAdress, string MethodName, object Body);
/// <summary>
/// Respuesta de envio asincrono
/// </summary>
public class SendResponse
{
    [JsonPropertyName("sent")]
    public bool Sent { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("msg")]
    public string Message { get; set; }
    /// <summary>
    /// Esto se calcula en c# para poder identificar la respuesta del webHook
    /// </summary>
    [JsonPropertyName("transaccion_id_hash")]
    public string TransactionIdHash { get; set; }
    /// <summary>
    /// Esto se calcula en c# para poder identificar la respuesta del webHook
    /// </summary>
    [JsonPropertyName("transaccion_id")]
    public string TransactionId { get; set; }
}
/// <summary>
/// *************************************Ejemplo de Respuesta exitosa
/// {
///  "_id": "bbb3505a-4c0b-43b3-71b2-5a13efd5aaba",
///  "blockHash": "0x0eeba074556f63cf772b23d00b77691b5b6d01895881a9e3ac10dcbc4fb12a14",
///  "blockNumber": "124277",
///  "cumulativeGasUsed": "325446",
///  "from": "0x59a1e71b7f4aa9f40b99b61be76558c3349aeb41",
///  "gasUsed": "325446",
///  "headers": {
///    "id": "8fb61863-6ef5-4c5f-47f2-73af0d170c1a",
///    "requestId": "bbb3505a-4c0b-43b3-71b2-5a13efd5aaba",
///    "requestOffset": "u0m1l769ao-u0su1it3ix-requests:0:21",
///    "timeElapsed": 8.811988548,
///    "timeReceived": "2024-04-08T21:06:50.087103112Z",
///    "type": "TransactionSuccess"
///  },
///  "nonce": "0",
///  "receivedAt": 1712610418905,
///  "status": "1",
///  "to": "0x3978b8f38782d061eb7d215dd7796c2ee3f54419",
///  "transactionHash": "0xf273e156bba479efa66a94b8c991c5e89136d4e9c475a603040af62afe1e7ad1",
///  "transactionIndex": "0"
///} 
/// *************************************example respuesta de ERROR
///{
///     "_id": "ca7dfed5-fb95-4c7f-5068-1264f7f62baf",
///   "errorCode": "FFEC100148",
///   "errorMessage": "Call failed: execution reverted: Movement identifier is already present",
///   "gapFillSucceeded": false,
///   "headers": {
///         "id": "2bee6fcf-b894-4f2a-4b0f-6bc0bdf18718",
///     "requestId": "ca7dfed5-fb95-4c7f-5068-1264f7f62baf",
///     "requestOffset": "u0m1l769ao-u0su1it3ix-requests:0:16",
///     "timeElapsed": 0.007205911,
///     "timeReceived": "2024-04-05T23:05:39.513041166Z",
///     "type": "Error"
///   },
///   "receivedAt": 1712358339524,
///   "requestPayload": "{\"from\":\"0x59a1e71b7f4aa9f40b99b61be76558c3349aeb41\",\"gas\":0,\"gasPrice\":0,\"headers\":{\"id\":\"ca7dfed5-fb95-4c7f-5068-1264f7f62baf\",\"type\":\"SendTransaction\"},\"method\":{\"inputs\":[{\"internalType\":\"string\",\"name\":\"movementId\",\"type\":\"string\"},{\"internalType\":\"int256\",\"name\":\"netStandardVolume\",\"type\":\"int256\"},{\"internalType\":\"string\",\"name\":\"transactionId\",\"type\":\"string\"},{\"internalType\":\"int64\",\"name\":\"operationalDate\",\"type\":\"int64\"},{\"internalType\":\"string\",\"name\":\"measurementUnit\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"metadata\",\"type\":\"string\"}],\"name\":\"Insert\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},\"params\":[\"mid16\",\"33\",\"tid16\",\"7878\",\"unidad-medidad\",\"meta data en json\"],\"to\":\"0x3978b8f38782d061eb7d215dd7796c2ee3f54419\",\"value\":0}"
/// }
/// </summary>
public class ReplyResponse
{
    public string _id { get; set; } 
    public string blockHash { get; set; } //falta ponerle [JsonPropertyName("sent")] para ajustar los nombres a standar c#
    public string blockNumber { get; set; }
    public string cumulativeGasUsed { get; set; }
    public string from { get; set; }
    public string gasUsed { get; set; }
    public Headers headers { get; set; }
    public string nonce { get; set; }
    public long receivedAt { get; set; }
    public string status { get; set; }
    public string to { get; set; }
    public string transactionHash { get; set; }
    public string transactionIndex { get; set; }
    
    //error
    public string errorCode { get; set; }
    public string errorMessage { get; set; }

    public bool gapFillSucceeded { get; set; }
    public string requestPayload { get; set; }

    public bool HasError => headers.type == "Error";
}
public class Headers
{
    public string id { get; set; }
    public string requestId { get; set; }
    public string requestOffset { get; set; }
    public double timeElapsed { get; set; }
    public string timeReceived { get; set; }
    public string type { get; set; }
}
