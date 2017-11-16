﻿using System;
using System.Threading.Tasks;
using SimpleUptime.Domain.Models;
using SimpleUptime.Infrastructure.Middlewares;
using SimpleUptime.Infrastructure.Repositories;
using ToyStorage;
using Xunit;

namespace SimpleUptime.IntegrationTests.Infrastructure.Repositories
{
    public class HttpMonitorRepositoryTests : IClassFixture<CloudStorageFixture>
    {
        private readonly HttpMonitorRepository _repository;

        public HttpMonitorRepositoryTests(CloudStorageFixture cloudStorageFixture)
        {
            var pipeline = new MiddlewarePipeline()
                .Use<IgnoreNotFoundExceptionMiddleware>()
                .UseJsonFormatter()
                .Use<BlobStorageMiddleware>();

            var documentCollection = new DocumentCollection(cloudStorageFixture.CloudBlobContainer, pipeline);

            _repository = new HttpMonitorRepository(documentCollection);
        }

        [Fact]
        public async Task GetReturnsEntity()
        {
            // Arrange
            var existingEntity = await GenerateAndPersistHttpMonitorAsync();

            // Act
            var entity = await _repository.GetAsync(existingEntity.Id);

            // Assert
            Assert.Equal(existingEntity.Id, entity.Id);
        }

        [Fact]
        public async Task GetReturnsNullWhenNotFound()
        {
            // Arrange
            var id = HttpMonitorId.Create();

            // Act
            var entity = await _repository.GetAsync(id);

            // Assert
            Assert.Null(entity);
        }

        [Fact]
        public async Task PutThrowsExceptionWhenEntityIsNull()
        {
            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.PutAsync(null));

            // Assert
            Assert.Equal("httpMonitor", exception.ParamName);
        }

        [Fact]
        public async Task PutCreatesEntity()
        {
            // Arrange
            var entity = GenerateHttpMonitor();

            // Act
            await _repository.PutAsync(entity);

            // Assert
            var readEntity = await _repository.GetAsync(entity.Id);
            Assert.Equal(readEntity.Id, entity.Id);
        }

        [Fact]
        public async Task PutUpdatesEntity()
        {
            // Arrange
            var entity = await GenerateAndPersistHttpMonitorAsync();
            var newUrl = new Uri("http://foo-bar.com/");
            entity.Url = newUrl;

            // Act
            await _repository.PutAsync(entity);

            // Assert
            var readEntity = await _repository.GetAsync(entity.Id);
            Assert.Equal(newUrl, readEntity.Url);
        }

        [Fact]
        public async Task DeleteRemovesEntityWhenFound()
        {
            // Arrange
            var entity = await GenerateAndPersistHttpMonitorAsync();

            // Act
            await _repository.DeleteAsync(entity.Id);

            // Assert
            var readEntity = await _repository.GetAsync(entity.Id);
            Assert.Null(readEntity);
        }

        [Fact]
        public async Task DeleteDoesNotThrowExceptionWhenNotFound()
        {
            // Arrange
            var id = HttpMonitorId.Create();

            // Act
            // Assert no exception thrown
            await _repository.DeleteAsync(id);
        }

        private HttpMonitor GenerateHttpMonitor()
        {
            return new HttpMonitor(HttpMonitorId.Create(), new Uri("https://example.com"));
        }

        private async Task<HttpMonitor> GenerateAndPersistHttpMonitorAsync()
        {
            var httpMonitor = GenerateHttpMonitor();

            await _repository.PutAsync(httpMonitor);

            return httpMonitor;
        }
    }
}