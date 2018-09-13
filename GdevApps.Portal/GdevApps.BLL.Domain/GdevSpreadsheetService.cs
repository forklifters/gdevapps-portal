using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GdevApps.BLL.Contracts;
using GdevApps.BLL.Models.GDevClassroomService;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GdevApps.BLL.Domain
{
    public class GdevSpreadsheetService : IGdevSpreadsheetService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IAspNetUserService _aspUserService;

        public GdevSpreadsheetService(
            IConfiguration configuration,
            ILogger<GdevClassroomService> logger,
            IAspNetUserService aspUserService
            )
        {
            _logger = logger;
            _configuration = configuration;
            _aspUserService = aspUserService;
        }

        public async Task<IEnumerable<GradebookStudent>> GetStudentsFromGradebook(string externalAccessToken, string gradebookId, string refreshToken, string userId)
        {
            SheetsService service;
            SpreadsheetsResource.ValuesResource.GetRequest request;
            Google.Apis.Sheets.v4.Data.ValueRange response;
            List<GradebookStudent> gradedebookStudents = new List<GradebookStudent>();
            GoogleCredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            IList<IList<object>> values;
            // Create Classroom API service.
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"]
            });

            string range = "A1:K207";
            var studentIndex = 7;
            SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum valueRenderOption = (SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum)1;

            try
            {
                request = service.Spreadsheets.Values.Get(gradebookId, range);
                request.ValueRenderOption = valueRenderOption;
                response = request.Execute();
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving students from GradeBook with id {gradebookId}. Refreshing the token and trying again");
                var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse { RefreshToken = refreshToken };
                 var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = _configuration["installed:client_id"],
                            ClientSecret = _configuration["installed:client_secret"]
                        }
                    }), "user", token);

                service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = _configuration["ApplicationName"]
                });

                request = service.Spreadsheets.Values.Get(gradebookId, range);
                request.ValueRenderOption = valueRenderOption;
                response = request.Execute();
                await UpdateAllTokens(userId, credentials);
            }
            catch (Exception ex)
            {
                return new List<GradebookStudent>();
            }

            values = response.Values;
            for (var r = studentIndex; r < values.Count; r++)
            {
                var studentName = values[r][2].ToString();
                if (string.IsNullOrEmpty(studentName))
                {
                    continue;
                }

                var email = values[r].Count >= 5 ? values[r][5].ToString() : "";
                var comment = values[r].Count >= 9 ? values[r][9].ToString() : "";
                var finalGrade = values[r].Count >= 3 ? values[r][3].ToString() : "";
                var photo = values[r][1].ToString();
                var parentEmail = values[r].Count >= 7 ? values[r][7].ToString() : "";

                gradedebookStudents.Add(new GradebookStudent
                {
                    GradebookId = gradebookId,
                    Email = email,
                    Comment = comment,
                    FinalGrade = finalGrade,
                    Id = values[r][5].ToString(), //email as Id
                    Photo = photo,
                    ParentEmail = parentEmail,
                    Name = studentName
                });
            }

            return gradedebookStudents;
        }

        public async Task<bool> IsGradeBook(string gradebookId, string externalAccessToken, string refreshToken, string userId, string gradeBookLink = "")
        {
            if (string.IsNullOrEmpty(gradebookId))
            {
                var regex = new Regex(@"/[-\w]{25,}/");
                var match = regex.Match(gradeBookLink);
                if (match.Success)
                {
                    gradebookId = match.Value.Replace("/", ""); ;
                }
            }

            SheetsService service;
            SpreadsheetsResource.GetRequest request;
            Google.Apis.Sheets.v4.Data.Spreadsheet response;

            GoogleCredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            // Create Classroom API service.
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"]
            });
            try
            {
                request = service.Spreadsheets.Get(gradebookId);
                response = await request.ExecuteAsync();
                if (response == null)
                {
                    return false;
                }

                return response.Sheets.Any(s => s.Properties.Title == "GradeBook") &&
                response.Sheets.Any(s => s.Properties.Title == "Settings") &&
                response.Sheets.Any(s => s.Properties.Title == "Statistics") &&
                response.Sheets.Any(s => s.Properties.Title == "Email Message");
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving courses from Google Classroom. Refreshing the token and trying again");
                var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse { RefreshToken = refreshToken };
                var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = _configuration["installed:client_id"],
                            ClientSecret = _configuration["installed:client_secret"]
                        }
                    }), "user", token);
                service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = _configuration["ApplicationName"]
                });
                // Define request parameters.
                request = service.Spreadsheets.Get(gradebookId);
                response = await request.ExecuteAsync();

                await UpdateAllTokens(userId, credentials);
                if (response == null)
                {
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return false;
            }

            throw new NotImplementedException();
        }

        private async Task UpdateAllTokens(string userId, UserCredential credentials)
        {
            var userLoginTokens = await _aspUserService.GetAllTokensByUserIdAsync(userId);
            if (userLoginTokens != null)
            {
                var accessTokenRecord = userLoginTokens.Where(t => t.Name == "access_token").First();
                accessTokenRecord.Value = credentials.Token.AccessToken;
                await _aspUserService.UpdateUserTokensAsync(accessTokenRecord);

                var expiresAtTokenRecord = userLoginTokens.Where(t => t.Name == "expires_at").FirstOrDefault();

                var issuedDate = credentials.Token.IssuedUtc;
                if (credentials.Token.ExpiresInSeconds.HasValue)
                {
                    expiresAtTokenRecord.Value = issuedDate.AddSeconds(credentials.Token.ExpiresInSeconds.Value).ToString("o",
                     System.Globalization.CultureInfo.InvariantCulture);
                    await _aspUserService.UpdateUserTokensAsync(expiresAtTokenRecord);
                }

                var tokenUpdatedRecord = userLoginTokens.Where(t => t.Name == "token_updated").First();
                tokenUpdatedRecord.Value = "true";
                await _aspUserService.UpdateUserTokensAsync(tokenUpdatedRecord);

                var tokenUpdatedTimeRecord = userLoginTokens.Where(t => t.Name == "token_updated_time").First();
                tokenUpdatedTimeRecord.Value = DateTime.UtcNow.ToString();
                await _aspUserService.UpdateUserTokensAsync(tokenUpdatedRecord);
            }
        }
    }
}