﻿using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Promitor.Core.Scraping.Configuration.Model;
using Promitor.Core.Scraping.Configuration.Serialization;
using Promitor.Core.Scraping.Configuration.Serialization.v1.Core;
using Promitor.Core.Scraping.Configuration.Serialization.v1.Model;
using Xunit;
using YamlDotNet.RepresentationModel;

namespace Promitor.Scraper.Tests.Unit.Serialization.v1.Core
{
    [Category("Unit")]
    public class MetricDefinitionDeserializerTests
    {
        private readonly Mock<IDeserializer<AzureMetricConfigurationV1>> _azureMetricConfigurationDeserializer;
        private readonly Mock<IDeserializer<ScrapingV1>> _scrapingDeserializer;
        private readonly Mock<IAzureResourceDeserializerFactory> _resourceDeserializerFactory;

        private readonly MetricDefinitionDeserializer _deserializer;

        public MetricDefinitionDeserializerTests()
        {
            _azureMetricConfigurationDeserializer = new Mock<IDeserializer<AzureMetricConfigurationV1>>();
            _scrapingDeserializer = new Mock<IDeserializer<ScrapingV1>>();
            _resourceDeserializerFactory = new Mock<IAzureResourceDeserializerFactory>();

            _deserializer = new MetricDefinitionDeserializer(
                _azureMetricConfigurationDeserializer.Object,
                _scrapingDeserializer.Object,
                _resourceDeserializerFactory.Object,
                NullLogger.Instance);
        }

        [Fact]
        public void Deserialize_NameSupplied_SetsName()
        {
            YamlAssert.PropertySet(
                _deserializer,
                "name: promitor_test_metric",
                "promitor_test_metric",
                d => d.Name);
        }

        [Fact]
        public void Deserialize_NameNotSupplied_Null()
        {
            YamlAssert.PropertyNull(_deserializer, "description: 'Test metric'", d => d.Name);
        }

        [Fact]
        public void Deserialize_DescriptionSupplied_SetsDescription()
        {
            YamlAssert.PropertySet(
                _deserializer,
                "description: 'This is a test metric'",
                "This is a test metric",
                d => d.Description);
        }

        [Fact]
        public void Deserialize_DescriptionNotSupplied_Null()
        {
            YamlAssert.PropertyNull(_deserializer, "name: metric", d => d.Description);
        }

        [Fact]
        public void Deserialize_ResourceTypeSupplied_SetsResourceType()
        {
            YamlAssert.PropertySet(
                _deserializer,
                "resourceType: ServiceBusQueue",
                ResourceType.ServiceBusQueue,
                d => d.ResourceType);
        }

        [Fact]
        public void Deserialize_ResourceTypeNotSupplied_Null()
        {
            YamlAssert.PropertyNull(
                _deserializer,
                "name: promitor_test_metric",
                d => d.ResourceType);
        }

        [Fact]
        public void Deserialize_LabelsSupplied_SetsLabels()
        {
            const string yamlText =
@"labels:
    app: promitor
    env: test";

            YamlAssert.PropertySet(
                _deserializer,
                yamlText,
                new Dictionary<string, string>{{"app", "promitor"}, {"env", "test"}},
                d => d.Labels);
        }

        [Fact]
        public void Deserialize_LabelsNotSupplied_Null()
        {
            YamlAssert.PropertyNull(_deserializer, "name: promitor_test_metric", d => d.Labels);
        }

        [Fact]
        public void Deserialize_AzureMetricConfigurationSupplied_UsesDeserializer()
        {
            // Arrange
            const string yamlText =
@"azureMetricConfiguration:
    metricName: ActiveMessages";
            var node = YamlUtils.CreateYamlNode(yamlText);
            var configurationNode = (YamlMappingNode) node.Children["azureMetricConfiguration"];
            var configuration = new AzureMetricConfigurationV1();

            _azureMetricConfigurationDeserializer.Setup(d => d.Deserialize(configurationNode)).Returns(configuration);

            // Act
            var definition = _deserializer.Deserialize(node);

            // Assert
            Assert.Same(configuration, definition.AzureMetricConfiguration);
        }

