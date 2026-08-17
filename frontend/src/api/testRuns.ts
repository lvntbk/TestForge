import type {
  TestRun,
  TestRunReport,
} from '../types/testRun'

const API_BASE_URL = '/api'

async function request<T>(
  path: string,
  options?: RequestInit,
): Promise<T> {
  const response = await fetch(
    `${API_BASE_URL}${path}`,
    options,
  )

  if (!response.ok) {
    const details = await response.text()

    throw new Error(
      details ||
        `İstek başarısız oldu: HTTP ${response.status}`,
    )
  }

  return response.json() as Promise<T>
}

export function createTestRun(
  repositoryUrl: string,
): Promise<TestRun> {
  return request<TestRun>('/test-runs', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ repositoryUrl }),
  })
}

export function getTestRun(
  testRunId: string,
  signal?: AbortSignal,
): Promise<TestRun> {
  return request<TestRun>(
    `/test-runs/${testRunId}`,
    { signal },
  )
}

export function getTestRunReport(
  testRunId: string,
  signal?: AbortSignal,
): Promise<TestRunReport> {
  return request<TestRunReport>(
    `/test-runs/${testRunId}/report`,
    { signal },
  )
}
