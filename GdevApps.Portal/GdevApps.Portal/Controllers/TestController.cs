using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GdevApps.BLL.Contracts;
using GdevApps.Portal.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GdevApps.Portal.Controllers
{
    public class TestController : Controller
    {

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAspNetUserService _aspNetUserService;


        public TestController(SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAspNetUserService aspNetUserService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _aspNetUserService = aspNetUserService;
        }

        public async Task<IActionResult> GetTokens()
        {
            var result = await _aspNetUserService.GetAllTokens();
            return Ok(result);
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Define request parameters.
                var accessTokenGoogle = await HttpContext.GetTokenAsync("Google", "access_token");
                var refreshToken = await HttpContext.GetTokenAsync("Google", "refresh_token");
                var accessToken = await HttpContext.GetTokenAsync("access_token");

                var user = await _userManager.GetUserAsync(User);
                string externalAccessToken = null;
                ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
                if (User.Identity.IsAuthenticated)
                {
                    var userFromManager = await _userManager.GetUserAsync(User);
                    string authenticationMethod = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationMethod)?.Value;
                    if (authenticationMethod != null)
                    {
                        externalAccessToken = await _userManager.GetAuthenticationTokenAsync(userFromManager, authenticationMethod, "access_token");
                    }
                    else
                    {
                        externalAccessToken = await _userManager.GetAuthenticationTokenAsync(userFromManager, "Google", "access_token");
                    }
                }

                GoogleCredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = googleCredential,
                    ApplicationName = "Gdevapps Portal"
                });

                // String spreadsheetId = "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms";//1IYcktfn-VBqzPDh_meN1TfpL5vCJmTx6YybrlPqhOQc
                //String range = "Class Data!A2:E";

                //String spreadsheetId = "1IYcktfn-VBqzPDh_meN1TfpL5vCJmTx6YybrlPqhOQc";//
                //String range = "GradeBook!A2:E";

                String spreadsheetId = "1JF459hxZuD_uKdBI7QXzfOhV8w8b_oaVpV3Ap6HYBC0";//
                String range = "Data!A1:C16";
                //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                ValueRange response = await service.Spreadsheets.Values.Get(spreadsheetId, range).ExecuteAsync();
                IList<IList<Object>> values = response.Values;
                for (var a = 0; a < values.Count; a++)
                {
                    for (var b = 0; b < values[a].Count; b++)
                    {
                        //var val = values[a][b].ToString();
                        //values[a][b] = string.IsNullOrEmpty(val) ? val : val.Replace("_1", "*");

                        var val = "=Column()+1";
                        values[a][b] = val;
                    }
                }

                var valueRange = new ValueRange()
                {
                    Range = range,
                    MajorDimension = "ROWS",
                    Values = values
                };

                var newSpreadSheet = await service.Spreadsheets.Create(new Spreadsheet()
                {
                    Properties = new SpreadsheetProperties()
                    {
                        Title = "Pash_test"
                    }
                }).ExecuteAsync();


                CopySheetToAnotherSpreadsheetRequest copyToRequest = new CopySheetToAnotherSpreadsheetRequest();
                copyToRequest.DestinationSpreadsheetId = newSpreadSheet.SpreadsheetId;
                var result = await service.Spreadsheets.Sheets.CopyTo(copyToRequest, spreadsheetId, 0).ExecuteAsync();

                DeleteSheetRequest deleteSheetRequest = new DeleteSheetRequest()
                {
                    SheetId = 0
                };

                BatchUpdateSpreadsheetRequest updateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest()
                {
                    Requests = new List<Request>
                    {
                        new Request()
                        {
                            DeleteSheet = deleteSheetRequest
                        }
                    },
                    ResponseIncludeGridData = false,
                    IncludeSpreadsheetInResponse = false
                };


                await service.Spreadsheets.BatchUpdate(updateSpreadsheetRequest, newSpreadSheet.SpreadsheetId).ExecuteAsync();

                if (result != null)
                    return Ok(result);

                //  var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
                //Formula
                //  updateRequest.ResponseValueRenderOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ResponseValueRenderOptionEnum.FORMULA;

                //  updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

                //service.Spreadsheets.Values.Update().

                //  await updateRequest.ExecuteAsync();

                if (values != null && values.Count > 0)
                {

                    return Ok(values);
                }
                else
                {

                    return BadRequest("No data found.");
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                throw;
            }

        }
    }
}