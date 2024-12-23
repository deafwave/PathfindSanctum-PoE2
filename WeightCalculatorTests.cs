using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements.Sanctum;
using Moq;
using Xunit;

namespace PathfindSanctum.Tests;

public class WeightCalculatorTests
{
    private readonly Mock<GameController> mockGameController;
    private readonly Mock<PathfindSanctumSettings> mockSettings;
    private readonly WeightCalculator calculator;

    public WeightCalculatorTests()
    {
        mockGameController = new Mock<GameController>();
        mockSettings = new Mock<PathfindSanctumSettings>();
        mockSettings.Setup(s => s.CurrentProfile).Returns("Default");
        mockSettings.Setup(s => s.Profiles).Returns(new Dictionary<string, ProfileContent>
        {
            ["Default"] = ProfileContent.CreateDefaultProfile(),
            ["No-Hit"] = ProfileContent.CreateNoHitProfile()
        });

        calculator = new WeightCalculator(mockGameController.Object, mockSettings.Object);
    }

    [Fact]
    public void CalculateRoomWeight_WithIronManacles_HighEvasion_AppliesNegativeWeight()
    {
        // Arrange
        var mockRoom = new Mock<SanctumRoomElement>();
        var mockLife = new Mock<Life>();
        mockLife.Setup(l => l.Evasion).Returns(7000);
        
        var mockPlayer = new Mock<Entity>();
        mockPlayer.Setup(p => p.GetComponent<Life>()).Returns(mockLife.Object);
        
        mockGameController.Setup(gc => gc.EntityListWrapper.Player).Returns(mockPlayer.Object);
        
        mockRoom.Setup(r => r.Data.Afflictions).Returns(new[] { "Iron Manacles" });
        mockRoom.Setup(r => r.Data.RoomType).Returns("Chalice");
        mockRoom.Setup(r => r.Data.Rewards).Returns(Array.Empty<string>());
        mockRoom.Setup(r => r.GetConnectedRooms()).Returns(new List<SanctumRoomElement>());

        // Act
        var (weight, debug) = calculator.CalculateRoomWeight(mockRoom.Object);

        // Assert
        Assert.True(weight < 1000000); // Base weight is 1000000, should be reduced
        Assert.Contains("High Evasion Build: -750000", debug);
    }

    // Add more tests for different affliction combinations, room types, etc.
} 
