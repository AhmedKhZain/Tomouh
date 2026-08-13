@echo off
echo 🔗 Fixing Auth.slnx and adding all projects...

:: 1. إضافة الـ Projects الأساسية
dotnet sln backend\services\Auth\Auth.slnx add backend\services\Auth\src\Auth.API\Auth.API.csproj
dotnet sln backend\services\Auth\Auth.slnx add backend\services\Auth\src\Auth.Application\Auth.Application.csproj
dotnet sln backend\services\Auth\Auth.slnx add backend\services\Auth\src\Auth.Domain\Auth.Domain.csproj
dotnet sln backend\services\Auth\Auth.slnx add backend\services\Auth\src\Auth.Infrastructure\Auth.Infrastructure.csproj

:: 2. إضافة مشاريع الـ Unit Tests
dotnet sln backend\services\Auth\Auth.slnx add backend\services\Auth\tests\Auth.Domain.UnitTests\Auth.Domain.UnitTests.csproj
dotnet sln backend\services\Auth\Auth.slnx add backend\services\Auth\tests\Auth.Application.UnitTests\Auth.Application.UnitTests.csproj

:: 3. إضافة الـ Shared Libraries
dotnet sln backend\services\Auth\Auth.slnx add backend\shared\Tomouh.Shared.Kernel\Tomouh.Shared.Kernel.csproj
dotnet sln backend\services\Auth\Auth.slnx add backend\shared\Tomouh.Shared.Infrastructure\Tomouh.Shared.Infrastructure.csproj

echo ✅ All projects successfully added to Auth.slnx!
pause