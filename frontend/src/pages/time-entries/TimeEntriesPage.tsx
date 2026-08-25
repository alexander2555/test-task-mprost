import { useMemo, useState } from 'react'
import {
  keepPreviousData,
  useMutation,
  useQuery,
  useQueryClient,
} from '@tanstack/react-query'
import { ApiError } from '../../api/client'
import {
  createTimeEntry,
  deleteTimeEntry,
  getEmployees,
  getProjects,
  getTimeEntries,
  updateTimeEntry,
} from './api'
import { TimeEntryModal } from './TimeEntryModal'
import type { TimeEntryFormValues, TimeEntryListItem } from './types'

const PAGE_SIZE = 10

// Страница табеля с фильтрацией, пагинацией и CRUD операциями для записей
export function TimeEntriesPage() {
  const now = new Date()
  const [year, setYear] = useState(now.getFullYear())
  const [month, setMonth] = useState(now.getMonth() + 1)
  const [employeeId, setEmployeeId] = useState('')
  const [projectId, setProjectId] = useState('')
  const [page, setPage] = useState(1)
  const [editingEntry, setEditingEntry] = useState<TimeEntryListItem | null>(
    null,
  )
  const [modalOpen, setModalOpen] = useState(false)
  const [pageError, setPageError] = useState<string | null>(null)

  const queryClient = useQueryClient()

  const queryKey = useMemo(
    () => ['time-entries', year, month, employeeId, projectId, page, PAGE_SIZE],
    [employeeId, month, page, projectId, year],
  )

  const timeEntriesQuery = useQuery({
    queryKey,
    queryFn: () =>
      getTimeEntries({
        year,
        month,
        employeeId: employeeId || undefined,
        projectId: projectId || undefined,
        page,
        pageSize: PAGE_SIZE,
      }),
    placeholderData: keepPreviousData,
  })

  const employeesQuery = useQuery({
    queryKey: ['employees'],
    queryFn: getEmployees,
  })

  const projectsQuery = useQuery({
    queryKey: ['projects'],
    queryFn: getProjects,
  })

  const saveMutation = useMutation({
    mutationFn: async (values: TimeEntryFormValues) => {
      if (editingEntry) {
        await updateTimeEntry(editingEntry.id, {
          ...values,
          version: editingEntry.version,
        })
      } else {
        await createTimeEntry(values)
      }
    },
    onSuccess: async () => {
      setModalOpen(false)
      setEditingEntry(null)
      await queryClient.invalidateQueries({ queryKey: ['time-entries'] })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: deleteTimeEntry,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['time-entries'] })
    },
  })

  const entries = timeEntriesQuery.data?.items ?? []
  const totalCount = timeEntriesQuery.data?.totalCount ?? 0
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))

  // Сбрасывает пагинацию на первую страницу при изменении фильтров
  function resetPage() {
    setPage(1)
  }

  // Открывает модальное окно для создания новой записи
  function openCreate() {
    setEditingEntry(null)
    setModalOpen(true)
  }

  // Открывает модальное окно для редактирования существующей записи
  function openEdit(entry: TimeEntryListItem) {
    setEditingEntry(entry)
    setModalOpen(true)
  }

  // Удаляет запись с подтверждением и обработкой ошибок
  async function removeEntry(entry: TimeEntryListItem) {
    if (!window.confirm(`Удалить запись от ${entry.date}?`)) {
      return
    }

    setPageError(null)

    try {
      await deleteMutation.mutateAsync(entry.id)
    } catch (error) {
      setPageError(
        error instanceof ApiError
          ? error.response.message
          : 'Не удалось удалить запись.',
      )
    }
  }

  const queryError =
    timeEntriesQuery.error instanceof ApiError
      ? timeEntriesQuery.error.response.message
      : timeEntriesQuery.error
        ? 'Не удалось загрузить записи.'
        : null

  return (
    <section className='page'>
      <div className='page-header'>
        <div>
          <h1>Табель</h1>
          <p className='muted'>Учёт трудозатрат сотрудников по проектам.</p>
        </div>
        <button
          type='button'
          className='button'
          onClick={openCreate}
        >
          Добавить запись
        </button>
      </div>

      <div className='filters'>
        <label>
          Год
          <input
            type='number'
            min='1'
            max='9998'
            value={year}
            onChange={(event) => {
              setYear(Number(event.target.value))
              resetPage()
            }}
          />
        </label>

        <label>
          Месяц
          <select
            value={month}
            onChange={(event) => {
              setMonth(Number(event.target.value))
              resetPage()
            }}
          >
            {Array.from({ length: 12 }, (_, index) => index + 1).map(
              (value) => (
                <option
                  key={value}
                  value={value}
                >
                  {String(value).padStart(2, '0')}
                </option>
              ),
            )}
          </select>
        </label>

        <label>
          Сотрудник
          <select
            value={employeeId}
            onChange={(event) => {
              setEmployeeId(event.target.value)
              resetPage()
            }}
          >
            <option value=''>Все сотрудники</option>
            {(employeesQuery.data ?? []).map((employee) => (
              <option
                key={employee.id}
                value={employee.id}
              >
                {employee.fullName}
              </option>
            ))}
          </select>
        </label>

        <label>
          Проект
          <select
            value={projectId}
            onChange={(event) => {
              setProjectId(event.target.value)
              resetPage()
            }}
          >
            <option value=''>Все проекты</option>
            {(projectsQuery.data ?? []).map((project) => (
              <option
                key={project.id}
                value={project.id}
              >
                {project.code}
              </option>
            ))}
          </select>
        </label>
      </div>

      {(queryError || pageError) && (
        <div className='error-banner'>{queryError ?? pageError}</div>
      )}

      <div className='table-wrap'>
        <table>
          <thead>
            <tr>
              <th>Дата</th>
              <th>Сотрудник</th>
              <th>Проект</th>
              <th>Часы</th>
              <th>Ставка</th>
              <th>Стоимость</th>
              <th>Комментарий</th>
              <th>Переработка</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {timeEntriesQuery.isLoading ? (
              <tr>
                <td colSpan={9}>Загрузка...</td>
              </tr>
            ) : entries.length === 0 ? (
              <tr>
                <td colSpan={9}>Записей за выбранный период нет.</td>
              </tr>
            ) : (
              entries.map((entry) => (
                <tr key={entry.id}>
                  <td>{entry.date}</td>
                  <td>{entry.employeeName}</td>
                  <td>{entry.projectCode}</td>
                  <td>{formatNumber(entry.hours)}</td>
                  <td>{formatMoney(entry.rate)}</td>
                  <td>{formatMoney(entry.amount)}</td>
                  <td>{entry.comment || '—'}</td>
                  <td>
                    {entry.overtime ? (
                      <span className='badge badge-warning'>Да</span>
                    ) : (
                      'Нет'
                    )}
                  </td>
                  <td className='row-actions'>
                    <button
                      type='button'
                      className='link-button'
                      onClick={() => openEdit(entry)}
                    >
                      Изменить
                    </button>
                    <button
                      type='button'
                      className='link-button danger'
                      onClick={() => void removeEntry(entry)}
                      disabled={deleteMutation.isPending}
                    >
                      Удалить
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className='table-footer'>
        <div>
          <strong>Итого:</strong>{' '}
          {formatNumber(timeEntriesQuery.data?.totalHours ?? 0)} ч ·{' '}
          {formatMoney(timeEntriesQuery.data?.totalAmount ?? 0)}
        </div>
        <div className='pagination'>
          <button
            type='button'
            className='button button-secondary'
            onClick={() => setPage((value) => Math.max(1, value - 1))}
            disabled={page <= 1}
          >
            Назад
          </button>
          <span>
            Страница {page} из {totalPages}
          </span>
          <button
            type='button'
            className='button button-secondary'
            onClick={() => setPage((value) => Math.min(totalPages, value + 1))}
            disabled={page >= totalPages}
          >
            Далее
          </button>
        </div>
      </div>

      <TimeEntryModal
        open={modalOpen}
        entry={editingEntry}
        employees={employeesQuery.data ?? []}
        projects={projectsQuery.data ?? []}
        saving={saveMutation.isPending}
        onClose={() => {
          if (!saveMutation.isPending) {
            setModalOpen(false)
            setEditingEntry(null)
          }
        }}
        onSubmit={(values) => saveMutation.mutateAsync(values)}
      />
    </section>
  )
}

// Форматирует число как денежную сумму в рублях
function formatMoney(value: number) {
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB',
    minimumFractionDigits: 2,
  }).format(value)
}

// Форматирует число с разделителями разрядов
function formatNumber(value: number) {
  return new Intl.NumberFormat('ru-RU', {
    maximumFractionDigits: 2,
  }).format(value)
}
