import {
  useEffect,
  useState,
  type FormEvent,
} from 'react'
import {
  createTestRun,
  getTestRun,
  getTestRunReport,
} from './api/testRuns'
import type {
  TestRun,
  TestRunReport,
  TestRunStatus,
} from './types/testRun'
import './App.css'

const PIPELINE_STEPS: TestRunStatus[] = [
  'Queued',
  'Cloning',
  'Analyzing',
  'Building',
  'Testing',
  'Completed',
]

const STATUS_LABELS: Record<TestRunStatus, string> = {
  Queued: 'Sırada',
  Cloning: 'Klonlanıyor',
  Analyzing: 'Analiz ediliyor',
  Building: 'Derleniyor',
  Testing: 'Test ediliyor',
  Completed: 'Tamamlandı',
  Failed: 'Başarısız',
}

function formatDuration(
  milliseconds: number | null,
): string {
  if (milliseconds === null) {
    return '—'
  }

  if (milliseconds < 1000) {
    return `${milliseconds} ms`
  }

  return `${(milliseconds / 1000).toFixed(1)} sn`
}

function getErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'Beklenmeyen bir hata oluştu.'
}

function App() {
  const [repositoryUrl, setRepositoryUrl] = useState(
    'https://github.com/lvntbk/evofit',
  )
  const [testRun, setTestRun] = useState<TestRun | null>(
    null,
  )
  const [report, setReport] =
    useState<TestRunReport | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<
    string | null
  >(null)

  useEffect(() => {
    if (
      testRun === null ||
      testRun.status === 'Completed' ||
      testRun.status === 'Failed'
    ) {
      return
    }

    const controller = new AbortController()

    const timeoutId = window.setTimeout(async () => {
      try {
        const updatedRun = await getTestRun(
          testRun.id,
          controller.signal,
        )

        if (updatedRun.status === 'Completed') {
          const completedReport = await getTestRunReport(
            updatedRun.id,
            controller.signal,
          )

          setReport(completedReport)
        }

        setTestRun(updatedRun)
      } catch (error) {
        if (
          error instanceof DOMException &&
          error.name === 'AbortError'
        ) {
          return
        }

        setErrorMessage(getErrorMessage(error))
      }
    }, 2000)

    return () => {
      window.clearTimeout(timeoutId)
      controller.abort()
    }
  }, [testRun])

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    const normalizedUrl = repositoryUrl.trim()

    if (!normalizedUrl) {
      setErrorMessage('Repository URL boş bırakılamaz.')
      return
    }

    setIsSubmitting(true)
    setErrorMessage(null)
    setReport(null)
    setTestRun(null)

    try {
      const createdRun = await createTestRun(normalizedUrl)
      setTestRun(createdRun)
    } catch (error) {
      setErrorMessage(getErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  const currentStepIndex = testRun
    ? PIPELINE_STEPS.indexOf(testRun.status)
    : -1

  return (
    <main className="app-shell">
      <section className="hero">
        <div className="brand">
          <span className="brand-mark">TF</span>
          <span>TestForge</span>
        </div>

        <div className="hero-content">
          <p className="eyebrow">
            AUTOMATED REPOSITORY TESTING
          </p>
          <h1>
            GitHub reponu gönder,
            <span> test sonucunu izle.</span>
          </h1>
          <p className="hero-description">
            .NET projelerini izole Docker container’larında
            derler, test eder ve yapılandırılmış sonuç
            raporu üretir.
          </p>
        </div>

        <form className="run-form" onSubmit={handleSubmit}>
          <label htmlFor="repositoryUrl">
            GitHub repository URL
          </label>

          <div className="input-row">
            <input
              id="repositoryUrl"
              type="url"
              value={repositoryUrl}
              onChange={(event) =>
                setRepositoryUrl(event.target.value)
              }
              placeholder="https://github.com/owner/repository"
              disabled={isSubmitting}
              required
            />

            <button type="submit" disabled={isSubmitting}>
              {isSubmitting
                ? 'Başlatılıyor...'
                : 'Testi Başlat'}
            </button>
          </div>
        </form>
      </section>

      {errorMessage && (
        <section className="alert alert-error">
          <strong>İşlem tamamlanamadı</strong>
          <span>{errorMessage}</span>
        </section>
      )}

      {testRun ? (
        <section className="dashboard">
          <header className="run-header">
            <div>
              <p className="section-label">
                {testRun.status === 'Completed' ||
                testRun.status === 'Failed'
                  ? 'SON TEST RUN'
                  : 'AKTİF TEST RUN'}
              </p>
              <h2>{testRun.repositoryUrl}</h2>
              <p className="run-id">{testRun.id}</p>
            </div>

            <span
              className={`status-badge status-${testRun.status.toLowerCase()}`}
            >
              <span className="status-dot" />
              {STATUS_LABELS[testRun.status]}
            </span>
          </header>

          <div className="pipeline">
            {PIPELINE_STEPS.map((step, index) => {
              const isCompleted =
                testRun.status === 'Completed' ||
                index < currentStepIndex

              const isActive =
                step === testRun.status &&
                testRun.status !== 'Completed'

              return (
                <div
                  className={`pipeline-step ${
                    isCompleted ? 'is-completed' : ''
                  } ${isActive ? 'is-active' : ''}`}
                  key={step}
                >
                  <span className="step-index">
                    {isCompleted ? '✓' : index + 1}
                  </span>
                  <span>{STATUS_LABELS[step]}</span>
                </div>
              )
            })}
          </div>

          {testRun.status === 'Failed' && (
            <div className="alert alert-error run-error">
              <strong>Pipeline başarısız oldu</strong>
              <span>
                {testRun.errorMessage ??
                  'Bilinmeyen bir hata oluştu.'}
              </span>
            </div>
          )}

          {report ? (
            <>
              <section className="metrics-grid">
                <article className="metric metric-passed">
                  <span>Passed</span>
                  <strong>{report.passedCount ?? 0}</strong>
                </article>

                <article className="metric metric-failed">
                  <span>Failed</span>
                  <strong>{report.failedCount ?? 0}</strong>
                </article>

                <article className="metric metric-skipped">
                  <span>Skipped</span>
                  <strong>{report.skippedCount ?? 0}</strong>
                </article>

                <article className="metric">
                  <span>Test süresi</span>
                  <strong>
                    {formatDuration(
                      report.testDurationMilliseconds,
                    )}
                  </strong>
                </article>
              </section>

              <section className="report-grid">
                <article className="report-card">
                  <header>
                    <div>
                      <p className="section-label">
                        BUILD REPORT
                      </p>
                      <h3>
                        {report.buildProjectPath ??
                          'Build projesi'}
                      </h3>
                    </div>
                    <span>
                      {formatDuration(
                        report.buildDurationMilliseconds,
                      )}
                    </span>
                  </header>

                  <pre>
                    {report.buildStandardOutput ||
                      report.buildStandardError ||
                      'Build çıktısı bulunamadı.'}
                  </pre>
                </article>

                <article className="report-card">
                  <header>
                    <div>
                      <p className="section-label">
                        TEST REPORT
                      </p>
                      <h3>
                        {report.testProjectPaths.join(', ') ||
                          'Test projesi'}
                      </h3>
                    </div>
                    <span>
                      Exit {report.testExitCode ?? '—'}
                    </span>
                  </header>

                  <pre>
                    {report.testStandardOutput ||
                      report.testStandardError ||
                      'Test çıktısı bulunamadı.'}
                  </pre>
                </article>
              </section>
            </>
          ) : (
            testRun.status !== 'Failed' && (
              <section className="waiting-card">
                <span className="spinner" />
                <div>
                  <strong>
                    {STATUS_LABELS[testRun.status]}
                  </strong>
                  <p>
                    Pipeline ilerledikçe bu ekran otomatik
                    güncellenecek.
                  </p>
                </div>
              </section>
            )
          )}
        </section>
      ) : (
        <section className="empty-state">
          <div className="empty-icon">⌁</div>
          <h2>İlk test çalışmanı başlat</h2>
          <p>
            Pipeline durumu ve ayrıntılı rapor burada
            görüntülenecek.
          </p>
        </section>
      )}
    </main>
  )
}

export default App
