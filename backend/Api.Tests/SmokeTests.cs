namespace Api.Tests;

// Тесты для проверки базовой конфигурации проекта
public sealed class SmokeTests
{
    // Базовый тест проверки конфигурации проекта. Если не проходит - проблема с тестовым окружением.
    [Fact]
    public void Test_project_is_configured()
    {
        Assert.True(true);
    }
}