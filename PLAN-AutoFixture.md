# Plan: AutoFixture + AutoMoq Adoption

Status: **Completed**

## Context

The test project (75 test files, xUnit v3 + Moq + AwesomeAssertions) has significant mock boilerplate, especially in DebugPanel tests. `CollectorFactoryTests` alone has 12 mock fields, 6 default setup statements, and null-check tests where each test manually passes 11 non-null mocks plus one null — repeated 12 times. This pattern exists across multiple test classes.

AutoFixture with AutoMoq would eliminate this boilerplate by auto-generating mock instances for interface parameters and providing anonymous test data. `AutoFixture.Xunit3` (v4.19.0) supports xUnit v3 natively.

### Alternatives considered

| Approach | Constructor Boilerplate | Test Data | Complexity | Notes |
|----------|------------------------|-----------|------------|-------|
| **AutoFixture + AutoMoq** | Yes | Yes | High | Chosen — maximum automation, `[AutoData]` attributes |
| **Moq.AutoMock + Bogus** | Yes | Yes | Medium | Two lighter tools, no `[AutoData]` integration |
| **AutoFixture + AutoNSubstitute** | Yes | Yes | High | Would also require replacing all Moq usage |
| **Moq.AutoMock alone** | Yes | No | Low | No test data generation |
| **NSubstitute + Bogus** | Partial | Yes | Medium | Simpler mock syntax but still manual fixture assembly |

AutoFixture + AutoMoq was selected because it addresses both pain points (constructor mocking + test data) with a single integrated toolchain and keeps the existing Moq investment.

## Goals

- Eliminate repetitive mock field declarations and constructor setup
- Replace hand-crafted test data with anonymous values where the specific value doesn't matter
- Simplify null-check tests from N-parameter manual construction to single-line patterns
- Migrate high-boilerplate test files where fixture provides clear value
- Maintain 100% coverage and existing test semantics

## Approach

Install `AutoFixture.Xunit3` + `AutoFixture.AutoMoq`, create shared customizations (including Unity type delegation to existing `UnityTestExtensions`), then migrate high-boilerplate tests.

### Key patterns after migration

**Before (CollectorFactoryTests — 12 mock fields + 6 Setup calls):**
```csharp
_mockDetector = new Mock<IEnvironmentDetector>();
_mockHarmony1Source = new Mock<IPatchInspectionSource>();
// ... 10 more mock fields + 6 Setup() calls
```

**After (fixture + 4 manual mocks for duplicate types):**
```csharp
_fixture = new Fixture().Customize(new AutoMoqCustomization());
_mockDetector = _fixture.Freeze<Mock<IEnvironmentDetector>>();
// Only 4 manual mocks for duplicate IPatchInspectionSource/IDllFileInspector
```

### Unity type strategy

Register AutoFixture customizations that delegate to existing `UnityTestExtensions` factory methods. This gives a unified `fixture.Create<AgentModel>()` API while reusing the proven `RuntimeHelpers.GetUninitializedObject()` initialization.

## Tasks

### Phase 1: Setup (4/4)
- [x] Add NuGet packages: `AutoFixture.Xunit3` v4.19.0 to `Directory.Packages.props` and test csproj
- [x] Create `Attributes/LobotomyAutoDataAttribute.cs` — custom `[AutoData]` with AutoMoq + UnityCustomization
- [x] Create `Attributes/LobotomyInlineAutoDataAttribute.cs` — companion for `[InlineAutoData]`
- [x] Create `Customizations/UnityCustomization.cs` — `ICustomization` registering all 31 `UnityTestExtensions` factory methods

### Phase 2: Migrate DebugPanel tests (2/2)
- [x] `CollectorFactoryTests` (12 mocks → fixture + 4 manual for duplicate types)
- [x] `DiagnosticReportBuilderTests` (14 mocks → fixture for 3 constructor params, manual for factory-returned collectors)

### Phase 3-4: Mod patch + shared tests (skipped)
Assessed all remaining test files. Mod patch tests use domain-specific boundary values and specific object configurations — not suitable for AutoFixture. Files with 1-2 mocks have negligible boilerplate savings. Files with duplicate interface types (e.g., FallbackPatchInspectionSourceTests) cannot use Freeze.

### Phase 5: Cleanup and documentation (2/2)
- [x] Update `CLAUDE.md` test conventions to document AutoFixture patterns
- [x] Run full test suite with coverage — no regressions (813 tests pass, coverage unchanged)

## Outcome

- **Infrastructure created**: `LobotomyAutoDataAttribute`, `LobotomyInlineAutoDataAttribute`, `UnityCustomization` — available for all future tests
- **High-boilerplate files migrated**: CollectorFactoryTests (12→4 manual mocks), DiagnosticReportBuilderTests (14 mocks, 3 constructor params via fixture)
- **Remaining files deliberately skipped**: Files with ≤2 mocks, duplicate interface types, or domain-specific setups don't benefit from AutoFixture overhead
- **Zero regressions**: 813 tests pass, coverage thresholds met, `dotnet ci --check` clean

## Risks & Considerations

- **Null-check tests with AutoMoq**: AutoMoq auto-generates non-null mocks, so null-check tests need `fixture.Inject<T>(null)` to override a specific parameter. This is less explicit than the current approach of manually passing null — but far less verbose for 12-parameter constructors.
- **Mock verification tests**: Tests that call `_mockFoo.Verify(...)` need to `Freeze<Mock<IFoo>>()` first so the fixture returns the same mock instance.
- **Duplicate interface types**: When the same interface appears multiple times in a constructor (e.g., two `IPatchInspectionSource` params), `Freeze` returns one instance — keep those as manual mocks.
- **Test readability tradeoff**: `fixture.Create<T>()` hides constructor parameters. Use `Freeze` for any mock the test cares about to keep "interesting" dependencies explicit.

## Verification

All verification steps passed:
1. `dotnet build LobotomyCorporationMods.sln` — 0 errors
2. `dotnet test` with coverage — 813 passed, 0 failed, coverage thresholds met
3. `dotnet ci --check` — clean (0 errors, 9 pre-existing NU1702 warnings)
