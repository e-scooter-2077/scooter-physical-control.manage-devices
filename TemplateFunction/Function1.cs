namespace TemplateFunction
{
    using System.Net;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// My function.
    /// </summary>
    public static class Function1
    {
        /// <summary>
        /// handles http requests.
        /// </summary>
        /// <param name="req">http request.</param>
        /// <param name="executionContext">function execution context.</param>
        /// <returns>http response.</returns>
        [Function("Function1")]
        public static HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Function1");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
