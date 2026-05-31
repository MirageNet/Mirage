
---

## Objective

Migrate the C# analyzer test suite away from inline string mocks and raw text verification. Transition to an external file-based test runner that validates test cases against the real, live `Mirage` library assembly.

---

## Phase 1: Establish the Real Assembly Reference

* **Goal:** Ensure the Roslyn compiler context used during analyzer tests has access to the actual `Mirage` types.
* **Action Steps:**
* Reference the dynamically generated Unity `Mirage.csproj` (or its compiled output `.dll`) from the Analyzer Test project.
* Locate the central `VerifyCS` helper (or custom `AnalyzerTest` configuration).
* Inject the real `Mirage` metadata reference into the test environment via `MetadataReference.CreateFromFile`.



---

## Phase 2: Implement File-Based Test Discovery

* **Goal:** Replace raw code strings inside tests with individual, fully typed `.cs` files.
* **Action Steps:**
* Create a designated test fixtures directory (e.g., `TestData/`) inside the test project.
* Ensure any `.cs` files inside this folder are configured to copy to the output directory or treat as embedded resources.
* Refactor the test runner to load these files dynamically by name using file I/O operations (mimicking the project's existing Mono.Cecil weaver test architecture).



---

## Phase 3: Define Test File Structure & Isolation Rules

* **Goal:** Maintain clean separation with exactly one primary source file per test case, while allowing shared helpers if necessary.
* **Action Steps:**
* Enforce a **1 C# file per test case** rule for standard test scenarios to maximize readability and compile isolation.
* For tests requiring shared data types or common network configurations, implement a utility mechanism in the test runner to load and append shared helper snippets, or pass multiple source files into the Roslyn verifier's `TestState.Sources` collection.



---

## Expected Outcome

The IDE will natively provide syntax highlighting, refactoring support, and immediate compile-error feedback directly inside the test data files. Any breaking changes or API drifts in the core `Mirage` assembly will instantly fail the analyzer tests during the compilation phase, before the analyzer code even executes.