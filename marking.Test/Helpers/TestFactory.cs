using marking.Common.Models;
using marking.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.IO;

namespace marking.Test.Helpers
{
    public class TestFactory
    {
        public static MarkingEntity GetMarkingEntity()
        {
            return new MarkingEntity
            {
                ETag = "*",
                PartitionKey = "MARKING",
                RowKey = Guid.NewGuid().ToString(),
                IdEmpleo = 1,
                DateTimeInOrOut = DateTime.UtcNow,
                Tipo = 1,
                consolidated = false,
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Marking MarkingRequest)
        {
            string request = JsonConvert.SerializeObject(MarkingRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{MarkingRequest}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid markingId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{markingId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(MarkingEntity todoRequest)
        {
            string request = JsonConvert.SerializeObject(todoRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
            };
        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static Marking GetMarkingRquest()
        {
            return new Marking
            {
                IdEmpleo = 1,
                DateTimeInOrOut = DateTime.UtcNow,
                Tipo = 1,
                consolidated = false,
            };
        }

        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.list)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null logger");
            }
            return logger;
        }
    }
}
