using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;

namespace GdevApps.BLL.Models
{
    public class TaskResult<T, U> 
        where T : class
        where U : ICredential
    {

        public TaskResult(){}

        public TaskResult(ResultType result, T obj, U credentials, List<string> errors = null)
        {
            this.Result = result;
            this.ResultObject = obj;
            this.Errors = errors ?? new List<string>();
            this.Credentials = credentials;
        }

        public ResultType Result { get; set; }
        public List<string> Errors { get; set; }
        public T ResultObject { get; set; }
        public U Credentials { get; set; }
    }
}