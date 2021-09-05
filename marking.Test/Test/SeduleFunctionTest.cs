using System;
using marking.Functions.Functions;
using marking.Test.Helpers;
using Xunit;

namespace todofc.Test.Test
{
    public class ScheduleFunctionTest
    {

        [Fact]
        public void ScheduleFunction_Showlg_log_Message()
        {
            // Arrange, preparar prueba unitaria
            MockCloudTableMarking mockmarkings = new MockCloudTableMarking(new Uri("http://127.0.0.1:10002/devstoreacconunt1/reports"));

            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.list);

            // Act, ejecutar como tal
            ScheduleFunction.Run(null, mockmarkings, mockmarkings, logger);
            string message = logger.Logs[0];

            // Assert, verificacion de resultado 
            Assert.Contains("Consolidated", message);
        }
    }
}
