import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { ApiError } from '../../api/client'
import type {
  EmployeeReference,
  ProjectReference,
  TimeEntryFormValues,
  TimeEntryListItem,
} from './types'

type Props = {
  open: boolean
  entry: TimeEntryListItem | null
  employees: EmployeeReference[]
  projects: ProjectReference[]
  saving: boolean
  onClose: () => void
  onSubmit: (values: TimeEntryFormValues) => Promise<void>
}

// Пустые значения формы для создания новой записи
const emptyValues: TimeEntryFormValues = {
  employeeId: '',
  projectId: '',
  date: '',
  hours: 8,
  comment: '',
}

// Модальное окно для создания и редактирования записей табеля с валидацией
export function TimeEntryModal({
  open,
  entry,
  employees,
  projects,
  saving,
  onClose,
  onSubmit,
}: Props) {
  const [serverError, setServerError] = useState<string | null>(null)
  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<TimeEntryFormValues>({ defaultValues: emptyValues })

  // Сбрасывает форму при открытии/закрытии модального окна
  useEffect(() => {
    if (!open) return

    setServerError(null)
    reset(
      entry
        ? {
            employeeId: entry.employeeId,
            projectId: entry.projectId,
            date: entry.date,
            hours: entry.hours,
            comment: entry.comment,
          }
        : emptyValues,
    )
  }, [entry, open, reset])

  if (!open) return null

  // Обрабатывает отправку формы с отображением ошибок валидации с сервера
  const submit = handleSubmit(async (values) => {
    setServerError(null)

    try {
      await onSubmit(values)
    } catch (error) {
      if (error instanceof ApiError) {
        if (error.response.errors) {
          for (const [field, messages] of Object.entries(
            error.response.errors,
          )) {
            if (
              field === 'employeeId' ||
              field === 'projectId' ||
              field === 'date' ||
              field === 'hours' ||
              field === 'comment'
            ) {
              setError(field, {
                type: 'server',
                message: messages.join(' '),
              })
            }
          }
        }

        setServerError(error.response.message)
        return
      }

      setServerError('Не удалось сохранить запись.')
    }
  })

  return (
    <div
      className='modal-backdrop'
      role='presentation'
    >
      <section
        className='modal'
        role='dialog'
        aria-modal='true'
        aria-labelledby='time-entry-modal-title'
      >
        <div className='modal-header'>
          <h2 id='time-entry-modal-title'>
            {entry ? 'Редактирование записи' : 'Новая запись'}
          </h2>
          <button
            type='button'
            className='button button-secondary'
            onClick={onClose}
            disabled={saving}
          >
            Закрыть
          </button>
        </div>

        <form
          className='form-grid'
          onSubmit={submit}
        >
          <label>
            Сотрудник
            <select
              {...register('employeeId', {
                required: 'Выберите сотрудника.',
              })}
            >
              <option value=''>Выберите сотрудника</option>
              {employees.map((employee) => (
                <option
                  key={employee.id}
                  value={employee.id}
                >
                  {employee.fullName}
                </option>
              ))}
            </select>
            {errors.employeeId && (
              <span className='field-error'>{errors.employeeId.message}</span>
            )}
          </label>

          <label>
            Проект
            <select
              {...register('projectId', {
                required: 'Выберите проект.',
              })}
            >
              <option value=''>Выберите проект</option>
              {projects.map((project) => (
                <option
                  key={project.id}
                  value={project.id}
                >
                  {project.code} — {project.name}
                </option>
              ))}
            </select>
            {errors.projectId && (
              <span className='field-error'>{errors.projectId.message}</span>
            )}
          </label>

          <label>
            Дата
            <input
              type='date'
              {...register('date', { required: 'Укажите дату.' })}
            />
            {errors.date && (
              <span className='field-error'>{errors.date.message}</span>
            )}
          </label>

          <label>
            Часы
            <input
              type='number'
              min='0.5'
              max='24'
              step='0.5'
              {...register('hours', {
                valueAsNumber: true,
                required: 'Укажите количество часов.',
                min: { value: 0.5, message: 'Минимум 0,5 часа.' },
                max: { value: 24, message: 'Максимум 24 часа.' },
                validate: (value) =>
                  value % 0.5 === 0 ||
                  'Количество часов должно быть кратно 0,5.',
              })}
            />
            {errors.hours && (
              <span className='field-error'>{errors.hours.message}</span>
            )}
          </label>

          <label className='form-wide'>
            Комментарий
            <textarea
              rows={3}
              {...register('comment')}
            />
          </label>

          {serverError && (
            <div className='error-banner form-wide'>{serverError}</div>
          )}

          <div className='modal-actions form-wide'>
            <button
              type='button'
              className='button button-secondary'
              onClick={onClose}
              disabled={saving}
            >
              Отмена
            </button>
            <button
              type='submit'
              className='button'
              disabled={saving}
            >
              {saving ? 'Сохранение...' : 'Сохранить'}
            </button>
          </div>
        </form>
      </section>
    </div>
  )
}
