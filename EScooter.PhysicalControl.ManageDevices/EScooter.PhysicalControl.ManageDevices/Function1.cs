using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EScooter.PhysicalControl.ManageDevices
{
    public static class Function1
    {
        [Function("Function1")]
        public static void Run([ServiceBusTrigger("", "", Connection = "")] string mySbMsg, FunctionContext context)
        {
            var logger = context.GetLogger("Function1");
            logger.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg}");
        }
    }
}
