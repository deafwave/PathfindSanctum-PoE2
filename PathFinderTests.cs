using ExileCore;
using ExileCore.PoEMemory.Elements.Sanctum;
using Moq;
using Xunit;

namespace PathfindSanctum.Tests;

public class PathFinderTests
{
    private readonly Mock<GameController> mockGameController;
    private readonly Mock<PathfindSanctumSettings> mockSettings;
    private readonly Mock<SanctumStateTracker> mockStateTracker;

    public PathFinderTests()
    {
        mockGameController = new Mock<GameController>();
        mockSettings = new Mock<PathfindSanctumSettings>();
        mockStateTracker = new Mock<SanctumStateTracker>();

        mockSettings.Setup(s => s.CurrentProfile).Returns("Default");
        mockSettings.Setup(s => s.Profiles).Returns(new Dictionary<string, ProfileContent>
        {
            ["Default"] = ProfileContent.CreateDefaultProfile()
        });
    }

    [Fact]
    public void FindBestPath_SimpleThreeRoomPath_ReturnsCorrectPath()
    {
        // Arrange
        var room1 = CreateMockRoom(0, 0, true);  // Player start
        var room2 = CreateMockRoom(0, 1, false); // Middle room
        var room3 = CreateMockRoom(0, 2, false); // End room with high reward

        room1.Setup(r => r.GetConnectedRooms()).Returns(new List<SanctumRoomElement> { room2.Object });
        room2.Setup(r => r.GetConnectedRooms()).Returns(new List<SanctumRoomElement> { room1.Object, room3.Object });
        room3.Setup(r => r.GetConnectedRooms()).Returns(new List<SanctumRoomElement> { room2.Object });

        // Set up room3 with a high-value reward
        room3.Setup(r => r.Data.Rewards).Returns(new[] { "Gold Key" });

        var roomsByLayer = new List<List<SanctumRoomElement>>
        {
            new() { room1.Object, room2.Object, room3.Object }
        };

        var pathFinder = new PathFinder(roomsByLayer, mockGameController.Object, mockSettings.Object, mockStateTracker.Object);

        // Act
        pathFinder.CreateRoomWeightMap();
        var path = pathFinder.FindBestPath();

        // Assert
        Assert.Equal(3, path.Count);
        Assert.Equal((0, 0), path[0]); // Start
        Assert.Equal((0, 1), path[1]); // Middle
        Assert.Equal((0, 2), path[2]); // End
    }

    private Mock<SanctumRoomElement> CreateMockRoom(int layer, int index, bool isCurrentRoom)
    {
        var room = new Mock<SanctumRoomElement>();
        room.Setup(r => r.Layer).Returns(layer);
        room.Setup(r => r.RoomIndex).Returns(index);
        room.Setup(r => r.IsCurrentRoom).Returns(isCurrentRoom);
        room.Setup(r => r.Data.RoomType).Returns("Chalice");
        room.Setup(r => r.Data.Afflictions).Returns(Array.Empty<string>());
        room.Setup(r => r.Data.Rewards).Returns(Array.Empty<string>());
        return room;
    }
} 
