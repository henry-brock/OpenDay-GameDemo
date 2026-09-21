# Test Results

NUnit result files produced by the Unity Test Framework, retained as evidence
for the Testing and Evaluation chapter of the accompanying dissertation.

| File | Suite | Tests | Result | Run |
|---|---|---|---|---|
| `editmode-results.xml` | EditMode | 15 | 15 passed, 0 failed | 2026-09-18 |
| `playmode-results.xml` | PlayMode | 10 | 10 passed, 0 failed | 2026-09-18 |

## Regenerating

From the `GameDemo` directory:

```
Unity -batchmode -nographics -projectPath . \
      -runTests -testPlatform EditMode -testResults TestResults/editmode-results.xml

Unity -batchmode -nographics -projectPath . \
      -runTests -testPlatform PlayMode -testResults TestResults/playmode-results.xml
```

**Do not add `-quit`.** With `-runTests` it causes the editor to exit before the
test runner starts: no results file is written, no error is logged, and the
process still returns exit code 0 — a failure that presents as a pass.
