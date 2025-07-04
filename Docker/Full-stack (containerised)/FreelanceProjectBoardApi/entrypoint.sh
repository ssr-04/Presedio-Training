#!/bin/bash
set -e

echo "🔄 Running EF Core migrations..."

# Point to the original source directory
cd /src
dotnet ef database update --project FreelanceProjectBoardApi.csproj

echo "🚀 Starting API..."
cd /app
exec dotnet FreelanceProjectBoardApi.dll
