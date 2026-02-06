using LotteryCrawler;
using System.Reflection.Emit;

public record AppConfig(int PickupAttempt = 1000, int MaxTry = 10000, int NumberGamesStudy = 30);