        [Fact]
        public void Deserialize_AzureMetricConfigurationNotSupplied_Null()
        {
            // Arrange
            const string yamlText = @"name: promitor_test_metric";
            var node = YamlUtils.CreateYamlNode(yamlText);

            _azureMetricConfigurationDeserializer.Setup(
                d => d.Deserialize(It.IsAny<YamlMappingNode>())).Returns(new AzureMetricConfigurationV1());

            // Act
            var definition = _deserializer.Deserialize(node);

            // Assert
            Assert.Null(definition.AzureMetricConfiguration);
        }

        [Fact]
        public void Deserialize_ScrapingSupplied_UsesDeserializer()
        {
            // Arrange
            const string yamlText =
@"scraping:
    interval: '00:05:00'";
            var node = YamlUtils.CreateYamlNode(yamlText);
            var scrapingNode = (YamlMappingNode)node.Children["scraping"];
            var scraping = new ScrapingV1();

            _scrapingDeserializer.Setup(d => d.Deserialize(scrapingNode)).Returns(scraping);

            // Act
            var definition = _deserializer.Deserialize(node);

            // Assert
            Assert.Same(scraping, definition.Scraping);
        }

        [Fact]
        public void Deserialize_ScrapingNotSupplied_Null()
        {
            // Arrange
            const string yamlText = "name: promitor_test_metric";
            var node = YamlUtils.CreateYamlNode(yamlText);

            _scrapingDeserializer.Setup(d => d.Deserialize(It.IsAny<YamlMappingNode>())).Returns(new ScrapingV1());

            // Act
            var definition = _deserializer.Deserialize(node);

            // Assert
            Assert.Null(definition.Scraping);
        }

        [Fact]
        public void Deserialize_ResourcesSupplied_UsesDeserializer()
        {
            // Arrange
            const string yamlText =
@"resourceType: Generic
resources:
- resourceUri: Microsoft.ServiceBus/namespaces/promitor-messaging
- resourceUri: Microsoft.ServiceBus/namespaces/promitor-messaging-2";
            var node = YamlUtils.CreateYamlNode(yamlText);

            var resourceDeserializer = new Mock<IDeserializer<AzureResourceDefinitionV1>>();
            _resourceDeserializerFactory.Setup(
                f => f.GetDeserializerFor(ResourceType.Generic)).Returns(resourceDeserializer.Object);

            var resources = new List<AzureResourceDefinitionV1>();
            resourceDeserializer.Setup(
                d => d.Deserialize((YamlSequenceNode) node.Children["resources"])).Returns(resources);

            // Act
            var definition = _deserializer.Deserialize(node);

            // Assert
            Assert.Same(resources, definition.Resources);
        }

        [Fact]
        public void Deserialize_ResourcesWithUnspecifiedResourceType_Null()
        {
            // Arrange
            const string yamlText =
@"resources:
- resourceUri: Microsoft.ServiceBus/namespaces/promitor-messaging
- resourceUri: Microsoft.ServiceBus/namespaces/promitor-messaging-2";
            var node = YamlUtils.CreateYamlNode(yamlText);

            var resourceDeserializer = new Mock<IDeserializer<AzureResourceDefinitionV1>>();
            _resourceDeserializerFactory.Setup(
                f => f.GetDeserializerFor(It.IsAny<ResourceType>())).Returns(resourceDeserializer.Object);

            var resources = new List<AzureResourceDefinitionV1>();
            resourceDeserializer.Setup(
                d => d.Deserialize((YamlSequenceNode)node.Children["resources"])).Returns(resources);

            // Act
            var definition = _deserializer.Deserialize(node);

            // Assert
            Assert.Null(definition.Resources);
        }
    }
}
