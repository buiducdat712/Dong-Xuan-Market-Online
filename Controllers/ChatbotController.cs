using Google.Apis.Auth.OAuth2;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Grpc.Auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly SessionsClient _sessionsClient;
        private readonly string _projectId = "your-project-id";
        private readonly string _sessionId = Guid.NewGuid().ToString();
        public ChatbotController()
        {
            // Đọc tệp JSON từ Google Cloud Console để lấy thông tin xác thực
            var credential = GoogleCredential.FromFile("path-to-your-json-file.json").ToChannelCredentials();
            // Khởi tạo SessionsClient với thông tin xác thực
            _sessionsClient = new SessionsClientBuilder
            {
                ChannelCredentials = credential
            }.Build();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            var session = SessionName.FromProjectSession(_projectId, _sessionId);
            var queryInput = new QueryInput
            {
                Text = new TextInput
                {
                    Text = message,
                    LanguageCode = "en"
                }
            };

            var request = new DetectIntentRequest
            {
                SessionAsSessionName = session,
                QueryInput = queryInput
            };

            var response = await _sessionsClient.DetectIntentAsync(request);
            var responseMessage = response.QueryResult.FulfillmentText;

            return Json(new { Response = responseMessage });
        }
    }
}
