using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UnivEnrollerApi.Data;
using UnivEnrollerApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Test.Helpers;
using Xunit;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace test;

public class UnitTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _applicationFactory;

    private readonly UnivEnrollerContext _db;

    public UnitTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _applicationFactory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // ... Configure test services
                });

        _client = _applicationFactory.CreateClient();

        string connectionstring = "Data Source=Courses.db";

        var optionsBuilder = new DbContextOptionsBuilder<UnivEnrollerContext>();
        optionsBuilder.UseSqlite(connectionstring);

        _db = new UnivEnrollerContext(optionsBuilder.Options);
    }

    [Fact]
    public async void Checkpoint02()
    {
        // Arrange
        var studentId = 3000;
        var expectedCourseIds = new int[] { 21, 10, 22 };
        var enrollmentWithGrade = 44;
        var grade = 4;
        var gradingDate = new DateTime(2022, 05, 05);

        // Act
        var response = await _client.GetFromJsonAsync<IEnumerable<object>>($"/student/{studentId}/courses");

        // Assert
        Assert.NotNull(response);
        var actualCourseIds = response.Select(r => ((JsonElement)r).GetProperty("courseId").GetInt32());
        Assert.Equal(expectedCourseIds, actualCourseIds);
        Random r = new Random();
        JsonElement e = (JsonElement)response.Skip(r.Next(0, response.Count())).Take(1).FirstOrDefault();
        Assert.NotNull(e.GetProperty("id"));
        Assert.NotNull(e.GetProperty("course"));

        e = (JsonElement)response.FirstOrDefault(re => ((JsonElement)re).GetProperty("id").GetInt32() == enrollmentWithGrade);
        Assert.NotNull(e);
        Assert.Equal(grade, e.GetProperty("grade").GetInt32());
        Assert.Equal(gradingDate, e.GetProperty("gradingDate").GetDateTime().Date);
    }
}