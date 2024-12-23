using ExileCore.PoEMemory.Elements.Sanctum;
using Moq;
using Xunit;

namespace PathfindSanctum.Tests;

public class SanctumStateTrackerTests
{
    private readonly SanctumStateTracker tracker;

    public SanctumStateTrackerTests()
    {
        tracker = new SanctumStateTracker();
    }

    [Fact]
    public void IsSameSanctum_NewHash_ReturnsFalse()
    {
        // Act
        var result = tracker.IsSameSanctum(123);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSameSanctum_SameHash_ReturnsTrue()
    {
        // Arrange
        tracker.IsSameSanctum(123); // First call to set the hash

        // Act
        var result = tracker.IsSameSanctum(123);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void UpdateRoomStates_NewRoom_AddsToStates()
    {
        // Arrange
        var mockRoom = new Mock<SanctumRoomElement>();
        mockRoom.Setup(r => r.Data.RoomType).Returns("Chalice");
        mockRoom.Setup(r => r.Data.Afflictions).Returns(new[] { "Iron Manacles" });
        mockRoom.Setup(r => r.Data.Rewards).Returns(new[] { "Gold Key" });
        mockRoom.Setup(r => r.IsKnown).Returns(true);

        var roomsByLayer = new List<List<SanctumRoomElement>>
        {
            new() { mockRoom.Object }
        };

        // Act
        tracker.UpdateRoomStates(roomsByLayer);

        // Assert
        var state = tracker.GetBestKnownState(0, 0);
        Assert.NotNull(state);
        Assert.Equal("Chalice", state.RoomType);
        Assert.Contains("Iron Manacles", state.Afflictions);
        Assert.Contains("Gold Key", state.Rewards);
    }
} 
