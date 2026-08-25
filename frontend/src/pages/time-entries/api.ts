import { apiFetch } from '../../api/client'
import type {
  EmployeeReference,
  ProjectReference,
  TimeEntryListResponse,
  TimeEntrySaveRequest,
} from './types'

// Параметры запроса для получения списка записей табеля с фильтрацией и пагинацией
export type TimeEntryQuery = {
  year: number
  month: number
  employeeId?: string
  projectId?: string
  page: number
  pageSize: number
}

// Получает список записей табеля с фильтрацией по периоду, сотруднику и проекту
export function getTimeEntries(
  query: TimeEntryQuery,
): Promise<TimeEntryListResponse> {
  const params = new URLSearchParams({
    year: String(query.year),
    month: String(query.month),
    page: String(query.page),
    pageSize: String(query.pageSize),
  })

  if (query.employeeId) params.set('employeeId', query.employeeId)
  if (query.projectId) params.set('projectId', query.projectId)

  return apiFetch<TimeEntryListResponse>(
    `/api/time-entries?${params.toString()}`,
  )
}

// Получает список сотрудников для справочника
export function getEmployees(): Promise<EmployeeReference[]> {
  return apiFetch<EmployeeReference[]>('/api/employees')
}

// Получает список проектов для справочника
export function getProjects(): Promise<ProjectReference[]> {
  return apiFetch<ProjectReference[]>('/api/projects')
}

// Создаёт новую запись табеля
export function createTimeEntry(
  request: TimeEntrySaveRequest,
): Promise<unknown> {
  return apiFetch<unknown>('/api/time-entries', {
    method: 'PUT',
    body: JSON.stringify(request),
  })
}

// Обновляет существующую запись табеля
export function updateTimeEntry(
  id: string,
  request: TimeEntrySaveRequest,
): Promise<unknown> {
  return apiFetch<unknown>(`/api/time-entries/${id}`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

// Удаляет запись табеля по ID
export function deleteTimeEntry(id: string): Promise<void> {
  return apiFetch<void>(`/api/time-entries/${id}`, {
    method: 'DELETE',
  })
}
