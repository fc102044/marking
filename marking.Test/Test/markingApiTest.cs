using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using marking.Common.Models;
using marking.Functions.Functions;
using marking.Test.Helpers;
using Xunit;
using marking.Functions.Entities;

namespace marking.Test.Test
{
    public class markingApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateMarker_Should_Return_200()
        {
            // Arrange, preparar prueba unitaria
            MockCloudTableMarking mockmarkings = new MockCloudTableMarking(new Uri("http://127.0.0.1:10002/devstoreacconunt1/reports"));
            Marking markingRequest = TestFactory.GetMarkingRquest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(markingRequest);

            // Act, ejecutar como tal
            IActionResult response = await markingApi.CreateMarking(request, mockmarkings, logger);

            // Assert, verificacion de resultado 
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        [Fact]
        public async void UpdateMarker_Should_Return_200()
        {
            // Arrange, preparar prueba unitaria
            MockCloudTableMarking mockMarker = new MockCloudTableMarking(new Uri("http://127.0.0.1:10002/devstoreacconunt1/reports"));
            Marking MarkingRequest = TestFactory.GetMarkingRquest();
            Guid markingId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(MarkingRequest);

            // Act, ejecutar como tal
            IActionResult response = await markingApi.UpdateMarking(request, mockMarker, markingId.ToString(), logger);

            // Assert, verificacion de resultado 
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        [Fact]
        public async void DeleteMarker_Should_Return_200()
        {
            // Arrange, preparar prueba unitaria
            MockCloudTableMarking mockMarker = new MockCloudTableMarking(new Uri("http://127.0.0.1:10002/devstoreacconunt1/reports"));
            Marking markingRequest = TestFactory.GetMarkingRquest();
            Guid MarkerId = Guid.NewGuid();
            MarkingEntity markinEntity = new MarkingEntity()
            {
                IdEmpleo = 1,
                ETag = "*",
            };
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(markingRequest);

            // Act, ejecutar como tal
            IActionResult response = await markingApi.DeleteMarking(request, markinEntity, mockMarker, MarkerId.ToString(), logger);

            // Assert, verificacion de resultado 
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        [Fact]
        public void GetMarkerByID_Should_Return_200()
        {
            // Arrange, preparar prueba unitaria
            MockCloudTableMarking mockMarker = new MockCloudTableMarking(new Uri("http://127.0.0.1:10002/devstoreacconunt1/reports"));
            Marking markingRequest = TestFactory.GetMarkingRquest();
            Guid MarkerId = Guid.NewGuid();
            MarkingEntity markinEntity = new MarkingEntity()
            {
                IdEmpleo = 1,
                ETag = "*",
            };
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(markingRequest);

            // Act, ejecutar como tal
            IActionResult response = markingApi.GetMarkingbyId(request, markinEntity, MarkerId.ToString(), logger);

            // Assert, verificacion de resultado 
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        //[Fact]
        //public void GetAllMarker_Should_Return_200()
        //{
        //    // Arrange, preparar prueba unitaria
        //    MockCloudTableMarking mockMarker = new MockCloudTableMarking(new Uri("http://127.0.0.1:10002/devstoreacconunt1/reports"));
        //    Marking markingRequest = TestFactory.GetMarkingRquest();

        //    DefaultHttpRequest request = TestFactory.CreateHttpRequest(markingRequest);

        //    // Act, ejecutar como tal
        //    IActionResult response = markingApi.GetAllMarking(request, mockMarker, logger);

        //    // Assert, verificacion de resultado 
        //    OkObjectResult result = (OkObjectResult)response;
        //    Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        //}

    }
}
