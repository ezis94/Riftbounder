# Sprint 1 Hotfix 2

Adds `global using Xunit;` to both test projects.

This fixes compiler errors where `[Fact]` and `Assert` were unavailable despite the xUnit package references being present.
