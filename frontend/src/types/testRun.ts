export type TestRunStatus =
  | 'Queued'
  | 'Cloning'
  | 'Analyzing'
  | 'Building'
  | 'Testing'
  | 'Completed'
  | 'Failed'

export interface TestRun {
  id: string
  repositoryUrl: string
  status: TestRunStatus
  createdAtUtc: string
  startedAtUtc: string | null
  completedAtUtc: string | null
  errorMessage: string | null
}

export interface TestRunReport {
  testRunId: string
  buildProjectPath: string | null
  buildExitCode: number | null
  buildDurationMilliseconds: number | null
  buildStandardOutput: string | null
  buildStandardError: string | null
  testProjectPaths: string[]
  testExitCode: number | null
  testDurationMilliseconds: number | null
  testStandardOutput: string | null
  testStandardError: string | null
  passedCount: number | null
  failedCount: number | null
  skippedCount: number | null
}
