#!/bin/bash

# 1. إنشاء الفولدرات الرئيسية
mkdir -p frontend
mkdir -p tests/IntegrationTests tests/E2ETests
mkdir -p backend/shared backend/services/Auth

echo "🚀 Creating Shared Libraries..."

# 2. إنشاء الـ Shared Class Libraries
dotnet new classlib -o backend/shared/Tomouh.Shared.Kernel
dotnet new classlib -o backend/shared/Tomouh.Shared.Infrastructure

# ربط الـ Infrastructure بالـ Kernel
dotnet add backend/shared/Tomouh.Shared.Infrastructure reference backend/shared/Tomouh.Shared.Kernel

echo "🔐 Creating Auth Microservice & Solutions..."

# 3. إنشاء مشاريع Auth Microservice
dotnet new webapi -o backend/services/Auth/src/Auth.API
dotnet new classlib -o backend/services/Auth/src/Auth.Application
dotnet new classlib -o backend/services/Auth/src/Auth.Domain
dotnet new classlib -o backend/services/Auth/src/Auth.Infrastructure

# 4. إنشاء مشاريع التست لخدمة Auth (xUnit)
dotnet new xunit -o backend/services/Auth/tests/Auth.Domain.UnitTests
dotnet new xunit -o backend/services/Auth/tests/Auth.Application.UnitTests

echo "🔗 Linking Auth Projects & Dependencies..."

# ربط الطبقات الداخلية لخدمة Auth
dotnet add backend/services/Auth/src/Auth.Application reference backend/services/Auth/src/Auth.Domain
dotnet add backend/services/Auth/src/Auth.Infrastructure reference backend/services/Auth/src/Auth.Application
dotnet add backend/services/Auth/src/Auth.API reference backend/services/Auth/src/Auth.Infrastructure

# ربط مشاريع Auth بالـ Shared Libraries
dotnet add backend/services/Auth/src/Auth.Domain reference backend/shared/Tomouh.Shared.Kernel
dotnet add backend/services/Auth/src/Auth.Infrastructure reference backend/shared/Tomouh.Shared.Infrastructure

# ربط مشاريع التست بالطبقات الخاصة بها وبمكتبة FluentAssertions
dotnet add backend/services/Auth/tests/Auth.Domain.UnitTests reference backend/services/Auth/src/Auth.Domain
dotnet add backend/services/Auth/tests/Auth.Domain.UnitTests package FluentAssertions

dotnet add backend/services/Auth/tests/Auth.Application.UnitTests reference backend/services/Auth/src/Auth.Application
dotnet add backend/services/Auth/tests/Auth.Application.UnitTests package FluentAssertions

echo "📦 Generating Auth Solution (Auth.sln)..."

# 5. إنشاء Auth.sln وإضافة جميع مشاريع Auth إليه
dotnet new sln -n Auth -o backend/services/Auth
dotnet sln backend/services/Auth/Auth.sln add backend/services/Auth/src/Auth.API/Auth.API.csproj
dotnet sln backend/services/Auth/Auth.sln add backend/services/Auth/src/Auth.Application/Auth.Application.csproj
dotnet sln backend/services/Auth/Auth.sln add backend/services/Auth/src/Auth.Domain/Auth.Domain.csproj
dotnet sln backend/services/Auth/Auth.sln add backend/services/Auth/src/Auth.Infrastructure/Auth.Infrastructure.csproj
dotnet sln backend/services/Auth/Auth.sln add backend/services/Auth/tests/Auth.Domain.UnitTests/Auth.Domain.UnitTests.csproj
dotnet sln backend/services/Auth/Auth.sln add backend/services/Auth/tests/Auth.Application.UnitTests/Auth.Application.UnitTests.csproj
dotnet sln backend/services/Auth/Auth.sln add backend/shared/Tomouh.Shared.Kernel/Tomouh.Shared.Kernel.csproj
dotnet sln backend/services/Auth/Auth.sln add backend/shared/Tomouh.Shared.Infrastructure/Tomouh.Shared.Infrastructure.csproj

echo "✅ Done! Auth Microservice and Shared structure created successfully."