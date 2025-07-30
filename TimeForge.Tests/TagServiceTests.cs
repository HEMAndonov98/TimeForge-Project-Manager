using Microsoft.Extensions.Logging;
using Moq;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services;
using TimeForge.ViewModels.Tag;

namespace TimeForge.Tests;

    [TestFixture]
    public class TagServiceTests
    {
        private Mock<ITimeForgeRepository> repoMock;
        private Mock<ILogger<TagService>> loggerMock;
        private TagService tagService;

        [SetUp]
        public void Setup()
        {
            // Arrange shared mocks and system under test
            repoMock = new Mock<ITimeForgeRepository>();
            loggerMock = new Mock<ILogger<TagService>>();
            tagService = new TagService(repoMock.Object, loggerMock.Object);
        }

        #region CreateTagAsync

        [Test]
        public async Task CreateTagAsync_ValidInput_AddsAndSavesTag()
        {
            // Arrange
            var input = new TagInputModel { Id = "123", Name = "Urgent", UserId = "user1" };
            repoMock.Setup(r => r.AddAsync(It.IsAny<Tag>()))
                    .ReturnsAsync(new Tag { Id = input.Id });

            // Act
            await tagService.CreateTagAsync(input);

            // Assert
            repoMock.Verify(r => r.AddAsync(It.Is<Tag>(t => t.Name == input.Name && t.UserId == input.UserId)), Times.Once);
            repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void CreateTagAsync_NullInput_ThrowsArgumentNullException()
        {
            TagInputModel input = null;
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => tagService.CreateTagAsync(input));
        }

        [Test]
        public void CreateTagAsync_AddAsyncThrows_ThrowsException()
        {
            // Arrange
            var input = new TagInputModel { Id = "123", Name = "Fail", UserId = "user1" };
            repoMock.Setup(r => r.AddAsync(It.IsAny<Tag>())).ThrowsAsync(new Exception("DB Error"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(() => tagService.CreateTagAsync(input));
        }

        #endregion

        #region GetTagByIdAsync

        [Test]
        public async Task GetTagByIdAsync_ValidId_ReturnsTagViewModel()
        {
            // Arrange
            var tag = new Tag { Id = "t1", Name = "Tag1" };
            repoMock.Setup(r => r.GetByIdAsync<Tag>("t1")).ReturnsAsync(tag);

            // Act
            var result = await tagService.GetTagByIdAsync("t1");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo("t1"));
            Assert.That(result.Name, Is.EqualTo("Tag1"));
        }

        [Test]
        public void GetTagByIdAsync_NullOrEmptyId_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => tagService.GetTagByIdAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => tagService.GetTagByIdAsync(""));
        }

        [Test]
        public void GetTagByIdAsync_NotFound_ThrowsInvalidOperationException()
        {
            // Arrange: repository returns null
            repoMock.Setup(r => r.GetByIdAsync<Tag>("missing")).ReturnsAsync((Tag)null);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => tagService.GetTagByIdAsync("missing"));
        }

        #endregion

        #region GetAllTagsAsync
        
        //TODO create an in memmory db to test these methods


        [Test]
        public void GetAllTagsAsync_NullOrEmptyUserId_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => tagService.GetAllTagsAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => tagService.GetAllTagsAsync(""));
        }

        #endregion

        #region DeleteTagAsync

        [Test]
        public void DeleteTag_ValidId_DeletesAndSaves()
        {
            // Arrange
            var tag = new Tag { Id = "t1", Name = "ToDelete" };
            repoMock.Setup(r => r.GetByIdAsync<Tag>("t1")).ReturnsAsync(tag);

            // Act & Assert (no exception expected)
            Assert.DoesNotThrowAsync(() => Task.Run(() => tagService.DeleteTagAsync("t1")));

            // Assert repo was used correctly
            repoMock.Verify(r => r.Delete(tag), Times.Once);
            repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void DeleteTag_NullOrEmptyId_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => Task.Run(() => tagService.DeleteTagAsync(null)));
            Assert.ThrowsAsync<ArgumentNullException>(() => Task.Run(() => tagService.DeleteTagAsync("")));
        }

        [Test]
        public void DeleteTag_TagNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            repoMock.Setup(r => r.GetByIdAsync<Tag>("missing")).ReturnsAsync((Tag)null);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => Task.Run(() => tagService.DeleteTagAsync("missing")));
        }

        #endregion
    }