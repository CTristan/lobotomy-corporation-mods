# Plan: AutoFixture + AutoMoq Adoption

Status: **Draft**

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
- Full migration of all 75 test files for consistency
- Maintain 100% coverage and existing test semantics

## Approach

Install `AutoFixture.Xunit3` + `AutoFixture.AutoMoq`, create shared customizations (including Unity type delegation to existing `UnityTestExtensions`), then migrate all tests — starting with the highest-boilerplate files to validate the approach.

### Key patterns after migration

**Before (CollectorFactoryTests constructor — 20 lines):**
```csharp
_mockDetector = new Mock<IEnvironmentDetector>();
_mockHarmony1Source = new Mock<IPatchInspectionSource>();
// ... 10 more mock fields + 6 Setup() calls
```

**After (2 lines):**
```csharp
private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
private CollectorFactory CreateFactory() => _fixture.Create<CollectorFactory>();
```

**Before (null-check test — long single line with 11 non-null + 1 null):**
```csharp
Action act = () => _ = new CollectorFactory(null, _mockHarmony1Source.Object, ...12 params...);
```

**After (freeze null + create):**
```csharp
_fixture.Inject<IEnvironmentDetector>(null);
Action act = () => _fixture.Create<CollectorFactory>();
```

**Before (Theory with InlineData):**
```csharp
[Theory]
[InlineData(0.5f, "test")]
```

**After (AutoData for anonymous values, InlineAutoData for mixed):**
```csharp
[Theory, AutoData]
public void Method_does_something(float value, string name) { ... }
```

### Unity type strategy

Register AutoFixture customizations that delegate to existing `UnityTestExtensions` factory methods. This gives a unified `fixture.Create<AgentModel>()` API while reusing the proven `RuntimeHelpers.GetUninitializedObject()` initialization. Types without existing factories will be added to the customization as needed.

## Tasks

### Phase 1: Setup (0/4)
- [ ] Add NuGet packages to test csproj: `AutoFixture.Xunit3`, `AutoFixture.AutoMoq`
- [ ] Create `TestHelpers/LobotomyAutoDataAttribute.cs` — custom `[AutoData]` attribute that applies `AutoMoqCustomization` automatically
- [ ] Create `TestHelpers/LobotomyInlineAutoDataAttribute.cs` — companion for `[InlineAutoData]` with AutoMoq
- [ ] Create `TestHelpers/UnityCustomization.cs` — `ICustomization` that registers `UnityTestExtensions` factories for common Unity types (AgentModel, CreatureModel, etc.)

### Phase 2: Migrate DebugPanel tests (0/8)
- [ ] `CollectorFactoryTests` (12 mocks → fixture)
- [ ] `DiagnosticReportBuilderTests` (10+ mocks → fixture)
- [ ] `DependencyCheckerTests`
- [ ] `ErrorLogCollectorTests`
- [ ] `FilesystemValidationCollectorTests`
- [ ] `KnownIssuesCheckerTests`
- [ ] `JsonKnownIssuesDatabaseTests`
- [ ] Remaining DebugPanel test files (model tests, report tests, etc.)

### Phase 3: Migrate mod patch tests (0/6)
- [ ] `BadLuckProtectionForGifts` tests
- [ ] `BugFixes` tests
- [ ] `FreeCustomization` tests
- [ ] `GiftAlertIcon` tests
- [ ] `NotifyWhenAgentReceivesGift` tests
- [ ] `WarnWhenAgentWillDieFromWorking` tests

### Phase 4: Migrate shared/infrastructure tests (0/2)
- [ ] `CommonTests` (logger, extensions, etc.)
- [ ] Any remaining test files

### Phase 5: Cleanup and documentation (0/3)
- [ ] Remove unused mock helper methods from `TestExtensions` that are now redundant
- [ ] Update `CLAUDE.md` test conventions section to document AutoFixture patterns
- [ ] Run full test suite with coverage to verify no regressions

## Risks & Considerations

- **Null-check tests with AutoMoq**: AutoMoq auto-generates non-null mocks, so null-check tests need `fixture.Inject<T>(null)` to override a specific parameter. This is less explicit than the current approach of manually passing null — but far less verbose for 12-parameter constructors.
- **Mock verification tests**: Tests that call `_mockFoo.Verify(...)` need to `Freeze<Mock<IFoo>>()` first so the fixture returns the same mock instance. This is standard AutoFixture/Moq practice but must be applied consistently.
- **Unity type edge cases**: Some Unity types have circular references (e.g., `UseSkill` ↔ `CreatureModel`). The customization must handle these the same way `UnityTestExtensions` does. If a Unity type doesn't have an existing factory method, add one to `UnityTestExtensions` first, then register it.
- **Test readability tradeoff**: `fixture.Create<CollectorFactory>()` hides constructor parameters. Mitigate by using `Freeze` for any mock that the test cares about — this makes the "interesting" dependencies explicit while hiding the irrelevant ones.
- **75 files to migrate**: Do this incrementally, verifying tests pass after each phase. Don't batch all changes into one commit.

## Verification

After each phase:
1. `dotnet build LobotomyCorporationMods.sln` — verify compilation
2. `dotnet test /p:CollectCoverage=true /p:CoverletOutput="./coverage.opencover.xml" /p:CoverletOutputFormat=opencover LobotomyCorporationMods.sln` — verify all tests pass with coverage
3. `dotnet ci --check` — verify formatting and CI compliance

After final phase:
4. Confirm coverage percentages have not decreased
5. Verify no test semantics changed (same number of test methods, same assertions)
