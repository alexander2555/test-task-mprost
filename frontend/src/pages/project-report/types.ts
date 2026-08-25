// Типы ответа проектного отчёта
export type ProjectReportRow = {
  projectId: string
  projectCode: string
  projectName: string
  hours: number
  amount: number
  budget: number
  percent: number | null
  overspent: boolean
  risk: boolean
}

export type ProjectReportTotals = {
  hours: number
  amount: number
  budget: number
  percent: number | null
  overspent: boolean
  risk: boolean
}

export type ProjectReportResponse = {
  rows: ProjectReportRow[]
  totals: ProjectReportTotals
}
