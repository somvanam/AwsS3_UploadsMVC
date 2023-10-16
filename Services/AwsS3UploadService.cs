using AwsS3_UploadsMVC.Interfaces;
using AwsS3_UploadsMVC.Models;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace AwsS3_UploadsMVC.Services
{
    public class AwsS3UploadService : IAwsS3Upload
    {  
        
        // Static variables to hold configuration values
        public static string _tokenUrl { get; private set; }
        public static string _clientId { get; private set; }
        public static string _clientSecretBase64 { get; private set; }
        public static string _CookieAddress { get; private set; }
        public static string _BaseAddress { get; private set; }
        public AwsS3UploadService(IConfiguration configuration)
        {
            _tokenUrl = configuration["AwsS3UploadSettings:tokenUrl"];
            _clientId = configuration["AwsS3UploadSettings:clientId"];
            _clientSecretBase64 = configuration["AwsS3UploadSettings:clientSecretBase64"];
            _CookieAddress = configuration["AwsS3UploadSettings:Cookie"];
            _BaseAddress = configuration["AwsS3UploadSettings:BaseAddress"];
            
        }
        public async Task<string> AwsS3Upload(IFormFile file)
        {
            var partSize = 5 * 1024 * 1024; // 5 MB parts
            int numberOfparts = (int)Math.Ceiling(Convert.ToDecimal(file.Length) / partSize);
            EtagModel etagModel = new EtagModel();
            List<byte[]> fileParts = await SplitFileIntoParts(file);
            JWToken token = await GetAccessToken();
            initializeModel initializationData = await InitializeUpload(token, file);
            getPreSignedUrlsModel preSignedUrls = await GetPreSignedUrls(token, initializationData, numberOfparts);

            await UploadFileParts(fileParts, preSignedUrls.parts);
            await CompleteMultipartUpload(token, initializationData, etagModel);

            return string.Empty;
        }

        private async Task<List<byte[]>> SplitFileIntoParts(IFormFile file)
        {
            List<byte[]> parts = new List<byte[]>();
            var partSize = 5 * 1024 * 1024; // 5 MB parts
            var buffer = new byte[partSize];
            var bytesRead = 0;
            using (var fileStream = file.OpenReadStream())
            {
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    // Add each part to the list
                    parts.Add(buffer.Take(bytesRead).ToArray());
                }
            }
            return parts;
        }

        private async Task<JWToken> GetAccessToken()
        {
            string tokenUrl = _tokenUrl;
            string clientId = _clientId;
            string clientSecretBase64 = _clientSecretBase64;

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId)
        });
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + clientSecretBase64);
                client.DefaultRequestHeaders.Add("Cookie", _CookieAddress);

                HttpResponseMessage response = await client.PostAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<JWToken>(responseContent);
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }
            }
        }

        private async Task<initializeModel> InitializeUpload(JWToken token, IFormFile file)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.access_token);
                client.BaseAddress = new Uri(_BaseAddress);
                var requestData = new
                {
                    fileName = file.FileName,
                    fileType = file.ContentType
                };
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("initialize", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<initializeModel>(responseContent);
                }
                else
                {
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }
            }
        }

        private async Task<getPreSignedUrlsModel> GetPreSignedUrls(JWToken token, initializeModel initializationData,int numberofparts)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.access_token);
                client.BaseAddress = new Uri(_BaseAddress);
                var requestData = new
                {
                    fileId = initializationData.fileId,
                    fileKey = initializationData.fileKey,
                    parts = numberofparts
                };
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("getPreSignedUrls", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<getPreSignedUrlsModel>(responseContent);
                }
                else
                {
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }
            }
        }

        private async Task UploadFileParts(List<byte[]> fileParts, List<Part> preSignedUrls)
        {
            int partNumber = 1;
            using (var client = new HttpClient())
            {
                foreach (var partData in fileParts)
                {
                    var preSignedUrl = preSignedUrls[partNumber - 1].signedUrl;
                    var content = new ByteArrayContent(partData);

                    var response = await client.PutAsync(preSignedUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Handle the response and ETag as needed
                    }
                    partNumber++;
                }
            }
        }

        private async Task CompleteMultipartUpload(JWToken token, initializeModel initializationData, EtagModel etagModel)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.access_token);
                client.BaseAddress = new Uri(_BaseAddress);
                var requestData = new
                {
                    fileId = initializationData.fileId,
                    fileKey = initializationData.fileKey,
                    parts = etagModel.parts
                };
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("finalize", content);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the response as needed
                }
                else
                {
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
                }
            }
        }



    }
}
