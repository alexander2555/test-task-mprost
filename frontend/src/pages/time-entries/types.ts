// Справочник сотрудника для выпадающих списков
export type EmployeeReference = {
  id: string
  fullName: string
  department: string
}

// Справочник проекта для выпадающих списков
export type ProjectReference = {
  id: string
  code: string
  name: string
}

// Элемент списка записей табеля с вычисляемыми полями (ставка, стоимость, переработка)
export type TimeEntryListItem = {
  id: string
  employeeId: string
  employeeName: string
  projectId: string
  projectCode: string
  date: string
  hours: number
  rate: number
  amount: number
  overtime: boolean
  comment: string
  version: number
}

// Ответ API со списком записей табеля и итоговыми суммами
export type TimeEntryListResponse = {
  items: TimeEntryListItem[]
  page: number
  pageSize: number
  totalCount: number
  totalHours: number
  totalAmount: number
}

// Значения формы для создания/редактирования записи табеля
export type TimeEntryFormValues = {
  employeeId: string
  projectId: string
  date: string
  hours: number
  comment: string
}

// Запрос на сохранение записи табеля (с версией для оптимистической блокировки при обновлении)
export type TimeEntrySaveRequest = TimeEntryFormValues & {
  version?: number
}
