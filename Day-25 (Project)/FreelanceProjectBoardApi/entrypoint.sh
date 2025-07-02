#!/bin/bash
set -e

echo "🔄 Running EF Core migrations..."

# Change to root where the .csproj exists
cd /app

# Run EF database update by pointing to the project
dotnet ef database update --project FreelanceProjectBoardApi.csproj

echo "🚀 Starting API..."
cd /app/out
exec dotnet FreelanceProjectBoardApi.dll
