﻿using Promitor.Scraper.Host.Validation.Steps;
using Promitor.Scraper.Tests.Unit.Stubs;
using System.ComponentModel;
using AutoMapper;
using Promitor.Core.Scraping.Configuration.Serialization.v1.Mapping;
using Promitor.Scraper.Tests.Unit.Builders.Metrics.v1;
using Xunit;

namespace Promitor.Scraper.Tests.Unit.Validation.Metrics.ResourceTypes
{
    [Category("Unit")]
    public class PostgreSqlMetricsDeclarationValidationStepTests
    {
        private readonly IMapper _mapper;

        public PostgreSqlMetricsDeclarationValidationStepTests()
        {
            var mapperConfig = new MapperConfiguration(c => c.AddProfile<V1MappingProfile>());
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public void PostgreSqlMetricsDeclaration_DeclarationWithoutAzureMetricName_Fails()
        {
            // Arrange
            var rawDeclaration = MetricsDeclarationBuilder.WithMetadata()
                .WithPostgreSqlMetric(azureMetricName: string.Empty)
                .Build(_mapper);
            var metricsDeclarationProvider = new MetricsDeclarationProviderStub(rawDeclaration, _mapper);

            // Act
            var scrapingScheduleValidationStep = new MetricsDeclarationValidationStep(metricsDeclarationProvider);
            var validationResult = scrapingScheduleValidationStep.Run();

            // Assert
            Assert.False(validationResult.IsSuccessful, "Validation is not successful");
        }

        [Fact]
        public void PostgreSqlMetricsDeclaration_DeclarationWithoutAzureMetricDescription_Succeeds()
        {
            // Arrange
            var rawDeclaration = MetricsDeclarationBuilder.WithMetadata()
                .WithPostgreSqlMetric(metricDescription: string.Empty)
                .Build(_mapper);
            var metricsDeclarationProvider = new MetricsDeclarationProviderStub(rawDeclaration, _mapper);

            // Act
            var scrapingScheduleValidationStep = new MetricsDeclarationValidationStep(metricsDeclarationProvider);
            var validationResult = scrapingScheduleValidationStep.Run();

            // Assert
            Assert.True(validationResult.IsSuccessful, "Validation is successful");
        }

        [Fact]
        public void PostgreSqlMetricsDeclaration_DeclarationWithoutServerName_Fails()
        {
            // Arrange
            var rawDeclaration = MetricsDeclarationBuilder.WithMetadata()
                .WithPostgreSqlMetric(serverName: string.Empty)
                .Build(_mapper);
            var metricsDeclarationProvider = new MetricsDeclarationProviderStub(rawDeclaration, _mapper);

            // Act
            var scrapingScheduleValidationStep = new MetricsDeclarationValidationStep(metricsDeclarationProvider);
            var validationResult = scrapingScheduleValidationStep.Run();

            // Assert
            Assert.False(validationResult.IsSuccessful, "Validation is not successful");
        }
    }
}
