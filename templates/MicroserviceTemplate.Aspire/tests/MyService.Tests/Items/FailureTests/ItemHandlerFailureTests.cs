using Moq;
using MyService.Application.Item.Handlers;
using MyService.Application.Item.Commands;
using MyService.Application.Item.Dtos;
using MyService.Domain.Entities;
using FluentValidation;
using MyService.Domain.Interfaces;
using MyService.Application.Item.Mappings;
using Mediator;

namespace MyService.Tests.Items.FailureTests;

public class ItemHandlerFailureTests
{
    [Fact]
    public async Task CreateItemHandler_RollsBack_WhenRepositoryThrows()
    {
        var repoMock = new Mock<IItemRepository>();
        var mapperMock = new Mock<IItemMapper>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var validatorMock = new Mock<IValidator<CreateItemCommand>>();

        var command = new CreateItemCommand(new CreateItemDto { Name = "Test", Quantity = 1 });
        validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        repoMock.Setup(r => r.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Insert failed"));

        var handler = new CreateItemHandler(repoMock.Object, mapperMock.Object, unitOfWorkMock.Object, validatorMock.Object);

        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None).AsTask());
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);  // Ensure RollbackAsync is called
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);   // Ensure CommitAsync is never called
    }

    [Fact]
    public async Task UpdateItemHandler_RollsBack_WhenRepositoryThrows()
    {
        var repoMock = new Mock<IItemRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var validatorMock = new Mock<IValidator<UpdateItemCommand>>();

        var command = new UpdateItemCommand { Id = 1, Dto = new UpdateItemDto { Name = "Test", Quantity = 2 } };
        validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        repoMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(new Item { Id = 1, Name = "Old", Quantity = 1 });
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Update failed"));

        var handler = new UpdateItemHandler(repoMock.Object, validatorMock.Object, unitOfWorkMock.Object);

        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None).AsTask());
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);  // Ensure RollbackAsync is called
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);   // Ensure CommitAsync is never called
    }

    [Fact]
    public async Task DeleteItemHandler_RollsBack_WhenRepositoryThrows()
    {
        var repoMock = new Mock<IItemRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var command = new DeleteItemCommand(id: 1);
        repoMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(new Item { Id = 1, Name = "DeleteMe", Quantity = 1 });
        repoMock.Setup(r => r.DeleteAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Delete failed"));

        var handler = new DeleteItemHandler(repoMock.Object, unitOfWorkMock.Object);

        await Assert.ThrowsAsync<Exception>(() => ((ICommandHandler<DeleteItemCommand, Unit>)handler).Handle(command, CancellationToken.None).AsTask());
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);  // Ensure RollbackAsync is called
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);   // Ensure CommitAsync is never called
    }
}