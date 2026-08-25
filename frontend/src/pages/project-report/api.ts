import { apiFetch } from '../../api/client'
import type { ProjectReportResponse } from './types'

// Загружает агрегированный отчёт за выбранный месяц
export function getProjectReport(
  year: number,
  month: number,
): Promise<ProjectReportResponse> {
  const params = new URLSearchParams({
    year: String(year),
    month: String(month),
  })

  return apiFetch<ProjectReportResponse>(
    `/api/reports/projects?${params.toString()}`,
  )
}
