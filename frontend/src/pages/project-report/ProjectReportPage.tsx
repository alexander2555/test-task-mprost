import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ApiError } from '../../api/client'
import { getProjectReport } from './api'
import type { ProjectReportRow } from './types'

// Страница агрегированного отчёта по проектам
export function ProjectReportPage() {
  const now = new Date()
  const [year, setYear] = useState(now.getFullYear())
  const [month, setMonth] = useState(now.getMonth() + 1)

  const reportQuery = useQuery({
    queryKey: ['project-report', year, month],
    queryFn: () => getProjectReport(year, month),
  })

  const errorMessage =
    reportQuery.error instanceof ApiError
      ? reportQuery.error.response.message
      : reportQuery.error
        ? 'Не удалось загрузить отчёт.'
        : null

  const rows = reportQuery.data?.rows ?? []
  const totals = reportQuery.data?.totals

  return (
    <section className='page'>
      <div className='page-header'>
        <div>
          <h1>Отчёт по проектам</h1>
          <p className='muted'>
            Стоимость трудозатрат и освоение бюджета за месяц.
          </p>
        </div>
      </div>

      <div className='filters report-filters'>
        <label>
          Год
          <input
            type='number'
            min='1'
            max='9998'
            value={year}
            onChange={(event) => setYear(Number(event.target.value))}
          />
        </label>

        <label>
          Месяц
          <select
            value={month}
            onChange={(event) => setMonth(Number(event.target.value))}
          >
            {Array.from({ length: 12 }, (_, index) => index + 1).map(
              (value) => (
                <option key={value} value={value}>
                  {String(value).padStart(2, '0')}
                </option>
              ),
            )}
          </select>
        </label>
      </div>

      {errorMessage && <div className='error-banner'>{errorMessage}</div>}

      <div className='table-wrap'>
        <table>
          <thead>
            <tr>
              <th>Проект</th>
              <th>Название</th>
              <th>Часы</th>
              <th>Стоимость</th>
              <th>Бюджет</th>
              <th>Освоено</th>
              <th>Статус</th>
            </tr>
          </thead>
          <tbody>
            {reportQuery.isLoading ? (
              <tr>
                <td colSpan={7}>Загрузка...</td>
              </tr>
            ) : rows.length === 0 ? (
              <tr>
                <td colSpan={7}>За выбранный месяц трудозатрат нет.</td>
              </tr>
            ) : (
              rows.map((row) => (
                <tr key={row.projectId} className={rowClassName(row)}>
                  <td>{row.projectCode}</td>
                  <td>{row.projectName}</td>
                  <td>{formatNumber(row.hours)}</td>
                  <td>{formatMoney(row.amount)}</td>
                  <td>{formatMoney(row.budget)}</td>
                  <td>{formatPercent(row.percent)}</td>
                  <td>{renderStatus(row)}</td>
                </tr>
              ))
            )}
          </tbody>

          {totals && rows.length > 0 && (
            <tfoot>
              <tr className='report-total'>
                <td colSpan={2}>Итого</td>
                <td>{formatNumber(totals.hours)}</td>
                <td>{formatMoney(totals.amount)}</td>
                <td>{formatMoney(totals.budget)}</td>
                <td>{formatPercent(totals.percent)}</td>
                <td>{renderStatus(totals)}</td>
              </tr>
            </tfoot>
          )}
        </table>
      </div>
    </section>
  )
}

// Возвращает CSS-класс для визуального статуса строки
function rowClassName(row: ProjectReportRow) {
  if (row.overspent) return 'report-row-overspent'
  if (row.risk) return 'report-row-risk'
  return undefined
}

// Отображает бизнес-статус освоения бюджета
function renderStatus(value: { risk: boolean; overspent: boolean }) {
  if (value.overspent) {
    return <span className='badge badge-danger'>Перерасход</span>
  }

  if (value.risk) {
    return <span className='badge badge-warning'>Риск</span>
  }

  return 'Норма'
}

function formatMoney(value: number) {
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB',
    minimumFractionDigits: 2,
  }).format(value)
}

function formatNumber(value: number) {
  return new Intl.NumberFormat('ru-RU', {
    maximumFractionDigits: 2,
  }).format(value)
}

function formatPercent(value: number | null) {
  if (value === null) return '—'

  return new Intl.NumberFormat('ru-RU', {
    maximumFractionDigits: 2,
  }).format(value) + ' %'
}
